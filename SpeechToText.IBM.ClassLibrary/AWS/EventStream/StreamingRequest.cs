using NAudio.Wave;
using SpeechToText.ClassLibrary.AWS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechToText.ClassLibrary.AWS.EventStream
{
    public class StreamingRequest
    {
        /// <summary>
        /// A generic list of headers. These 3 headers will be sent with each message.
        /// </summary>
        public static List<Header> Headers { get; private set; } = new List<Header>()
        {
            new Header()
            {
                HeaderName=":content-type",
                HeaderValueType=HeaderType.STRING,
                ValueString="application/octet-stream"
            },
            new Header()
            {
                HeaderName=":message-type",
                HeaderValueType=HeaderType.STRING,
                ValueString="event"
            },
            new Header()
            {
                HeaderName=":event-type",
                HeaderValueType=HeaderType.STRING,
                ValueString="AudioEvent"
            }
        };

        /// <summary>
        /// Wraps the audio chunk inside wav header.
        /// </summary>
        /// <param name="audio">Audio chunk in little endian bit ordering.</param>
        /// <param name="format">The actual wave format used in audio chunk.</param>
        /// <returns>A wave frame.</returns>
        public static byte[] IncludeWaveHeader(byte[] audio, WaveFormat format)
        {
            if (BitConverter.IsLittleEndian)
            {
                return IncludeWaveHeaderLE(audio, format);
            }
            else
            {
                return IncludeWaveHeaderBE(audio, format);
            }
        }

        /// <summary>
        /// Encodes the audio data using AWS event stream encoding. 
        /// Each data frame contains three headers combined with a chunk of raw audio bytes.
        /// See wave format specs <see cref="http://soundfile.sapp.org/doc/WaveFormat/"/>.
        /// 
        /// </summary>
        /// <param name="payload">A byte array containing a chunk of audio in raw pcm</param>
        /// <returns>An event stream encoded message frame</returns>
        public static byte[] EncodeAudio(byte[] payload)
        {
            if (BitConverter.IsLittleEndian)
            {
                return EncodeAudioIntoEventStreamLE(payload);
            }
            else
            {
                return EncodeAudioIntoEventStreamBE(payload);
            }
        }

        #region Little Endian

        /// <summary>
        /// The default byte ordering assumed for WAVE data files is little-endian. This handles
        /// the wav header in little endian environment.
        /// </summary>
        /// <param name="audio">Audio chunk in little endian bit ordering.</param>
        /// <param name="format">The actual wave format used in audio chunk.</param>
        /// <returns>A wave frame containing little endian data.</returns>
        private static byte[] IncludeWaveHeaderLE(byte[] audio, WaveFormat format)
        {
            IEnumerable<byte> wave = new List<byte>();

            //  Contains the letters "RIFF" in ASCII form (0x52494646 big - endian form).
            var ChunkID = Encoding.ASCII.GetBytes("RIFF")/*.Reverse()*/;
            wave = wave.Concat(ChunkID);

            // little endian (Max value)
            var ChunkSize = BitConverter.GetBytes(4294967295);
            wave = wave.Concat(ChunkSize);


            // big endian
            var Format = Encoding.ASCII.GetBytes("WAVE").Reverse();
            wave = wave.Concat(Format);

            // big endian
            var Subchunk1ID = Encoding.ASCII.GetBytes("fmt ").Reverse();
            wave = wave.Concat(Subchunk1ID);

            //16 for PCM. little endian
            var Subchunk1Size = BitConverter.GetBytes(16);
            wave = wave.Concat(Subchunk1Size);

            // PCM=1, 2 bytes, little endian
            var AudioFormat = BitConverter.GetBytes((short)1);
            wave = wave.Concat(AudioFormat);

            // 2 bytes, little endian
            var NumChannels = BitConverter.GetBytes((short)format.Channels);
            wave = wave.Concat(NumChannels);

            //4 bytes, little endian
            var SampleRate = BitConverter.GetBytes(format.SampleRate);
            wave = wave.Concat(SampleRate);
            //little endian
            var ByteRate = BitConverter.GetBytes(format.SampleRate * format.Channels * format.BitsPerSample / 8);
            wave = wave.Concat(ByteRate);
            //little endian
            var BlockAlign = BitConverter.GetBytes((short)(format.Channels * format.BitsPerSample / 8));
            wave = wave.Concat(BlockAlign);
            //little endian
            var BitsPerSample = BitConverter.GetBytes((short)format.BitsPerSample);
            wave = wave.Concat(BitsPerSample);


            // big endian
            var Subchunk2ID = Encoding.ASCII.GetBytes("data").Reverse();
            wave = wave.Concat(Subchunk2ID);

            //little endian
            var Subchunk2Size = BitConverter.GetBytes(4294967295);
            wave = wave.Concat(Subchunk2Size);
            //little endian
            wave = wave.Concat(audio);

            return EncodeAudio(wave.ToArray());
        }

        private static byte[] EncodeAudioIntoEventStreamLE(byte[] audio)
        {
            var formatedHeaders = BuildHeadersLE(Headers);

            int totalByteLength = Prelude.Total_Bytes_LENGTH + // size of this buffer
                Prelude.Total_Headers_Bytes_LENGTH + // size of header length buffer
                Message.CHECKSUM_LENGTH + // prelude crc32
                formatedHeaders.Length + // total size of headers
                audio.Length + // total size of payload
                Message.CHECKSUM_LENGTH; // size of crc32 of the total message


            var preludeBytes = BitConverter.GetBytes(totalByteLength).Reverse()
                .Concat(BitConverter.GetBytes(formatedHeaders.Length).Reverse())
                .ToArray();

            /* 
             * Calculate the prelude CRC checksum & convert it into a
             * 4-byte big-endian integer.
            */
            var preludeCrc = BitConverter
                .GetBytes(ChecksumHelper
                    .ComputeCrc32(preludeBytes, preludeBytes.Length))
                .Reverse();


            // construct the envelope containing preludeBytes (prelude), prelude CRC, headers & raw audio bytes (payload).
            IEnumerable<byte> message = preludeBytes
                .Concat(preludeCrc)
                .Concat(formatedHeaders)
                .Concat(audio);

            /* 
             * Message CRC: The 4-byte CRC checksum of the message.
             */
            uint messageCrc = ChecksumHelper.ComputeCrc32(message.ToArray(), message.Count());

            BitConverter
                .GetBytes(messageCrc).Reverse();

            // complete the message by adding the messageCrc at the end.
            message = message.Concat(BitConverter
                .GetBytes(messageCrc).Reverse());


            return message.ToArray();
        }
        
        /// <summary>
        /// Build the headers for event-stream on a Little Endian architecture. See the event-stream documentation here 
        /// <see cref="https://docs.aws.amazon.com/en_pv/transcribe/latest/dg/event-stream.html"/>
        /// </summary>
        /// <param name="headers">List of the headers to be built.</param>
        /// <returns></returns>
        private static byte[] BuildHeadersLE(List<Header> headers)
        {
            IEnumerable<byte> headerBytes = new List<byte>();
            foreach (var header in headers)
            {
                // (1 byte) The byte-length of the header name. 
                var headerNameByteLength = (byte)header.HeaderName.Length;

                // variable length
                var headerName = Encoding.UTF8.GetBytes(header.HeaderName);

                // Header value type is 1 byte long
                var valueType = (byte)header.HeaderValueType;

                // 2 bytes
                var valueStringByteLength = BitConverter.GetBytes((short)header.ValueString.Length)
                    .Reverse();

                // variable length
                var valueString = Encoding.UTF8.GetBytes(header.ValueString);


                headerBytes = headerBytes.Append(headerNameByteLength)
                    .Concat(headerName)
                    .Append(valueType)
                    .Concat(valueStringByteLength)
                    .Concat(valueString);
            }
            return headerBytes.ToArray();
        }

        #endregion

        #region Big Endian

        /// <summary>
        /// Files written using the big-endian byte ordering scheme have the identifier RIFX instead of RIFF. 
        /// This handles the wav header in big endian environment.
        /// </summary>
        /// <param name="audio">Audio chunk in big endian bit ordering.</param>
        /// <param name="format">The actual wave format used in audio chunk.</param>
        /// <returns>A wave frame containing big endian data.</returns>
        private static byte[] IncludeWaveHeaderBE(byte[] audio, WaveFormat format)
        {
            IEnumerable<byte> wave = new List<byte>();

            //  Contains the letters "RIFFX" in ASCII form (0x52494646 big - endian form).
            var ChunkID = Encoding.ASCII.GetBytes("RIFFX");
            wave = wave.Concat(ChunkID);

            // little endian (Max value)
            var ChunkSize = BitConverter.GetBytes(4294967295).Reverse();
            wave = wave.Concat(ChunkSize);


            // big endian
            var Format = Encoding.ASCII.GetBytes("WAVE");
            wave = wave.Concat(Format);

            // big endian
            var Subchunk1ID = Encoding.ASCII.GetBytes("fmt ");
            wave = wave.Concat(Subchunk1ID);

            //16 for PCM. little endian
            var Subchunk1Size = BitConverter.GetBytes(16).Reverse();
            wave = wave.Concat(Subchunk1Size);

            // PCM=1, 2 bytes little endian
            var AudioFormat = BitConverter.GetBytes((short)1).Reverse();
            wave = wave.Concat(AudioFormat);

            // 2 bytes little endian
            var NumChannels = BitConverter.GetBytes((short)format.Channels).Reverse();
            wave = wave.Concat(NumChannels);

            //4 bytes little endian
            var SampleRate = BitConverter.GetBytes(format.SampleRate).Reverse();
            wave = wave.Concat(SampleRate);

            //little endian
            var ByteRate = BitConverter.GetBytes(format.SampleRate * format.Channels * format.BitsPerSample / 8).Reverse();
            wave = wave.Concat(ByteRate);

            //little endian
            var BlockAlign = BitConverter.GetBytes((short)(format.Channels * format.BitsPerSample / 8)).Reverse();
            wave = wave.Concat(BlockAlign);

            //little endian
            var BitsPerSample = BitConverter.GetBytes((short)format.BitsPerSample).Reverse();
            wave = wave.Concat(BitsPerSample);


            // big endian
            var Subchunk2ID = Encoding.ASCII.GetBytes("data");
            wave = wave.Concat(Subchunk2ID);

            //little endian
            var Subchunk2Size = BitConverter.GetBytes(4294967295).Reverse();
            wave = wave.Concat(Subchunk2Size);

            //big endian
            wave = wave.Concat(audio);

            return EncodeAudio(wave.ToArray());
        }

        private static byte[] EncodeAudioIntoEventStreamBE(byte[] rawAudio)
        {
            var formatedHeaders = BuildHeadersBE(Headers);

            int totalByteLength = 4 + // size of this buffer
                4 + // size of header length buffer
                4 + // prelude crc32
                formatedHeaders.Length + // total size of headers
                rawAudio.Length + // total size of payload
                4; // size of crc32 of the total message


            var preludeBytes = BitConverter.GetBytes(totalByteLength)
               .Concat(BitConverter.GetBytes(formatedHeaders.Length))
               .ToArray();

            /* 
             * Calculate the prelude CRC checksum & convert it into a
             * 4-byte big-endian integer.
            */
            var preludeCrc = BitConverter
                .GetBytes(ChecksumHelper
                    .ComputeCrc32(preludeBytes, preludeBytes.Length));


            // construct the envelope containing preludeBytes (prelude), prelude CRC, headers & raw audio bytes (payload).
            IEnumerable<byte> message = preludeBytes
                .Concat(preludeCrc)
                .Concat(formatedHeaders)
                .Concat(rawAudio);

            /* 
             * Message CRC: The 4-byte CRC checksum of the message.
             */
            uint messageCrc = ChecksumHelper.ComputeCrc32(message.ToArray(), message.Count());

            // complete the message by adding the messageCrc at the end.
            message = message.Concat(BitConverter
                .GetBytes(messageCrc));


            return message.ToArray();
        }

        /// <summary>
        /// Build the headers for event-stream on a Big Endian architecture. See the event-stream documentation here 
        /// <see cref="https://docs.aws.amazon.com/en_pv/transcribe/latest/dg/event-stream.html"/>
        /// </summary>
        /// <param name="headers">List of the headers to be built.</param>
        /// <returns></returns>
        private static byte[] BuildHeadersBE(List<Header> headers)
        {
            IEnumerable<byte> headerBytes = new List<byte>();
            foreach (var header in headers)
            {
                // (1 byte) The byte-length of the header name. 
                var headerNameByteLength = (byte)header.HeaderName.Length;

                // variable length
                var headerName = Encoding.UTF8.GetBytes(header.HeaderName);

                // Header value type is 1 byte long
                var valueType = (byte)header.HeaderValueType;

                // 2 bytes
                var valueStringByteLength = BitConverter.GetBytes((short)header.ValueString.Length).Reverse();

                // variable length
                var valueString = Encoding.UTF8.GetBytes(header.ValueString);


                headerBytes = headerBytes.Append(headerNameByteLength)
                    .Concat(headerName)
                    .Append(valueType)
                    .Concat(valueStringByteLength)
                    .Concat(valueString);
            }
            return headerBytes.ToArray();
        }

        #endregion
    }
}

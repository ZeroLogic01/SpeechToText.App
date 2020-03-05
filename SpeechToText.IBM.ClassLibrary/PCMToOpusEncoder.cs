using Concentus.Enums;
using Concentus.Oggfile;
using Concentus.Structs;
using System.IO;
using System.Linq;

namespace SpeechToText.ClassLibrary
{
    /// <summary>
    /// A class for encoding PCM audio samples using opus encoder &amp; writes the encoded audio (packed inside ogg container)
    /// to underlying <see cref="MemoryStream"/> object
    /// </summary>
    public class PCMToOpusEncoder
    {
        #region Fields

        private readonly OpusOggWriteStreamRealTime _opusOggStreamWriter;
        private readonly MemoryStream _opusHeaderStream;
        private readonly OpusEncoder _encoder;

        #endregion

        #region Constructor

        public PCMToOpusEncoder(int rate = 16000, int bitRate = 24, int channels = 1)
        {
            _encoder = OpusEncoder.Create(rate, channels, OpusApplication.OPUS_APPLICATION_VOIP);
            _encoder.Bitrate = bitRate * 1024;
            _encoder.ForceMode = OpusMode.MODE_AUTO;
            _encoder.SignalType = OpusSignal.OPUS_SIGNAL_VOICE;
            _encoder.Complexity = 0;

            _opusHeaderStream = new MemoryStream();
            _opusOggStreamWriter = new OpusOggWriteStreamRealTime(_encoder, _opusHeaderStream, null, rate);
        }
        #endregion

        #region Methods

        /// <summary>
        /// Writes a buffer of PCM audio samples to the encoder and packetizer. Runs Opus encoding and potentially outputs one or more pages to the underlying Ogg stream.
        /// You can write any non-zero number of samples that you want here; there are no restrictions on length or packet boundaries
        /// </summary>
        /// <param name="pcmAudio"></param>
        /// <returns>Encoded audio packed inside ogg container.</returns>
        public byte[] EncodePCM(byte[] pcmAudio)
        {
            short[] packet = BytesToShorts(pcmAudio);
            using (MemoryStream encodedAudioStream = new MemoryStream())
            {
                _opusOggStreamWriter.EncodedAudioStream = encodedAudioStream;
                _opusOggStreamWriter.WriteSamples(packet, 0, packet.Length);

                var opusHeaderByteArray = _opusHeaderStream.ToArray();
                var encodedAudioByteArray = encodedAudioStream.ToArray();

                // combine both 
                var encodedBuffer = opusHeaderByteArray.Concat(encodedAudioByteArray);
                return encodedBuffer.ToArray();
            }
        }

        /// <summary>
        /// Call when you are finished recording. This operation will close the underlying stream as well.
        /// </summary>
        public void Fininsh()
        {
            _opusOggStreamWriter.Finish();
        }


        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static short[] BytesToShorts(byte[] input)
        {
            return BytesToShorts(input, 0, input.Length);
        }

        /// <summary>
        /// Converts interleaved byte samples (such as what you get from a capture device)
        /// into linear short samples (that are much easier to work with)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static short[] BytesToShorts(byte[] input, int offset, int length)
        {
            short[] processedValues = new short[length / 2];
            for (int c = 0; c < processedValues.Length; c++)
            {
                processedValues[c] = (short)(((int)input[(c * 2) + offset]) << 0);
                processedValues[c] += (short)(((int)input[(c * 2) + 1 + offset]) << 8);
            }

            return processedValues;
        }
        #endregion

    }
}

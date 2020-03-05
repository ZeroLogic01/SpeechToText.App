using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeechToText.ClassLibrary.AWS.EventStream
{
    public class StreamingReponse
    {
        /// <summary>
        /// Decodes the event stream encoded payload.
        /// </summary>
        /// <param name="payload">Encoded payload.</param>
        /// <returns></returns>
        public static (List<Header>, byte[]) Decode(byte[] payload)
        {
            if (BitConverter.IsLittleEndian)
            {
                return DecodeLE(payload);
            }
            else
            {
                return DecodeBE(payload);
            }
        }

        #region Little Endian Architecture

        /// <summary>
        /// Decode the payload (Big Endian data) on a Little endian CPU processor architecture.
        /// </summary>
        /// <param name="payload">The event stream encoded data.</param>
        public static (List<Header>, byte[]) DecodeLE(byte[] payload)
        {
            if (payload.Length < Message.MINIMUM_MESSAGE_LENGTH)
            { throw new InvalidOperationException("Provided message too short to accommodate event stream message overhead"); }

            var totalByteLength = BitConverter.ToUInt32(payload.Take(Prelude.Total_Bytes_LENGTH)
                .Reverse().ToArray(), 0);

            if (payload.Length != totalByteLength)
            {
                throw new ArgumentException("Reported message length does not match received message length");
            }

            var headerBytesLength = BitConverter.ToInt32(payload
                .Skip(Prelude.Total_Bytes_LENGTH)
                .Take(Prelude.Total_Headers_Bytes_LENGTH)
                .Reverse()
                .ToArray(), 0);

            int preludeLengthWithCRC = Prelude.PRELUDE_LENGTH + Message.CHECKSUM_LENGTH;

            var headers = ExtractHeadersLE(payload
                .Skip(preludeLengthWithCRC).Take(headerBytesLength));


            // Now trim the prelude, prelude CRC & headers from payload
            payload = payload.Skip(preludeLengthWithCRC + headerBytesLength).ToArray();
            // trim the message CRC as well
            payload = payload.Take(Math.Max(0, payload.Count() - Message.CHECKSUM_LENGTH)).ToArray();

            return (headers, payload);
        }

        /// <summary>
        /// Extract the headers (Big Endian data) on a Little endian CPU processor architecture.
        /// </summary>
        public static List<Header> ExtractHeadersLE(IEnumerable<byte> data)
        {
            List<Header> headers = new List<Header>();

            for (int i = 0; i < data.Count();)
            {
                Header header = new Header();

                header.HeaderNameByteLength = data.Skip(i++).FirstOrDefault();
                header.HeaderName = Encoding
                                    .UTF8.GetString(data
                                        .Skip(i)
                                        .Take(header.HeaderNameByteLength).ToArray());

                i += header.HeaderNameByteLength;

                header.HeaderValueType = (HeaderType)data.Skip(i++).FirstOrDefault();
                header.ValueStringByteLength = BitConverter
                                    .ToInt16(data
                                        .Skip(i)
                                        .Take(2)
                                        .Reverse()
                                        .ToArray(), 0);

                i += 2;
                header.ValueString = Encoding.UTF8.GetString(data
                    .Skip(i)
                        .Take(header.ValueStringByteLength)
                        .ToArray());

                i += header.ValueStringByteLength;

                headers.Add(header);
            }

            return headers;
        }

        #endregion


        #region Big Endian Architecture

        /// <summary>
        /// Decode the payload (Big Endian data) on a Big endian CPU processor architecture.
        /// </summary>
        /// <param name="payload">The event stream encoded data.</param>
        public static (List<Header>, byte[]) DecodeBE(byte[] payload)
        {
            if (payload.Length < Message.MINIMUM_MESSAGE_LENGTH)
            { throw new InvalidOperationException("Provided message too short to accommodate event stream message overhead"); }

            var totalByteLength = BitConverter.ToUInt32(payload.Take(Prelude.Total_Bytes_LENGTH).ToArray(), 0);

            if (payload.Length != totalByteLength)
            {
                throw new ArgumentException("Reported message length does not match received message length");
            }
            var headerBytesLength = BitConverter.ToInt32(payload.Skip(Prelude.Total_Bytes_LENGTH)
                .Take(Prelude.Total_Headers_Bytes_LENGTH).ToArray(), 0);

            #region CRC checksum validation code removed

            //var wirePreludeCrc = BitConverter.ToUInt32(payload.Skip(8).Take(4).ToArray(), 0);

            //var computedPreludeCrc = ChecksumHelper.ComputeCrc32(payload.Take(8).ToArray(), 8);
            //if (wirePreludeCrc != computedPreludeCrc)
            //{
            //    throw new ArgumentException($"Prelude checksum failure: expected {wirePreludeCrc}, computed {computedPreludeCrc}");
            //}


            //var wireMessageCrc = BitConverter.ToInt32(payload
            //    .Skip(Math.Max(0, payload.Count() - 4))
            //    .ToArray(), 0);

            //var messageBody = payload
            //    .Take(Math.Max(0, payload.Count() - 4)).ToArray();

            //var computedMessageCrc = ChecksumHelper
            //    .ComputeCrc32(messageBody, messageBody.Length);

            //if (wireMessageCrc != computedMessageCrc)
            //{
            //    throw new ArgumentException($"Message checksum failure: expected {wireMessageCrc}, computed {computedMessageCrc}");
            //}

            #endregion

            int preludeLengthWithCRC = Prelude.PRELUDE_LENGTH + Message.CHECKSUM_LENGTH;

            List<Header> headers = ExtractHeadersBE(payload.Skip(preludeLengthWithCRC).Take(headerBytesLength));

            // Now trim the prelude, prelude CRC & headers from payload
            payload = payload.Skip(preludeLengthWithCRC + headerBytesLength).ToArray();
            // trim the message CRC as well
            payload = payload.Take(Math.Max(0, payload.Count() - Message.CHECKSUM_LENGTH)).ToArray();

            return (headers, payload);
        }

        /// <summary>
        /// Extract the headers (Big Endian data) on a Big endian CPU processor architecture.
        /// </summary>
        public static List<Header> ExtractHeadersBE(IEnumerable<byte> data)
        {
            List<Header> headers = new List<Header>();

            for (int i = 0; i < data.Count();)
            {
                Header header = new Header();

                header.HeaderNameByteLength = data.Skip(i++).FirstOrDefault();
                header.HeaderName = Encoding
                                    .UTF8.GetString(data
                                        .Skip(i)
                                        .Take(header.HeaderNameByteLength).ToArray());

                i += header.HeaderNameByteLength;

                header.HeaderValueType = (HeaderType)data.Skip(i++).FirstOrDefault();
                header.ValueStringByteLength = BitConverter
                                    .ToInt16(data
                                        .Skip(i)
                                        .Take(2).ToArray(), 0);

                i += 2;
                header.ValueString = Encoding.UTF8.GetString(data
                    .Skip(i)
                        .Take(header.ValueStringByteLength)
                        .ToArray());

                i += header.ValueStringByteLength;

                headers.Add(header);
            }

            return headers;
        }

        #endregion

    }

}

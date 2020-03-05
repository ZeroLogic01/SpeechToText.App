using AWS.Checksums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.AWS.Util
{
    public static class  ChecksumHelper
    {
        /// <summary>
        /// The entry point function to perform a CRC32(Ethernet, gzip) computation.
        /// Selects a suitable implementation based on hardware capabilities.
        /// Pass 0 in the previousCrc32 parameter as an initial value unless continuing
        /// to update a running crc in a subsequent call.
        /// </summary>
        /// <param name="input">Byte array whose Checksum you want to calculate</param>
        /// <param name="length">Total length of the input</param>
        /// <param name="previousCrc32">Pass 0 in the previousCrc32 parameter as an initial value unless continuing
        /// to update a running crc in a subsequent call.</param>
        /// <returns></returns>
        public static uint ComputeCrc32(byte[] buf, int length, uint previousCrc = 0)
        {
            CRC crc = new CRC32();
            return crc.ComputeRunning(buf, length, previousCrc);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.AWS.EventStream
{
    public static class Message
    {
        /// <summary>
        /// Checksums are always CRC32 hashes.
        /// </summary>
        public static readonly int CHECKSUM_LENGTH = 4;

        /// <summary>
        /// Messages must include a full prelude, a prelude checksum, and a message checksum
        /// </summary>
        public static readonly int MINIMUM_MESSAGE_LENGTH = Prelude.PRELUDE_LENGTH + (CHECKSUM_LENGTH * 2);
    }
}

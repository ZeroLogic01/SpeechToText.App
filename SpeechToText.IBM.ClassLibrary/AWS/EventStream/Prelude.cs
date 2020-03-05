using AWS.Checksums;
using SpeechToText.ClassLibrary.AWS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.AWS.EventStream
{
    class Prelude
    {
        /// <summary>
        /// (4 bytes) Total length of the message.
        /// </summary>
        public readonly static int Total_Bytes_LENGTH = 4;

        /// <summary>
        /// (4 bytes) Total length of the headers.
        /// </summary>
        public readonly static int Total_Headers_Bytes_LENGTH = 4;

        /// <summary>
        /// The prelude consists of two components <see cref="Total_Bytes_LENGTH"/> &amp; 
        /// <see cref="Total_Headers_Bytes_LENGTH"/>.
        /// </summary>
        public readonly static int PRELUDE_LENGTH = Total_Bytes_LENGTH + Total_Headers_Bytes_LENGTH;

    }
}

namespace SpeechToText.ClassLibrary.AWS.EventStream
{
    /// <summary>
    /// Meta-data annotating the message, such as the message type, content type, and so on. 
    /// Messages have multiple headers. Headers are key-value pairs where the key is a UTF-8 string. 
    /// Headers can appear in any order in the headers portion of the message and any given header 
    /// can appear only once.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// (1 byte) The byte-length of the header name. 
        /// </summary>
        public byte HeaderNameByteLength { get; set; }

        /// <summary>
        /// (Variable length) The name of the header indicating the header type. 
        /// </summary>
        public string HeaderName { get; set; }

        /// <summary>
        /// (1 byte) An enumeration indicating the header value type.
        /// </summary>
        public HeaderType HeaderValueType { get; set; }

        /// <summary>
        /// (2 bytes) The byte-length of the header value string. 
        /// </summary>
        public short ValueStringByteLength { get; set; }

        /// <summary>
        /// (Variable length) The value of the header string. Valid values for this field depend on the type of header. 
        /// For valid values, see the following frame descriptions.
        /// </summary>
        public string ValueString { get; set; }

    }
}

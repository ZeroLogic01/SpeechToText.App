using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class SpeechRecognitionAlternative
    {
        public SpeechRecognitionAlternative() { }

        [JsonProperty("transcript", NullValueHandling = NullValueHandling.Ignore)]
        public string Transcript { get; set; }
        [JsonProperty("confidence", NullValueHandling = NullValueHandling.Ignore)]
        public double? Confidence { get; set; }
        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> Timestamps { get; set; }
        [JsonProperty("word_confidence", NullValueHandling = NullValueHandling.Ignore)]
        public List<List<string>> WordConfidence { get; set; }
    }
}

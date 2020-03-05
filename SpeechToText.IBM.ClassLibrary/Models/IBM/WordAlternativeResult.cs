using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class WordAlternativeResult
    {
        public WordAlternativeResult() { }

        [JsonProperty("confidence", NullValueHandling = NullValueHandling.Ignore)]
        public double? Confidence { get; set; }
        [JsonProperty("word", NullValueHandling = NullValueHandling.Ignore)]
        public string Word { get; set; }
    }
}

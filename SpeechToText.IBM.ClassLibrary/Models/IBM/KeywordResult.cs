using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class KeywordResult
    {
        public KeywordResult() { }

        [JsonProperty("normalized_text", NullValueHandling = NullValueHandling.Ignore)]
        public string NormalizedText { get; set; }
        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? StartTime { get; set; }
        [JsonProperty("end_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? EndTime { get; set; }
        [JsonProperty("confidence", NullValueHandling = NullValueHandling.Ignore)]
        public double? Confidence { get; set; }
    }
}

using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class SpeakerLabelsResult
    {
        public SpeakerLabelsResult() { }

        [JsonProperty("from", NullValueHandling = NullValueHandling.Ignore)]
        public float? From { get; set; }
        [JsonProperty("to", NullValueHandling = NullValueHandling.Ignore)]
        public float? To { get; set; }
        [JsonProperty("speaker", NullValueHandling = NullValueHandling.Ignore)]
        public long? Speaker { get; set; }
        [JsonProperty("confidence", NullValueHandling = NullValueHandling.Ignore)]
        public float? Confidence { get; set; }
        [JsonProperty("final", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FinalResults { get; set; }
    }
}

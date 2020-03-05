using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class AudioMetricsHistogramBin
    {
        public AudioMetricsHistogramBin() { }

        [JsonProperty("begin", NullValueHandling = NullValueHandling.Ignore)]
        public float? Begin { get; set; }
        [JsonProperty("end", NullValueHandling = NullValueHandling.Ignore)]
        public float? End { get; set; }
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }
    }
}

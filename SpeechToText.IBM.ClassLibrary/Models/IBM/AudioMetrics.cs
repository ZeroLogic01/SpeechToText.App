using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class AudioMetrics
    {
        public AudioMetrics() { }

        [JsonProperty("sampling_interval", NullValueHandling = NullValueHandling.Ignore)]
        public float? SamplingInterval { get; set; }

        [JsonProperty("accumulated", NullValueHandling = NullValueHandling.Ignore)]
        public AudioMetricsDetails Accumulated { get; set; }
    }
}

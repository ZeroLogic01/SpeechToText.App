using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class ProcessingMetrics
    {
        public ProcessingMetrics() { }

        [JsonProperty("processed_audio", NullValueHandling = NullValueHandling.Ignore)]
        public ProcessedAudio ProcessedAudio { get; set; }
        [JsonProperty("wall_clock_since_first_byte_received", NullValueHandling = NullValueHandling.Ignore)]
        public float? WallClockSinceFirstByteReceived { get; set; }
        [JsonProperty("periodic", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Periodic { get; set; }
    }
}

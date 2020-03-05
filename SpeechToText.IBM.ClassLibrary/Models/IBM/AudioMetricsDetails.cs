using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class AudioMetricsDetails
    {
        public AudioMetricsDetails() { }

        [JsonProperty("final", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Final { get; set; }
        [JsonProperty("end_time", NullValueHandling = NullValueHandling.Ignore)]
        public float? EndTime { get; set; }
        [JsonProperty("signal_to_noise_ratio", NullValueHandling = NullValueHandling.Ignore)]
        public float? SignalToNoiseRatio { get; set; }
        [JsonProperty("speech_ratio", NullValueHandling = NullValueHandling.Ignore)]
        public float? SpeechRatio { get; set; }
        [JsonProperty("high_frequency_loss", NullValueHandling = NullValueHandling.Ignore)]
        public float? HighFrequencyLoss { get; set; }
        [JsonProperty("direct_current_offset", NullValueHandling = NullValueHandling.Ignore)]
        public List<AudioMetricsHistogramBin> DirectCurrentOffset { get; set; }
        [JsonProperty("clipping_rate", NullValueHandling = NullValueHandling.Ignore)]
        public List<AudioMetricsHistogramBin> ClippingRate { get; set; }
        [JsonProperty("speech_level", NullValueHandling = NullValueHandling.Ignore)]
        public List<AudioMetricsHistogramBin> SpeechLevel { get; set; }
        [JsonProperty("non_speech_level", NullValueHandling = NullValueHandling.Ignore)]
        public List<AudioMetricsHistogramBin> NonSpeechLevel { get; set; }
    }
}

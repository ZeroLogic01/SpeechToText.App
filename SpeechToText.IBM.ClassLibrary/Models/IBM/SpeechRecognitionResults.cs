using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class SpeechRecognitionResults
    {
        public SpeechRecognitionResults() { }

        [JsonProperty("results", NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeechRecognitionResult> Results { get; set; }
        [JsonProperty("result_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? ResultIndex { get; set; }
        [JsonProperty("speaker_labels", NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeakerLabelsResult> SpeakerLabels { get; set; }
        [JsonProperty("processing_metrics", NullValueHandling = NullValueHandling.Ignore)]
        public ProcessingMetrics ProcessingMetrics { get; set; }
        [JsonProperty("audio_metrics", NullValueHandling = NullValueHandling.Ignore)]
        public AudioMetrics AudioMetrics { get; set; }
        [JsonProperty("warnings", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Warnings { get; set; }
    }
}

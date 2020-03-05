using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class ProcessedAudio
    {
        public ProcessedAudio() { }

        [JsonProperty("received", NullValueHandling = NullValueHandling.Ignore)]
        public float? Received { get; set; }
        [JsonProperty("seen_by_engine", NullValueHandling = NullValueHandling.Ignore)]
        public float? SeenByEngine { get; set; }
        [JsonProperty("transcription", NullValueHandling = NullValueHandling.Ignore)]
        public float? Transcription { get; set; }
        [JsonProperty("speaker_labels", NullValueHandling = NullValueHandling.Ignore)]
        public float? SpeakerLabels { get; set; }
    }
}

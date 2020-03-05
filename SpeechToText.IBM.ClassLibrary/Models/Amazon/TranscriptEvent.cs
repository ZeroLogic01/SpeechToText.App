using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.Amazon
{
    public class TranscriptEvent
    {
        [JsonProperty("Transcript", NullValueHandling = NullValueHandling.Ignore)]
        public Transcript Transcript { get; set; }
    }
}

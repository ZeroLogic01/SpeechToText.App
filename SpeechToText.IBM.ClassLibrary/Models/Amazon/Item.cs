using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models.Amazon
{
    public class Item
    {
        [JsonProperty("Content", NullValueHandling = NullValueHandling.Ignore)]
        public string Content { get; set; }

        [JsonProperty("EndTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EndTime { get; set; }

        [JsonProperty("StartTime", NullValueHandling = NullValueHandling.Ignore)]
        public long StartTime { get; set; }

        [JsonProperty("Type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}

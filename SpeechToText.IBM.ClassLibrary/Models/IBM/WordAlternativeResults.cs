using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class WordAlternativeResults
    {
        public WordAlternativeResults() { }

        [JsonProperty("start_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? StartTime { get; set; }

        [JsonProperty("end_time", NullValueHandling = NullValueHandling.Ignore)]
        public double? EndTime { get; set; }

        [JsonProperty("alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<WordAlternativeResult> Alternatives { get; set; }
    }
}

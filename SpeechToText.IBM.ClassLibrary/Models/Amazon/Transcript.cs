using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.Amazon
{
    public class Transcript
    {
        [JsonProperty("Results", NullValueHandling = NullValueHandling.Ignore)]
        public List<Result> Results { get; set; }
    }
}

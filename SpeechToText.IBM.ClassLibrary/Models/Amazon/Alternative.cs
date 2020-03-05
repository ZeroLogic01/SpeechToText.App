using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.Amazon
{
    public class Alternative
    {
       
        [JsonProperty("Items", NullValueHandling = NullValueHandling.Ignore)]
        public List<Item> Items { get; set; }

        [JsonProperty("Transcript", NullValueHandling = NullValueHandling.Ignore)]
        public string Transcript { get; set; }
    }
}

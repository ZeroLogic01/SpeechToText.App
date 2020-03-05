using Newtonsoft.Json;

namespace SpeechToText.ClassLibrary.Models
{
    public class ServiceState
    {
        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        private string Message { set { Error = value; } }
    }
}

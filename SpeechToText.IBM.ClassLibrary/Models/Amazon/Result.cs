using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.Amazon
{
    public class Result
    {

        [JsonProperty("ResultId", NullValueHandling = NullValueHandling.Ignore)]
        public string ResultId { get; set; }

        [JsonProperty("Alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<Alternative> Alternatives { get; set; }


        [JsonProperty("StartTime", NullValueHandling = NullValueHandling.Ignore)]
        public long StartTime { get; set; }

        [JsonProperty("EndTime", NullValueHandling = NullValueHandling.Ignore)]
        public long EndTime { get; set; }

        /// <summary>
        /// Indicates whether the response is a partial response containing the 
        /// transcription results so far or if it is a complete transcription of the audio segment.
        /// </summary>
        [JsonProperty("IsPartial", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsPartial { get; set; }



    }
}

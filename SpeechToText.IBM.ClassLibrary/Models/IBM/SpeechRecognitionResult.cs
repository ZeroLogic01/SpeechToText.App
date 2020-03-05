using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    public class SpeechRecognitionResult
    {
        public SpeechRecognitionResult() { }

        [JsonProperty("final", NullValueHandling = NullValueHandling.Ignore)]
        public bool? FinalResults { get; set; }
        [JsonProperty("alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeechRecognitionAlternative> Alternatives { get; set; }
        [JsonProperty("keywords_result", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<KeywordResult>> KeywordsResult { get; set; }
        [JsonProperty("word_alternatives", NullValueHandling = NullValueHandling.Ignore)]
        public List<WordAlternativeResults> WordAlternatives { get; set; }
    }
}

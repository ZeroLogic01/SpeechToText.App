using Newtonsoft.Json;
using System.Collections.Generic;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    /// <summary>
    /// Request header details to be sent with WebSocket client calls.
    /// </summary>
    public class IBMRequestHeader
    {
        /// <summary>
        /// Action to be taken, like start & stop action.
        /// </summary>
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        /// <summary>
        /// The format (MIME type) of the audio. For more information about specifying an
        /// audio format.
        /// </summary>
        [JsonProperty("content-type", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }


        /// <summary>
        /// If true, the service returns interim results as a stream of JSON SpeechRecognitionResults objects. 
        /// If false, the service returns a single SpeechRecognitionResults object with final results only. 
        /// See Interim results <see cref="https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-output#interim"/>
        /// </summary>
        [JsonProperty("interim_results", NullValueHandling = NullValueHandling.Ignore)]
        public bool InterimResults { get; set; }


        /// <summary>
        /// If `true`, the service returns time alignment for each word. By default, no
        /// timestamps are returned. See [Word
        /// timestamps](https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-output#word_timestamps).
        /// (optional, default to false)
        /// </summary>
        [JsonProperty("timestamps", NullValueHandling = NullValueHandling.Ignore)]
        public bool TimeStamp { get; set; }

        /// <summary>
        /// Name of the model. Default value is "en-US_BroadbandModel".
        /// </summary>
        [JsonProperty("model", NullValueHandling = NullValueHandling.Ignore)]
        public string Model { get; set; }

        /// <summary>
        /// The time in seconds after which, if only silence (no speech) is detected in
        /// streaming audio, the connection is closed with a 400 error. The parameter is useful for stopping audio
        /// submission from a live microphone when a user simply walks away. Use `-1` for infinity. See [Inactivity
        /// timeout](https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-input#timeouts-inactivity).
        /// (optional)
        /// </summary>
        [JsonProperty("inactivity_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public long? InactivityTimeout { get; set; }

        /// <summary>
        /// An array of keyword strings to spot in the audio. Each keyword string can include one
        /// or more string tokens. Keywords are spotted only in the final results, not in interim hypotheses. If you
        /// specify any keywords, you must also specify a keywords threshold. You can spot a maximum of 1000 keywords.
        /// Omit the parameter or specify an empty array if you do not need to spot keywords. See [Keyword
        /// spotting](https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-output#keyword_spotting).
        /// (optional)
        /// </summary>
        [JsonProperty("keywords", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> Keywords { get; set; }

        /// <summary>
        /// If `true`, the service converts dates, times, series of digits and numbers,
        /// phone numbers, currency values, and internet addresses into more readable, conventional representations in
        /// the final transcript of a recognition request. For US English, the service also converts certain keyword
        /// strings to punctuation symbols. By default, the service performs no smart formatting.
        ///
        /// **Note:** Applies to US English, Japanese, and Spanish transcription only.
        ///
        /// See [Smart
        /// formatting](https://cloud.ibm.com/docs/services/speech-to-text?topic=speech-to-text-output#smart_formatting).
        /// (optional, default to false)
        /// </summary>
        [JsonProperty("smart_formatting", NullValueHandling = NullValueHandling.Ignore)]
        public bool SmartFormatting { get; set; }

    }
}

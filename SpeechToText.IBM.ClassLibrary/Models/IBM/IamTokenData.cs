using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.Models.IBM
{
    /// <summary>
    /// IAM Token Data.
    /// </summary>
    internal class IamTokenData
    {
        #region Properties

        [JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenType { get; set; }

        [JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExpiresIn { get; set; }

        [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Expiration { get; set; }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets the IamTokenData from the iam.bluemix.net using api-key.
        /// </summary>
        /// <param name="apikey">The API key for IBM speech to text api.</param>
        /// <returns></returns>
        internal static Task<IamTokenData> GetIamToken(string apikey)
        {
            return Task.Run(() =>
            {
                var request = (HttpWebRequest)WebRequest.Create("https://iam.bluemix.net/identity/token");
                request.Proxy = null;
                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/x-www-form-urlencoded";

                using (TextWriter tw = new StreamWriter(request.GetRequestStream()))
                {
                    tw.Write($"grant_type=urn:ibm:params:oauth:grant-type:apikey&apikey={apikey}");
                }
                var resp = request.GetResponse();
                using (TextReader tr = new StreamReader(resp.GetResponseStream()))
                {
                    var s = tr.ReadToEnd();
                    return JsonConvert.DeserializeObject<IamTokenData>(s);
                }
            });
        }

        #endregion
    }
}
using SpeechToText.ClassLibrary.AWS.Signers;
using SpeechToText.ClassLibrary.AWS.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.AWS
{
    public class PresignedUrl
    {
        static readonly string AWSAccessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
        static readonly string AWSSecretKey = ConfigurationManager.AppSettings["AWSSecretKey"];

        /// <summary>
        /// Construct a pre-signed URL.
        /// The signature V4 authorization data is embedded in the URL as query parameters.
        /// </summary>
        public static string Get(string region, string languageCode,
            int sampleRate)
        {
            if (string.IsNullOrWhiteSpace(AWSAccessKey))
            {
                throw new NullReferenceException($"AWS Access Key is required");
            }

            if (string.IsNullOrWhiteSpace(AWSSecretKey))
            {
                throw new NullReferenceException($"AWS Secret Key is required");
            }

            //Currently only pcm is valid.
            string mediaEncoding = "pcm";
            
            var serviceName = "transcribe";
            //var serviceName = "trans";

            var endpointUri = $"wss://transcribestreaming.{region}.amazonaws.com:8443/stream-transcription-websocket";

            var headers = new Dictionary<string, string>();

            // The length of time in seconds until the credentials expire. 
            // The maximum value is 300 seconds (5 minutes)
            var expiresOn = DateTime.UtcNow.AddMinutes(5);
            var period = Convert.ToInt64((expiresOn.ToUniversalTime() - DateTime.UtcNow).TotalSeconds);

            // construct the query parameter string to accompany the url
            var queryParams = new StringBuilder();
            queryParams.AppendFormat("{0}={1}", "language-code", HttpHelpers.UrlEncode(languageCode));
            queryParams.AppendFormat("&{0}={1}", "sample-rate", HttpHelpers.UrlEncode(sampleRate.ToString()));
            queryParams.AppendFormat("&{0}={1}", "media-encoding", HttpHelpers.UrlEncode(mediaEncoding));
            queryParams.AppendFormat("&{0}={1}", AWS4SignerBase.X_Amz_Expires, HttpHelpers.UrlEncode(period.ToString()));

            var signer = new AWS4SignerForQueryParameterAuth
            {
                EndpointUri = new Uri(endpointUri),
                HttpMethod = "GET",
                Service = serviceName,
                Region = region
            };

            /* Create a hash of the payload. For a GET request, the payload is an empty string.
             * Note: No need to create hash every time. This is  the Sha256Hash for an empty string.
             * ComputeSha256Hash(string.Empty) returns "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
             */
            var payloadHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
            var authorization = signer.ComputeSignature(headers,
                                                        queryParams.ToString(),
                                                        payloadHash,
                                                        AWSAccessKey,
                                                        AWSSecretKey);

            // build the presigned url to incorporate the authorization element
            var urlBuilder = new StringBuilder(endpointUri.ToString());

            // add our query params
            urlBuilder.AppendFormat("?{0}", queryParams.ToString());

            // and finally the Signature V4 authorization string components
            urlBuilder.AppendFormat("&{0}", authorization);

            var presignedUrl = urlBuilder.ToString();

            //Console.WriteLine("\nComputed presigned url:\n{0}", presignedUrl);

            return presignedUrl;
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

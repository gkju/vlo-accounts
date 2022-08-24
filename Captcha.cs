using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RestSharp;

namespace VLO_BOARDS
{
    public class Captcha
    {
        public static string ErrorName = "CaptchaError";
        public static string ErrorStatus = "Bad captcha";
        public static readonly float Threshold = 0.7f;
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly CaptchaCredentials _credentials;
        private static readonly Regex AnRegex = new ("^[0-9a-zA-Z_-]*$");
        
        public Captcha(IHttpClientFactory clientFactory, CaptchaCredentials credentials)
        {
            _clientFactory = clientFactory;
            _credentials = credentials;
        }
        /// <summary>
        /// Verifies google recaptcha v3 response
        /// </summary>
        /// <param name="response"> Recaptcha response </param>
        /// <returns> User score </returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<float> VerifyCaptcha(string response)
        {
            if (!AnRegex.IsMatch(response))
            {
                throw new ArgumentException("Invalid response str");
            }
            
            var client = new RestClient("https://www.google.com/recaptcha/api/");

            var req = new RestRequest("siteverify");
            
            req.AddParameter("secret", _credentials.PrivateKey);
            req.AddParameter("response", response);

            var res = await client.ExecutePostAsync<GoogleResponse>(req);
            var googleRes = res.Data;
            
            if (googleRes is null)
            {
                return -1;
            }
            
            return googleRes.score;
        }
    }
    
    public class CaptchaApiParams
    {
        public string secret;
        public string response;
    }

    public class CaptchaCredentials
    {
        public readonly string PrivateKey;
        public readonly string PublicKey;
        
        public CaptchaCredentials(string privateKey, string publicKey)
        {
            this.PrivateKey = privateKey;
            this.PublicKey = publicKey;
        }
    }

    public class GoogleResponse
    {
        public bool success { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
        public float score { get; set; }
    }
}
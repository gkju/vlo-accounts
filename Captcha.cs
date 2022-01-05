using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VLO_BOARDS
{
    public class Captcha
    {
        public static string ErrorName = "CaptchaError";
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
            
            var client = _clientFactory.CreateClient();

            string url =
                $"https://www.google.com/recaptcha/api/siteverify?secret={_credentials.privateKey}&response={response}";

            var httpResponse = await client.PostAsync(url, new StringContent(""));
            
            var googleRes = JsonSerializer.Deserialize<GoogleResponse>(await httpResponse.Content.ReadAsStringAsync());

            if (googleRes == null)
            {
                return -1;
            }
            
            return googleRes.score;
        }
    }

    public class CaptchaCredentials
    {
        public readonly string privateKey;
        public readonly string publicKey;
        
        public CaptchaCredentials(string privateKey, string publicKey)
        {
            this.privateKey = privateKey;
            this.publicKey = publicKey;
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
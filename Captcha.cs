using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace VLO_BOARDS
{
    public class Captcha
    {
        public static string ErrorName = "CaptchaError";
        
        private readonly IHttpClientFactory _clientFactory;
        private readonly CaptchaCredentials _credentials;
        
        public Captcha(IHttpClientFactory clientFactory, CaptchaCredentials credentials)
        {
            _clientFactory = clientFactory;
            _credentials = credentials;
        }

        public async Task<float> verifyCaptcha(string response)
        {
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
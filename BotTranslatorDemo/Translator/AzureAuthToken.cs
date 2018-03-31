using BotTranslatorDemo.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotTranslatorDemo.Translator
{
    public class AzureAuthToken
    {
        private const string OcpApimSubscriptionKeyHeader = StringConstants.OcpApimSubscriptionKeyHeader;

        private static readonly Uri ServiceUrl = new Uri(Settings.GetCognitiveServicesTokenUri());

        private static readonly TimeSpan TokenCacheDuration = new TimeSpan(0, 5, 0);

        private string _storedTokenValue = string.Empty;

        private DateTime _storedTokenTime = DateTime.MinValue;

        public string SubscriptionKey { get; }

        public HttpStatusCode RequestStatusCode { get; private set; }

        public AzureAuthToken(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "A subscription key is required");

            SubscriptionKey = key;
            RequestStatusCode = HttpStatusCode.InternalServerError;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (string.IsNullOrWhiteSpace(this.SubscriptionKey))
                return string.Empty;

            if ((DateTime.Now - _storedTokenTime) < TokenCacheDuration)
                return _storedTokenValue;

            try
            {
                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = ServiceUrl;
                    request.Content = new StringContent(string.Empty);
                    request.Headers.TryAddWithoutValidation(OcpApimSubscriptionKeyHeader, SubscriptionKey);

                    client.Timeout = TimeSpan.FromSeconds(180);
                    var response = await client.SendAsync(request);
                    RequestStatusCode = response.StatusCode;
                    response.EnsureSuccessStatusCode();

                    var token = await response.Content.ReadAsStringAsync();
                    _storedTokenTime = DateTime.Now;
                    _storedTokenValue = "Bearer " + token;

                    return _storedTokenValue;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

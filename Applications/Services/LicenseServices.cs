using Applications.Models;
using static System.Net.WebRequestMethods;

namespace Applications.Services
{
    public class LicenseServices
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri;
        public LicenseServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _baseUri = "https://localhost:7010/api/licenses/{0}";
        }
        public async Task<HttpResponseMessage> GetData(string endpoint)
        {
            var result = new HttpResponseMessage();
            try
            {
                result = await _httpClient.GetAsync(string.Format(_baseUri, endpoint));
            }
            catch
            {
                result = new HttpResponseMessage();
            }
            return result;
        }
        public async Task<HttpResponseMessage> PostData(string endpoint, object content)
        {
            var result = new HttpResponseMessage();
            try
            {
                result = await _httpClient.PostAsJsonAsync(string.Format(_baseUri,endpoint), content);
            }
            catch
            {
                result = new HttpResponseMessage();
            }
            return result;
        }
    }
}

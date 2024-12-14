using System.Net.Http.Headers;

namespace api.Services
{
    public class FornaxService
    {
        private readonly HttpClient _httpClient;
        private readonly string _fornaxApiUrl = "https://fornaxapi.example.com/open"; // Replace with the actual Fornax API URL

        public FornaxService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> OpenFile(string filePath)
        {
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

            var response = await _httpClient.PostAsync(_fornaxApiUrl, content);
            response.EnsureSuccessStatusCode();

            return response;
        }
    }

}

using System.Diagnostics;
using System.Net.Http.Json;

namespace EmploymentApp.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:8000/api";

        public ApiClient()
        {
            var handler = new HttpClientHandler();
            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private string BuildUrl(string endpoint)
        {
            if (endpoint.StartsWith("http://") || endpoint.StartsWith("https://"))
                return endpoint;

            if (endpoint.StartsWith("/"))
                return $"{BaseUrl}{endpoint}";

            return $"{BaseUrl}/{endpoint}";
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, string token = null)
        {
            try
            {
                var url = BuildUrl(endpoint);
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GET request error: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data, string token = null)
        {
            try
            {
                var url = BuildUrl(endpoint);
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(data)
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"POST request error: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data, string token = null)
        {
            try
            {
                var url = BuildUrl(endpoint);
                var request = new HttpRequestMessage(HttpMethod.Put, url)
                {
                    Content = JsonContent.Create(data)
                };

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PUT request error: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint, string token = null)
        {
            try
            {
                var url = BuildUrl(endpoint);
                var request = new HttpRequestMessage(HttpMethod.Delete, url);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.SendAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DELETE request error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> GetAsJsonAsync<T>(string endpoint, string token = null)
        {
            try
            {
                var response = await GetAsync(endpoint, token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GET as JSON error: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PostAsJsonAsync<T>(string endpoint, object data, string token = null)
        {
            try
            {
                var response = await PostAsync(endpoint, data, token);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                return default;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"POST as JSON error: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
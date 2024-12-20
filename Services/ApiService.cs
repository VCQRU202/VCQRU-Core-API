using RestSharp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CoreApi_BL_App.Services
{
    public class ApiService
    {
        private readonly RestClient _restClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(RestClient restClient, ILogger<ApiService> logger)
        {
            _restClient = restClient;
            _logger = logger;
        }

        // Generic method to make API requests using RestSharp
        public async Task<T> CallApiAsync<T>(string url, Method method, object content = null,
                                             string appId = null, string apiKey = null)
        {
            try
            {
                // Create RestRequest
                var request = new RestRequest(url, method);

                // Add headers if provided
                if (!string.IsNullOrEmpty(appId))
                {
                    request.AddHeader("app-id", appId); // Add the app-id header
                }

                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.AddHeader("api-key", apiKey); // Add the api-key header
                }

                request.AddHeader("Content-Type", "application/json"); // Default Content-Type header

                // If there's content, serialize it to JSON and add to the request
                if (content != null)
                {
                    var jsonContent = JsonSerializer.Serialize(content);
                    request.AddJsonBody(jsonContent); // Automatically sets content-type to application/json
                }

                // Send the request
                var response = await _restClient.ExecuteAsync(request);

                // Check if the response is successful
                if (!response.IsSuccessful)
                {
                    _logger.LogError($"API call failed: {response.StatusCode} - {response.ErrorMessage}");
                    throw new Exception($"API call failed: {response.StatusCode} - {response.ErrorMessage}");
                }

                // Deserialize the content into the specified type (T)
                var result = JsonSerializer.Deserialize<T>(response.Content);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling API: {ex.Message}");
                throw;
            }
        }
    }
}

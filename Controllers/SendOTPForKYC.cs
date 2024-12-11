using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendOTPForKYCController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly ILogger<SendOTPForKYCController> _logger;

        // Constructor that injects configuration and logger
        public SendOTPForKYCController(IConfiguration config, ILogger<SendOTPForKYCController> logger)
        {
            _connectionString = config.GetConnectionString("defaultConnectionbeta");
            _logger = logger;
        }

        [HttpPost]
        [Route("sendotp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            var response = new Response();

            try
            {
                // Retrieve Aadhaar number from the request object
                string aadharNo = request.AadharNo;
                if (string.IsNullOrEmpty(aadharNo))
                {
                    response.Status = false;
                    response.Message = "Aadhar number is required.";
                    return BadRequest(response);
                }

                // External API URL for OTP request
                string apiUrl = "https://test.zoop.one/in/identity/okyc/otp/request";

                // Prepare the request body
                var body = new
                {
                    mode = "sync",
                    data = new
                    {
                        customer_aadhaar_number = aadharNo,
                        consent = "Y",
                        consent_text = "Approve_the_values_here"
                    }
                };

                // Prepare HTTP client and send POST request
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("app-id", "648d7d9a22658f001d0193ac");
                    client.DefaultRequestHeaders.Add("api-key", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");

                    var jsonContent = new StringContent(JObject.FromObject(body).ToString(), Encoding.UTF8, "application/json");
                    var apiResponse = await client.PostAsync(apiUrl, jsonContent);

                    // Check for successful response from the API
                    if (!apiResponse.IsSuccessStatusCode)
                    {
                        response.Status = false;
                        response.Message = apiResponse.ReasonPhrase;
                        _logger.LogError($"API call failed: {apiResponse.ReasonPhrase}");
                        return StatusCode((int)apiResponse.StatusCode, response);
                    }

                    // Parse the API response content
                    var apiResponseContent = await apiResponse.Content.ReadAsStringAsync();
                    JObject jObj = JObject.Parse(apiResponseContent);

                    // Validate the response data
                    if (jObj["success"]?.ToString() == "True" &&
                        jObj["response_code"]?.ToString() == "100" &&
                        jObj["result"]?["is_otp_sent"]?.ToString() == "True" &&
                        jObj["result"]?["is_number_linked"]?.ToString() == "True" &&
                        jObj["result"]?["is_aadhaar_valid"]?.ToString() == "True")
                    {
                        response.Status = true;
                        response.Message = "OTP sent successfully!";
                        response.Data = jObj["request_id"]?.ToString();
                        return Ok(response);
                    }
                    else
                    {
                        response.Status = false;
                        response.Message = jObj["metadata"]?["reason_message"]?.ToString();
                        response.Data = jObj;
                        return BadRequest(response);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the OTP request.");
                response.Status = false;
                response.Message = "An error occurred while processing the request.";
                response.Data = ex.StackTrace;
                return StatusCode(500, response);
            }
        }
    }

    // Define the Response class structure for API responses
    public class Response
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }

    // Define the Request DTO
    public class SendOTPRequest
    {
        public string AadharNo { get; set; }
    }
}

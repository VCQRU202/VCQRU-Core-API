using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorAppSettingController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<VendorAppSettingController> _logger;

        public VendorAppSettingController(DatabaseManager databaseManager, ILogger<VendorAppSettingController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> VendorAppSetting([FromBody] VendorAppSettingClass req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Comp_ID))
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid request data."));
            }

            try
            {
                string query = $@"
                    SELECT TOP 1 
                        CompData, 
                        RegistrationFields, 
                        EKYCSelectedOptions, 
                        kyc_Details, 
                        Claim_Settings 
                    FROM BrandSettings 
                    WHERE Comp_ID = '{req.Comp_ID}' 
                    ORDER BY [Comp_ID] DESC";

                DataTable dt = await _databaseManager.ExecuteDataTableAsync(query);
                JArray compDataArray;
                if (dt.Rows.Count > 0)
                {
                    // Parse and validate JSON data
                    string compDataString = dt.Rows[0]["CompData"].ToString();
                    JObject compData = string.IsNullOrEmpty(compDataString) ? new JObject() : JObject.Parse(compDataString);
                    string logo = compData["Logo"]?.ToString();

                    var compDataArray1 = compData.Properties()
                             .Select(prop => prop.Value.ToString())
                             .ToArray();


                    // Print array values
                    foreach (var value in compDataArray1)
                    {
                        Console.WriteLine(value);
                        //compData = compDataArray1;
                    }

                    compDataArray = new JArray { compData["jToken"] };
                    string registrationFieldsString = dt.Rows[0]["RegistrationFields"].ToString();
                    JArray registrationFields = string.IsNullOrEmpty(registrationFieldsString) ? new JArray() : JArray.Parse(registrationFieldsString);

                    // Process "Values" field
                    foreach (var field in registrationFields)
                    {
                        if (field["Values"] == null || field["Values"].Type != JTokenType.Array)
                        {
                            // Ensure "Values" is a JArray, even if it is null or not an array
                            field["Values"] = new JArray();
                        }
                        else if (field["Values"].Type == JTokenType.String)
                        {
                            // Convert comma-separated string to JArray
                            string[] valuesArray = field["Values"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            field["Values"] = JArray.FromObject(valuesArray);
                        }
                    }


                    string kycDetailsString = dt.Rows[0]["kyc_Details"].ToString();
                    JObject kycDetails = string.IsNullOrEmpty(kycDetailsString) ? new JObject() : JObject.Parse(kycDetailsString);

                    string claimSettingsString = dt.Rows[0]["Claim_Settings"].ToString();
                    JObject claimSettings = string.IsNullOrEmpty(claimSettingsString) ? new JObject() : JObject.Parse(claimSettingsString);

                    // Combine parsed data into response object
                    var responseData = new
                    {
                        CompData1 = compDataArray1,
                        RegistrationFields1 = registrationFields,
                        KycDetails1 = kycDetails,
                        ClaimSettings1 = claimSettings
                    };

                    _logger.LogInformation("Final Response Data: {ResponseData}", JsonConvert.SerializeObject(responseData));

                    // Send response
                    return Ok(new ApiResponseVendorSetting<object>(true, "Data retrieved successfully.", responseData));

                    //return Ok(new ApiResponseVendorSetting<object>(true, "Data retrieved successfully.", responseData));
                }
                else
                {
                    return BadRequest(new ApiResponseVendorSetting<object>(false, "No records found for the given Comp_ID."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving vendor app settings.");
                return StatusCode(500, new ApiResponseVendorSetting<object>(false, "An unexpected error occurred."));
            }
        }
    }

    // Request Model
    public class VendorAppSettingClass
    {
        public string Comp_ID { get; set; }
    }
      public class ApiResponseVendorSetting<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponseVendorSetting(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }

 
}

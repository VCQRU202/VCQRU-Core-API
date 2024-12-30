using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
                return BadRequest(new ApiResponseVendorSetting<object>(false, "Invalid request data."));
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

                if (dt.Rows.Count > 0)
                {
                    // Parse JSON fields and handle potential null or empty values
                    string compDataString = dt.Rows[0]["CompData"].ToString();
                    var compData = string.IsNullOrEmpty(compDataString) ? new JObject() : JObject.Parse(compDataString);

                    string registrationFieldsString = dt.Rows[0]["RegistrationFields"].ToString();
                    var registrationFields = string.IsNullOrEmpty(registrationFieldsString) ? new JArray() : JArray.Parse(registrationFieldsString);

                    // Normalize "Values" field in registrationFields
                    foreach (var field in registrationFields)
                    {
                        if (field["Values"] == null || field["Values"].Type != JTokenType.Array)
                        {
                            field["Values"] = new JArray();
                        }
                        else if (field["Values"].Type == JTokenType.String)
                        {
                            string[] valuesArray = field["Values"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            field["Values"] = JArray.FromObject(valuesArray);
                        }
                    }

                    string kycDetailsString = dt.Rows[0]["kyc_Details"].ToString();
                    var kycDetails = string.IsNullOrEmpty(kycDetailsString) ? new JObject() : JObject.Parse(kycDetailsString);

                    string claimSettingsString = dt.Rows[0]["Claim_Settings"].ToString();
                    var claimSettings = string.IsNullOrEmpty(claimSettingsString) ? new JObject() : JObject.Parse(claimSettingsString);

                    // Combine parsed data into a response object
                    var responseData = new
                    {
                        CompData = compData,
                        RegistrationFields = registrationFields,
                        KycDetails = kycDetails,
                        ClaimSettings = claimSettings
                    };

                    _logger.LogInformation("VendorAppSetting data retrieved successfully: {ResponseData}", JsonConvert.SerializeObject(responseData));
                    return Ok(new ApiResponseVendorSetting<object>(true, "Data retrieved successfully.", responseData));
                }
                else
                {
                    return NotFound(new ApiResponseVendorSetting<object>(false, "No records found for the given Comp_ID."));
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

    // Response Wrapper
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

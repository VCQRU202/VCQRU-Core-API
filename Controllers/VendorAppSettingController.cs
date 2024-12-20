using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                    
                    string compDataString = dt.Rows[0]["CompData"].ToString();
                    var compData = string.IsNullOrEmpty(compDataString) ? new Dictionary<string, object>() : JObject.Parse(compDataString).ToObject<Dictionary<string, object>>();

                    string registrationFieldsString = dt.Rows[0]["RegistrationFields"].ToString();
                    var registrationFields = string.IsNullOrEmpty(registrationFieldsString) ? new List<Dictionary<string, object>>() : JArray.Parse(registrationFieldsString).ToObject<List<Dictionary<string, object>>>();

                    string kycDetailsString = dt.Rows[0]["kyc_Details"].ToString();
                    var kycDetails = string.IsNullOrEmpty(kycDetailsString) ? new Dictionary<string, object>() : JObject.Parse(kycDetailsString).ToObject<Dictionary<string, object>>();

                    string claimSettingsString = dt.Rows[0]["Claim_Settings"].ToString();
                    var claimSettings = string.IsNullOrEmpty(claimSettingsString) ? new Dictionary<string, object>() : JObject.Parse(claimSettingsString).ToObject<Dictionary<string, object>>();

                    // Combine parsed data into a single list of rows
                    var rows = new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object>
                        {
                            { "CompData", compData },
                            { "RegistrationFields", registrationFields },
                            { "KycDetails", kycDetails },
                            { "ClaimSettings", claimSettings }
                        }
                    };

                    // Return the transformed data
                    return Ok(new ApiResponse<object>(true, "Successfully", rows));
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

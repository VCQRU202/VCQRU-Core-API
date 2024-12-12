using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

                if (dt.Rows.Count > 0)
                {
                    // Parse CompData
                    string compDataString = dt.Rows[0]["CompData"].ToString();
                    JObject compData = JObject.Parse(compDataString);

                    // Parse RegistrationFields
                    string registrationFieldsString = dt.Rows[0]["RegistrationFields"].ToString();
                    JArray registrationFields = JArray.Parse(registrationFieldsString);

                    // Process "Values" field in RegistrationFields
                    foreach (var field in registrationFields)
                    {
                        if (field["Values"] != null && field["Values"].Type == JTokenType.String)
                        {
                            string[] valuesArray = field["Values"].ToString().Split(',');
                            field["Values"] = JArray.FromObject(valuesArray);
                        }
                    }

                    // Parse kyc_Details
                    string kycDetailsString = dt.Rows[0]["kyc_Details"].ToString();
                    JObject kycDetails = JObject.Parse(kycDetailsString);

                    // Parse Claim_Settings
                    string claimSettingsString = dt.Rows[0]["Claim_Settings"].ToString();
                    JObject claimSettings = JObject.Parse(claimSettingsString);

                    // Combine parsed data into response object
                    var responseData = new
                    {
                        CompData = compData,
                        RegistrationFields = registrationFields,
                        KycDetails = kycDetails,
                        ClaimSettings = claimSettings
                    };

                    return Ok(new ApiResponse<object>(true, "Data retrieved successfully.", responseData));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "No records found for the given Comp_ID."));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving vendor app settings.");
                return StatusCode(500, new ApiResponse<object>(false, "An unexpected error occurred."));
            }
        }
    }

    // Request Model
    public class VendorAppSettingClass
    {
        public string Comp_ID { get; set; }
    }

 
}

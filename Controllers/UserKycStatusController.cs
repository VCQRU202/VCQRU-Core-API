using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserKycStatusController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<UserKycStatusController> _logger;

        public UserKycStatusController(DatabaseManager databaseManager, ILogger<UserKycStatusController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UserKycStatus([FromBody] UserKycStatusClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            try
            {
                // Validate mobile number
                if (string.IsNullOrEmpty(req.Mobile) || req.Mobile.Length < 10)
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid mobile number."));

                string Mobile = req.Mobile.Substring(req.Mobile.Length - 10);

                // Validate company ID
                if (string.IsNullOrEmpty(req.Comp_ID) || req.Comp_ID.Length <= 4)
                    return BadRequest(new ApiResponse<object>(false, "Company ID cannot be null or less than 5 characters."));

                // Validate M_Consumerid
                if (!int.TryParse(req.M_Consumerid, out int M_Consumerid))
                    return BadRequest(new ApiResponse<object>(false, "Invalid consumer ID."));

                var UserkycData = new UserkycData();

                // SQL query to get KYC status
                string query = @"SELECT TOP 1 
                                    [M_Consumerid],
                                    CASE WHEN panekycStatus = 'Online' THEN '1' WHEN panekycStatus = 'Failed' THEN '2' ELSE '0' END AS panekycStatus,
                                    CASE WHEN aadharkycStatus = 'Online' THEN '1' WHEN aadharkycStatus = 'Failed' THEN '2' ELSE '0' END AS aadharkycStatus,
                                    CASE WHEN bankekycStatus = 'Online' THEN '1' WHEN bankekycStatus = 'Failed' THEN '2' ELSE '0' END AS bankekycStatus,
                                    CASE WHEN UPIKYCSTATUS = 'Online' THEN '1' WHEN UPIKYCSTATUS = 'Failed' THEN '2' ELSE '0' END AS UPIKYCSTATUS,
                                    CASE WHEN VRKbl_KYC_status = 1 THEN 'Approved' WHEN VRKbl_KYC_status = 2 THEN 'Rejected' ELSE 'Pending' END AS VRKbl_KYC_status
                                 FROM M_Consumer 
                                 WHERE M_Consumerid = @M_Consumerid 
                                 ORDER BY [M_Consumerid] DESC";

                var parameters = new { M_Consumerid };
                var dt = await _databaseManager.ExecuteDataTableAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    // Map data to response object
                    UserkycData.M_Consumerid = M_Consumerid;
                    UserkycData.PanekycStatusString = dt.Rows[0]["panekycStatus"].ToString();
                    UserkycData.AadharkycStatusString = dt.Rows[0]["aadharkycStatus"].ToString();
                    UserkycData.BankekycStatusString = dt.Rows[0]["bankekycStatus"].ToString();
                    UserkycData.UPI_KYC_StatusString = dt.Rows[0]["UPIKYCSTATUS"].ToString();
                    UserkycData.VRKbl_KYC_StatusString = dt.Rows[0]["VRKbl_KYC_status"].ToString();

                    // Query to fetch additional KYC details
                    string query1 = @"SELECT TOP 1 kyc_Details 
                                      FROM BrandSettings  
                                      WHERE Comp_ID = @Comp_ID 
                                      ORDER BY [Comp_ID] DESC";

                    var dt1 = await _databaseManager.ExecuteDataTableAsync(query1, new { req.Comp_ID });

                    if (dt1.Rows.Count > 0)
                    {
                        string kyc_DetailsString = dt1.Rows[0]["kyc_Details"].ToString();
                        JObject KycData = string.IsNullOrEmpty(kyc_DetailsString) ? new JObject() : JObject.Parse(kyc_DetailsString);
                        UserkycData.AadharekycEnable = KycData["AadharCard"]?.ToString();
                        UserkycData.PanekycEnable = KycData["PANCard"]?.ToString();
                        UserkycData.BankkyccEnable = KycData["AccountDetails"]?.ToString();
                        UserkycData.UpikyccEnable = KycData["UPI"]?.ToString();
                    }

                    return Ok(new ApiResponse<object>(true, "KYC status retrieved successfully.", UserkycData));
                }

                return BadRequest(new ApiResponse<object>(false, "Record not available."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in UserKycStatus method");
                return StatusCode(500, new ApiResponse<object>(false, "An unexpected error occurred."));
            }
        }
    }

    public class UserKycStatusClass
    {
        public string Comp_ID { get; set; }
        public string Mobile { get; set; }
        public string M_Consumerid { get; set; }
    }

    public class UserkycData
    {
        public int M_Consumerid { get; set; }
        public string PanekycStatusString { get; set; }
        public string AadharkycStatusString { get; set; }
        public string BankekycStatusString { get; set; }
        public string VRKbl_KYC_StatusString { get; set; }
        public string PanekycEnable { get; set; }
        public string AadharekycEnable { get; set; }
        public string UpikyccEnable { get; set; }
        public string BankkyccEnable { get; set; }
        public string UPI_KYC_StatusString { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(bool success, string message, T data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}

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
                UserkycData.CountNotification = 0;

                string queryNoti = $"select count(*) as CountNoti from Tbl_notificationUser where comp_id='{req.Comp_ID}' and m_consumerid='{M_Consumerid}' and status='0'";
                var dtNoti = await _databaseManager.ExecuteDataTableAsync(queryNoti);

                if (dtNoti.Rows.Count > 0)
                {
                    UserkycData.CountNotification = Convert.ToInt32(dtNoti.Rows[0]["CountNoti"]);
                }
                    // SQL query
                    string query = $"SELECT TOP 1 [M_Consumerid],case when panekycStatus ='Online' Then '1' when panekycStatus='Failed' then '2' else '0' end panekycStatus,case when aadharkycStatus ='Online' Then '1'  when aadharkycStatus='Failed' then '2'  else '0' end aadharkycStatus,case when bankekycStatus ='Online' Then '1' when bankekycStatus='Failed' then '2' else '0' end bankekycStatus,case when UPIKYCSTATUS = 1 Then '1' when UPIKYCSTATUS= 2 then '2' else '0' end UPIKYCSTATUS,case when VRKbl_KYC_status =1 Then 'Approved' when VRKbl_KYC_status =2 Then 'Rejected'  else 'Pending' end VRKbl_KYC_status FROM M_Consumer WHERE M_Consumerid = '{M_Consumerid}' ORDER BY [M_Consumerid] DESC";
                var dt = await _databaseManager.ExecuteDataTableAsync(query);

                if (dt.Rows.Count > 0)
                {
                    // Map data to response object
                    UserkycData.M_Consumerid = M_Consumerid;
                    UserkycData.PanekycStatusString = dt.Rows[0]["panekycStatus"].ToString();
                    UserkycData.AadharkycStatusString = dt.Rows[0]["aadharkycStatus"].ToString();
                    UserkycData.BankekycStatusString = dt.Rows[0]["bankekycStatus"].ToString();
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
                        //UserkycData.AadharekycEnable = KycData["AadharCard"]?.ToString();
                        //UserkycData.PanekycEnable = KycData["PANCard"]?.ToString();
                        //UserkycData.BankkyccEnable = KycData["AccountDetails"]?.ToString();
                        //UserkycData.UpikyccEnable = KycData["UPI"]?.ToString();
                        UserkycData.AadharekycEnable = string.IsNullOrEmpty(KycData["AadharCard"]?.ToString()) ? "No" : KycData["AadharCard"].ToString();
                        UserkycData.PanekycEnable = string.IsNullOrEmpty(KycData["PANCard"]?.ToString()) ? "No" : KycData["PANCard"].ToString();
                        UserkycData.BankkyccEnable = string.IsNullOrEmpty(KycData["AccountDetails"]?.ToString()) ? "No" : KycData["AccountDetails"].ToString();
                        UserkycData.UpikyccEnable = string.IsNullOrEmpty(KycData["UPI"]?.ToString()) ? "No" : KycData["UPI"].ToString();

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
    }

}


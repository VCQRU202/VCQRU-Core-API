using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateOTPForKYCAadharController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public ValidateOTPForKYCAadharController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTPForKYCAadhar([FromBody] ValidateOTPForKYCAadharClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            try
            {
                // Validate Aadhar Number
                if (string.IsNullOrEmpty(req.AadharNo) || req.AadharNo.Length < 12)
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid Aadhar number."));

                // Validate Consumer ID
                if (!int.TryParse(req.M_Consumerid, out int M_Consumerid))
                    return BadRequest(new ApiResponse<object>(false, "Invalid consumer ID."));

                // Check if consumer exists
                DataTable consumerData = await _databaseManager.SelectTableDataAsync(
                    "m_consumer",
                    "TOP 1 [M_Consumerid], [aadharkycStatus]",
                    $"[M_Consumerid] = '{M_Consumerid}' ORDER BY [M_Consumerid] DESC"
                );

                if (consumerData.Rows.Count == 0)
                    return BadRequest(new ApiResponse<object>(false, "Consumer record not available."));

                string aadharKycStatus = consumerData.Rows[0]["aadharkycStatus"].ToString();
                if (aadharKycStatus == "Offline" || aadharKycStatus == "Online")
                    return BadRequest(new ApiResponse<object>(false, "KYC is already verified."));

                // Check for duplicate Aadhar
                DataTable duplicateAadharData = await _databaseManager.SelectTableDataAsync(
                    "tblKycAadharDataDetails",
                    "*",
                    $"AadharNo = '{req.AadharNo}' AND IsaadharVerify = 1"
                );

                if (duplicateAadharData.Rows.Count > 0)
                    return BadRequest(new ApiResponse<object>(false, "This Aadhar number is already in use."));

                // Validate OTP with external service
                string result = _databaseManager.ValidateOtpAadhar(
                    req.Request_Id,
                    req.Otp,
                    "in/identity/okyc/otp/verify",
                    "https://live.zoop.one/",
                    "648d7d9a22658f001d0193ac",
                    "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR"
                );

                JObject responseObj = JObject.Parse(result);
                string responseCode = responseObj["response_code"]?.ToString();
                string responseMessage = responseObj["response_message"]?.ToString();

                if (!string.IsNullOrEmpty(responseCode) && responseCode != "0")
                {
                    return BadRequest(new ApiResponse<object>(false, responseMessage ?? "Error occurred while validating OTP."));
                }

                // Handle successful OTP validation
                if (responseObj["success"]?.ToString() == "True")
                {
                    string aadharName = responseObj["result"]["user_full_name"]?.ToString().ToUpper() ?? string.Empty;

                    // Update consumer record with KYC details
                    string updateConsumerQuery = $@"
                        UPDATE m_consumer 
                        SET aadharkycStatus = 'Online', 
                            aadharNumber = '{req.AadharNo}', 
                            AadharHolderName = '{aadharName}' 
                        WHERE M_Consumerid = {M_Consumerid}";
                    await _databaseManager.ExecuteNonQueryAsync(updateConsumerQuery);

                    // Insert KYC details into history table
                    string insertHistoryQuery = $@"
                        INSERT INTO tblKycAadharDataDetailsHistry 
                        (KycMode, M_Consumerid, AadharNo, AadharName, AadharRefrenceId, AadharReqdate, AadharRemarks, ResponseCode, IsaadharVerify) 
                        VALUES 
                        ('Online', '{M_Consumerid}', '{req.AadharNo}', '{aadharName}', '{responseObj["request_id"]}', 
                        GETDATE(), '{responseMessage}', '{responseCode}', 1)";
                    await _databaseManager.ExecuteNonQueryAsync(insertHistoryQuery);

                    return Ok(new ApiResponse<object>(true, "KYC verified successfully."));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Failed to verify KYC. Please try again."));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while validating OTP: {ex.Message}"));
            }
        }
    }

    public class ValidateOTPForKYCAadharClass
    {
        public string M_Consumerid { get; set; }
        public string AadharNo { get; set; }
        public string Request_Id { get; set; }
        public string Otp { get; set; }
    }
}

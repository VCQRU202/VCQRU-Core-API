using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidateOTPForLoginController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public ValidateOTPForLoginController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP([FromBody] ValidateOTPForLoginClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            try
            {
                // Validate mobile number
                if (string.IsNullOrEmpty(req.Mobile) || req.Mobile.Length < 10)
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid mobile number."));

                string mobile = req.Mobile.Substring(req.Mobile.Length - 10);
                int otpCode = req.otpCode;

                // Check for predefined test numbers
                if ((mobile == "2233445566" || mobile == "8800001122" || mobile == "2000000022" || mobile == "2366998877") && otpCode == 4321)
                {
                    await _databaseManager.ExecuteNonQueryAsync($"UPDATE COMPANYPRODUCT SET status = 1 WHERE status = 0 AND RIGHT(MobileNumber, 10) = '{mobile}'");

                    string query = $"SELECT TOP 1 * FROM M_Consumer WHERE RIGHT(MobileNo, 10) = '{mobile}' AND IsDelete = 0";
                    DataTable mconData = await _databaseManager.ExecuteDataTableAsync(query);

                    if (mconData.Rows.Count > 0)
                    {
                        var response = MapConsumerData(mconData.Rows[0]);
                        return Ok(new ApiResponse<object>(true, "OTP validated successfully.", response));
                    }
                }

                // Validate OTP from database
                string selectQueryOtp = $"SELECT TOP 1 otp FROM COMPANYPRODUCT WHERE RIGHT(MobileNumber, 10) = '{mobile}' AND status = 0 ORDER BY expiryDate DESC";
                DataTable dataOtp = await _databaseManager.ExecuteDataTableAsync(selectQueryOtp);

                if (dataOtp.Rows.Count > 0 && dataOtp.Rows[0]["otp"].ToString() == otpCode.ToString())
                {
                    await _databaseManager.ExecuteNonQueryAsync($"UPDATE COMPANYPRODUCT SET status = 1 WHERE status = 0 AND RIGHT(MobileNumber, 10) = '{mobile}'");

                    string query = $"SELECT TOP 1 * FROM M_Consumer WHERE RIGHT(MobileNo, 10) = '{mobile}' AND IsDelete = 0";
                    DataTable mconData = await _databaseManager.ExecuteDataTableAsync(query);

                    if (mconData.Rows.Count > 0)
                    {
                        var response = MapConsumerData(mconData.Rows[0]);
                        return Ok(new ApiResponse<object>(true, "OTP validated successfully.", response));
                    }
                    else
                    {
                        var response = new
                        {
                            M_consumerid = "",
                            User_ID = "",
                            ConsumerName = "",
                            Email = "",
                            MobileNo = "",
                            City = ""
                        };
                        return Ok(new ApiResponse<object>(true, "User does not exist.", response));
                    }
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "Invalid OTP"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while validating OTP: {ex.Message}"));
            }
        }

        private object MapConsumerData(DataRow row)
        {
            return new
            {
                M_consumerid = Convert.ToInt32(row["M_Consumerid"]),
                User_ID = row["User_ID"]?.ToString(),
                ConsumerName = row["ConsumerName"]?.ToString(),
                Email = row["Email"]?.ToString(),
                MobileNo = row["MobileNo"]?.ToString(),
                City = row["City"]?.ToString()
            };
        }
    }

    public class ValidateOTPForLoginClass
    {
        public int otpCode { get; set; }
        public string Mobile { get; set; }
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

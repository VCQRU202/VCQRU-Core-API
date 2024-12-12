using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using CoreApi_BL_App.Services;
using Azure;

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
                if (string.IsNullOrEmpty(req.Mobile) || req.Mobile.Length < 10)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid mobile number."));
                }

                string mobile = req.Mobile.Substring(req.Mobile.Length - 10);
                string otpCode = req.otpCode;

                // Check for predefined test numbers
                if ((mobile == "2233445566" || mobile == "8800001122" || mobile == "2000000022" || mobile == "2366998877") && otpCode.Trim() == "4321")
                {
                    // Query to fetch consumer details
                    _databaseManager.ExecuteNonQueryAsync($"update COMPANYPRODUCT set status = 1 where status = '0' and right(MobileNumber,10) = '{mobile}'");

                    string query = $"SELECT TOP 1 * FROM M_Consumer WHERE RIGHT([MobileNo], 10) = '{mobile}' AND IsDelete = 0";

                    DataTable mconData = await _databaseManager.ExecuteDataTableAsync(query);
                    if (mconData.Rows.Count > 0)
                    {
                        DataRow row = mconData.Rows[0];
                        // Map the retrieved data to User_Details
                        var response = new
                        {
                            M_consumerid = Convert.ToInt32(row["M_Consumerid"]),
                            User_ID = row["User_ID"]?.ToString(),
                            ConsumerName = row["ConsumerName"]?.ToString(),
                            Email = row["Email"]?.ToString(),
                            MobileNo = row["MobileNo"]?.ToString(),
                            City = row["City"]?.ToString()
                        };

                        Dictionary<string, object> inputParameters = new Dictionary<string, object>
                                      {
                                          { "@User_ID", response.M_consumerid }
                                      };
                        List<string> outputParameters = new List<string> { "@User_ID", "@M_Consumerid" };
                        DataTable profiledata = await _databaseManager.ExecuteStoredProcedureDataTableAsync("PROC_appGetUserDetails", inputParameters);
                        return Ok(new ApiResponse<object>(true, "OTP validated successfully.!", response));
                        
                    }
                }

                string selectQueryOtp = "SELECT top 1 otp FROM COMPANYPRODUCT WHERE right(MobileNumber,10) = '" + mobile.Substring(mobile.Length - 10).ToString() + "'  and status=0 order by  [expiryDate] Desc, DateAdd(Second, -1, Cast([expiryDate] as time)) desc";
                DataTable dataOtp = await _databaseManager.ExecuteDataTableAsync(selectQueryOtp);
                
                if (dataOtp.Rows.Count > 0)
                {
                    string otp = dataOtp.Rows[0]["otp"].ToString();
                    if (otp == otpCode)
                    {
                        _databaseManager.ExecuteNonQueryAsync($"update COMPANYPRODUCT set status = 1 where status = '0' and right(MobileNumber,10) = '{mobile}'");


                        var userDetails = new User_Details();
                        string query = $"SELECT TOP 1 * FROM M_Consumer WHERE RIGHT([MobileNo], 10) = '{mobile}' AND IsDelete = 0";

                        DataTable mconData = await _databaseManager.ExecuteDataTableAsync(query);
                        if (mconData.Rows.Count > 0)
                        {
                            DataRow row = mconData.Rows[0];
                            // Map the retrieved data to User_Details
                            var response = new
                            {
                                M_consumerid = Convert.ToInt32(row["M_Consumerid"]),
                                User_ID = row["User_ID"]?.ToString(),
                                ConsumerName = row["ConsumerName"]?.ToString(),
                                Email = row["Email"]?.ToString(),
                                MobileNo = row["MobileNo"]?.ToString(),
                                City = row["City"]?.ToString()
                            };

                            Dictionary<string, object> inputParameters = new Dictionary<string, object>
                                      {
                                          { "@User_ID", response.M_consumerid }
                                      };
                            List<string> outputParameters = new List<string> { "@User_ID", "@M_Consumerid" };
                            DataTable profiledata = await _databaseManager.ExecuteStoredProcedureDataTableAsync("PROC_appGetUserDetails", inputParameters);
                            return Ok(new ApiResponse<object>(true, "OTP validated successfully.", response));
                            //return Ok(new ApiResponse<User_Details>(true, "OTP validated successfully.!", userDetails));
                        }
                        else
                        {
                            var response = new
                            {
                                M_consumerid = "",
                                User_ID = "",
                                ConsumerName = "",
                                Email = "",
                                MobileNo ="",
                                City = ""
                            };

                            // Map the retrieved data to User_Details
                      

                          
                          return Ok(new ApiResponse<object>(false, "User is not exists!.", response));

                        }
                        
                        //return Ok(new ApiResponse<User_Details>(true, "OTP validated successfully.!", userDetails));
                        }
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "Invalid OTP"));
                }
                return BadRequest(new ApiResponse<object>(false, "Invalid Otp !."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while validating OTP: {ex.Message}"));
            }
        }
    }

    public class ValidateOTPForLoginClass
    {
        public string otpCode { get; set; }
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

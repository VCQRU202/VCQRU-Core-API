using Microsoft.AspNetCore.Http;
using CoreApi_BL_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using CoreApi_BL_App.Services;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Text;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendOTPLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseManager _databaseManager;

        public SendOTPLoginController(IConfiguration configuration, DatabaseManager databaseManager)
        {
            _configuration = configuration;
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Data is null."));

            try
            {
                string compName = req.compName;
                string mobile = req.mobile;
                if (string.IsNullOrEmpty(mobile) || mobile.Length < 10)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid mobile number."));
                }

                mobile = mobile.Substring(mobile.Length - 10);

                // Check for deleted account
                DataTable dt = await _databaseManager.SelectTableDataAsync("[M_Consumer]", "[User_ID],[MobileNo],[Email],Password", $"RIGHT([MobileNo],10) = '{mobile}' AND IsDelete=0");
                if (dt.Rows.Count == 0 && compName == "Milton abrasives pvt ltd")
                {
                    return BadRequest(new ApiResponse<object>(false, "User account does not exist!"));
                }

                // Generate OTP
                int rdmNumber = Utility.RandomNumber(1000, 9999);
                string otpMsg = compName switch
                {
                    "BlackCobra" => $"%3C%23%3E Your Employee verification OTP is {rdmNumber} vcqru.com. DNQVEFPOrfl, agvkT8I1MqO",
                    "MAHAVIR METAL INDUSTRIES" => $"Your OTP is {rdmNumber}, kindly enter the OTP to verify your mobile number on the Orio app. DNQVEFPOrfl, https://www.oriobathfittings.com",
                    _ => $"Your Employee verification OTP is {rdmNumber} www.vcqru.com DNQVEFPOrfl",
                };

                // Send OTP
                string url = compName == "MAHAVIR METAL INDUSTRIES"
                    ? $"https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91{mobile}&type=OTP&sender=VCQRUI&body={otpMsg}&template_id=1007983550590342720&&Source=API"
                    : $"https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91{mobile}&type=OTP&sender=VCQRUI&body={otpMsg}&template_id=1007399099979047799&&Source=API";

                await Utility.SendSMS(url);

                // Save OTP to the database
                DateTime expDate = DateTime.Now.AddYears(1);
                string query = $"INSERT INTO CompanyProduct ([expiryDate], [otp], [status],[mobileNumber]) VALUES ('{expDate:yyyy-MM-dd HH:mm:ss}', {rdmNumber}, 0, '91{mobile}')";
                int result = await _databaseManager.ExecuteNonQueryAsync(query);

                if (result > 0)
                {
                    return Ok(new ApiResponse<object>(true, "OTP sent successfully"));
                }
                else
                {
                    return StatusCode(500, new ApiResponse<object>(false, "Failed to save OTP"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error in SendOTP API: {ex.Message}"));
            }
        }
    }

    public class SendOTPClass
    {
        public string compName { get; set; }
        public string mobile { get; set; }
    }

    public static class Utility
    {
        public static int RandomNumber(int min, int max)
        {
            return Random.Shared.Next(min, max);
        }

        public static async Task SendSMS(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using Stream responseStream = response.GetResponseStream();
                using StreamReader reader = new StreamReader(responseStream);
                await reader.ReadToEndAsync();
            }
        }
    }
}

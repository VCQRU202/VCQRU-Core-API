using Microsoft.AspNetCore.Http;
using CoreApi_BL_App.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using CoreApi_BL_App.Services;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;


namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendOTPLoginController : ControllerBase
    {

        private readonly DatabaseManager _databaseManager;

        public SendOTPLoginController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
        [HttpPost]
        public async Task<IActionResult> SendOTPLogin([FromBody] SendOTPClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Data is null."));
            try
            {

                string compName = req.compName;
                string mobile = req.mobile;
                if (mobile.Length < 10)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter valid mobile number."));
                }
                mobile = mobile.Substring(mobile.Length - 10);

                #region // For deleted account

                DataTable dt = await _databaseManager.SelectTableDataAsync("[M_Consumer]", "[User_ID],[MobileNo],[Email],Password", "right([MobileNo],10) = '" + mobile + "' and IsDelete=0");
                if (dt.Rows.Count == 0 && compName == "Milton abrasives pvt ltd")
                {
                        return BadRequest(new ApiResponse<object>(false, "User account does not exist .!"));
                }
                    
                

                #endregion               
                int rdmNumber = Utility.RandomNumber(1000, 9999);      
                string otpMsg = string.Empty;
                if (compName == "BlackCobra")
                {
                    otpMsg = "%3C%23%3E Your Employee verification OTP is " + rdmNumber + " vcqru.com. DNQVEFPOrfl, agvkT8I1MqO";
                }
                else if (compName == "MAHAVIR METAL INDUSTRIES")
                {
                    otpMsg = "Your OTP is " + rdmNumber + ", kindly enter the OTP to verify your mobile number on the Orio app. DNQVEFPOrfl, https://www.oriobathfittings.com";
                }
                else
                {
                    otpMsg = "Your Employee verification OTP is " + rdmNumber + " www.vcqru.com DNQVEFPOrfl";
                }

               

                #region//** Sent OTP through SMS
                //Utility.SendSMSFromAlfa(mobile, otpMsg, "OTP", compName);

                otpMsg = otpMsg.Replace("&", "%26");
             

                string URL = "";
                if (compName == "MAHAVIR METAL INDUSTRIES")
                { // added by tej
                    URL = String.Format("https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91" + mobile + "&type=OTP&sender=VCQRUI&body=" + otpMsg + "&template_id=1007983550590342720&&Source=API");
                }
                else
                {
                    URL = String.Format("https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91" + mobile + "&type=OTP&sender=VCQRUI&body=" + otpMsg + "&template_id=1007399099979047799&&Source=API");
                }
                DateTime expDate = System.DateTime.Now.AddYears(1);
                // string Result  = await _data
              

                string Result =  _databaseManager.SendOTPLogin(mobile, otpMsg, "OTP", compName, URL);
                var jOBJ = JObject.Parse(Result);
                //string NameMatchScore = "0.00";

                //int count = await _databaseManager("[expiryDate], [otp], [status],[mobileNumber]", "'" + expDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + rdmNumber + "', 0, '" + mobile + "'", "[dbo].[CompanyProduct]");

                string query11 = $"INSERT INTO CompanyProduct ([expiryDate], [otp], [status],[mobileNumber]) VALUES ('" + expDate.ToString("yyyy-MM-dd HH:mm:ss") + "','" + rdmNumber + "', 0, '91" + mobile + "' )";
                int count = await _databaseManager.ExecuteNonQueryAsync(query11);

                #endregion
                return Ok(new ApiResponse<object>(true, "OTP sent successfully"));

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Find Error in SendOTPLogin api with : {ex.Message}"));

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
            return Random.Shared.Next(min, max); // Using Random.Shared for thread-safe generation
        }
      
    }

   

}

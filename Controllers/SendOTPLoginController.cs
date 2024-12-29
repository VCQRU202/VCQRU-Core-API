using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models;
using System.Data;
using CoreApi_BL_App.Models.Vendor;
using Azure;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.AspNetCore.Http.Json;
using System.Net;
using System.Text;
using System;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendOTPLoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SendOTPLoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpPost]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Data is null."));
            var connectionString = _configuration.GetConnectionString("defaultConnectionbeta");
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
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string selectQuery = "SELECT [User_ID],[MobileNo],[Email],Password FROM M_Consumer WHERE right([MobileNo],10) = @mobile  and IsDelete=0";
                    SqlCommand cmd = new SqlCommand(selectQuery, conn);
                     cmd.Parameters.AddWithValue("@mobile", mobile.Substring(mobile.Length - 10));
                    await conn.OpenAsync();
                    SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (compName == "Milton abrasives pvt ltd" && reader.HasRows)
                    {
                        return BadRequest(new ApiResponse<object>(false, "User account does not exist .!"));
                    }
                    
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

                if (mobile.Length == 10)
                    mobile = "91" + mobile;

                #region//** Sent OTP through SMS
             Utility.SendSMSFromAlfa(mobile, otpMsg, "OTP", compName);
                #endregion
                DateTime expDate = System.DateTime.Now.AddYears(1);            
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO CompanyProduct ([expiryDate], [otp], [status],[mobileNumber]) VALUES ('" + expDate.ToString("yyyy-MM-dd HH:mm:ss") + "', '" + rdmNumber + "', 0, '" + mobile + "' )";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    await conn.OpenAsync();
                    var id = await cmd.ExecuteScalarAsync();

                    return Ok(new ApiResponse<object>(true, "OTP sent successfully"));
                }

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
        public static void SendSMSFromAlfa(string sPhoneNo, string sMessage, string msg_type, string compname)
        {
            sMessage = sMessage.Replace("&", "%26");
            if (sPhoneNo.Length == 10)
                sPhoneNo = "+91" + sPhoneNo;
            else if (sPhoneNo.Length == 12)
                sPhoneNo = "+" + sPhoneNo;
            if (msg_type == "OTP")
            {
                string sResponse = "";
                string URL1 = String.Format("http://msgapi.knowlarity.com/api/sms?key=YAD0BJzW&to=91" + sPhoneNo + "&from=VCQRUI&body=" + sMessage + "&entityid=1001407084320227804&templateid=1007399099979047799");
               // string URL = String.Format("https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91" + sPhoneNo + "&type=OTP&sender=VCQRUI&body=" + sMessage + "&template_id=1007399099979047799&&Source=API");
                HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(URL1);
                myReq.Method = "GET";
                HttpWebResponse Webresponse = (HttpWebResponse)myReq.GetResponse();
                Stream rstream = Webresponse.GetResponseStream();
                StreamReader reader1 = new StreamReader(rstream, Encoding.UTF8);
                sResponse = reader1.ReadToEnd();
                reader1.Close();
            }
            //smslog(sResponse);
            

            string URL = "";
            if (compname == "MAHAVIR METAL INDUSTRIES")
            { // added by tej
                URL = String.Format("https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91" + sPhoneNo + "&type=OTP&sender=VCQRUI&body=" + sMessage + "&template_id=1007983550590342720&&Source=API");
            }
            else
            {
                URL = String.Format("https://api.kaleyra.io/v1/HXIN1745186923IN/messages?to=91" + sPhoneNo + "&type=OTP&sender=VCQRUI&body=" + sMessage + "&template_id=1007399099979047799&&Source=API");
            }
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            //            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            //            ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;

                        WebRequest request = WebRequest.Create(URL);
        request.Method = "POST";
                        string postData = "";
        byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        request.ContentType = "application/x-www-form-urlencoded";
                        request.Headers.Add("api-key", "A8630797ed2577e3a9166d386937db77f");
                        request.ContentLength = byteArray.Length;
                        Stream dataStream = request.GetRequestStream();
        dataStream.Write(byteArray, 0, byteArray.Length);
                        dataStream.Close();
                        WebResponse response = request.GetResponse();
        Console.WriteLine(((HttpWebResponse) response).StatusDescription);
                        dataStream = response.GetResponseStream();
                        StreamReader reader = new StreamReader(dataStream);
        string responseFromServer = reader.ReadToEnd();
        Console.WriteLine(responseFromServer);
                        reader.Close();
                        dataStream.Close();
                        response.Close();
            


        }
}

   

}

using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendOTPForKYCAadharController : ControllerBase
    {


        private readonly DatabaseManager _databaseManager;

        public SendOTPForKYCAadharController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> SendOTPForKYCAadhar([FromBody] SendOTPForKYCAadharClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                if (string.IsNullOrEmpty(req.AadharNo) || req.AadharNo.Length < 12)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid aadharNo number."));
                }            

                if (!int.TryParse(req.M_Consumerid, out int M_Consumerid))
                    return BadRequest(new ApiResponse<object>(false, "Invalid consumer ID."));


                DataTable dt = new DataTable();
                // dt2 = db.SelectTableData("m_consumer", "top 1 [M_Consumerid],[aadharkycStatus]", "[M_Consumerid] = '" + M_Consumerid.ToString() + "' order by  [M_Consumerid] Desc");
                string query1 = $"select top 1 [M_Consumerid],[aadharkycStatus] from m_consumer where [M_Consumerid] = '" + M_Consumerid.ToString() + "' order by  [M_Consumerid] Desc";
                dt = await _databaseManager.ExecuteDataTableAsync(query1);
                verifyKycDataDetail verifyKycDataDetail = new verifyKycDataDetail();
                if (dt.Rows.Count <= 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Record Not available."));
                }
                else if (dt.Rows[0]["aadharkycStatus"].ToString() == "Ofline")
                {
                    return BadRequest(new ApiResponse<object>(false, "Kyc alerady varified."));
                }
                else if (dt.Rows[0]["aadharkycStatus"].ToString() == "Online")
                {
                    return BadRequest(new ApiResponse<object>(false, "Kyc alerady varified."));
                }
                DataTable Alrddt = new DataTable();
                string querydt2 = " select * from tblKycAadharDataDetails where AadharNo='" + req.AadharNo + "' and IsaadharVerify=1";
                Alrddt = await _databaseManager.ExecuteDataTableAsync(querydt2);
                if (Alrddt.Rows.Count > 0)
                {
                    return BadRequest(new ApiResponse<object>(false, req.AadharNo + "This Aadhar number is already in use, Please try with another!"));

                }


                DataTable dt4 = new DataTable();
                //dt4 = db.SelectTableData("tblKycAadharDataDetails", "top 1 [M_Consumerid]", "[M_Consumerid] = '" + M_Consumerid.ToString() + "' and IsaadharVerify='0'  order by  [M_Consumerid] Desc");
                string qrydt4 = "select top 1 [M_Consumerid] from tblKycAadharDataDetails where [M_Consumerid] = '" + M_Consumerid.ToString() + "' and IsaadharVerify='0'  order by  [M_Consumerid] Desc";
                Alrddt = await _databaseManager.ExecuteDataTableAsync(qrydt4);
                if (Alrddt.Rows.Count > 0)
                {
                    await _databaseManager.ExecuteNonQueryAsync($"delete from tblKycAadharDataDetails where M_Consumerid='" + M_Consumerid.ToString() + "'");

                }

                //DataTable Cdt = db.SelectTableData("tblKycAadharDataDetails", " top 1 ReqCount", "M_Consumerid='" + M_Consumerid + "' and Status=1 and IsaadharVerify=1 order by Id desc");
                string qryCdt = "select top 1 ReqCount from tblKycAadharDataDetails where [M_Consumerid] = '" + M_Consumerid.ToString() + "' and Status=1 and IsaadharVerify=1 order by Id desc";
                DataTable Cdt = await _databaseManager.ExecuteDataTableAsync(qryCdt);
                if (Cdt.Rows.Count > 0)
                {
                    int ReqCount = Convert.ToInt32(Cdt.Rows[0]["ReqCount"].ToString());
                    if (ReqCount >= 1)
                    {
                        return BadRequest(new ApiResponse<object>(false, "You have reachared maximum limit : " + ReqCount + ""));

                    }
                }

                //DataTable dt6 = new DataTable();
                //dt6 = db.SelectTableData("tblKycPanDataDetails", " top 1 [M_Consumerid],[InputPanName] ", " [M_Consumerid] = '" + M_Consumerid.ToString() + "'  order by  [M_Consumerid] Desc");
                //string qrydt6 = "select top 1 [M_Consumerid],[InputPanName] from tblKycPanDataDetails where [M_Consumerid] = '" + M_Consumerid.ToString() + "' order by  [M_Consumerid] Desc";
                //dt6 = await _databaseManager.ExecuteDataTableAsync(qrydt6);
                //if (dt6.Rows.Count > 0)
                //{
                //    UserNameForValidatePan = dt6.Rows[0]["InputPanName"].ToString();
                //}
                //else
                //{
                //    return BadRequest(new ApiResponse<object>(false, "Please enter valid name at start !"));

                //}

                string Result = _databaseManager.sendotpAadhar(req.AadharNo, "in/identity/okyc/otp/request", "https://live.zoop.one/", "648d7d9a22658f001d0193ac", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                var jOBJ = JObject.Parse(Result);
                _databaseManager.ExceptionLogs("Aadhar OTP Sent RESPONSE : "+ Result);
                string NameMatchScore = "0.00";


                verifyKycDataDetail.M_Consumerid = req.M_Consumerid;
                verifyKycDataDetail.AadharRefrenceId = jOBJ["request_id"]?.ToString();
                verifyKycDataDetail.AadharRemarks = jOBJ["response_message"]?.ToString();
                verifyKycDataDetail.ResponseCode = jOBJ["response_code"]?.ToString();
                //verifyKycDataDetail.IsaadharVerify = false;
                verifyKycDataDetail.Status = false;

                if (jOBJ["success"]?.ToString() == "True" && jOBJ["result"]?["is_otp_sent"]?.ToString() == "True")
                {
                    verifyKycDataDetail.AadharRefrenceId = jOBJ["request_id"]?.ToString();
                    verifyKycDataDetail.AadharRemarks = jOBJ["response_message"]?.ToString();
                    verifyKycDataDetail.ResponseCode = jOBJ["ResponseCode"]?.ToString();
                    verifyKycDataDetail.Status = true;
                    //verifyKycDataDetail.IsaadharVerify = true;
                }
                /*
                var client = new RestClient("https://live.zoop.one/in/identity/okyc/otp/request");
                client.Timeout = -1;
                var request = new RestRequest(Method.Post);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.AddHeader("app-id", "648d7d9a22658f001d0193ac");
                request.AddHeader("api-key", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                request.AddHeader("Content-Type", "application/json");
                var body = "{\"mode\":\"sync\",\"data\":{\"customer_aadhaar_number\":\"" + req.AadharNo + "\",\"consent\":\"Y\",\"consent_text\":\"Approve_the_values_here\"}}";
                            
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                */

                //IRestResponse response = client.Execute(request);

                //await _databaseManager.ExceptionLogs("Send OTP For KYC | Body |" + jOBJ + " | Response | " + jOBJ.Content + " | StatusDescription " + response.ErrorMessage);

                //if (response.Content == "")
                //{
                //    return BadRequest(new ApiResponse<object>(false, "Api response issue, please wait and try later some time!"));
                //}
                //JObject jOBJ  = JObject.Parse(Result);
                string statusCode = jOBJ["response_code"].ToString();
                if (statusCode == "101")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid Aadhar Number"));
                }
                if (statusCode == "102")
                {
                    return BadRequest(new ApiResponse<object>(false, "Multiple Records Found"));
                }
                if (statusCode == "103")
                {
                    return BadRequest(new ApiResponse<object>(false, "Partial Record Found!"));
                }
                if (statusCode == "106")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid Aadhar Number!"));
                }
                if (statusCode == "105" || statusCode == "104")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid Aadhar Number!"));
                }
                if (statusCode == "107")
                {
                    return BadRequest(new ApiResponse<object>(false, "Duplicate Transaction!"));
                }
                if (statusCode == "108" || statusCode == "109" || statusCode == "110" || statusCode == "111")
                {
                    return BadRequest(new ApiResponse<object>(false, "Server Down!"));
                }
                if (statusCode == "99")
                {
                    return BadRequest(new ApiResponse<object>(false, "Unknown Error!"));
                }
                if (statusCode == "113")
                {
                    return BadRequest(new ApiResponse<object>(false, "Something went wrong, Please contact to service provider!"));
                }
                if (statusCode == "106")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid Aadhar Number!"));
                }

                if (jOBJ["result"]["is_aadhaar_valid"].ToString() != "True")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid aadhar number!"));
                }


                if (jOBJ["result"]["is_number_linked"].ToString() != "True")
                {


                    //int a = await _databaseManager.InsertAsync("KycMode,M_Consumerid,req.AadharNo,AadharReqdate,IsaadharVerify", " '" + req.kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "','" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','0'", "tblKycAadharDataDetailsHistry");
                     int a = await _databaseManager.ExecuteNonQueryAsync($"insert into tblKycAadharDataDetailsHistry(KycMode,M_Consumerid,req.AadharNo,AadharReqdate,IsaadharVerify)values('" + req.kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '0')");

                    if (a == 1)
                    {
                        DataTable dt3 = new DataTable();
                        dt3 = await _databaseManager.SelectTableDataAsync("tblKycAadharDataDetails", "top 1 [M_Consumerid]", "[M_Consumerid] = '" + M_Consumerid.ToString() + "'  order by  [M_Consumerid] Desc");
                        //string querydt3 = $"SELECT top 1 [M_Consumerid] FROM tblKycAadharDataDetails WHERE [M_Consumerid] = '" + M_Consumerid.ToString() + "'  order by  [M_Consumerid] Desc";

                        // dt3 = await _databaseManager.ExecuteDataTableAsync(dt3);
                        if (dt3.Rows.Count > 0)
                        {
                            await _databaseManager.UpdateAsync("KycMode='" + req.kycMode + "',req.AadharNo='" + req.AadharNo + "',AadharReqdate='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',IsaadharVerify='0' ", "M_Consumerid='" + M_Consumerid.ToString() + "' ", "tblKycAadharDataDetails");
                            //_databaseManager.ExecuteNonQueryAsync($"update tblKycAadharDataDetails set KycMode='" +kycMode + "',req.AadharNo='" + req.AadharNo + "',AadharReqdate='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',IsaadharVerify='0' where M_Consumerid='" + M_Consumerid.ToString() + "' ");

                            return BadRequest(new ApiResponse<object>(false, "Mobile number not linked!"));
                        }
                        else
                        {
                            //int b = await _databaseManager.InsertAsync("KycMode,M_Consumerid,req.AadharNo,AadharReqdate,IsaadharVerify", " '" + req.kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '0'", "tblKycAadharDataDetails");
                            _databaseManager.ExecuteNonQueryAsync($"insert into tblKycAadharDataDetails(KycMode,M_Consumerid,req.AadharNo,AadharReqdate,IsaadharVerify)values('" +req.kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', '0')");

                            return BadRequest(new ApiResponse<object>(false, "Mobile number not linked!"));
                        }


                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Record not inserted!."));

                    }
                    // return BadRequest(new ApiResponse<object>(false, "Invalid Otp !."));


                }


                if (jOBJ["result"]["is_otp_sent"].ToString() != "True")
                {
                    return BadRequest(new ApiResponse<object>(false, "OTP not sent!"));

                }

                if (jOBJ["success"].ToString() == "True" && jOBJ["response_code"].ToString() == "100")
                {
                    return Ok(new ApiResponse<object>(true, "OTP send successfully!", verifyKycDataDetail));

                }
                else if (jOBJ["success"].ToString() == "False" || jOBJ["response_code"].ToString() == "101")
                {
                    return BadRequest(new ApiResponse<object>(false, jOBJ["response_message"].ToString()));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, jOBJ["response_message"].ToString()));

                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while Send OTP For KYC Aadhar: {ex.Message}"));
            }
        }
    }

    public class SendOTPForKYCAadharClass
    {
        public string M_Consumerid { get; set; }
        public string AadharNo { get; set; }
        public Boolean kycMode { get; set; }
     


    }



}

using Azure;
using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data;
using System.Net;
using System.Reflection;
using static System.Net.WebRequestMethods;

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
                

                if (string.IsNullOrEmpty(req.AadharNo) || req.AadharNo.Length < 12)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter a valid aadharNo number."));
                }
                string UserNameForValidatePan = "";
                string kycMode = "Offline";


                if (!int.TryParse(req.M_Consumerid, out int M_Consumerid))
                    return BadRequest(new ApiResponse<object>(false, "Invalid consumer ID."));
                DataTable dt = new DataTable();
                dt = await _databaseManager.SelectTableDataAsync("m_consumer", " top 1 [M_Consumerid],[aadharkycStatus]", "[M_Consumerid] = '" + req.M_Consumerid.ToString() + "' order by  [M_Consumerid] Desc");
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
                Alrddt = await _databaseManager.SelectTableDataAsync("tblKycAadharDataDetails", "*", "AadharNo='" + req.AadharNo + "' and IsaadharVerify=1");
                if (Alrddt.Rows.Count > 0)
                {
                    return BadRequest(new ApiResponse<object>(false, req.AadharNo + "This Aadhar number is already in use, Please try with another!"));
                }


                DataTable dt4 = new DataTable();
                dt4 = await _databaseManager.SelectTableDataAsync("tblKycAadharDataDetails", "top 1 [M_Consumerid]", "[M_Consumerid] = '" + M_Consumerid.ToString() + "' and IsaadharVerify='0'  order by  [M_Consumerid] Desc");
                if (dt4.Rows.Count > 0)
                {
                    await _databaseManager.DeleteAsync("M_Consumerid='" + M_Consumerid.ToString() + "' ", "tblKycAadharDataDetails");
                }

                DataTable Cdt = await _databaseManager.SelectTableDataAsync("tblKycAadharDataDetails", " top 1 ReqCount", "M_Consumerid='" + M_Consumerid + "'and Status=1 and IsaadharVerify=1 order by Id desc");
                if (Cdt.Rows.Count > 0)
                {
                    int ReqCount = Convert.ToInt32(Cdt.Rows[0]["ReqCount"].ToString());
                    if (ReqCount >= 1)
                    {
                        return BadRequest(new ApiResponse<object>(false, "You have reachared maximum limit : " + ReqCount + ""));
                    }
                }

                //DataTable dt6 = new DataTable();
                //dt6 = await _databaseManager.SelectTableDataAsync("tblKycPanDataDetails", " top 1 [M_Consumerid],[InputPanName] ", " [M_Consumerid] = '" + M_Consumerid.ToString() + "'  order by  [M_Consumerid] Desc");
                //if (dt6.Rows.Count > 0)
                //{
                //   UserNameForValidatePan = dt6.Rows[0]["InputPanName"].ToString();
                //}
                //else
                //{
                //    return BadRequest(new ApiResponse<object>(false, "Please enter valid name at start !"));
                //}

                string Result = _databaseManager.validateotpAadhar(req.Request_Id, req.Otp, "in/identity/okyc/otp/verify", "https://live.zoop.one/", "648d7d9a22658f001d0193ac", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                var jOBJ = JObject.Parse(Result);
                string NameMatchScore = "0.00";


                verifyKycDataDetail.M_Consumerid = req.M_Consumerid;
                verifyKycDataDetail.AadharRefrenceId = jOBJ["request_id"]?.ToString();
                verifyKycDataDetail.AadharRemarks = jOBJ["response_message"]?.ToString();
                verifyKycDataDetail.ResponseCode = jOBJ["response_code"]?.ToString();
                verifyKycDataDetail.IsaadharVerify = false;
                verifyKycDataDetail.Status = false;


               
                string statusCode = jOBJ["response_code"].ToString();
                if (statusCode == "101")
                {
                    return BadRequest(new ApiResponse<object>(false, "No Record Found!"));
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
                    return BadRequest(new ApiResponse<object>(false, "Invalid OTP!"));
                }
                if (statusCode == "105" || statusCode == "105" || statusCode == "104")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid OTP!"));
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
                if (statusCode == "114")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid OTP!."));
                }
                if (jOBJ["response_code"].ToString() == "112")
                {
                    await _databaseManager.DeleteAsync("M_Consumerid='" + M_Consumerid.ToString() + "' ", "tblKycPanDataDetails");
                    return BadRequest(new ApiResponse<object>(false, "Please wait and try some time later"));
                   
                }
               
                if (jOBJ["success"].ToString() == "True")
                {
                    kycMode = "Online";
                    verifyKycDataDetail.AadharRefrenceId = jOBJ["request_id"]?.ToString();
                    verifyKycDataDetail.AadharRemarks = jOBJ["response_message"]?.ToString();
                    verifyKycDataDetail.ResponseCode = jOBJ["ResponseCode"]?.ToString();
                    verifyKycDataDetail.AadharName = jOBJ["result"]["user_full_name"].ToString();
                    string AadharName = verifyKycDataDetail.AadharName;
                    AadharName = AadharName.ToUpper();
                    string MPanName = UserNameForValidatePan.ToUpper();
                    verifyKycDataDetail.Status = true;
                    verifyKycDataDetail.IsaadharVerify = true;

                    await _databaseManager.UpdateAsync("aadharkycStatus='" + kycMode + "',aadharNumber='" + req.AadharNo + "',AadharHolderName= '" + jOBJ["result"]["user_full_name"].ToString() + "' ", "M_Consumerid='" + M_Consumerid.ToString() + "' ", "M_Consumer");
                    //int a = await _databaseManager.InsertAsync("KycMode,M_Consumerid,AadharNo,AadharName,AadharRefrenceId,AadharReqdate,AadharRemarks,ResponseCode,IsaadharVerify", " '" + kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "','" + verifyKycDataDetail.AadharName + "','" + verifyKycDataDetail.AadharRefrenceId + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verifyKycDataDetail.AadharRemarks + "','" + verifyKycDataDetail.ResponseCode + "', '" + verifyKycDataDetail.IsaadharVerify + "'", "tblKycAadharDataDetailsHistry");
                    string query11 = $"INSERT INTO tblKycAadharDataDetailsHistry (KycMode,M_Consumerid,AadharNo,AadharName,AadharRefrenceId,AadharReqdate,AadharRemarks,ResponseCode,IsaadharVerify) VALUES ('" + kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "','" + verifyKycDataDetail.AadharName + "','" + verifyKycDataDetail.AadharRefrenceId + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verifyKycDataDetail.AadharRemarks + "','" + verifyKycDataDetail.ResponseCode + "', '" + verifyKycDataDetail.IsaadharVerify + "' )";
                    int a = await _databaseManager.ExecuteNonQueryAsync(query11);

                    if (a == 1)
                    {
                        DataTable dt3 = new DataTable();
                        dt3 = await _databaseManager.SelectTableDataAsync("tblKycAadharDataDetails", "top 1 [M_Consumerid]", "[M_Consumerid] = '" + M_Consumerid.ToString() + "'  order by  [M_Consumerid] Desc");
                        if (dt3.Rows.Count > 0)
                        {
                            await _databaseManager.UpdateAsync("KycMode='" + kycMode + "',AadharNo='" + req.AadharNo + "',AadharName='" + verifyKycDataDetail.AadharName + "',AadharRefrenceId='" + verifyKycDataDetail.AadharRefrenceId + "',AadharReqdate='" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "',AadharRemarks='" + verifyKycDataDetail.AadharRemarks + "',ResponseCode='" + verifyKycDataDetail.ResponseCode + "',IsaadharVerify='" + verifyKycDataDetail.IsaadharVerify + "' ", "M_Consumerid='" + M_Consumerid.ToString() + "' ", "tblKycAadharDataDetails");
                            return Ok(new ApiResponse<object>(true, "Verified successfully", verifyKycDataDetail));
                        }
                        else
                        {
                            //int b = await _databaseManager.InsertAsync("KycMode,M_Consumerid,AadharNo,AadharName,AadharRefrenceId,AadharReqdate,AadharRemarks,ResponseCode,IsaadharVerify", " '" + kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "','" + verifyKycDataDetail.AadharName + "','" + verifyKycDataDetail.AadharRefrenceId + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verifyKycDataDetail.AadharRemarks + "','" + verifyKycDataDetail.ResponseCode + "', '" + verifyKycDataDetail.IsaadharVerify + "'", "tblKycAadharDataDetails");

                            string query1 = $"INSERT INTO tblKycAadharDataDetails (KycMode,M_Consumerid,AadharNo,AadharName,AadharRefrenceId,AadharReqdate,AadharRemarks,ResponseCode,IsaadharVerify) VALUES ('" + kycMode + "', '" + M_Consumerid.ToString() + "','" + req.AadharNo + "','" + verifyKycDataDetail.AadharName + "','" + verifyKycDataDetail.AadharRefrenceId + "', '" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "','" + verifyKycDataDetail.AadharRemarks + "','" + verifyKycDataDetail.ResponseCode + "', '" + verifyKycDataDetail.IsaadharVerify + "' )";
                            int b = await _databaseManager.ExecuteNonQueryAsync(query1);
                            return Ok(new ApiResponse<object>(true, "Verified successfully", verifyKycDataDetail));
                        }                     
                        
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Record not inserted!."));
                    }


                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, jOBJ["response_message"]?.ToString())) ;
                }

            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Validate OTP for aadhar kyc API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
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
  
   

    //public class ApiResponse<T>
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //    public T Data { get; set; }

    //    public ApiResponse(bool success, string message, T data = default)
    //    {
    //        Success = success;
    //        Message = message;
    //        Data = data;
    //    }
    //}


}

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
            string dbConsumername;
            string AadharName;
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
               // string Result = "{\"request_id\":\"d15bff39-e31d-42ee-9280-8d86617a31e5\",\"task_id\":\"706eb1c2-382b-492b-b4d2-c585b23f800d\",\"group_id\":\"c5df5b3f-fdbd-40ad-9c1e-2929484eac0f\",\"success\":true,\"response_code\":\"100\",\"response_message\":\"Valid Authentication\",\"result\":{\"user_full_name\":\"Subhash Baban Parihar\",\"user_aadhaar_number\":\"********2586\",\"user_dob\":\"27/7/1996\",\"user_gender\":\"M\",\"user_address\":{\"country\":\"India\",\"dist\":\"Beed\",\"state\":\"Maharashtra\",\"po\":\"Beed\",\"loc\":\"AT kalegao post nalvandi\",\"vtc\":\"Bid\",\"subdist\":\"Bid\",\"street\":\"\",\"house\":\"\",\"landmark\":\"\"},\"address_zip\":\"431122\",\"user_profile_image\":\"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\",\"user_has_image\":true,\"aadhaar_xml_raw\":\"https://storage.googleapis.com/zoop-okyc-document-prod/d15bff39-e31d-42ee-9280-8d86617a31e5-2024-12-22T18%3A38%3A25.544Z.xml?GoogleAccessId=okyc-production%40zoop-production.iam.gserviceaccount.com&Expires=1734899906&Signature=NGKltUfnXm78eO8YM4uSzJjAL76Gy55s4wnDQeo3JITpvbVEdP3Dk%2B8%2F1d1UeC1kAzJMwSSzPnInXsDqrysW3PRXDDQgcQKUs2SW9ILsCj1gYoYkON0HYpwlFiPd0WobX3ERrzsSEyQTM%2BE5C%2BM1obX%2FllfeH3j70anFJDdJ9r6JRLhlXOul0WdU7JRy%2B%2BEvEfOSXordB0uc9WFwnb46t7jeTCpyQQhnH77wDqa7wy8WMqt9idrh6AsbmuF3WvW09nIjzKorw%2BDgahJzVlaeIMd44TG583Po%2FEcKzlF3PxaaAsFsUmjLp93BjaAsJ0CAZmKp2b8f5jN378iDuHloKQ%3D%3D\",\"user_zip_data\":\"https://storage.googleapis.com/zoop-okyc-document-prod/d15bff39-e31d-42ee-9280-8d86617a31e5-2024-12-22T18%3A38%3A25.815Z.zip?GoogleAccessId=okyc-production%40zoop-production.iam.gserviceaccount.com&Expires=1734899906&Signature=SJa%2FUwY89sK%2FTyvV8a%2FktsVqxiTgSHDh5%2FkQXp75bCOqqejykseVXo017ZqVwB0411wv2DI9MOVLuaLOkFN8l7KPYPPV%2BD3drIf97s1fu58rMaVtJyxj1PfAvhrxjeNHapSWVZLy9j%2BT2ZZZrG04IDe8n18SCoFci8P%2BesEv9o36LGmsk3bmJB5oPmX7Y50l9PG%2FBr9wlQUMhYvjtcI5MCMbaFsCKtidiNYgyWv9rxveLzeeHERj09u4xF8TWGoaHA%2F8dMN2xpFqYetoGgWLjOCjWB%2Fo9QgdAydlPVUe7FSBgKrWmEfvBoxLu7JMbV%2BQVrTc6cwKng9KqEfORPE5tA%3D%3D\",\"user_parent_name\":\"S/O: Baban Parihar\",\"aadhaar_share_code\":\"2406\",\"user_mobile_verified\":false,\"reference_id\":\"258620241223000825074\"},\"metadata\":{\"billable\":\"Y\",\"reason_code\":\"TXN\",\"reason_message\":\"Transaction Successful\"},\"request_timestamp\":\"2024-12-22T18:38:23.903Z\",\"response_timestamp\":\"2024-12-22T18:38:26.322Z\"}";
                string Result = _databaseManager.validateotpAadhar(req.Request_Id, req.Otp, "in/identity/okyc/otp/verify", "https://live.zoop.one/", "648d7d9a22658f001d0193ac", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                var jOBJ = JObject.Parse(Result);
                string NameMatchScore = "0.00";
                _databaseManager.ExceptionLogs("Aadhar OTP Validate RESPONSE : " + Result);

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
                     AadharName = verifyKycDataDetail.AadharName;
                    AadharName = AadharName.ToUpper();
                    string MPanName = UserNameForValidatePan.ToUpper();
                    verifyKycDataDetail.Status = true;
                    verifyKycDataDetail.IsaadharVerify = true;

                    #region Validattion process 
                    string consumerqry = $"select*from m_Consumer where M_Consumerid='{req.M_Consumerid}' and IsDelete=0";
                    var Consumerdata = await _databaseManager.ExecuteDataTableAsync(consumerqry);
                    if (Consumerdata.Rows.Count > 0)
                    {
                         dbConsumername= Consumerdata.Rows[0]["ConsumerName"].ToString() ;
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Invalid User Details."));
                    }


                    string dataqry = $"select kyc_details from brandsettings where comp_id='{req.Comp_id}'";
                    var resultdt =await _databaseManager.ExecuteDataTableAsync(dataqry);
                    if (resultdt.Rows.Count > 0)
                    {
                        var jdata = JObject.Parse(resultdt.Rows[0][0].ToString());
                        string isaadharenable = jdata["AadharCard"].ToString();
                        string ispanenable = jdata["PANCard"].ToString();
                        string isAccountenable = jdata["AccountDetails"].ToString();
                        string isUPIenable = jdata["UPI"].ToString();
                        if(ispanenable== "Yes")
                        {
                            //dbConsumername = AadharName;
                            double matchPercentage = GetLevenshteinMatchPercentage(dbConsumername, AadharName);

                            if (matchPercentage < 70)
                            {
                                return BadRequest(new ApiResponse<object>(false, "Aadhaar does not match the registered name. Please verify and try again."));
                            }
                        }
                        else if(ispanenable== "No")
                        {
                            string updatequery = $"update M_Consumer set ConsumerName='{AadharName}' where M_Consumerid='{req.M_Consumerid}'";
                            await _databaseManager.ExecuteNonQueryAsync(updatequery);
                        }
                        else if (string.IsNullOrEmpty(ispanenable) && isaadharenable=="Yes")
                        {
                            string updatequery = $"update M_Consumer set ConsumerName='{AadharName}' where M_Consumerid='{req.M_Consumerid}'";
                            await _databaseManager.ExecuteNonQueryAsync(updatequery);
                        }

                    }
                    #endregion

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

        public int LevenshteinDistance(string a, string b)
        {
            var lengthA = a.Length;
            var lengthB = b.Length;
            var distance = new int[lengthA + 1, lengthB + 1];

            for (int i = 0; i <= lengthA; distance[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distance[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
            {
                for (int j = 1; j <= lengthB; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(Math.Min(
                        distance[i - 1, j] + 1,       // Deletion
                        distance[i, j - 1] + 1),      // Insertion
                        distance[i - 1, j - 1] + cost); // Substitution
                }
            }

            return distance[lengthA, lengthB];
        }

        public double GetLevenshteinMatchPercentage(string a, string b)
        {
            int distance = LevenshteinDistance(a, b);
            int maxLength = Math.Max(a.Length, b.Length);

            // Match percentage = (1 - (distance / maxLength)) * 100
            return (1.0 - (double)distance / maxLength) * 100;
        }

    }

    public class ValidateOTPForKYCAadharClass
    {
        public string M_Consumerid { get; set; }
        public string AadharNo { get; set; }
        public string Request_Id { get; set; }
        public string Otp { get; set; }
        public string Comp_id { get; set; }
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

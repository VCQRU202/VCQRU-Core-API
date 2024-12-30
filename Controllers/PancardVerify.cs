using Azure;
using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data.SqlClient;
using System.Net;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PancardVerify : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public PancardVerify(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
       



        [HttpPost]
        public async Task<IActionResult> GetFieldSettings([FromBody] pancardverifyrequest Req)
        {
            try
            {
                string query = $"SELECT TOP 1 ReqCount FROM tblKycPanDataDetails WHERE M_Consumerid = '{Req.M_Consumerid}' AND Status = 1 AND IspanVerify = 1 ORDER BY Id DESC";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(query);

                if (dtconsu.Rows.Count > 0)
                {
                    int ReqCount = Convert.ToInt32(dtconsu.Rows[0]["ReqCount"].ToString());
                    if (ReqCount >= 1)
                    {
                        return StatusCode(404, new ApiResponse<object>(false, $"You have reached the maximum limit: {ReqCount}", null));
                    }
                }

                string query9 = $"SELECT TOP 1 ReqCount FROM tblKycPanDataDetails WHERE pancardNumber = '{Req.Pancard}' AND Status = 1 AND IspanVerify = 1 ORDER BY Id DESC";
                var dtconsu9 = await _databaseManager.ExecuteDataTableAsync(query9);

                if (dtconsu9.Rows.Count > 0)
                {
                    int ReqCount = Convert.ToInt32(dtconsu9.Rows[0]["ReqCount"].ToString());
                    if (ReqCount >= 1)
                    {
                        return StatusCode(404, new ApiResponse<object>(false, $"Pan number already used.", null));
                    }
                }

                string query2 = $"SELECT TOP 1 M_Consumerid FROM m_consumer WHERE M_Consumerid ='{Req.M_Consumerid}' ";
                var dtcon = await _databaseManager.ExecuteDataTableAsync(query2);

                if (dtcon.Rows.Count <= 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid M Consumerid. Please logout and login again in the app!", null));
                }

                string Result = _databaseManager.VerifyPan(Req.Pancard,Req.PanHolderName, "api/v1/in/identity/pan/lite", "https://live.zoop.one/", "648d7d9a22658f001d0193ac", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                var jOBJ = JObject.Parse(Result);
                _databaseManager.ExceptionLogs("pan api response : "+ Result);
                string NameMatchScore = "0.00";

                var verifyKycDataDetail = new verifyKycDataDetail
                {
                    M_Consumerid = Req.M_Consumerid,
                    PanRefrenceId = jOBJ["request_id"]?.ToString(),
                    PanRemarks = jOBJ["response_message"]?.ToString(),
                    ResponseCode = jOBJ["response_code"]?.ToString(),
                    IspanVerify = false,
                    Status = false
                };

                if(verifyKycDataDetail.PanRemarks== "No Record Found")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid pan card number", null));
                }


                if (jOBJ["success"]?.ToString() == "True" && jOBJ["result"]?["pan_status"]?.ToString() == "VALID")
                {
                    verifyKycDataDetail.PanRefrenceId = jOBJ["request_id"]?.ToString();
                    verifyKycDataDetail.PanName = jOBJ["result"]?["user_full_name"]?.ToString();
                    NameMatchScore = jOBJ["result"]?["name_match_score"]?.ToString();
                    verifyKycDataDetail.Status = true;
                    verifyKycDataDetail.IspanVerify = true;
                }

                if (Convert.ToDecimal( jOBJ["result"]?["name_match_score"]) < 70)
                {
                    return BadRequest(new ApiResponse<object>(false, "Input name is not matched with your pan number"));
                }

                string queryinsert = @"
            INSERT INTO tblKycPanDataDetails
            (M_Consumerid, PanName, PanRefrenceId, PanReqdate, PanRemarks, ResponseCode, NameMatchScore, IspanVerify, Status,pancardNumber,InputPanName)
            VALUES
            (@M_Consumerid, @PanName, @PanRefrenceId, @PanReqdate, @PanRemarks, @ResponseCode, @NameMatchScore, @IspanVerify, @Status,@pancardNumber,@Inputpanname)";

                var parameters = new Dictionary<string, object>
                    {
                        { "@M_Consumerid", verifyKycDataDetail.M_Consumerid },
                        { "@PanName", verifyKycDataDetail.PanName },
                        { "@PanRefrenceId", verifyKycDataDetail.PanRefrenceId },
                        { "@PanReqdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "@PanRemarks", verifyKycDataDetail.PanRemarks },
                        { "@ResponseCode", verifyKycDataDetail.ResponseCode },
                        { "@NameMatchScore", NameMatchScore },
                        { "@IspanVerify", verifyKycDataDetail.IspanVerify },
                        { "@Status", verifyKycDataDetail.Status },
                       // { "@dateofbirth", Req.Dateofbirth },
                        { "@Inputpanname", Req.PanHolderName },
                        { "@pancardNumber", Req.Pancard }
                    };

                var parametersreturn = new Dictionary<string, object>
                    {
                        { "M_Consumerid", verifyKycDataDetail.M_Consumerid },
                        { "PanName", verifyKycDataDetail.PanName },
                        { "PanRefrenceId", verifyKycDataDetail.PanRefrenceId },
                        { "PanReqdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                        { "PanRemarks", verifyKycDataDetail.PanRemarks },
                        { "ResponseCode", verifyKycDataDetail.ResponseCode },
                        { "NameMatchScore", NameMatchScore },
                        { "IspanVerify", verifyKycDataDetail.IspanVerify },
                        { "pancardNumber", Req.Pancard },
                        { "Status", verifyKycDataDetail.Status }
                    };

                int rowsAffected = await _databaseManager.ExecuteNonQueryAsync(queryinsert, parameters);

                if (rowsAffected > 0)
                {
                    string queryinsert1 = $"update M_Consumer set panekycStatus ='Online',ConsumerName=@consumername where M_Consumerid=@M_Consumerid";
                   var parameters1 = new Dictionary<string, object>
                    {
                        { "@M_Consumerid", verifyKycDataDetail.M_Consumerid },
                        { "@consumername", verifyKycDataDetail.PanName },
                        { "@Status", verifyKycDataDetail.Status }
                    };
                    if (verifyKycDataDetail.Status == false)
                    {
                      return  BadRequest(new ApiResponse<object>(false, verifyKycDataDetail.PanRemarks, parametersreturn));
                    }

                    int rowsAffected1 = await _databaseManager.ExecuteNonQueryAsync(queryinsert1, parameters1);
                    return Ok(new ApiResponse<object>(true, "Successfully verified PAN card.", parametersreturn));
                }

                return StatusCode(500, new ApiResponse<object>(false, "Failed to insert verification data.", null));



                
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Panverify API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
            }
        }


    }

    public class pancardverifyrequest
    {
        public string Pancard { get; set; }
        public string PanHolderName { get; set; }
        public string M_Consumerid { get; set; }
       
    }
}

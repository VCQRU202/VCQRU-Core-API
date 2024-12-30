using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UPI_verification : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public UPI_verification(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> UPIVerify([FromBody] UPIREQUEST Req)
        {
            try
            {
                string query = $"SELECT TOP 1 * FROM tbl_UPIVerificationdata WHERE M_Consumer_id = '{Req.M_Consumerid}' AND Status = 'True' AND ResponseCode ='100' and Responsemsg='Valid Authentication' ORDER BY Id DESC";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(query);

                if (dtconsu.Rows.Count > 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Kyc already verified", null));
                }

                string query1 = $"select*from tbl_UPIVerificationdata   WHERE UPIID='{Req.UPIID}' AND Status = 'True' AND ResponseCode ='100' and Responsemsg='Valid Authentication' ORDER BY Id DESC";
                var dtconsu1 = await _databaseManager.ExecuteDataTableAsync(query);

                if (dtconsu.Rows.Count > 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Entered Upiid already used", null));
                }



                string query2 = $"SELECT TOP 1 * FROM m_consumer WHERE M_Consumerid ='{Req.M_Consumerid}' ";
                var dtcon = await _databaseManager.ExecuteDataTableAsync(query2);

                if (dtcon.Rows.Count <= 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid M Consumerid. please provide valid data!", null));
                }

                string Result = _databaseManager.VerifyUPIKYC(Req.UPIID, "api/v1/in/financial/upi/verification", "https://test.zoop.one/", "63cb411df196f9001e1c2236", "DD4JKV5-EGD4609-KCXBG6P-92ZSCP5");
               // string Result = "{\"request_id\":\"98b26e57-d022-4170-a634-283b1cb16e2f\",\"task_id\":\"0952c232bfb14251afaf55be7eed02df\",\"group_id\":\"9047bb19-d18e-4573-b38b-b69355ba2554\",\"success\":true,\"response_code\":\"100\",\"response_message\":\"Valid Authentication\",\"metadata\":{\"billable\":\"Y\"},\"result\":{\"beneficiary_name\":\"BIPIN MAURYA\"},\"request_timestamp\":\"2024-12-20T11:24:00.745Z\",\"response_timestamp\":\"2024-12-20T11:24:04.591Z\"}";
                _databaseManager.ExceptionLogs("UPI VERIFY API RESPONSE FOR UPI ID : " + Req.UPIID + " Response Data : " + Result);
                var jOBJ = JObject.Parse(Result);

                if (jOBJ["success"]?.ToString().ToUpper() == "TRUE" && jOBJ["response_code"]?.ToString() == "100")
                {
                    string insertquery = $"insert into tbl_UPIVerificationdata (M_Consumer_id,Mobileno,UPIID,Status,ResponseCode,Responsemsg,Benificiryname,APIResponsedata)values('{Req.M_Consumerid}','{dtcon.Rows[0]["mobileno"].ToString()}','{Req.UPIID}','{jOBJ["success"]?.ToString()}','{jOBJ["response_code"]?.ToString()}','{jOBJ["response_message"]?.ToString()}','{jOBJ["result"]?["beneficiary_name"]?.ToString()}','{Result}')";
                    await _databaseManager.ExecuteNonQueryAsync(insertquery);

                    string updateqry = $"update M_Consumer set UPIKYCSTATUS='1',UPIId='{Req.UPIID}' where M_Consumerid='{Req.M_Consumerid}'";
                    await _databaseManager.ExecuteNonQueryAsync(updateqry);
                    Dictionary<string, object> Responsedata = new Dictionary<string, object>
                    {
                        {"M_Consumerid",Req.M_Consumerid },
                        {"Consumername",jOBJ["result"]?["beneficiary_name"]?.ToString() },
                        {"UPIID",Req.UPIID },
                        {"Status",jOBJ["success"]?.ToString().ToUpper() },
                        {"ResponseCode",jOBJ["response_code"]?.ToString() },
                    };
                    return Ok(new ApiResponse<object>(true, "KYC Validate Successfully", Responsedata));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "UPI verification failed"));
                }

            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in UPIVERIFICATION API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
            }
        }

    }

    public class UPIREQUEST
    {
        public string UPIID { get; set; }
        public string M_Consumerid { get; set; }
        public string Comp_id { get; set; }
    }
}

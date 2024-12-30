using Azure;
using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountVerification : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public BankAccountVerification(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetFieldSettings([FromBody] bankaccountrequest Req)
        {
            try
            {
                verifyKycDataDetail verifyKycDataDetail = new verifyKycDataDetail();
                bank_responses Log = new bank_responses();
                if (Req == null)
                    return BadRequest(new ApiResponse<object>(false, "Request data is null."));
                Log.Account_No = Req.AccountNo;
                Log.M_Consumerid = Convert.ToInt32(Req.M_Consumerid);
                Log.IFSC_Code = Req.IFSCCode;
                Log.Entry_Date = DateTime.Now;
                Log.Account_HolderNm = "";
                Log.DML = "I";
                Log.Bank_ID = "";
                Log.Bank_Name = Req.BankName;
                Log.Account_HolderNm = "";
                Log.Branch = "";
                Log.City = "";
                Log.RTGS_Code = "";
                Log.Account_Type = "";
                Log.Flag = false;
                Log.Address = "";
                string kycMode = "Offline";

                string Querystring = $"select top 1 [M_Consumerid],[ConsumerName],[bankekycStatus] from m_consumer where [M_Consumerid] = '{Req.M_Consumerid}' order by  [M_Consumerid] Desc";
                var dtcon = await _databaseManager.ExecuteDataTableAsync(Querystring);
                if (dtcon.Rows.Count < 0)
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid M Consumerid or Please try to login again!", null));
                }
                else if (dtcon.Rows[0]["bankekycStatus"].ToString() == "Ofline" || dtcon.Rows[0]["bankekycStatus"].ToString() == "Online")
                {
                    return BadRequest(new ApiResponse<object>(false, "Kyc alerady varified.", null));
                }
                string Querystring2 = $"select top 1 AccountNo from tblKycBankDataDetails where AccountNo='{Req.AccountNo}' and Status=1 and ResponseCode=100";
                var dtcon2 = await _databaseManager.ExecuteDataTableAsync(Querystring2);
                if (dtcon2.Rows.Count > 0)
                {
                    return BadRequest(new ApiResponse<object>(false, Req.AccountNo + " account number already in use, Please try another account details !", null));
                }

                string Querystring3 = $"select top 1 [M_Consumerid] from tblKycBankDataDetails where [M_Consumerid] = '{Req.M_Consumerid}' and IsBankAccountVerify='1'  order by  [M_Consumerid] Desc";
                var dtcon3 = await _databaseManager.ExecuteDataTableAsync(Querystring3);
                if (dtcon3.Rows.Count > 0)
                {
                    string qurtyupate = $"update M_Consumer set bankekycStatus='Online' where M_Consumerid='{Req.M_Consumerid}'";
                    int rowsAffected1 = await _databaseManager.ExecuteNonQueryAsync(qurtyupate);
                    return BadRequest(new ApiResponse<object>(false, "Bank KYC is already verified!", null));
                }
                string Querystring4 = $"select top 1 ReqCount from tblKycBankDataDetails where M_Consumerid='{Req.M_Consumerid}' and Status=1 and IsBankAccountVerify=1 order by Id desc";
                var dtcon4 = await _databaseManager.ExecuteDataTableAsync(Querystring4);
                if (dtcon4.Rows.Count > 0)
                {
                    string deleteqry = $"delete from tblKycBankDataDetails where M_Consumerid='{Req.M_Consumerid}'";
                    int rowsAffected1 = await _databaseManager.ExecuteNonQueryAsync(deleteqry);
                }
                if (Req.AccountNo.Length < 9 || Req.AccountNo.Length > 18)
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter correct account number!", null));
                }
                if (Req.IFSCCode.Length < 9 || Req.IFSCCode.Length > 12)
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid IFSC Code!", null));
                }

                string Querystring5 = $"select top 1 Bank_ID,Account_HolderNm,Bank_Name,Branch,City,RTGS_Code,Account_Type,Address,Flag from M_BankAccount where [M_Consumerid] ='{Req.M_Consumerid}' order by  Row_ID Desc";
                var dtcon5 = await _databaseManager.ExecuteDataTableAsync(Querystring5);
                if (dtcon5.Rows.Count > 0)
                {
                    Log.DML = "U";
                    Log.Account_HolderNm = dtcon5.Rows[0]["Account_HolderNm"].ToString();
                    Log.Bank_ID = dtcon5.Rows[0]["Bank_ID"].ToString();
                    Log.Bank_Name = string.IsNullOrEmpty(dtcon5.Rows[0]["Bank_Name"].ToString()) ? Log.Bank_Name : dtcon5.Rows[0]["Bank_Name"].ToString();
                    Log.Branch = dtcon5.Rows[0]["Branch"].ToString();
                    Log.City = dtcon5.Rows[0]["City"].ToString();
                    Log.RTGS_Code = dtcon5.Rows[0]["RTGS_Code"].ToString();
                    Log.Account_Type = dtcon5.Rows[0]["Account_Type"].ToString();
                    Log.Flag = Convert.ToBoolean(dtcon5.Rows[0]["Flag"]);
                    Log.Address = dtcon5.Rows[0]["Address"].ToString();
                }

                string Querystring6 = $"select top 1 [M_Consumerid],[InputPanName] from tblKycPanDataDetails where [M_Consumerid] ='{Req.M_Consumerid}' order by  [M_Consumerid] Desc";
                var dtcon6 = await _databaseManager.ExecuteDataTableAsync(Querystring6);
                if (dtcon6.Rows.Count > 0)
                {
                    Req.UserNameForValidatePan = dtcon6.Rows[0]["InputPanName"].ToString();
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Please enter correct pan name in pan kyc api!", null));
                }
                // string Result = @"{""request_id"":""22501334-bbaa-46e7-9221-4671ec60aacd"",""task_id"":""90ce1a30-b756-4fa6-a7c0-a3aa171cfb42"",""group_id"":""1ad2c004-d7f1-4a5a-ab4b-ca546b306e57"",""success"":true,""response_code"":""100"",""response_message"":""Valid Authentication"",""metadata"":{""billable"":""Y"",""reason_code"":""TXN"",""reason_message"":""Transaction Successful""},""result"":{""bank_ref_no"":""434719933335"",""beneficiary_name"":""MR BIPIN MAURYA"",""transaction_remark"":""Transaction Successful"",""verification_status"":""VERIFIED""},""request_timestamp"":""2024-12-12T13:39:07.867Z"",""response_timestamp"":""2024-12-12T13:39:09.405Z""}";
                string Result = _databaseManager.AccountNumber(Req.AccountNo, Req.IFSCCode, "api/v1/in/financial/bav/lite", "https://live.zoop.one/", "648d7d9a22658f001d0193ac", "W5Q2V99-JFC4D4D-QS0PG29-C6DNJYR");
                if (string.IsNullOrEmpty(Result))
                {
                    return BadRequest(new ApiResponse<object>(false, "Api response issue, please wait and try some time later!", null));
                }
                JObject jOBJ = JObject.Parse(Result);
                string statusCode = jOBJ["response_code"].ToString();
                if (statusCode == "101")
                {
                    return BadRequest(new ApiResponse<object>(false, "No Record Found!", null));

                }
                if (statusCode == "102")
                {
                    return BadRequest(new ApiResponse<object>(false, "Multiple Records Found", null));

                }
                if (statusCode == "103")
                {
                    return BadRequest(new ApiResponse<object>(false, "Partial Record Found!", null));

                }
                if (statusCode == "106")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid IFSC Code!", null));

                }



                if (statusCode == "105" || statusCode == "104")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid account number!", null));

                }
                if (statusCode == "107")
                {
                    return BadRequest(new ApiResponse<object>(false, "Duplicate Transaction!", null));

                }
                if (statusCode == "108" || statusCode == "109" || statusCode == "110" || statusCode == "111")
                {
                    return BadRequest(new ApiResponse<object>(false, "Server Down!", null));

                }

                if (statusCode == "99")
                {
                    return BadRequest(new ApiResponse<object>(false, "Unknown Error!", null));

                }
                if (statusCode == "113")
                {
                    return BadRequest(new ApiResponse<object>(false, "Something went wrong, Please contact to service provider!", null));

                }
                if (statusCode == "106")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid account number!", null));

                }
                if (statusCode == "112")
                {
                    return BadRequest(new ApiResponse<object>(false, "Api Amount Issue", null));
                }
                verifyKycDataDetail.BankRefrenceId = jOBJ["request_id"].ToString();
                verifyKycDataDetail.BankRemarks = jOBJ["response_message"].ToString();
                verifyKycDataDetail.ResponseCode = jOBJ["response_code"].ToString();
                verifyKycDataDetail.IsBankAccountVerify = false;
                verifyKycDataDetail.Status = false;


                if (jOBJ["success"].ToString() == "True")
                {
                    verifyKycDataDetail.AccountHolderName = jOBJ["result"]["beneficiary_name"].ToString();
                    verifyKycDataDetail.BankRefrenceId = jOBJ["result"]["bank_ref_no"].ToString();
                    Log.Account_HolderNm = jOBJ["result"]["beneficiary_name"].ToString();
                    Log.Flag = true;
                    kycMode = "Online";


                    string querydata = $"select top 1 [M_Consumerid],[InputPanName] from tblKycPanDataDetails where [M_Consumerid]='{Req.M_Consumerid}' order by  [M_Consumerid] Desc";
                    var dt4 = await _databaseManager.ExecuteDataTableAsync(querydata);


                    if (dt4.Rows.Count > 0)
                    {
                        Req.UserNameForValidatePan = dt4.Rows[0]["InputPanName"].ToString();

                        string ArrStr1 = verifyKycDataDetail.AccountHolderName.ToUpper();

                        string[] returnedArray1 = ArrStr1.Split(' ');
                        string ArrStr2 = Req.UserNameForValidatePan.ToUpper();
                        string[] returnedArray2 = ArrStr2.Split(' ');

                        int MatchCount = 0;
                        foreach (string author in returnedArray1)
                        {
                            // Clean up the author string: remove prefixes and normalize case/whitespace
                            string cleanedAuthor = CleanAuthorName(author);

                            // Clean up the AccountHolderName: normalize case/whitespace
                            string cleanedAccountHolderName = CleanAuthorName(dtcon.Rows[0]["ConsumerName"].ToString());

                            // Check if the cleaned author matches the cleaned AccountHolderName
                            if (cleanedAccountHolderName.Contains(cleanedAuthor))
                            {
                                MatchCount = MatchCount + 1;
                            }
                        }
                        //foreach (string author in returnedArray1)
                        //{
                        //    string BankNAME = author.Trim();
                        //    if ( Req.AccountHolderName.Contains(BankNAME))
                        //        MatchCount = MatchCount + 1;

                        //}
                        if (MatchCount == 0)
                        {
                            return BadRequest(new ApiResponse<object>(false, "Account Holder Name : " + ArrStr1 + " and Input name : " + Req.UserNameForValidatePan + " should be same", null));

                        }
                        else
                        {
                            string Updatequery = $"Update M_consumer set bankekycStatus='Online' where M_Consumerid='{Req.M_Consumerid}'";
                            await _databaseManager.ExecuteNonQueryAsync(Updatequery);
                            Dictionary<string, object> inputParameters = new Dictionary<string, object>
                        {
                        { "@Bank_ID", Log.Bank_ID },
                        { "@Bank_Name", Log.Bank_Name },
                        { "@Account_HolderNm", Req.AccountHolderName },
                        { "@Account_No", Req.AccountNo },
                        { "@Branch", Log.Branch },
                        { "@IFSC_Code", Log.IFSC_Code },
                        { "@City", Log.City },
                        { "@RTGS_Code", Log.RTGS_Code },
                        { "@Account_Type", Log.Account_Type },
                        { "@Address", Log.Address },
                        { "@Entry_Date", Log.Entry_Date },
                        { "@Flag", Log.Flag },
                        { "@DML", Log.DML },
                        { "@M_Consumerid", Req.M_Consumerid },
                        { "@Comp_id", Log.Comp_id }
                    };
                            _databaseManager.ExecuteStoredProcedureNONQUERY("PROC_InsertUpdateBankAccount", inputParameters);
                            Dictionary<string, object> jdata = new Dictionary<string, object>
                        {
                        { "M_Consumerid", Req.M_Consumerid },
                        { "Account_HolderNm", Req.AccountHolderName },
                        { "Account_No", Req.AccountNo },
                        { "IFSC_Code", Log.IFSC_Code },
                        { "bank_ref_no", jOBJ["result"]["bank_ref_no"].ToString() },
                        { "Flag", Log.Flag }
                        };
                            string Insertqrry1 = $"Insert into tblKycBankDataDetailsHistry(M_Consumerid,AccountHolderName,BankRefrenceId,BankReqdate,IsBankAccountVerify,BankRemarks,ResponseCode,Status,IFSC_Code,AccountNo) values('{Req.M_Consumerid}','{Req.AccountHolderName}','{verifyKycDataDetail.BankRefrenceId}','{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{true}','{verifyKycDataDetail.BankRemarks}','{verifyKycDataDetail.ResponseCode}','{true}','{Req.IFSCCode}','{Req.AccountNo}')";
                            var aa = await _databaseManager.ExecuteNonQueryAsync(Insertqrry1);

                            if (aa == 1)
                            {
                                string querydata1 = $"select top 1 [M_Consumerid] from tblKycBankDataDetails where [M_Consumerid]='{Req.M_Consumerid}' order by  [M_Consumerid] Desc";
                                var dt3 = await _databaseManager.ExecuteDataTableAsync(querydata1);


                                if (dt3.Rows.Count > 0)
                                {
                                    string queryupdate2 = $"update tblKycBankDataDetails set AccountHolderName='{verifyKycDataDetail.AccountHolderName}',BankRefrenceId='{verifyKycDataDetail.BankRefrenceId}',BankReqdate='{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',IsBankAccountVerify='{true}',BankRemarks='{verifyKycDataDetail.BankRemarks}',ResponseCode='{verifyKycDataDetail.ResponseCode}',Status='{true}',IFSC_Code='{Req.IFSCCode}',AccountNo='{Req.AccountNo}' where M_Consumerid='{Req.M_Consumerid}'";
                                    var dtn = await _databaseManager.ExecuteNonQueryAsync(queryupdate2);
                                }
                                else
                                {
                                    string insertqueyy = $"Insert into tblKycBankDataDetails(M_Consumerid,AccountHolderName,BankRefrenceId,BankReqdate,IsBankAccountVerify,BankRemarks,ResponseCode,Status,IFSC_Code,AccountNo,KycMode) values('{Req.M_Consumerid}','{verifyKycDataDetail.AccountHolderName}','{verifyKycDataDetail.BankRefrenceId}','{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{true}','{verifyKycDataDetail.BankRemarks}','{verifyKycDataDetail.ResponseCode}','{true}','{Req.IFSCCode}','{Req.AccountNo}','{kycMode}')";
                                    var dtn = await _databaseManager.ExecuteNonQueryAsync(insertqueyy);
                                }
                            }

                            return Ok(new ApiResponse<object>(true, "Verified successfully", jdata));
                        }

                    }

                    string Insertqrry = $"Insert into tblKycBankDataDetailsHistry(M_Consumerid,AccountHolderName,BankRefrenceId,BankReqdate,IsBankAccountVerify,BankRemarks,ResponseCode,Status,IFSC_Code,AccountNo) values('{Req.M_Consumerid}','{Req.AccountHolderName}','{verifyKycDataDetail.BankRefrenceId}','{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{verifyKycDataDetail.IsBankAccountVerify}','{verifyKycDataDetail.BankRemarks}','{verifyKycDataDetail.ResponseCode}','{verifyKycDataDetail.Status}','{Req.IFSCCode}','{Req.AccountNo}')";
                    var a = await _databaseManager.ExecuteNonQueryAsync(Insertqrry);

                    if (a == 1)
                    {
                        string querydata1 = $"select top 1 [M_Consumerid] from tblKycBankDataDetails where [M_Consumerid]='{Req.M_Consumerid}' order by  [M_Consumerid] Desc";
                        var dt3 = await _databaseManager.ExecuteDataTableAsync(querydata1);

                        string Updatequery = $"Update M_consumer set bankekycStatus='Online' where M_Consumerid='{Req.M_Consumerid}'";
                        await _databaseManager.ExecuteNonQueryAsync(Updatequery);

                        if (dt3.Rows.Count > 0)
                        {
                            string queryupdate2 = $"update tblKycBankDataDetails set AccountHolderName='{verifyKycDataDetail.AccountHolderName}',BankRefrenceId='{verifyKycDataDetail.BankRefrenceId}',BankReqdate='{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',IsBankAccountVerify='{verifyKycDataDetail.IsBankAccountVerify}',BankRemarks='{verifyKycDataDetail.BankRemarks}',ResponseCode='{verifyKycDataDetail.ResponseCode}',Status='{verifyKycDataDetail.Status}',IFSC_Code='{Req.IFSCCode}',AccountNo='{Req.AccountNo}' where M_Consumerid='{Req.M_Consumerid}'";
                            var dtn = await _databaseManager.ExecuteNonQueryAsync(queryupdate2);

                        }
                        else
                        {
                            string insertqueyy = $"Insert into tblKycBankDataDetails(M_Consumerid,AccountHolderName,BankRefrenceId,BankReqdate,IsBankAccountVerify,BankRemarks,ResponseCode,Status,IFSC_Code,AccountNo,KycMode) values('{Req.M_Consumerid}','{verifyKycDataDetail.AccountHolderName}','{verifyKycDataDetail.BankRefrenceId}','{System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}','{verifyKycDataDetail.IsBankAccountVerify}','{verifyKycDataDetail.BankRemarks}','{verifyKycDataDetail.ResponseCode}','{verifyKycDataDetail.Status}','{Req.IFSCCode}','{Req.AccountNo}','{kycMode}')";
                            var dtn = await _databaseManager.ExecuteNonQueryAsync(insertqueyy);

                        }

                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Record not inserted!", null));


                    }

                }
                else if (jOBJ["success"].ToString() == "False")
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid account  number!", null));

                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid account number!", null));

                }


                _databaseManager.ExceptionLogs("Bank Account Verification For KYC | Account No |" + Req.AccountNo + " | Response | " + Result);

                return Ok(new ApiResponse<object>(true, "Successfully verified PAN card.", null));
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Bank Account API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
            }
           
        }

        public string CleanAuthorName(string name)
        {
            // Remove common prefixes like "MR", "MS", "MRS" (if needed)
            string[] prefixes = { "MR", "MS", "MRS" };
            foreach (var prefix in prefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    name = name.Substring(prefix.Length).Trim(); // Remove the prefix
                    break;
                }
            }

            // Normalize case and remove extra spaces
            name = name.Trim().ToLower();  // Converts to lowercase and trims leading/trailing spaces
            name = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", " "); // Replace multiple spaces with a single space

            return name;
        }
    }

    public class bankaccountrequest
    {
        public string AccountNo { get; set; }
        public string AccountHolderName { get; set; }
        public string IFSCCode { get; set; }
        public string M_Consumerid { get; set; }
        public string UserNameForValidatePan { get; set; }
        public string BankName { get; set; }
    }

    public class verifyKycDataDetail
    {
        public string M_Consumerid { get; set; }
        public string AadharName { get; set; }
        public string AadharRefrenceId { get; set; }
        public bool IsaadharVerify { get; set; }
        public string PanName { get; set; }
        public string PanRefrenceId { get; set; }
        public bool IspanVerify { get; set; }
        public string AccountHolderName { get; set; }
        public string BankRefrenceId { get; set; }
        public bool IsBankAccountVerify { get; set; }
        public bool Status { get; set; }
        public string PanRemarks { get; set; }
        public string AadharRemarks { get; set; }
        public string BankRemarks { get; set; }

        public string ResponseCode { get; set; }

        public string Msg { get; set; }
        public bool UploadDocReq { get; set; }
    }
}

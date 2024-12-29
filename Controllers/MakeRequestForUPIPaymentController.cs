using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Data;
using System.Reflection;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MakeRequestForUPIPaymentController : ControllerBase
    {

        private readonly DatabaseManager _databaseManager;

        public MakeRequestForUPIPaymentController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> MakeRequestForUPIPayment([FromBody] MakeRequestForUPIPaymentClass req)
        {
            //M_Consumerid,Comp_ID
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                if (req.Comp_ID != "")
                {
                    string MobileNo = string.Empty;
                     string ConsumerName = string.Empty;
                string ConsumerEmail = string.Empty;

                    decimal Amt = Convert.ToDecimal(req.Amount);
                    decimal WalletBal = 0m;
                    decimal ChargeAmt = 0m;

                    decimal charge = 0m;
                    bool ChargeType = true;

                    DataTable Consdt = await _databaseManager.SelectTableDataAsync("M_Consumer", "MobileNo,ConsumerName,Email", "m_consumer='" + req.M_Consumerid + "'");

                    if (Consdt.Rows.Count == 0)
                    {
                        return BadRequest(new ApiResponse<object>(false, "Invalid user details !"));
                    }
                    else
                    {
                        MobileNo = Consdt.Rows[0]["MobileNo"].ToString();
                        ConsumerName = Consdt.Rows[0]["ConsumerName"].ToString();
                        ConsumerEmail = Consdt.Rows[0]["Email"].ToString();
                    }

                    DataTable Comdt = await _databaseManager.SelectTableDataAsync("Comp_Reg", "Comp_ID", "Comp_ID='" + req.Comp_ID + "'");

                    if (Comdt.Rows.Count == 0)
                    {
                        return BadRequest(new ApiResponse<object>(false, "Invalid Vendor details !"));
                    }
                    else
                    {
                        Dictionary<string, object> inputParametersWalletBalance = new Dictionary<string, object>
                                {
                                    {"@Comp_Id", req.Comp_ID }
                                };
                        DataTable Baldt = await _databaseManager.ExecuteStoredProcedureDataTableAsync("USP_GetVendorCashWalletBal", inputParametersWalletBalance);
                        if (Baldt.Rows.Count > 0)
                            WalletBal = Convert.ToDecimal(Baldt.Rows[0][0].ToString());
                    }
                    #region  Dynamic slab charges
                    string SlabId = "3";
                    if (Amt > 0 && Amt <= 1000)
                        SlabId = "1";
                    else if (Amt > 1000 && Amt <= 25000)
                        SlabId = "2";
                    else if (Amt > 25000)
                        SlabId = "3";

                    DataTable Chargedt = new DataTable();
                    Dictionary<string, object> inputParametersUPICharge = new Dictionary<string, object>
                                {
                                    {"@SlabId", SlabId },
                                    {"@Comp_Id", req.Comp_ID },

                                };
                    Chargedt = await _databaseManager.ExecuteStoredProcedureDataTableAsync("inputParametersUPICharge", inputParametersUPICharge);
                    if (Chargedt.Rows.Count > 0)
                    {
                        charge = Convert.ToDecimal(Chargedt.Rows[0]["Charge_Amount"].ToString());
                        ChargeType = Convert.ToBoolean(Chargedt.Rows[0]["Charge_Type"].ToString());
                        if (ChargeType)
                        {
                            ChargeAmt = charge;
                        }
                        else
                        {
                            ChargeAmt = Amt * charge / 100;
                        }
                    }
                    #endregion


                    decimal GstAmount = ChargeAmt * 18 / 100;
                    decimal Debited = Amt + ChargeAmt + GstAmount;
                    decimal RemainingAmt = WalletBal - Debited;
                    if (WalletBal < Debited)
                    {
                        return BadRequest(new ApiResponse<object>(false, "Insufficient wallet balance !"));
                    }


                    string UniqueNumber = req.Comp_ID.Replace("-", "") + DateTime.Now.ToString("yMMddHHmmssff");
                    // string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmssff");

                    string queryUpiClaimReq = $"INSERT INTO tblUPITransactionDetails (Comp_Id,M_Consumerid,MobileNo,ConsumerName,ConsumerEmailId,Code1,Code2,Amount,Charge_Amount,Charge_Type,TCharge_Amount,GstAmount,RefenceId,Status,UPI_Id) VALUES ('" + req.Comp_ID + "','" + req.M_Consumerid + "','" + MobileNo + "','" + ConsumerName + "','" + ConsumerEmail + "','" + req.Code1 + "','" + req.Code2 + "','" + Amt + "','" + charge + "','" + ChargeType + "','" + ChargeAmt + "','" + GstAmount + "','" + UniqueNumber + "','Pending','" + req.UPIId + "' )";
                    int RequestId = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimReq);
                    if (RequestId > 0)
                    {

                        

                        //var client = new RestClient("https://api.instantpay.in/payments/payout");
                        //client.Timeout = -1;
                        //var request = new RestRequest(Method.POST);
                        //// ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        //request.AddHeader("X-Ipay-Auth-Code", "1");
                        ////  request.AddHeader("X-Ipay-Client-Id", "YWY3OTAzYzNlM2ExZTJlOZh0WjOBBKD33wYkrJweAJ4=");
                        //request.AddHeader("X-Ipay-Client-Id", "YWY3OTAzYzNlM2ExZTJlOZh0WjOBBKD33wYkrJweAJ4=");
                        //request.AddHeader("X-Ipay-Client-Secret", "6ab3362d2e36fb58cc2bedfc172ac470edd25505c5a962c82d894d929b6a8242");//Prod
                        //                                                                                                              //request.AddHeader("X-Ipay-Client-Secret", "4f5f586231c03c33848488269a659f9e733f2add24f8fe1481768430660e1bd8");//Prod
                        //                                                                                                              // request.AddHeader("X-Ipay-Client-Secret", "3f2582df2f885f181737dd2a80389fbc764987b6f977913921e2cd0db58c9ced"); // For UAT mode

                        //request.AddHeader("X-Ipay-Endpoint-Ip", "103.100.218.202");
                        //request.AddHeader("Content-Type", "application/json");
                        //var body = "{\"payer\":{\"bankProfileId\": \"0\",\"accountNumber\":\"" + Mobileno.Remove(0, 2) + "\"},\"payee\":{\"name\":\"" + ConsumerName + "\",\"accountNumber\":\"" + UPIId + "\",\"bankIfsc\":\"\"},\"transferMode\":\"UPI\",\"transferAmount\":\"" + Amount + "\",\"externalRef\":\"" + UniqueNumber + "\",\"latitude\":\"20.2326\",\"longitude\":\"78.1228\",\"remarks\":\"UPI\",\"alertEmail\":\"" + ConsumerEmail + "\",\"purpose\":\"REIMBURSEMENT\"}";
                        //request.AddParameter("application/json", body, ParameterType.RequestBody);
                        //IRestResponse response = client.Execute(request);
                        //Dbstring.ExceptionLogs("Bank MakeRequestForUPIPayment For KYC | Body |" + body + " | Response | " + response.Content + " | StatusDescription " + response.ErrorMessage);


                        //if (response.Content == "")
                        //{
                        //    Res.Status = false;
                        //    Res.Message = "Api response issue, please wait and try some time later!";
                        //    Res.Data = response.ErrorMessage;
                        //    return Res;
                        //}

                       // JObject jOBJ = JObject.Parse(response.Content);
                       // string StatusCode = jOBJ["statuscode"].ToString();

                        string Result = _databaseManager.instantpayout(MobileNo, ConsumerName, req.UPIId, req.Amount, UniqueNumber, ConsumerEmail, "https://api.instantpay.in/", "payments/payout");
                        var jOBJ = JObject.Parse(Result);

                        string StatusCode = jOBJ["statuscode"].ToString();
                        string orderid = "NA";
                        string txnReferenceId = "NA";

                        if (jOBJ["statuscode"].ToString() == "TXN")
                        {
                            orderid = jOBJ["orderid"].ToString();
                            txnReferenceId = jOBJ["data"]["txnReferenceId"].ToString();
                           // Res.Status = true;
                           // Res.Message = jOBJ["status"].ToString();

                            //db.Insertion("Comp_Id,Service_ID,M_Consumerid,OldBal,NewBal,Amount,Cr_Dr_Type,PayrefId", "'" + req.Comp_ID + "','SRV1029','" + req.M_Consumerid + "','" + WalletBal + "','" + RemainingAmt + "','" + Debited + "','Debit','" + RequestId + "'", "tblCashWalletBalance");

                            string queryUpiUpi = $"INSERT INTO tblCashWalletBalance (Comp_Id,Service_ID,M_Consumerid,OldBal,NewBal,Amount,Cr_Dr_Type,PayrefId) VALUES ('" + req.Comp_ID + "','SRV1029','" + req.M_Consumerid + "','" + WalletBal + "','" + RemainingAmt + "','" + Debited + "','Debit','" + RequestId + "' )";
                            int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiUpi);
                            await _databaseManager.UpdateAsync("OrderId='" + orderid + "',Status='Success',Remarks='" + jOBJ["status"].ToString() + "'", "Id='" + RequestId + "'", "tblUPITransactionDetails");
                            await _databaseManager.UpdateAsync("Amount='" + RemainingAmt + "',Updated_date='" + System.DateTime.Now + "'", "Comp_ID='" + req.Comp_ID + "'", "Paytm_balance");
                            string Final = "{\"StatusCode\":\"" + StatusCode + "\",\"orderid\":\"" + orderid + "\",\"txnReferenceId\":\"" + txnReferenceId + "\"}";
                            return Ok(new ApiResponse<object>(true, jOBJ["status"].ToString(), Final));
                        }
                        else if (jOBJ["statuscode"].ToString() == "TUP")
                        {
                            //orderid = jOBJ["orderid"].ToString();
                            //txnReferenceId = jOBJ["data"]["txnReferenceId"].ToString();


                            await _databaseManager.UpdateAsync("Comp_Id,Service_ID,M_Consumerid,OldBal,NewBal,Amount,Cr_Dr_Type,PayrefId", "'" + req.Comp_ID + "','SRV1029','" + req.M_Consumerid + "','" + WalletBal + "','" + RemainingAmt + "','" + Debited + "','Debit','" + RequestId + "'", "tblCashWalletBalance");
                            await _databaseManager.UpdateAsync("OrderId='" + orderid + "',Remarks='" + jOBJ["status"].ToString() + "'", "Id='" + RequestId + "'", "tblUPITransactionDetails");
                            await _databaseManager.UpdateAsync("Amount='" + RemainingAmt + "',Updated_date='" + System.DateTime.Now + "'", "Comp_ID='" + req.M_Consumerid + "'", "Paytm_balance");
                            await _databaseManager.UpdateAsync("OrderId='" + orderid + "',Status='Failed',Remarks='" + jOBJ["status"].ToString() + "'", "Id='" + RequestId + "'", "tblUPITransactionDetails");
                            string Final = "{\"StatusCode\":\"" + StatusCode + "\",\"orderid\":\"" + orderid + "\",\"txnReferenceId\":\"" + txnReferenceId + "\"}";
                            return Ok(new ApiResponse<object>(true, jOBJ["status"].ToString(), Final));
                        }
                        else
                        {

                            orderid = jOBJ["orderid"].ToString();
                           // Res.Status = false;
                            //Res.Message = jOBJ["status"].ToString();
                           await _databaseManager.UpdateAsync("OrderId='" + orderid + "',Status='Failed',Remarks='" + jOBJ["status"].ToString() + "'", "Id='" + RequestId + "'", "tblUPITransactionDetails");
                            string Final = "{\"StatusCode\":\"" + StatusCode + "\",\"orderid\":\"" + orderid + "\",\"txnReferenceId\":\"" + txnReferenceId + "\"}";
                            
                            return BadRequest(new ApiResponse<object>(false, jOBJ["status"].ToString(), Final));
                        }

                        //string Final = "{\"StatusCode\":\"" + StatusCode + "\",\"orderid\":\"" + orderid + "\",\"txnReferenceId\":\"" + txnReferenceId + "\"}";

                        //JObject FinalObj = JObject.Parse(Final);
                        //Res.Data = FinalObj;
                        //return Res;
                    }
                    else
                    {

                        return BadRequest(new ApiResponse<object>(false, "Something went wrong !"));
                    }

                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid login detail!"));
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while MakeRequestForUPIPayment: {ex.Message}"));
            }
        }
    }

    public class MakeRequestForUPIPaymentClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }
        public string UPIId { get; set; }
        public string Amount { get; set; }
        public string Mobileno { get; set; }
        public string ConsumerName { get; set; }
        public string ConsumerEmail { get; set; }
        public string Code1 { get; set; }
        public string Code2 { get; set; }
    }


}
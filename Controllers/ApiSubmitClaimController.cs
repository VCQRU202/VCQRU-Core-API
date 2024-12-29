using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiSubmitClaimController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public ApiSubmitClaimController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> ApiSubmitClaim([FromBody] ApiSubmitClaimClass req)
        {
            //M_Consumerid,Comp_ID
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                string Comp_ID = "NA";
                string m_consumerid = "";
                string UPIId = string.Empty;
                string ServiceId = "NA";
                int Amount = 0;
                string ProductId = req.ProductId;
                string Productvalue = req.Productvalue;
               // string claimdetails = "{\"ProductId\":" + ProductId + ",\"Productvalue\":" + Productvalue + ",\"Userid\":\"" + MobileNo + "\",\"claimdate\":\"" + System.DateTime.Now + "\"}";
                string claimdate = System.DateTime.Now.ToString();
                string pancard_number = string.Empty; // oci
                string KYC_status = string.Empty;
                string aadharCardKycStatus = string.Empty; // Default to "No" if not exists
                string panCardKycStatus = string.Empty;
                string accountDetailsKycStatus = string.Empty;
                string upiKycStatus = string.Empty;
                string MobileNo = string.Empty;
                string Claimpointuser = "0";
                string ConsumerName = string.Empty;
                string ConsumerEmail = string.Empty;
                if (req.Comp_ID != "")
                {
                    Comp_ID = req.Comp_ID;
                    if (req.ServiceId != "") { ServiceId = req.ServiceId; }
                    //if (req.Amount > 0) { Amount = req.Amount; }
                    if (req.UPIId != "") { UPIId = req.UPIId; }
                    if (req.UPIId != "") { UPIId = req.UPIId; }
                    if (req.M_Consumerid != "") { m_consumerid = req.M_Consumerid; }

                    DataTable dt = await _databaseManager.SelectTableDataAsync("m_consumer", "top 1 ConsumerName,Email,UPIId,panekycStatus,aadharkycStatus,bankekycStatus,VRKbl_KYC_status,UPIKYCSTATUS,MobileNo", "M_Consumerid= '" + req.M_Consumerid + "' and IsDelete='0' order by  [M_Consumerid] Desc");
                    if (dt.Rows.Count > 0)
                    {
                        if ((req.UPIId.Length < 5) && (dt.Rows[0]["UPIId"].ToString().Length > 3)) // tej
                        {
                            req.UPIId = dt.Rows[0]["UPIId"].ToString();
                        }
                        MobileNo = dt.Rows[0]["MobileNo"].ToString();
                         ConsumerName = dt.Rows[0]["ConsumerName"].ToString();
                         ConsumerEmail = dt.Rows[0]["Email"].ToString();
                        KYC_status = "0";

                        DataTable dt1 = await _databaseManager.SelectTableDataAsync("BrandSettings", "top 1 kyc_Details", "Comp_ID= '" + req.Comp_ID + "' order by  row_ID Desc");
                        if (dt1.Rows.Count > 0)
                        {
                            string kycDetailsString = dt1.Rows[0]["kyc_Details"].ToString();
                            if (!string.IsNullOrEmpty(kycDetailsString))
                            {
                                var kycDetails = JObject.Parse(kycDetailsString);
                                aadharCardKycStatus = kycDetails["AadharCard"]?.ToString() ?? "";
                                panCardKycStatus = kycDetails["PANCard"]?.ToString() ?? "";
                                accountDetailsKycStatus = kycDetails["AccountDetails"]?.ToString() ?? "";
                                upiKycStatus = kycDetails["UPI"]?.ToString() ?? "";
                                if (dt.Rows[0]["VRKbl_KYC_status"].ToString() == "2")
                                {
                                    return BadRequest(new ApiResponse<object>(false, "Your KYC was rejected. Please check the details entered and update your profile with correct information. Once completed contact to support on 08047278314."));
                                }
                                if (dt.Rows[0]["VRKbl_KYC_status"].ToString() == "1")
                                {
                                    KYC_status = "1";
                                }
                                if (KYC_status != "1") {
                                    if (aadharCardKycStatus == "Yes")
                                    {
                                        if (dt.Rows[0]["aadharkycStatus"].ToString() != "Online")
                                        {
                                            KYC_status = "0";
                                            return BadRequest(new ApiResponse<object>(false, "Aadhar KYC is pending!."));
                                        }
                                    }
                                    if (panCardKycStatus == "Yes")
                                    {
                                        if (dt.Rows[0]["panekycStatus"].ToString() != "Online")
                                        {
                                            KYC_status = "0";
                                            return BadRequest(new ApiResponse<object>(false, "PAN KYC is pending!."));
                                        }
                                    }
                                    if (accountDetailsKycStatus == "Yes")
                                    {
                                        if (dt.Rows[0]["bankekycStatus"].ToString() != "Online")
                                        {
                                            KYC_status = "0";
                                            return BadRequest(new ApiResponse<object>(false, "Bank Account KYC is pending!."));
                                        }
                                    }
                                    if (aadharCardKycStatus != "No" && panCardKycStatus != "No" && accountDetailsKycStatus != "No")
                                    { // offline kyc

                                        if (dt.Rows[0]["VRKbl_KYC_status"].ToString() == "0" || dt.Rows[0]["VRKbl_KYC_status"].ToString() == "" || dt.Rows[0]["VRKbl_KYC_status"].ToString() == null)
                                        {
                                            return BadRequest(new ApiResponse<object>(false, "Your KYC is pending approval. Please wait for confirmation."));
                                        }
                                        if (dt.Rows[0]["VRKbl_KYC_status"].ToString() == "" || dt.Rows[0]["VRKbl_KYC_status"].ToString() == null)
                                        {
                                            return BadRequest(new ApiResponse<object>(false, "Please complete your KYC first before making any claim."));
                                        }
                                    }

                                }
                                

                            }
                            else
                            {
                                return BadRequest(new ApiResponse<object>(false, "Kyc setting is pending by administrator!."));
                            }
                        }
                        else
                        {
                            return BadRequest(new ApiResponse<object>(false, "Kyc setting is pending by administrator!."));
                        }

                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Invalid user, please login again!."));
                    }

                    DataTable dtcp = null;
                    DataTable dtcondition = new DataTable();
                    int tp = 0;
                    int tprr = 0;        //oci
                    if (dtcp != null)
                    {
                        dtcp.Rows.Clear();
                    }
                    DataTable lrdt = await _databaseManager.SelectTableDataAsync("scrap_entry se inner join Pro_Enq pe on pe.Received_Code1=se.code1 and pe.Received_Code2=se.code2 inner join m_consumer ms on ms.MobileNo=pe.MobileNo", "isnull(sum(loyalty),0) as loyalty", "pe.is_success=1 and  ms.M_Consumerid=" + req.M_Consumerid + "");
                    string loyalty_return = lrdt.Rows[0]["loyalty"].ToString();
                    if (loyalty_return == "")
                        loyalty_return = "0";
                    dtcp = await _databaseManager.SelectTableDataAsync("dbo.[BLoyaltyPointsEarned] bl left join[dbo].[M_ServiceSubscriptionTrans] ms on bl.sst_id = ms.sst_id left join[dbo].M_ServiceSubscription mss on mss.Subscribe_Id = ms.Subscribe_Id left join dbo.claimredeem cl on cl.compid=mss.Comp_ID", "mss.Comp_ID,isnull(sum(bl.points), 0) point,isnull(cl.p_cash,0) p_cash", "bl.M_consumerid = '" + req.M_Consumerid + "' and Comp_ID='" + req.Comp_ID + "' group by mss.Comp_ID,cl.p_cash");
                    if (dtcp.Rows.Count > 0)
                    {
                        //Tej mahavir paint,lubigen,oci
                        if (req.UPIId.Length < 5 && (req.ServiceId == "SRV1029"))
                        {
                            return BadRequest(new ApiResponse<object>(false, "UPIID is not valid."));
                        }

                        int loyaltybonus = 0;
                            DataTable bldt = await _databaseManager.SelectTableDataAsync("buildloyalty_offers", "isnull(sum(points),0) as points", "m_consumerid=" + req.M_Consumerid + "");
                            loyaltybonus = Convert.ToInt32(bldt.Rows[0]["points"].ToString());
                            int totalpoint = (Convert.ToInt32(dtcp.Rows[0]["point"].ToString()) - Convert.ToInt32(loyalty_return)) + loyaltybonus;

                            DataTable Rdmdt = await _databaseManager.SelectTableDataAsync("[BPointsTransaction] bl inner join [M_consumer] mc on mc.[M_Consumerid]=bl.[RedeemBy]", "case when Sum(bl.[RedeemPoints]) is null then 0 else cast(Sum(isnull(bl.[RedeemPoints],0))as int) end RedeemPoints", "mc.[m_consumerid]='" + req.M_Consumerid + "' and (bl.bpstatus is null or bl.bpstatus <>'FAILURE')");
                            int redeempoint = Convert.ToInt32(Rdmdt.Rows[0]["RedeemPoints"].ToString());
                            float totalpointrt = Convert.ToInt32(dtcp.Rows[0]["p_cash"].ToString());
                            DataTable dtclaimpoint = await _databaseManager.SelectTableDataAsync("ClaimDetails", "isnull(Sum( Amount),0)", "right(Mobileno,10)='" + MobileNo.Substring(MobileNo.Length - 10, 10) + "' and Comp_id='" + req.Comp_ID + "' and Isapproved=1 ");
                            if (dtclaimpoint.Rows.Count > 0)
                            {
                                Claimpointuser = dtclaimpoint.Rows[0][0].ToString();
                            }

                            #region UPI Claim
                            int UPIclaimapply = 0;
                            if (ServiceId == "SRV1029")
                            {
                                DataTable UPIdt = await _databaseManager.SelectTableDataAsync("[tblupitransactiondetails] cl inner join [M_consumer] mc on mc.MobileNo=cl.Mobileno", "case when Sum(cl.Amount) is null then 0 else Sum(isnull(cl.Amount,0)) end", "mc.[M_ConsumeriD]='" + req.M_Consumerid + "' and (cl.status='Success') and cl.Code1 <> '' and cl.Code2 <>''");
                                if (UPIdt.Rows.Count > 0)
                                    UPIclaimapply = Convert.ToInt32(UPIdt.Rows[0][0].ToString());
                            }
                            Claimpointuser = (Convert.ToInt32(Claimpointuser) + UPIclaimapply).ToString();
                            #endregion                            
                            dtcondition = await _databaseManager.SelectTableDataAsync("point_redeem_condition", "top 1 codition_point,condition_match", "comp_id='" + dtcp.Rows[0]["Comp_ID"].ToString() + "' and isactive=1 and selection_id=case when (select count(*) from paytmtransaction where m_consumerid='" + dt.Rows[0][1].ToString() + "' and pstatus='SUCCESS' and comp_id='Comp-1283')>0 then 2 else 1 end ");

                            if (dtcondition.Rows.Count > 0) // Live                                                                                                                                                                                                                                                                                                                        
                            {
                                tp = totalpoint - (redeempoint + Convert.ToInt32(Claimpointuser));

                                if (tp < 0)
                                {
                                    return BadRequest(new ApiResponse<object>(false, "Insufficient points to claim. Earn more points to proceed."));
                                }
                                else if (tp < Convert.ToInt32(dtcondition.Rows[0]["codition_point"]))
                                {
                                    return BadRequest(new ApiResponse<object>(false, "Insufficient points to claim. Earn more points to proceed."));
                                }
                                else if (tp < Convert.ToInt32(req.Productvalue))
                                {
                                    return BadRequest(new ApiResponse<object>(false, "Insufficient points to claim. Earn more points to proceed."));
                                }
                            }

                            int ptforrd = Convert.ToInt32(req.Productvalue);
                            DataTable dtcondition1 = await _databaseManager.SelectTableDataAsync("[ClaimDetails] left join M_Consumer on right(M_Consumer.MobileNo, 10) = right([ClaimDetails].Mobileno, 10)", "COALESCE(SUM(ClaimDetails.Amount), 0) AS TotalAmount", "M_Consumer.M_Consumerid = '" + req.M_Consumerid + "' and[Isapproved] = 0");
                            int giftValue = Convert.ToInt32(req.Productvalue);
                            if (ptforrd >= Convert.ToInt32(dtcondition.Rows[0]["codition_point"].ToString()))
                            {

                                if (Convert.ToInt32(dtcondition1.Rows[0][0]) > 0)
                                {
                                    return BadRequest(new ApiResponse<object>(false, "Your previous claim with " + dtcondition1.Rows[0][0].ToString() + " points is awaiting approval. Please wait for confirmation."));
                                }
                                else {

                                if (ServiceId == "SRV1001")  //Live
                                {

                                    if (ptforrd >= Convert.ToInt32(dtcondition.Rows[0]["codition_point"].ToString()))
                                    {
                                        string query11 = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id]) VALUES (GETDATE(),'" + MobileNo + "'," + ptforrd.ToString() + ",1,null,0,'" + req.Comp_ID + "')";
                                        int count = await _databaseManager.ExecuteNonQueryAsync(query11);

                                        DataTable gifttable = await _databaseManager.SelectTableDataAsync("Claim_gift", "gift_id,Gift_name, Gift_value", "gift_id='" + req.ProductId + "' and status=1 order by Gift_value asc ");
                                        int Avlaible_Point = tp - (ptforrd);
                                        gifttable.Columns.Add("AvailablePoint");
                                        gifttable.Columns.Add("ClaimType");
                                        gifttable.Columns.Add("RedeemPoint");
                                        gifttable.Columns.Add("ClaimDate");
                                        for (int i = 0; i < gifttable.Rows.Count; i++)
                                        {
                                            gifttable.Rows[i]["Gift_name"].ToString();
                                            gifttable.Rows[i]["RedeemPoint"] = ptforrd.ToString();
                                            gifttable.Rows[i]["AvailablePoint"] = Avlaible_Point;
                                            gifttable.Rows[i]["ClaimType"] = "Gift Claim";
                                            gifttable.Rows[i]["ClaimDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                        }

                                        gifttable.DefaultView.Sort = "RedeemPoint ASC";
                                        gifttable = gifttable.DefaultView.ToTable();

                                        var data = gifttable.AsEnumerable().Select(row =>
                                        {
                                            return gifttable.Columns.Cast<DataColumn>().ToDictionary(
                                                column => column.ColumnName,
                                                column => row[column] // Extract the column value
                                            );
                                        }).FirstOrDefault(); // Select the first row or return null if empty

                                        //var rows = gifttable.AsEnumerable().Select(row =>
                                        //{
                                        //    return gifttable.Columns.Cast<DataColumn>().ToDictionary(
                                        //        column => column.ColumnName,
                                        //        column => row[column] // Return column value
                                        //    );
                                        //});
                                        return Ok(new ApiResponse<object>(true, "Your Request Registered with US for points " + ptforrd, data));
                                    }
                                    else
                                    {
                                        return BadRequest(new ApiResponse<object>(false, "Insufficient points to claim. Earn more points to proceed."));
                                    }
                                }
                                if (ServiceId == "SRV1029")
                                {
                                    #region Daily Limit Details
                                    Dictionary<string, object> inputParametersUPI = new Dictionary<string, object>
                                {
                                    {"@Comp_id", req.Comp_ID },
                                    {"@Service_id", req.ServiceId }
                                };
                            
                                    DataTable Clmdt = await _databaseManager.ExecuteStoredProcedureDataTableAsync("USP_GetUPILimitDetails", inputParametersUPI);
                                    if (Clmdt.Rows.Count > 0)
                                    {
                                        int DailyLimit = Convert.ToInt32(Clmdt.Rows[0]["Daily_Limit"].ToString());
                                        if (DailyLimit < Amount)
                                        {
                                            return BadRequest(new ApiResponse<object>(false, "Per day transaction limit exceeded!"));
                                        }
                                        string IsClaimrequired = Clmdt.Rows[0]["IsClaimReq"].ToString();
                                        string IsApprovalRequired = Clmdt.Rows[0]["IsApprovalReq"].ToString();
                                        if (IsClaimrequired == "1" && IsApprovalRequired == "1")
                                        {
                                            string queryUpiClaimReq = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id],[UPIID]) VALUES (GETDATE(),'" + MobileNo + "'," + Amount.ToString() + ",1,null,0,'" + req.Comp_ID + "','" + req.UPIId + "' )";
                                            int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimReq);
                                            return Ok(new ApiResponse<object>(true, "Your Request Registered with US for points " + Amount));
                                        }
                                        else if (IsClaimrequired == "0" && IsApprovalRequired == "0")
                                        {  // failed claim code check payment sri anannta/vsc
                                            bool ClmStatus = false;
                                            ClmStatus = await _databaseManager.MakeUPIpayment(req.Comp_ID, MobileNo, ConsumerName, ConsumerEmail, req.UPIId, Amount.ToString(), req.M_Consumerid);
                                            if (ClmStatus)
                                            {
                                                string queryUpiClaimApprove = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id],[UPIID]) VALUES (GETDATE(),'" + MobileNo + "'," + Amount.ToString() + ",1,null,1,'" + req.Comp_ID + "','" + req.UPIId + "' )";
                                                int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimApprove);
                                                return Ok(new ApiResponse<object>(true, "Your Request Registered with US for points " + Amount));
                                            }
                                            else
                                            {
                                                string queryUpiClaimReject = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id],[UPIID]) VALUES (GETDATE(),'" + MobileNo + "'," + Amount.ToString() + ",1,null,2,'" + req.Comp_ID + "','" + req.UPIId + "' )";
                                                int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimReject);

                                                return BadRequest(new ApiResponse<object>(false, "Something went wrong ,Please contact to help desk!"));
                                            }
                                        }
                                        else if ((IsClaimrequired == "1") && (IsApprovalRequired == "0"))
                                        {
                                            bool ClmStatus = false;                                            
                                             ClmStatus = await _databaseManager.MakeUPIpayment(req.Comp_ID, MobileNo, ConsumerName, ConsumerEmail, req.UPIId, Amount.ToString(), req.M_Consumerid);


                                            if (ClmStatus)
                                            {                                               
                                                string queryUpiClaimappr = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id],[UPIID]) VALUES (GETDATE(),'" + MobileNo + "'," + Amount.ToString() + ",1,null,1,'" + req.Comp_ID + "','" + req.UPIId + "' )";
                                                int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimappr);
                                                return Ok(new ApiResponse<object>(true, "Your Request Registered with US for points " + Amount));
                                            }
                                            else
                                            {
                                          
                                                string queryUpiClaimRejec = $"INSERT INTO ClaimDetails ([Claim_date],[Mobileno],[Amount],[document_status],[action_date],[Isapproved],[Comp_id],[UPIID]) VALUES (GETDATE(),'" + MobileNo + "'," + Amount.ToString() + ",1,null,2,'" + req.Comp_ID + "','" + req.UPIId + "' )";
                                                int count = await _databaseManager.ExecuteNonQueryAsync(queryUpiClaimRejec);
                                                return BadRequest(new ApiResponse<object>(false, "Something went wrong ,Please contact to help desk!"));
                                            }

                                        
                                        }

                                      
                                    }
                                    else
                                    {
                                        return BadRequest(new ApiResponse<object>(false, "Please contact to help desk!"));
                                    }
                                    #endregion

                                }

                            }
                            }
                            else
                            {
                                return BadRequest(new ApiResponse<object>(false, "Insufficient points to claim. Earn more points to proceed."));
                            }



                            

                       

                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "No points availble!"));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Session out, Please login again!"));
                }
                return BadRequest(new ApiResponse<object>(false, "Session out, Please login again!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while validating OTP: {ex.Message}"));
            }
        }
    }

    public class ApiSubmitClaimClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }
        public string ServiceId { get; set; }
        public string ProductId { get; set; }
        public string Productvalue { get; set; }
        public string UPIId { get; set; }
        


    }


}

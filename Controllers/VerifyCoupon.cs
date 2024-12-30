using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Numerics;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerifyCoupon : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public VerifyCoupon(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> Verifycouponcode([FromBody] VerifyCouponRequest req)
        {
            string Invalimsg = "The entered code is invalid. Please enter a valid 13-digit code or call us at 7353000903.";
            string Results=string.Empty;
            Object9420 Reg = new Object9420();
            object returnData=null;


            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Coupon code is null. Please provide a valid coupon code."));
            try
            {
                if (!string.IsNullOrEmpty(req.MobileNO) && req.MobileNO.Length == 12)
                {
                    req.MobileNO = req.MobileNO.Substring(2, 10);
                }

                if (req.UniqueCode.Length != 13)
                    return BadRequest(new ApiResponse<object>(false, "The coupon code must be exactly 13 digits long and contain only numbers. Please enter a valid code."));
                if (req.MobileNO.Length != 10 ||
                 !"56789".Contains(req.MobileNO[0]) ||
                 !req.MobileNO.All(char.IsDigit))
                {
                    return BadRequest(new ApiResponse<object>(false, "Entered mobile number is invalid. Kindly enter a valid mobile number."));
                }
               


                if (req.MobileNO.Length==10)
                    req.MobileNO="91"+req.MobileNO;
                Reg.Comp_ID = req.Comp_ID;
                Reg.Received_Code1 = req.UniqueCode.Substring(0, 5).ToString().Trim();
                Reg.Received_Code2 = req.UniqueCode.Substring(5, 8).ToString().Trim();
                Reg.Code1 = Convert.ToInt32(req.UniqueCode.Substring(0, 5).Trim());
                Reg.Code2 = Convert.ToInt32(req.UniqueCode.Substring(5, 8).Trim());
                Reg.dealerid = req.distributorID;
                Reg.consumer_name = req.ConsumerName;
                Reg.designation = req.designation;
                Reg.Lat = req.latitude;
                Reg.Long = req.longitude;
                Reg.Dial_Mode = req.Mode;
                Reg.Mode_Detail = GetIP();
                Reg.Mobile_No = req.MobileNO;
                TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indianTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);
                Reg.Enq_Date = Convert.ToDateTime(indianTime);
                Reg.callerdate = indianTime;
                Reg.callertime = indianTime.ToString("hh:mm:ss");
                var data =await _databaseManager.ExecuteDataTableAsync($"select*from M_Code where Code1='{Reg.Code1}' and Code2='{Reg.Code2}'");
                if (data.Rows.Count<=0 )
                {
                    string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='InvalidCode'";
                    var msg= await  _databaseManager.ExecuteDataTableAsync(SMStemplate);
                    if (msg.Rows.Count > 0)
                    {
                        Results= msg.Rows[0][0].ToString();
                    }
                    else
                    {

                        Results = Invalimsg;
                    }
                    Reg.Is_Success = 0;
                    await _databaseManager.InsertProductInquiryAsync(Reg);
                    returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                    return Ok(new ApiResponse<object>(false, Results, returnData));
                }
                bool statuscode=await  FindCheckedStatus(data);
                if (statuscode)
                {
                    string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='InvalidCode'";
                    var msg = await _databaseManager.ExecuteDataTableAsync(SMStemplate);
                    if (msg.Rows.Count > 0)
                    {
                        Results = msg.Rows[0][0].ToString();
                    }
                    else
                    {
                        Results = Invalimsg;
                    }
                    Reg.Is_Success = 0;
                    await _databaseManager.InsertProductInquiryAsync(Reg);
                    returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                    return Ok(new ApiResponse<object>(false, Results, returnData));
                }
                string qreydata= $"select [Row_ID],[Code1],[Code2], Use_Count,[Pro_ID] from M_Code where ([DispatchFlag] = 1) AND ([ReceiveFlag] = 1) AND (Batch_No IS NOT NULL  OR [Batch_No] IS NULL) AND  ([ScrapeFlag] =  0 OR [ScrapeFlag] IS NULL) AND [Code1] ='{Reg.Code1}'  AND [Code2] ='{Reg.Code2}'";
                var CodeStatus = await _databaseManager.ExecuteDataTableAsync(qreydata);
                if (CodeStatus.Rows.Count > 0)
                {
                    Dictionary<string, object> inputParameters = new Dictionary<string, object>
                    {
                        { "@code1", Reg.Code1 },
                        { "@code2", Reg.Code2}, 
                        { "@url", "" }
                    };
                    var dsres = await _databaseManager.ExecuteStoredProcedureDataTableAsync("Proc_GetProductDtsByCode1andcode2", inputParameters);
                    if (dsres.Rows.Count > 0)
                    {
                        #region Validate App and Code
                        Dictionary<string, object> inputParameterscodestatus = new Dictionary<string, object>
                        {
                            { "@pro_id", "" },
                            { "@Code1", Reg.Code1 },
                            { "@Code2", Reg.Code2}
                            
                        };
                        var productdata = await _databaseManager.ExecuteStoredProcedureDataTableAsync("Proc_GetServicesAssignAgainstProduct", inputParameterscodestatus);
                        if (productdata.Rows.Count > 0)
                        {
                            if(req.Comp_ID!= productdata.Rows[0]["Comp_id"].ToString())
                            {
                                returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                                return BadRequest(new ApiResponse<object>(false, "The entered coupon code is invalid, kindly enter a valid coupon code", returnData));
                            }
                        }
                        #endregion
                        #region Validate KYC If Code check not allow
                        string query = $"SELECT kyc_Details FROM BrandSettings WHERE Comp_ID='{req.Comp_ID}'";
                        var kycdetailsjson = await _databaseManager.ExecuteDataTableAsync(query);

                        if (kycdetailsjson.Rows.Count > 0)
                        {
                            JObject jdata = JObject.Parse(kycdetailsjson.Rows[0]["kyc_Details"].ToString());
                            string AadharCardkyc = jdata["AadharCard"].ToString();
                            string PANCardkyc = jdata["PANCard"].ToString();
                            string AccountDetailskyc = jdata["AccountDetails"].ToString();
                            string UPIkyc = jdata["UPI"].ToString();
                            string Codecheck = jdata["Codecheck"].ToString();

                            // Check if any KYC is Yes
                            bool isKycCompleted = AadharCardkyc == "Yes" || PANCardkyc == "Yes" || AccountDetailskyc == "Yes" || UPIkyc == "Yes";

                            // Initialize the list of KYC types to check
                            List<string> selectedKycTypes = new List<string>();

                            if (AadharCardkyc == "Yes") selectedKycTypes.Add("AadharCard");
                            if (PANCardkyc == "Yes") selectedKycTypes.Add("PANCard");
                            if (AccountDetailskyc == "Yes") selectedKycTypes.Add("AccountDetails");
                            if (UPIkyc == "Yes") selectedKycTypes.Add("UPI");

                            if (Codecheck == "No")
                            {
                                // If any KYC is completed, check the status of each selected KYC
                                if (isKycCompleted)
                                {
                                    string verifykyc = $"SELECT panekycStatus, aadharkycStatus, bankekycStatus, UPIKYCSTATUS FROM M_Consumer WHERE mobileno='{req.MobileNO}'";
                                    var consumerkyc = await _databaseManager.ExecuteDataTableAsync(verifykyc);

                                    if (consumerkyc.Rows.Count > 0)
                                    {
                                        string mpanekycStatus = consumerkyc.Rows[0]["panekycStatus"].ToString();
                                        string maadharkycStatus = consumerkyc.Rows[0]["aadharkycStatus"].ToString();
                                        string mbankekycStatus = consumerkyc.Rows[0]["bankekycStatus"].ToString();
                                        string mUPIKYCSTATUS = consumerkyc.Rows[0]["UPIKYCSTATUS"].ToString();

                                        // Loop through each selected KYC type and verify its status in DB
                                        foreach (var kycType in selectedKycTypes)
                                        {
                                            string kycStatus = "Approved"; // Default status is "Approved", will update if needed

                                            // Assign the respective KYC status for each type
                                            if (kycType == "AadharCard")
                                            {
                                                kycStatus = maadharkycStatus;
                                            }
                                            else if (kycType == "PANCard")
                                            {
                                                kycStatus = mpanekycStatus;
                                            }
                                            else if (kycType == "AccountDetails")
                                            {
                                                kycStatus = mbankekycStatus;
                                            }
                                            else if (kycType == "UPI")
                                            {
                                                kycStatus = mUPIKYCSTATUS;
                                            }

                                            // Check if the KYC status is not "Approved"
                                            if (kycStatus != "Online" && kycStatus != "1")
                                            {
                                                // If any KYC is not approved, return a failure message
                                                 returnData = new
                                                {
                                                    points = 0,
                                                    cash = "",
                                                    Status = "Failed",
                                                    CodeType = "Invalid",
                                                    CodeNumber = $"{Reg.Code1}{Reg.Code2}",
                                                    Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                                };

                                                return BadRequest(new ApiResponse<object>(false, "Please complete your KYC to check coupon code", returnData));
                                            }
                                        }

                                       
                                        Codecheck = "Yes"; 
                                    }
                                    else
                                    {
                                        // No KYC information found for the user
                                         returnData = new
                                        {
                                            points = 0,
                                            cash = "",
                                            Status = "Failed",
                                            CodeType = "Invalid",
                                            CodeNumber = $"{Reg.Code1}{Reg.Code2}",
                                            Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                        };

                                        return BadRequest(new ApiResponse<object>(false, "No KYC information found", returnData));
                                    }
                                }
                                else
                                {
                                    // If no KYC is completed, return failure message
                                     returnData = new
                                    {
                                        points = 0,
                                        cash = "",
                                        Status = "Failed",
                                        CodeType = "Invalid",
                                        CodeNumber = $"{Reg.Code1}{Reg.Code2}",
                                        Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                                    };

                                    return BadRequest(new ApiResponse<object>(false, "Code check not allowed", returnData));
                                }
                            }
                        }
                        #endregion


                        if (CodeStatus.Rows[0]["Use_Count"].ToString() == "0")
                        {
                            var resultdatamsg=await _databaseManager.ServiceRequestCheck(Reg, dsres,false);
                            Results= resultdatamsg.Message;
                            if (Results == "")
                            {
                                return BadRequest(new ApiResponse<object>(false, "Something went wrong ! . Please check your report or contact to service provider.", null));
                            }
                            Results = resultdatamsg.Message;
                            return Ok(new ApiResponse<object>(false, Results, resultdatamsg.ReturnData));
                        }
                        else if (string.IsNullOrEmpty(CodeStatus.Rows[0]["Use_Count"].ToString()))
                        {
                            var resultdatamsg = await _databaseManager.ServiceRequestCheck(Reg,dsres,false);
                            Results = resultdatamsg.Message;
                            if (Results == "")
                            {
                                return BadRequest(new ApiResponse<object>(false, "Something went wrong ! . Please check your report or contact to service provider.", null));
                            }
                            return Ok(new ApiResponse<object>(true, Results, resultdatamsg.ReturnData));
                        }
                        else if (Convert.ToInt32(CodeStatus.Rows[0]["Use_Count"]) >0)
                        {
                            var resultdatamsg = await _databaseManager.ServiceRequestCheck(Reg, dsres, true);
                            Results = resultdatamsg.Message;
                            if (Results == "")
                            {
                                return BadRequest(new ApiResponse<object>(false, "Something went wrong ! . Please check your report or contact to service provider.", null));
                            }
                            return Ok(new ApiResponse<object>(false, Results, resultdatamsg.ReturnData));
                        }
                    }
                    else
                    {
                        if (CodeStatus.Rows[0]["Use_Count"].ToString() != "0")
                        {
                            Reg.Is_Success = 2;
                            DateTime Enq_Date = Convert.ToDateTime(DateTime.Now);
                            if (req.latitude != null && req.longitude != null)
                            {
                                location loc = new location();
                                loc.code1 = Reg.Received_Code1;
                                loc.code2 = Reg.Received_Code2;
                                loc.latitude = req.latitude;
                                loc.longitude = req.longitude;
                                loc.EnqDate = Enq_Date;
                                //db1.GetLocationFromLongLat(loc);   Use Location Convert API Here
                            }

                            string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='Checked'";
                            var msg = await _databaseManager.ExecuteDataTableAsync(SMStemplate);
                            if (msg.Rows.Count > 0)
                            {
                                Results = msg.Rows[0][0].ToString();
                            }
                            else
                            {
                                Results = "The service of the coupon is not valid kindly connect to your service provider";
                            }
                            Reg.Is_Success = 0;
                            await _databaseManager.InsertProductInquiryAsync(Reg);
                            returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                            return Ok(new ApiResponse<object>(false, Results, returnData));
                        }
                        else
                        {
                            string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='InvalidCode'";
                            var msg = await _databaseManager.ExecuteDataTableAsync(SMStemplate);
                            if (msg.Rows.Count > 0)
                            {
                                Results = msg.Rows[0][0].ToString();
                            }
                            else
                            {
                                Results = Invalimsg;
                            }
                            Reg.Is_Success = 0;
                            await _databaseManager.InsertProductInquiryAsync(Reg);
                            returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                            return Ok(new ApiResponse<object>(false, Results, returnData));
                        }
                    }
                }
                else
                {
                    string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='InvalidCode'";
                    var msg = await _databaseManager.ExecuteDataTableAsync(SMStemplate);
                    if (msg.Rows.Count > 0)
                    {
                        Results = msg.Rows[0][0].ToString();
                    }
                    else
                    {
                        Results = Invalimsg;
                    }
                    Reg.Is_Success = 0;
                    await _databaseManager.InsertProductInquiryAsync(Reg);
                    returnData = new { points = 0, cash = "", Status = "Failed", CodeType = "Invalid", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                    return Ok(new ApiResponse<object>(false, Results, returnData));
                }
                return StatusCode(500, new ApiResponse<object>(false, $"Internal Server Error: "));
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Code verify API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
               
            }
        }
        public async Task<bool> FindCheckedStatus(DataTable dt)
        {
            Object9420 Reg = new Object9420();
            if (dt.Rows[0]["Use_Type"].ToString() != "L")
            {
                Reg.Comp_type = "D"; 
                string qry = $"SELECT Status FROM Comp_Reg WHERE Comp_ID = (SELECT comp_id FROM pro_reg WHERE pro_id = '{dt.Rows[0]["pro_id"].ToString()}')";
                var data = await _databaseManager.ExecuteDataTableAsync(qry);
                if (data == null || data.Rows.Count == 0)
                {
                    return false;
                }
                else
                {
                    string status = data.Rows[0]["Status"].ToString();
                    if (status != "1")
                    {
                        return true;  
                    }
                    else
                    {
                        return false; 
                    }
                }
            }
            else
            {
                Reg.Comp_type = "L";
                return false;
            }
        }
        public string GetIP()
        {
            string ipAddress = Request.Headers["X-Forwarded-For"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                // If no "X-Forwarded-For" header is present, fallback to the RemoteIpAddress
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            else
            {
                // If "X-Forwarded-For" contains multiple IPs, take the first one
                string[] ipAddresses = ipAddress.Split(',');
                ipAddress = ipAddresses[0]; // Get the first IP in the list
            }

            return ipAddress;
        }
    }
    public class VerifyCouponRequest
    {
        public string UniqueCode { get; set; }
        public string MobileNO { get; set; }
        public string Comp_ID { get; set; }
        public string Mode { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string? PinCode { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? callerdate { get; set; }
        public string? distributorID { get; set; }
        public string? ConsumerName { get; set; }
        public string? designation { get; set; }


    }
}

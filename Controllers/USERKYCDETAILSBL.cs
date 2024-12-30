using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class USERKYCDETAILSBL : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public USERKYCDETAILSBL(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> JsonGet([FromBody] KYCDETAILSREQUEST Req)
        {
            try
            {
                var dtconsu = await _databaseManager.ExecuteDataSetAsync($"exec GetKycDetails '{Req.M_Consumerid}'");

                if (dtconsu.Tables.Count > 0)
                {
                    var kycDetail = new kycdetailsresponse();

                    if (dtconsu.Tables[0].Rows.Count > 0)
                    {
                        kycDetail.AccountHoldername = MaskSensitiveData(dtconsu.Tables[0].Rows[0]["AccountHolderName"].ToString()) ?? null;
                        kycDetail.IfscCode = MaskSensitiveData(dtconsu.Tables[0].Rows[0]["Ifsc_Code"].ToString()) ?? null;
                        kycDetail.AccountNo = MaskSensitiveData(dtconsu.Tables[0].Rows[0]["AccountNo"].ToString()) ?? null;
                        kycDetail.Bank_Name = MaskSensitiveData(dtconsu.Tables[0].Rows[0]["Bank_Name"].ToString()) ?? null;
                    }

                    if (dtconsu.Tables[1].Rows.Count > 0)
                    {
                        kycDetail.PanName = dtconsu.Tables[1].Rows[0]["PanName"].ToString() ?? null;
                        kycDetail.PanCardNumber = MaskSensitiveData(dtconsu.Tables[1].Rows[0]["PanCardNumber"].ToString());
                        //kycDetail.DateOfBirth = dtconsu.Tables[1].Rows[0]["DateOfBirth"].ToString()
                    }

                    if (dtconsu.Tables[2].Rows.Count > 0)
                    {
                        kycDetail.AadharNo = MaskSensitiveData(dtconsu.Tables[2].Rows[0]["AadharNo"].ToString()) ?? null;
                        kycDetail.AadharName = dtconsu.Tables[2].Rows[0]["AadharName"].ToString() ?? null;
                    }

                    if (dtconsu.Tables[3].Rows.Count > 0)
                    {
                        kycDetail.UpiId = MaskSensitiveData(dtconsu.Tables[3].Rows[0]["UpiId"].ToString()) ?? null;
                        kycDetail.UPINAME = dtconsu.Tables[3].Rows[0]["Benificiryname"].ToString() ?? null;
                    }

                    return Ok(new ApiResponse<kycdetailsresponse>(true, "KYC details fetched successfully.", kycDetail));
                }
                else
                {
                    return StatusCode(404, new ApiResponse<object>(false, "No records found for the given M_Consumerid.", null));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in USERKYCDETAILS API: " + ex.Message + " ,Stack Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred.", ex.Message));
            }
        }

        private string MaskSensitiveData(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 4)
                return input;

            int length = input.Length;
            string firstTwo = input.Substring(0, 2);
            string lastTwo = input.Substring(length - 2, 2);
            string maskedPart = new string('*', length - 4);

            return firstTwo + maskedPart + lastTwo;
        }
    }

    public class KYCDETAILSREQUEST
    {
        public int M_Consumerid { get; set; }
        public string? Mobileno { get; set; }
        public string? Comp_id { get; set; }
    }

    public class kycdetailsresponse
    {
        public string AccountHoldername { get; set; }
        public string IfscCode { get; set; }
        public string AccountNo { get; set; }
        public string Bank_Name { get; set; }
        public string PanName { get; set; }
        public string PanCardNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string AadharNo { get; set; }
        public string AadharName { get; set; }
        public string UpiId { get; set; }
        public string UPINAME { get; set; }
    }
}

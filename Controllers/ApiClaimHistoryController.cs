using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiClaimHistoryController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public ApiClaimHistoryController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> ApiClaimHistory([FromBody] ApiClaimHistoryClass req)
        {
            //M_Consumerid,Comp_ID
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                if (req.Comp_ID != "" && req.M_Consumerid != "")
                {
                    DataTable dt = await _databaseManager.SelectTableDataAsync("m_consumer", "top 1 mobileNo", "M_Consumerid= '" + req.M_Consumerid + "' and IsDelete='0' order by  [M_Consumerid] Desc");
                    if (dt.Rows.Count > 0)
                    {
                        Dictionary<string, object> inputParameters = new Dictionary<string, object>
                        {
                            { "@M_consumerid", req.M_Consumerid }
                        };
                        DataTable dt1 = await _databaseManager.ExecuteStoredProcedureDataTableAsync("sp_get_Cunsumer_point", inputParameters);
                        if (dt1.Rows.Count > 0)
                        {
                            
                            string giftimages = "";
                            DataTable claimhistory = await _databaseManager.SelectTableDataAsync(
      "ClaimDetails as a, Claim_gift as b",
      "format(Claim_date,'dd MMM yyyy HH:mm tt') as Claim_date, a.Amount, a.Isapproved, a.Mobileno, a.vendor_comment as Message, b.Gift_name, b.Gift_value, b.Gift_desc, b.Gift_image, b.gift_id",
      "a.Amount = b.Gift_value and a.Comp_id = b.CompID and a.Mobileno like '%" + dt.Rows[0]["mobileNo"] + "' and a.Comp_id = '" + req.Comp_ID + "' order by a.Row_id desc"
  );

                            if (!claimhistory.Columns.Contains("gift_images"))
                            {
                                claimhistory.Columns.Add("gift_images", typeof(string));
                            }

                            if (claimhistory.Rows.Count > 0)
                            {
                                foreach (DataRow claimRow in claimhistory.Rows)
                                {
                                    // Fetch gift images for the current gift_id
                                    DataTable claimImages = await _databaseManager.SelectTableDataAsync(
                                        "gifttable_images",
                                        "gift_images",
                                        "gift_id = '" + claimRow["gift_id"] + "'"
                                    );

                                    // Collect gift images as a list
                                 
                                    if (claimImages.Rows.Count > 0)
                                    {
                                        giftimages = claimImages.Rows[0]["gift_images"].ToString().TrimEnd(',');
                                    }

                                    // Add gift_images to each claim row
                                    claimRow["gift_images"] = giftimages;
                                    if (giftimages.ToString().Length > 5)
                                    {
                                        claimRow["gift_images"] = "https://qa.vcqru.com/" + giftimages.ToString().Replace("~/", "");
                                    }
                                }

                                // Transform DataTable to the desired output structure
                                var groupedData = claimhistory.AsEnumerable()
                                    .Select(row => new
                                    {
                                        Claim_date = row["Claim_date"],
                                        Isapproved = row["Isapproved"],
                                        Gift_name = row["Gift_name"],
                                        Gift_value = row["Gift_value"],
                                        Gift_image = "https://qa.vcqru.com/" + row["Gift_image"].ToString().Replace("~/", ""),
                                        Amount = row["Amount"],
                                        Gift_desc = row["Gift_desc"]?.ToString() ?? "",
                                        Message = row["Message"]?.ToString() ?? "",
                                        gift_id = row["gift_id"],
                                        gift_images = row["gift_images"].ToString()
                                            .Split(',')
                                            .Where(img => !string.IsNullOrEmpty(img))
                                            .Select(img => img.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                                                ? img.Trim()
                                                : "https://qa.vcqru.com/" + img.Trim().Replace("~/", ""))
                                            .ToList()
                                    })
                                    .ToList();

                                return Ok(new ApiResponse<object>(true, "Your Claim History is here.", groupedData));
                            }

                            else
                            {
                                return Ok(new ApiResponse<object>(false, "No claim history found.", null));
                            }

                        }
                        else
                        {
                            return BadRequest(new ApiResponse<object>(false, "No Record found!."));
                        }
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "User not login!."));
                    }               
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid login detail!"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while validating OTP: {ex.Message}"));
            }
        }
    }

    public class ApiClaimHistoryClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }

    }


}

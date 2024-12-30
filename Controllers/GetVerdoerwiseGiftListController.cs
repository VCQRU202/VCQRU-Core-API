using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Reflection;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetVerdoerwiseGiftListController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public GetVerdoerwiseGiftListController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
        [HttpPost]
        public async Task<IActionResult> GetVerdoerwiseGiftList([FromBody] GetVerdoerwiseGiftListClassClass req)
        {
            //M_Consumerid,Comp_ID,limit
            if (req == null)
            return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                string ServiceId = "";

                if (req.Comp_ID != "" && req.M_Consumerid != "")
                {
                    DataTable dt = await _databaseManager.SelectTableDataAsync("m_consumer", "top 1 M_Consumerid,Vrkabel_User_Type,UPIId,mobileNo", "M_Consumerid= '"+ req.M_Consumerid+"' and IsDelete='0' order by  [M_Consumerid] Desc");
                    if (dt.Rows.Count > 0)
                    {
                        Dictionary<string, object> inputParameters1 = new Dictionary<string, object>
                        {
                            { "@M_consumerid", req.M_Consumerid },
                            {"@Compid", req.Comp_ID }
                        };

                        DataTable dt1 = await _databaseManager.ExecuteStoredProcedureDataTableAsync("sp_get_Cunsumer_point_Vendorwise", inputParameters1);                        
                        if (dt1.Rows.Count > 0)
                        {

                            Dictionary<string, object> inputParameters2 = new Dictionary<string, object>
                                {
                                    {"@companyid", req.Comp_ID }
                                };
                            DataTable Srvdt = await _databaseManager.ExecuteStoredProcedureDataTableAsync("BL_ServiceID", inputParameters2);
                            if (Srvdt.Rows.Count > 0)
                            {
                                ServiceId = Srvdt.Rows[0]["Service_ID"].ToString();
                            }
                            else
                            {
                                return BadRequest(new ApiResponse<object>(false, "Service not assign, Please contact to administrator."));
                            }
                            DataTable gifttable = new DataTable();

                            if (ServiceId == "SRV1001")
                            {
                                 gifttable = await _databaseManager.SelectTableDataAsync("Claim_gift AS a LEFT JOIN gifttable_images AS b ON  a.gift_id = b.gift_id ", "a.gift_id,a.Gift_name, a.Gift_value,a.Gift_desc,a.Gift_image,a.status,a.CompID,a.Gift_point,  CASE WHEN b.gift_images LIKE '%,%' THEN LEFT(b.gift_images, LEN(b.gift_images) - 1) ELSE b.gift_images END AS gift_images", "CompID='" + req.Comp_ID + "' and status=1 order by Gift_value asc ");

                            }
                            else
                            {
                                 gifttable = await _databaseManager.SelectTableDataAsync("Claim_gift", "gift_id,Gift_name,Gift_value,Gift_desc,Gift_image,status,CompID,Gift_point", "CompID='" + req.Comp_ID + "' and status=1 order by Gift_value asc ");

                            }
                            if (gifttable.Rows.Count > 0)
                            {
                               

                                gifttable.Columns.Add("btn_flag", typeof(Int32));
                                gifttable.Columns.Add("gift_message");
                                gifttable.Columns.Add("UPIID"); 
                                gifttable.Columns.Add("ServiceId"); 
                                DataTable Redeems = await _databaseManager.SelectTableDataAsync("BPointsTransaction", "cast(isnull(sum(RedeemPoints),0) as int) as Redeem_Points", "RedeemBy=" + req.M_Consumerid + "");
                                DataTable Claims = await _databaseManager.SelectTableDataAsync("ClaimDetails", " case when Sum(Amount) is null then 0 else Sum(isnull(Amount,0)) end as Claim", "Mobileno like '%" + dt.Rows[0]["mobileNo"] + "' and (Isapproved=1) and Comp_id='" + dt1.Rows[0]["Compid"].ToString() + "'");
                                DataTable condition = await _databaseManager.SelectTableDataAsync("point_redeem_condition", " [codition_point],[condition_match]", "comp_id='" + req.Comp_ID + "'");

                                int Total_Point = Convert.ToInt32(dt1.Rows[0]["point"].ToString());
                                int Redeem_Point = Convert.ToInt32(Redeems.Rows[0]["Redeem_Points"].ToString());
                                int Claim_Apply = Convert.ToInt32(Claims.Rows[0]["Claim"].ToString());
                                int condition_Point = Convert.ToInt32(condition.Rows[0]["codition_point"].ToString());
                                int Avlaible_Point = Total_Point - (Redeem_Point + Claim_Apply);

                                for (int i = 0; i < gifttable.Rows.Count; i++)
                                {
                                    
                                    gifttable.Rows[i]["ServiceId"] = ServiceId;
                                    gifttable.Rows[i]["Gift_desc"].ToString();
                                    gifttable.Rows[i]["Gift_name"].ToString();
                                    gifttable.Rows[i]["Gift_value"].ToString();
                                    gifttable.Rows[i]["Gift_desc"].ToString();
                                    gifttable.Rows[i]["Gift_image"] = "https://qa.vcqru.com/" + gifttable.Rows[i]["Gift_image"].ToString().Replace("~/", "");
                                    if (ServiceId == "SRV1001")
                                    {
                                        if (gifttable.Rows[i]["gift_images"].ToString().Length > 5)
                                        {
                                            gifttable.Rows[i]["gift_images"] = "https://qa.vcqru.com/" + gifttable.Rows[i]["gift_images"].ToString().Replace("~/", "");
                                        }
                                    }
                                        // gifttable.Rows[i]["gift_images"].ToString();
                                    gifttable.Rows[0]["CompID"] = req.Comp_ID;
                                    if (condition_Point <= Avlaible_Point)
                                    {
                                        gifttable.Rows[i]["btn_flag"] = 0;
                                        if (Avlaible_Point >= Convert.ToInt32(gifttable.Rows[i]["Gift_point"].ToString()))
                                        {
                                            gifttable.Rows[i]["btn_flag"] = 1;
                                            gifttable.Rows[i]["gift_message"] = "";
                                        }
                                        else
                                        {
                                            gifttable.Rows[i]["btn_flag"] = 0;
                                            gifttable.Rows[i]["gift_message"] = "You are not eligible for claim";
                                        }
                                        gifttable.AcceptChanges();
                                    }
                                }

                                gifttable.DefaultView.Sort = "Gift_value ASC";
                                gifttable = gifttable.DefaultView.ToTable();

                                var rows = gifttable.AsEnumerable().Select(row =>
                                {
                                    return gifttable.Columns.Cast<DataColumn>()
                                        .ToDictionary(column => column.ColumnName, column =>
                                        {
                                            // Handle "gift_images" column: Convert to an array or empty array if null/empty
                                            if (column.ColumnName == "gift_images")
                                            {
                                                if (row[column] == DBNull.Value || string.IsNullOrWhiteSpace(row[column].ToString()))
                                                {
                                                    return Array.Empty<string>(); // Return empty array
                                                }

                                                return row[column].ToString()
                                                    .Split(',')
                                                    .Select(img => img.StartsWith("http") ? img : $"https://qa.vcqru.com/{img.Trim()}") // Prepend base URL if needed
                                                    .Where(img => !string.IsNullOrWhiteSpace(img)) // Remove empty or invalid entries
                                                    .ToArray();
                                            }

                                            // Handle "UPIID" column: Convert to empty string if null
                                            if (column.ColumnName == "UPIID")
                                            {
                                                return row[column] == DBNull.Value ? "" : row[column].ToString();
                                            }

                                            // Handle "Gift_image" column: Prepend base URL if not an absolute URL
                                            if (column.ColumnName == "Gift_image")
                                            {
                                                return row[column].ToString().StartsWith("http")
                                                    ? row[column].ToString()
                                                    : $"https://qa.vcqru.com/{row[column].ToString().Trim()}";
                                            }

                                            return row[column]; // Return other column values as is
                                        });
                                });



                                return Ok(new ApiResponse<object>(true, "Your gifts are.!", rows));
                            }
                            else
                            {
                                return BadRequest(new ApiResponse<object>(false, "There are no gift table."));
                            }

                        }
                        else
                        {
                            return BadRequest(new ApiResponse<object>(false, "You have not enough point to claim."));                            
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
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred : {ex.Message}"));
            }
        }
    }

    public class GetVerdoerwiseGiftListClassClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }

    }


}
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetNotificationController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public GetNotificationController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetNotification([FromBody] GetNotificationClass req)
        {
            //M_Consumerid,Comp_ID
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {

                if (req.Comp_ID != "" && req.M_Consumerid != "")
                {
                    DataTable dt = await _databaseManager.SelectTableDataAsync("m_consumer", "top 1 M_Consumerid,mobileNo", "M_Consumerid= '" + req.M_Consumerid + "' and IsDelete='0' order by  [M_Consumerid] Desc");
                    if (dt.Rows.Count > 0)
                    {
                        Dictionary<string, object> inputParameters1 = new Dictionary<string, object>
                        {
                            { "@M_consumerid", req.M_Consumerid },
                            {"@Compid", req.Comp_ID }
                        };

                        DataTable dt1 = await _databaseManager.ExecuteStoredProcedureDataTableAsync("sp_get_notification", inputParameters1);

                        if (dt1.Rows.Count > 0)
                        {
                            string baseUrl = "https://qa.vcqru.com/";

                            var rows = dt1.AsEnumerable().Select(row =>
                            {
                                return dt1.Columns.Cast<DataColumn>()
                                    .ToDictionary(column => column.ColumnName, column =>
                                    {
                                        // Handle 'profile_image' column
                                        if (column.ColumnName == "profile_image")
                                        {
                                            return row[column] != DBNull.Value && !string.IsNullOrEmpty(row[column].ToString())
                                                ? baseUrl + row[column].ToString()
                                                : "";
                                        }

                                        // Handle specific columns ('apiurl', 'title') and convert to empty string if null or empty
                                        if (column.ColumnName == "apiurl" || column.ColumnName == "title")
                                        {
                                            return row[column] != DBNull.Value && !string.IsNullOrEmpty(row[column].ToString())
                                                ? row[column].ToString()
                                                : "";
                                        }

                                        // Default behavior for other columns
                                        return row[column];
                                    });
                            }).ToList();



                            return Ok(new ApiResponse<object>(true, "Successfully", rows));
                        }
                        else
                        {
                            return BadRequest(new ApiResponse<object>(false, "No notifications yet..."));
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
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while Banner: {ex.Message}"));
            }
        }
    }

    public class GetNotificationClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }
    }


}

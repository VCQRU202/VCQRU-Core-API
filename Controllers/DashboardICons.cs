using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardICons : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public DashboardICons(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> ICONS([FromBody] dashboardiconsrequest Req)
        {
            try
            {
                string sqlqry = $"SELECT DashboardIcons FROM BrandSettings WHERE Comp_ID='{Req.Comp_id}'";
                var Resultdata = await _databaseManager.ExecuteDataTableAsync(sqlqry);

                if (Resultdata.Rows.Count > 0)
                {
                    string jdata = Resultdata.Rows[0]["DashboardIcons"].ToString();
                    if (string.IsNullOrEmpty(jdata))
                    {
                        return BadRequest(new ApiResponse<object>(false, "Icon data is empty."));
                    }
                    JObject parsedJson = JObject.Parse(jdata);
                    var dynamicData = new Dictionary<string, string>();
                    foreach (var property in parsedJson.Properties())
                    {
                        if (property.Value.Type == JTokenType.Array || property.Value.Type == JTokenType.Object)
                        {
                            dynamicData[property.Name] = "No"; 
                        }
                        else
                        {
                            dynamicData[property.Name] = property.Value.ToString();
                        }
                    }
                    return Ok(new ApiResponse<object>(true, "Icon data fetched successfully.", dynamicData));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "No icons found."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in ICONS API: " + ex.Message + " ,StackTrace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(true, "An error occurred while processing your request."));
            }
        }
    }

    public class dashboardiconsrequest
    {
        public string? Comp_id { get; set; }
    }
}

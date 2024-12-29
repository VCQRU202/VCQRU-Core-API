using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandSetting : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public BrandSetting(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> JsonGet([FromBody] BrandsettingRequest Req)
        {
            try
            {
                string Querystring = $"SELECT CompData FROM BrandSettings WHERE Comp_ID='{Req.Comp_id}'";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(Querystring);

                if (dtconsu.Rows.Count > 0)
                {
                    // Extract CompData as a string (assuming it contains a JSON string)
                    var compDataJson = dtconsu.Rows[0]["CompData"].ToString();

                    // Check if CompData is not empty or null
                    if (!string.IsNullOrEmpty(compDataJson))
                    {
                        // Deserialize the JSON string into the appropriate model
                        var settingsModel = JsonConvert.DeserializeObject<BrandSettingcs>(compDataJson);

                        return Ok(new ApiResponse<BrandSettingcs>(true, "Brand settings retrieved successfully", settingsModel));
                    }
                    else
                    {
                        return StatusCode(404, new ApiResponse<object>(false, "CompData is empty or null.", null));
                    }
                }
                else
                {
                    return StatusCode(404, new ApiResponse<object>(false, "No records found for the given Comp_id.", null));
                }
            }
            catch (SqlException sqlEx)
            {
                return StatusCode(500, new ApiResponse<object>(false, "Database error occurred.", sqlEx.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred.", ex.Message));
            }
        }
    }
}

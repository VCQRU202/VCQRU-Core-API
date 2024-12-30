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
                string Querystring = $"SELECT CompData,Claim_Settings FROM BrandSettings WHERE Comp_ID='{Req.Comp_id}'";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(Querystring);

                if (dtconsu.Rows.Count > 0)
                {
                    var compDataJson = dtconsu.Rows[0]["CompData"].ToString();
                    var claimSettingsJson = dtconsu.Rows[0]["Claim_Settings"].ToString();

                    if (string.IsNullOrEmpty(compDataJson))
                        return NotFound(new ApiResponse<object>(false, "Comp data is empty or null.", null));

                    if (string.IsNullOrEmpty(claimSettingsJson))
                        return NotFound(new ApiResponse<object>(false, "Claim settings are empty or null.", null));

                    var brandSettings = JsonConvert.DeserializeObject<BrandSettingcs>(compDataJson);
                    var claimSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(claimSettingsJson);

                    foreach (var setting in claimSettings)
                    {
                       // var property = brandSettings.GetType().GetProperty(setting.Key);
                        var property = brandSettings.GetType()
                        .GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, setting.Key, StringComparison.OrdinalIgnoreCase));
                        if (property != null && property.CanWrite)
                        {
                            try
                            {
                                var convertedValue = Convert.ChangeType(setting.Value, property.PropertyType);
                                property.SetValue(brandSettings, convertedValue);
                            }
                            catch
                            {
                                // Log the error and continue
                                Console.WriteLine($"Failed to set property: {setting.Key}");
                            }
                        }
                    }

                    return Ok(new ApiResponse<object>(true, "Brand settings retrieved successfully", brandSettings));
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "No records found for the given Comp_ID.", null));
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

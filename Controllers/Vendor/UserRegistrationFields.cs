using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using CoreApi_BL_App.Services;

namespace CoreApi_BL_App.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationFields : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public UserRegistrationFields(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetFieldSettings([FromBody] BrandsettingRequest Req)
        {
            try
            {
                string Querystring = $"SELECT RegistrationFields FROM BrandSettings WHERE Comp_ID='{Req.Comp_id}'";
                var dtconsu = await _databaseManager.ExecuteDataTableAsync(Querystring);

                if (dtconsu.Rows.Count > 0)
                {
                    // Extract RegistrationFields as a string (assuming it contains a JSON string)
                    var registrationFieldsJson = dtconsu.Rows[0]["RegistrationFields"].ToString();

                    // Check if RegistrationFields is not empty or null
                    if (!string.IsNullOrEmpty(registrationFieldsJson))
                    {
                        // Deserialize the JSON string into the appropriate model (List<FieldSetting>)
                        var fieldSettings = JsonConvert.DeserializeObject<List<FieldSetting>>(registrationFieldsJson);

                        return Ok(new ApiResponse<List<FieldSetting>>(true, "Registration fields retrieved successfully", fieldSettings));
                    }
                    else
                    {
                        return StatusCode(404, new ApiResponse<object>(false, "RegistrationFields is empty or null.", null));
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

using System;
using System.Data;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationFields : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public UserRegistrationFields(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFieldSetting([FromBody] List<FieldSetting> fieldSettings)
        {
            if (fieldSettings == null || !fieldSettings.Any())
            {
                return BadRequest(new ApiResponse<object>(false, "Field settings are required."));
            }

            try
            {
                // Convert the field settings list to JSON
                var json = JsonConvert.SerializeObject(fieldSettings);

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
                {
                    string query = "INSERT INTO BrandSettings (RegistrationFields) VALUES (@RegistrationFields)";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@RegistrationFields", SqlDbType.NVarChar) { Value = json });

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return Ok(new ApiResponse<object>(true, "Field settings inserted successfully."));
                        }
                        else
                        {
                            return StatusCode(500, new ApiResponse<object>(false, "An error occurred while inserting data into the database."));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error occurred.", ex.Message));
            }
        }

        // GET: Retrieve form field settings from the database
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFieldSettings(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
                {
                    string query = "SELECT RegistrationFields FROM BrandSettings WHERE ID = @ID";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            var fieldSettings = JsonConvert.DeserializeObject<List<FieldSetting>>(result.ToString());
                            return Ok(new ApiResponse<List<FieldSetting>>(true, "Field settings retrieved successfully", fieldSettings));
                        }
                        else
                        {
                            return NotFound(new ApiResponse<object>(false, "Field settings not found for the provided ID."));
                        }
                    }
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


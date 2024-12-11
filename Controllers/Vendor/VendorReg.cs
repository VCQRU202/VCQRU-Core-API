using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Collections.Generic;
using System.Data;
using CoreApi_BL_App.Models;

namespace CoreApi_BL_App.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorReg : ControllerBase
    {
        private readonly string _connectionString;

        public VendorReg(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("defaultConnectionbeta");
        }

        // Create or Update Field API
        [HttpPost("fields")]
        public IActionResult CreateOrUpdateField([FromBody] List<FieldDefinitionDto> fieldDefinitions)
        {
            try
            {
                // Serialize the list of FieldDefinitionDto objects into a single JSON string
                string fieldDataJson = JsonSerializer.Serialize(fieldDefinitions);

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO BrandSettings (RegistrationFields, Created)
                        VALUES (@RegistrationFields, GETDATE())";

                    using (var command = new SqlCommand(query, connection))
                    {
                        // Set the parameter for FieldData to the serialized JSON string
                        command.Parameters.AddWithValue("@RegistrationFields", fieldDataJson);

                        // Execute the query to insert data
                        command.ExecuteNonQuery();
                    }
                }

                // Returning success response
                return Ok(new ApiResponse<object>(true, "Fields created successfully"));
            }
            catch (Exception ex)
            {
                // Returning error response
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
        }

        // Get Field by ID API
        [HttpGet("fields/{id}")]
        public IActionResult GetFieldById(int id)
        {
            List<FieldDefinitionDto> fieldDefinitions = null;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT RegistrationFields FROM BrandSettings WHERE ID = @ID";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var fieldDataJson = reader["RegistrationFields"].ToString();
                                // Deserialize JSON array to a list of FieldDefinitionDto objects
                                fieldDefinitions = JsonSerializer.Deserialize<List<FieldDefinitionDto>>(fieldDataJson);
                            }
                        }
                    }
                }

                if (fieldDefinitions != null)
                {
                    // Returning success response with data
                    return Ok(new ApiResponse<List<FieldDefinitionDto>>(true, "Field data retrieved successfully", fieldDefinitions));
                }
                else
                {
                    // Returning not found response
                    return NotFound(new ApiResponse<object>(false, "Field data not found"));
                }
            }
            catch (Exception ex)
            {
                // Returning error response
                return BadRequest(new ApiResponse<object>(false, ex.Message));
            }
        }
    }

    // DTOs
    public class FieldDefinitionDto
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public string Hint { get; set; }  // Added Hint property for additional guidance
        public string Regex { get; set; } // Added Regex property for validation patterns
    }

    public class DropdownMasterDto
    {
        public string MasterType { get; set; }
        public string MasterValue { get; set; }
        public bool IsActive { get; set; }
    }
}
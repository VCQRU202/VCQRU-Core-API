using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class E_KYCSelectOption : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public E_KYCSelectOption(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> PostEKYC([FromBody] E_KYCSelectOptions request)
        {
            if (request.SelectedOptions == null || request.SelectedOptions.Count == 0)
                return BadRequest(new ApiResponse<object>(false, "At least one option must be selected."));

            string jsonOptions = System.Text.Json.JsonSerializer.Serialize(request.SelectedOptions);

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
            {
                string query = "INSERT INTO BrandSettings (EKYCSelectedOptions) VALUES ( @EKYCSelectedOptions); SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@EKYCSelectedOptions", jsonOptions);

                await conn.OpenAsync();
                var id = await cmd.ExecuteScalarAsync();

                // Return a success response
                var response = new ApiResponse<object>(true, "Data saved successfully.", new { Id = id });
                return Ok(response);
            }
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetEKYC(int Id)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
            {
                string query = "SELECT Id, EKYCSelectedOptions FROM BrandSettings WHERE Id = @Id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", Id);

                await conn.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                    return NotFound(new ApiResponse<object>(false, "No data found for the given ID."));

                EKYCResponse response = null;

                while (await reader.ReadAsync())
                {
                    response = new EKYCResponse
                    {
                        ID = reader.GetInt32(0),
                        SelectedOptions = System.Text.Json.JsonSerializer.Deserialize<List<string>>(reader.GetString(1)),
                    };
                }

                // Return a success response with the data
                return Ok(new ApiResponse<EKYCResponse>(true, "Data retrieved successfully.", response));
            }
        }
    }

}

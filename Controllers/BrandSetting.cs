using CoreApi_BL_App.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    
   
    
    [Route("api/[controller]")]
    [ApiController]
    public class BrandSetting : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BrandSetting(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // POST: Insert data into the database
        [HttpPost]
        public async Task<IActionResult> Brandsettingpost(IFormFile logo, IFormFile splashImage, [FromForm] string backgroundColor, [FromForm] string compname)
        {
            if (logo == null || splashImage == null || string.IsNullOrEmpty(backgroundColor) || string.IsNullOrEmpty(compname))
            {
                return BadRequest(new ApiResponse<object>(false, "Logo,Splash Image, Background Color, and Company Name are required."));
            }

            try
            {
                // Saving the logo to the server
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", logo.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logo.CopyToAsync(stream);
                }
                var splashImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", splashImage.FileName);
                using (var stream = new FileStream(splashImagePath, FileMode.Create))
                {
                    await splashImage.CopyToAsync(stream);
                }

                var brandData = new
                {
                    LogoUrl = "/uploads/" + logo.FileName,           // Path for logo
                    SplashImageUrl = "/uploads/" + splashImage.FileName, // Path for splash image
                    BackgroundColor = backgroundColor,
                    Compname = compname
                };

                var json = JsonConvert.SerializeObject(brandData);

                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
                {
                    string query = "INSERT INTO BrandSettings (CompData) VALUES (@CompData)";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@CompData", SqlDbType.NVarChar) { Value = json });

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            return Ok(new ApiResponse<object>(true, "Data inserted successfully."));
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

        // GET: Retrieve data from the database
        [HttpGet("{id}")]
        public async Task<IActionResult> JsonGet(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
                {
                    string query = "SELECT CompData FROM BrandSettings WHERE ID=@ID";
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.Add(new SqlParameter("@ID", SqlDbType.Int) { Value = id });

                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            var settingsModel = JsonConvert.DeserializeObject<BrandSettingcs>(result.ToString());
                            return Ok(new ApiResponse<BrandSettingcs>(true, "Brand settings retrieved successfully", settingsModel));
                        }
                        else
                        {
                            return NotFound(new ApiResponse<object>(false, "Brand settings not found for the provided ID."));
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


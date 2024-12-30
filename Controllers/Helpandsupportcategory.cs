using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpAndSupportCategoryController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<HelpAndSupportCategoryController> _logger;

        public HelpAndSupportCategoryController(DatabaseManager databaseManager, ILogger<HelpAndSupportCategoryController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }


        public async Task<IActionResult> RaiseTicket([FromBody] cls_Support req)
        {
            if (req == null)
            {
                return BadRequest(new ApiResponse<object>(false, "Help and support request cannot be empty."));
            }

            try
            {
                string sqlQuery = $"SELECT helpandsupport FROM BrandSettings WHERE Comp_ID = '{req.Comp_id}'";
                var result = await _databaseManager.ExecuteDataTableAsync(sqlQuery);

                if (result.Rows.Count > 0)
                {
                    var jsonData = result.Rows[0]["helpandsupport"].ToString();
                    var categories = JsonConvert.DeserializeObject<List<cls_categoryresponse>>(jsonData);
                    if (categories == null || categories.Count == 0)
                    {
                        return NotFound(new ApiResponse<object>(false, "No valid categories found in the data."));
                    }
                    return Ok(new ApiResponse<List<cls_categoryresponse>>(true, "Category fetched successfully!", categories));
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "No data found for the given company ID."));
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError($"JSON Parsing error: {jsonEx.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, "Invalid JSON data format."));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Help and Support category: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, "An error occurred while processing the request."));
            }
        }
    }

    public class cls_Support
    {
        public string Comp_id { get; set; }
    }
    public class cls_categoryresponse
    {
        public string id { get; set; }
        public string topic { get; set; }
    }

}

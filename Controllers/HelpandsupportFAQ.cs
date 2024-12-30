using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HelpandsupportFAQ : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<TicketRaiseController> _logger;

        public HelpandsupportFAQ(DatabaseManager databaseManager, ILogger<TicketRaiseController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        [HttpPost("get-faq")]
        public async Task<IActionResult> RaiseTicket([FromBody] cls_faq Req)
        {
            if (Req == null || string.IsNullOrEmpty(Req.id))
            {
                return BadRequest(new ApiResponse<object>(false, "Topic ID cannot be empty."));
            }

            try
            {
                string sqlQuery = $"SELECT FAQ FROM BrandSettings WHERE Comp_ID = '{Req.Comp_id}'";
                var result = await _databaseManager.ExecuteDataTableAsync(sqlQuery);

                if (result.Rows.Count > 0)
                {
                    // Replace with actual data from the database in production
                    var jsonData = result.Rows[0]["FAQ"].ToString();

                    // Deserialize JSON data
                    var faqList = JsonConvert.DeserializeObject<List<cls_faqresponse>>(jsonData);
                    if (faqList == null || faqList.Count == 0)
                    {
                        return NotFound(new ApiResponse<object>(false, "FAQ not found."));
                    }

                    // Filter by topicid
                    var filteredFAQs = faqList.Where(f => f.topicid == Req.id).ToList();
                    if (filteredFAQs.Count == 0)
                    {
                        return NotFound(new ApiResponse<object>(false, $"No FAQs found for topic"));
                    }

                    return Ok(new ApiResponse<List<cls_faqresponse>>(true, "FAQs fetched successfully!", filteredFAQs));
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
                _logger.LogError($"Error in Help and Support FAQ: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<object>(false, "An error occurred while processing the request."));
            }
        }

    }

    public class cls_faq
    {
        public string id { get; set; }
        public string Comp_id { get; set; }
    }
    public class cls_faqresponse
    {
        public string id { get; set; }
        public string topicid { get; set; }
        public string faqquestion { get; set; }
        public string faqanswer { get; set; }
    }
}

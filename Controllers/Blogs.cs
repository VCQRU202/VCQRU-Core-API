using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Blogs : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Blogs(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }
        [HttpPost]
        public async Task<IActionResult> Verifycouponcode([FromBody] Introrequest Req)
        {
            if (Req == null || string.IsNullOrEmpty(Req.Comp_ID))
                return BadRequest(new ApiResponse<object>(false, "Invalid request. Comp_ID is required."));

            try
            {
                string qry = $"SELECT Blogs FROM BrandSettings WHERE Comp_ID ='{Req.Comp_ID}' ";
                var result = await _databaseManager.ExecuteDataTableAsync(qry);

                if (result.Rows.Count > 0)
                {
                    string? existingJsonData = result.Rows[0]["Blogs"].ToString();
                    if (!string.IsNullOrEmpty(existingJsonData))
                    {
                        var allInfos = JsonConvert.DeserializeObject<List<BlogsInfo>>(existingJsonData);
                        

                        return Ok(new ApiResponse<object>(true, "Blogs data fetched successfully.", allInfos));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Blogs data not found."));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid Company data."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in Blogs API: " + ex.Message + ", Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error."));
            }
        }
    }

    
    public class BlogsInfo
    {
        public string CompanyId { get; set; }
        public string Header { get; set; }
        public string Contains { get; set; }
        public string ImagePath { get; set; }
        public string filetype { get; set; }
        public string uploaded_date { get; set; }
    }
}

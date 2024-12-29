using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Introduction : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Introduction(DatabaseManager databaseManager)
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
                string qry = $"SELECT Introdata FROM BrandSettings WHERE Comp_ID ='{Req.Comp_ID}' ";
                var result = await _databaseManager.ExecuteDataTableAsync(qry);

                if (result.Rows.Count > 0)
                {
                    string? existingJsonData = result.Rows[0]["Introdata"].ToString();
                    if (!string.IsNullOrEmpty(existingJsonData))
                    {
                        var allInfos = JsonConvert.DeserializeObject<List<ManufacturerInfo>>(existingJsonData);
                        string baseUrl = "https://qa.vcqru.com"; 
                        foreach (var info in allInfos)
                        {
                            if (!string.IsNullOrEmpty(info.ImagePath) && !info.ImagePath.StartsWith("http"))
                            {
                                info.ImagePath = $"{baseUrl}{info.ImagePath.TrimStart('~').Replace("\\", "/")}";
                            }
                        }

                        return Ok(new ApiResponse<object>(true, "Intro data fetched successfully.", allInfos));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "Intro data not found."));
                    }
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Comp_ID not found."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in Introduction API: " + ex.Message + ", Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error."));
            }
        }

    }
    public class Introrequest
    {
        public string? Comp_ID { get; set; }
    }
    public class ManufacturerInfo
    {
        public string CompanyId { get; set; }
        public string Header { get; set; }
        public string Contains { get; set; }
        public string ImagePath { get; set; }
    }
}

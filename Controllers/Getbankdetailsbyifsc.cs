using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Getbankdetailsbyifsc : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Getbankdetailsbyifsc(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]      
        public async Task<IActionResult> getbankdetails([FromBody] Isfcrequesst Req)
        {
            if (Req == null)
                return BadRequest(new ApiResponse<object>(false, "Invalid request data"));
            try
            {
                string query = $"select  bank,ifsc,branch,city from ifsc where ifsc='{Req.ifsccode}'";
                var result = await _databaseManager.ExecuteDataTableAsync(query);
                if (result.Rows.Count > 0) 
                {
                    var response = new
                    {
                        bank = result.Rows[0]["bank"].ToString(),
                        ifsc = result.Rows[0]["ifsc"].ToString(),
                        branch = result.Rows[0]["branch"].ToString(),
                        city = result.Rows[0]["city"].ToString()
                    };
                    return Ok(new ApiResponse<object>(true, "Bank details fetch successfully", response));
                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid ifsc code"));
                }
            }
            catch (Exception ex) 
            {
                _databaseManager.ExceptionLogs("Find Error in getbankdetailsbyifsc API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error.", ex.Message));
            }
        }
    }

    public class Isfcrequesst
    {
        public string? ifsccode { get; set; }
        public string? Comp_id { get; set; }
    }

}

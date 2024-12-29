using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Productcatalog : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Productcatalog(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsumer([FromBody] productcatalogrequest Req)
        {
            if (Req == null)
                return BadRequest(new ApiResponse<object>(false, "Invalid Request."));
            

            try
            {
                string jsondata = "";
                string jsonqry = $"Select ProductId,ProductName,ProductDescription,Category,StockQuantity,Price,ImagePath from tblProducts_catalog where Comp_id='{Req.Comp_id}'";
                var result = await _databaseManager.ExecuteDataTableAsync(jsonqry);
                if (result.Rows.Count > 0)
                {
                    var response = new
                    {
                        ProductId = result.Rows[0]["ProductId"].ToString(),
                        ProductName = result.Rows[0]["ProductName"].ToString(),
                        ProductDescription = result.Rows[0]["ProductDescription"].ToString(),
                        Category = result.Rows[0]["Category"].ToString(),
                        StockQuantity = result.Rows[0]["StockQuantity"].ToString(),
                        Price = result.Rows[0]["Price"].ToString(),
                        ImagePath = result.Rows[0]["ImagePath"].ToString()
                    };
                    return Ok(new ApiResponse<object>(true, "Product catalog found sucessfully", response));
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "Brand settings not found"));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Error in Profiledetails API: " + ex.Message + " ,Track and Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while processing data: {ex.Message}"));
            }
        }
    }
    public class productcatalogrequest
    {
        public string Comp_id { get; set; }
    }
   
}

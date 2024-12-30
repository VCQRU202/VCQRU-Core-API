using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCatalogController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        private const string BaseUrl = "https://qa.vcqru.com"; // Store this in a config file for easy changes
       

        public ProductCatalogController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> GetProductCatalog([FromBody] ProductCatalogRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Comp_id))
            {
                return BadRequest(new ApiResponse<object>(false, "Invalid request: 'Comp_id' is required."));
            }

            try
            {
                  string ProductQuery = $"SELECT ProductId, ProductName, ProductDescription, Point, StockQuantity, Price, ImagePath FROM tblProducts_catalog WHERE Comp_id = '{request.Comp_id}' AND Isactive = 1 AND Isdelete = 0";
                 var result = await _databaseManager.ExecuteDataTableAsync(ProductQuery);

                if (result.Rows.Count > 0)
                {
                    var allInfos = JsonConvert.DeserializeObject<List<ProductCatalogData>>(JsonConvert.SerializeObject(result));

                    foreach (var info in allInfos)
                    {
                        if (!string.IsNullOrEmpty(info.ImagePath) && !info.ImagePath.StartsWith("http"))
                        {
                            info.ImagePath = $"{BaseUrl}{info.ImagePath.TrimStart('~').Replace("\\", "/")}";
                        }
                    }

                    return Ok(new ApiResponse<object>(true, "Product catalog found successfully", allInfos));
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "No products found for the provided company."));
                }
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs($"Error in GetProductCatalog API: {ex.Message} ,Track and Trace: {ex.StackTrace}");
                return StatusCode(500, new ApiResponse<object>(false, "An error occurred while processing the request."));
            }
        }
    }

    public class ProductCatalogRequest
    {
        public string Comp_id { get; set; }
    }

    public class ProductCatalogData
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string StockQuantity { get; set; }
        public string Price { get; set; }
        public string ImagePath { get; set; }
    }

   
}

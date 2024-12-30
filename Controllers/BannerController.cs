using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannerController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public BannerController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> Banner([FromBody] BannerClass req)
        {
            //M_Consumerid,Comp_ID
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));
            try
            {
                if (req.Comp_ID != "")
                {
                    DataTable dt = await _databaseManager.SelectTableDataAsync("Tbl_bannerVcqru", "imagePath", "compid='" + req.Comp_ID + "'");
                    
                    if (dt.Rows.Count > 0)
                    {
                        string baseUrl = "https://qa.vcqru.com/";

                        var rows = dt.AsEnumerable().Select(row =>
                        {
                            return dt.Columns.Cast<DataColumn>()
                                .ToDictionary(column => column.ColumnName, column =>
                                {
                                    // If the column is 'imagePath', concatenate the base URL
                                    return column.ColumnName == "imagePath" ? baseUrl + row[column].ToString() : row[column];
                                });
                        }).ToList();

                        return Ok(new ApiResponse<object>(true, "Successfully", rows));
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<object>(false, "No records found."));
                    }

                }
                else
                {
                    return BadRequest(new ApiResponse<object>(false, "Invalid login detail!"));
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while Banner: {ex.Message}"));
            }
        }
    }

    public class BannerClass
    {
        public string Comp_ID { get; set; }
    }


}

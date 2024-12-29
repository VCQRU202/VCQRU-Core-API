using CoreApi_BL_App.Models;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiCodeCheckHistoryController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<ApiCodeCheckHistoryController> _logger;

        public ApiCodeCheckHistoryController(DatabaseManager databaseManager, ILogger<ApiCodeCheckHistoryController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ApiCodeCheckHistory([FromBody] ApiCodeCheckHistoryClass req)
        {
            // Validate input parameters
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            if (string.IsNullOrEmpty(req.Comp_ID) || string.IsNullOrEmpty(req.M_Consumerid))
                return BadRequest(new ApiResponse<object>(false, "Invalid login detail!"));

            try
            {
                // Use a helper method to get data based on the limit
                var result = await GetTransactionDataAsync(req.M_Consumerid, req.Comp_ID, req.limit);

                if (result == null)
                    return BadRequest(new ApiResponse<object>(false, "Record not found!"));
                var rows = result.Rows.Cast<DataRow>().Select(row =>
           result.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => row[col])
       ).ToList();
                return Ok(new ApiResponse<object>(true, "Successfully retrieved transaction data.", rows));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while retrieving transaction data: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred: {ex.Message}"));
            }
        }

        private async Task<DataTable> GetTransactionDataAsync(string consumerId, string compId, string limit)
        {
            var inputParameters = new Dictionary<string, object>
    {
        { "@M_Consumerid", consumerId },  // Ensure this is included
        { "@distributorid", null },
        { "@Comp_id", compId }
    };

            DataTable dtTrans;

            // Call the appropriate stored procedure based on the 'limit' parameter
            if (limit == "Full")
            {

                dtTrans = await _databaseManager.ExecuteStoredProcedureDataTableAsync("Gettransactionlist", inputParameters);
            }
            else
            {
                dtTrans = await _databaseManager.ExecuteStoredProcedureDataTableAsync("Gettransactionlist10", inputParameters);
            }

            if (dtTrans.Rows.Count > 0)
            {
                return dtTrans;
            }

            return null;
        }

    }

    public class ApiCodeCheckHistoryClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }
        public string limit { get; set; }
    }


}

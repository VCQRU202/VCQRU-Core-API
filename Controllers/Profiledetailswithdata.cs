using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Profiledetailswithdata : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public Profiledetailswithdata(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateConsumer([FromBody] profiledetailsrequest Req)
        {
            if (Req == null)
                return BadRequest(new ApiResponse<object>(false, "Consumer data is null."));
            if (string.IsNullOrEmpty(Req.Mobileno) || string.IsNullOrEmpty(Req.Comp_id))
                return BadRequest(new ApiResponse<object>(false, "Mobile number or Company ID is missing."));
            if (Req.Mobileno.Length == 10)
                Req.Mobileno = "91" + Req.Mobileno;

            if (Req.Mobileno.Length != 12)
                return BadRequest(new ApiResponse<object>(false, "Invalid Mobile Number"));

            try
            {
                string jsondata = "";
                string jsonqry = $"Select profilesettings from BrandSettings where comp_id='{Req.Comp_id}'";
                var result = await _databaseManager.ExecuteDataTableAsync(jsonqry);

                if (result.Rows.Count > 0)
                {
                    string consqry = "exec USP_PROFILEDEATILS_BL @Mobileno, @Comp_id";
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Mobileno", Req.Mobileno },
                        { "@Comp_id", Req.Comp_id }
                    };

                    var consumerdata = await _databaseManager.ExecuteDataTableAsync(consqry, parameters);
                    jsondata = result.Rows[0]["profilesettings"].ToString();

                    // Deserialize profilesettings JSON into a list of FieldData objects
                    List<FieldData> fieldDataList = JsonConvert.DeserializeObject<List<FieldData>>(jsondata);

                    // Loop through each field and populate dynamic data
                    foreach (var item in fieldDataList)
                    {
                        // Dynamically match field name and assign corresponding data from the consumer data
                        if (consumerdata.Rows.Count > 0)
                        {
                            if (consumerdata.Columns.Contains(item.FieldName))
                            {
                                item.data = consumerdata.Rows[0][item.FieldName]?.ToString();
                            }
                            else
                            {
                                item.data = null; // Set data to null if the field doesn't exist in consumer data
                            }
                        }
                    }

                    // Return the populated field data as JSON
                    return Ok(new ApiResponse<object>(true, "Consumer data processed successfully", fieldDataList));
                }
                else
                {
                    return NotFound(new ApiResponse<object>(false, "Brand settings not found"));
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                _databaseManager.ExceptionLogs("Error in Profiledetails API: " + ex.Message + " ,Track and Trace: " + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while processing data: {ex.Message}"));
            }
        }
    }
}

public class FieldData
{
    public string LableName { get; set; }
    public string FieldName { get; set; }
    public string FieldType { get; set; }
    public string Hint { get; set; }
    public string Regex { get; set; }
    public bool IsMandatory { get; set; }
    public List<string> Values { get; set; }
    public bool IsActive { get; set; }
    public string data { get; set; }
}

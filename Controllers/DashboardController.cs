using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;

        public DashboardController(DatabaseManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        [HttpPost]
        public async Task<IActionResult> Dashboard([FromBody] DashboardClass req)
        {
            if (req == null)
                return BadRequest(new ApiResponse<object>(false, "Request data is null."));

            if (string.IsNullOrEmpty(req.Comp_ID))
                return BadRequest(new ApiResponse<object>(false, "Invalid company ID."));

            try
            {
                var mobileNo = await GetConsumerMobileNumber(req.M_Consumerid);
                if (string.IsNullOrEmpty(mobileNo))
                    return BadRequest(new ApiResponse<object>(false, "Record not available."));

                var userDetails = await GetUserDetails(mobileNo);
                if (userDetails == null)
                    return BadRequest(new ApiResponse<object>(false, "No user details found."));

                var query = await GetDashboardData(req.M_Consumerid, req.Comp_ID, mobileNo);
                return Ok(new ApiResponse<object>(true, "Successfully fetched dashboard data.", query));
            }
            catch (Exception ex)
            {
                _databaseManager.ExceptionLogs("Find Error in Dashboard API :" + ex.Message + "  ,Track and Trace :" + ex.StackTrace);
                return StatusCode(500, new ApiResponse<object>(false, $"Error occurred while processing the request: {ex.Message}"));
            }
        }

        private async Task<string> GetConsumerMobileNumber(string consumerId)
        {
            string query = "SELECT TOP 1 MobileNo FROM m_consumer WHERE M_Consumerid = @M_Consumerid ORDER BY M_Consumerid DESC";
            var parameters = new Dictionary<string, object> { { "@M_Consumerid", consumerId } };
            var dt = await _databaseManager.ExecuteDataTableAsync(query, parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0]["MobileNo"].ToString().Substring(dt.Rows[0]["MobileNo"].ToString().Length - 10) : null;
        }

        private async Task<DataRow> GetUserDetails(string mobileNo)
        {
            
            var parameters = new Dictionary<string, object> { { "@User_ID", mobileNo } };
            var dt = await _databaseManager.ExecuteStoredProcedureDataTableAsync("PROC_appGetUserDetails", parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        private async Task<Query_responses> GetDashboardData(string consumerId, string compId, string mobileNo)
        
        {
            var query = new Query_responses();
            var claimAmount = await GetClaimAmount(mobileNo, compId);
            var dashboardData = await GetDashboardStats(consumerId, compId);

            query.totalCode = dashboardData.TotalCode;
            query.successCode = dashboardData.SuccessCode;
            query.reedemPoint = (dashboardData.ReedemPoints + claimAmount).ToString();
            query.transferredCash = dashboardData.TransferredCash ?? "0";
            query.totalPoint = dashboardData.TotalPoints.ToString();
            query.totalcounterfeit = dashboardData.TotalCounterfeit.ToString();
            query.successcounterfeit = dashboardData.SuccessCounterfeit.ToString();

            return query;
        }

        private async Task<int> GetClaimAmount(string mobileNo, string compId)
        {
            string query = @"
                SELECT COALESCE(SUM(cl.Amount), 0) AS Amount
                FROM ClaimDetails cl
                INNER JOIN M_consumer mc ON RIGHT(mc.MobileNo, 10) = RIGHT(cl.Mobileno, 10)
                WHERE right(mc.MobileNo,10) = @MobileNo AND mc.IsDelete = 0 AND cl.Isapproved = 1 AND cl.Comp_id = @CompID";
            var parameters = new Dictionary<string, object> { { "@MobileNo", mobileNo }, { "@CompID", compId } };
            var dt = await _databaseManager.ExecuteDataTableAsync(query, parameters);
            return dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0]["Amount"]) : 0;
        }

        private async Task<DashboardStats> GetDashboardStats(string consumerId, string compId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@M_Consumerid", Convert.ToInt32(consumerId) },
                { "@compid", compId }
            };
            var dtTrans = await _databaseManager.ExecuteStoredProcedureDataSetAsync("[USP_dashboarddata_BL]", parameters);

            var totalPoints = await GetTotalPoints(consumerId, compId);

            var totalCash = dtTrans.Tables[0].Rows.Count > 3 ? dtTrans.Tables[0].Rows[3][0].ToString() : "0";

            var counterfeitStats = await GetCounterfeitStats(consumerId, compId);

            return new DashboardStats
            {
                TotalCode = dtTrans.Tables[0].Rows[0][0].ToString(),
                SuccessCode = dtTrans.Tables[0].Rows[2][0].ToString(),
                ReedemPoints = Convert.ToInt32(dtTrans.Tables[0].Rows[1][0]),
                TotalPoints = totalPoints,
                TransferredCash = totalCash,
                TotalCounterfeit = counterfeitStats.TotalCounterfeit,
                SuccessCounterfeit = counterfeitStats.SuccessCounterfeit
            };
        }

        private async Task<decimal> GetTotalPoints(string consumerId, string compId)
        {
            string query = @"
                SELECT COALESCE(SUM(CAST(bp.Points AS INT)), 0) AS TotalPoints
                FROM BLoyaltyPointsEarned bp
                INNER JOIN M_ServiceSubscriptionTrans mss ON mss.SST_Id = bp.SST_id
                INNER JOIN M_ServiceSubscription ms ON ms.Subscribe_Id = mss.Subscribe_Id
                WHERE bp.M_Consumerid = @ConsumerId AND ms.Comp_ID = @CompID";
            var parameters = new Dictionary<string, object> { { "@ConsumerId", consumerId }, { "@CompID", compId } };
            var dt = await _databaseManager.ExecuteDataTableAsync(query, parameters);
            return dt.Rows.Count > 0 ? Convert.ToDecimal(dt.Rows[0]["TotalPoints"]) : 0;
        }

        private async Task<CounterfeitStats> GetCounterfeitStats(string consumerId,string compId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "@M_Consumerid", Convert.ToInt32(consumerId) },
                { "@Comp_id", compId }
            };
            var dtTranscounter = await _databaseManager.ExecuteStoredProcedureDataSetAsync("[USP_Counterfeitcodecount]", parameters);
            return new CounterfeitStats
            {
                TotalCounterfeit = dtTranscounter.Tables[0].Rows[0][0].ToString(),
                SuccessCounterfeit = dtTranscounter.Tables[0].Rows[1][0].ToString()
            };
        }
    }

    public class DashboardClass
    {
        public string M_Consumerid { get; set; }
        public string Comp_ID { get; set; }
    }

    public class Query_responses
    {
        public string totalCode { get; set; }
        public string successCode { get; set; }
        public string reedemPoint { get; set; }
        public string transferredCash { get; set; }
        public string totalPoint { get; set; }
        public string totalcounterfeit { get; set; }
        public string successcounterfeit { get; set; }
    }

    public class DashboardStats
    {
        public string TotalCode { get; set; }
        public string SuccessCode { get; set; }
        public int ReedemPoints { get; set; }
        public decimal TotalPoints { get; set; }
        public string TransferredCash { get; set; }
        public string TotalCounterfeit { get; set; }
        public string SuccessCounterfeit { get; set; }
    }

    public class CounterfeitStats
    {
        public string TotalCounterfeit { get; set; }
        public string SuccessCounterfeit { get; set; }
    }
}

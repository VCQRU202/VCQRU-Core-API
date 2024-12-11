
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Data.SqlClient;
using CoreApi_BL_App.Models;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class productRegstration : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public productRegstration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #region method to genrate compney id and check the compneyID present in the databse or not
        private string GenerateProductId()
        {
            var random = new Random();
            var length = 4;
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var productId = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                productId.Append(chars[random.Next(chars.Length)]);
            }
            return productId.ToString();
        }

        // methos to check productid is present in the table or not 

        private async Task<bool> ProductIdExistsAsync(string productId)
        {
            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT COUNT(1) FROM Pro_Reg WHERE Pro_ID = @Pro_ID";

                    using(SqlCommand cmd = new SqlCommand(query,connection))
                    {
                        cmd.Parameters.AddWithValue("@Pro_ID", productId);
                        var count = (int)await cmd.ExecuteScalarAsync();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
        #endregion

        #region post method for the product reg
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductRegrastration product)
        {
            if (product == null)
            {
                return BadRequest("invalid product");
            }

            string productID = GenerateProductId();
            while (await ProductIdExistsAsync(productID))
            {
                productID = GenerateProductId();
            }
            product.Pro_ID=productID;

            //logic to insert the product in the databse

            using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("defaultConnectionbeta")))
            {
                try
                {
                    await connection.OpenAsync();

                    string query = "INSERT INTO Pro_Reg (Pro_ID, Pro_Name, Pro_Desc, Pro_Doc, Pro_Entry_Date) VALUES (@Pro_ID, @Pro_Name, @Pro_Desc, @Pro_Doc, @Pro_Entry_Date)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {                  
                        cmd.Parameters.AddWithValue("@Pro_ID", product.Pro_ID);
                        cmd.Parameters.AddWithValue("@Pro_Name", product.Pro_ID);
                        cmd.Parameters.AddWithValue("@Pro_Desc", product.Pro_ID);
                        cmd.Parameters.AddWithValue("@Pro_Doc", product.Pro_ID);
                        cmd.Parameters.AddWithValue("@Pro_Entry_Date", product.Pro_Entry_Date ?? DateTime.Now);

                        var result = await cmd.ExecuteNonQueryAsync();
                        if(result>0)
                        {
                            return Ok("Product added successfully.");
                        }
                        else
                        {
                            return StatusCode(500, "Internal server error while adding product.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

          }
        #endregion

    }
}

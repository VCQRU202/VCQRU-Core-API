using CoreApi_BL_App.Models;
using Microsoft.Data.SqlClient;
using RestSharp;
using System.Data;


namespace CoreApi_BL_App.Services
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Generic method to execute a query and return DataTable
        public async Task<DataTable> ExecuteDataTableAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        // Generic method to execute a query and return DataSet
        public async Task<DataSet> ExecuteDataSetAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        // Generic method to execute a non-query (INSERT, UPDATE, DELETE)
        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    return await command.ExecuteNonQueryAsync();
                }
            }
        }

        // Execute a stored procedure and return DataTable
        public async Task<DataTable> ExecuteStoredProcedureDataTableAsync(string storedProcedureName, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
        }

        // Execute a stored procedure and return DataSet
        public async Task<DataSet> ExecuteStoredProcedureDataSetAsync(string storedProcedureName, Dictionary<string, object> parameters = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                        }
                    }

                    using (var dataAdapter = new SqlDataAdapter(command))
                    {
                        var dataSet = new DataSet();
                        dataAdapter.Fill(dataSet);
                        return dataSet;
                    }
                }
            }
        }

        public Dictionary<string, object> ExecuteStoredProcedure(string storedProcedureName, Dictionary<string, object> inputParameters, List<string> outputParameters)
        {
            Dictionary<string, object> outputValues = new Dictionary<string, object>();

            // SQL connection and command setup
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    foreach (var param in inputParameters)
                    {
                        // Ensure null values are handled correctly
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    // Add output parameters
                    foreach (var outputParam in outputParameters)
                    {
                        SqlParameter outParam = new SqlParameter(outputParam, SqlDbType.NVarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);
                    }

                    try
                    {
                        // Open the connection asynchronously (if appropriate for your app)
                        conn.Open();

                        // Execute the command (could be ExecuteNonQuery, ExecuteScalar, etc., based on your needs)
                        cmd.ExecuteNonQuery();

                        // Retrieve the output parameter values
                        foreach (var outputParam in outputParameters)
                        {
                            outputValues[outputParam] = cmd.Parameters[outputParam].Value;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        // Handle SQL-specific errors (e.g., connection issues, query issues)
                        // Log the error or add custom handling as needed
                        throw new ApplicationException("An SQL error occurred while executing the stored procedure.", sqlEx);
                    }
                    catch (TimeoutException timeoutEx)
                    {
                        // Handle timeout errors (e.g., SQL Server timeout)
                        // Optionally add retry logic if necessary
                        throw new ApplicationException("The operation timed out while executing the stored procedure.", timeoutEx);
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unexpected exceptions
                        // You could log the error or rethrow as needed
                        throw new ApplicationException("An unexpected error occurred while executing the stored procedure.", ex);
                    }
                    finally
                    {
                        // Ensure the connection is properly closed (even if an exception occurred)
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return outputValues;
        }

        public Dictionary<string, object> ExecuteStoredProcedureWithParameters(
     string storedProcedureName,
     Dictionary<string, object> inputParameters,
     List<string> outputParameters)
        {
            Dictionary<string, object> outputValues = new Dictionary<string, object>();

            // SQL connection and command setup
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add input parameters
                    foreach (var param in inputParameters)
                    {
                        // Ensure null values are handled correctly
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    // Add output parameters
                    foreach (var outputParam in outputParameters)
                    {
                        SqlParameter outParam = new SqlParameter(outputParam, SqlDbType.NVarChar, 50)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);
                    }

                    try
                    {
                        // Open the connection asynchronously (if appropriate for your app)
                        conn.Open();

                        // Execute the command (could be ExecuteNonQuery, ExecuteScalar, etc., based on your needs)
                        cmd.ExecuteNonQuery();

                        // Retrieve the output parameter values
                        foreach (var outputParam in outputParameters)
                        {
                            outputValues[outputParam] = cmd.Parameters[outputParam].Value;
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        // Handle SQL-specific errors (e.g., connection issues, query issues)
                        // Log the error or add custom handling as needed
                        throw new ApplicationException("An SQL error occurred while executing the stored procedure.", sqlEx);
                    }
                    catch (TimeoutException timeoutEx)
                    {
                        // Handle timeout errors (e.g., SQL Server timeout)
                        // Optionally add retry logic if necessary
                        throw new ApplicationException("The operation timed out while executing the stored procedure.", timeoutEx);
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unexpected exceptions
                        // You could log the error or rethrow as needed
                        throw new ApplicationException("An unexpected error occurred while executing the stored procedure.", ex);
                    }
                    finally
                    {
                        // Ensure the connection is properly closed (even if an exception occurred)
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                    }
                }
            }

            return outputValues;
        }


        public async Task<DataTable> SelectTableDataAsync(string tableName, string selectClause, string whereClause)
        {
            DataTable dataTable = new DataTable();

            try
            {
                // Validate input for potential SQL injection risks
                if (HasSpecialChar(selectClause) || HasSpecialChar(tableName) || HasSpecialChar(whereClause))
                    throw new ArgumentException("Invalid characters in SQL parameters.");

                using (var sqlConnection = new SqlConnection(_connectionString))
                {
                    string query = $"SELECT {selectClause} FROM {tableName}";

                    if (!string.IsNullOrWhiteSpace(whereClause))
                        query += $" WHERE {whereClause}";

                    using (var selectCommand = new SqlCommand(query, sqlConnection))
                    {
                        selectCommand.CommandType = CommandType.Text;

                        await sqlConnection.OpenAsync();
                        using (var sqlDataAdapter = new SqlDataAdapter(selectCommand))
                        {
                            sqlDataAdapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error (Assume a logger is configured, replace `Console.WriteLine` with your logging mechanism)
                Console.WriteLine($"Error in SelectTableDataAsync: {ex.Message}");
            }

            return dataTable;
        }

        private bool HasSpecialChar(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            // Check for special characters in the input
            //foreach (char c in input)
            //{
            //    //if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ')
            //        if (!char.IsLetterOrDigit(c) && c != '_')
            //            return true;
            //}

            return false;
        }

        public async Task<int> InsertAsync(string columnName, string columnValue, string tableName)
        {
            if (HasSpecialChar(columnName) || HasSpecialChar(columnValue) || HasSpecialChar(tableName))
            {
                throw new ArgumentException("Invalid characters in input parameters.");
            }

            try
            {
                string query = $"INSERT INTO {tableName} ({columnName}) VALUES (@ColumnValue)";

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    // Add parameterized value
                    command.Parameters.AddWithValue("@ColumnValue", columnValue);

                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error (replace with ILogger or your logging mechanism)
                Console.WriteLine($"Error in InsertAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> UpdateAsync(string columnNameWithValue, string condition, string tableName)
        {
            if (HasSpecialChar(columnNameWithValue) || HasSpecialChar(condition) || HasSpecialChar(tableName))
            {
                throw new ArgumentException("Invalid characters in input parameters.");
            }

            try
            {
                // Construct the query
                string query = $"UPDATE {tableName} SET {columnNameWithValue}";
                if (!string.IsNullOrEmpty(condition))
                {
                    query += $" WHERE {condition}";
                }

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    // Open the connection and execute the query
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error (replace with ILogger or your logging mechanism)
                Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
                return 0;
            }
        }

        public async Task<int> DeleteAsync(string condition, string tableName)
        {
            if (HasSpecialChar(condition) || HasSpecialChar(tableName))
            {
                throw new ArgumentException("Invalid characters in input parameters.");
            }

            try
            {
                // Build query
                string query = $"DELETE FROM {tableName}";
                if (!string.IsNullOrEmpty(condition))
                {
                    query += $" WHERE {condition}";
                }

                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(query, connection))
                {
                    // Open the connection and execute the query
                    await connection.OpenAsync();
                    return await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error (use ILogger or a logging framework in a real-world application)
                Console.WriteLine($"Error in DeleteAsync: {ex.Message}");
                return 0;
            }
        }

        public string sendotpAadhar(string AadharNo, string URL, string baseUrl, string appId, string apiKey)
        {
            var options = new RestClientOptions(baseUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest(URL, Method.Post);
            request.AddHeader("app-id", appId);
            request.AddHeader("api-key", apiKey);
            request.AddHeader("Content-Type", "application/json");
            var body = new
            {
                mode = "sync",
                data = new
                {
                    customer_aadhaar_number = AadharNo,
                    consent = "Y",
                    consent_text = "Approve_the_values_here"
                }
            };
            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            try
            {
                RestResponse response = client.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {ex.Message}");
                throw;
            }
        }

        public string validateotpAadhar(string Request_Id,string otp, string URL, string baseUrl, string appId, string apiKey)
        {
            var options = new RestClientOptions(baseUrl)
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest(URL, Method.Post);
            request.AddHeader("app-id", appId);
            request.AddHeader("api-key", apiKey);
            request.AddHeader("Content-Type", "application/json");
            var body = new
            {
                mode = "sync",
                data = new
                {
                    request_id = Request_Id,
                    otp = otp,
                    consent = "Y",
                    consent_text = "here_i_declare_all_info_is_correct"
                }
            };
            var jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
            try
            {
                RestResponse response = client.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {ex.Message}");
                throw;
            }
        }

    }
}

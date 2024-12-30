using CoreApi_BL_App.Models;
using CoreApi_BL_App.Models.Vendor;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Data;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Data.SqlClient;
using RestSharp;
using System.Data;
namespace CoreApi_BL_App.Services
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        private static readonly object lockObject = new object();


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

        public void ExecuteStoredProcedureNONQUERY(string storedProcedureName, Dictionary<string, object> inputParameters)
        {
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

                    try
                    {
                        // Open the connection
                        conn.Open();

                        // Execute the stored procedure (ExecuteNonQuery for commands that don't return a result set)
                        cmd.ExecuteNonQuery();
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
                        throw new ApplicationException("The operation timed out while executing the stored procedure.", timeoutEx);
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unexpected exceptions
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
        }

        public string VerifyPan(string pancard, string panHolderName, string URL, string baseUrl, string appId, string apiKey)
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
                    customer_pan_number = pancard,
                    consent = "Y",
                    pan_holder_name = panHolderName.ToUpper(),
                    consent_text = "I_consent_to_this_information_being_shared_with_zoop.one"
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

        public string VerifyUPIKYC(string UPIID, string URL, string baseUrl, string appId, string apiKey)
        {
            string Orderid = Guid.NewGuid().ToString().Replace("-","");
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
                    customer_upi_id = UPIID,
                    consent = "Y",
                    consent_text = "I hear by declare my consent agreement for fetching my information via ZOOP API."
                },
                task_id = Orderid
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
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
        public string AccountNumber(string AccountNumber, string Ifsc, string URL, string baseUrl, string appId, string apiKey)
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
                    account_number = AccountNumber,
                    ifsc = Ifsc,
                    consent = "Y",
                    consent_text = "I_consent_to_this_information_being_shared_with_zoop.one"
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


        public async Task<object> ExecuteStoredProcedureScalarAsync(string storedProcedureName, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters to the command
                    foreach (var param in parameters)
                    {
                        // Ensure correct handling of DBNull for null values
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    try
                    {
                        // Execute the stored procedure and return the scalar result
                        return await command.ExecuteScalarAsync();
                    }
                    catch (SqlException sqlEx)
                    {
                        // Log SQL exception (sqlEx.Message) depending on your logging framework
                        throw new Exception("An error occurred while executing the stored procedure.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        // Log general exception (ex.Message) depending on your logging framework
                        throw new Exception("An unexpected error occurred.", ex);
                    }
                }
            }
        }


        public int InsertScalar(string columnName, string columnValue, string tableName)
        {
            int num = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(columnValue) || string.IsNullOrWhiteSpace(tableName))
                {
                    throw new ArgumentException("Invalid input parameters.");
                }
                string query = $"INSERT INTO {tableName} ({columnName}) VALUES (@ColumnValue); SELECT SCOPE_IDENTITY();";
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ColumnValue", columnValue);
                        connection.Open();
                        num = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                num = 0;
            }

            return num;
        }

        public int Insertion(string columnName, string columnValue, string tableName)
        {
            int num = 0;
            try
            {
                if (string.IsNullOrWhiteSpace(columnName) || string.IsNullOrWhiteSpace(columnValue) || string.IsNullOrWhiteSpace(tableName))
                {
                    throw new ArgumentException("Invalid input parameters.");
                }
                string query = $"INSERT INTO {tableName} ({columnName}) VALUES (@ColumnValue);";
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ColumnValue", columnValue);
                        connection.Open();
                        num = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                num = 0;
            }
            return num;
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


        public string validateotpAadhar(string Request_Id, string otp, string URL, string baseUrl, string appId, string apiKey)

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


        public string SendOTPLogin(string sPhoneNo, string sMessage, string msg_type, string compname = "", string smsURL = "")
        {


            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // ServicePointManager.ServerCertificateValidationCallback = (snder, cert, chain, error) => true;

            WebRequest request = WebRequest.Create(smsURL);
            request.Method = "POST";
            string postData = "";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("api-key", "A8630797ed2577e3a9166d386937db77f");
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();

            try
            {
                // RestResponse response = client.Execute(responseFromServer);
                return responseFromServer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {ex.Message}");
                throw;
            }
        }
        public void ExceptionLogs(string txt)
        {
            try
            {
                string basePath = Path.Combine(Directory.GetCurrentDirectory(), "LogManager");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                string logFilePath = Path.Combine(basePath, $"LogManager_{currentDate}.txt");
                string logMessage = $"{DateTime.Now:yyyy/MM/dd hh:mm:ss tt} : {txt}\r\n";
                lock (lockObject)
                {
                    File.AppendAllText(logFilePath, logMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }

        public async Task<int> InsertProductInquiryAsync(Object9420 Reg)
        {
            var inputParameters = new Dictionary<string, object>
            {
                { "@Dial_Mode", Reg.Dial_Mode },
                { "@Enq_Date", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") },
                { "@Mode_Detail", Reg.Mode_Detail },
                { "@MobileNo", Reg.Mobile_No },
                { "@Received_Code1", Reg.Received_Code1 },
                { "@Received_Code2", Reg.Received_Code2 },
                { "@Is_Success", Reg.Is_Success.ToString() },
                { "@IsDraw", "0" },
                { "@SST_ID", "0" },
                { "@City", Reg.City },
                { "@callercircle", Reg.callercircle },
                { "@Retailer_Name", Reg.Retailer_Name },
                { "@Image", Reg.Image },
                { "@network", Reg.network },
                { "@callerdate", DateTime.Today.ToString("yyyy/MM/dd") },
                { "@callertime", Reg.callertime },
                { "@M_Codeid", Reg.M_code_Row_ID },
                { "@Proid", Reg.Pro_ID },
                { "@Compid", Reg.Comp_ID },
                { "@dealer_mobile", Reg.dealer_mobile },
                { "@dealerid", Reg.dealerid },
                { "@Latitude", Reg.Lat },
                { "@Longitude", Reg.Long }
            };

            try
            {
                var i = await ExecuteStoredProcedureScalarAsync("PROC_InsertProductInquery", inputParameters);
                if (i != null)
                    return Convert.ToInt32(i);

                // Return a failure code if the result is null
                return -1;
            }
            catch (Exception ex)
            {
                // Log the error (ex.Message or ex.ToString()) depending on your logging framework
                return -1; // Indicates failure
            }
        }
        public async Task ExecuteStoredProcedureNonQueryAsync(string storedProcedureName, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters to the command
                    foreach (var param in parameters)
                    {
                        // Ensure correct handling of DBNull for null values
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    try
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                    catch (SqlException sqlEx)
                    {
                        // Log SQL exception (sqlEx.Message) depending on your logging framework
                        throw new Exception("An error occurred while executing the stored procedure.", sqlEx);
                    }
                    catch (Exception ex)
                    {
                        // Log general exception (ex.Message) depending on your logging framework
                        throw new Exception("An unexpected error occurred.", ex);
                    }
                }
            }
        }

        public async Task<(string Message, object ReturnData)> ServiceRequestCheck(Object9420 Reg, DataTable dsres, bool IsCheckedUse_Count)
        {
            string DefaultComments = ""; string CompName = string.Empty; string result = string.Empty; string UserType = string.Empty;
            DataTable dtServiceAssign = new DataTable();
            DataTable dtTotalCodesCount = new DataTable();
            string strSeperator = "<br/><br/>";

            string message = "";
            object returnData = null;
            try
            {
                Dictionary<string, object> inputParameters = new Dictionary<string, object>
                {
                        { "@Code1", Reg.Received_Code1 },
                        { "@Code2", Reg.Received_Code2 }

                };
                DataSet dsServicesAssign = await ExecuteStoredProcedureDataSetAsync("Proc_GetServicesAssignAgainstProduct", inputParameters);

                if (!IsCheckedUse_Count)
                {
                    Reg.Is_Success = 1;
                    if (dsServicesAssign.Tables.Count <= 2)
                    {
                        dtServiceAssign = dsServicesAssign.Tables[0];
                        dtTotalCodesCount = dsServicesAssign.Tables[1];
                        if (dtServiceAssign.Rows.Count > 0)
                        {
                            Reg.Comp_Name = dsres.Rows[0]["Comp_Name"].ToString();
                            Reg.Comp_ID = dtServiceAssign.Rows[0]["Comp_ID"].ToString();
                            DataRow[] drw = dtServiceAssign.Select("Service_id = 'SRV1023'");
                            strSeperator = "*";
                            string strReturnMSG = string.Empty;
                            #region Entry in M_consumer_M_code
                            string json = string.Empty;
                            long intM_Consumer_MCOde;
                            json = JsonConvert.SerializeObject(Reg, Newtonsoft.Json.Formatting.Indented);
                            intM_Consumer_MCOde = await InsertProductInquiryAsync(Reg);
                            Dictionary<string, object> inputParametersupdatecode = new Dictionary<string, object>
                                {
                                    { "@Received_Code1", Reg.Code1 },
                                    { "@Received_Code2", Reg.Code2 },
                                    { "@Is_Success", Reg.Is_Success }
                                };
                            await ExecuteStoredProcedureNonQueryAsync("PROC_UpdateM_CodeUse_Count", inputParametersupdatecode);
                            Reg.intM_Consumer_MCOde = intM_Consumer_MCOde;
                            #endregion


                            for (int i = 0; i < dtServiceAssign.Rows.Count; i++)
                            {
                                if (i > 0)
                                    result = result + strSeperator;
                                string stServiceid = dtServiceAssign.Rows[i]["Service_ID"].ToString();
                                DataRow[] dr = dtServiceAssign.Select("Service_id = '" + stServiceid + "'");
                                switch (stServiceid)
                                {
                                    case "SRV1023":  // E-Warranty
                                        {

                                        }
                                        break;
                                    case "SRV1001":  // Buildloyality
                                        {
                                            Dictionary<string, object> Insertcodeforpoint = new Dictionary<string, object>
                                            {
                                                { "@code1", Reg.Code1 },
                                                { "@code2", Reg.Code2 },
                                                { "@SST_Id", Convert.ToInt32(dr[0]["SST_Id"]) },
                                                { "@intM_Consumer_MCOde", intM_Consumer_MCOde }
                                            };
                                            var dt = await ExecuteStoredProcedureDataTableAsync("Proc_SaveCodeDtsForBuiltLoyalty", Insertcodeforpoint);
                                            if (dt.Rows.Count > 0)
                                            {
                                                Reg.M_ConsumerID = Convert.ToInt32(dt.Rows[0]["M_consumerid"]);
                                                string strM_consumerid = dt.Rows[0]["M_consumerid"].ToString();
                                                string strEarnedPoints = dt.Rows[0]["Points"].ToString();
                                                string IsCashConvert = dt.Rows[0]["IsCashConvert"].ToString();
                                                string AwardNameBL = dt.Rows[0]["AwardNameBL"].ToString();
                                                returnData = new { points = strEarnedPoints, cash = "", Status = "Success", CodeType = "Build Loyality", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                                                string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='SRV1001'";
                                                var msg = await ExecuteDataTableAsync(SMStemplate);
                                                if (msg.Rows.Count > 0)
                                                {
                                                    strReturnMSG = msg.Rows[0][0].ToString();

                                                }
                                                else
                                                {
                                                    strReturnMSG = $"Congratulation you have earn {strEarnedPoints} points against Coupon code {Reg.Code1}{Reg.Code2} from {Reg.Comp_Name}";
                                                }
                                                result = result + (strReturnMSG.Replace("<points>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName).Replace("<br>", "\n").Replace("<BR>", "\n").Replace("</BR>", "\n");
                                            }
                                            else
                                            {
                                                result = "Reached Frequency limit cross";
                                                //Need to handle Reached Frequency
                                            }

                                        }
                                        break;

                                    case "SRV1018":  // AntiCounterfiet
                                        {
                                            string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='SRV1018'";
                                            var msg = await ExecuteDataTableAsync(SMStemplate);
                                            if (msg.Rows.Count > 0)
                                            {
                                                strReturnMSG = msg.Rows[0][0].ToString();

                                            }
                                            else
                                            {
                                                strReturnMSG = $"Congratulation you have purchase an authentic product against Coupon code {Reg.Code1}{Reg.Code2} from {Reg.Comp_Name}";
                                            }
                                            result = result + strReturnMSG.ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName).Replace("<br>", "\n").Replace("<BR>", "\n").Replace("</BR>", "\n");
                                            returnData = new { points = "", cash = "", Status = "Success", CodeType = "Anti Counterfiet", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                                        }
                                        break;
                                    case "SRV1005":  // Cash Transfer
                                        {
                                            Dictionary<string, object> Insertcodeforpoint = new Dictionary<string, object>
                                            {
                                                { "@code1", Reg.Code1 },
                                                { "@code2", Reg.Code2 },
                                                { "@SST_Id", Convert.ToInt32(dr[0]["SST_Id"]) },
                                                { "@intM_Consumer_MCOde", intM_Consumer_MCOde }
                                            };
                                            var dt = await ExecuteStoredProcedureDataTableAsync("Proc_SaveCodeDtsForBuiltLoyalty", Insertcodeforpoint);
                                            if (dt.Rows.Count > 0)
                                            {
                                                Reg.M_ConsumerID = Convert.ToInt32(dt.Rows[0]["M_consumerid"]);
                                                string strEarnedPoints = dt.Rows[0]["Points"].ToString();
                                                string dIscash = "";
                                                string Iscash = dt.Rows[0]["Iscash"].ToString();
                                                string proName = dsres.Rows[0]["Pro_Name"].ToString();
                                                returnData = new { points = "", cash = Iscash, Status = "Success", CodeType = "Cash Transfer", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                                                string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='SRV1005'";
                                                var msg = await ExecuteDataTableAsync(SMStemplate);
                                                if (msg.Rows.Count > 0)
                                                {
                                                    strReturnMSG = msg.Rows[0][0].ToString();
                                                }
                                                else
                                                {
                                                    strReturnMSG = $"Congratulation you have earn {strEarnedPoints} Cash against Coupon code {Reg.Code1}{Reg.Code2} from {Reg.Comp_Name}";
                                                }
                                                result = result + (strReturnMSG.Replace("<cash>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName).Replace("<br>", "\n").Replace("<BR>", "\n").Replace("</BR>", "\n");
                                            }
                                            else
                                            {
                                                result = "Reached Frequency limit cross";
                                                //Need to handle Reached Frequency
                                            }
                                        }
                                        break;
                                    case "SRV1028":  // Instant PAyout
                                        {
                                            #region Instant Payout
                                            string selectbank = $"select top 1 b.Bank_Name,b.Account_No,b.IFSC_Code,b.Account_HolderNm,b.Branch from M_BankAccount b inner join M_Consumer m on m.M_Consumerid=b.M_Consumerid where m.MobileNo='{Reg.Mobile_No}' and m.IsDelete=0";
                                            var Bnkdata = await ExecuteDataTableAsync(selectbank);
                                            string AccountNumber = "xxxxxxxxx01234";
                                            string IFSCCode = "NA";
                                            string Bank_Name = "NA";
                                            string Account_HolderNm = "NA";
                                            string Branch = "NA";

                                            if (Bnkdata.Rows.Count > 0)
                                            {
                                                AccountNumber = Bnkdata.Rows[0]["Account_No"].ToString();
                                                IFSCCode = Bnkdata.Rows[0]["IFSC_Code"].ToString();
                                                Account_HolderNm = Bnkdata.Rows[0]["Account_HolderNm"].ToString();
                                                Branch = Bnkdata.Rows[0]["Branch"].ToString();
                                                Bank_Name = Bnkdata.Rows[0]["Bank_Name"].ToString();
                                            }

                                            Dictionary<string, object> Insertcodeforpoint = new Dictionary<string, object>
                                            {
                                                { "@code1", Reg.Code1 },
                                                { "@code2", Reg.Code2 },
                                                { "@SST_Id", Convert.ToInt32(dr[0]["SST_Id"]) },
                                                { "@intM_Consumer_MCOde", intM_Consumer_MCOde }
                                            };
                                            var dt = await ExecuteStoredProcedureDataTableAsync("Proc_SaveCodeDtsForBuiltLoyalty", Insertcodeforpoint);
                                            if (dt.Rows.Count > 0)
                                            {
                                                Reg.M_ConsumerID = Convert.ToInt32(dt.Rows[0]["M_consumerid"]);
                                                string strEarnedPoints = dt.Rows[0]["Points"].ToString();
                                                string dIscash = "";
                                                string Iscash = dt.Rows[0]["Iscash"].ToString();
                                                string proName = dsres.Rows[0]["Pro_Name"].ToString();
                                                returnData = new { points = "", cash = strEarnedPoints, Status = "Success", CodeType = "Instant Payout", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };

                                                if (dt.Rows[0]["ReachedFrequency"].ToString() == "0")
                                                {
                                                    string dtto = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                                    string M_Consumerid = dt.Rows[0]["M_Consumerid"].ToString();
                                                    string bankId = "ACC" + M_Consumerid;
                                                    string AcData = "NA";
                                                    string PaymentData = "";
                                                    string qryOnlDs = $"select top 1 ReqCount from tblKycBankDataDetails where AccountNo='{AccountNumber}' and IFSC_Code='{IFSCCode}' and ResponseCode=100 and Status=1 and IsBankAccountVerify=1";
                                                    var OnlDs = await ExecuteDataTableAsync(selectbank);
                                                    string qryBbkdt = $"select top 1 b.AccountNo,b.IFSC_Code,b.AccountHolderName from tblKycBankDataDetails b inner join M_Consumer m on m.M_Consumerid=b.M_Consumerid where m.MobileNo = '{Reg.Mobile_No}' and m.IsDelete=0 and b.ResponseCode=100 and b.Status=1 and b.Status=1 and AccountNo is not null and IFSC_Code is not null and AccountHolderName is not null order by b.Id desc";
                                                    var Bbkdt = await ExecuteDataTableAsync(qryBbkdt);
                                                    #region EKYC Applicable Status
                                                    int IsKYCApplicable = 0;
                                                    string qrydtekyc = $"select Is_Applicable from tbl_EKYC_Applicable where Comp_id='{Reg.Comp_ID}'";
                                                    var dtekyc = await ExecuteDataTableAsync(qrydtekyc);
                                                    if (dtekyc.Rows.Count > 0)
                                                    {
                                                        IsKYCApplicable = Convert.ToInt32(dtekyc.Rows[0]["Is_Applicable"]);
                                                    }
                                                    #endregion
                                                    if (OnlDs.Rows.Count > 0)
                                                    {
                                                        int ReqCount = Convert.ToInt32(OnlDs.Rows[0]["ReqCount"].ToString());
                                                        if (ReqCount >= 1 && Bbkdt.Rows.Count > 0)
                                                        {
                                                            string DbAccount_HolderNm = Bbkdt.Rows[0]["AccountHolderName"].ToString();
                                                            PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, DbAccount_HolderNm, IFSCCode);
                                                        }
                                                        else
                                                        {
                                                            if (IsKYCApplicable == 1)
                                                            {
                                                                AcData = OnlineAccountVerification(AccountNumber, IFSCCode, M_Consumerid);
                                                                if (AcData == "Success")
                                                                {
                                                                    PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                                }
                                                                else
                                                                {
                                                                    string ResponseStatus = "Invalid Bank Details";
                                                                    string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                    int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (!string.IsNullOrEmpty(AccountNumber) && !string.IsNullOrEmpty(IFSCCode))
                                                                {
                                                                    PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                                }
                                                                else
                                                                {
                                                                    string ResponseStatus = "Invalid Bank Details";
                                                                    string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                    int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (Bbkdt.Rows.Count == 0 && AcData != "Success")
                                                    {
                                                        if (Bnkdata.Rows.Count > 0 && CompName == "SAGAR PETROLEUMS PRIVATE LIMITED")
                                                        {
                                                            if (IsKYCApplicable == 1)
                                                            {
                                                                AcData = OnlineAccountVerification(AccountNumber, IFSCCode, M_Consumerid);
                                                                if (AcData == "Success")
                                                                {
                                                                    PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                                }
                                                                else
                                                                {
                                                                    string ResponseStatus = "Invalid Bank Details";
                                                                    string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                    int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                if (!string.IsNullOrEmpty(AccountNumber) && !string.IsNullOrEmpty(IFSCCode))
                                                                {
                                                                    PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                                }
                                                                else
                                                                {
                                                                    string ResponseStatus = "Invalid Bank Details";
                                                                    string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                    int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            int cnt = Insertion("Bank_ID,Bank_Name,Account_No,IFSC_Code,Account_HolderNm,Branch,M_Consumerid,Entry_Date,TNC", "'" + bankId + "','" + Bank_Name + "','" + AccountNumber + "','" + IFSCCode + "','" + Account_HolderNm + "','" + Branch + "','" + M_Consumerid + "','" + dtto + "','1'", "M_BankAccount");
                                                            var length = AccountNumber.Length;
                                                            AccountNumber = new String('X', length - 4) + AccountNumber.Substring(length - 4);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (IsKYCApplicable == 1)
                                                        {
                                                            AcData = OnlineAccountVerification(AccountNumber, IFSCCode, M_Consumerid);
                                                            if (AcData == "Success")
                                                            {
                                                                PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                            }
                                                            else
                                                            {
                                                                string ResponseStatus = "Invalid Bank Details";
                                                                string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (!string.IsNullOrEmpty(AccountNumber) && !string.IsNullOrEmpty(IFSCCode))
                                                            {
                                                                PaymentData = await OnlineAccountPayment(Reg.Comp_ID, Reg.Received_Code1, Reg.Received_Code2, M_Consumerid, Reg.Mobile_No, Iscash, AccountNumber, Account_HolderNm, IFSCCode);
                                                            }
                                                            else
                                                            {
                                                                string ResponseStatus = "Invalid Bank Details";
                                                                string UniqueNumber = DateTime.Now.ToString("yyMMddHHmmss");
                                                                int RequestId = InsertScalar("(pdate,M_consumerid,mobileno,Amount,pstatus,compId,orderid,Rec_code1,Rec_code2,comment,AccountNo,AccountHolderName,Mode,IFSCCode)", "'" + System.DateTime.Now + "','" + M_Consumerid + "','" + Reg.Mobile_No + "','" + Iscash + "','Failure','" + Reg.Comp_ID + "','" + UniqueNumber + "','" + Reg.Received_Code1 + "','" + Reg.Received_Code2 + "','" + ResponseStatus + "','" + AccountNumber + "','" + Account_HolderNm + "','A2A','" + IFSCCode + "'", "paytmtransaction");
                                                            }
                                                        }
                                                        var length = AccountNumber.Length;
                                                        AccountNumber = new String('X', length - 4) + AccountNumber.Substring(length - 4);
                                                    }
                                                    string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='SRV1028'";
                                                    var msg = await ExecuteDataTableAsync(SMStemplate);
                                                    if (msg.Rows.Count > 0)
                                                    {
                                                        strReturnMSG = msg.Rows[0][0].ToString();
                                                    }
                                                    else
                                                    {
                                                        strReturnMSG = $"Congratulation you have earn {strEarnedPoints} points against Coupon code {Reg.Code1}{Reg.Code2} from {Reg.Comp_Name}";
                                                    }
                                                    result = result + (strReturnMSG.Replace("<points>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName).Replace("<br>", "\n").Replace("<BR>", "\n").Replace("</BR>", "\n");
                                                }

                                                #endregion
                                            }
                                            break;
                                        }
                                    case "SRV1029":// Cashback-UPI
                                        {
                                            #region build loyalty
                                            Dictionary<string, object> Insertcodeforpoint = new Dictionary<string, object>
                                            {
                                                { "@code1", Reg.Code1 },
                                                { "@code2", Reg.Code2 },
                                                { "@SST_Id", Convert.ToInt32(dr[0]["SST_Id"]) },
                                                { "@intM_Consumer_MCOde", intM_Consumer_MCOde }
                                            };
                                            var dt = await ExecuteStoredProcedureDataTableAsync("Proc_SaveCodeDtsForBuiltLoyalty", Insertcodeforpoint);
                                            if (dt.Rows.Count > 0)
                                            {
                                                // string strM_consumerid = dt.Rows[0]["M_consumerid"].ToString();
                                                string strEarnedPoints = dt.Rows[0]["Points"].ToString();
                                                string dstrEarnedPoints = "";
                                                string IsCashConvert = dt.Rows[0]["IsCashConvert"].ToString();
                                                string AwardNameBL = dt.Rows[0]["AwardNameBL"].ToString();
                                                returnData = new { points = "", cash = strEarnedPoints, Status = "Success", CodeType = "Cashback Upi", CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };

                                                DataTable Bdt = await SelectTableDataAsync("M_Consumer", "top 1 M_Consumerid,ConsumerName,Email,UPIId,VRKbl_KYC_status", "MobileNo = '" + Reg.Mobile_No + "' and IsDelete=0");


                                                string M_Consumerid = Bdt.Rows[0]["M_Consumerid"].ToString();
                                                string ConsumerName = Bdt.Rows[0]["ConsumerName"].ToString();
                                                string ConsumerEmail = Bdt.Rows[0]["Email"].ToString();
                                                string UPIId = Bdt.Rows[0]["UPIId"].ToString();
                                                string VRKbl_KYC_status = Bdt.Rows[0]["VRKbl_KYC_status"].ToString(); // wembley

                                                if (dt.Rows[0]["ReachedFrequency"].ToString() == "0")
                                                {
                                                    string compnm = dsres.Rows[0]["Comp_Name"].ToString();
                                                    string M_Consumeridbank = string.Empty;
                                                    string UPIKYCSTATUS = string.Empty;
                                                    DataTable Bdtbank = await SelectTableDataAsync("M_Consumer", "top 1 * ", "MobileNo = '" + Reg.Mobile_No + "' and IsDelete=0");
                                                    if (Bdtbank.Rows.Count > 0)
                                                    {
                                                        UPIKYCSTATUS = Bdtbank.Rows[0]["UPIKYCSTATUS"].ToString();
                                                        M_Consumeridbank = Bdtbank.Rows[0]["M_Consumerid"].ToString();
                                                    }
                                                  bool statusofpayout= await MakeUPIpayment(Reg.Comp_ID, Reg.Mobile_No, ConsumerName, ConsumerEmail, UPIId, strEarnedPoints, M_Consumerid, Reg.Received_Code1, Reg.Received_Code2);
                                                    if (IsCashConvert == "0") // Covert points to cash will do end user
                                                    {
                                                        strReturnMSG = "Congrats! Won rs. <points> against <PRONAME> for coupon <CODE1><CODE2> , amount has been initiated through UPI payment.";
                                                        result = result + (strReturnMSG.Replace("<points>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString());
                                                    }
                                                    else
                                                    {
                                                        strReturnMSG = "Congrats! Won rs. <points> against <PRONAME> for coupon <CODE1><CODE2> , amount has been initiated through UPI payment.";
                                                        result = result + (strReturnMSG.Replace("<dpoints>", dstrEarnedPoints).Replace("<points>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName.ToUpper());
                                                    }

                                                }
                                                else if (Convert.ToInt32(dt.Rows[0]["ReachedFrequency"]) > 0)
                                                {
                                                    //Need to Handle Reached Frequncy
                                                    strReturnMSG = "Congrats! Won rs. <points> against <PRONAME> for coupon <CODE1><CODE2> , amount has been initiated through UPI payment.";
                                                    result = result + (strReturnMSG.Replace("<dpoints>", dstrEarnedPoints).Replace("<points>", strEarnedPoints)).ToUpper().Replace("<CODE1>", Reg.Received_Code1).Replace("<CODE2>", Reg.Received_Code2).Replace("<PRONAME>", dsres.Rows[0]["Pro_Name"].ToString()).Replace("<CMPNAME>", CompName.ToUpper());
                                                }
                                            }
                                            #endregion
                                            break;
                                        }


                                    default:
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Reg.Is_Success = 0;
                            await InsertProductInquiryAsync(Reg);
                            result = "Service of the coupon " + Reg.Received_Code1 + Reg.Received_Code2 + " has been deactivated.";
                        }
                    }
                    else
                    {
                        string SMStemplate = $"select  MsgBody from M_CodeCheckReturnMessage where comp_id='{Reg.Comp_ID}' and IsActive=1 and IsDelete=0 and Service_ID='Already'";
                        var msg = await ExecuteDataTableAsync(SMStemplate);
                        if (msg.Rows.Count > 0)
                        {
                            result = msg.Rows[0][0].ToString();
                        }
                        else
                        {
                            result = "The entered coupon code is invalid, kindly enter a valid coupon code";
                        }
                        Reg.Is_Success = 0;
                        await InsertProductInquiryAsync(Reg);

                    }
                    if (string.IsNullOrEmpty(result))
                    {
                        Reg.Is_Success = 0;
                        await InsertProductInquiryAsync(Reg);
                        result = "Service of the coupon " + Reg.Received_Code1 + Reg.Received_Code2 + " has been deactivated.";

                    }
                }
                else if (IsCheckedUse_Count)
                {
                    string Servicename = "Already Used";
                    string serqry = $"select top 1 s.ServiceName from M_ServiceSubscription ms inner join M_Service s on s.Service_ID=ms.Service_ID where Comp_ID='{Reg.Comp_ID}' order by Subscribe_Id desc";
                    var servieiddt = await ExecuteDataTableAsync(serqry);
                    if (servieiddt.Rows.Count > 0)
                    {
                        Servicename = servieiddt.Rows[0]["ServiceName"].ToString();
                    }
                    Reg.Comp_Name = dsres.Rows[0]["Comp_Name"].ToString();
                    
                    Reg.Is_Success = 2;
                    await InsertProductInquiryAsync(Reg);
                    result = $"The Entered Coupon code {Reg.Code1}{Reg.Code2} from {Reg.Comp_Name} Is Already Varified.";
                    returnData = new { points = 0, cash = "", Status = "UnSuccess", CodeType = Servicename, CodeNumber = $"{Reg.Code1}{Reg.Code2}", Codecheckeddate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
                }
            }
            catch (Exception ex)
            {
                ExceptionLogs("Find Exception in Codecheck API with log" + ex.Message + " TRack Exception : " + ex.StackTrace);
                result = "Something Went Wrong";
            }
            message = result;
            return (message, returnData);
        }

        public static bool MakeUPIpaymentdata(string Comp_id, string Mobileno, string ConsumerName, string ConsumerEmail, string UPIId, string Amount, string M_Consumerid, string Code1 = "", string Code2 = "")
        {
            bool Rsp = false;

            try
            {
                // string BaseUrl = ConfigurationManager.AppSettings["APIBaseUrl"];
                string str = String.Format("https://vrkableuat.vcqru.com/api/MakeRequestForUPIPayment");
                WebRequest request = WebRequest.Create(str);
                request.Method = "POST";
                string postData = "{\"Comp_id\":\"" + Comp_id + "\",\"M_Consumerid\":\"" + M_Consumerid + "\",\"Mobileno\":\"" + Mobileno + "\",\"ConsumerName\":\"" + ConsumerName + "\",\"ConsumerEmail\":\"" + ConsumerEmail + "\",\"UPIId\":\"" + UPIId + "\",\"Amount\":\"" + Amount + "\",\"Code1\":\"" + Code1 + "\",\"Code2\":\"" + Code2 + "\",\"WebEncData\":\"d861967d37065f549c13e39cc9a8b9_UPI\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/json";//application/     x-www-form-urlencoded

                request.ContentType = "application/json";//application/     x-www-form-urlencoded

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                JObject jObjects = JObject.Parse(responseFromServer);
                DataTable dt = new DataTable();
                if (jObjects["Status"].ToString() == "True" || jObjects["Status"].ToString() == "true")
                    Rsp = true;
            }
            catch (Exception ex)
            {

            }
            return Rsp;
        }

        public async Task<bool> MakeUPIpayment(string Comp_id, string Mobileno, string ConsumerName, string ConsumerEmail, string UPIId, string Amount, string M_Consumerid, string Code1 = "", string Code2 = "")
        {
            bool Rsp = false;

            try
            {
                //string BaseUrl = ConfigurationManager.AppSettings["APIBaseUrl"];
                string str = String.Format("https://api2.vcqru.com/api/MakeRequestForUPIPayment");
                WebRequest request = WebRequest.Create(str);
                request.Method = "POST";
                string postData = "{\"Comp_id\":\"" + Comp_id + "\",\"M_Consumerid\":\"" + M_Consumerid + "\",\"Mobileno\":\"" + Mobileno + "\",\"ConsumerName\":\"" + ConsumerName + "\",\"ConsumerEmail\":\"" + ConsumerEmail + "\",\"UPIId\":\"" + UPIId + "\",\"Amount\":\"" + Amount + "\",\"Code1\":\"" + Code1 + "\",\"Code2\":\"" + Code2 + "\",\"WebEncData\":\"d861967d37065f549c13e39cc9a8b9_UPI\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = "application/json";//application/     x-www-form-urlencoded

                request.ContentType = "application/json";//application/     x-www-form-urlencoded

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                JObject jObjects = JObject.Parse(responseFromServer);
                DataTable dt = new DataTable();
                if (jObjects["Status"].ToString() == "True" || jObjects["Status"].ToString() == "true")
                {
                    Rsp = true;
                    //return true;
                }
                else
                {
                    Rsp = false;
                    // return false;
                }

                //Rsp = true;
            }
            catch (Exception ex)
            {
                return false;
            }
            return Rsp;
        }

        public async Task<string> OnlineAccountPayment(string Comp_Id, string Code1, string Code2, string M_Consumerid, string MobileNo, string Amount, string AccountNo, string BeneName, string IFSCCode)
        {
            string Rsp = "NA";

            string EndPoint = "";
            Dictionary<string, object> modekey = new Dictionary<string, object>
                    {
                        { "@CompId", Comp_Id }
                    };
            var Modedt = await ExecuteStoredProcedureDataTableAsync("GetPayoutModedetails", modekey);

            if (Modedt.Rows.Count > 0)
            {

                //if (Modedt.Rows[0][0].ToString() == "IMPS")
                //    EndPoint = "MakeHDFC_IMPS_PaymentRequest";
                //else if (Modedt.Rows[0][0].ToString() == "NEFT")
                //    EndPoint = "MakeHDFC_NEFT_PaymentRequest";

                //if (IFSCCode.Contains("hdfc"))
                //    EndPoint = "MakeHDFC_A2A_PaymentRequest";

                EndPoint = "MakeHDFC_IMPS_PaymentRequest";

            }


            string str = String.Format("https://vrkableuat.vcqru.com/api/" + EndPoint);
            WebRequest request = WebRequest.Create(str);
            request.Method = "POST";
            string postData = "{\"Comp_Id\":\"" + Comp_Id + "\",\"Code1\":\"" + Code1 + "\",\"Code2\":\"" + Code2 + "\",\"M_Consumerid\":\"" + M_Consumerid + "\",\"MobileNo\":\"" + MobileNo + "\",\"Amount\":\"" + Amount + "\",\"AccountNo\":\"" + AccountNo + "\",\"BeneName\":\"" + BeneName + "\",\"IFSCCode\":\"" + IFSCCode + "\",\"WebEncData\":\"d861967d37065f549c13e39cc9a8b9\"}"; byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/json";
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Rsp = responseFromServer;
            return Rsp;
        }

        public static string OnlineAccountVerification(string AccountNo, string IFSCCode, string M_Consumerid)
        {
            string Rsp = "NA";

            try
            {
                string str = String.Format("https://vrkableuat.vcqru.com/api/WebAccountVerification");
                WebRequest request = WebRequest.Create(str);
                request.Method = "POST";
                string postData = "{\"M_Consumerid\":\"" + M_Consumerid + "\",\"AccountNo\":\"" + AccountNo + "\",\"IFSCCode\":\"" + IFSCCode + "\",\"WebEncData\":\"d861967d37065f549c13e39cc9a8b9\"}";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentType = "application/json";
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                using (WebResponse response = request.GetResponse())
                {
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    using (Stream dataStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        string responseFromServer = reader.ReadToEnd();
                        Rsp = responseFromServer;
                    }
                }
            }
            catch (Exception ex)
            {
                Rsp = "Error: " + ex.Message;
            }
            return Rsp;
        }

        public string instantpayout(string MobileNo, string ConsumerName, string UPIId, string Amount, string UniqueNumber, string ConsumerEmail, string baseUrl, string URL)
        {





            try
            {
                var options = new RestClientOptions(baseUrl)
                {
                    MaxTimeout = -1,
                };
                var client = new RestClient(options);
                var request = new RestRequest(URL, Method.Post);
                //   var client = new RestClient("https://api.instantpay.in/payments/payout");
                // client.Timeout = -1;
                //var request = new RestRequest(Method.POST);
                // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                request.AddHeader("X-Ipay-Auth-Code", "1");
                //request.AddHeader("X-Ipay-Client-Id", "YWY3OTAzYzNlM2ExZTJlOZh0WjOBBKD33wYkrJweAJ4="); --live
                //request.AddHeader("X-Ipay-Client-Secret", "6ab3362d2e36fb58cc2bedfc172ac470edd25505c5a962c82d894d929b6a8242");// live


                request.AddHeader("X-Ipay-Client-Id", "YWY3OTAzYzNlM2ExZTJlOZh0WjOBBKD33wYkrJweAJ4=");
                request.AddHeader("X-Ipay-Client-Secret", "ab5fd7d0eecd9213e719f730adb07e609b0d37fdd940c50c8e7801d0912f8b9a"); // For UAT mode


                request.AddHeader("X-Ipay-Endpoint-Ip", "103.100.218.202");
                request.AddHeader("Content-Type", "application/json");
                var body = "{\"payer\":{\"bankProfileId\": \"0\",\"accountNumber\":\"" + MobileNo.Substring(MobileNo.Length - 10, 10).ToString() + "\"},\"payee\":{\"name\":\"" + ConsumerName + "\",\"accountNumber\":\"" + UPIId + "\",\"bankIfsc\":\"\"},\"transferMode\":\"UPI\",\"transferAmount\":\"" + Amount + "\",\"externalRef\":\"" + UniqueNumber + "\",\"latitude\":\"20.2326\",\"longitude\":\"78.1228\",\"remarks\":\"UPI\",\"alertEmail\":\"" + ConsumerEmail + "\",\"purpose\":\"REIMBURSEMENT\"}";
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                //IRestResponse response = client.Execute(request);
                RestResponse response = client.Execute(request);
                //  Dbstring.ExceptionLogs("Bank MakeRequestForUPIPayment For KYC | Body |" + body + " | Response | " + response.Content + " | StatusDescription " + response.ErrorMessage);


                // RestResponse response = client.Execute(responseFromServer);
                return response.Content;
                //return responseFromServer;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CreateTicketAsync(string description,string M_Consumerid,string Comp_id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = "INSERT INTO Tickets (Description, CreatedAt, Status,Comp_id,M_Consumerid) OUTPUT INSERTED.TicketId VALUES (@Description, @CreatedAt, @Status,@Comp_id,@M_Consumerid)";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@Status", "Open"); 
                    command.Parameters.AddWithValue("@Comp_id", M_Consumerid); 
                    command.Parameters.AddWithValue("@M_Consumerid", Comp_id); 

                    var ticketId = (int)await command.ExecuteScalarAsync();
                    return ticketId;
                }
            }
        }

        // Save image paths associated with the ticket
        public async Task SaveTicketImagesAsync(int ticketId, List<string> imagePaths)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                foreach (var path in imagePaths)
                {
                    var query = "INSERT INTO TicketImages (TicketId, ImagePath) VALUES (@TicketId, @ImagePath)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TicketId", ticketId);
                        command.Parameters.AddWithValue("@ImagePath", path);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}

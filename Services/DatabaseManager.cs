using CoreApi_BL_App.Models;
using Microsoft.Data.SqlClient;
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

       


    }
}

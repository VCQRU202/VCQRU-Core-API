using Microsoft.Extensions.Configuration; // Required for IConfiguration
using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;


namespace CoreApi_BL_App.Services
{
	public interface IDatabaseService
	{
		// Define methods that will be implemented in the SQL_DB class
		DataSet ExecuteQuery(string query, params SqlParameter[] parameters);
	}

	public class SQL_DB : IDatabaseService
	{
		private readonly string _connectionString;


		public SQL_DB(IConfiguration configuration)
		{
			// Fetching the connection string from appsettings.json
			_connectionString = configuration.GetConnectionString("defaultConnectionbeta");
		}

		#region Execute a SELECT query and return a DataSet
		public DataSet ExecuteQuery(string query, params SqlParameter[] parameters)
		{
			DataSet dataSet = new DataSet();
			using (var connection = new SqlConnection(_connectionString))
			{
			using (var command = new SqlCommand(query, connection))
				{ 
					if (parameters != null && parameters.Length > 0)
					{
						command.Parameters.AddRange(parameters);
					}
					using (var dataAdapter = new SqlDataAdapter(command))
					{
						try
						{
							connection.Open();
							dataAdapter.Fill(dataSet);
						}
						catch (Exception ex)
						{
							throw new Exception("Error executing query", ex);
						}
						finally
						{
							connection.Close();
						}
					}
				}
			}
			return dataSet;
		}
		#endregion

		#region Execute a non-query command (INSERT, UPDATE, DELETE)
		public int ExecuteNonQuery(string query, params SqlParameter[] parameters)
		     {
		     	using(var connection = new SqlConnection(_connectionString))
		     	{
		     		using (var command = new SqlCommand(query, connection))
		     		{
		     			command.Parameters.AddRange(parameters);
		     			connection.Open();
		     			int result = command.ExecuteNonQuery(); 
		     			return result;
		     		}
		     	}
		     }
		#endregion

		#region execute select Query to find the single data from the database	
		public object ExecuteScalar(string query, params SqlParameter[] parameters)
		{
			using(var connection = new SqlConnection(_connectionString))
			{
				using(var command = new SqlCommand(query,connection))
				{
					command.Parameters.AddRange(parameters);

					connection.Open();
					var result = command.ExecuteScalar();  
					return result;
				}
			}
		}
		#endregion

		#region execute select query to store result in datatable
		public DataTable ExecuteQueryAsDataTable(string query, params SqlParameter[] parameters)
		{
			DataTable dataTable = new DataTable();
			using(var connection = new SqlConnection(_connectionString))
			{
				using(var cmd = new SqlCommand(query, connection))
				{
					if (parameters != null)
					{
                     cmd.Parameters.AddRange(parameters);
					}
					// Using SqlDataAdapter to fill the DataTable with the query result
					using(var adapter= new SqlDataAdapter(cmd))
					{
						try
						{
							connection.Open();
							adapter.Fill(dataTable);
						}
						catch(Exception ex)
						{
							throw new Exception("Error executing query", ex);
						}
						finally
						{
							connection.Close();
						}
					}
				}
			}
			return dataTable;
		}
		#endregion

		#region Method to exexute the Stored procedure
		public int ExecuteProcedure(string ProcedureName, params SqlParameter[] parameters)
		{
			using(var conn = new SqlConnection(_connectionString))
			{
				using(var cmd = new SqlCommand(ProcedureName,conn))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					if (parameters!=null && parameters.Length > 0)
					{
						cmd.Parameters.AddRange(parameters);
					}
					try
					{
						conn.Open();
						int result = cmd.ExecuteNonQuery();  
						return result;
					}
					catch(Exception ex)
					{
						throw new Exception("Error executing stored procedure", ex);
					}
					finally
					{
						conn.Close();
					}
				}
			}
		}
		#endregion
	}
}

namespace CoreApi_BL_App.Models
{
	public class LogManager
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly string _logDirectory;

		public LogManager(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
			_logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
			Directory.CreateDirectory(_logDirectory); // Ensure log directory exists
		}

		public void ExceptionLogs(string message)
		{
			try
			{
				string logFilePath = Path.Combine(_logDirectory, $"LogManager_{DateTime.Now:yyyyMMdd}.txt");
				string logEntry = $"{DateTime.Now:yyyy/MM/dd hh:mm:ss tt} : {message}";

				File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
			}
			catch (Exception ex)
			{
				// Optionally log this to a separate logging service or console
				Console.WriteLine($"Error logging exception: {ex.Message}");
			}
		}
	}
}


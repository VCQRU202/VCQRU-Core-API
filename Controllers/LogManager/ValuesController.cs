using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // Required for ILogger<T>

namespace CoreApi_BL_App.Controllers.LogManager
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		// Inject ILogger<T> into the controller
		private readonly ILogger<ValuesController> _logger;

		// Constructor injection of logger
		public ValuesController(ILogger<ValuesController> logger)
		{
			_logger = logger;
		}

		// Example: GET request to fetch values
		[HttpGet]
		public IActionResult GetValues()
		{
			_logger.LogInformation("GET request received to fetch values.");

			try
			{
				var data = new[] { "Value1", "Value2", "Value3" }; // Example data
				_logger.LogInformation("Successfully fetched values.");
				return Ok(data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while fetching values.");
				return StatusCode(500, "Internal server error");
			}
		}

		// Example: POST request to create a value
		[HttpPost]
		public IActionResult CreateValue([FromBody] string newValue)
		{
			_logger.LogInformation($"POST request received to create a new value: {newValue}");

			try
			{
				// Simulate saving data (e.g., to a database)
				_logger.LogInformation($"Successfully created new value: {newValue}");
				return CreatedAtAction(nameof(CreateValue), new { value = newValue }, newValue);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred while creating a new value.");
				return StatusCode(500, "Internal server error");
			}
		}
	}
}

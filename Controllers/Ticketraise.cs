using CoreApi_BL_App.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_BL_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketRaiseController : ControllerBase
    {
        private readonly DatabaseManager _databaseManager;
        private readonly ILogger<TicketRaiseController> _logger;

        public TicketRaiseController(DatabaseManager databaseManager, ILogger<TicketRaiseController> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        [HttpPost("raise-ticket")]
       
        public async Task<IActionResult> RaiseTicket([FromForm] TicketRaiseDto ticketRaiseDto)
        {
            if (ticketRaiseDto == null || string.IsNullOrEmpty(ticketRaiseDto.Description))
            {
                return BadRequest("Ticket description cannot be empty.");
            }

            try
            {
                // Create the ticket in the database
                var ticketId = await _databaseManager.CreateTicketAsync(ticketRaiseDto.Description, ticketRaiseDto.M_Consumerid, ticketRaiseDto.Comp_id);

                var imagePaths = new List<string>();
                string imgpathadd = DateTime.Now.ToString("yyyyMMddHHmmss");
                string uploadDirectory = "Uploads";

                // Ensure the 'Uploads' directory exists
                if (!Directory.Exists(uploadDirectory))
                {
                    Directory.CreateDirectory(uploadDirectory);
                }

                // Process images if any are provided
                if (ticketRaiseDto.Images != null && ticketRaiseDto.Images.Any())
                {
                    foreach (var image in ticketRaiseDto.Images)
                    {
                        // Validate file type (e.g., only allow images)
                        if (!IsValidImageFile(image))
                        {
                            return BadRequest(new ApiResponse<object>(false, "Only image files are allowed."));
                        }

                        // Generate a unique file name
                        var fileName = Path.GetFileNameWithoutExtension(image.FileName) + "_" + imgpathadd + Path.GetExtension(image.FileName);
                        var filePath = Path.Combine(uploadDirectory, fileName); // Store in 'Uploads' folder

                        // Save the image to the file system
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        imagePaths.Add(filePath);
                    }

                    // Save image paths to the database
                    await _databaseManager.SaveTicketImagesAsync(ticketId, imagePaths);
                }

                var jdata = new
                {
                    TicketId = ticketId,
                };

                return Ok(new ApiResponse<object>(true, "Ticket raised successfully!", jdata));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error raising ticket: {ex.Message}");
                return BadRequest(new ApiResponse<object>(false, "An error occurred while raising the ticket."));
            }
        }


        // Validate if the file is an image (you can expand this based on your needs)
        private bool IsValidImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
            var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return fileExtension != null && allowedExtensions.Contains(fileExtension);
        }
    }

    public class TicketRaiseDto
    {
        public string Comp_id { get; set; }
        public string M_Consumerid { get; set; }
        public string Description { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}

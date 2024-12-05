using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tryqrcode.Models;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.IO;
using ZXing; 

namespace tryqrcode.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
        [HttpPost]
        public IActionResult Index(IFormFile qrImage)
		{
            if (qrImage == null || qrImage.Length == 0)
            {
                _logger.LogError("No file uploaded or file is empty.");
                return Json(new { success = false, message = "No file uploaded or file is empty." });
            }

            _logger.LogInformation("File uploaded successfully. File Name: {0}, File Size: {1} bytes", qrImage.FileName, qrImage.Length);

            try
            {
                // Save file to memory stream to process
                using (var memoryStream = new MemoryStream())
                {
                    qrImage.CopyTo(memoryStream);

                    // Try processing the image as a Bitmap
                    using (var bitmap = new Bitmap(memoryStream))
                    {
                        _logger.LogInformation("Processing QR code from image...");

                        // Initialize the Barcode reader to read QR code
                        var reader = new BarcodeReader();
                        var result = reader.Decode(bitmap);

                       
                        if (result == null)
                        {
                            _logger.LogError("Unable to decode QR code from image.");
                            return Json(new { success = false, message = "Unable to decode QR code from image." });
                        }

                        var decodedText = result.Text;
                        _logger.LogInformation($"Decoded QR code data: {decodedText}");

                        // Check if the decoded text is a valid URL
                        if (Uri.IsWellFormedUriString(decodedText, UriKind.Absolute))
                        {
                            return Json(new { success = true, redirectUrl = decodedText });
                        }

                        return Json(new { success = true, message = "QR code decoded successfully.", data = decodedText });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception with detailed message
                _logger.LogError($"Error processing QR code: {ex.Message} - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error processing QR code: {ex.Message}" });
            }
        }
        public class QRCodeData
        {
            public string? QrData { get; set; }
        }
        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
using api.Entities;
using api.Services;
using api.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Hangfire;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PdfController : ControllerBase
    {
        private readonly PdfService _pdfService;
        private readonly AppDbContext _context;
        private readonly FornaxService _fornaxService;

        public PdfController(
            PdfService pdfService,
            AppDbContext context,
            FornaxService fornaxService)
        {
            _pdfService = pdfService;
            _context = context;
            _fornaxService = fornaxService;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                var filePath = Path.GetTempFileName();
                using (var stream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(stream);
                }

                // Process the PDF
                var outputDirectory = Path.Combine(AppContext.BaseDirectory, "split_pdfs");
                Directory.CreateDirectory(outputDirectory);

                //BackgroundJob.Enqueue(() => _pdfService.ProcessPdfAsync(filePath, outputDirectory));
                var splitFileUrls = await _pdfService.ProcessPdfAsync(filePath, outputDirectory);
                return Ok(splitFileUrls);
                //return Ok();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error uploading PDF: {ex}");
                return StatusCode(500, "Internal server error");
            }
           
        }

        [HttpGet("split-files")]
        public IActionResult GetSplitFiles()
        {
            var files = _context.PdfMetadata.Select(m => new { m.Id, m.SplitFileName }).ToList();
            return Ok(files);
        }


        [HttpPost("open-in-fornax")]
        public async Task<IActionResult> OpenInFornax([FromBody] UrlRequest request)
        {
            var url = request.Url;
            var metadata = _context.PdfMetadata.FirstOrDefault(m => m.SplitFileName == url);
            if (metadata == null)
                return NotFound("File not found");

            var filePath = metadata.SplitFileName;
            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found on disk");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileContent = Convert.ToBase64String(fileBytes);
            
            return Ok(new
            {
                Message = "File content retrieved successfully",
                FileName = metadata.SplitFileName,
                FileContent = fileContent
            });
        }


        // Forward the file data to Fornax API (pseudo-code)
        //var fornaxResponse = await _fornaxService.OpenFile(metadata.SplitFileName);

        //return Ok(new
        //{
        //    Message = "File opened in Fornax",
        //    Response = fornaxResponse
        //});
    }

}

public class UrlRequest { public string Url { get; set; } }
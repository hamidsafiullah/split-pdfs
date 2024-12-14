using api.Context;
using api.Entities;
using api.Hubs;
using api.Services;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.SignalR;
using System;

namespace api.Services
{
    public class PdfService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PdfService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public void SplitPdf(string inputPdfPath, string outputDirectory)
        {
            using (PdfReader reader = new PdfReader(inputPdfPath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                int totalPages = pdfDoc.GetNumberOfPages();
                for (int i = 1; i <= totalPages; i++)
                {
                    string outputPath = Path.Combine(outputDirectory, $"Page-{i}.pdf");
                    using (PdfDocument singlePagePdf = new PdfDocument(new PdfWriter(outputPath)))
                    {
                        pdfDoc.CopyPagesTo(i, i, singlePagePdf);
                    }
                }
            }
        }


        public async Task<List<string>> ProcessPdfAsync(string filePath, string outputDirectory)
        {
            var existingFiles = Directory.GetFiles(outputDirectory);
            foreach (var file in existingFiles)
            {
                System.IO.File.Delete(file);
            }

            List<string> splitFileUrls = new List<string>();
            SplitPdf(filePath, outputDirectory);
            int i = 0;

            foreach (var splitFile in Directory.GetFiles(outputDirectory))
            {
                var metadata = new PdfMetaData
                {
                    OriginalFileName = Path.GetFileName(filePath),
                    SplitFileName = splitFile,
                    UploadedDate = DateTime.Now
                };
                _context.PdfMetadata.Add(metadata);
                i ++;
                await _hubContext.Clients.All.SendAsync("Receive", $"Split file page {i} created");

                var splitFileUrl = splitFile;
                splitFileUrls.Add(splitFileUrl);
            }
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("Receive", "PDF processing complete");

            return splitFileUrls;
        }


    }
    
}

using Framework.Utilities.PdfService;
using Microsoft.AspNetCore.Mvc;

namespace Framework.Test.API.Controllers
{
    [ApiController]
    [Route("pdf")]
    public class PdfExporterController : ControllerBase
    {
        [HttpGet]
        public FileContentResult PdfExporterTest()
        {
            try
            {
                var data = new List<PdfTestItem>();
                for (int i = 0; i < 100; i++)
                    data.Add(new PdfTestItem { Name = $"Item{i}", Amount = i });

                var res = PdfExporter<PdfTestItem>.ExportAsPdf(data, new PdfExporterConfiguration { DocumentHeader = "Test Pdf", FileName = "TestPdf" });
                return res;
            }
            catch
            {
                throw;
            }
        }
    }
    class PdfTestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Amount { get; set; }

    }
}
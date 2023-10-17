namespace Framework.Utilities.PdfService
{
    public class PdfExporterConfiguration
    {
        public string DocumentHeader { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public int MaxColumnTitleLength { get; set; } = 100;
        public bool Landscape { get; set; } = false;
        public int MaxColumCount { get; set; } = int.MaxValue;
        public PdfExporterConfiguration()
        {
            FileName = DateAsFileName();
        }
        public PdfExporterConfiguration(string fileName)
        {
            FileName = !string.IsNullOrEmpty(fileName) ? fileName : DateAsFileName();
        }

        private static string DateAsFileName()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
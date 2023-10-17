using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Framework.Utilities.PdfService
{
    public static class PdfExporter<T>
    {
        private const int DefaultFontSize = 9;
        private const int HeaderFontSize = 12;

        private static PdfExporterConfiguration Configuration { get; set; } = new();

        public static FileContentResult ExportAsPdf(IEnumerable<T> items, PdfExporterConfiguration configuration)
        {
            Configuration = configuration;
            Validate(items);
            FixFileName();
            return ConvertToPdf(items);
        }

        private static FileContentResult ConvertToPdf(IEnumerable<T> items)
        {
            byte[] pdfBytes = GeneratePdfBytes(items);
            var fileName = Path.GetFileNameWithoutExtension(Configuration.FileName);
            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = $"{fileName}.pdf" };
        }

        private static byte[] GeneratePdfBytes(IEnumerable<T> items)
        {
            var documentHeader = Configuration.DocumentHeader;
            var columnCount = items.First()?.GetType().GetProperties().Take(Configuration.MaxColumCount).ToList().Count;
            var pageSize = Configuration.Landscape ? PageSizes.A4.Landscape() : PageSizes.A4;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(pageSize);
                    page.Margin(20);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(DefaultFontSize).FontFamily(PdfExporter<T>.GetFontFamily()));

                    page.Header()
                        .ShowOnce()
                        .Background(Colors.LightGreen.Medium)
                        .AlignCenter()
                        .Text(documentHeader)
                        .SemiBold().FontSize(HeaderFontSize).FontColor(Colors.Blue.Darken4);

                    page.Content().Element(ComposeContent);

                    page.Footer().AlignCenter().Text(x => { x.CurrentPageNumber(); });
                });
            });

            var pdfBytes = doc.GeneratePdf();
            return pdfBytes;

            void ComposeContent(IContainer container)
            {
                container.PaddingVertical(40).Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Element(ComposeTable);

                });
            }

            void ComposeTable(IContainer container)
            {
                container.Table(table =>
                {
                    var headers = GetColumnHeaders();
                    // step 1
                    table.ColumnsDefinition(columns =>
                    {
                        foreach (var dataHeader in headers)
                        {
                            columns.RelativeColumn();
                        }
                    });

                    // step 2
                    table.Header(header =>
                    {
                        foreach (var dataHeader in headers)
                        {
                            header.Cell().Element(CellStyle).Text(dataHeader);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container.DefaultTextStyle(x => x.SemiBold().FontSize(HeaderFontSize).LineHeight(1.6F)).Background("#E5F9DB").BorderColor(Colors.BlueGrey.Medium).BorderVertical(0.1f);
                        }
                    });

                    // step 3
                    var i = 0;
                    foreach (var item in items)
                    {
                        if (item is not null)
                        {
                            var props = item.GetType().GetProperties().Take(Configuration.MaxColumCount);
                            foreach (var prop in props)
                            {
                                var cellValue = GetCellValue(item, prop);
                                table.Cell().Background(i % 2 == 0 ? "#F8F6F4" : Colors.White).BorderColor(Colors.BlueGrey.Medium).BorderVertical(0.1f).Element(CellStyle).Text(cellValue);
                            }

                            static IContainer CellStyle(IContainer container)
                            {
                                return container.Padding(3);
                            }
                        }
                        i++;
                    }
                });
            }

            List<string> GetColumnHeaders()
            {
                var props = typeof(T).GetProperties().Take(Configuration.MaxColumCount).ToList();
                if (props == null || props.Count == 0)
                    return new List<string>();

                var propNames = props.Select(prop => prop.Name).ToList();
                var headers = propNames.Select(GetColumnHeaderText).ToList();
                return headers;
            }

            string GetCellValue(T classObject, PropertyInfo prop)
            {
                var cell = prop?.GetValue(classObject, null)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(cell) && Configuration.MaxColumnTitleLength > 0)
                    cell = cell.Length <= Configuration.MaxColumnTitleLength ? cell : string.Concat(cell[..Configuration.MaxColumnTitleLength], "...");
                return cell;
            }

        }

        private static string GetColumnHeaderText(string propName)
        {
            if (string.IsNullOrEmpty(propName))
                return string.Empty;
            if (Configuration.MaxColumnTitleLength > 0 && propName.Length >= Configuration.MaxColumnTitleLength)
                return string.Concat(propName.AsSpan(0, Configuration.MaxColumnTitleLength), "...");
            return propName;
        }
        private static void FixFileName()
        {
            if (Configuration.FileName.Contains(".pdf", comparisonType: StringComparison.InvariantCultureIgnoreCase))
                Configuration.FileName = Configuration.FileName.Replace(".pdf", "", StringComparison.InvariantCultureIgnoreCase);
        }

        private static void Validate(IEnumerable<T> items)
        {
            if (string.IsNullOrEmpty(Configuration.DocumentHeader))
                throw new Exception("Document header is null!");
            if (!items.Any())
                throw new Exception("There is no data to export!");
        }

        private static string GetFontFamily()
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            return isWindows ? "Arial" : "DejaVu Sans";
        }
    }
}


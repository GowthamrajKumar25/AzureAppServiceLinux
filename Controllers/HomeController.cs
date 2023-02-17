using AzureAppServiceLinux.Models;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AzureAppServiceLinux.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public ActionResult ExportToPDF()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VVhjQlFadFlJXGFWfVJpTGpQdk5xdV9DaVZUTWY/P1ZhSXxRdk1hW31fdXxRR2ZeVkU=");
            string fullPath = System.IO.Path.GetFullPath(@"Data/HtmlString.html");
            string htmlString = System.IO.File.ReadAllText(fullPath);
            string baseurl = System.IO.Path.GetFullPath(@"Data/");

            byte[] finalBytes = null;

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            try
            {
                HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();

                if (isLinux)
                {
                    string shellFilePath = System.IO.Path.GetFullPath(@"dependenciesInstall.sh");
                    InstallDependecies(shellFilePath);
                    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
                   
                    //Initialize HTML to PDF converter
                    htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
                    BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
                    //blinkConverterSettings.BlinkPath = System.IO.Path.GetFullPath(@"BlinkBinariesLinux");

                    //Set command line arguments to run without sandbox.
                    blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
                    blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");

                    blinkConverterSettings.AdditionalDelay = 4000;
                    htmlConverter.ConverterSettings = blinkConverterSettings;
                }
                else if (isWindows)
                {

                    //Initialize HTML to PDF converter
                    htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
                    BlinkConverterSettings settings = new BlinkConverterSettings();
                    settings.AdditionalDelay = 10000;
                    htmlConverter.ConverterSettings = settings;
                }

                using (PdfDocument document = new PdfDocument())
                {
                    document.EnableMemoryOptimization = true;
                    PdfMergeOptions options = new PdfMergeOptions();
                    options.ExtendMargin = true;
                    document.PageSettings.Margins.Top = 50;
                    document.PageSettings.Margins.Bottom = 50;
                    document.PageSettings.Margins.Left = 20;
                    document.PageSettings.Margins.Right = 20;

                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        var pdFDocument = htmlConverter.Convert(fullPath);

                        pdFDocument.Save(outputStream);
                        var bytes = outputStream.ToArray();
                        PdfDocument.Merge(document, options, bytes);
                        pdFDocument.Close(true);
                        outputStream.Close();
                    }

                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        document.Save(outputStream);
                        finalBytes = outputStream.ToArray();
                        outputStream.Close();
                    }

                   
                }
            }
            catch (Exception ex)
            {
                var pdFDocument = new PdfDocument();
                // Set the page size.
                pdFDocument.PageSettings.Size = PdfPageSize.A4;
                //Add a page to the document.
                PdfPage page = pdFDocument.Pages.Add();

                //Create PDF graphics for the page.
                PdfGraphics graphics = page.Graphics;
                //Set the font.
                PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 12);
                //Draw the text.
                graphics.DrawString(ex.InnerException.ToString(), font, PdfBrushes.Black, new Syncfusion.Drawing.PointF(0, 0));
                MemoryStream str = new MemoryStream();
                pdFDocument.Save(str);
                finalBytes = str.ToArray();
                pdFDocument.Close(true);
                str.Close();
            }
            return File(finalBytes, System.Net.Mime.MediaTypeNames.Application.Pdf, "AzureAppServiceLinux.pdf");
        }
        // [C# Code]
        private void InstallDependecies(string shellFilePath)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c " + shellFilePath,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
            process.Start();
            process.WaitForExit();
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
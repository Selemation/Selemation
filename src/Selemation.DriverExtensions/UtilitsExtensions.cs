using System;
using System.IO;
using OpenQA.Selenium;
using Selemation.Tools.Logger;
using Serilog;

namespace Selemation.DriverExtensions
{
    public static class UtilitsExtensions
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();

        /// <summary>
        /// Take screenshot and saving to root folder of solution
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="error"></param>
        /// <param name="path"></param>
        public static void TakeScreenshot(this IWebDriver driver, string error = "", string path= "..\\..\\..\\..\\..\\")
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            string currentPath = Path.GetDirectoryName(typeof(UtilitsExtensions).Assembly.Location) ?? "";
            var folderPath = Path.Combine(currentPath, path, "Screens");
            Directory.CreateDirectory(folderPath);
            var imageType = ScreenshotImageFormat.Png;
            var uri = new Uri(driver.Url);
            string imgPath = Path.Combine(folderPath,
                $"{error}_{uri.Host}_{DateTime.Now:HH.mm.ss}.{imageType.ToString().ToLower()}");
            screenshot.SaveAsFile(imgPath, imageType);
            Logger.Information($"Scrennshoot Saved:{error}");
        }
    }
}
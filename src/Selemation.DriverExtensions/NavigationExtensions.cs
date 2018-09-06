using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selemation.Tools.Logger;
using Serilog;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Selemation.DriverExtensions
{
    public static class NavigationExtensions
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();

        /// <summary>
        /// wait while driver will be on specifed url
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        public static void WaitUrl(this IWebDriver driver, string url)
        {
            var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(50),
                TimeSpan.FromMilliseconds(200));
            wait.Until(ExpectedConditions.UrlContains(url));
            Logger.Information($"Endpoint was present: {driver.Url}");
        }

        /// <summary>
        /// Navigate driver to URL.
        /// If redirect was failed - try to redirect several times
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url">Url for redirect</param>
        public static void GoToUrl(this IWebDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);

            for (int i = 0; i < 10 && !Tools.Tools.UrlIsSame(driver.Url, url); ++i)
            {
                driver.Navigate().GoToUrl(url);
                driver.Navigate().Refresh();
            }

            if (!Tools.Tools.UrlIsSame(driver.Url, url))
                throw new Exception($"Not redirected, current {driver.Url}");
        }

    }
}
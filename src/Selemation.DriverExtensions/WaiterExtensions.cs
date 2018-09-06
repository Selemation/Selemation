using System;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Selemation.Tools.Logger;
using Serilog;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Selemation.DriverExtensions
{
    public static class WaiterExtensions
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();

        /// <summary>
        /// wait untill all js will loaded
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeoutSeconds"></param>
        public static void WaitForJq(this IWebDriver driver, int timeoutSeconds)
        {
            var javaScriptExecutor = driver as IJavaScriptExecutor;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));

            Func<IWebDriver, bool> readyCondition = webDriver => (bool)javaScriptExecutor
                .ExecuteScript("return (document.readyState == 'complete' && jQuery.active == 0)");
            wait.Until(readyCondition);
        }

        public static void WaitForJs(this IWebDriver driver, int timeoutSeconds)
        {
            var javaScriptExecutor = driver as IJavaScriptExecutor;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));

            Func<IWebDriver, bool> readyCondition = webDriver => (bool)javaScriptExecutor
                .ExecuteScript("return document.readyState == 'complete'");
            wait.Until(readyCondition);
        }

        public static void WaitForAngular(this IWebDriver driver, int timeoutSeconds)
        {
            var javaScriptExecutor = driver as IJavaScriptExecutor;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));

            string angularReadyScript = "return angular.element(document).injector().get('$http').pendingRequests.length === 0";

            Func<IWebDriver, bool> readyCondition = webDriver => (bool)javaScriptExecutor
                .ExecuteScript(angularReadyScript);
            wait.Until(readyCondition);
        }

        public static IWebElement WaitElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            var webElement = GeneralWaiter(driver, timeoutInSeconds, drv => drv.FindElement(by));

            return webElement;
        }

        /// <summary>
        /// wait while something that looks like button will be presented on page
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="by"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public static IWebElement WaitClickable(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            var webElement = GeneralWaiter(driver, timeoutInSeconds, ExpectedConditions.ElementToBeClickable(by));

            return webElement;
        }

        /// <summary>
        /// Complicated waiter. Has overrided features to be able log attemts of finding of elements.
        /// Needed for better debugging.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        private static IWebElement GeneralWaiter(this IWebDriver driver, int timeoutInSeconds,
            Func<IWebDriver, IWebElement> condition)
        {
            if (timeoutInSeconds <= 0) return condition(driver);

            var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(timeoutInSeconds),
                new TimeSpan(0, 0, 0, 0, 200));
            int i = 0;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                return wait.Until(drv =>
                {
                    try
                    {
                        ++i;
                        var element = condition(driver);
                        stopwatch.Stop();
                        Logger.Information($"Element readed after {i} Iteration: {stopwatch.ElapsedMilliseconds:N}");
                        return element;
                    }
                    catch (Exception e)
                    {
                        if (i % 10 == 1)
                        {
                            Logger.Error($"Iter: {i}", e);
                        }

                        throw;
                    }
                });
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                Logger.Error($"Iter: {i}, time:{stopwatch.ElapsedMilliseconds:N}", e);
                throw;
            }
        }
    }
}
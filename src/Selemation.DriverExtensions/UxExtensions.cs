using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using Selemation.Tools.Logger;
using Serilog;

namespace Selemation.DriverExtensions
{
    public static class UxExtensions
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();

        /// <summary>
        /// Click on element, clear and type
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="text"></param>
        public static void ClearAndType(this IWebDriver driver, By locator, string text)
        {
            var findElement = driver.WaitElement(locator, 10);
            findElement.Click();
            findElement.Clear();
            findElement.SendKeys(text);
        }

        /// <summary>
        /// SLowly input symbols (needed for some complicated cases with bad JavaScript on input elements)
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="text"></param>
        public static void TypeSlowly(this IWebDriver driver, By locator, string text)
        {
            var findElement = driver.WaitElement(locator, 10);
            findElement.Clear();
            for (var i = 0; i < text.Length; i++)
            {
                Thread.Sleep(10);
                findElement.SendKeys(text.Substring(i, 1));
            }
        }

        public static IWebElement SelectElement(this IWebDriver driver, int numOfElement, string cssSelector)
        {
            ReadOnlyCollection<IWebElement> readOnlyCollection = driver.FindElements(By.CssSelector(cssSelector));
            IWebElement element = readOnlyCollection[numOfElement];

            return element;
        }

        public static bool IsElementPresent(this IWebDriver driver, By by)
        {
            try
            {
                driver.FindElement(by);

                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        public static bool IsElementPresentWithWait(this IWebDriver driver, By by, int time = 5)
        {
            try
            {
                driver.WaitElement(by, time);
                return true;
            }

            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }
       
        public static IWebElement FirstExistingElementFrom(this IWebDriver driver, string[] selectors)
        {
            var listOfSelectors = selectors.Select(By.CssSelector).ToList();

            By existedSelector = listOfSelectors.FirstOrDefault(x => driver.IsElementPresentWithWait(x, 2));
            return driver.FindElement(existedSelector);
        }
    }
}
using OpenQA.Selenium;
using Selemation.Tools.Logger;
using Serilog;

namespace Selemation.DriverExtensions
{
    public static class JsExecuterExtensions
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger();

        public static void TypeByJs(this IWebDriver driver, string cssSelector, string text)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"document.querySelector('{cssSelector}').value ='{text}';");
        }

        public static void ClickByJs(this IWebDriver driver, string cssSelector)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"document.querySelector('{cssSelector}').click();");
        }

        public static void SetUpCoursorByJs(this IWebDriver driver, string cssSelector)
        {
            var js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"document.querySelector('{cssSelector}').focus();");
            js.ExecuteScript($"document.querySelector('{cssSelector}').blur();");
        }
    }
}
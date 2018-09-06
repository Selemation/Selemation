using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using Selemation.Driver.Configuration;
using Selemation.DriverExtensions;
using Selemation.Tools.Logger;
using Serilog;

namespace Selemation.Driver
{
    public class DriverContainer : IDisposable
    {
        public IWebDriver Driver;
        private readonly WebDriverOptions _webDriverOptions;
        protected readonly ILogger Logger = LoggerFactory.GetLogger();

        public DriverContainer(WebDriverOptions webDriverOptions)
        {
            _webDriverOptions = webDriverOptions;
            Logger.Information($"Inited Fixture Driver: {_webDriverOptions.DriverType}, host: {_webDriverOptions.SeleniumServer}");

            InitializeDriver();
        }

        private void InitializeDriver()
        {
            switch (_webDriverOptions.DriverType)
            {
                case DriverType.RemoteDriver:

                    CreateRemoteDriver();
                    break;

                case DriverType.LocalChromeDriver:

                    Assembly assembly = typeof(DriverContainer).Assembly;
                    string directoryName = Path.GetDirectoryName(assembly.Location);
                    Driver = new ChromeDriver(directoryName);
                    Driver.Manage().Window.Maximize();
                    break;
            }

            Driver.Manage().Cookies.DeleteAllCookies();
        }

        private void CreateRemoteDriver()
        {
            var commandExecutor = new AuthorizedHttpCommandExecutor(
                new Uri(_webDriverOptions.SeleniumServer), TimeSpan.FromSeconds(60),
                useBasicAuth: true);

            bool hasCap = false;
            var caps = new DesiredCapabilities();

            if (_webDriverOptions.Capabilities != null && _webDriverOptions.Capabilities.Any())
            {
                hasCap = true;
                foreach (var capability in _webDriverOptions.Capabilities)
                {
                    caps.SetCapability(capability.Key, capability.Value);
                }
            }

            if (!string.IsNullOrEmpty(_webDriverOptions.PassBuildToCapabilitiesFromEnvVariable))
            {
                var buildNameVariable =
                    Environment.GetEnvironmentVariable(_webDriverOptions.PassBuildToCapabilitiesFromEnvVariable);
                if (!string.IsNullOrEmpty(buildNameVariable))
                {
                    hasCap = true;
                    caps.SetCapability("build", buildNameVariable);
                }
            }

            if (!hasCap)
            {
                Driver = new RemoteWebDriver(commandExecutor, new ChromeOptions().ToCapabilities());
            }
            else
            {
                Driver = new RemoteWebDriver(commandExecutor, caps);
            }

            Driver.Manage().Window.Size = new Size(1280, 2024);
        }

        public void ErrorScrenshoot()
        {
            Driver.TakeScreenshot("Error");
        }

        public void Dispose()
        {
            try
            {
                Driver.Quit();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser 
            }
        }

        public void ErrorScreenshotWithNum(int val)
        {
            Driver.TakeScreenshot($"ErrorAttempt#{val}_for");
        }
    }
}
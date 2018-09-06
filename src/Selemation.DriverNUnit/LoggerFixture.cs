using NUnit.Framework;
using Selemation.Driver.Configuration;
using Selemation.Tools;
using Selemation.Tools.Logger;
using Serilog;
using Serilog.Core;

namespace Selemation.DriverNUnit
{
    /// <summary>
    /// Fixture just for initialize logger
    /// </summary>
    public abstract class LoggerFixture
    {
        protected readonly ILogger Logger = LoggerFactory.GetLogger();
        protected readonly WebDriverOptions WebDriverOptions;

        protected LoggerFixture()
        {
            WebDriverOptions = ConfigBuilder<WebDriverOptions>.GetConfig("driver");
        }
      

        [OneTimeTearDown]
        public void LoggerTearDown()
        {
            Log.CloseAndFlush();
            (Logger as Logger).Dispose();
        }
    }
}
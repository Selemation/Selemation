using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using Selemation.Driver;

namespace Selemation.DriverNUnit
{
    /// <summary>
    /// Fixture for run all test in one selenium driver instance. 
    /// Had Prepare method for setting up initial conditions
    /// </summary>
    public abstract class SharedDriverFixture: LoggerFixture, IDisposable
    {
        protected readonly DriverContainer DriverContainer;
        protected readonly IWebDriver Driver;
        protected SharedDriverFixture()
        {
            DriverContainer = new DriverContainer(WebDriverOptions);
            Driver = DriverContainer.Driver;
            Logger.Information("----------------- Start preparation -----------------");
            PrepareInner();
            Logger.Information("----------------- End preparation -----------------");
        }

        [TearDown]
        public void TearDown()
        {
            if (!Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Success))
            {
                try
                {
                    DriverContainer.ErrorScrenshoot();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error was occured during TearDown Screenshoot");
                }
            }
        }

        public void FinalTearDown()
        {
            DriverContainer.Dispose();
        }

        /// <summary>
        /// Proxi method to avoid warning "virtual mehtod in constructor"
        /// </summary>
        private void PrepareInner()
        {
            Prepare();
        }
        
        /// <summary>
        /// You should override it in you need some specific preconditions
        /// </summary>
        public abstract void Prepare();

        public void Dispose()
        {
            FinalTearDown();
        }
    }
}
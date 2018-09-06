using System;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using Selemation.Driver;

namespace Selemation.DriverNUnit
{
    /// <summary>
    /// Fixture that provide unique selenium instance for each test case
    /// </summary>
    public abstract class ExclusiveDriverFixture: LoggerFixture
    {
        protected DriverContainer DriverContainer;
        protected IWebDriver Driver;

        [SetUp]
        public void SetUp()
        {
            DriverContainer = new DriverContainer(WebDriverOptions);
            Driver = DriverContainer.Driver;
        }

        [TearDown]
        public void TearDown()
        {
            if(!Equals(TestContext.CurrentContext.Result.Outcome, ResultState.Success))
            {
                try
                {
                    DriverContainer.ErrorScrenshoot();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error was occured during TearDown");
                }
            }
            DriverContainer.Dispose();
        }
    }
}
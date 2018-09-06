using NUnit.Framework;
using Selemation.DriverExtensions;
using Selemation.DriverNUnit;

namespace Selemation.TestExamples
{
    public class SimpleTest : ExclusiveDriverFixture
    {
        [Test]
        public void Test()
        {
            string url = "https://www.google.com/";
            Logger.Information($"current url: {url}");
            Driver.GoToUrl(url);
            
            Logger.Information("Url opened");
        }
    }
}
using System.Collections.Generic;

namespace Selemation.Driver.Configuration
{
    public class WebDriverOptions
    {
        public DriverType DriverType { get; set; }

        public string SeleniumServer { get; set; }

        public Dictionary<string, string> Capabilities { get; set; }

        public string PassBuildToCapabilitiesFromEnvVariable { get; set; }
    }
}

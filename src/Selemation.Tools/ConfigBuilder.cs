using System;
using Microsoft.Extensions.Configuration;

namespace Selemation.Tools
{
    public class ConfigBuilder<T> where T : new()
    {
        private static readonly object Lock = new object();

        private static T Config { get; set; }

        public static T GetConfig(string name)
        {
            lock (Lock)
            {
                if (Config == null)
                {
                    Config = ReadConfig(name);
                }
            }

            return Config;
        }

        private static T ReadConfig(string name) 
        {
            string env = Environment.GetEnvironmentVariable("NET_ENVIRONMENT") ?? "local";
            var config = new T();
            new ConfigurationBuilder()
                .AddJsonFile($"Settings/{name}.json", false)
                .AddJsonFile($"Settings/{name}.{env}.json", true)
                .Build()
                .Bind(config);

            return config;
        }
    }
}
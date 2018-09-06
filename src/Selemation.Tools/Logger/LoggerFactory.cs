using System;
using System.IO;
using Serilog;

namespace Selemation.Tools.Logger
{
    public class LoggerFactory
    {
        private static ILogger SeriLogger { get; set; }

        private static readonly object Lock = new object();

        public static ILogger GetLogger()
        {
            lock (Lock)
            {
                if (SeriLogger == null)
                {
                    SeriLogger = CreateSeriLogger();
                }
            }

            return SeriLogger;
        }

        private static Serilog.Core.Logger CreateSeriLogger()
        {
            SerilogOptions options = ConfigBuilder<SerilogOptions>.GetConfig("serilog");

            string currentPath = Path.GetDirectoryName(typeof(LoggerFactory).Assembly.Location) ?? "";
            var folderPath = Path.Combine(currentPath, "..\\..\\..\\..\\..\\");
            string env = Environment.GetEnvironmentVariable("NET_ENVIRONMENT") ?? "development";

            var builer = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.RollingFile(pathFormat: $"{folderPath}/logs/roll/log-{{Date}}.txt",
                    flushToDiskInterval: new TimeSpan(0, 0, 0, 500))
                .WriteTo.File(path: $"{folderPath}/logs/log.txt",
                    flushToDiskInterval: new TimeSpan(0, 0, 0, 500));

            if (!string.IsNullOrEmpty(options.SeqReader))
            {
                builer.WriteTo.Seq(serverUrl: options.SeqReader, period: TimeSpan.Zero);
            }

            var serilog = builer
                .WriteTo.ColoredConsole()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithProperty("Application", options.AppName)
                .Enrich.WithProperty("SessionId", Guid.NewGuid())
                .Enrich.WithProperty("Environment", env)
                .CreateLogger();

            return serilog;
        }
    }
}
using System;
using Selemation.Tools.Logger;
using Serilog;

namespace Selemation.Tools
{
    public class AttemptManager
    {
        protected static readonly ILogger Logger = LoggerFactory.GetLogger();

        public static void Attempt(Action action, int attemptCount = 3)
        {
            int attempt = 0;
            bool isDone = false;
            while (!isDone && attempt <= attemptCount)
            {
                try
                {
                    action();
                    isDone = true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    isDone = false;
                    Logger.Error(ex, $"Error on attempt number {attempt}");
                }
            }

            if (!isDone)
            {
                throw new Exception("Attempts limit is reached");
            }
        }

        public static void SafeTry(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
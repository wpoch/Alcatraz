using Alcatraz.Core.Log;

namespace Alcatraz.Core.Categories
{
    public class LogCategorizer
    {
        public static LogMessage Categorize(LogMessage log)
        {
            const string HOST_KEY = "log4net:HostName";

            if (log.Properties.ContainsKey(HOST_KEY))
            {
                var server = log.Properties[HOST_KEY];
                switch (log.Properties[HOST_KEY])
                {
                    case "WSFTYSDES002S":
                        server = "LOCAL";
                        break;                        
                    case "NBSFDESWEBSITE":
                    case "NBSF000DES01":
                        server = "DEV";
                        break;
                    case "NBSFVMWWEBSITE":                    
                        server = "TEST";
                        break;
                }
                log.Categories.Add(server);
            }
            return log;
        }
    }
}
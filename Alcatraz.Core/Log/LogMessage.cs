using System;
using System.Collections.Generic;

namespace Alcatraz.Core.Log
{
    [Serializable]
    public enum LogLevel
    {
        Trace = 2,
        Debug = 4,
        Info = 8,
        Warn = 16,
        Error = 32,
        Fatal = 64
    }
    
    public class LogMessage
    {
        public LogMessage()
        {
            Properties = new Dictionary<string, string>();
            Tags = new List<string>();
            Categories = new List<string>();
        }

        /// <summary>
        /// Logger Name.
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// Log Level.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Log Message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Thread Name.
        /// </summary>
        public string ThreadName { get; set; }

        /// <summary>
        /// Time Stamp.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// Properties collection.
        /// </summary>
        public Dictionary<string, string> Properties { get; set; }

        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }

        /// <summary>
        /// An exception message to associate to this message.
        /// </summary>
        public string ExceptionString { get; set; }
    }
}
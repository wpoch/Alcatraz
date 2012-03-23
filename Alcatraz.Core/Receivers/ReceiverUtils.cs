using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using Alcatraz.Core.Helpers;
using Alcatraz.Core.Log;

namespace Alcatraz.Core.Receivers
{
    public static class ReceiverUtils
    {
        private static readonly DateTime s1970 = new DateTime(1970, 1, 1);

        /// <summary>
        /// We can share settings to improve performance
        /// </summary>
        private static readonly XmlReaderSettings XmlSettings = CreateSettings();

        /// <summary>
        /// We can share parser context to improve performance
        /// </summary>
        private static readonly XmlParserContext XmlContext = CreateContext();

        public static string GetTypeDescription(Type type)
        {
            var attr = (DisplayNameAttribute) Attribute.GetCustomAttribute(type, typeof (DisplayNameAttribute), true);
            return attr != null ? attr.DisplayName : type.ToString();
        }

        private static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings {CloseInput = false, ValidationType = ValidationType.None};
        }

        private static XmlParserContext CreateContext()
        {
            var nt = new NameTable();
            var nsmanager = new XmlNamespaceManager(nt);
            nsmanager.AddNamespace("log4j", "http://jakarta.apache.org/log4j/");
            return new XmlParserContext(nt, nsmanager, "elem", XmlSpace.None, Encoding.UTF8);
        }

        /// <summary>
        /// Parse LOG4JXml from xml stream
        /// </summary>
        public static LogMessage ParseLog4JXmlLogEvent(Stream logStream, string defaultLogger)
        {
            // In case of ungraceful disconnect 
            // logStream is closed and XmlReader throws the exception,
            // which we handle in TcpReceiver
            using (XmlReader reader = XmlReader.Create(logStream, XmlSettings, XmlContext))
                return ParseLog4JXmlLogEvent(reader, defaultLogger);
        }

        /// <summary>
        /// Parse LOG4JXml from string
        /// </summary>
        public static LogMessage ParseLog4JXmlLogEvent(string logEvent, string defaultLogger)
        {
            try
            {
                using (var reader = new XmlTextReader(logEvent, XmlNodeType.Element, XmlContext))
                    return ParseLog4JXmlLogEvent(reader, defaultLogger);
            }
            catch (Exception e)
            {
                return new LogMessage
                           {
                               // Create a simple log message with some default values
                               LoggerName = defaultLogger,
                               ThreadName = "NA",
                               Message = logEvent,
                               TimeStamp = DateTime.Now,
                               Level = LogLevel.Info,
                               ExceptionString = e.Message
                           };
            }
        }

        /// <summary>
        /// Here we expect the log event to use the log4j schema.
        /// Sample:
        ///     <log4j:event logger="Statyk7.Another.Name.DummyManager" timestamp="1184286222308" level="ERROR" thread="1">
        ///         <log4j:message>This is an Message</log4j:message>
        ///         <log4j:properties>
        ///             <log4j:data name="log4jmachinename" value="remserver" />
        ///             <log4j:data name="log4net:HostName" value="remserver" />
        ///             <log4j:data name="log4net:UserName" value="REMSERVER\Statyk7" />
        ///             <log4j:data name="log4japp" value="Test.exe" />
        ///         </log4j:properties>
        ///     </log4j:event>
        /// </summary>
        /// 
        /// Implementation inspired from: http://geekswithblogs.net/kobush/archive/2006/04/20/75717.aspx
        /// 
        public static LogMessage ParseLog4JXmlLogEvent(XmlReader reader, string defaultLogger)
        {
            var logMsg = new LogMessage();

            reader.Read();
            if ((reader.MoveToContent() != XmlNodeType.Element) || (reader.Name != "log4j:event"))
                throw new Exception("The Log Event is not a valid log4j Xml block.");

            logMsg.LoggerName = reader.GetAttribute("logger");
            logMsg.Level = EnumExtensions.Parse<LogLevel>(reader.GetAttribute("level"));
            logMsg.ThreadName = reader.GetAttribute("thread");

            long timeStamp;
            if (long.TryParse(reader.GetAttribute("timestamp"), out timeStamp))
                logMsg.TimeStamp = s1970.AddMilliseconds(timeStamp).ToLocalTime();

            int eventDepth = reader.Depth;
            reader.Read();
            while (reader.Depth > eventDepth)
            {
                if (reader.MoveToContent() == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "log4j:message":
                            logMsg.Message = reader.ReadString();
                            break;

                        case "log4j:throwable":
                            logMsg.Message += Environment.NewLine + reader.ReadString();
                            break;

                        case "log4j:locationInfo":
                            break;

                        case "log4j:properties":
                            reader.Read();
                            while (reader.MoveToContent() == XmlNodeType.Element
                                   && reader.Name == "log4j:data")
                            {
                                string name = reader.GetAttribute("name");
                                string value = reader.GetAttribute("value");
                                logMsg.Properties[name] = value;
                                reader.Read();
                            }
                            break;
                    }
                }
                reader.Read();
            }

            return logMsg;
        }
    }
}
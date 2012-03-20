using System.Diagnostics;
using log4net;

namespace Alcatraz.Core.Adapters
{
    public class Log4NetTraceListener : TextWriterTraceListener 
    {
        public ILog Log = LogManager.GetLogger(typeof(Log4NetTraceListener));

        public override void Write(string message)
        {
            Log.Debug(message);
        }

        public override void WriteLine(string message)
        {
            Log.Debug(message);
        }
    }
}
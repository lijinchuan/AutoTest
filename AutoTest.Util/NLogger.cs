using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest.Util
{
    public class NLogger : ILogger
    {
        private readonly Logger log;

        public NLogger(Type type)
        {
            log = LogManager.GetLogger(type.FullName);
        }

        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log(LogLevel.Warn, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public void Error(Exception ex)
        {
            Log(LogLevel.Error, null, null, ex);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Error, format, args, ex);
        }

        public void Fatal(Exception ex, string format, params object[] args)
        {
            Log(LogLevel.Fatal, format, args, ex);
        }

        private void Log(LogLevel level, string format, object[] args)
        {
            log.Log(typeof(NLogger), new LogEventInfo(level, log.Name, null, format, args));
        }

        private void Log(LogLevel level, string format, object[] args, Exception ex)
        {
            log.Log(typeof(NLogger), new LogEventInfo(level, log.Name, null, format, args, ex));
        }
    }
}

using log4net;
using log4net.Config;

using System;
using System.Xml;

namespace Arrba.Parser.Logger
{
    public class LogService : ILogService
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LogService));
        public LogService()
        {
            var xml = @"<log4net>
                        <appender name=""PapertrailRemoteSyslogAppender"" type=""log4net.Appender.RemoteSyslogAppender"">
                          <facility value=""Local6"" />
                          <identity value=""%date{yyyy-MM-ddTHH:mm:ss.ffffffzzz} %P{log4net:HostName} Arrba.Parser"" />
                          <layout type=""log4net.Layout.PatternLayout"" value=""%level - %message%newline"" />
                          <remoteAddress value=""logs4.papertrailapp.com"" />
                          <remotePort value=""26303"" />
                        </appender>

                        <appender name=""AnsiColorTerminalAppender"" type=""log4net.Appender.AnsiColorTerminalAppender"">
                            <mapping>
                            <level value=""INFO"" />
                            <forecolor value=""Green"" />
                            </mapping>
                            <mapping>
                            <level value=""ERROR"" />
                            <forecolor value=""Red"" />
                            </mapping>
                            <mapping>
                            <level value=""DEBUG"" />
                            <forecolor value=""Yellow"" />
                            </mapping>
                            <layout type=""log4net.Layout.PatternLayout"">
                            <conversionpattern value=""%date [%thread] %-5level - %message%newline"" />
                            </layout>
                        </appender>
                        <root>
                          <level value=""ALL"" />
                          <appender-ref ref=""AnsiColorTerminalAppender"" />
                          <appender-ref ref=""PapertrailRemoteSyslogAppender"" />

                        </root>
                      </log4net>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            var repo = LogManager.CreateRepository(
               System.Reflection.Assembly.GetEntryAssembly(),
               typeof(log4net.Repository.Hierarchy.Hierarchy)
            );

            XmlConfigurator.Configure(repo, doc.DocumentElement);
        }

        public void Info(string msg)
        {
            _log.Info(msg);
        }

        public void Info(string msg, Exception ex)
        {
            _log.Info(msg, ex);
        }

        public void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public void Debug(string msg, Exception ex)
        {
            _log.Debug(msg, ex);
        }

        public void Warn(string msg)
        {
            _log.Warn(msg);
        }

        public void Warn(string msg, Exception ex)
        {
            _log.Warn(msg, ex);
        }

        public void Error(string msg)
        {
            _log.Error(msg);
        }

        public void Error(string msg, Exception ex)
        {
            _log.Error(msg, ex);
        }
    }
}

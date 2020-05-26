using System.IO;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace SparqlForHumans.Logger
{
    public class Logger
    {
        public static NLog.Logger Init()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget
            {
                Name = "logfile",
                FileName = Path.Combine("${specialfolder:UserProfile}", "SparqlForHumans","Logs", "Log ${date:format=yyyy-MM-dd}.log"),
                ArchiveAboveSize = 5242880,
                Layout = new CsvLayout
                {
                    Columns =
                    {
                        new CsvColumn("Time", "${longdate}"),
                        new CsvColumn("Severity", "${uppercase:${level}}"),
                        new CsvColumn("Memory", "${gc:property=TotalMemory}"),
                        new CsvColumn("Location",
                            "${callsite:className=True:fileName=True:includeSourcePath=False:methodName=True}"),
                        new CsvColumn("Detail", "${message}"),
                        new CsvColumn("Exception", "${exception:format=ToString}")
                    }
                }
            };
            config.AddTarget(fileTarget);

            var consoleTarget = new ConsoleTarget("logconsole")
            {
                Layout = "${longdate} ${uppercase:${level}} ${gc:property=TotalMemory} ${message}"
            };
            config.AddTarget(consoleTarget);

            // Step 3. Define rules
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

            // Step 4. Activate the configuration
            LogManager.Configuration = config;

            return LogManager.GetCurrentClassLogger();
        }
    }
}
using NLog;
using System;

using Core;

namespace project
{
    class Program
    {
        static void OtherMain(string[] args)
        {
            ushort value = 0;
            ushort returnv = 0;
            RegisterPC PC = new RegisterPC();

            setupLogger();
            Console.WriteLine("Hello World!");

            value = 256;
            PC.setValue(value);
            returnv = PC.getValue();

            value = 65000;
            PC.setValue(value);
            returnv = PC.getValue();

            value = 257;
            PC.setValue(value);
            returnv = PC.getValue();
        }

        static void setupLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "log.log" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
                        
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logconsole);
                        
            NLog.LogManager.Configuration = config;
        }
    }
}

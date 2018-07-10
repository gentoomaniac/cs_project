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
            CPU6510 cpu = new CPU6510();

            setupLogger();
            Console.WriteLine("Hello World!");

            value = 256;
            cpu.PC = value;
            returnv = cpu.PC;

            value = 65000;
            cpu.PC = value;
            returnv = cpu.PC;

            value = 257;
            cpu.PC = value;
            returnv = cpu.PC;
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

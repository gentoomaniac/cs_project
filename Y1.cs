using System.Threading;

using NLog;

namespace Y1
{
    class SystemClock
    {
        private Thread systemClockThread;
        private int systemClockRate;
        private Mutex cpuMutex;

        private bool doRun;

        private Logger log;

        public SystemClock(Mutex cpuMutex)
        {
            log = log = NLog.LogManager.GetCurrentClassLogger();
            doRun = true;

            this.cpuMutex = cpuMutex;
            log.Debug("initially locking cpu ...");
            this.cpuMutex.WaitOne();
        }

        public void start()
        {
            log.Debug("starting system clock thread ...");
            ThreadStart systemClock = new ThreadStart(systemClockRunner);
            systemClockThread = new Thread(systemClock);
            systemClockThread.Start();
        }

        public void resume()
        {
            doRun = true;
            systemClockThread.Start();
        }

        public void suspend()
        {
            systemClockThread.Suspend();
        }

        public void halt(bool blocking=true)
        {
            doRun = false;
            if (blocking)
                systemClockThread.Join();
            log.Debug("system clock halted.");
        }

        public void cleanup()
        {
            cpuMutex.ReleaseMutex();
            log.Debug("system clock stopped.");
        }

        private void systemClockRunner()
        {
            log.Debug("... system clock started");
            while(doRun){
                Thread.Sleep(100);  // ToDo: this is just a placeholder
                log.Debug("SystemClock tick:");
                cpuMutex.ReleaseMutex();
                log.Debug("- cpu mutex released");
                cpuMutex.WaitOne();
                log.Debug("- cpu mutex acquired");
            }
        }
    }
}
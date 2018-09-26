using System.Threading;

using NLog;

using CycleLock;

namespace Y1
{
    class SystemClock
    {
        private Thread systemClockThread;
        private int systemClockRate;
        private Lock cpuLock;

        private bool doRun;

        private Logger log;

        public SystemClock(Lock cpuLock)
        {
            log = log = NLog.LogManager.GetCurrentClassLogger();
            doRun = true;

            this.cpuLock = cpuLock;
            log.Debug("initially locking cpu ...");
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

        public void halt(bool blocking=true)
        {
            doRun = false;
            if (blocking)
                systemClockThread.Join();
            log.Debug("system clock halted.");
        }

        public void cleanup()
        {
            log.Debug("system clock stopped.");
        }

        private void systemClockRunner()
        {
            log.Debug("... system clock started");
            while(doRun){
                Thread.Sleep(100);  // ToDo: this is just a placeholder
                cpuLock.startCycle();
            }
        }
    }
}
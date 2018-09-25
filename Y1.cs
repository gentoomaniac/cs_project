using System.Threading;

using NLog;

namespace Y1
{
    class SystemClock
    {
        private Thread systemClockThread;
        private int systemClockRate;
        private AutoResetEvent cpuCycleUnlockEvent;

        private bool doRun;

        private Logger log;

        public SystemClock(AutoResetEvent cpuCycleUnlockEvent)
        {
            log = log = NLog.LogManager.GetCurrentClassLogger();
            doRun = true;

            this.cpuCycleUnlockEvent = cpuCycleUnlockEvent;
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
                log.Debug("SystemClock tick:");
                log.Debug("... cpu tick");
                releaseAndWait(cpuCycleUnlockEvent);
                log.Debug("... cpu tack");
            }
        }

        private void releaseAndWait(AutoResetEvent e)
        {
            e.Set();        // release component
            //e.Reset();      // reset event status
            e.WaitOne();    // block on event until component finishes cycle
            //e.Reset();      // reset event status
        }
    }
}
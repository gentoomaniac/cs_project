using System.Threading;

namespace CycleLock
{
    class Lock
    {
        protected int CycleCount;
        public Lock() {CycleCount = 0;}
        public void startCycle() {}
        public void enterCycle() {CycleCount++;}
        public void exitCycle() {}
        public void WaitOne() {}
        public int getCycleCount() {return CycleCount;}
        public void resetCycleCount() {CycleCount = 0;}
    }

    class AutoResetLock : Lock
    {
        private AutoResetEvent e;
        public AutoResetLock() {e = new AutoResetEvent(false);}
        new public void startCycle() {e.Set(); e.WaitOne(); base.startCycle();}

        new public void enterCycle() {e.WaitOne(); base.enterCycle();}
        
        new public void exitCycle() {e.Set(); base.exitCycle();}
        
        new public void WaitOne() {e.WaitOne(); base.WaitOne();}
    }

    class AlwaysOpenLock : Lock{ }
}
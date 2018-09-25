using System.Threading;

namespace CycleLock
{
    class Lock
    {
        private AutoResetEvent e;
        private int CycleCount;

        public Lock()
        {
            CycleCount = 0;
            e = new AutoResetEvent(false);
        }

        public void startCycle() {e.Set(); e.WaitOne();}

        public void enterCycle() {e.WaitOne(); CycleCount++;}
        
        public void exitCycle() {e.Set();}
        
        public void WaitOne() {e.WaitOne();}
        
        public int getCycleCount() {return CycleCount;}
        public void resetCycleCount() {CycleCount = 0;}
    }
}
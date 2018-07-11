using NLog;

namespace MOS
{
    class CPU6510
    {
        public ushort PC;
        public byte PCH
        {
            get { return (byte)(PC>>8);}
            set { PC = (ushort)((value<<8) | PCL); }
        }
        public byte PCL
        {
            get { return (byte)(PC&0x00ff);}
            set { PC = (ushort)((PC&0x00) | value); }
        }
        public byte S;
        public byte P;
        public byte A;
        public byte X;
        public byte Y;
        
        private Logger log;

        public CPU6510()
        {
            log = NLog.LogManager.GetCurrentClassLogger();
        }
    }
}
using NLog;

namespace Core
{
    class CPU6510
    {
        public ushort PC;
        public byte PCH
        {
            get { return (byte)(PC>>8);}
            set
            {
                ushort newPC = (ushort)(value<<8);
                newPC = (ushort)(newPC|PCL);
                PC = newPC;
            }
        }
        public byte PCL
        {
            get{ return (byte)(PC&0x00ff);}
            set
            {
                ushort newPC = (ushort)(PC&0x00);
                newPC = (ushort)(newPC|value);
                PC = newPC;
            }
        }
        public byte S;
        public byte P;
        public byte A;
        public byte X;
        public byte Y;
        
        private Logger logger;

        public CPU6510()
        {
            logger = NLog.LogManager.GetCurrentClassLogger();
        }
    }
}
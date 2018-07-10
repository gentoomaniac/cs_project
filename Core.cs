using NLog;

namespace Core
{
    class CPU6510
    {
        private RegisterPC PC;
        private Register8 S;
        private Register8 P;
        private Register8 A;
        private Register8 X;
        private Register8 Y;
        
    }

    class Register8
    {
        private byte value = 0;

        public byte getValue(){return this.value;}
        public void setValue(byte v){this.value = v;}
    }

    class RegisterPC
    {
        public Register8 PCH;
        public Register8 PCL;

        private Logger logger;

        public RegisterPC() {
            PCH = new Register8();
            PCL = new Register8();
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        public ushort getValue()
        {
            ushort value = PCH.getValue();
            value = (ushort)(value<<8);
            value = (ushort)(value | PCL.getValue());
            this.logger.Debug(string.Format("PC constructed value: {0}", value));
            return value;
        }
        public void setValue(ushort v)
        {
            PCH.setValue((byte)(v>>8));
            PCL.setValue((byte)(v));
            this.logger.Debug(string.Format("PC updated. PCH: {0}\tPCL: {1}", PCH.getValue(), PCL.getValue()));
        }
        
    }
}
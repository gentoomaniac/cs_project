using NLog;

namespace MOS
{
    public enum ProcessorStatus {
        N = 0x80, // Negative
        V = 0x40, // Overflow
        X = 0x20, // unused
        B = 0x10, // Break
        D = 0x08, // Decimal mode
        I = 0x04, // Interrupt disabled
        Z = 0x02, // Zero
        C = 0x01, // Carry
    }

    class CPU6510
    {
        /*  Program Counter

            This register points the address from which the next
            instruction byte (opcode or parameter) will be fetched.
            Unlike other registers, this one is 16 bits in length. The
            low and high 8-bit halves of the register are called PCL
            and PCH, respectively.

            The Program Counter may be read by pushing its value on
            the stack. This can be done either by jumping to a
            subroutine or by causing an interrupt. */
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

        /*  Stack pointer

            The NMOS 65xx processors have 256 bytes of stack memory,
            ranging from $0100 to $01FF. The S register is a 8-bit
            offset to the stack page. In other words, whenever
            anything is being pushed on the stack, it will be stored
            to the address $0100+S.

            The Stack pointer can be read and written by transfering
            its value to or from the index register X (see below) with
            the TSX and TXS instructions. */
        public byte S;

        /*  Processor status

            This 8-bit register stores the state of the processor. The
            bits in this register are called flags. Most of the flags
            have something to do with arithmetic operations.

            The P register can be read by pushing it on the stack
            (with PHP or by causing an interrupt). If you only need to
            read one flag, you can use the branch instructions.
            Setting the flags is possible by pulling the P register
            from stack or by using the flag set or clear instructions. */
        public byte P;

        /*  Accumulator

            The accumulator is the main register for arithmetic and
            logic operations. Unlike the index registers X and Y, it
            has a direct connection to the Arithmetic and Logic Unit
            (ALU). This is why many operations are only available for
            the accumulator, not the index registers. */
        public byte A;

        /*  Index register X

            This is the main register for addressing data with
            indices. It has a special addressing mode, indexed
            indirect, which lets you to have a vector table on the
            zero page. */
        public byte X;

        /*  Index register Y

            The Y register has the least operations available. On the
            other hand, only it has the indirect indexed addressing
            mode that enables access to any memory place without
            having to use self-modifying code. */
        public byte Y;

        private byte[] memory; 

        private Logger log;

        private static ushort PAGE_SIZE;

        static CPU6510()
        {
            PAGE_SIZE = 256;
        }

        public CPU6510(byte[] memory)
        {
            log = NLog.LogManager.GetCurrentClassLogger();

            this.memory = memory;
        }

        /* Logical and arithmetic commands */
        // A | addr
        public void ORA(ushort address) {
            A |= memory[address];

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
        }
        // A & addr
        public void AND(ushort address) {
            A &= memory[address];

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
        }
        // A ^ addr
        public void EOR(ushort address) {
            A ^= memory[address];

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
        }
        // A + addr
        public void ADC(ushort address) {
            setProcessorStatusBit(ProcessorStatus.V, isSet:( ((A + memory[address]) & (byte)ProcessorStatus.N) != (A & (byte)ProcessorStatus.N) ));
            A += memory[address];
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
        }

        private void setProcessorStatusBit(ProcessorStatus s, bool isSet = true)
        {
            if (isSet)
                P = (byte)(P | (byte)s);
            else 
                P = (byte)(P & (byte)(0xff ^ (byte)s));
        }
    }
}
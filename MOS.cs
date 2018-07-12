using NLog;

using Exceptions;

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
        private static ushort PAGE_SIZE;

        static CPU6510()
        {
            PAGE_SIZE = 256;
        }

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

        public CPU6510(byte[] memory)
        {
            log = NLog.LogManager.GetCurrentClassLogger();

            this.memory = memory;
        }

        /* Maps opcodes to the actual commands and takes care of the different adressing modes
         */
        public void opcodeMapper(byte opcode)
        {
            switch(opcode)
            {
                case 0x05:
                    ORA(zeropageAdressing(getNextCodeByte()));
                    break;
                case 0x25:
                    AND(zeropageAdressing(getNextCodeByte()));
                    break;
                case 0x45:
                    EOR(zeropageAdressing(getNextCodeByte()));
                    break;

                default:
                    throw new IllegalOpcodeException(string.Format("{0} is an illigal Opcode", opcode.ToString("x2")));
            }
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


        /* HELPERS */
        private void setProcessorStatusBit(ProcessorStatus s, bool isSet = true)
        {
            if (isSet)
                P = (byte)(P | (byte)s);
            else 
                P = (byte)(P & (byte)(0xff ^ (byte)s));
        }

        private ushort getWordFromMemory(ushort addr)
        {
            ushort word = (ushort)(((ushort)memory[addr+1]) << 8);
            return (ushort)(word | (ushort)memory[addr]);
        }
        private ushort getWordFromZeropage(byte addr)
        {
            return getWordFromMemory(addr);
        }
        /* get the next code byte from memory and increment PC */
        private byte getNextCodeByte() {return memory[PC++];}
        /* get the next code word from memory and increment PC accordingly */
        private ushort getNextCodeWord()
        {
            ushort word = getWordFromMemory(PC);
            PC += 2;
            return word;
        }

        /* Methods to resolve different adressing modes
         */
        // ToDo: The 6502 bugs
        // addressing, which is rather a "no addressing mode at all"-option: Instructions which do not address an arbitrary memory location only supports this mode.
        private ushort impliedAdressing(ushort addr) {return 0;}
        // addressing, supported by bit-shifting instructions, turns the "action" of the operation towards the accumulator.
        //ToDo:
        private ushort accumulatorAdressing(){return 0;}
        // addressing, which refers to the byte immediately following the opcode for the instruction.
        // ToDo: this is prob incorrect
        private ushort immidiateAdressing(ushort addr) {return (ushort)(addr + 1);}
        // addressing, which refers to a given 16-bit address
        private ushort absoluteAdressing(ushort addr) {return addr;}
        // absolute addressing, indexed by either the X and Y index registers: These adds the index register to a base address, forming the final "destination" for the operation.
        private ushort indexedAdressing(ushort addr, byte offset) {return (ushort)(addr + offset);}
        // addressing, which is similar to absolute addressing, but only works on addresses within the zeropage.
        private ushort zeropageAdressing(byte addr) {return (ushort)addr;}
        // Effective address is zero page address plus the contents of the given register (X, or Y).
        private ushort zeropageIndexedAdressing(byte addr, byte offset) {return (ushort)(addr + offset);}
        // addressing, which uses a single byte to specify the destination of conditional branches ("jumps") within 128 bytes of where the branching instruction resides.
        private ushort relativeAdressing(ushort addr, byte offset) {return (ushort)(addr + offset);}
        // addressing, which takes the content of a vector as its destination address.
        private ushort absoluteIndirectAdressing(ushort addr) {return getWordFromMemory(addr);}
        // addressing, which uses the X index register to select one of a range of vectors in zeropage and takes the address from that pointer. Extremely rarely used!
        private ushort indexedIndirectAdressing() {return getWordFromZeropage(X);}
        // addressing, which adds the Y index register to the contents of a pointer to obtain the address. Very flexible instruction found in anything but the most trivial machine language routines!
        private ushort indirectIndexedAdressing(ushort addr){return getWordFromMemory((ushort)(getWordFromMemory(addr) + Y));}
    }
}
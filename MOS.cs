using System.Threading;

using NLog;

using CycleLock;
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


    // https://en.wikipedia.org/wiki/Endianness#Little-endian
    class CPU6510
    {
        public static ushort PAGE_SIZE = 256;
        public static ushort STACK_OFFSET = 0x0100;

        public static ushort NMI_VECTOR = 0xfffa;
        public static ushort RESET_VECTOR = 0xfffc;
        public static ushort IRQ_VECTOR = 0xfffe;

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
            get { return (byte)(PC>>8); }
            set { PC = (ushort)((value<<8) | PCL); }
        }
        public byte PCL
        {
            get { return (byte)(PC&0x00ff); }
            set { PC = (ushort)((PC&0xff00) | value); }
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
        public byte S = 0xff;

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
        private Lock cycleLock;

        private Thread executionLoopThread;
        private bool exitExecutionLoop;

        private Logger log;

        public CPU6510(byte[] memory, Lock cyckleLock)
        {
            log = NLog.LogManager.GetCurrentClassLogger();
            exitExecutionLoop = false;

            this.memory = memory;
            this.cycleLock = cyckleLock;
        }

        public void start()
        {
            log.Debug("starting execution loop thread ...");
            ThreadStart execChild = new ThreadStart(executionLoop);
            executionLoopThread = new Thread(execChild);
            executionLoopThread.Start();
        }

        public void stop(bool blocking=true)
        {
            log.Debug("stopping execution loop thread ...");
            exitExecutionLoop = true;
            if (blocking)
                executionLoopThread.Join();
            log.Debug("execution loop stopped.");
        }

        public void join() {executionLoopThread.Join();}

        /* Maps opcodes to the actual commands and takes care of the different adressing modes
         */
        public void opcodeMapper(byte opcode)
        {
            switch(opcode)
            {
                case 0x01: ORA(indexedIndirectAdressing(getNextCodeByte())); break;
                case 0x05: ORA(zeropageAdressing(getNextCodeByte())); break;
                case 0x08: PHP(); break;
                case 0x11: ORA(indirectIndexedZeropageAdressing(getNextCodeByte())); break;
                case 0x15: ORA(zeropageIndexedAdressing(getNextCodeByte(), X)); break;
                case 0x25: AND(zeropageAdressing(getNextCodeByte())); break;
                case 0x28: PLP(); break;
                case 0x45: EOR(zeropageAdressing(getNextCodeByte())); break;
                case 0x48: PHA(); break;
                case 0x68: PLA(); break;
                case 0x85: STA(zeropageAdressing(getNextCodeByte())); break;
                case 0x8a: TXA(); break;
                case 0x8d: STA(absoluteAdressing(getNextCodeWord())); break;
                case 0x98: TYA(); break;
                case 0x9a: TXS(); break;
                case 0xa2: LDX(PC); break;
                case 0xa8: TAY(); break;
                case 0xaa: TAX(); break;
                case 0xba: TSX(); break;
                case 0xea: NOP(); break;
                default:
                    log.Debug(string.Format("0x{0} is an unimplemented Opcode", opcode.ToString("x2")));
                    //throw new IllegalOpcodeException(string.Format("0x{0} is an illigal Opcode", opcode.ToString("x2")));
                    break;
            }
            log.Debug(string.Format("0x{0} took {1} cycles", opcode.ToString("x2"), cycleLock.getCycleCount()));
            cycleLock.resetCycleCount();
        }

        /* ************************************************ */
        /* * Logical and arithmetic commands              * */
        /* ************************************************ */
        // A | addr
        public void ORA(ushort address) {
            cycleLock.enterCycle();
            A |= getByteFromMemory(address, lockToCycle:false);

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }
        // A & addr
        public void AND(ushort address) {
            cycleLock.enterCycle();
            A &= getByteFromMemory(address, lockToCycle:false);

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }
        // A ^ addr
        public void EOR(ushort address) {
            cycleLock.enterCycle();
            A ^= getByteFromMemory(address, lockToCycle:false);

            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }
        // A + addr
        public void ADC(ushort address) {
            cycleLock.enterCycle();
            byte oldA = A;
            byte memValue = getByteFromMemory(address, lockToCycle:false);
            A += memValue;
            setProcessorStatusBit(ProcessorStatus.V, isSet:(checkForOverflow(oldA, A)));
            setProcessorStatusBit(ProcessorStatus.C, isSet:(oldA + memValue > 0xff));
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));

            cycleLock.exitCycle();
        }

        // A - addr
        public void SBC(ushort address) {
            cycleLock.enterCycle();
            byte oldA = A;
            byte memValue = getByteFromMemory(address, lockToCycle:false);
            A -= memValue;
            setProcessorStatusBit(ProcessorStatus.V, isSet:(checkForOverflow(oldA, A)));
            setProcessorStatusBit(ProcessorStatus.C, isSet:(oldA - memValue < 0x00));
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));

            cycleLock.exitCycle();
        }

        public void CMP(ushort address) {
            cycleLock.enterCycle();
            byte memValue = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.C, isSet:(A - memValue < 0x00));
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(A == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((A & (byte)ProcessorStatus.N) != 0));

            cycleLock.exitCycle();
        }

        public void CPX(ushort address) {
            cycleLock.enterCycle();
            byte memValue = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.C, isSet:(X - memValue < 0x00));
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(X == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((X & (byte)ProcessorStatus.N) != 0));

            cycleLock.exitCycle();
        }

        public void CPY(ushort address) {
            cycleLock.enterCycle();
            byte memValue = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.C, isSet:(Y - memValue < 0x00));
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(Y == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((Y & (byte)ProcessorStatus.N) != 0));

            cycleLock.exitCycle();
        }

        public void DEC(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            memValue -= 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

         public void DEX() {
            cycleLock.enterCycle();
            X -= 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(X == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((X & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

         public void DEY() {
            cycleLock.enterCycle();
            Y -= 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(Y == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((Y & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void INC(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            memValue += 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

         public void INX() {
            cycleLock.enterCycle();
            X += 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(X == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((X & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

         public void INY() {
            cycleLock.enterCycle();
            Y += 1;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(Y == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((Y & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void ASL() {
            cycleLock.enterCycle();
            setProcessorStatusBit(ProcessorStatus.C, isSet:((A & (byte)ProcessorStatus.N) != 0));
            A = (byte)(A << 1);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(A == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((A & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void ASL(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            setProcessorStatusBit(ProcessorStatus.C, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            memValue = (byte)(memValue << 1);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

        public void ROL() {
            cycleLock.enterCycle();
            bool oldCarryflag = isProcessorStatusBitSet(ProcessorStatus.C);

            setProcessorStatusBit(ProcessorStatus.C, isSet:((A & (byte)ProcessorStatus.N) != 0));
            A = (byte)(A << 1);
            A = setBits(A, 0x01, set:oldCarryflag);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(A == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((A & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void ROL(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            bool oldCarryflag = isProcessorStatusBitSet(ProcessorStatus.C);

            setProcessorStatusBit(ProcessorStatus.C, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            memValue = (byte)(memValue << 1);
            memValue = setBits(memValue, 0x01, set:oldCarryflag);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

        public void LSR() {
            cycleLock.enterCycle();
            setProcessorStatusBit(ProcessorStatus.C, isSet:((A & 0x01) != 0));
            A = (byte)(A >> 1);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(A == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((A & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void LSR(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            setProcessorStatusBit(ProcessorStatus.C, isSet:((memValue & 0x01) != 0));
            memValue = (byte)(memValue >> 1);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

        public void ROR() {
            cycleLock.enterCycle();
            bool oldCarryflag = isProcessorStatusBitSet(ProcessorStatus.C);

            setProcessorStatusBit(ProcessorStatus.C, isSet:((A & 0x01) != 0));
            A = (byte)(A >> 1);
            A = setBits(A, 0x80, set:oldCarryflag);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(A == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((A & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();
        }

        public void ROR(ushort address) {
            byte memValue = getByteFromMemory(address, lockToCycle:true);

            cycleLock.enterCycle();
            bool oldCarryflag = isProcessorStatusBitSet(ProcessorStatus.C);

            setProcessorStatusBit(ProcessorStatus.C, isSet:((memValue & 0x01) != 0));
            memValue = (byte)(memValue >> 1);
            memValue = setBits(memValue, 0x80, set:oldCarryflag);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:(memValue == 0));
            setProcessorStatusBit(ProcessorStatus.N, isSet:((memValue & (byte)ProcessorStatus.N) != 0));
            cycleLock.exitCycle();

            storeByteInMemory(address, memValue, lockToCycle: true);
        }

        public void NOP()
        {
            cycleLock.enterCycle();
            cycleLock.exitCycle();
        }

        /* ************************************************ */
        /* * Move commands                                * */
        /* ************************************************ */
        public void LDA(ushort address)
        {
            cycleLock.enterCycle();
            A = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void STA(ushort address)
        {
            cycleLock.enterCycle();
            storeByteInMemory(address, A, lockToCycle:false);
            cycleLock.exitCycle();
        }

        public void STX(ushort address)
        {
            cycleLock.enterCycle();
            storeByteInMemory(address, X, lockToCycle:false);
            cycleLock.exitCycle();
        }

        public void LDX(ushort address)
        {
            cycleLock.enterCycle();
            X = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( X == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (X & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void LDY(ushort address)
        {
            cycleLock.enterCycle();
            Y = getByteFromMemory(address, lockToCycle:false);
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( Y == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (Y & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void STY(ushort address)
        {
            cycleLock.enterCycle();
            storeByteInMemory(address, Y, lockToCycle:false);
            cycleLock.exitCycle();
        }

        public void TAX()
        {
            cycleLock.enterCycle();
            X = A;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( X == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (X & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void TXA()
        {
            cycleLock.enterCycle();
            A = X;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void TAY()
        {
            cycleLock.enterCycle();
            Y = A;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( Y == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (Y & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void TYA()
        {
            cycleLock.enterCycle();
            A = Y;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void TSX()
        {
            cycleLock.enterCycle();
            X = S;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( X == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (X & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        public void TXS()
        {
            cycleLock.enterCycle();
            S = X;
            cycleLock.exitCycle();
        }

        // PLA (short for "PulL Accumulator") is the mnemonic for a machine language instruction which retrieves
        // a byte from the stack and stores it in the accumulator, and adjusts the stack pointer to reflect the removal of that byte.
        public void PLA()
        {
            byte value = getByteFromMemory((ushort)(STACK_OFFSET + S), lockToCycle:true);
            S += 1;

            cycleLock.enterCycle();
            A = value;
            setProcessorStatusBit(ProcessorStatus.Z, isSet:( A == 0 ));
            setProcessorStatusBit(ProcessorStatus.N, isSet:( (A & (byte)ProcessorStatus.N) != 0 ));
            cycleLock.exitCycle();
        }

        // PHA (short for "PusH Accumulator") is the mnemonic for a machine language instruction which stores a copy of the current
        // content of the accumulator onto the stack, and adjusting the stack pointer to reflect the addition.
        public void PHA()
        {
            cycleLock.enterCycle();
            storeByteInMemory((ushort)(STACK_OFFSET + S), A, lockToCycle:false);
            S--;
            cycleLock.exitCycle();
        }

        // PLP (short for "PulL Processor flags") is the mnemonic for a machine language instruction which retrieves a set of status
        // flags previously "pushed" onto the stack (usually by a PHP instruction) from the stack, and adjusting the stack pointer
        // to reflect the removal of a byte.
        public void PLP()
        {
            byte value = getByteFromMemory((ushort)(STACK_OFFSET + S), lockToCycle:true);
            S += 1;

            cycleLock.enterCycle();
            P = value;
            cycleLock.exitCycle();
        }

        // PHP (short for "PusH Processor flags") is the mnemonic for a machine language instruction which stores the current state
        // of the processor status flags onto the stack, and adjusting the stack pointer to reflect the addition.
        public void PHP()
        {
            cycleLock.enterCycle();
            storeByteInMemory((ushort)(STACK_OFFSET + S), P, lockToCycle:false);
            S--;
            cycleLock.exitCycle();
        }

        // BPL (short for "Branch if PLus") is the mnemonic for a machine language instruction which branches, or "jumps", to the address
        // specified if, and only if the negative flag is clear. If the netagive flag is set when the CPU encounters a BPL instruction,
        // the CPU will continue at the instruction following the BPL rather than taking the jump.
        public void BPL(sbyte offset)
        {
            if (!isProcessorStatusBitSet(ProcessorStatus.N))
                PC = (ushort)(PC + offset);
        }

        public void BMI(sbyte offset)
        {
            if (isProcessorStatusBitSet(ProcessorStatus.N))
                PC = (ushort)(PC + offset);
        }
        public void BVC(sbyte offset)
        {
            if (!isProcessorStatusBitSet(ProcessorStatus.V))
                PC = (ushort)(PC + offset);
        }
        public void BVS(sbyte offset)
        {
            if (isProcessorStatusBitSet(ProcessorStatus.V))
                PC = (ushort)(PC + offset);
        }
        public void BCC(sbyte offset)
        {
            if (!isProcessorStatusBitSet(ProcessorStatus.C))
                PC = (ushort)(PC + offset);
        }
        public void BCS(sbyte offset)
        {
            if (isProcessorStatusBitSet(ProcessorStatus.C))
                PC = (ushort)(PC + offset);
        }
        public void BNE(sbyte offset)
        {
            if (!isProcessorStatusBitSet(ProcessorStatus.Z))
                PC = (ushort)(PC + offset);
        }
        public void BEQ(sbyte offset)
        {
            if (isProcessorStatusBitSet(ProcessorStatus.Z))
                PC = (ushort)(PC + offset);
        }

        // BRK (short for "BReaKpoint") is the mnemonic for a machine language instruction which sets the break and interrupt
        // flags, increments the program counter by two and stores it along with the processor status flags onto the stack.
        // Finally it raises an IRQ interrupt event (load IRQ_VECTOR into PC).
        public void BRK()
        {

            setProcessorStatusBit(ProcessorStatus.B, isSet:true);
            setProcessorStatusBit(ProcessorStatus.I, isSet:true);

            PC += 2;

            push(PCH, lockToCycle:true);
            push(PCL, lockToCycle:true);
            push(P, lockToCycle:true);

            PC = getWordFromMemory(IRQ_VECTOR, (ushort)(IRQ_VECTOR+1));

        }

        // RTI (short for "ReTurn from Interrupt") is the mnemonic for a machine language instruction which returns the CPU
        // from an interrupt service routine to the "mainline" program that was interrupted. It does this by pulling first
        // the processor status flags (similar to PLP), and then the program counter, from the stack, effectively handling
        // program execution back to the address pulled from the stack.
        public void RTI()
        {
            P = pop(lockToCycle:true);
            PCL = pop(lockToCycle:true);
            PCH = pop(lockToCycle:true);
        }

        /* HELPERS */
        // Save byte to stack
        public void push(byte value, bool lockToCycle=true)
        {
            if (lockToCycle)
                cycleLock.enterCycle();
            storeByteInMemory((ushort)(STACK_OFFSET+S), value, lockToCycle:false);
            S--;
            if (lockToCycle)
                cycleLock.exitCycle();
        }
        public byte pop(bool lockToCycle=true)
        {
            if (lockToCycle)
                cycleLock.enterCycle();
            S++;
            byte value = getByteFromMemory((ushort)(STACK_OFFSET+S), lockToCycle:false);
            if (lockToCycle)
                cycleLock.exitCycle();

            return value;
        }

        public static bool checkForOverflow(byte vOld, byte vNew)
        {
            if ((vOld & 0x80) == 0 && (vNew & 0x80) !=0)
                return true;
            else if ((vOld & 0x80) != 0 && (vNew & 0x80) == 0)
                return true;
            return false;
        }

        //ToDo: check Enum and get rid of all the casting
        public void setProcessorStatusBit(ProcessorStatus s, bool isSet=true)
        {
            if (isSet)
                P = (byte)(P | (byte)s);
            else 
                P = (byte)(P & (byte)(0xff ^ (byte)s));
        }

        public static byte setBits(byte value, byte mask, bool set=true)
        {
            if (set)
                return (byte)(value | mask);
            else
                return (byte)(value & (byte)(0xff ^ mask));
        }

        public bool isProcessorStatusBitSet(ProcessorStatus s)
        {
            return (P & (byte)s) != 0;
        }

        private byte getByteFromMemory(ushort addr, bool lockToCycle=true)
        {
            byte b;
            if (lockToCycle)
                cycleLock.enterCycle();

            b = memory[addr];
            if (lockToCycle)
                cycleLock.exitCycle();
            log.Debug(string.Format("next byte loaded: {0}", b.ToString("x2")));
            return b;
        }

        private void storeByteInMemory(ushort addr, byte b, bool lockToCycle=true)
        {
            if (lockToCycle)
                cycleLock.enterCycle();

            memory[addr] = b;
            if (lockToCycle)
                cycleLock.exitCycle();
            log.Debug(string.Format("saved byte {0} to address {1}", b.ToString("x2"), addr.ToString("x4")));
        }

        private ushort getWordFromMemory(ushort lo, ushort hi)
        {
            ushort word = (ushort)(((ushort)getByteFromMemory(hi)) << 8);
            return (ushort)(word | (ushort)getByteFromMemory(lo));
        }
        private ushort getWordFromMemory(ushort addr, bool pageBoundry=false)
        {
            // if we ignore page boundries or the second byte is still on the same page
            if (!pageBoundry || (addr%PAGE_SIZE) < PAGE_SIZE-1)
                return getWordFromMemory(addr, (ushort)(addr+1));
            // otherwise take the pages 0x00 address for the high byte (see http://www.oxyron.de/html/opcodes02.html "The 6502 bugs")
            else
                return getWordFromMemory(addr, (ushort)((addr/(PAGE_SIZE-1)) << 8));
        }
        private ushort getWordFromZeropage(byte addr) {return getWordFromMemory(addr, pageBoundry:true);}

        /* get the next code byte/word from memory and increment PC */
        private byte getNextCodeByte() {return getByteFromMemory(PC++);}
        private ushort getNextCodeWord()
        {
            ushort word = getWordFromMemory(PC, (ushort)(PC+1));
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
        private ushort indexedIndirectAdressing(byte addr) {return getWordFromZeropage((byte)(addr+X));}
        // addressing, which adds the Y index register to the contents of a pointer to obtain the address. Very flexible instruction found in anything but the most trivial machine language routines!
        private ushort indirectIndexedAdressing(ushort addr, bool pageBoundry=false){
            return getWordFromMemory(
                (ushort)(getWordFromMemory(addr, pageBoundry:pageBoundry) + Y),
                pageBoundry:pageBoundry
            );
        }
        private byte indirectIndexedZeropageAdressing(byte addr){return (byte)(indirectIndexedAdressing(addr, pageBoundry:true));}

        private void executionLoop()
        {
            // https://www.pagetable.com/?p=410
            // load reset vector
            PC = getWordFromMemory(0xfffc);
            log.Debug(string.Format("loading reset vector took {0} cycles", cycleLock.getCycleCount()));
            cycleLock.resetCycleCount();

            // while(!exitExecutionLoop)
            for (int i = 0; i<10; i++)
            {
                opcodeMapper(getNextCodeByte());
            }
        }
    }
}
using System;

using NUnit.Framework;

using CycleLock;
using MOS;

namespace TestInstructions
{
   class InstructionTetsts
    {
        private int NUMBER_TEST_RUNS = 10000;

        [Test]
        public void testCheckForOverflow()
        {
            Random rnd = new Random();

            Assert.True(CPU6510.checkForOverflow(0x80, 0x7f));  // underflow
            Assert.True(CPU6510.checkForOverflow(0x7f, 0x80));  // overflow
            Assert.False(CPU6510.checkForOverflow(0x00, 0x00));
            Assert.False(CPU6510.checkForOverflow(0x80, 0x81));
            Assert.False(CPU6510.checkForOverflow(0x81, 0x80));
            Assert.False(CPU6510.checkForOverflow(0x7e, 0x7f));
            Assert.False(CPU6510.checkForOverflow(0x7f, 0x7e));

            // overflow
            for (int i = 0; i < 1000; i++){
                Assert.True(CPU6510.checkForOverflow((byte)rnd.Next(0x00,0x7f), (byte)rnd.Next(0x80, 0xff)));
            }
            // underflow
            for (int i = 0; i < 1000; i++){
                Assert.True(CPU6510.checkForOverflow((byte)rnd.Next(0x80,0xff), (byte)rnd.Next(0x00, 0x7f)));
            }

            for (int i = 0; i < 1000; i++){
                Assert.False(CPU6510.checkForOverflow((byte)rnd.Next(0x00,0x7f), (byte)rnd.Next(0x00, 0x7f)));
                Assert.False(CPU6510.checkForOverflow((byte)rnd.Next(0x80,0xff), (byte)rnd.Next(0x80, 0xff)));
            }
        }

        [Test]
        public void testSetBits()
        {
            byte value;
            byte mask;
            byte newValue;
            Random rnd = new Random();

            for (int i = 0; i < 1000; i++){
                value = (byte)rnd.Next(0, 0xff);
                mask = (byte)rnd.Next(0, 0xff);
                newValue = (byte)(value|mask);
                Assert.AreEqual(newValue, CPU6510.setBits(value, mask, set:true));
            }

            for (int i = 0; i < 1000; i++){
                value = (byte)rnd.Next(0, 0xff);
                mask = (byte)rnd.Next(0, 0xff);
                newValue = (byte)(value & (byte)(0xff ^ mask));
                Assert.AreEqual(newValue, CPU6510.setBits(value, mask, set:false));
            }
        }

        [Test]
        public void testORA()
        {
            byte oldA;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.ORA(addr);

                Assert.AreEqual( oldA | blankMemory[addr], cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testAND()
        {
            byte oldA;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.AND(addr);

                Assert.AreEqual(oldA & blankMemory[addr], cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testEOR()
        {
            byte oldA;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.EOR(addr);

                Assert.AreEqual(oldA ^ blankMemory[addr], cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }
        [Test]
        public void testADC()
        {
            byte oldA;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.ADC(addr);

                Assert.AreEqual((byte)(oldA + blankMemory[addr]), cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual(oldA + blankMemory[addr] > 0xff, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.A & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
                // overflow or underflow?
                if (CPU6510.checkForOverflow(oldA, cpu.A))
                    Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
                else
                    Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
            }
        }

        [Test]
        public void testSBC()
        {
            byte oldA;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.SBC(addr);

                Assert.AreEqual((byte)(oldA - blankMemory[addr]), cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual(oldA - blankMemory[addr] < 0x00, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.A & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
                // overflow or underflow?
                if (CPU6510.checkForOverflow(oldA, cpu.A))
                    Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
                else
                    Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
            }
        }

        [Test]
        public void testCMP()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                cpu.CMP(addr);

                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual(cpu.A - blankMemory[addr] < 0x00, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.A & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testCPX()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.X = (byte)rnd.Next(0,255);
                cpu.CPX(addr);

                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual(cpu.X - blankMemory[addr] < 0x00, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.X & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testCPY()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.Y = (byte)rnd.Next(0,255);
                cpu.CPY(addr);

                // zero bit set?
                Assert.AreEqual(cpu.Y == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual(cpu.Y - blankMemory[addr] < 0x00, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.Y & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testDEC()
        {
            byte num;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                blankMemory[addr] = num;
                cpu.DEC(addr);

                Assert.AreEqual((byte)(num - 1), blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((blankMemory[addr] & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testDEX()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);
                cpu.X = num;
                cpu.DEX();

                Assert.AreEqual((byte)(num - 1), cpu.X);
                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((cpu.X & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testDEY()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);
                cpu.Y = num;
                cpu.DEY();

                Assert.AreEqual((byte)(num - 1), cpu.Y);
                // zero bit set?
                Assert.AreEqual(cpu.Y == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((cpu.Y & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testINC()
        {
            byte num;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                blankMemory[addr] = num;
                cpu.INC(addr);

                Assert.AreEqual((byte)(num + 1), blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((blankMemory[addr] & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testINX()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);
                cpu.X = num;
                cpu.INX();

                Assert.AreEqual((byte)(num + 1), cpu.X);
                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((cpu.X & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testINY()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);
                cpu.Y = num;
                cpu.INY();

                Assert.AreEqual((byte)(num + 1), cpu.Y);
                // zero bit set?
                Assert.AreEqual(cpu.Y == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((cpu.Y & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testASL()
        {
            byte num;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                blankMemory[addr] = num;
                cpu.ASL(addr);

                Assert.AreEqual((byte)(num << 1), blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((blankMemory[addr] & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testASL_A()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);
                cpu.A = num;
                cpu.ASL();

                Assert.AreEqual((byte)(num << 1), cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.A & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testROL()
        {
            byte carryflag;
            byte num;
            byte newNum;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryflag = (byte)rnd.Next(0, 1);
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet: carryflag == 1);
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                newNum = CPU6510.setBits((byte)(num << 1), 0x01, set: carryflag == 1);

                blankMemory[addr] = num;
                cpu.ROL(addr);

                Assert.AreEqual((newNum), blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((blankMemory[addr] & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testROL_A()
        {
            byte carryflag;
            byte num;
            byte newNum;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryflag = (byte)rnd.Next(0, 1);
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet: carryflag == 1);
                num = (byte)rnd.Next(0,255);
                newNum = CPU6510.setBits((byte)(num << 1), 0x01, set: carryflag == 1);

                cpu.A = num;
                cpu.ROL();

                Assert.AreEqual((newNum), cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A== 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual((cpu.A & 0x80) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testLSR()
        {
            byte num;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                blankMemory[addr] = num;
                cpu.LSR(addr);

                Assert.AreEqual((byte)(num >> 1), blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x01) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual(false, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testLSR_A()
        {
            byte num;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                num = (byte)rnd.Next(0,255);

                cpu.A = num;
                cpu.LSR();

                Assert.AreEqual((byte)(num >> 1), cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x01) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual(false, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testROR()
        {
            byte carryflag;
            byte num;
            byte newNum;
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryflag = (byte)rnd.Next(0, 1);
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet: carryflag == 1);
                addr = (ushort)rnd.Next(0,0xffff);
                num = (byte)rnd.Next(0,255);
                newNum = CPU6510.setBits((byte)(num >> 1), 0x80, set: carryflag == 1);

                blankMemory[addr] = num;
                cpu.ROR(addr);

                Assert.AreEqual(newNum, blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(blankMemory[addr] == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x01) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual(false, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testROR_A()
        {
            byte carryflag;
            byte num;
            byte newNum;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryflag = (byte)rnd.Next(0, 1);
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet: carryflag == 1);
                num = (byte)rnd.Next(0,255);
                newNum = CPU6510.setBits((byte)(num >> 1), 0x80, set: carryflag == 1);

               cpu.A = num;
                cpu.ROR();

                Assert.AreEqual(newNum, cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.AreEqual((num & 0x01) != 0, cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.AreEqual(false, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testLDA()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.LDA(addr);

                Assert.AreEqual(cpu.A, blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTA()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                cpu.A = (byte)rnd.Next(0,255);
                cpu.STA(addr);

                Assert.AreEqual(blankMemory[addr], cpu.A);
            }
        }

        [Test]
        public void testLDX()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.LDX(addr);

                Assert.AreEqual(cpu.X, blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.X >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTX()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                cpu.X = (byte)rnd.Next(0,255);
                cpu.STX(addr);

                Assert.AreEqual(blankMemory[addr], cpu.X);
            }
        }

        [Test]
        public void testLDY()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.LDY(addr);

                Assert.AreEqual(cpu.Y, blankMemory[addr]);
                // zero bit set?
                Assert.AreEqual(cpu.Y == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.Y >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTY()
        {
            ushort addr;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0,0xffff);
                cpu.Y = (byte)rnd.Next(0,255);
                cpu.STY(addr);

                Assert.AreEqual(blankMemory[addr], cpu.Y);
            }
        }

        [Test]
        public void testTAX()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.A = (byte)rnd.Next(0,0xff);
                cpu.TAX();

                Assert.AreEqual(cpu.X, cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.X >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testTXA()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.X = (byte)rnd.Next(0,0xff);
                cpu.TXA();

                Assert.AreEqual(cpu.A, cpu.X);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testTAY()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.A = (byte)rnd.Next(0,0xff);
                cpu.TAY();

                Assert.AreEqual(cpu.Y, cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.Y == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.Y >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testTYA()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.Y = (byte)rnd.Next(0,0xff);
                cpu.TYA();

                Assert.AreEqual(cpu.A, cpu.Y);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testTSX()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.S = (byte)rnd.Next(0,0xff);
                cpu.TSX();

                Assert.AreEqual(cpu.X, cpu.S);
                // zero bit set?
                Assert.AreEqual(cpu.X == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.X >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testTXS()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.X = (byte)rnd.Next(0,0xff);
                cpu.TXS();

                Assert.AreEqual(cpu.S, cpu.X);
            }
        }

        [Test]
        public void testPLA()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();
            byte oldS;

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                blankMemory[CPU6510.STACK_OFFSET + cpu.S] = (byte)rnd.Next(0,0xff);
                oldS = cpu.S;
                cpu.PLA();

                Assert.AreEqual(blankMemory[CPU6510.STACK_OFFSET + (byte)(oldS+1)], cpu.A);
                // zero bit set?
                Assert.AreEqual(cpu.A == 0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual(cpu.A >= 0x80, cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testPHA()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();
            byte oldS;

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.A = (byte)rnd.Next(0,0xff);
                oldS = cpu.S;
                cpu.PHA();

                Assert.AreEqual(cpu.A, blankMemory[CPU6510.STACK_OFFSET + oldS]);
            }
        }

        [Test]
        public void testPLP()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();
            byte oldS;

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                blankMemory[CPU6510.STACK_OFFSET + cpu.S] = (byte)rnd.Next(0,0xff);
                oldS = cpu.S;
                cpu.PLP();

                Assert.AreEqual(blankMemory[CPU6510.STACK_OFFSET + (byte)(oldS+1)], cpu.P);
            }
        }

        [Test]
        public void testPHP()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();
            byte oldS;

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0,0xff);
                oldS = cpu.S;
                cpu.PHP();

                Assert.AreEqual(cpu.P, blankMemory[CPU6510.STACK_OFFSET + oldS]);
            }
        }

        [Test]
        public void testBPL()
        {
            sbyte offset;
            ushort oldPC;
            bool negativeFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                negativeFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.N, isSet:negativeFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BPL(offset);

                if (negativeFlag)
                    Assert.AreEqual(oldPC, cpu.PC);
                else
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
            }
        }

        [Test]
        public void testBMI()
        {
            sbyte offset;
            ushort oldPC;
            bool negativeFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                negativeFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.N, isSet:negativeFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BMI(offset);

                if (negativeFlag)
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
                else
                    Assert.AreEqual(oldPC, cpu.PC);
            }
        }

        [Test]
        public void testBVC()
        {
            sbyte offset;
            ushort oldPC;
            bool overflowFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                overflowFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.V, isSet:overflowFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BVC(offset);

                if (overflowFlag)
                    Assert.AreEqual(oldPC, cpu.PC);
                else
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
            }
        }

        [Test]
        public void testBVS()
        {
            sbyte offset;
            ushort oldPC;
            bool overflowFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                overflowFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.V, isSet:overflowFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BVS(offset);

                if (overflowFlag)
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
                else
                    Assert.AreEqual(oldPC, cpu.PC);
            }
        }

        [Test]
        public void testBCC()
        {
            sbyte offset;
            ushort oldPC;
            bool carryFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet:carryFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BCC(offset);

                if (carryFlag)
                    Assert.AreEqual(oldPC, cpu.PC);
                else
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
            }
        }

        [Test]
        public void testBCS()
        {
            sbyte offset;
            ushort oldPC;
            bool carryFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                carryFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.C, isSet:carryFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BCS(offset);

                if (carryFlag)
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
                else
                    Assert.AreEqual(oldPC, cpu.PC);
            }
        }

        [Test]
        public void testBNE()
        {
            sbyte offset;
            ushort oldPC;
            bool zeroFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                zeroFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.Z, isSet:zeroFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BNE(offset);

                if (zeroFlag)
                    Assert.AreEqual(oldPC, cpu.PC);
                else
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
            }
        }

        [Test]
        public void testBEQ()
        {
            sbyte offset;
            ushort oldPC;
            bool zeroFlag;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                zeroFlag = rnd.Next(0, 1) == 1;
                cpu.setProcessorStatusBit(ProcessorStatus.Z, isSet:zeroFlag);
                offset = (sbyte)rnd.Next(0x00, 0xff);
                cpu.PC = (ushort)rnd.Next(0, 0xffff);
                oldPC = cpu.PC;

                cpu.BEQ(offset);

                if (zeroFlag)
                    Assert.AreEqual((ushort)(oldPC+offset), cpu.PC);
                else
                    Assert.AreEqual(oldPC, cpu.PC);
            }
        }

        [Test]
        public void testBRK()
        {
            byte newPCH, newPCL;
            byte oldS;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = 0x00;
                cpu.PC = (ushort)rnd.Next(0x00, 0xff00);
                newPCH = (byte)((cpu.PC+2)>>8);
                newPCL = (byte)((cpu.PC+2)&0x00ff);
                oldS = (byte)rnd.Next(0x00, 0xff);
                cpu.S = oldS;
                blankMemory[CPU6510.IRQ_VECTOR] = (byte)rnd.Next(0x00, 0xff);
                blankMemory[CPU6510.IRQ_VECTOR+1] = (byte)rnd.Next(0x00, 0xff);

                cpu.BRK();

                Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.B));
                Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.I));

                Assert.AreEqual(newPCH, blankMemory[CPU6510.STACK_OFFSET + (byte)(cpu.S+3)]);
                Assert.AreEqual(newPCL, blankMemory[CPU6510.STACK_OFFSET + (byte)(cpu.S+2)]);
                Assert.AreEqual(cpu.P, blankMemory[CPU6510.STACK_OFFSET + (byte)(cpu.S+1)]);
                Assert.AreEqual((byte)(oldS-3), cpu.S);

                Assert.AreEqual(blankMemory[CPU6510.IRQ_VECTOR], cpu.PCL);
                Assert.AreEqual(blankMemory[CPU6510.IRQ_VECTOR+1], cpu.PCH);
            }
        }

        [Test]
        public void testRTI()
        {
            byte pch, pcl, p;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.S = (byte)rnd.Next(0x00, 0xff);
                pch = (byte)rnd.Next();
                pcl = (byte)rnd.Next();
                p = (byte)rnd.Next();

                cpu.push(pch);
                cpu.push(pcl);
                cpu.push(p);

                cpu.RTI();

                Assert.AreEqual(p, cpu.P);
                Assert.AreEqual(pcl, cpu.PCL);
                Assert.AreEqual(pch, cpu.PCH);
            }
        }

        [Test]
        public void testJSR()
        {
            ushort addr;
            byte pch, pcl, s;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                addr = (ushort)rnd.Next(0x0000, 0xffff);
                s = (byte)rnd.Next(0x00, 0xff);
                cpu.S  = s;
                cpu.PC = (ushort)rnd.Next(0x0000, 0xffff);
                pch = (byte)((cpu.PC+2)>>8);
                pcl = (byte)((cpu.PC+2)&0x00ff);

                cpu.JSR(addr);

                Assert.AreEqual((byte)(s-2), cpu.S);

                Assert.AreEqual(pcl, blankMemory[CPU6510.STACK_OFFSET + (byte)(cpu.S+1)]);
                Assert.AreEqual(pch, blankMemory[CPU6510.STACK_OFFSET + (byte)(cpu.S+2)]);

                Assert.AreEqual(addr, cpu.PC);
            }
        }

        [Test]
        public void testRTS()
        {
            ushort pc;
            byte s;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                s = (byte)rnd.Next(0x00, 0xff);
                cpu.S  = s;
                pc = (ushort)rnd.Next(0x0000, 0xffff);
                cpu.push((byte)(pc>>8));
                cpu.push((byte)(pc&0x00ff));

                cpu.RTS();

                Assert.AreEqual(s, cpu.S);
                Assert.AreEqual((ushort)(pc+1), cpu.PC);
            }
        }

        [Test]
        public void testJMP()
        {
            ushort address;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                address = (ushort)rnd.Next(0x0000, 0xffff);

                cpu.JMP(address);

                Assert.AreEqual(address, cpu.PC);
            }
        }

        [Test]
        public void testBIT()
        {
            ushort address;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                address = (ushort)rnd.Next(0x0000, 0xffff);
                blankMemory[address] = (byte)rnd.Next(0x00, 0xff);
                cpu.A = (byte)rnd.Next(0x00, 0xff);

                cpu.BIT(address);

                Assert.AreEqual(blankMemory[address]&(byte)ProcessorStatus.N, cpu.P&(byte)ProcessorStatus.N);
                Assert.AreEqual(blankMemory[address]&(byte)ProcessorStatus.V, cpu.P&(byte)ProcessorStatus.V);
                Assert.AreEqual((blankMemory[address]&cpu.A)==0, cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
            }
        }

        [Test]
        public void testCLC()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.CLC();
                Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.C));
            }
        }

        [Test]
        public void testSEC()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.SEC();
                Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.C));
            }
        }

        [Test]
        public void testCLD()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.CLD();
                Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.D));
            }
        }

        [Test]
        public void testSED()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.SED();
                Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.D));
            }
        }

        [Test]
        public void testCLI()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.CLI();
                Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.I));
            }
        }

        [Test]
        public void testSEI()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.SEI();
                Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.I));
            }
        }

        [Test]
        public void testCLV()
        {
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < NUMBER_TEST_RUNS; i++)
            {
                cpu.P = (byte)rnd.Next(0x00, 0xff);
                cpu.CLV();
                Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
            }
        }
    }
}
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
        public void testORA()
        {
            byte oldA;
            ushort addr = 0x00;
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
                Assert.AreEqual((oldA|blankMemory[addr]), cpu.A);
                // zero bit set?
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testAND()
        {
            byte oldA;
            ushort addr = 0x00;
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
                Assert.AreEqual((oldA&blankMemory[addr]), cpu.A);
                // zero bit set?
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testEOR()
        {
            byte oldA;
            ushort addr = 0x00;
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

                Assert.AreEqual((oldA^blankMemory[addr]), cpu.A);
                // zero bit set?
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }
        [Test]
        public void testADC()
        {
            byte oldA;
            ushort addr = 0x00;
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

                Assert.AreEqual((oldA+blankMemory[addr])%0x0100, cpu.A);
                // zero bit set?
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // carryover set?
                Assert.True((oldA+blankMemory[addr] > 0xff) == cpu.isProcessorStatusBitSet(ProcessorStatus.C));
                // negative bit set?
                Assert.True((((oldA+blankMemory[addr])%0x0100) >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
                // positive > negative overflow?
                if (oldA < 0x80 && ((oldA+blankMemory[addr])%0x0100) >= 0x80)
                    Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
                // negative > positive underflow?  ToDo: can this really happen here?
                else if (oldA > 0x80 && ((oldA+blankMemory[addr])%0x0100) < 0x80)
                    Assert.True(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
                else
                    // This fails every now and then the test
                    Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
            }
        }

        [Test]
        public void testLDA()
        {
            ushort addr = 0x00;
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
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTA()
        {
            ushort addr = 0x00;
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
            ushort addr = 0x00;
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
                Assert.True((cpu.X == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.X >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTX()
        {
            ushort addr = 0x00;
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
            ushort addr = 0x00;
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
                Assert.True((cpu.Y == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.Y >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
            }
        }

        [Test]
        public void testSTY()
        {
            ushort addr = 0x00;
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
                Assert.True((cpu.X == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.X >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
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
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
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
                Assert.True((cpu.Y == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.Y >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
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
                Assert.True((cpu.A == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.A >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
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
                Assert.True((cpu.X == 0) == cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.True((cpu.X >= 0x80) == cpu.isProcessorStatusBitSet(ProcessorStatus.N));
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

                Assert.AreEqual(blankMemory[CPU6510.STACK_OFFSET + oldS], cpu.A);
                // zero bit set?
                Assert.AreEqual((cpu.A == 0), cpu.isProcessorStatusBitSet(ProcessorStatus.Z));
                // negative bit set?
                Assert.AreEqual((cpu.A >= 0x80), cpu.isProcessorStatusBitSet(ProcessorStatus.N));
                // stack pointer changed accordingly?
                Assert.AreEqual((byte)(oldS+1), cpu.S);
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
                // stack pointer changed accordingly?
                Assert.AreEqual((byte)(oldS-1), cpu.S);
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

                Assert.AreEqual(blankMemory[CPU6510.STACK_OFFSET + oldS], cpu.P);
                // stack pointer changed accordingly?
                Assert.AreEqual((byte)(oldS+1), cpu.S);
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
                // stack pointer changed accordingly?
                Assert.AreEqual((byte)(oldS-1), cpu.S);
            }
        }
    }
}
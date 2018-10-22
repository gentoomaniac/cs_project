using System;

using NUnit.Framework;

using CycleLock;
using MOS;

namespace TestInstructions
{
   class InstructionTetsts
    {
        [Test]
        public void testORA()
        {
            byte oldA;
            ushort addr = 0x00;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < 100; i++)
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

            for (int i = 0; i < 100; i++)
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

            for (int i = 0; i < 100; i++)
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

            for (int i = 0; i < 100; i++)
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
                    Assert.False(cpu.isProcessorStatusBitSet(ProcessorStatus.V));
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

            for (int i = 0; i < 100; i++)
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

            for (int i = 0; i < 100; i++)
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
    }
}
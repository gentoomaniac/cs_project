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
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.ORA(addr);
                Assert.AreEqual((oldA|blankMemory[addr]), cpu.A);
            }

            blankMemory[addr] = 0x00;
            cpu.A = 0x00;
            oldA = cpu.A;
            cpu.ORA(addr);
            Assert.AreEqual(0x02, cpu.P);

            blankMemory[addr] = 0x81;
            cpu.A = 0x02;
            oldA = cpu.A;
            cpu.ORA(addr);
            Assert.AreEqual(0x80, cpu.P);
        }

        [Test]
        public void testAND()
        {
            byte oldA;
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.AND(addr);
                Assert.AreEqual((oldA&blankMemory[addr]), cpu.A);
            }

            // Test Status Register
            blankMemory[addr] = 0x05;
            cpu.A = 0x62;
            oldA = cpu.A;
            cpu.AND(addr);
            Assert.AreEqual(0x02, cpu.P);

            blankMemory[addr] = 0x81;
            cpu.A = 0x82;
            oldA = cpu.A;
            cpu.AND(addr);
            Assert.AreEqual(0x80, cpu.P);
        }

        [Test]
        public void testEOR()
        {
            byte oldA;
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.EOR(addr);
                Assert.AreEqual((oldA^blankMemory[addr]), cpu.A);
            }

            // Test Status Register
            blankMemory[addr] = 0x07;
            cpu.A = 0x07;
            oldA = cpu.A;
            cpu.EOR(addr);
            Assert.AreEqual(0x02, cpu.P);

            blankMemory[addr] = 0x81;
            cpu.A = 0x03;
            oldA = cpu.A;
            cpu.EOR(addr);
            Assert.AreEqual(0x80, cpu.P);
        }
        public void testADC()
        {
            byte oldA;
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);
            Random rnd = new Random();

            for (int i = 0; i < 100; i++)
            {
                blankMemory[addr] = (byte)rnd.Next(0,255);
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                cpu.EOR(addr);
                Assert.AreEqual((oldA^blankMemory[addr]), cpu.A);
            }

            // Test Status Register
            blankMemory[addr] = 0x07;
            cpu.A = 0x07;
            oldA = cpu.A;
            cpu.EOR(addr);
            Assert.AreEqual(0x02, cpu.P);

            blankMemory[addr] = 0x81;
            cpu.A = 0x03;
            oldA = cpu.A;
            cpu.EOR(addr);
            Assert.AreEqual(0x80, cpu.P);
        }
    }
}
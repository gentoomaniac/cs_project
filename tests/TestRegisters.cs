using System;

using NUnit.Framework;

using CycleLock;
using MOS;

namespace TestRegisters
{
    public class RegisterTests
    {
        private int NUMBER_TEST_RUNS = 10000;

        [Test]
        public void TestRegisterPCL()
        {
            byte value, pch;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);

            value = 0x00;
            cpu.PCL = value;
            Assert.AreEqual(value, cpu.PCL);

            // all bits set
            value = 0xff;
            cpu.PCL = value;
            Assert.AreEqual(value, cpu.PCL);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = NUMBER_TEST_RUNS; i>0; i--)
            {
                cpu.PC = (ushort)rnd.Next(0x0000, 0xffff);
                pch = cpu.PCH;
                value = (byte)rnd.Next(0x00, 0xff);
                cpu.PCL = value;
                Assert.AreEqual(value, cpu.PCL);
                Assert.AreEqual(pch, cpu.PCH);
            }
        }

        [Test]
        public void TestRegisterPCH()
        {
            byte value, pcl;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);

            value = 0x00;
            cpu.PCH = value;
            Assert.AreEqual(value, cpu.PCH);

            // all bits set
            value = 0xff;
            cpu.PCH = value;
            Assert.AreEqual(value, cpu.PCH);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = NUMBER_TEST_RUNS; i>0; i--)
            {
                cpu.PC = (ushort)rnd.Next(0x0000, 0xffff);
                pcl = cpu.PCL;
                value = (byte)rnd.Next(0x00, 0xff);
                cpu.PCH = value;
                Assert.AreEqual(value, cpu.PCH);
                Assert.AreEqual(pcl, cpu.PCL);
            }
        }

        [Test]
        public void TestRegisterPC()
        {
            ushort value;
            byte[] blankMemory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(blankMemory, cpuLock);

            value = 0x00;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // all bits in PCL
            value = 0xff;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // overflow into PCH
            value = 0x0100;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = NUMBER_TEST_RUNS; i>0; i--)
            {
                value = (ushort)rnd.Next(0x00, 0xffff);
                cpu.PC = value;
                Assert.AreEqual(value, cpu.PC);
            }
        }
    }
}
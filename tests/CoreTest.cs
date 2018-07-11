using System;

using NUnit.Framework;

using MOS;

namespace CoreTests
{
    public class RegisterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRegisterPCL()
        {
            byte value = 0;
            byte[] blankMemory = new byte[65536];
            CPU6510 cpu = new CPU6510(blankMemory);

            value = 0;
            cpu.PCL = value;
            Assert.AreEqual(value, cpu.PCL);

            // all bits set
            value = 255;
            cpu.PCL = value;
            Assert.AreEqual(value, cpu.PCL);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = 100; i>0; i--)
            {
                value = (byte)rnd.Next(0,255);
                cpu.PCL = value;
                Assert.AreEqual(value, cpu.PCL);
            }
        }

        [Test]
        public void TestRegisterPCH()
        {
            byte value = 0;
            byte[] blankMemory = new byte[65536];
            CPU6510 cpu = new CPU6510(blankMemory);

            value = 0;
            cpu.PCH = value;
            Assert.AreEqual(value, cpu.PCH);

            // all bits set
            value = 255;
            cpu.PCH = value;
            Assert.AreEqual(value, cpu.PCH);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = 100; i>0; i--)
            {
                value = (byte)rnd.Next(0,255);
                cpu.PCH = value;
                Assert.AreEqual(value, cpu.PCH);
            }
        }

        [Test]
        public void TestRegisterPC()
        {
            ushort value = 0;
            byte[] blankMemory = new byte[65536];
            CPU6510 cpu = new CPU6510(blankMemory);

            value = 0;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // all bits in PCL
            value = 255;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // overflow into PCH
            value = 256;
            cpu.PC = value;
            Assert.AreEqual(value, cpu.PC);

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = 100; i>0; i--)
            {
                value = (ushort)rnd.Next(0,65535);
                cpu.PC = value;
                Assert.AreEqual(value, cpu.PC);
            }
        }

        [Test]
        public void testORA()
        {
            byte oldA;
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            CPU6510 cpu = new CPU6510(blankMemory);

            blankMemory[addr] = 10;
            cpu.A = 0x01;
            oldA = cpu.A;
            cpu.ORA(addr);
            Assert.AreEqual((oldA|blankMemory[addr]), cpu.A);
            Assert.AreEqual(0x00, cpu.P);

            blankMemory[addr] = 0x00;
            cpu.A = 0x00;
            oldA = cpu.A;
            cpu.ORA(addr);
            Assert.AreEqual((oldA|blankMemory[addr]), cpu.A);
            Assert.AreEqual(0x02, cpu.P);

            blankMemory[addr] = 0x81;
            cpu.A = 0x02;
            oldA = cpu.A;
            cpu.ORA(addr);
            Assert.AreEqual((oldA|blankMemory[addr]), cpu.A);
            Assert.AreEqual(0x80, cpu.P);
        }
    }
}
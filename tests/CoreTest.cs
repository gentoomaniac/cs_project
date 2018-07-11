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
            CPU6510 cpu = new CPU6510();

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
            CPU6510 cpu = new CPU6510();

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
            CPU6510 cpu = new CPU6510();

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
    }
}
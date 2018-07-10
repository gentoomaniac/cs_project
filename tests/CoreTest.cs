using System;

using NUnit.Framework;

using Core;

namespace CoreTests
{
    public class RegisterTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRegister8()
        {
            byte value = 0;
            Register8 register = new Register8();

            value = 0;
            register.setValue(value);
            Assert.AreEqual(value, register.getValue());

            // all bits set
            value = 255;
            register.setValue(value);
            Assert.AreEqual(value, register.getValue());

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = 100; i>0; i--)
            {
                value = (byte)rnd.Next(0,255);
                register.setValue(value);
                Assert.AreEqual(value, register.getValue());
            }
        }

        [Test]
        public void TestPCH()
        {
            ushort value = 0;
            RegisterPC PC = new RegisterPC();

            value = 0;
            PC.setValue(value);
            Assert.AreEqual(value, PC.getValue());

            // all bits in PCL
            value = 255;
            PC.setValue(value);
            Assert.AreEqual(value, PC.getValue());

            // overflow into PCH
            value = 256;
            PC.setValue(value);
            Assert.AreEqual(value, PC.getValue());

            // do some random tests of the register
            Random rnd = new Random();
            for(int i = 100; i>0; i--)
            {
                value = (ushort)rnd.Next(0,65535);
                PC.setValue(value);
                Assert.AreEqual(value, PC.getValue());
            }
        }
    }
}
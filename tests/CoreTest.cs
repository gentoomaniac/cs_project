using System;

using NUnit.Framework;

using Core;

namespace Tests
{
    public class project
    {
        [SetUp]
        public void Setup()
        {
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
            value = 256;
            PC.setValue(value);
            Assert.AreEqual(value, PC.getValue());

            // overflow into PCH
            value = 257;
            PC.setValue(value);
            Assert.AreEqual(value, PC.getValue());

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
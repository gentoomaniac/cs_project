using System;

using NUnit.Framework;

using MOS;
using Y1;

namespace ClockTests
{
     class ClockTests
    {
        [Test]
        public void testORA()
        {
            byte oldA;
            ushort addr = 0x002a;
            byte[] blankMemory = new byte[65536];
            CPU6510 cpu = new CPU6510(blankMemory);
            Random rnd = new Random();
            SystemClock y1 = new SystemClock(cpu.getSystemClockMutex());
            y1.start();

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
            CPU6510 cpu = new CPU6510(blankMemory);
            Random rnd = new Random();
            SystemClock y1 = new SystemClock(cpu.getSystemClockMutex());
            y1.start();

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
            CPU6510 cpu = new CPU6510(blankMemory);
            Random rnd = new Random();
            SystemClock y1 = new SystemClock(cpu.getSystemClockMutex());
            y1.start();

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

    public class OpcodeMapperOraTests
    {
        // make sure this works
        [Test]
        public void testORAZeropageIndexedIndirect()
        {
            ushort addr = 0x0100;
            byte[] memory = new byte[65536];
            CPU6510 cpu = new CPU6510(memory);
            Random rnd = new Random();
            byte opcode = 0x01;
            byte oldA;

            for (int i = 0; i < 100; i++)
            {
                // instruction byte after opcode
                memory[addr] = (byte)rnd.Next(2,ushort.MaxValue);
                cpu.PC = addr;
                cpu.A = (byte)rnd.Next(0,255);
                cpu.X = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                memory[memory[memory[addr]+cpu.X]] = (byte)rnd.Next(0,255);
        
                cpu.opcodeMapper(opcode);
                Assert.AreEqual((oldA|memory[memory[memory[addr]+cpu.X]]), cpu.A);
            }
        }

        [Test]
        public void testORAZeropage()
        {
            ushort addr = 0x0100;
            byte[] memory = new byte[65536];
            CPU6510 cpu = new CPU6510(memory);
            Random rnd = new Random();
            byte opcode = 0x05;
            byte oldA;

            for (int i = 0; i < 100; i++)
            {
                // instruction byte after opcode
                memory[addr] = (byte)rnd.Next(2,255);
                cpu.PC = addr;
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                memory[memory[addr]] = (byte)rnd.Next(0,255);
        
                cpu.opcodeMapper(opcode);
                Assert.AreEqual((oldA|memory[memory[addr]]), cpu.A);
            }
        }

        [Test]
        public void testORAZeropageindirectIndexed()
        {
            ushort addr = 0x0100;
            byte[] memory = new byte[65536];
            CPU6510 cpu = new CPU6510(memory);
            Random rnd = new Random();
            byte opcode = 0x11 ;
            byte oldA;

            for (int i = 0; i < 100; i++)
            {
                // instruction byte after opcode
                memory[addr] = (byte)rnd.Next(2,255);
                cpu.PC = addr;
                cpu.A = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                memory[memory[addr]] = (byte)rnd.Next(0,255);
        
                cpu.opcodeMapper(opcode);
                Assert.AreEqual((oldA|memory[memory[addr]+cpu.Y]), cpu.A);
            }
        }

        [Test]
        public void testORAZeropageIndexed()
        {
            ushort addr = 0x0100;
            byte[] memory = new byte[65536];
            CPU6510 cpu = new CPU6510(memory);
            Random rnd = new Random();
            byte opcode = 0x15;
            byte oldA;

            for (int i = 0; i < 100; i++)
            {
                // instruction byte after opcode
                memory[addr] = (byte)rnd.Next(2,ushort.MaxValue);
                cpu.PC = addr;
                cpu.A = (byte)rnd.Next(0,255);
                cpu.X = (byte)rnd.Next(0,255);
                oldA = cpu.A;
                memory[memory[addr]+cpu.X] = (byte)rnd.Next(0,255);
        
                cpu.opcodeMapper(opcode);
                Assert.AreEqual((oldA|memory[memory[addr]+cpu.X]), cpu.A);
            }
        }
    }
}
using System;

using NUnit.Framework;

using CycleLock;
using MOS;

namespace CoreTests
{
    public class OpcodeMapperOraTests
    {
        // make sure this works
        [Test]
        public void testORAZeropageIndexedIndirect()
        {
            ushort addr = 0x0100;
            byte[] memory = new byte[65536];
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(memory, cpuLock);
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
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(memory, cpuLock);
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
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(memory, cpuLock);
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
            Lock cpuLock = new AlwaysOpenLock();
            CPU6510 cpu = new CPU6510(memory, cpuLock);
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
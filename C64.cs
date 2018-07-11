using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using NLog;

using MOS;

namespace commodore
{
    [Flags]
    public enum Latch { LORAM = 0x01, HIRAM = 0x02, CHAREN = 0x04, }
 
    class C64
    {
        private CPU6510 cpu;
        private byte[] memory;

        // ROM
        private byte[] basicRom;
        private byte[] characterRom;
        private byte[] kernalRom;


        private Logger log;

        public C64(string basicRomFileName, string characterRomFileName, string kernalRomFileName)
        {
            log = log = NLog.LogManager.GetCurrentClassLogger();

            cpu = new CPU6510(memory);
            memory = new byte[ushort.MaxValue+1];

            basicRom = new byte[8192];
            loadRomFromFile(basicRom, basicRomFileName);
            characterRom = new byte[4096];
            loadRomFromFile(characterRom, characterRomFileName);
            kernalRom = new byte[8192];
            loadRomFromFile(kernalRom, kernalRomFileName);
        }

        public void initialize()
        {
            memory[0x00] = 0xff;
            memory[0x01] = 0x07;
            updateMemoryBanks();
        }

        /* Depending on the latch byte in memory this function will load the different ROMs into memory
         https://www.c64-wiki.com/wiki/Bank_Switching#CPU_Control_Lines
        */
        public void updateMemoryBanks()
        {
            if ( (memory[0x01] & (byte)Latch.LORAM) != 0 )
                Array.Copy(basicRom, 0x00, memory, 0xa000, basicRom.Length);
            if ( (memory[0x01] & (byte)Latch.HIRAM) != 0 )
                Array.Copy(kernalRom, 0x00, memory, 0xe000, kernalRom.Length);
            if ( (memory[0x01] & (byte)Latch.CHAREN) != 0 )
                log.Debug("ToDo: CHAREN is set, I/O should be mapped");
            else
                Array.Copy(characterRom, 0x00, memory, 0xd000, characterRom.Length);
        }

        public void dumpMemory(ushort offset, byte[] memory)
        {
            string[] rowAsHexidecimal;
            char[] rowAsCharacter;
            byte[] memoryRow = new byte[16];
            ushort realOffset = (ushort)(offset - (offset%16));

            for (int i=realOffset; i < memory.Length; i+=16)
            {
                Array.Copy(memory, i, memoryRow, 0, 16);
                rowAsHexidecimal = Array.ConvertAll<byte, string>(memoryRow, holdByte => holdByte.ToString("x2"));
                rowAsCharacter = Array.ConvertAll<byte, char>(memoryRow, holdByte => (char)holdByte);
                log.Debug(
                    string.Format("{0}h: {1}\t{2}",i.ToString("x4"),
                                    string.Join(" ", rowAsHexidecimal),
                                    Regex.Replace(string.Join("", rowAsCharacter), "\\s", ".")));
            }
        }

        public void dumpMemory()
        {
            dumpMemory(0, this.memory);
        }

        public void dumpMemory(ushort offset)
        {
            dumpMemory(offset, this.memory);
        }

        public void dumpRoms()
        {
            log.Debug("--- Dumping basic ROM:");
            dumpMemory(0, this.basicRom);

            log.Debug("--- Dumping character ROM:");
            dumpMemory(0, this.characterRom);

            log.Debug("--- Dumping kernal ROM:");
            dumpMemory(0, this.kernalRom);
        }

        private void loadRomFromFile(byte[] rom, string romFileName)
        {
            byte[] romFileContent = File.ReadAllBytes(romFileName);
            Array.Copy(romFileContent, rom, rom.Length);
        }
    }
}
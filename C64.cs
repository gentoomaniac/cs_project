using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using NLog;

using MOS;
using Y1;

namespace commodore
{
    [Flags]
    public enum Latch { LORAM = 0x01, HIRAM = 0x02, CHAREN = 0x04, }

 
    class C64
    {
        // Y1 Crystal frequency NTSC
        public static int Y1_NTSC = 14318180;
        // Y1_NTSC/14
        public static int CLOCK_NTSC = 1022727;

        // Y1 Crystal frequency PAL
        public static int Y1_PAL = 17734475;
        // Y1_NTSC/18
        public static int CLOCK_PAL = 985248;


        private CPU6510 cpu;
        private byte[] memory;

        private SystemClock y1;
        private int systemClockRate;
        private Mutex cpuMutex;

        // flag to indicate online state. Used for instance in the system clock.
        private bool powerSwitch;

        // ROM
        private byte[] basicRom;
        private byte[] characterRom;
        private byte[] kernalRom;

        private Logger log;

        public C64(string basicRomFileName, string characterRomFileName, string kernalRomFileName, bool isPal=true)
        {
            log = log = NLog.LogManager.GetCurrentClassLogger();

            if (isPal)
                systemClockRate = CLOCK_PAL;
            else
                systemClockRate = CLOCK_NTSC;

            memory = new byte[ushort.MaxValue+1];
            cpu = new CPU6510(memory);

            basicRom = new byte[8192];
            loadRomFromFile(basicRom, basicRomFileName);
            characterRom = new byte[4096];
            loadRomFromFile(characterRom, characterRomFileName);
            kernalRom = new byte[8192];
            loadRomFromFile(kernalRom, kernalRomFileName);

            y1 = new SystemClock(cpu.getCycleUnlockEventObject());
        }

        public void powerOn()
        {
            log.Debug("Initializing system");
            powerSwitch = true;

            log.Debug("Initializing memory ...");
            memory[0x00] = 0xff;
            memory[0x01] = 0x07;
            log.Debug("Updating memory banks ...");
            updateMemoryBanks();

            cpu.start();
            y1.start();
        }

        public void powerOff()
        {
            log.Debug("powering off system ...");
            powerSwitch = false;

            y1.halt();
            cpu.stop();
            y1.cleanup();
            log.Debug("system powered off!");
        }

        /* Depending on the latch byte in memory this function will load the different ROMs into memory
         https://www.c64-wiki.com/wiki/Bank_Switching#CPU_Control_Lines
        */
        public void updateMemoryBanks()
        {
            if ( (memory[0x01] & (byte)Latch.LORAM) != 0 )
                loadToMemory(basicRom, 0xa000);
            if ( (memory[0x01] & (byte)Latch.HIRAM) != 0 )
                loadToMemory(kernalRom, 0xe000);
            if ( (memory[0x01] & (byte)Latch.CHAREN) != 0 )
                log.Debug("ToDo: CHAREN is set, I/O should be mapped");
            else
                loadToMemory(characterRom, 0xd000);
        }

        public void loadToMemory(byte[] src, ushort offset)
        {
            Array.Copy(src, 0x00, memory, offset, src.Length);
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
                    string.Format("{0}h: {1}\t{2}",
                        i.ToString("x4"),
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace brainfuck
{
    class Program
    {
        public static long PROCESS_ALL_ACCESS = (0x000F0000L | 0x00100000L | 0xFFF);
        public static int moduleAddr;
        public static int index = 0;
        public static int ii = 0;
        public static int arg0 = 0;
        public static int arg1 = 0;
        public static int[] block = new int[200000];
        public static IntPtr handle;

        [DllImport("kernel32", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess,
            IntPtr bInheritHandle,
            IntPtr dwProcessId
        );
        [DllImport("kernel32", SetLastError = true)]
        public static extern int ReadProcessMemory(
           IntPtr hProcess,
           int lpBase,
           byte[] lpBuffer,
           int nSize,
           int lpNumberOfBytesRead
        );
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten
        );
        [DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern int GetAsyncKeyState(int vKey);

        public static int RPMemory(IntPtr handle, int address)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(handle, address, buffer, buffer.Length, 0);
            return BitConverter.ToInt32(buffer, 0);
        }
        public static void WPMemory(IntPtr handle, int address, int val)
        {
            var array = BitConverter.GetBytes(val);
            int bytesWritten;
            WriteProcessMemory(handle, (IntPtr)address, array, (uint)array.Length, out bytesWritten);
        }
        public static string[] Read(string file)
        {
            string text = System.IO.File.ReadAllText(file);
            string beg = text.Substring(0, text.IndexOf(";"));
            string comm = text.Substring(text.IndexOf(";") + 1);
            string[] done = {beg, comm};
            return done;
        }
        static bool GetModule(Process process, string modName)
        {
            foreach (ProcessModule modul in process.Modules)
            {
                if (modul.ModuleName == modName)
                {
                    moduleAddr = (int)modul.BaseAddress;
                    return true;
                }
            }
            return false;
        }
        static void Comms(char comm, string a, IntPtr handle)
        {
            if (comm == '+')
            {
                block[index] += 1;
            }
            if (comm == '-')
            {
                block[index] -= 1;
            }
            if (comm == '>')
            {
                index += 1;
            }
            if (comm == '<')
            {
                if (index != 0)
                {
                    index -= 1;
                }
                else
                {
                    Console.WriteLine("Out of boundary.");
                }
            }
            if (comm == '[')
            {
                if (block[index] == 0)
                {
                    int loop = 1;
                    while (loop > 0)
                    {
                        ii++;
                        char c = a[ii];
                        if (c == '[')
                        {
                            loop++;
                        }
                        else
                        if (c == ']')
                        {
                            loop--;
                        }
                    }
                }
            }
            if (comm == ']')
            {
                int loop = 1;
                while (loop > 0)
                {
                    ii--;
                    char c = a[ii];
                    if (c == '[')
                    {
                        loop--;
                    }
                    else
                    if (c == ']')
                    {
                        loop++;
                    }
                }
                ii--;
            }
            if (comm == '.')
            {
                Console.Write(/*(char)*/block[index]); //changed it, so it doesn't convert int to char
            }
            if (comm == ',')
            {
                Console.WriteLine("Input:");
                ConsoleKeyInfo key = Console.ReadKey();
                block[index] = (int)key.KeyChar;
                Console.Clear();
            }
            if (comm == ';')
            {
                Console.WriteLine("Input:");
                Int32.TryParse(Console.ReadLine(), out block[index]);
                Console.Clear();
            }
            if (comm == ':')
            {
                arg0 = block[index];
            }
            if (comm == '(')
            {
                arg1 = index;
                block[arg1] = RPMemory(handle, arg0);
            }
            if (comm == ')')
            {
                arg1 = block[index];
                WPMemory(handle, arg0, arg1);
            }
            if (comm == '@')
            {
                arg1 = index;
                if(GetAsyncKeyState(arg0) < 0)
                {
                    block[arg1] = 1;
                }
                else
                {
                    block[arg1] = 0;
                }
            }
            if (comm == '*')
            {
                block[index] += moduleAddr;
            }
            ii++;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Filename, or path to file:");
            string[] all = Read(Console.ReadLine());
            Console.Clear();
            try
            {
                Process process = Process.GetProcessesByName(all[0].Substring(0, all[0].IndexOf(":")))[0];
                handle = OpenProcess((int)PROCESS_ALL_ACCESS, IntPtr.Zero, new IntPtr(process.Id));
                if (GetModule(process, all[0].Substring(all[0].IndexOf(":")+1)))
                {
                    Console.WriteLine("Module Found | Baseaddress: {0}", moduleAddr);
                    Thread.Sleep(1000);
                    Console.Clear();

                    while (ii < all[1].Length)
                    {
                        char comm = all[1][ii];
                        Comms(comm, all[1], handle);
                        //Console.WriteLine(all[1].Substring(ii)); //This writes out what commad the program interprets
                    }
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Module doesn't exist");
                }
            }
            catch(IndexOutOfRangeException)
            {
                Console.WriteLine("Process Not Found");
            }
            //I use this, when there is no process to attach to:

            /*while (ii < all[1].Length)
            {
                char comm = all[1][ii];
                Comms(comm, all[1], handle);
                Console.WriteLine(all[1].Substring(ii));
            }
            Console.ReadKey();*/
        }
    }
}

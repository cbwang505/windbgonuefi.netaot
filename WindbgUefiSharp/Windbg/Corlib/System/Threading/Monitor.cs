using System.Runtime.InteropServices;
using EfiSharp;

namespace System.Threading
{
    public static unsafe class Monitor
    
    {
        public static void Enter(object obj)
        {
            Lock();
        }

        public static void Exit(object obj)
        {
            Unlock();
        }

        [DllImport("*")]
        static extern void Lock();

        [DllImport("*")]
        static extern void Unlock();
    }

    public static unsafe class Thread
    {

        public static void Sleep(int multi)
        {
            UefiApplication.SystemTable->BootServices->Sleep(multi);
            return;
        }
    }
  
}

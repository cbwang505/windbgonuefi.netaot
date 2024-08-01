
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{
    public static class Debug
    {
        //temp
       // [DllImport("*")]
       private static  void Panic(string message)
       {
           Console.WriteLine(message);
           Debug.Halt(true);
       }

        public static void WriteLine(string s) 
        {
            /*for(int i = 0; i < s.Length; i++) 
            {
                DebugWrite(s[i]);
            }
            DebugWriteLine();
            s.Dispose();*/

            Console.WriteLine(s);
            return;
        }

        public static void WriteLine()
        {
            DebugWriteLine();
        }

        public static void Write(char c)
        {
            DebugWrite(c);
        }

        [DllImport("NativeUefi", EntryPoint = "DebugBreak")]
        public static extern void DebugBreak();

        public static bool Halt(bool dumpstack=false)
        {
            if (dumpstack)
            {
                System.Diagnostics.Process.DumpThis();
            }
            Console.WriteLine("Vmbus DebugPoint Halt");

            while (true)
            {
                System.Threading.Thread.Sleep(100);
            }
            return true;
        } 
         static void Write(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                DebugWrite(s[i]);
            }
            s.Dispose();
        }
      


       
        internal static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                //RhFailFastReason.InternalError = 1
                Panic(message);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        internal static void Assert(bool condition)
        {
            if (!condition)
            {
                Panic("InternalError");
            }
        }

        [DllImport("*")]
        static extern void DebugWrite(char c);

        [DllImport("*")]
        static extern void DebugWriteLine();
    }
}

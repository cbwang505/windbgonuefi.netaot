using EfiSharp;
using System.Collections.Generic;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace System
{
    //TODO Add beep, https://github.com/fpmurphy/UEFI-Utilities-2019/blob/master/MyApps/Beep/Beep.c
    public static unsafe class Console
    {
        //Read and ReadLine
        //sizeof(ReadKey) = 3 bytes => sizeof(_buffer) = 1.536kb

        private static ushort _bufferIndex;
        private static ushort _bufferLength;
        private const ushort BufferCapacity = 512;

        //These colours are used by efi at boot up without prompting the user and so are used here just to match
        private const ConsoleColor DefaultBackgroundColour = ConsoleColor.Black;
        private const ConsoleColor DefaultForegroundColour = ConsoleColor.Gray;

        static Console()
        {


        }

        //This method is not perfect as it can only be used once for a key
        //i.e. once a key has been detected with this method, consecutive attempts will return false
        // public static bool KeyAvailable => UefiApplication.SystemTable->BootServices->CheckEvent(UefiApplication.In->WaitForKeyEx) == EFI_STATUS.EFI_SUCCESS;





        //TODO Check if this is possible on efi
        /*public static int CursorSize
        {
            [UnsupportedOSPlatform("browser")]
            get { return ConsolePal.CursorSize; }
            [SupportedOSPlatform("windows")]
            set { ConsolePal.CursorSize = value; }
        }*/
        public static int CursorSize => 25;

        //TODO Add SupportedOSPlatformAttribute
        //[SupportedOSPlatform("windows")]
        public static bool NumberLock
        {
            get;
            private set;
        }

        //[SupportedOSPlatform("windows")]
        public static bool CapsLock
        {
            get;
            private set;
        }

        private static EFI_STATUS PartialKeyInterrupt(EFI_KEY_DATA* keyData)
        {
            NumberLock = (keyData->KeyState.KeyToggleState & EFI_KEY_TOGGLE_STATE.EFI_NUM_LOCK_ACTIVE) != 0;
            CapsLock = (keyData->KeyState.KeyToggleState & EFI_KEY_TOGGLE_STATE.EFI_CAPS_LOCK_ACTIVE) != 0;
            return EFI_STATUS.EFI_SUCCESS;
        }

        //[UnsupportedOSPlatform("browser")]
        public static ConsoleColor BackgroundColor
        {
            get => (ConsoleColor)((byte)UefiApplication.Out->Mode->Attribute >> 4);
            set
            {
                //Only lower nibble colours are supported by efi
                if ((uint)value >= 8) return;
                UefiApplication.Out->SetAttribute(((nuint)value << 4) + (uint)ForegroundColor);
            }
        }

        //[UnsupportedOSPlatform("browser")]
        public static ConsoleColor ForegroundColor
        {
            get => (ConsoleColor)(UefiApplication.Out->Mode->Attribute & 0b1111);
            set => UefiApplication.Out->SetAttribute(((nuint)BackgroundColor << 4) + (uint)value);
        }

        public static int BufferWidth
        {
            //[UnsupportedOSPlatform("browser")]
            get
            {
                UefiApplication.Out->QueryMode((nuint)UefiApplication.Out->Mode->Mode, out nuint width, out _);
                return (int)width;
            }
            //[SupportedOSPlatform("windows")]
            //set { }
        }

        public static int BufferHeight
        {
            //[UnsupportedOSPlatform("browser")]
            get
            {
                UefiApplication.Out->QueryMode((nuint)UefiApplication.Out->Mode->Mode, out _, out nuint height);
                return (int)height;
            }
            //[SupportedOSPlatform("windows")]
            //set { }
        }

        //[UnsupportedOSPlatform("browser")]
        public static void ResetColor()
        {
            UefiApplication.Out->SetAttribute(((nuint)DefaultBackgroundColour << 4) + (nuint)DefaultForegroundColour);
        }

        public static bool CursorVisible
        {
            //[SupportedOSPlatform("windows")]
            get => UefiApplication.Out->Mode->CursorVisible;
            //[UnsupportedOSPlatform("browser")]
            set => UefiApplication.Out->EnableCursor(value);
        }

        //TODO Enforce maximum, EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.QueryMode(...)
        //[UnsupportedOSPlatform("browser")]
        public static int CursorLeft
        {
            get => UefiApplication.Out->Mode->CursorColumn;
            set
            {
                if (value >= 0)
                {
                    UefiApplication.Out->SetCursorPosition((nuint)value, (nuint)CursorTop);
                }
            }
        }

        //TODO Enforce maximum, EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.QueryMode(...)
        //[UnsupportedOSPlatform("browser")]
        public static int CursorTop
        {
            get => UefiApplication.Out->Mode->CursorRow;
            set
            {
                if (value >= 0)
                {
                    UefiApplication.Out->SetCursorPosition((nuint)CursorLeft, (nuint)value);
                }
            }
        }

        //TODO Add ValueTuple?
        /// <summary>Gets the position of the cursor.</summary>
        /// <returns>The column and row position of the cursor.</returns>
        /// <remarks>
        /// Columns are numbered from left to right starting at 0. Rows are numbered from top to bottom starting at 0.
        /// </remarks>
        //[UnsupportedOSPlatform("browser")]
        /*public static (int Left, int Top) GetCursorPosition()
        {
            return (CursorLeft, CursorTop);
        }*/

        public static void Clear()
        {
            UefiApplication.Out->ClearScreen();
        }

        //[UnsupportedOSPlatform("browser")]
        //TODO Enforce maximum, EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL.QueryMode(...)
        public static void SetCursorPosition(int left, int top)
        {
            if (left >= 0 && top >= 0)
            {
                UefiApplication.Out->SetCursorPosition((nuint)left, (nuint)top);
            }
        }

        //
        // Give a hint to the code generator to not inline the common console methods. The console methods are
        // not performance critical. It is unnecessary code bloat to have them inlined.
        //
        // Moreover, simple repros for codegen bugs are often console-based. It is tedious to manually filter out
        // the inlined console writelines from them.
        //
        [MethodImpl(MethodImplOptions.NoInlining)]
        //[UnsupportedOSPlatform("browser")]
        //TODO handle control chars
        public static int Read()
        {

            return 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        //[UnsupportedOSPlatform("browser")]
        public static string ReadLine()
        {

            return "";

        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine()
        {
            //TODO Make line terminator changeable
            Write(Environment.NewLine);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(bool value)
        {
            Write(value);
            WriteLine();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char value)
        {
            Write(value);
            WriteLine();
        }

        //TODO Add nullable?
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[] buffer)
        {
            Write(buffer);
            WriteLine();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
            WriteLine();
        }

        //TODO Add decimal type
        /*[MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(decimal value) { }*/

        //TODO check if float algorithm works as well for doubles
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(double value)
        {
            Write(value);
            WriteLine();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(float value)
        {
            Write(value);
            WriteLine();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(int value)
        {
            Write(value);
            WriteLine();
        }

        //TODO Add CLSCompliantAttribute?
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WriteLine(uint value)
        {
            Write(value);
            WriteLine();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(long value)
        {
            Write(value);
            WriteLine();
        }

        //TODO Add CLSCompliantAttribute?
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(ulong value)
        {
            Write(value);
            WriteLine();
        }

        //TODO Add .ToString(), Nullable?
        /*[MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(object? value) { } */
        [DllImport("NativeUefi", EntryPoint = "WriteLineWrapper")]
        public static extern void WriteLineWrapper(IntPtr value);

        [MethodImpl(MethodImplOptions.NoInlining)]
        //TODO Add Nullable?
        public static void WriteLine(string value)
        {
            WriteLineWrapper(value);
        }
        [RuntimeExport("WriteLineReal")]
        public static void WriteLineReal(string value)
        {
            Write(value);
            WriteLine();
        }
        //TODO Add format string
        /*[MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0, object? arg1) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, object? arg0, object? arg1, object? arg2) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string format, params object?[]? arg)
        {
            if (arg == null)                       // avoid ArgumentNullException from String.Format
                - // faster than Out.WriteLine(format, (Object)arg);
            else
                -
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0, object? arg1) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, object? arg0, object? arg1, object? arg2) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string format, params object?[]? arg)
        {
            if (arg == null)                   // avoid ArgumentNullException from String.Format
                - // faster than Out.Write(format, (Object)arg);
            else
                -
        }*/

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(bool value)
        {
            if (value)
            {
                Write("True");
            }
            else
            {
                Write("False");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char value)
        {
            char* pValue = stackalloc char[2];
            pValue[0] = value;
            pValue[1] = '\0';

            Write(pValue);
        }

        //Todo Add nullable?
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char[] buffer)
        {
            if (buffer == null) return;

            fixed (char* pBuffer = buffer)
            {
                UefiApplication.Out->OutputString(pBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(char[] buffer, int index, int count)
        {
            int maxIndex = index + count;
            if (buffer == null || index >= count || maxIndex > buffer.Length) return;

            //TODO Rewrite, this should be possible without a for loop
            char* pBuffer = stackalloc char[count + 1];
            for (int i = 0; i < count; i++)
            {
                pBuffer[i] = buffer[index + i];
            }
            pBuffer[count] = '\0';

            UefiApplication.Out->OutputString(pBuffer);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(double value)
        {
            if (value < 0)
            {
                Write('-');
                value = -value;
            }

            //Print integer component of double
            //TODO Check if iLength will be inaccurate if (ulong)value == 0 or 1
            //17 is used since at a maximum, a double can store that many digits in its mantissa
            int iLength = Write((ulong)value, 17);
            int fLength = 17 - iLength;

            //Print decimal component of double
            Write('.');

            //Test for zeros after the decimal point followed by more numbers, if found, pValue will be printed which is a less accurate method but can handle that
            if ((ulong)((value - (ulong)value) * 10) == 0)
            {
                char* pValue = stackalloc char[fLength + 1];
                value -= (ulong)value;
                for (int i = 0; i < fLength; i++)
                {
                    value *= 10;
                    pValue[i] = (char)((ulong)value % 10 + '0');
                }

                Write(pValue);
                return;
            }

            //This method is more accurate since it avoids repeated multiplication of the number but loses zeros at the front of the decimal part
            long tenPower = 10;
            for (int i = 0; i < fLength - 1; i++)
            {
                tenPower *= 10;
            }

            //Retrieve decimal component of mantissa as integer
            ulong fPart = (ulong)((value - (ulong)value) * tenPower);

            //Print decimal component of double
            Write(fPart, fLength);
        }

        //TODO Add decimal Type
        /*[MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(decimal value) { }*/

        //TODO replace length guess with https://stackoverflow.com/a/6092298, the current implementation breaks for both specific values in a way that is probably fixable but I currently have
        //no clue why and because it cannot handle floating point numbers with large exponents that lead to more than nine total digits(still only nine significant figures though)
        //TODO Once more features are supported, add something like https://github.com/Ninds/Ryu.NET instead of either of these methods
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(float value)
        {
            if (value < 0)
            {
                Write('-');
                value = -value;
            }

            //Print integer component of float
            //TODO Check if iLength will be inaccurate if (ulong)value == 0 or 1
            //9 is used since at a maximum, a float can store that many digits in its mantissa
            int iLength = Write((ulong)value, 9);
            int fLength = 9 - iLength;

            //Print decimal component of float
            Write('.');

            //Test for zeros after the decimal point followed by more numbers, if found, pValue will be printed which is a less accurate method but can handle that
            if ((uint)((value - (uint)value) * 10) == 0)
            {
                char* pValue = stackalloc char[fLength + 1];
                value -= (uint)value;
                for (int i = 0; i < fLength; i++)
                {
                    value *= 10;
                    pValue[i] = (char)((uint)value % 10 + '0');
                }

                Write(pValue);
                return;
            }

            //This method is more accurate since it avoids repeated multiplication of the number but loses zeros at the front of the decimal part
            int tenPower = 10;
            for (int i = 0; i < fLength - 1; i++)
            {
                tenPower *= 10;
            }

            //Retrieve decimal component of mantissa as integer
            uint fPart = (uint)((value - (uint)value) * tenPower);
            //uint fPart2 = (uint)(value * tenPower - (uint)value * tenPower);

            //Print decimal component of float
            Write(fPart, fLength);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(int value)
        {
            //This is needed to prevent value overflowing for -value being >int.MaxValue, I tried simply adding Write((uint)(-value), 1)); but that fails for all negative numbers.
            uint unsignedValue = (uint)value;

            if (value < 0)
            {
                Write('-');
                unsignedValue = (uint)(-value);
            }

            Write(unsignedValue, 10);
        }

        //TODO Add CLSCompliantAttribute?
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(uint value)
        {
            Write(value, 10);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(long value)
        {
            if (value < 0)
            {
                Write('-');
                value = -value;
            }

            Write((ulong)value, 20);
        }

        //TODO Add CLSCompliantAttribute? 
        //[CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(ulong value)
        {
            Write(value, 20);
        }

        private static int Write(ulong value, int decimalLength)
        {
            //It would be possible to use char[] here but that requires freeing afterwards unlike stack allocations where are removed automatically
            char* pValue = stackalloc char[decimalLength + 1];
            sbyte digitPosition = (sbyte)(decimalLength - 1); //This is designed to go negative for numbers with decimalLength digits

            do
            {
                pValue[digitPosition--] = (char)(value % 10 + '0');
                value /= 10;
            } while (value > 0);

            Write(&pValue[digitPosition + 1]);

            //actual length of integer in terms of decimal digits
            return decimalLength - 1 - digitPosition;
        }

        //TODO Add .ToString(), Nullable?
        /*[MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(object? value) { }*/

        [MethodImpl(MethodImplOptions.NoInlining)]
        //TODO Add Nullable?
        public static void WriteOld(string value)
        {
            UefiApplication.Out->OutputString(value);
        }

        [DllImport("NativeUefi", EntryPoint = "OutputStringWrapper")]
        public static extern void OutputStringWrapper(char* value);


        [MethodImpl(MethodImplOptions.NoInlining)]
        //TODO Add Nullable?
        public static void Write(string value)
        {
            if (value == null)
            {
                return;
            }
            fixed (char* pStr = value)
            {
                OutputStringWrapper(pStr);
            }

            return;
        }


        [RuntimeExport("ConsoleOutputString")]
        public static void Print(char* value)
        {
            UefiApplication.Out->OutputString(value);
        }
        public static void Write(char* value)
        {
            OutputStringWrapper(value);
            

            return;
        }



        public static void HexDump(List<byte> bytes, int bytesPerLine = 16)
        {
             HexDump(bytes.ToArray(), bytesPerLine);
             return;

        }


        public static void HexDump(ByteList bytes, int bytesPerLine = 16)
        {
             HexDump(bytes.ToArray(), bytesPerLine);
             return;

        }



        public static void HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null)
            {
                //return "<null>";
                return;
            }

            if (bytes.Length == 0)
            {
                //return "<null>";
                return;
            }

            char spacechar = ' ';
            char dotchr = '.';
            char zerochar = '0';
            string charhex = "0123456789ABCDEF";
            char[] HexChars = charhex.ToCharArray();
            int bytesPerLinedouble = bytesPerLine * 2;
            char[] line = new char[bytesPerLinedouble]; 
            char[] linehex = new char[bytesPerLine];
            int bytesLength = bytes.Length;
            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                for (var k = 0; k < line.Length; k++)
                {
                    line[k] = zerochar;
                }
                for (var k = 0; k < linehex.Length; k++)
                {
                    linehex[k] = dotchr;
                }
                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (i + j < bytesLength)
                    {
                        int hexColumn = j * 2;
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];

                      //  linehex[j] = (b < 32 ? dotchr : (char)b);
                        linehex[j] = string.IsASCII((char)b)?(char)b: dotchr;
                    }
                    else
                    {
                        break;
                    }
                }

                string startstr = i.ToString("x");
                string resultline = "";
                if (startstr.Length < 4)
                {
                    for (int j = 0; j < 4 - startstr.Length; j++)
                    {
                        resultline += zerochar;
                    }
                }
              
                resultline += startstr;
                resultline += spacechar;
                for (var k = 0; k < line.Length; k+=2)
                {

                    resultline += line[k];
                    resultline += line[k+1];
                    resultline += spacechar;
                }

                for (var k = 0; k < linehex.Length; k++)
                {
                    resultline += linehex[k];
                }

                Console.WriteLine(resultline);
                resultline.Dispose();
            }

            return;

        }


        public static string HexDumpOld(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null)
            {
                return "<null>";
            }
            if (bytes.Length == 0)
            {
                return "<null>";
            }

            int bytesLength = bytes.Length;
            string charhex = "0123456789ABCDEF";
            char[] HexChars = charhex.ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                                         // + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                             + bytesPerLine;    // - characters to show the ascii value
                                                // + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)
            char tmp = ' ';
            char dotchr = '.';
            char[] line = System.Linq.Enumerable.RepeatArray(tmp, lineLength);

            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            // StringBuilder result = new StringBuilder(expectedLines * lineLength);
            string result = "";
            WriteLine(line.Length);


            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];


                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    /*if (j > 0 && (j & 7) == 0)
                    {
                        hexColumn++;
                    }*/

                    if (hexColumn > line.Length || charColumn > line.Length)
                    {
                        break;
                    }

                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = tmp;
                        line[hexColumn + 1] = tmp;
                        line[charColumn] = dotchr;
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? dotchr : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                string resultline = (new String(line));
                resultline += (Environment.NewLine);
                result += resultline;

            }

            Write(result);
            return result.ToString();
        }


    }
}
using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace EfiSharp
{
    public static unsafe class UefiApplication
    {
        public static EFI_SYSTEM_TABLE* SystemTable { get; private set; }
        public static EFI_HANDLE ImageHandle { get; private set; }
        public static readonly EFI_HANDLE SerialIoHandle;
        public static EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL* In = null;
        public static EFI_SERIAL_IO_PROTOCOL* SerialIo = null;
        public static EFI_LOADED_IMAGE_PROTOCOL* loadedimage = null;
        public static EFI_DEVICE_PATH_TO_TEXT_PROTOCOL* devpath = null;
        public static EFI_SHELL_PROTOCOL* shelltool = null;

        //TODO Allow printing to standard error
        public static EFI_SIMPLE_TEXT_OUTPUT_PROTOCOL* Out { get; private set; }

        public static IntPtr ImageBase { get; private set; }
        static void Main() { }



        [DllImport("NativeUefi", EntryPoint = "InitializeDebugAgentWindbg")]
        public static extern void InitializeDebugAgentWindbg(EFI_HANDLE ImageHandle,
            EFI_SYSTEM_TABLE* SystemTable,
            int InitFlag,
            IntPtr ImageBase,
            UInt32 ImageSize);

        [DllImport("NativeUefi", EntryPoint = "SetupDebugAgentEnvironmentWindbg")]
        public static extern void SetupDebugAgentEnvironmentWindbg(EFI_HANDLE ImageHandle,
            EFI_SYSTEM_TABLE* SystemTable,
            IntPtr ImageBase);
            //IntPtr Context);

        [DllImport("NativeUefi", EntryPoint = "DumpPdb")]
        public static extern void DumpPdb(IntPtr modbase);


        [DllImport("NativeUefi", EntryPoint = "mainexp")]
        public static extern void mainexp();



        private static unsafe UInt32 FetchModulePdbInfo()
        {
            UInt32 ret = 0;
            EFI_STATUS status = EFI_STATUS.EFI_SUCCESS;
            ulong filesize = 0;
            SHELL_FILE_HANDLE filehdl = null;
            IntPtr filebuf = IntPtr.Zero;
            string modfile = @"fs0:\windbg.efi";
            fixed (char* pStr = modfile)
            {
                status = shelltool->OpenFileByName(pStr, &filehdl, 1);
                if (status != EFI_STATUS.EFI_SUCCESS)
                {
                    Console.WriteLine("FetchModulePdbInfo OpenFileByName Failed " + status.ToString("x"));
                    return ret;
                }

                status = shelltool->GetFileSize(filehdl, &filesize);
                if (status != EFI_STATUS.EFI_SUCCESS)
                {
                    Console.WriteLine("FetchModulePdbInfo GetFileSize Failed " + status.ToString("x"));
                    return ret;
                }

                Console.WriteLine("FetchModulePdbInfo  :=> " + modfile + " GetFileSize :=> " + filesize.ToString("x"));

                filebuf = StartupCodeHelpers.malloc(filesize);

                status = shelltool->ReadFile(filehdl, &filesize, (void*)filebuf);
                if (status != EFI_STATUS.EFI_SUCCESS)
                {
                    Console.WriteLine("FetchModulePdbInfo ReadFile Failed " + status.ToString("x"));
                    return ret;
                }

                // IntPtr filesizebase = new IntPtr(filesize);
                IntPtr filesizebase = new IntPtr(0x30000);
                System.Diagnostics.Process.MaxFunctionAddress = ImageBase + filesizebase;

              //  DumpPdb(filebuf);
            }

            return PELoader.FetchCheckSum(filebuf);
        }

        [RuntimeExport("EfiMain")]
        private static unsafe long EfiMain(EFI_HANDLE imageHandle, EFI_SYSTEM_TABLE* systemTable)
        {




            SystemTable = systemTable;
            ImageHandle = imageHandle;
            //Prevent system reboot after 5 minutes
            SystemTable->BootServices->SetWatchdogTimer(0, 0);
            //Console Setup
            SetupExtendedConsoleinput(out In);
            Out = SystemTable->ConOut;



            EFI_STATUS status = SetupLoadedimage(imageHandle, out loadedimage);

            status = SetupLoadedShell(imageHandle, out shelltool);
            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                status = SetupLoadedShellSub(imageHandle, out shelltool);
            }
            status = SetupDevPath(imageHandle, out devpath);
          

            ImageBase = loadedimage->ImageBase;
            UInt32 ImageSize = (UInt32)loadedimage->ImageSize;
           
           
            IntPtr filesizebase = new IntPtr(0x30000);
            System.Diagnostics.Process.MaxFunctionAddress = ImageBase + filesizebase;
            UInt32 CheckSum = FetchModulePdbInfo();

         
            /*
            status = SetupSerialIo(imageHandle, out SerialIo); 
            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                status = SetupSerialIoSub(imageHandle, out SerialIo);
            }
            */
            char* pathtext = devpath->ConvertDevicePathToText(loadedimage->FilePath, false, true);
            pathtext += 1;
            string efipath = new string(pathtext);
            Console.WriteLine("EfiPath:=>" + efipath);
            System.Diagnostics.Process.RunGlobalPE(ImageBase);
           
            ByteList Buffer = new ByteList(4);

            InitializeDebugAgentWindbg(imageHandle, systemTable, 0, ImageBase, ImageSize);

            Console.WriteLine("ImageBase:=>" + ImageBase.ToString("x") + ",ImageSize:=>" + ImageSize.ToString("x") + ",CheckSum:=>" + CheckSum.ToString("x"));

            //Debug.Halt();


            SetupDebugAgentEnvironmentWindbg(imageHandle, systemTable,ImageBase);

            /*mainexp();
            Debug.Halt();*/


            WINDBG_SYMBOLS_INFO pSyntheticSymbolInfo = new WINDBG_SYMBOLS_INFO();
            pSyntheticSymbolInfo.BaseOfDll = ImageBase;
            pSyntheticSymbolInfo.SizeOfImage = ImageSize;
            pSyntheticSymbolInfo.CheckSum = CheckSum;
            pSyntheticSymbolInfo.ProcessId = 0;
            pSyntheticSymbolInfo.SymbolPathBuffer = efipath;
            Windbg.KdpSymbolReportSyntheticWindbg(pSyntheticSymbolInfo);
            Debug.DebugBreak();
            Debug.DebugBreak();
            return 0;
            Console.WriteLine("fake");

            string[] ls1 = new string[0x2];
            ls1[0] = "ls";
            ls1[1] = "ls1";
            Console.WriteLine(ls1[0]);
            List<string> ls = new List<string>();

            ls.Add("fake1");
            ls.Add("fake2");
            ls.Add("fake3");
            ls.Add("fake4");
            ls.Add("fake5");
            ls.Add("fake6");

            /*for (int i = 0; i < ls.Count; i++)
            {
                Console.WriteLine(ls[i]);

                // Console.ConsoleSetup();

            }*/
            foreach (string o in ls)
            {
                // Console.WriteLine(o);
            }
            foreach (string o in ls1)
            {
                Console.WriteLine(o);
            }


            KD_PACKET pck = new KD_PACKET(true);

            uint len = sizeof(KD_PACKET);
            uint lendata = pck.GetRawDataSize();


            Console.WriteLine("KD_PACKET len :=>" + len.ToString("x") + ",lendata :=>" + lendata.ToString("x"));



            List<int> lsi = Enumerable.Range(0, 100);

            Console.WriteLine(lsi.Count);
            List<byte> bts = lsi.Select(h =>
            {
                byte ret = Unsafe.As<int, byte>(ref h);
                return ret;
            });

            Console.HexDump(bts);
            return 0;


        }

        //Note that there is no need to close protocols opened with EFI_OPEN_PROTOCOL.Get_Protocol
        private static EFI_STATUS SetupExtendedConsoleinput(out EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL* protocol)
        {
            EFI_STATUS status = SystemTable->BootServices->OpenProtocol(SystemTable->ConsoleInHandle,
                EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL.EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL_GUID, out void* pProtocol,
                ImageHandle, EFI_HANDLE.NullHandle, EFI_OPEN_PROTOCOL.GET_PROTOCOL);
            protocol = (EFI_SIMPLE_TEXT_INPUT_EX_PROTOCOL*)pProtocol;
            return status;
        }

        private static EFI_STATUS SetupLoadedimage(EFI_HANDLE imageHandle, out EFI_LOADED_IMAGE_PROTOCOL* protocol)
        {
            EFI_STATUS status = SystemTable->BootServices->HandleProtocol(imageHandle, EFI_GLOBAL.EFI_LOADED_IMAGE_PROTOCOL_GUID, out void* pProtocol);
            protocol = (EFI_LOADED_IMAGE_PROTOCOL*)pProtocol;
            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("EFI_GLOBAL SetupLoadedimage Failed " + status.ToString("x"));
            }
            return status;
        }

        private static EFI_STATUS SetupDevPath(EFI_HANDLE imageHandle, out EFI_DEVICE_PATH_TO_TEXT_PROTOCOL* protocol)
        {
            EFI_STATUS status = SystemTable->BootServices->LocateProtocol(EFI_GLOBAL.EFI_DEVICE_PATH_TO_TEXT_PROTOCOL_GUID, out void* pProtocol);
            protocol = (EFI_DEVICE_PATH_TO_TEXT_PROTOCOL*)pProtocol;
            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("EFI_GLOBAL SetupDevPath Failed " + status.ToString("x"));
            }
            return status;
        }


        private static EFI_STATUS SetupLoadedShell(EFI_HANDLE imageHandle, out EFI_SHELL_PROTOCOL* protocol)
        {
            // EFI_STATUS status = SystemTable->BootServices->HandleProtocol(imageHandle, EFI_GLOBAL.EFI_SHELL_PROTOCOL_GUID, out void* pProtocol);
            EFI_STATUS status = SystemTable->BootServices->OpenProtocol(ImageHandle,
                EFI_GLOBAL.EFI_SHELL_PROTOCOL_GUID, out void* pProtocol,
                ImageHandle, EFI_HANDLE.NullHandle, EFI_OPEN_PROTOCOL.GET_PROTOCOL);

            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("OpenProtocol SetupLoadedShell Failed " + status.ToString("x"));
            }
            protocol = (EFI_SHELL_PROTOCOL*)pProtocol;
            return status;
        }

        private static EFI_STATUS SetupLoadedShellSub(EFI_HANDLE imageHandle, out EFI_SHELL_PROTOCOL* protocol)
        {
            EFI_STATUS status = SystemTable->BootServices->LocateProtocol(EFI_GLOBAL.EFI_SHELL_PROTOCOL_GUID, out void* pProtocol);
            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("LocateProtocol SetupLoadedShell Failed " + status.ToString("x"));
            }

            protocol = (EFI_SHELL_PROTOCOL*)pProtocol;
            return status;
        }

        private static EFI_STATUS SetupSerialIo(EFI_HANDLE imageHandle, out EFI_SERIAL_IO_PROTOCOL* protocol)
        {
            /*EFI_STATUS status = SystemTable->BootServices->OpenProtocol(SerialIoHandle,
                EFI_GLOBAL.EFI_DEBUG_PORT_PROTOCOL_GUID, out void* pProtocol,
                ImageHandle, EFI_HANDLE.NullHandle, EFI_OPEN_PROTOCOL.BY_DRIVER | EFI_OPEN_PROTOCOL.EXCLUSIVE);*/


            EFI_STATUS status = SystemTable->BootServices->HandleProtocol(imageHandle, EFI_GLOBAL.EFI_SERIAL_IO_PROTOCOL_GUID, out void* pProtocol);
            protocol = (EFI_SERIAL_IO_PROTOCOL*)pProtocol;

            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("HandleProtocol SetupSerialIo Failed " + status.ToString("x"));
            }
            return status;
        }

        private static EFI_STATUS SetupSerialIoSub(EFI_HANDLE imageHandle, out EFI_SERIAL_IO_PROTOCOL* protocol)
        {
            /*EFI_STATUS status = SystemTable->BootServices->OpenProtocol(SerialIoHandle,
                EFI_GLOBAL.EFI_DEBUG_PORT_PROTOCOL_GUID, out void* pProtocol,
                ImageHandle, EFI_HANDLE.NullHandle, EFI_OPEN_PROTOCOL.BY_DRIVER | EFI_OPEN_PROTOCOL.EXCLUSIVE);*/


            EFI_STATUS status = SystemTable->BootServices->LocateProtocol(EFI_GLOBAL.EFI_SERIAL_IO_PROTOCOL_GUID, out void* pProtocol);
            protocol = (EFI_SERIAL_IO_PROTOCOL*)pProtocol;

            if (status != EFI_STATUS.EFI_SUCCESS)
            {
                Console.WriteLine("LocateProtocol SetupSerialIo Failed " + status.ToString("x"));
            }
            return status;
        }
    }
}

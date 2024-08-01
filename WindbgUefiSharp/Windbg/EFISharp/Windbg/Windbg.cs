using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace EfiSharp
{
    public class Windbg
    {
        [DllImport("NativeUefi", EntryPoint = "UefiMemoryPresent")]
        public static extern bool UefiMemoryPresent(IntPtr StartingAddr, UInt32 Size);

        [DllImport("NativeUefi", EntryPoint = "HvMemoryReadPresent")]
        public static extern bool HvMemoryReadPresent(IntPtr StartingAddr);

        [DllImport("NativeUefi", EntryPoint = "GetApicTimerCurrentCount")]
        public static extern UInt32 GetApicTimerCurrentCount();

        [DllImport("NativeUefi", EntryPoint = "KdpDeleteBreakpoint")]
        public static extern bool KdpDeleteBreakpoint(UInt32 BpEntry);

        [DllImport("NativeUefi", EntryPoint = "KdpAddBreakpoint")]
        public static extern UInt32 KdpAddBreakpoint(IntPtr Address);

        [RuntimeExport("KdpSymbolReportSynthetic")]
        public static unsafe KCONTINUE_STATUS
            KdpSymbolReportSynthetic(KD_SYMBOLS_INFO* pSyntheticSymbolInfo)
        {
            Console.WriteLine("BaseOfDll:=>" + pSyntheticSymbolInfo->BaseOfDll.ToString("x") + ",CheckSum:=>" + pSyntheticSymbolInfo->CheckSum.ToString("x") + ",SizeOfImage:=>" + pSyntheticSymbolInfo->SizeOfImage.ToString("x"));
            string sympath = new string(pSyntheticSymbolInfo->SymbolPathBuffer);
            Console.WriteLine("SymbolPathBuffer:=>" + sympath);

            KdpSymbol(pSyntheticSymbolInfo, sympath, true);
            return KCONTINUE_STATUS.ContinueSuccess;
        }

        public static unsafe KCONTINUE_STATUS KdpSymbolReportSyntheticWindbg(WINDBG_SYMBOLS_INFO pSyntheticSymbolInfo)
        {
            Console.WriteLine("BaseOfDll:=>" + pSyntheticSymbolInfo.BaseOfDll.ToString("x") + ",CheckSum:=>" + pSyntheticSymbolInfo.CheckSum.ToString("x") + ",SizeOfImage:=>" + pSyntheticSymbolInfo.SizeOfImage.ToString("x"));

            KdpSymbolWindbg(pSyntheticSymbolInfo, true);
            return KCONTINUE_STATUS.ContinueSuccess;
        }


        public static UInt32 CurrentPacketId = HvDef.INITIAL_PACKET_ID | HvDef.SYNC_PACKET_ID;
        public static UInt32 RemotePacketId = HvDef.INITIAL_PACKET_ID | HvDef.SYNC_PACKET_ID;

        public static unsafe void KdpSymbol(KD_SYMBOLS_INFO* SymbolInfo, string sympath, bool sendonce)
        {
            DBGKD_ANY_WAIT_STATE_CHANGE WaitStateChange = new DBGKD_ANY_WAIT_STATE_CHANGE();
            WaitStateChange.NewState = HvDef.DbgKdLoadSymbolsStateChange;
            WaitStateChange.ProcessorLevel = 0;
            WaitStateChange.Processor = 0;
            WaitStateChange.NumberProcessors = 1;
            WaitStateChange.Thread = 1;
            WaitStateChange.ProgramCounter = 1;

            WaitStateChange.LoadSymbols.UnloadSymbols = 0;
            WaitStateChange.LoadSymbols.BaseOfDll = (UInt64)SymbolInfo->BaseOfDll;
            WaitStateChange.LoadSymbols.ProcessId = 0;
            WaitStateChange.LoadSymbols.CheckSum = SymbolInfo->CheckSum;
            WaitStateChange.LoadSymbols.SizeOfImage = SymbolInfo->SizeOfImage;
            ByteList databts;
            if (sendonce)
            {
                string ntpath = @"\SystemRoot\system32\ntoskrnl.exe";
                databts = new ByteList(ntpath.ToByteArray());
                WaitStateChange.LoadSymbols.PathNameLength = ntpath.Length;
            }
            else
            {
                databts = new ByteList(sympath.ToByteArray());
                WaitStateChange.LoadSymbols.PathNameLength = sympath.Length;
            }

            PSTRING MessageHeader = new PSTRING(WaitStateChange.GetRawDataBytes());
            PSTRING MessageData = new PSTRING(databts);
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_CHANGE64;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            WaitStateChange.Dispose();

            KD_PACKET_ALL ret = Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_STATE_MANIPULATE);
            Console.HexDump(ret.MessageHeader.Buffer);
            fixed (byte* pbytes = ret.MessageHeader.Buffer.GetArrayPtr())
            {
                DBGKD_MANIPULATE_STATE64* pManipulateState = (DBGKD_MANIPULATE_STATE64*)pbytes;
                if ((pManipulateState->ApiNumber >= HvDef.DbgKdApiMin && pManipulateState->ApiNumber <= HvDef.DbgKdApiMax))
                {
                    Console.WriteLine("ApiNumber " + pManipulateState->ApiNumber.ToString("x"));
                }
            }
            return;
        }

        public static unsafe KCONTINUE_STATUS KdpDbgKdGetVersionApi()
        {
            DBGKD_MANIPULATE_STATE64 AnyStateChange = new DBGKD_MANIPULATE_STATE64();
            DBGKD_GET_VERSION64 KdVersionBlock = new DBGKD_GET_VERSION64();
            KdVersionBlock.MajorVersion = 0xf;
            KdVersionBlock.MinorVersion = 0x4a61;
            KdVersionBlock.ProtocolVersion = HvDef.DBGKD_64BIT_PROTOCOL_VERSION2;
            KdVersionBlock.KdSecondaryVersion = HvDef.KD_SECONDARY_VERSION_AMD64_CONTEXT;
            KdVersionBlock.Flags = 0x47;
            KdVersionBlock.MachineType = HvDef.IMAGE_FILE_MACHINE_NATIVE;
            KdVersionBlock.MaxPacketType = HvDef.PACKET_TYPE_MAX;
            KdVersionBlock.MaxStateChange = 3;
            KdVersionBlock.MaxManipulate = 0x33;
            KdVersionBlock.Simulation = HvDef.DBGKD_SIMULATION_NONE;
            AnyStateChange.ApiNumber = HvDef.DbgKdGetVersionApi;
            AnyStateChange.GetVersion64 = KdVersionBlock;
            AnyStateChange.ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(AnyStateChange.GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            AnyStateChange.Dispose();

            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;

        }

        public static unsafe KCONTINUE_STATUS KdpRestoreBreakPoint(DBGKD_MANIPULATE_STATE64* pManipulateState
          )
        {
            UInt32 BreakPointHandle = pManipulateState->RestoreBreakPoint.BreakPointHandle;
            KdpDeleteBreakpoint(BreakPointHandle);
            pManipulateState->ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);
            return KCONTINUE_STATUS.ContinueSuccess;
        }

        public static unsafe KCONTINUE_STATUS KdpWriteBreakPoint(DBGKD_MANIPULATE_STATE64* pManipulateState)
        {
            IntPtr BreakPointAddress = new IntPtr(pManipulateState->WriteBreakPoint.BreakPointAddress);
            pManipulateState->WriteBreakPoint.BreakPointHandle =
                KdpAddBreakpoint(BreakPointAddress);
            pManipulateState->ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);
            return KCONTINUE_STATUS.ContinueSuccess;
        }

        public static unsafe KCONTINUE_STATUS KdpReadControlSpace(DBGKD_MANIPULATE_STATE64* pManipulateState, ref KSPECIAL_REGISTERS SpecialRegisters)
        {

            IntPtr TargetBaseAddress = new IntPtr(pManipulateState->ReadMemory.TargetBaseAddress);
            UInt32 Length = pManipulateState->ReadMemory.TransferCount;
            ByteList retdta = new ByteList((int)Length);
            if (TargetBaseAddress == new IntPtr(2))
            {
                ByteList ctxbts = SpecialRegisters.GetRawDataBytes();
                ctxbts.CopyTo(retdta,0, (int)Length);

            }
            else
            {
                retdta.Fill((int)Length);
            }
           
            pManipulateState->ReadMemory.ActualBytesRead = Length;
            pManipulateState->ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = new PSTRING(retdta);
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;

        }


        public static unsafe KCONTINUE_STATUS KdpGetContextEx(DBGKD_MANIPULATE_STATE64* pManipulateState,
           ref CONTEXT Context)
        {

            ByteList ctxbts = Context.GetRawDataBytes();
            int Length = (int)pManipulateState->ContextEx.ByteCount;
            ByteList retdta = new ByteList(Length);
            ctxbts.CopyTo(retdta, (int)pManipulateState->ContextEx.Offset,(int)Length);
            pManipulateState->ReturnStatus = 0;
            pManipulateState->ContextEx.BytesCopied = Length;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = new PSTRING(retdta);
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;
        }


        public static unsafe KCONTINUE_STATUS KdpSetContextEx(DBGKD_MANIPULATE_STATE64* pManipulateState)
        {

          
            int Length = (int)pManipulateState->ContextEx.ByteCount;
            pManipulateState->ReturnStatus = 0;
            pManipulateState->ContextEx.BytesCopied = Length;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;
        }



        public static unsafe KCONTINUE_STATUS KdpReadVirtualMemory(DBGKD_MANIPULATE_STATE64* pManipulateState,
           ref CONTEXT Context)
        {
            UInt32 Length = pManipulateState->ReadMemory.TransferCount;
            IntPtr rspval = new IntPtr(Context.Rsp);
            ByteList retdta = new ByteList((int)Length);
            IntPtr TargetBaseAddress = new IntPtr(pManipulateState->ReadMemory.TargetBaseAddress);
            IntPtr TargetBaseAddressEnd = TargetBaseAddress + Length;
            bool fetched = false;
            if (CachedMemoryMap != null)
            {
                foreach (CachedMemoryRegion mapentry in CachedMemoryMap)
                {
                    if (TargetBaseAddress >= mapentry.MapAddress && TargetBaseAddressEnd <= mapentry.MapAddressEnd)
                    {
                        int offset = (int)(TargetBaseAddress - mapentry.MapAddress);

                        Console.WriteLine("KdpReadVirtualMemory CachedMemoryMap TargetBaseAddress" + TargetBaseAddress.ToString("x") + " MapAddress " + mapentry.MapAddress.ToString("x") + " offset " + offset.ToString("x") + " Length " + Length.ToString("x"));
                        mapentry.Buffer.CopyTo(retdta, offset, (int)Length);
                       
                        fetched = true;
                    }
                }


            }

            if (!fetched)
            {

                if (rspval != IntPtr.Zero && TargetBaseAddress >= rspval - 0x1000 &&
                    TargetBaseAddress < rspval + 0x1000)
                {
                    retdta.From(rspval, 0, 0, (int)Length);

                }
                else
                {
                    if (UefiMemoryPresent(TargetBaseAddress, Length) && HvMemoryReadPresent(TargetBaseAddress))
                    {
                        retdta.From(TargetBaseAddress, 0, 0, (int)Length);
                    }
                    else
                    {
                        retdta.Fill((int)Length);
                    }
                }
            }
            pManipulateState->ReturnStatus = 0;
            pManipulateState->ReadMemory.ActualBytesRead = Length;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = new PSTRING(retdta);
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;
        }

        public static unsafe KCONTINUE_STATUS KdpWriteControlSpace(DBGKD_MANIPULATE_STATE64* pManipulateState)
        {

            UInt32 Length = pManipulateState->WriteMemory.TransferCount;
            
            IntPtr TargetBaseAddress = new IntPtr(pManipulateState->ReadMemory.TargetBaseAddress);
            pManipulateState->WriteMemory.ActualBytesWritten = Length;
            pManipulateState->ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;
        }
        public static unsafe KCONTINUE_STATUS KdpWriteVirtualMemory(DBGKD_MANIPULATE_STATE64* pManipulateState,
          ref  CONTEXT Context)
        {
            UInt32 Length = pManipulateState->WriteMemory.TransferCount;
            IntPtr rspval = new IntPtr(Context.Rsp);
            IntPtr TargetBaseAddress = new IntPtr(pManipulateState->ReadMemory.TargetBaseAddress);
            pManipulateState->WriteMemory.ActualBytesWritten = Length;
            pManipulateState->ReturnStatus = 0;
            PSTRING MessageHeader = new PSTRING(pManipulateState->GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_MANIPULATE;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            //  AnyStateChange.Dispose();
            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            return KCONTINUE_STATUS.ContinueSuccess;
        }

        public static unsafe KCONTINUE_STATUS KdpSendWaitContinue(UInt32 PacketType, ref CONTEXT Context, ref KSPECIAL_REGISTERS SpecialRegisters)
        {
            KCONTINUE_STATUS status = KCONTINUE_STATUS.ContinueSuccess;
            while (status == KCONTINUE_STATUS.ContinueSuccess)
            {


                KD_PACKET_ALL ret = Vmbus.EatPendingManipulatePacketPromise(PacketType);
                if (ret == null)
                {
                    Console.WriteLine("KD_PACKET_ALL null");
                    return KCONTINUE_STATUS.ContinueError;
                }


                if (ret.MessageHeader == null)
                {
                    Console.WriteLine("KD_PACKET_ALL ret.MessageHeader null");
                    return KCONTINUE_STATUS.ContinueError;
                }

                fixed (byte* pbytes = ret.MessageHeader.Buffer.GetArrayPtr())
                {
                    DBGKD_MANIPULATE_STATE64* pManipulateState = (DBGKD_MANIPULATE_STATE64*)pbytes;
                    if (!(pManipulateState->ApiNumber >= HvDef.DbgKdApiMin &&
                          pManipulateState->ApiNumber <= HvDef.DbgKdApiMax))
                    {
                        Console.WriteLine("UnSupport ApiNumber " + pManipulateState->ApiNumber.ToString("x"));

                        return KCONTINUE_STATUS.ContinueError;
                    }
                    else
                    {

                        if (pManipulateState->ApiNumber == HvDef.DbgKdGetVersionApi)
                        {
                            Console.WriteLine("KdpDbgKdGetVersionApi");
                            status = KdpDbgKdGetVersionApi();
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdReadVirtualMemoryApi)
                        {
                            Console.WriteLine("DbgKdReadVirtualMemoryApi");
                            status = KdpReadVirtualMemory(pManipulateState,ref Context);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdWriteVirtualMemoryApi)
                        {
                            Console.WriteLine("DbgKdWriteVirtualMemoryApi");
                            status = KdpWriteVirtualMemory(pManipulateState,ref Context);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdReadControlSpaceApi)
                        {
                            Console.WriteLine("DbgKdReadControlSpaceApi");
                            status = KdpReadControlSpace(pManipulateState,ref SpecialRegisters);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdWriteControlSpaceApi)
                        {
                            Console.WriteLine("DbgKdWriteControlSpaceApi");
                            status = KdpWriteControlSpace(pManipulateState);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdRestoreBreakPointApi)
                        {
                            Console.WriteLine("DbgKdRestoreBreakPointApi");
                            status = KdpRestoreBreakPoint(pManipulateState);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdWriteBreakPointApi)
                        {
                            Console.WriteLine("DbgKdWriteBreakPointApi");
                            status = KdpWriteBreakPoint(pManipulateState);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdClearAllInternalBreakpointsApi)
                        {
                            Console.WriteLine("DbgKdClearAllInternalBreakpointsApi");
                            status = KCONTINUE_STATUS.ContinueSuccess;
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdGetContextExApi)
                        {
                            Console.WriteLine("DbgKdGetContextExApi");
                            status = KdpGetContextEx(pManipulateState,ref Context);
                        }
                        else if (pManipulateState->ApiNumber == HvDef.DbgKdSetContextApi| pManipulateState->ApiNumber == HvDef.DbgKdSetContextExApi)
                        {
                            Console.WriteLine("KdpSetContextEx");
                            status = KdpSetContextEx(pManipulateState);
                        }
                        else
                        {
                            Console.WriteLine("ApiNumber " + pManipulateState->ApiNumber.ToString("x"));
                            status = KCONTINUE_STATUS.ContinueError;
                        }
                    }


                }
            }

            return KCONTINUE_STATUS.ContinueError;
        }

        public static unsafe void KdpSymbolWindbg(WINDBG_SYMBOLS_INFO SymbolInfo, bool sendonce)
        {
            DBGKD_ANY_WAIT_STATE_CHANGE WaitStateChange = new DBGKD_ANY_WAIT_STATE_CHANGE();
            WaitStateChange.NewState = HvDef.DbgKdLoadSymbolsStateChange;
            WaitStateChange.ProcessorLevel = 0;
            WaitStateChange.Processor = 0;
            WaitStateChange.NumberProcessors = 1;
            WaitStateChange.Thread = 1;
            WaitStateChange.ProgramCounter = 1;

            WaitStateChange.LoadSymbols.UnloadSymbols = 0;
            WaitStateChange.LoadSymbols.BaseOfDll = (UInt64)SymbolInfo.BaseOfDll;
            WaitStateChange.LoadSymbols.ProcessId = 0;
            WaitStateChange.LoadSymbols.CheckSum = SymbolInfo.CheckSum;
            WaitStateChange.LoadSymbols.SizeOfImage = SymbolInfo.SizeOfImage;
            string sympath = SymbolInfo.SymbolPathBuffer;
            ByteList databts;
            if (sendonce)
            {
                string ntpath = @"\SystemRoot\system32\ntoskrnl.exe";
                databts = new ByteList(ntpath.ToByteArray());
                WaitStateChange.LoadSymbols.PathNameLength = ntpath.Length;
            }
            else
            {
                databts = new ByteList(sympath.ToByteArray());
                WaitStateChange.LoadSymbols.PathNameLength = sympath.Length;
            }

            PSTRING MessageHeader = new PSTRING(WaitStateChange.GetRawDataBytes());
            PSTRING MessageData = new PSTRING(databts);
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_CHANGE64;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            WaitStateChange.Dispose();

            Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE);

            Console.WriteLine("Vmbus.EatPendingManipulatePacketPromise(HvDef.PACKET_TYPE_KD_ACKNOWLEDGE)");
            /*CONTEXT Context=new CONTEXT();
            while (true)
            {
                KCONTINUE_STATUS  ret= KdpSendWaitContinue(HvDef.PACKET_TYPE_KD_STATE_MANIPULATE,ref Context);
                if (ret != KCONTINUE_STATUS.ContinueSuccess)
                {
                    break;
                }
            }*/

            return;
        }

        [RuntimeExport("KdpReport")]
        public static bool KdpReport(ref EXCEPTION_RECORD64 ExceptionRecord, ref CONTEXT Context,ref KSPECIAL_REGISTERS SpecialRegisters,
            bool SecondChanceException)
        {
            Console.WriteLine("KdpReportExceptionStateChange");
            DBGKD_ANY_WAIT_STATE_CHANGE WaitStateChange = new DBGKD_ANY_WAIT_STATE_CHANGE();
            WaitStateChange.NewState = HvDef.DbgKdExceptionStateChange;

            WaitStateChange.ProcessorLevel = 0;
            WaitStateChange.Processor = 0;
            WaitStateChange.NumberProcessors = 1;
            WaitStateChange.Thread = 1;
            WaitStateChange.ProgramCounter = GetApicTimerCurrentCount();

            WaitStateChange.Exception.ExceptionRecord = ExceptionRecord;
            WaitStateChange.Exception.FirstChance = SecondChanceException ? 0 : 1;

            WaitStateChange.ControlReport.Dr6 = Context.Dr6;

            WaitStateChange.ControlReport.Dr7 = Context.Dr7;


            /* Copy i386 specific segments */
            WaitStateChange.ControlReport.SegCs = (UInt16)Context.SegCs;
            WaitStateChange.ControlReport.SegDs = (UInt16)Context.SegDs;
            WaitStateChange.ControlReport.SegEs = (UInt16)Context.SegEs;
            WaitStateChange.ControlReport.SegFs = (UInt16)Context.SegFs;

            /* Copy EFlags */
            WaitStateChange.ControlReport.EFlags = Context.EFlags;

            /* Set Report Flags */
            WaitStateChange.ControlReport.ReportFlags = HvDef.REPORT_INCLUDES_SEGS;
            if (WaitStateChange.ControlReport.SegCs == HvDef.KGDT64_R0_CODE)
            {
                WaitStateChange.ControlReport.ReportFlags |= HvDef.REPORT_STANDARD_CS;
            }

            WaitStateChange.ControlReport.InstructionCount = 0x10;


            PSTRING MessageHeader = new PSTRING(WaitStateChange.GetRawDataBytes());
            PSTRING MessageData = null;
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.PACKET_LEADER;
            Packet.PacketId = CurrentPacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = HvDef.PACKET_TYPE_KD_STATE_CHANGE64;
            KD_PACKET_ALL packetAll = new KD_PACKET_ALL(Packet, MessageHeader, MessageData);
            Vmbus.KdSendPacketVmbus(packetAll, null);
            WaitStateChange.Dispose();

            while (true)
            {
                KCONTINUE_STATUS ret = KdpSendWaitContinue(HvDef.PACKET_TYPE_KD_STATE_MANIPULATE, ref Context,ref SpecialRegisters);
                if (ret != KCONTINUE_STATUS.ContinueSuccess)
                {
                    break;
                }
            }
            return true;
        }


        [RuntimeExport("KdpSendControlPacket")]
        public static void KdpSendControlPacket(UInt16 PacketType, UInt32 PacketId)
        {
            Console.WriteLine("KdpSendControlPacket");
            KD_PACKET Packet = new KD_PACKET(true);
            Packet.PacketLeader = HvDef.CONTROL_PACKET_LEADER;
            Packet.PacketId = PacketId;
            Packet.ByteCount = 0;
            Packet.Checksum = 0;
            Packet.PacketType = PacketType;
            Vmbus.KdSendPacketVmbus(Packet, null, null, null);
            Packet.Dispose();
            return;
        }

        private static List<CachedMemoryRegion> CachedMemoryMap = null;


        [RuntimeExport("AddCachedMemoryRegion")]
        public static void AddCachedMemoryRegion(IntPtr MapAddress, IntPtr Buffer, UInt32 len)
        {
            if (CachedMemoryMap == null)
            {
                CachedMemoryMap = new List<CachedMemoryRegion>();
            }

            ByteList buf = new ByteList((int)len);

            buf.From(MapAddress, 0, (int)len);


            CachedMemoryMap.Add(new CachedMemoryRegion(buf, MapAddress));

            return;
        }
    }
}

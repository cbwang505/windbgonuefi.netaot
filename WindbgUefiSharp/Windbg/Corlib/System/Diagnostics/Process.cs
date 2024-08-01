using System.Collections.Generic;
using Internal.Runtime;
using Internal.Runtime.CompilerHelpers;
using System.Reflection.PortableExecutable;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EfiSharp;
using Internal.Runtime.NativeFormat;

namespace System.Diagnostics
{

    public class FunctionTraceMap
    {
        public IntPtr FunctionAddress;
        public string FunctionName;

        public FunctionTraceMap(IntPtr functionAddress, string functionName)
        {
            FunctionAddress = functionAddress;
            FunctionName = functionName;
        }
    }
    public static unsafe class Process
    {
        public static void Start(byte[] exe)
        {
            fixed (byte* ptr = exe)
            {
                DOSHeader* doshdr = (DOSHeader*)ptr;
                NtHeaders64* nthdr = (NtHeaders64*)(ptr + doshdr->e_lfanew);

                if (!nthdr->OptionalHeader.BaseRelocationTable.VirtualAddress) return;
                if (nthdr->OptionalHeader.ImageBase != 0x140000000) return;

                byte* newPtr = (byte*)StartupCodeHelpers.malloc(nthdr->OptionalHeader.SizeOfImage);
                StartupCodeHelpers.memset(newPtr, 0, (int)nthdr->OptionalHeader.SizeOfImage);
                StartupCodeHelpers.memcpy(newPtr, ptr, nthdr->OptionalHeader.SizeOfHeaders);

                DOSHeader* newdoshdr = (DOSHeader*)newPtr;
                NtHeaders64* newnthdr = (NtHeaders64*)(newPtr + newdoshdr->e_lfanew);

                IntPtr moduleSeg = IntPtr.Zero;
                SectionHeader* sections = ((SectionHeader*)(newPtr + newdoshdr->e_lfanew + sizeof(NtHeaders64)));
                for (int i = 0; i < newnthdr->FileHeader.NumberOfSections; i++)
                {
                    if (*(ulong*)sections[i].Name == 0x73656C75646F6D2E) moduleSeg = (IntPtr)((ulong)newPtr + sections[i].VirtualAddress);
                    StartupCodeHelpers.memcpy((byte*)((ulong)newPtr + sections[i].VirtualAddress), ptr + sections[i].PointerToRawData, sections[i].SizeOfRawData);
                }
                FixImageRelocations(newdoshdr, newnthdr, (long)((ulong)newPtr - newnthdr->OptionalHeader.ImageBase));

                delegate*<void> p = (delegate*<void>)((ulong)newPtr + newnthdr->OptionalHeader.AddressOfEntryPoint);
                //TO-DO disposing
                StartupCodeHelpers.InitializeModules(moduleSeg);
                //  StartThread(p);
                //StartupCodeHelpers.free((IntPtr)ptr);
            }
        }

        private static bool SecondDump = false;

        [RuntimeExport("DumpStackTracFrame")]
        public static unsafe void DumpStackTracFrame(IntPtr ripval,IntPtr rspval, IntPtr startval, IntPtr endval)
        {
          //  Debug.Assert(SecondDump== false, "DumpStackTracFrame SecondDump");
            IntPtr modbase = UefiApplication.ImageBase;
          //  Debug.Assert(modbase != IntPtr.Zero, "modbase != IntPtr.Zero");
            SecondDump=true;
            IntPtr startcheck = endval -0x100;
            IntPtr rspvalchk = rspval;
            IntPtr difflen = new IntPtr(1);
            int idx = 0;
            DumpFunctionAddressAndName(rspval, ripval);
            while (true)
            {
                if (idx > 10)
                {
                    break;
                }
                if (rspvalchk > startcheck)
                {
                    break;
                }
                Debug.Assert(rspvalchk > startval && rspvalchk < endval, "rspval overflow");


                IntPtr rspvalOldValue = new IntPtr(NativePrimitiveDecoder.ReadUInt64(rspvalchk));


                if (rspvalOldValue != IntPtr.Zero)
                {

                    if (rspvalOldValue > modbase && rspvalOldValue < MaxFunctionAddress)
                    {

                        if (DumpFunctionAddressAndName(rspvalchk, rspvalOldValue))
                        {
                            idx++;
                        }

                       
                    }
                }

                rspvalchk += difflen;

            }
            SecondDump=false;
            Console.WriteLine("Summary modbase:=>" + modbase.ToString("x") + ",rip:=>" + ripval.ToString("x") + ",rsp:=>" + rspval.ToString("x") );
            //Debug.Halt();
            return;
        }

        [DllImport("NativeUefi", EntryPoint = "GetReadyToRunHeader")]
        public static extern IntPtr GetReadyToRunHeader();
        
        
        [DllImport("NativeUefi", EntryPoint = "DumpThis")]
        public static extern void DumpThis();

        public static unsafe void RunGlobalPE(IntPtr ModuleStart)
        {
            bool dumpstack = false;


            //FixImageRelocations(newdoshdr, newnthdr, (long)((ulong)newPtr - newnthdr->OptionalHeader.ImageBase));

            // delegate*<void> p = (delegate*<void>)((ulong)newPtr + newnthdr->OptionalHeader.AddressOfEntryPoint);
            //TO-DO disposing
            // StartupCodeHelpers.InitializeModules(moduleSeg);


            IntPtr moduleSeg = (IntPtr)GetReadyToRunHeader();
            //StartupCodeHelpers.InitializeModulesStaticsAndClassConstructors(moduleSeg);



            IntPtr TypeManagerIndirectionPtr =
                StartupCodeHelpers.GetModuleSection(moduleSeg, ReadyToRunSectionType.TypeManagerIndirection);
            if (TypeManagerIndirectionPtr == IntPtr.Zero)
            {
                Console.WriteLine("TypeManagerIndirectionPtr == IntPtr.Zero");
                return;
            }

            IntPtr InterfaceDispatchTablePtr =
                StartupCodeHelpers.GetModuleSection(moduleSeg, ReadyToRunSectionType.InterfaceDispatchTable);


            if (InterfaceDispatchTablePtr == IntPtr.Zero)
            {
                Console.WriteLine("InterfaceDispatchTablePtr == IntPtr.Zero");
                return;
            }

            TypeManagerHandleRaw* TypeManagerObj = (TypeManagerHandleRaw*)StartupCodeHelpers.malloc(sizeof(TypeManagerHandleRaw));

            TypeManagerObj->OsModuleBase = ModuleStart;

            TypeManagerObj->DispatchMap = InterfaceDispatchTablePtr;


            IntPtr TypeManagerObjData = (IntPtr)(TypeManagerObj);


            Console.WriteLine("TypeManagerObjData:=>" + TypeManagerObjData.ToString("x"));


            *(IntPtr*)TypeManagerIndirectionPtr = TypeManagerObjData;



            IntPtr GCStaticRegionnPtr =
                StartupCodeHelpers.GetModuleSection(moduleSeg, ReadyToRunSectionType.GCStaticRegion);
            if (GCStaticRegionnPtr == IntPtr.Zero)
            {
                Console.WriteLine("GCStaticRegionnPtr == IntPtr.Zero");
                Debug.Halt();
                return;
            }
            const int gcareasize = 0x1000;

            while (true)
            {

                IntPtr GCStaticRegionnPtrOldValue = *(IntPtr*)GCStaticRegionnPtr;
                if (GCStaticRegionnPtrOldValue == IntPtr.Zero)
                {
                    break;
                }
                if (dumpstack)
                {
                    Console.WriteLine("GCStaticRegionnPtrOldValue:=>" + GCStaticRegionnPtrOldValue.ToString("x"));
                }



                IntPtr GCStaticRegionnPtrFixed = StartupCodeHelpers.malloc(gcareasize);

                StartupCodeHelpers.MemSet((byte*)GCStaticRegionnPtrFixed, 0, (int)gcareasize);


                *(IntPtr*)GCStaticRegionnPtrOldValue = GCStaticRegionnPtrFixed;
                if (dumpstack)
                {
                    Console.WriteLine("GCStaticRegionnPtrFixed:=>" + GCStaticRegionnPtrFixed.ToString("x"));


                    IntPtr GCStaticRegionnPtrFixedchk2 = *(IntPtr*)(GCStaticRegionnPtrFixed + 0x10);

                    Console.WriteLine("GCStaticRegionnPtrFixedchk2:=>" + GCStaticRegionnPtrFixedchk2.ToString("x"));
                }

                GCStaticRegionnPtr += IntPtr.Size;
            }
            Dictionary<int, int> methodRvaToTokenMap = new Dictionary<int, int>();
            RunStackTrace(ModuleStart, methodRvaToTokenMap);
            //Debug.Halt();
            //  StartThread(p);
            //StartupCodeHelpers.free((IntPtr)ptr);
            return;
        }
        private static void* ReadRelPtr32(byte* address)
             => address + *(int*)address;


        public static List<FunctionTraceMap> FunctionTokenMap = null;
        public static IntPtr MaxFunctionAddress;
        public static unsafe void RunStackTrace(IntPtr ModuleStart, Dictionary<int, int> methodRvaToTokenMap)
        {

            IntPtr moduleSeg = (IntPtr)GetReadyToRunHeader();
            IntPtr length = 0;
            IntPtr pMap =
                StartupCodeHelpers.GetModuleSectionWithLength(moduleSeg, ReadyToRunSectionType.BlobIdStackTraceMethodRvaToTokenMapping, ref length);
            Debug.Assert(pMap != IntPtr.Zero, "ReadyToRunSectionType.BlobIdStackTraceMethodRvaToTokenMapping == IntPtr.Zero");
            int* rvaToTokenMap = (int*)pMap;
            IntPtr pMapend = pMap + length;

            int rvaToTokenMapEntryCount = (int)((int)length / (2 * sizeof(int)));
            byte* pCurrentend = (byte*)pMapend;
            Console.WriteLine("BlobIdStackTraceMethodRvaToTokenMapping pMap " + pMap.ToString("x") + ",pMapend " + pMapend.ToString("x") + ",rvaToTokenMapEntryCount:=>" + rvaToTokenMapEntryCount.ToString("x"));

            for (int entryIndex = 0; entryIndex < rvaToTokenMapEntryCount; entryIndex++)
            {
                int* pRelPtr32 = &rvaToTokenMap[2 * entryIndex + 0];
                if (*pRelPtr32 == 0)
                {
                    break;
                }
                IntPtr pointer = (IntPtr)((IntPtr)pRelPtr32 + *pRelPtr32);
                int methodRva = (int)(pointer - UefiApplication.ImageBase);
                int token = rvaToTokenMap[2 * entryIndex + 1];

                if (token >> 0x18 != 0x27)
                {
                    continue;
                }

                if (methodRva == 0)
                {
                    continue;
                }

                if (!methodRvaToTokenMap.ContainsKey(methodRva))
                {

                    methodRvaToTokenMap.Add(methodRva, token);
                }
                // 
            }

            DumpMethodRvaToTokenMap(moduleSeg, methodRvaToTokenMap);
            return;

        }

        private static bool DumpFunctionAddressAndName(IntPtr rspval, IntPtr FunPtr)
        {
            int len = FunctionTokenMap.Count;
            IntPtr modbase = UefiApplication.ImageBase;

            if(!(FunPtr > modbase && FunPtr < MaxFunctionAddress))
            {
                return false;
            }

            IntPtr difflen = new IntPtr(1);
            for (int j = 0; j < len; j++)
            {
                int k = j + 1;
                FunctionTraceMap tmpj = FunctionTokenMap[j];
                IntPtr FunPtrj = tmpj.FunctionAddress - modbase;
                IntPtr FunDiff = FunPtr - tmpj.FunctionAddress;
                if (k == len)
                {
                    //  Console.WriteLine("Reach End:=>" + rspval.ToString("x") + ":base+" + FunPtrj.ToString("x") + "="+ FunPtr.ToString("x") + ":=>" + tmpj.FunctionName);
                    //break;
                    return false;
                }
                FunctionTraceMap tmpk = FunctionTokenMap[k];
                if (tmpk.FunctionAddress > FunPtr && FunPtr > tmpj.FunctionAddress - difflen)
                {
                    Console.WriteLine(FunPtr.ToString("x") + ":=>" +":base+" + FunPtrj.ToString("x") + "=" +  tmpj.FunctionName + "+" + FunDiff.ToString("x"));
                    return true;
                    // break;
                }
            }
            return false;
        }

        private static void FunctionTokenMapSort()
        {
            IntPtr MaxFunctionAddressStack=new IntPtr(0);
            int len = FunctionTokenMap.Count;
            for (int i = 0; i < len - 1; i++)
            {
                for (int j = 0; j < len - 1; j++)
                {
                    int k = j + 1;
                    FunctionTraceMap tmpj = FunctionTokenMap[j];
                    FunctionTraceMap tmpk = FunctionTokenMap[k];
                    if (tmpj.FunctionAddress > MaxFunctionAddressStack)
                    {
                        MaxFunctionAddressStack = tmpj.FunctionAddress;
                    }
                    if (tmpk.FunctionAddress > MaxFunctionAddressStack)
                    {
                        MaxFunctionAddressStack = tmpk.FunctionAddress;
                    }
                    if (tmpj.FunctionAddress > tmpk.FunctionAddress|| tmpj.FunctionAddress == tmpk.FunctionAddress) //判断是否满足交换条件
                    {


                        //交换位置
                        FunctionTokenMap[j] = tmpk;
                        FunctionTokenMap[k] = tmpj;

                    }
                }
            }

            Console.WriteLine("MaxFunctionAddressStack:=>" + MaxFunctionAddressStack.ToString("x"));
            return;
        }
        private static void DumpMethodRvaToTokenMap(IntPtr moduleSeg, Dictionary<int, int> methodRvaToTokenMap)
        {
            IntPtr modbase = UefiApplication.ImageBase;
            //  MaxFunctionAddress = 0;
            FunctionTokenMap = new List<FunctionTraceMap>();
            IntPtr length = 0;
            IntPtr pEmbeddedMetadata =
                    StartupCodeHelpers.GetModuleSectionWithLength(moduleSeg, ReadyToRunSectionType.ReflectionMapBlobEmbeddedMetadata, ref length);
            Debug.Assert(pEmbeddedMetadata != IntPtr.Zero, "ReadyToRunSectionType.ReflectionMapBlobEmbeddedMetadata == IntPtr.Zero");
            byte* pCurrent = (byte*)pEmbeddedMetadata;
            byte* pCurrentsave = (byte*)pEmbeddedMetadata;

            if (methodRvaToTokenMap.Count > 0)
            {

                foreach (KeyValuePair<int, int> kv in methodRvaToTokenMap)
                {
                    Debug.Assert(kv != null, "methodRvaToTokenMap.KeyValuePair == IntPtr.Zero");
                    IntPtr methodRva =new IntPtr(kv.Key);
                    IntPtr metthodPtr = methodRva + modbase;

                    if (!(metthodPtr > modbase && metthodPtr < MaxFunctionAddress))
                    {
                        continue;
                    }

                    int token = kv.Value & 0xffffff;
                    pCurrent = (byte*)pEmbeddedMetadata;
                    pCurrent += token;
                    // Console.WriteLine("methodRva:=>" + methodRva.ToString("x") + ",token:=>" + token.ToString("x"));

                    UInt32 memberReference_parent = NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent);
                    if (memberReference_parent == 0)
                    {
                        Console.WriteLine("memberReference_parent == 0");
                        // Debug.Halt();
                    }

                    UInt32 memberReference_name = NativePrimitiveDecoder.DecodeUnsigned(ref pCurrent);
                    if (memberReference_name == 0)
                    {
                        Console.WriteLine("memberReference_name == 0");
                        // Debug.Halt();
                    }

                    IntPtr pCurrentstringsave = pEmbeddedMetadata + memberReference_name + 1;
                    string methodname = string.FromASCII(pCurrentstringsave).Trim();

                    FunctionTraceMap traceMap = new FunctionTraceMap(metthodPtr, methodname);
                    FunctionTokenMap.Add(traceMap);


                }
            }
            else
            {
                Console.WriteLine("methodRvaToTokenMap.Count Is Null Invalied Assembly:=>" + methodRvaToTokenMap.Count.ToString("x"));
                Debug.Halt();
            }

            FunctionTokenMapSort();
            return;
        }

        public static void checkgc()
        {
            IntPtr moduleSeg = (IntPtr)GetReadyToRunHeader();


            IntPtr GCStaticRegionnPtr =
                StartupCodeHelpers.GetModuleSection(moduleSeg, ReadyToRunSectionType.GCStaticRegion);
            if (GCStaticRegionnPtr == IntPtr.Zero)
            {
                Console.WriteLine("GCStaticRegionnPtr == IntPtr.Zero");
                return;
            }

            IntPtr GCStaticRegionnPtrOldValue = *(IntPtr*)GCStaticRegionnPtr;


            Console.WriteLine("GCStaticRegionnPtrOldValue:=>" + GCStaticRegionnPtrOldValue.ToString("x"));

            IntPtr GCStaticRegionnPtrFixed = *(IntPtr*)GCStaticRegionnPtrOldValue;

            Console.WriteLine("GCStaticRegionnPtrFixed:=>" + GCStaticRegionnPtrFixed.ToString("x"));




            IntPtr GCStaticRegionnPtrFixedchk2 = *(IntPtr*)(GCStaticRegionnPtrFixed + 0x10);

            Console.WriteLine("GCStaticRegionnPtrFixedchk2:=>" + GCStaticRegionnPtrFixedchk2.ToString("x"));


            if (GCStaticRegionnPtrFixedchk2 != IntPtr.Zero)
            {

                IntPtr GCStaticRegionnPtrFixedchk3 = *(IntPtr*)(GCStaticRegionnPtrFixedchk2 + 0x20);

                Console.WriteLine("GCStaticRegionnPtrFixedchk3:=>" + GCStaticRegionnPtrFixedchk3.ToString("x"));

            }
        }
        /*[DllImport("*")]
        static unsafe extern void memset(byte* ptr, int c, int count);

        [DllImport("*")]
        static unsafe extern void memcpy(byte* dest, byte* src, ulong count);*/

        /*[DllImport("StartThread")]
        static extern void StartThread(delegate*<void> func);*/

        static void FixImageRelocations(DOSHeader* dos_header, NtHeaders64* nt_header, long delta)
        {
            ulong size;
            long* intruction;
            DataDirectory* reloc_block =
                (DataDirectory*)(nt_header->OptionalHeader.BaseRelocationTable.VirtualAddress +
                    (ulong)dos_header);

            while (reloc_block->VirtualAddress)
            {
                size = (ulong)((reloc_block->Size - sizeof(DataDirectory)) / sizeof(ushort));
                ushort* fixup = (ushort*)((ulong)reloc_block + (ulong)sizeof(DataDirectory));
                for (ulong i = 0; i < size; i++, fixup++)
                {
                    if (10 == *fixup >> 12)
                    {
                        intruction = (long*)(reloc_block->VirtualAddress + (ulong)dos_header + (*fixup & 0xfffu));
                        *intruction += delta;
                    }
                }
                reloc_block = (DataDirectory*)(reloc_block->Size + (ulong)reloc_block);
            }
        }
    }
}
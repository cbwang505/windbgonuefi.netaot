using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    public static class HvDef
    {
        public static UInt32 magichdr = 0x56867960;
        public static UInt32 magichdrend = 0x87283679;
        public static UInt32 magicreplyhdr = 0x15957899;
        public static UInt32 magicreplyhdrend = 0x36133574;
        public static UInt32 BREAKIN_PACKET = 0x62626262;
        public static UInt32 BREAKIN_PACKET_BYTE = 0x62;
        public static UInt32 PACKET_LEADER = 0x30303030;
        public static byte PACKET_LEADER_BYTE = 0x30;
        public static UInt32 CONTROL_PACKET_LEADER = 0x69696969;
        public static byte CONTROL_PACKET_LEADER_BYTE = 0x69;
        public static byte PACKET_TRAILING_BYTE = 0xAA;

        public static UInt32 INITIAL_PACKET_ID = 0x80800000;
        public static UInt32 SYNC_PACKET_ID = 0x00000800;

        //
        // Packet Types
        //
        public const UInt16 PACKET_TYPE_UNUSED = 0;
        public const UInt16 PACKET_TYPE_KD_STATE_CHANGE32 = 1;
        public const UInt16 PACKET_TYPE_KD_STATE_MANIPULATE = 2;
        public const UInt16 PACKET_TYPE_KD_DEBUG_IO = 3;
        public const UInt16 PACKET_TYPE_KD_ACKNOWLEDGE = 4;
        public const UInt16 PACKET_TYPE_KD_RESEND = 5;
        public const UInt16 PACKET_TYPE_KD_RESET = 6;
        public const UInt16 PACKET_TYPE_KD_STATE_CHANGE64 = 7;
        public const UInt16 PACKET_TYPE_KD_POLL_BREAKIN = 8;
        public const UInt16 PACKET_TYPE_KD_TRACE_IO = 9;
        public const UInt16 PACKET_TYPE_KD_CONTROL_REQUEST = 10;
        public const UInt16 PACKET_TYPE_KD_FILE_IO = 11;
        public const byte PACKET_TYPE_MAX = 12;
        //
        // Wait State Change Types
        //
        public static UInt32 DbgKdMinimumStateChange = 0x00003030;
        public static UInt32 DbgKdExceptionStateChange = 0x00003030;
        public static UInt32 DbgKdLoadSymbolsStateChange = 0x00003031;
        public static UInt32 DbgKdCommandStringStateChange = 0x00003032;
        public static UInt32 DbgKdMaximumStateChange = 0x00003033;

        //
        // This is combined with the basic state change code
        // if the state is from an alternate source
        //
        public static UInt32 DbgKdAlternateStateChange = 0x00010000;
        public static UInt32 DbgKdApiMin = 0x00003000;
        public static UInt32 DbgKdApiMax = 0x00003600;

        //
        // Manipulate Types
        //
        public static UInt32 DbgKdMinimumManipulate = 0x00003130;
        public static UInt32 DbgKdReadVirtualMemoryApi = 0x00003130;
        public static UInt32 DbgKdWriteVirtualMemoryApi = 0x00003131;
        public static UInt32 DbgKdGetContextApi = 0x00003132;
        public static UInt32 DbgKdSetContextApi = 0x00003133;
        public static UInt32 DbgKdWriteBreakPointApi = 0x00003134;
        public static UInt32 DbgKdRestoreBreakPointApi = 0x00003135;
        public static UInt32 DbgKdContinueApi = 0x00003136;
        public static UInt32 DbgKdReadControlSpaceApi = 0x00003137;
        public static UInt32 DbgKdWriteControlSpaceApi = 0x00003138;
        public static UInt32 DbgKdReadIoSpaceApi = 0x00003139;
        public static UInt32 DbgKdWriteIoSpaceApi = 0x0000313A;
        public static UInt32 DbgKdRebootApi = 0x0000313B;
        public static UInt32 DbgKdContinueApi2 = 0x0000313C;
        public static UInt32 DbgKdReadPhysicalMemoryApi = 0x0000313D;
        public static UInt32 DbgKdWritePhysicalMemoryApi = 0x0000313E;
        public static UInt32 DbgKdQuerySpecialCallsApi = 0x0000313F;
        public static UInt32 DbgKdSetSpecialCallApi = 0x00003140;
        public static UInt32 DbgKdClearSpecialCallsApi = 0x00003141;
        public static UInt32 DbgKdSetInternalBreakPointApi = 0x00003142;
        public static UInt32 DbgKdGetInternalBreakPointApi = 0x00003143;
        public static UInt32 DbgKdReadIoSpaceExtendedApi = 0x00003144;
        public static UInt32 DbgKdWriteIoSpaceExtendedApi = 0x00003145;
        public static UInt32 DbgKdGetVersionApi = 0x00003146;
        public static UInt32 DbgKdWriteBreakPointExApi = 0x00003147;
        public static UInt32 DbgKdRestoreBreakPointExApi = 0x00003148;
        public static UInt32 DbgKdCauseBugCheckApi = 0x00003149;
        public static UInt32 DbgKdSwitchProcessor = 0x00003150;
        public static UInt32 DbgKdPageInApi = 0x00003151;
        public static UInt32 DbgKdReadMachineSpecificRegister = 0x00003152;
        public static UInt32 DbgKdWriteMachineSpecificRegister = 0x00003153;
        public static UInt32 OldVlm1 = 0x00003154;
        public static UInt32 OldVlm2 = 0x00003155;
        public static UInt32 DbgKdSearchMemoryApi = 0x00003156;
        public static UInt32 DbgKdGetBusDataApi = 0x00003157;
        public static UInt32 DbgKdSetBusDataApi = 0x00003158;
        public static UInt32 DbgKdCheckLowMemoryApi = 0x00003159;
        public static UInt32 DbgKdClearAllInternalBreakpointsApi = 0x0000315A;
        public static UInt32 DbgKdFillMemoryApi = 0x0000315B;
        public static UInt32 DbgKdQueryMemoryApi = 0x0000315C;
        public static UInt32 DbgKdSwitchPartition = 0x0000315D;
        public static UInt32 DbgKdWriteCustomBreakpointApi = 0x0000315E;
        public static UInt32 DbgKdGetContextExApi = 0x0000315F;
        public static UInt32 DbgKdSetContextExApi = 0x00003160;
        public static UInt32 DbgKdMaximumManipulate = 0x00003161;

        //
        // Debug I/O Types
        //
        public static UInt32 DbgKdPrintStringApi = 0x00003230;
        public static UInt32 DbgKdGetStringApi = 0x00003231;

        //
        // Trace I/O Types
        //
        public static UInt32 DbgKdPrintTraceApi = 0x00003330;
        public static UInt16 IMAGE_FILE_MACHINE_NATIVE = 0x8664;


        public static int PACKET_MAX_SIZE = 4000;
        public static int PACKET_HEADER_SIZE = 0x38;


        // Protocol Versions
        //
        public static byte DBGKD_64BIT_PROTOCOL_VERSION1 = 5;
        public static byte DBGKD_64BIT_PROTOCOL_VERSION2 = 6;


        public static byte KD_SECONDARY_VERSION_DEFAULT = 0;
        public static byte KD_SECONDARY_VERSION_AMD64_OBSOLETE_CONTEXT_1 = 0;
        public static byte KD_SECONDARY_VERSION_AMD64_OBSOLETE_CONTEXT_2 = 1;
        public static byte KD_SECONDARY_VERSION_AMD64_CONTEXT = 2;
        public static byte DBGKD_SIMULATION_NONE = 0;


        public static UInt16 REPORT_INCLUDES_SEGS = 0x0001;
        public static UInt16 REPORT_STANDARD_CS = 0x0002;

        public static UInt16 RPL_MASK = 0x0003;
        public static UInt16 MODE_MASK = 0x0001;
        public static UInt16 KGDT64_NULL = 0x0000;
        public static UInt16 KGDT64_R0_CODE = 0x0010;
        public static UInt16 KGDT64_R0_DATA = 0x0018;
        public static UInt16 KGDT64_R3_CMCODE = 0x0020;
        public static UInt16 KGDT64_R3_DATA = 0x0028;
        public static UInt16 KGDT64_R3_CODE = 0x0030;
        public static UInt16 KGDT64_SYS_TSS = 0x0040;
        public static UInt16 KGDT64_R3_CMTEB = 0x0050;
        public static UInt16 KGDT64_R0_LDT = 0x0060;


    }
    public class Utils
    {
        public const UInt32 VSM_PAGE_SIZE = 0x1000;
        public const UInt32 VSM_PAGE_SIZE_DOUBLE = 0x2000;
        public const UInt32 PAGE_SIZE = VSM_PAGE_SIZE;
        public static UInt32 ALIGN_UP(UInt32 x)
        {
            return ((PAGE_SIZE - 1) & x) ? ((x + PAGE_SIZE) & ~(PAGE_SIZE - 1)) : x;
        }

        public static UInt32 ALIGN_UP_FIX(UInt32 x, UInt32 y)
        {
            return ((y - 1) & x) ? ((x + y) & ~(y - 1)) : x;
        }



    }


    public enum vmbus_packet_type : UInt16
    {
        VM_PKT_INVALID = 0x0,
        VM_PKT_SYNCH = 0x1,
        VM_PKT_ADD_XFER_PAGESET = 0x2,
        VM_PKT_RM_XFER_PAGESET = 0x3,
        VM_PKT_ESTABLISH_GPADL = 0x4,
        VM_PKT_TEARDOWN_GPADL = 0x5,
        VM_PKT_DATA_INBAND = 0x6,
        VM_PKT_DATA_USING_XFER_PAGES = 0x7,
        VM_PKT_DATA_USING_GPADL = 0x8,
        VM_PKT_DATA_USING_GPA_DIRECT = 0x9,
        VM_PKT_CANCEL_REQUEST = 0xa,
        VM_PKT_COMP = 0xb,
        VM_PKT_DATA_USING_ADDITIONAL_PKT = 0xc,
        VM_PKT_ADDITIONAL_DATA = 0xd
    };


    public enum VMBUS_ENUM
    {
        VMBUS_MESSAGE_CONNECTION_ID = 1,
        VMBUS_MESSAGE_CONNECTION_ID_4 = 4,
        VMBUS_MESSAGE_PORT_ID = 1,
        VMBUS_EVENT_CONNECTION_ID = 2,
        VMBUS_EVENT_PORT_ID = 2,
        VMBUS_MONITOR_CONNECTION_ID = 3,
        VMBUS_MONITOR_PORT_ID = 3,
        VMBUS_MESSAGE_SINT = 2,
    };
    public enum KDP_STATUS
    {
        KDP_PACKET_RECEIVED = 0,
        KDP_PACKET_TIMEOUT = 1,
        KDP_PACKET_RESEND = 2,
        KDP_PACKET_RECHECK = 3,
        KDP_PACKET_CONTINUE = 4
    }


    /* Version 1 messages */
    public enum vmbus_channel_message_type
    {
        CHANNELMSG_INVALID = 0,
        CHANNELMSG_OFFERCHANNEL = 1,
        CHANNELMSG_RESCIND_CHANNELOFFER = 2,
        CHANNELMSG_REQUESTOFFERS = 3,
        CHANNELMSG_ALLOFFERS_DELIVERED = 4,
        CHANNELMSG_OPENCHANNEL = 5,
        CHANNELMSG_OPENCHANNEL_RESULT = 6,
        CHANNELMSG_CLOSECHANNEL = 7,
        CHANNELMSG_GPADL_HEADER = 8,
        CHANNELMSG_GPADL_BODY = 9,
        CHANNELMSG_GPADL_CREATED = 10,
        CHANNELMSG_GPADL_TEARDOWN = 11,
        CHANNELMSG_GPADL_TORNDOWN = 12,
        CHANNELMSG_RELID_RELEASED = 13,
        CHANNELMSG_INITIATE_CONTACT = 14,
        CHANNELMSG_VERSION_RESPONSE = 15,
        CHANNELMSG_UNLOAD = 16,
        CHANNELMSG_UNLOAD_RESPONSE = 17,
        CHANNELMSG_18 = 18,
        CHANNELMSG_19 = 19,
        CHANNELMSG_20 = 20,
        CHANNELMSG_TL_CONNECT_REQUEST = 21,
        CHANNELMSG_COUNT
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct M128A
    {
        public UInt64 Low64;
        public UInt64 High64;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class KD_PACKET
    {
        public UInt32 PacketLeader;
        public UInt16 PacketType;
        public UInt16 ByteCount;
        public UInt32 PacketId;
        public UInt32 Checksum;

        public KD_PACKET(bool newobj)
        {
            PacketLeader = 0;
            PacketType = 0;
            ByteCount = 0;
            PacketId = 0;
            Checksum = 0;
        }

        public KD_PACKET(UInt16 PacketTypectrl)
        {
            PacketLeader = 0x69696969;
            PacketType = PacketTypectrl;
            ByteCount = 0;
            PacketId = 0;
            Checksum = 0;
        }


    }

    public class KD_PACKET_ALL
    {
        public KD_PACKET Packet;
        public PSTRING MessageHeader;
        public PSTRING MessageData;

        public KD_PACKET_ALL(KD_PACKET packet, PSTRING header, PSTRING data)
        {
            this.Packet = packet;
            this.MessageHeader = header;
            this.MessageData = data;
        }

        public KD_PACKET_ALL()
        {
            this.Packet = new KD_PACKET(true);
            this.MessageHeader = null;
            this.MessageData = null;
        }
        public UInt32 KdpCalculateChecksum(List<byte> rawdatas)
        {
            UInt32 Checksum = 0;
            for (int i = 0; i < rawdatas.Count; i++)
            {
                UInt32 tmp = (UInt32)rawdatas[i];
                Checksum += tmp;
            }

            return Checksum;
        }


        public override void Dispose()
        {
            this.Packet.Dispose();
            if (this.MessageHeader != null)
            {
                this.MessageHeader.Dispose();
            }
            if (this.MessageData != null)
            {
                this.MessageData.Dispose();
            }
            base.Dispose();
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VMBUSRING_HDR
    {
        public UInt32 magic;
        public UInt32 flag;
        public UInt32 msgsize;
        public UInt32 seqnum;
        public UInt32 checksum;
        public UInt32 magicend;

        public VMBUSRING_HDR(bool newobj)
        {
            magic = 0;
            magicend = 0;
            msgsize = 0;
            checksum = 0;
            flag = 0;
            seqnum = 0;
        }
        public VMBUSRING_HDR(UInt32 sizemsg, UInt32 seqnumval) : this(sizemsg, seqnumval, 0)
        {

        }
        public VMBUSRING_HDR(UInt32 sizemsg, UInt32 seqnumval, UInt32 checksummsg)
        {
            magic = HvDef.magichdr;
            magicend = HvDef.magichdrend;
            msgsize = sizemsg;
            checksum = checksummsg;
            seqnum = seqnumval;
            flag = 1;
        }

        public VMBUSRING_HDR(bool reply, UInt32 seqnumreply, UInt32 checksummsg)
        {
            magic = HvDef.magicreplyhdr;
            magicend = HvDef.magicreplyhdrend;
            msgsize = 0;
            checksum = checksummsg;
            seqnum = seqnumreply;
            flag = 2;
        }

        public override string ToString()
        {
            return "VMBUSRING_HDR magic:" + magic.ToString("x") + ",magicend:" + magicend.ToString("x") + ",msgsize:" + msgsize.ToString("x") + ",checksum:" + msgsize.ToString("x") + ",seqnum:" + seqnum.ToString("x") + ",flag:" + flag.ToString("x");
        }
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PSTRING
    {
        public PSTRING(ushort length, ushort maximumLength, ByteList buffer)
        {
            Length = length;
            MaximumLength = maximumLength;
            Buffer = buffer;
        }

        public PSTRING(ByteList buffer)
        {
            Length = (ushort)buffer.Count;
            MaximumLength = (ushort)buffer.Count;
            Buffer = buffer;
        }
        public PSTRING(ByteList buffer, bool newobj)
        {
            Length = (ushort)buffer.Capaciity;
            MaximumLength = (ushort)buffer.Capaciity;
            Buffer = buffer;
        }

        public UInt16 Length;
        public UInt16 MaximumLength;
        public ByteList Buffer;

        public override void Dispose()
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
            }
            base.Dispose();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class CachedMemoryRegion
    {
        public CachedMemoryRegion(ByteList buffer, IntPtr mapAddress)
        {
            Buffer = buffer;
            MapAddress = mapAddress;
            Length = buffer.Count;
        }

        public IntPtr MapAddressEnd
        {
            get
            {
                return MapAddress + Length;
            }
        }
        public UInt32 Length;
        public ByteList Buffer;
        public IntPtr MapAddress;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class kvec
    {
        public kvec(ByteList iovBase, uint iovLen)
        {
            iov_base = iovBase;
            iov_len = iovLen;
        }

        public ByteList iov_base;
        public UInt32 iov_len;
        public override void Dispose()
        {
            if (iov_base != null)
            {
                iov_base.Dispose();

            }
            base.Dispose();
        }
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class VMBUSRAW_HDR
    {
        public UInt32 flags;
        public UInt32 msgsize;
        public override string ToString()
        {
            return "VMBUSRAW_HDR flags:" + flags.ToString("x") + ",msgsize:" + msgsize.ToString("x");
        }
    };
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class vmpacket_descriptor
    {
        public UInt16 type;
        public UInt16 offset8;
        public UInt16 len8;
        public UInt16 flags;
        public UInt64 trans_id;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class KD_CONTEXT
    {
        public UInt32 KdpDefaultRetries;
        public bool KdpControlCPending;
        public bool KdpControlReturn;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct hv_ring_buffer
    {
        /* Offset in bytes from the start of ring data below */
        public UInt32 write_index;

        /* Offset in bytes from the start of ring data below */
        public UInt32 read_index;

        public UInt32 interrupt_mask;

        /*
         * WS2012/Win8 and later versions of Hyper-V implement interrupt
         * driven flow management. The feature bit feat_pending_send_sz
         * is set by the host on the host->guest ring buffer, and by the
         * guest on the guest->host ring buffer.
         *
         * The meaning of the feature bit is a bit complex in that it has
         * semantics that apply to both ring buffers.  If the guest sets
         * the feature bit in the guest->host ring buffer, the guest is
         * telling the host that:
         * 1) It will set the pending_send_sz field in the guest->host ring
         *    buffer when it is waiting for space to become available, and
         * 2) It will read the pending_send_sz field in the host->guest
         *    ring buffer and interrupt the host when it frees enough space
         *
         * Similarly, if the host sets the feature bit in the host->guest
         * ring buffer, the host is telling the guest that:
         * 1) It will set the pending_send_sz field in the host->guest ring
         *    buffer when it is waiting for space to become available, and
         * 2) It will read the pending_send_sz field in the guest->host
         *    ring buffer and interrupt the guest when it frees enough space
         *
         * If either the guest or host does not set the feature bit that it
         * owns, that guest or host must do polling if it encounters a full
         * ring buffer, and not signal the other end with an interrupt.
         */
        public UInt32 pending_send_sz;

        public fixed UInt32 reserved1[12];


        public UInt32 feature_bits;


        public fixed byte reserved2[4028];

        /*
         * Ring data starts here + RingDataStartOffset
         * !!! DO NOT place any fields below this !!!
         */
        public fixed byte buffer[1];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe class hv_ring_buffer_info
    {
        public UInt64 ring_buffer;
        public UInt64 buf;
        // public UInt64 bufpage;
        public UInt32 buf_size;



        public UInt32 ring_datasize;      /* < ring_size */
        public UInt32 priv_read_index;
        /*
         * The ring buffer mutex lock. This lock prevents the ring buffer from
         * being freed while the ring buffer is being accessed.
         */
        // struct mutex ring_buffer_mutex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe class hv_device
    {
        public hv_device()
        {
            recv_buf = new hv_ring_buffer_info();
            send_buf = new hv_ring_buffer_info();
        }

        public UInt32 nvsp_version;


        public byte rescind;
        public byte tx_disable; /* if true, do not wake up queue again */

        /* Receive buffer allocated by us but manages by NetVSP */
        public hv_ring_buffer_info recv_buf;


        /* Send buffer allocated by us */
        public hv_ring_buffer_info send_buf;

        public UInt32 buf_gpadl_handle;
        public UInt32 sig_event;
        public UInt32 child_relid;
        // EFI_EVENT channel_recv_event;
        public byte channel_recv_signal;

        public override string ToString()
        {
            return "hv_device send_buf_ring:" + send_buf.ring_buffer.ToString("x") + ",send_buf:" +
                   send_buf.buf.ToString("x") + ",recv_buf_ring:" + recv_buf.ring_buffer.ToString("x") + ",recv_buf:" +
                   recv_buf.buf.ToString("x") + ",sig_event:" +
                   sig_event.ToString("x");
        }
    }

    public enum KCONTINUE_STATUS : UInt32
    {
        ContinueError = 0,
        ContinueSuccess,
        ContinueProcessorReselected,
        ContinueNextProcessor
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct KD_SYMBOLS_INFO
    {
        public IntPtr BaseOfDll;
        public UInt32 ProcessId;
        public UInt32 CheckSum;
        public UInt32 SizeOfImage;
        public fixed char SymbolPathBuffer[0x100];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WINDBG_SYMBOLS_INFO
    {
        public IntPtr BaseOfDll;
        public UInt32 ProcessId;
        public UInt32 CheckSum;
        public UInt32 SizeOfImage;
        public string SymbolPathBuffer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct EXCEPTION_RECORD64
    {
        public UInt32 ExceptionCode;
        public UInt32 ExceptionFlags;
        public UInt64 ExceptionRecord;
        public UInt64 ExceptionAddress;
        public UInt32 NumberParameters;
        public UInt32 __unusedAlignment;
        public fixed UInt64 ExceptionInformation[15];
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKM_EXCEPTION64
    {
        public EXCEPTION_RECORD64 ExceptionRecord;
        public UInt32 FirstChance;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct DBGKD_CONTROL_REPORT
    {
        public UInt64 Dr6;
        public UInt64 Dr7;
        public UInt32 EFlags;
        public UInt16 InstructionCount;
        public UInt16 ReportFlags;
        public fixed byte InstructionStream[16];
        public UInt16 SegCs;
        public UInt16 SegDs;
        public UInt16 SegEs;
        public UInt16 SegFs;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_LOAD_SYMBOLS64
    {
        public UInt64 PathNameLength;
        public UInt64 BaseOfDll;
        public UInt64 ProcessId;
        public UInt32 CheckSum;
        public UInt32 SizeOfImage;
        public byte UnloadSymbols;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_GET_VERSION64
    {
        public UInt16 MajorVersion;
        public UInt16 MinorVersion;
        public byte ProtocolVersion;
        public byte KdSecondaryVersion;
        public UInt16 Flags;
        public UInt16 MachineType;
        public byte MaxPacketType;
        public byte MaxStateChange;
        public byte MaxManipulate;
        public byte Simulation;
        public UInt16 Unused;
        public UInt64 KernBase;
        public UInt64 PsLoadedModuleList;
        public UInt64 DebuggerDataList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct CONTEXT
    {
        public UInt64 P1Home;
        public UInt64 P2Home;
        public UInt64 P3Home;
        public UInt64 P4Home;
        public UInt64 P5Home;
        public UInt64 P6Home;
        public UInt32 ContextFlags;
        public UInt32 MxCsr;
        public UInt16 SegCs;
        public UInt16 SegDs;
        public UInt16 SegEs;
        public UInt16 SegFs;
        public UInt16 SegGs;
        public UInt16 SegSs;
        public UInt32 EFlags;
        public UInt64 Dr0;
        public UInt64 Dr1;
        public UInt64 Dr2;
        public UInt64 Dr3;
        public UInt64 Dr6;
        public UInt64 Dr7;
        public UInt64 Rax;
        public UInt64 Rcx;
        public UInt64 Rdx;
        public UInt64 Rbx;
        public UInt64 Rsp;
        public UInt64 Rbp;
        public UInt64 Rsi;
        public UInt64 Rdi;
        public UInt64 R8;
        public UInt64 R9;
        public UInt64 R10;
        public UInt64 R11;
        public UInt64 R12;
        public UInt64 R13;
        public UInt64 R14;
        public UInt64 R15;
        public UInt64 Rip;
        public fixed byte u[0x200];
        public fixed byte VectorRegister[0x1a0];
        public UInt64 VectorControl;
        public UInt64 DebugControl;
        public UInt64 LastBranchToRip;
        public UInt64 LastBranchFromRip;
        public UInt64 LastExceptionToRip;
        public UInt64 LastExceptionFromRip;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct KDESCRIPTOR
    {
        public fixed UInt16 Pad[3];
        public UInt16 Limit;
        public UInt64 Base;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KSPECIAL_REGISTERS
    {
        public UInt64 Cr0;
        public UInt64 Cr2;
        public UInt64 Cr3;
        public UInt64 Cr4;
        public UInt64 KernelDr0;
        public UInt64 KernelDr1;
        public UInt64 KernelDr2;
        public UInt64 KernelDr3;
        public UInt64 KernelDr6;
        public UInt64 KernelDr7;
        public KDESCRIPTOR Gdtr;
        public KDESCRIPTOR Idtr;
        public UInt16 Tr;
        public UInt16 Ldtr;
        public UInt32 MxCsr;
        public UInt64 DebugControl;
        public UInt64 LastBranchToRip;
        public UInt64 LastBranchFromRip;
        public UInt64 LastExceptionToRip;
        public UInt64 LastExceptionFromRip;
        public UInt64 Cr8;
        public UInt64 MsrGsBase;
        public UInt64 MsrGsSwap;
        public UInt64 MsrStar;
        public UInt64 MsrLStar;
        public UInt64 MsrCStar;
        public UInt64 MsrSyscallMask;
    }


    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct DBGKD_ANY_WAIT_STATE_CHANGE
    {
        [FieldOffset(0)] public UInt32 NewState;
        [FieldOffset(4)] public UInt16 ProcessorLevel;
        [FieldOffset(6)] public UInt16 Processor;
        [FieldOffset(8)] public UInt32 NumberProcessors;
        [FieldOffset(0x10)] public UInt64 Thread;
        [FieldOffset(0x18)] public UInt64 ProgramCounter;

        [FieldOffset(0x20)] public DBGKM_EXCEPTION64 Exception;
        [FieldOffset(0x20)] public DBGKD_LOAD_SYMBOLS64 LoadSymbols;
        [FieldOffset(0xc0)] public DBGKD_CONTROL_REPORT ControlReport;


        [FieldOffset(0xef)] public byte pad;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_READ_MEMORY64
    {
        public UInt64 TargetBaseAddress;
        public UInt32 TransferCount;
        public UInt32 ActualBytesRead;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_WRITE_MEMORY64
    {
        public UInt64 TargetBaseAddress;
        public UInt32 TransferCount;
        public UInt32 ActualBytesWritten;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_WRITE_BREAKPOINT64
    {
        public UInt64 BreakPointAddress;
        public UInt32 BreakPointHandle;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_RESTORE_BREAKPOINT
    {
        public UInt32 BreakPointHandle;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DBGKD_CONTEXT_EX
    {
        public UInt32 Offset;
        public UInt32 ByteCount;
        public UInt32 BytesCopied;
    }
  

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public unsafe struct DBGKD_MANIPULATE_STATE64
    {
        [FieldOffset(0)] public UInt32 ApiNumber;
        [FieldOffset(4)] public UInt16 ProcessorLevel;
        [FieldOffset(6)] public UInt16 Processor;
        [FieldOffset(8)] public UInt32 ReturnStatus;

        [FieldOffset(0x10)] public DBGKD_GET_VERSION64 GetVersion64;
        [FieldOffset(0x10)] public DBGKD_READ_MEMORY64 ReadMemory;
        [FieldOffset(0x10)] public DBGKD_WRITE_MEMORY64 WriteMemory;
        [FieldOffset(0x10)] public DBGKD_WRITE_BREAKPOINT64 WriteBreakPoint;
        [FieldOffset(0x10)] public DBGKD_RESTORE_BREAKPOINT RestoreBreakPoint;
        [FieldOffset(0x10)] public DBGKD_CONTEXT_EX ContextEx;
        [FieldOffset(0x37)] public byte pad;
    }
}

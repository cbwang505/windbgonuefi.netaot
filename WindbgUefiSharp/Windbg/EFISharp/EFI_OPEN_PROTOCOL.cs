using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    [Flags]
    public enum EFI_OPEN_PROTOCOL : uint
    {
        BY_HANDLE_PROTOCOL = 0x00000001,
        GET_PROTOCOL = 0x00000002,
        TEST_PROTOCOL = 0x00000004,
        BY_CHILD_CONTROLLER = 0x00000008,
        BY_DRIVER = 0x00000010,
        EXCLUSIVE = 0x00000020
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_TPL
    {
        ulong Value;

        public static implicit operator EFI_TPL(ulong value) => new EFI_TPL() { Value = value };
        public static implicit operator ulong(EFI_TPL value) => value.Value;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct _LIST_ENTRY
    {
        public _LIST_ENTRY* Flink;
        public _LIST_ENTRY* Blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct LIST_ENTRY
    {
        public _LIST_ENTRY* Flink;
        public _LIST_ENTRY* Blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_LIST_ENTRY
    {
        public _LIST_ENTRY* Flink;
        public _LIST_ENTRY* Blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FLOCK
    {
        public EFI_TPL Tpl;
        public EFI_TPL OwnerTpl;
        public ulong Lock;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct SHELL_FILE_HANDLE
    {
        void* Value;

        public static implicit operator SHELL_FILE_HANDLE(void* value) => new SHELL_FILE_HANDLE() { Value = value };
        public static implicit operator void*(SHELL_FILE_HANDLE value) => value.Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_SHELL_FILE_INFO
    {
        public EFI_LIST_ENTRY Link;
        public EFI_STATUS Status;
        public char* FullName;
        public char* FileName;
        public SHELL_FILE_HANDLE Handle;
        public EFI_FILE_INFO* Info;
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_TIME
    {
        public ushort Year;       // 1998 - 20XX
        public byte Month;      // 1 - 12
        public byte Day;        // 1 - 31
        public byte Hour;       // 0 - 23
        public byte Minute;     // 0 - 59
        public byte Second;     // 0 - 59
        public byte Pad1;
        public uint Nanosecond; // 0 - 999,999,999
        public short TimeZone;   // -1440 to 1440 or 2047
        public byte Daylight;
        public byte Pad2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_FILE_INFO
    {
        public ulong Size;
        public ulong FileSize;
        public ulong PhysicalSize;
        public EFI_TIME CreateTime;
        public EFI_TIME LastAccessTime;
        public EFI_TIME ModificationTime;
        public ulong Attribute;
        public fixed char FileName[128];
    }
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_SHELL_DEVICE_NAME_FLAGS
    {
        uint Value;

        public static implicit operator EFI_SHELL_DEVICE_NAME_FLAGS(uint value) => new EFI_SHELL_DEVICE_NAME_FLAGS() { Value = value };
        public static implicit operator uint(EFI_SHELL_DEVICE_NAME_FLAGS value) => value.Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_DEVICE_PATH_PROTOCOL
    {
        public byte Type;
        public byte SubType;
        public fixed byte Length[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_SHELL_PROTOCOL
    {
        public readonly delegate*<EFI_HANDLE*, char*, char**, EFI_STATUS*, EFI_STATUS> Execute;
        public readonly delegate*<char*, EFI_STATUS> GetEnv;
        public readonly delegate*<char*, char*, bool, EFI_STATUS> SetEnv;
        public readonly delegate*<char*, bool*, EFI_STATUS> GetAlias;
        public readonly delegate*<char*, char*, bool, bool, EFI_STATUS> SetAlias;
        public readonly delegate*<char*, char*, char**, EFI_STATUS> GetHelpText;
        public readonly delegate*<char*, EFI_STATUS> GetDevicePathFromMap;
        public readonly delegate*<EFI_DEVICE_PATH_PROTOCOL**, EFI_STATUS> GetMapFromDevicePath;
        public readonly delegate*<char*, EFI_STATUS> GetDevicePathFromFilePath;
        public readonly delegate*<EFI_DEVICE_PATH_PROTOCOL*, EFI_STATUS> GetFilePathFromDevicePath;
        public readonly delegate*<EFI_DEVICE_PATH_PROTOCOL*, char*, EFI_STATUS> SetMap;
        public readonly delegate*<char*, EFI_STATUS> GetCurDir;
        public readonly delegate*<char*, char*, EFI_STATUS> SetCurDir;
        public readonly delegate*<char*, ulong, EFI_SHELL_FILE_INFO**, EFI_STATUS> OpenFileList;
        public readonly delegate*<EFI_SHELL_FILE_INFO**, EFI_STATUS> FreeFileList;
        public readonly delegate*<EFI_SHELL_FILE_INFO**, EFI_STATUS> RemoveDupInFileList;
        public readonly delegate*<bool> BatchIsActive;
        public readonly delegate*<bool> IsRootShell;
        public readonly delegate*<void> EnablePageBreak;
        public readonly delegate*<void> DisablePageBreak;
        public readonly delegate*<bool> GetPageBreak;
        public readonly delegate*<EFI_HANDLE, EFI_SHELL_DEVICE_NAME_FLAGS, byte*, char**, EFI_STATUS> GetDeviceName;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_STATUS> GetFileInfo;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_FILE_INFO*, EFI_STATUS> SetFileInfo;
        public readonly delegate*<char*, SHELL_FILE_HANDLE*, ulong, EFI_STATUS> OpenFileByName;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_STATUS> CloseFile;
        public readonly delegate*<char*, ulong, SHELL_FILE_HANDLE*, EFI_STATUS> CreateFile;
        public readonly delegate*<SHELL_FILE_HANDLE, ulong*, void*, EFI_STATUS> ReadFile;
        public readonly delegate*<SHELL_FILE_HANDLE, ulong*, void*, EFI_STATUS> WriteFile;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_STATUS> DeleteFile;
        public readonly delegate*<char*, EFI_STATUS> DeleteFileByName;
        public readonly delegate*<SHELL_FILE_HANDLE, ulong*, EFI_STATUS> GetFilePosition;
        public readonly delegate*<SHELL_FILE_HANDLE, ulong, EFI_STATUS> SetFilePosition;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_STATUS> FlushFile;
        public readonly delegate*<char*, EFI_SHELL_FILE_INFO**, EFI_STATUS> FindFiles;
        public readonly delegate*<SHELL_FILE_HANDLE, EFI_SHELL_FILE_INFO**, EFI_STATUS> FindFilesInDir;
        public readonly delegate*<SHELL_FILE_HANDLE, ulong*, EFI_STATUS> GetFileSize;
        public readonly delegate*<EFI_DEVICE_PATH_PROTOCOL*, SHELL_FILE_HANDLE*, EFI_STATUS> OpenRoot;
        public readonly delegate*<EFI_HANDLE, SHELL_FILE_HANDLE*, EFI_STATUS> OpenRootByHandle;
        public EFI_EVENT ExecutionBreak;
        public uint MajorVersion;
        public uint MinorVersion;
        // Added for Shell 2.1
        public readonly delegate*<EFI_GUID*, char*, EFI_STATUS> RegisterGuidName;
        public readonly delegate*<EFI_GUID*, char**, EFI_STATUS> GetGuidName;
        public readonly delegate*<char*, EFI_GUID*, EFI_STATUS> GetGuidFromName;
        public readonly delegate*<char*, uint*, EFI_STATUS> GetEnvEx;
    }

}

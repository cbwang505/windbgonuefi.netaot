using System;
using System.Runtime.InteropServices;

namespace EfiSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_DEVICE_PATH
    {
        public byte Type;
        public byte SubType;
        public fixed byte Length[2];
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_LOADED_IMAGE_PROTOCOL
    {
        public uint Revision;
        public EFI_HANDLE ParentHandle;
        public EFI_SYSTEM_TABLE* SystemTable;

        // Source location of image
        public EFI_HANDLE DeviceHandle;
        public EFI_DEVICE_PATH* FilePath;
        public void* Reserved;

        // Images load options
        public uint LoadOptionsSize;
        public void* LoadOptions;

        // Location of where image was loaded
        public IntPtr ImageBase;
        public ulong ImageSize;
        public EFI_MEMORY_TYPE ImageCodeType;
        public EFI_MEMORY_TYPE ImageDataType;

        // If the driver image supports a dynamic unload request
        public readonly delegate*<EFI_HANDLE, EFI_STATUS> Unload;
    }


    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct EFI_DEVICE_PATH_TO_TEXT_PROTOCOL
    {
        public readonly delegate*<EFI_DEVICE_PATH*, bool, bool, EFI_STATUS> ConvertDeviceNodeToText;
        public readonly delegate*<EFI_DEVICE_PATH*, bool, bool, char*> ConvertDevicePathToText;
    }
}
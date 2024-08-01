namespace Internal.Runtime
{
    //
    // ReadyToRunSectionType IDs are used by the runtime to look up specific global data sections
    // from each module linked into the final binary. New sections should be added at the bottom
    // of the enum and deprecated sections should not be removed to preserve ID stability.
    //
    // This list should be kept in sync with the runtime version at
    // https://github.com/dotnet/coreclr/blob/master/src/inc/readytorun.h
    //
    public enum ReadyToRunSectionType : int
    {
        //
        // CoreCLR ReadyToRun sections
        //
        CompilerIdentifier = 100,
        ImportSections = 101,
        RuntimeFunctions = 102,
        MethodDefEntryPoints = 103,
        ExceptionInfo = 104,
        DebugInfo = 105,
        DelayLoadMethodCallThunks = 106,
        // 107 is deprecated - it was used by an older format of AvailableTypes
        AvailableTypes = 108,
        InstanceMethodEntryPoints = 109,
        InliningInfo = 110, // Added in v2.1, deprecated in 4.1
        ProfileDataInfo = 111, // Added in v2.2
        ManifestMetadata = 112, // Added in v2.3
        AttributePresence = 113, // Added in V3.1
        InliningInfo2 = 114, // Added in 4.1
        ComponentAssemblies = 115, // Added in 4.1
        OwnerCompositeExecutable = 116, // Added in 4.1

        //
        // CoreRT ReadyToRun sections
        //
        StringTable = 200, // Unused
        GCStaticRegion = 201,
        ThreadStaticRegion = 202,
        InterfaceDispatchTable = 203,
        TypeManagerIndirection = 204,
        EagerCctor = 205,
        FrozenObjectRegion = 206,
        GCStaticDesc = 207,
        ThreadStaticOffsetRegion = 208,
        ThreadStaticGCDescRegion = 209,
        ThreadStaticIndex = 210,
        LoopHijackFlag = 211,
        ImportAddressTables = 212,

        // Sections 300 - 399 are reserved for RhFindBlob backwards compatibility
        ReadonlyBlobRegionStart = 300,
        ReadonlyBlobRegionEnd = 399,
        BlobIdStackTraceMethodRvaToTokenMapping=0x147,
        ReflectionMapBlobEmbeddedMetadata=0x139
    }

    public static class StackTraceDataCommand
    {
        public const byte UpdateOwningType = 0x01;
        public const byte UpdateName = 0x02;
        public const byte UpdateSignature = 0x04;
        public const byte UpdateGenericSignature = 0x08; // Just a shortcut - sig metadata has the info

        public const byte IsStackTraceHidden = 0x10;
    }
}

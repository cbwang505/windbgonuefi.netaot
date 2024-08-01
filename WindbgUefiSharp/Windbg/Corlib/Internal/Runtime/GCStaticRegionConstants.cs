namespace Internal.Runtime
{
    internal static class GCStaticRegionConstants
    {
        /// <summary>
        /// Flag set if the corresponding GCStatic entry has not yet been initialized and
        /// the corresponding EEType pointer has been changed into a instance pointer of
        /// that EEType.
        /// </summary>
        public const int Uninitialized = 0x1;

        /// <summary>
        /// Flag set if the next pointer loc points to GCStaticsPreInitDataNode.
        /// Otherise it is the next GCStatic entry.
        /// </summary>
        public const int HasPreInitializedData = 0x2;

        public const int Mask = Uninitialized | HasPreInitializedData;
    }
}

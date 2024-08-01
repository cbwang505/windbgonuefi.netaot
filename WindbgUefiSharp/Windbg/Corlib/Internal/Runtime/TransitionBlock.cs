using System;
using System.Runtime;

namespace Internal.Runtime
{
    internal struct TransitionBlock
    {
        private IntPtr m_returnBlockPadding;

        private ReturnBlock m_returnBlock;

        private IntPtr m_alignmentPadding;

        private IntPtr m_ReturnAddress;

        public const int InvalidOffset = -1;

        public unsafe static int GetOffsetOfReturnValuesBlock()
        {
            return sizeof(IntPtr);
        }

        public unsafe static int GetOffsetOfArgumentRegisters()
        {
            return sizeof(TransitionBlock);
        }

        public unsafe static byte GetOffsetOfArgs()
        {
            return (byte)sizeof(TransitionBlock);
        }

        public static bool IsStackArgumentOffset(int offset)
        {
            int ofsArgRegs = GetOffsetOfArgumentRegisters();
            return offset >= ofsArgRegs + 32;
        }

        public static bool IsArgumentRegisterOffset(int offset)
        {
            int ofsArgRegs = GetOffsetOfArgumentRegisters();
            if (offset >= ofsArgRegs)
            {
                return offset < ofsArgRegs + 32;
            }
            return false;
        }

        public static int GetArgumentIndexFromOffset(int offset)
        {
            return (offset - GetOffsetOfArgumentRegisters()) / IntPtr.Size;
        }

        public static int GetStackArgumentIndexFromOffset(int offset)
        {
            return (offset - GetOffsetOfArgs()) / 8;
        }

        public static bool IsFloatArgumentRegisterOffset(int offset)
        {
            return offset < 0;
        }

        public static int GetOffsetOfFloatArgumentRegisters()
        {
            return -GetNegSpaceSize();
        }

        public unsafe static int GetNegSpaceSize()
        {
            int negSpaceSize = 0;
            return negSpaceSize + sizeof(FloatArgumentRegisters);
        }

        public static int GetThisOffset()
        {
            return GetOffsetOfArgumentRegisters();
        }
    }

}

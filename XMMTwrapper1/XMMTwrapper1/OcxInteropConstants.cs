using System;

namespace XMMTwrapper1.OcxInteropConstants
{
    internal static class OcxInteropConstants
    {
        // ---- VARTYPE (VARIANT.vt) ----
        public const ushort VT_EMPTY = 0x0000;
        public const ushort VT_I2 = 0x0002;
        public const ushort VT_I4 = 0x0003;
        public const ushort VT_R4 = 0x0004;
        public const ushort VT_R8 = 0x0005;
        public const ushort VT_BSTR = 0x0008;
        public const ushort VT_BOOL = 0x000B;
        public const ushort VT_VARIANT = 0x000C;
        public const ushort VT_UI1 = 0x0011;

        public const ushort VT_BYREF = 0x4000;

        public const ushort VT_BYREF_I2 = (ushort)(VT_BYREF | VT_I2); // 0x4002
        public const ushort VT_BYREF_I4 = (ushort)(VT_BYREF | VT_I4); // 0x4003

        // ---- IDispatch Invoke flags ----
        public const ushort DISPATCH_METHOD = 0x0001;
        public const ushort DISPATCH_PROPERTYGET = 0x0002;
        public const ushort DISPATCH_PROPERTYPUT = 0x0004;
        public const ushort DISPATCH_PROPERTYPUTREF = 0x0008;

        // ---- HRESULTs you’ll see a lot ----
        public const int S_OK = 0x00000000;
        public const int DISP_E_TYPEMISMATCH = unchecked((int)0x80020005);
        public const int DISP_E_BADPARAMCOUNT = unchecked((int)0x8002000E);

        // ---- Architecture note ----
        // In x86: C++ LONG == 32-bit signed == C# int
    }
}
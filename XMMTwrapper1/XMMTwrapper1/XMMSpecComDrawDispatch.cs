using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

internal static class XMMSpecComDrawDispatch
{
    // VARTYPE (use ushort to avoid sign/convert headaches)
    private const ushort VT_I2 = 2;
    private const ushort VT_BYREF = 0x4000;

    // Replace if your actual DISPID differs
    public const int DISPID_SpecDraw = 14;

    // Invoke flags
    private const ushort DISPATCH_METHOD = 0x1;

    [ComImport]
    [Guid("00020400-0000-0000-C000-000000000046")] // IID_IDispatch
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IDispatch
    {
        [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
        [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out IntPtr ppTInfo);

        [PreserveSig]
        int GetIDsOfNames(
            ref Guid riid,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)]
            string[] rgszNames,
            uint cNames,
            uint lcid,
            [MarshalAs(UnmanagedType.LPArray)] int[] rgDispId);

        [PreserveSig]
        int Invoke(
            int dispIdMember,
            ref Guid riid,
            uint lcid,
            ushort wFlags,
            ref DISPPARAMS pDispParams,
            IntPtr pVarResult,
            IntPtr pExcepInfo,
            out uint puArgErr);
    }

    // Correct VARIANT layout: 16 bytes: vt(2) + reserved(6) + data(8)
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    private struct VARIANT
    {
        [FieldOffset(0)] public ushort vt;
        [FieldOffset(2)] public ushort wReserved1;
        [FieldOffset(4)] public ushort wReserved2;
        [FieldOffset(6)] public ushort wReserved3;

        [FieldOffset(8)] public short iVal;     // VT_I2
        [FieldOffset(8)] public IntPtr byref;   // VT_*|VT_BYREF (e.g. piVal)
    }

    /// <summary>
    /// Calls Spectrum.Draw(pFFT As Integer(ByRef), size As Integer, sampfreq As Integer) where VB6 Integer = Int16.
    /// Expects pFftShorts to be a pointer to the first short sample (short*), pinned for the duration of the call.
    /// </summary>
    public static void InvokeSpecDraw(object ocx, IntPtr pFftShorts, short size, short sampHz)
    {
        if (ocx is null) throw new ArgumentNullException(nameof(ocx));
        if (pFftShorts == IntPtr.Zero) throw new ArgumentNullException(nameof(pFftShorts));

        var disp = (IDispatch)ocx;

        // COM rgvarg is reversed: [0]=last arg, [1]=middle, [2]=first
        // VB signature: Draw(pFFT, size, sampfreq)
        // So rgvarg must be: sampfreq, size, pFFT
        VARIANT[] args = new VARIANT[3];

        args[0] = new VARIANT { vt = VT_I2, iVal = sampHz };                         // sampfreq
        args[1] = new VARIANT { vt = VT_I2, iVal = size };                         // size
        args[2] = new VARIANT { vt = (ushort)(VT_I2 | VT_BYREF), byref = pFftShorts }; // pFFT (piVal)

        int cbVar = Marshal.SizeOf<VARIANT>();
        IntPtr pArgs = Marshal.AllocHGlobal(cbVar * args.Length);

        var dp = new DISPPARAMS
        {
            cArgs = args.Length,
            cNamedArgs = 0,
            rgdispidNamedArgs = IntPtr.Zero,
            rgvarg = pArgs
        };

        try
        {
            for (int i = 0; i < args.Length; i++)
                Marshal.StructureToPtr(args[i], pArgs + (i * cbVar), fDeleteOld: false);

            Guid iidNull = Guid.Empty;
            uint argErr;

            int hr = disp.Invoke(
                DISPID_SpecDraw,
                ref iidNull,
                lcid: 0,                 // LOCALE_USER_DEFAULT also fine; 0 usually OK
                wFlags: DISPATCH_METHOD,
                ref dp,
                pVarResult: IntPtr.Zero,
                pExcepInfo: IntPtr.Zero,
                out argErr);

            if (hr != 0)
            {
                // 0x80020005 + argErr=2 -> pFFT VARIANT mismatch
                throw new COMException($"SpecDraw IDispatch.Invoke failed. hr=0x{hr:X8}, argErr={argErr}", hr);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(pArgs);
        }
    }
}

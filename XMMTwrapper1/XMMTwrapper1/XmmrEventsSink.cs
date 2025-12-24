using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


[ComVisible(true)]
[Guid("527A76C5-3C2D-4D68-99EC-B4D2F53CAE74")] // dispinterface IID from OCX
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] // we provide vtable shape explicitly
public interface IDXMMREventsDispatch
{
    // IDispatch (after IUnknown). Return HRESULT (int).
    [PreserveSig] int GetTypeInfoCount(out uint pctinfo);
    [PreserveSig] int GetTypeInfo(uint iTInfo, uint lcid, out ITypeInfo? ppTInfo);

    [PreserveSig]
    int GetIDsOfNames(
        ref Guid riid,
        [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames,
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

[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public sealed class DxmmrEventsSink : StandardOleMarshalObject, IDXMMREventsDispatch
{
    private const int S_OK = 0;
    private const int E_NOTIMPL = unchecked((int)0x80004001);

    public event Action<short[] /*fft*/, short /*size*/, short /*sampfreq*/>? NotifyFftRaw;
    // Managed event: you can handle this in the Form and call axXMMSpec1.Draw(...)
    public event Action<int[] /*XY*/>? NotifyGetXY;

    public volatile bool AcceptFft = true;

    public DxmmrEventsSink()
    {
        Log.Mark($"[Sink] DxmmrEventsSink constructed T{Environment.CurrentManagedThreadId}");
    }

    public int GetTypeInfoCount(out uint pctinfo) { pctinfo = 0; return S_OK; }
    public int GetTypeInfo(uint iTInfo, uint lcid, out ITypeInfo? ppTInfo) { ppTInfo = null; return E_NOTIMPL; }

    public int GetIDsOfNames(ref Guid riid, string[] rgszNames, uint cNames, uint lcid, int[] rgDispId)
        => E_NOTIMPL;

    public int Invoke(
    int dispIdMember,
    ref Guid riid,
    uint lcid,
    ushort wFlags,
    ref DISPPARAMS dp,
    IntPtr pVarResult,
    IntPtr pExcepInfo,
    out uint puArgErr)
    {
        puArgErr = 0;

        //[id(0x0000000b)]
        //void OnNotifyFFT(short* pFFT, short size, short sampfreq);
        if (dispIdMember == 0x0000000b) return InvokeGetFft(
                                        dispIdMember,
                                        ref riid,
                                        lcid,
                                        wFlags,
                                        ref dp,
                                        pVarResult,
                                        pExcepInfo,
                                        out puArgErr);          // FFT event only


        //[id(0x0000000c)]
        //void OnNotifyXY(long* pXY);           //pXY[1024] (X values first 512, Y values second 512
        if (dispIdMember == 0x0000000c) return InvokeGetXY(
                                        dispIdMember,
                                        ref riid,
                                        lcid,
                                        wFlags,
                                        ref dp,
                                        pVarResult,
                                        pExcepInfo,
                                        out puArgErr);          // FFT event only

        //[id(0x00000005)]
        //void OnNotifyNMMR(long* pNMMR);       ?TODO find size
        if (dispIdMember == 0x00000005) return InvokeGetNmmr(
                                dispIdMember,
                                ref riid,
                                lcid,
                                wFlags,
                                ref dp,
                                pVarResult,
                                pExcepInfo,
                                out puArgErr);          // FFT event only

        return S_OK;
    }

    private int InvokeGetFft(
                int dispIdMember,
                ref Guid riid,
                uint lcid,
                ushort wFlags,
                ref DISPPARAMS dp,
                IntPtr pVarResult,
                IntPtr pExcepInfo,
                out uint puArgErr)
    {
        puArgErr = 0;

        // Fast filters first
        if (dispIdMember != 11) return S_OK;          // FFT event only
        if (dp.cArgs != 3) return S_OK;
        if (dp.rgvarg == IntPtr.Zero) return S_OK;
        if (!AcceptFft) return S_OK;

        // COM args are reversed in rgvarg:
        // [0]=sampfreq  [1]=size  [2]=pFFT
        IntPtr vSamp = VariantUtil.ArgVariantPtr(dp.rgvarg, 0);
        IntPtr vSize = VariantUtil.ArgVariantPtr(dp.rgvarg, 1);
        IntPtr vFft = VariantUtil.ArgVariantPtr(dp.rgvarg, 2);

        // Read simple scalar args (your logs show these are VT_I2)
        short sampfreq = VariantUtil.ReadI2(vSamp);
        short size = VariantUtil.ReadI2(vSize);

        // Basic sanity
        if (size <= 0 || size > 32767) return S_OK;

        // FFT pointer must be VT_I2|VT_BYREF (0x4002) in this OCX event
        if (!VariantUtil.IsByRefI2(vFft)) return S_OK;

        // This is the key: get the actual base address of the short[] buffer
        IntPtr pFFT = VariantUtil.ReadByrefPtr(vFft);
        if (pFFT == IntPtr.Zero) return S_OK;

        // Copy immediately while pointer is valid (DO NOT keep pFFT around)
        var fft = new short[size];
        Marshal.Copy(pFFT, fft, 0, size);

        // Minimal optional debug (safe + cheap-ish)
        // Log.Msg($"[FFT11] size={size} sampHz={sampfreq} pFFT=0x{pFFT.ToInt64():X} first={fft[0]} second={fft.Length>1?fft[1]:0}");
        // Log.Msg("[FFT11] preview: " + string.Join(", ", fft.Take(Math.Min(64, fft.Length))));

        //Following calls the Form function to handle raw data
        NotifyFftRaw?.Invoke(fft, size, sampfreq);

        return S_OK;
    }

    private int InvokeGetXY(
                int dispIdMember,
                ref Guid riid,
                uint lcid,
                ushort wFlags,
                ref DISPPARAMS dp,
                IntPtr pVarResult,
                IntPtr pExcepInfo,
                out uint puArgErr)
    {
        puArgErr = 0;

        // Fast filters first
        if (dispIdMember != 0x0000000c) return S_OK;          
        if (dp.cArgs != 1) return S_OK;
        if (dp.rgvarg == IntPtr.Zero) return S_OK;

        //pointer to long XY[1024]
        IntPtr vXY = VariantUtil.ArgVariantPtr(dp.rgvarg, 0);

        // XY pointer must be VT_I4|VT_BYREF (0x4002) in this OCX event
        if (!VariantUtil.IsByRefI4(vXY)) return S_OK;

        // This is the key: get the actual base address of the short[] buffer
        IntPtr pXY = VariantUtil.ReadByrefPtr(vXY);
        if (pXY == IntPtr.Zero) return S_OK;

        // Copy immediately while pointer is valid (DO NOT keep pXY around)
        int size = 1024;                //int X[512] followed by int Y[512]
        var XY = new int[size];
        Marshal.Copy(pXY, XY, 0, size);

        // Minimal optional debug (safe + cheap-ish)
        // Log.Msg($"[FFT11] size={size} sampHz={sampfreq} pFFT=0x{pFFT.ToInt64():X} first={fft[0]} second={fft.Length>1?fft[1]:0}");
        // Log.Msg("[FFT11] preview: " + string.Join(", ", fft.Take(Math.Min(64, fft.Length))));

        //Following calls Form control Spec.Draw
        NotifyGetXY?.Invoke(XY);

        //Following calls the Form function to handle raw data
        //NotifyXYRaw?.Invoke(fft, size, sampfreq);

        return S_OK;
    }

    private int InvokeGetNmmr(
                int dispIdMember,
                ref Guid riid,
                uint lcid,
                ushort wFlags,
                ref DISPPARAMS dp,
                IntPtr pVarResult,
                IntPtr pExcepInfo,
                out uint puArgErr)
    {
        puArgErr = 0;

        // Fast filters first
        if (dispIdMember != 0x00000005) return S_OK;
        if (dp.cArgs != 1) return S_OK;
        if (dp.rgvarg == IntPtr.Zero) return S_OK;

        // COM args are reversed in rgvarg:
        // [0]=Nmmr array of ? longs
        IntPtr vNmmr = VariantUtil.ArgVariantPtr(dp.rgvarg, 0);

        // FFT pointer must be VT_I2|VT_BYREF (0x4002) in this OCX event
        if (!VariantUtil.IsByRefI2(vNmmr)) return S_OK;

        // This is the key: get the actual base address of the short[] buffer
        IntPtr pNmmr = VariantUtil.ReadByrefPtr(vNmmr);
        if (pNmmr == IntPtr.Zero) return S_OK;

        // Copy immediately while pointer is valid (DO NOT keep pFFT around)
        //
/*      typedef struct {
                DWORD m_markfreq;
                DWORD m_spacefreq;
                DWORD m_siglevel;
                DWORD m_sqlevel;
                DWORD m_codeswitch;
                DWORD m_codeview;
                DWORD m_notch1;
                DWORD m_notch2;
                DWORD m_baud;
                DWORD m_fig;
                DWORD m_radiofreq;
                DWORD m_Reserved[53];
        }NMMR;
*/
        int size = 64 * sizeof(long);
        var Nmmr = new long[size];
        Marshal.Copy(pNmmr, Nmmr, 0, size);

        // Minimal optional debug (safe + cheap-ish)
        // Log.Msg($"[FFT11] size={size} sampHz={sampfreq} pFFT=0x{pFFT.ToInt64():X} first={fft[0]} second={fft.Length>1?fft[1]:0}");
        // Log.Msg("[FFT11] preview: " + string.Join(", ", fft.Take(Math.Min(64, fft.Length))));

        //Following calls Form control Spec.Draw
        //NotifyGetXY?.Invoke(fft, size, sampfreq);

        //Following calls the Form function to handle raw data
        //NotifyXYRaw?.Invoke(fft, size, sampfreq);

        return S_OK;
    }
}




internal static class VariantUtil
{
    public const ushort VT_BYREF = 0x4000;
    public const ushort VT_I2 = 0x0002;
    public const ushort VT_I4 = 0x0003;

    // VARIANT layout notes:
    // - vt is at offset 0 (ushort)
    // - value/ptr union begins at offset 8
    // - VARIANT size is 16 bytes on x86, 24 bytes on x64
    private static int VariantSize => (IntPtr.Size == 4) ? 16 : 24;
    private const int VtOffset = 0;
    private const int UnionOffset = 8;

    public static ushort ReadVt(IntPtr pVariant)
        => (ushort)Marshal.ReadInt16(pVariant, VtOffset);

    public static bool IsByRefI2(IntPtr pVariant)
        => ReadVt(pVariant) == (VT_BYREF | VT_I2);

    public static bool IsByRefI4(IntPtr pVariant)
        => ReadVt(pVariant) == (VT_BYREF | VT_I4);

    public static short ReadI2(IntPtr pVariant)
        => (short)Marshal.ReadInt16(pVariant, UnionOffset);

    public static int ReadI4(IntPtr pVariant)
        => Marshal.ReadInt32(pVariant, UnionOffset);

    /// <summary>
    /// For BYREF variants, returns the pointer stored in the union.
    /// For VT_I2|VT_BYREF this is equivalent to VARIANTARG.piVal.
    /// For VT_I4|VT_BYREF this is equivalent to VARIANTARG.plVal.
    /// </summary>
    public static IntPtr ReadByrefPtr(IntPtr pVariant)
        => Marshal.ReadIntPtr(pVariant, UnionOffset);

    public static IntPtr ArgVariantPtr(IntPtr rgvarg, int index)
        => rgvarg + index * VariantSize;

    // Convenience helpers when you *expect* a specific BYREF type.
    public static IntPtr ReadByRefI2PtrOrNull(IntPtr pVariant)
        => IsByRefI2(pVariant) ? ReadByrefPtr(pVariant) : IntPtr.Zero;

    public static IntPtr ReadByRefI4PtrOrNull(IntPtr pVariant)
        => IsByRefI4(pVariant) ? ReadByrefPtr(pVariant) : IntPtr.Zero;

    public static short[] CopyI2Array(IntPtr pI2, int count)
    {
        if (pI2 == IntPtr.Zero) throw new ArgumentNullException(nameof(pI2));
        if (count <= 0) return Array.Empty<short>();

        var dst = new short[count];
        Marshal.Copy(pI2, dst, 0, count);
        return dst;
    }

    public static int[] CopyI4Array(IntPtr pI4, int count)
    {
        if (pI4 == IntPtr.Zero) throw new ArgumentNullException(nameof(pI4));
        if (count <= 0) return Array.Empty<int>();

        var dst = new int[count];
        Marshal.Copy(pI4, dst, 0, count);
        return dst;
    }
}


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
    public event Action<short[] /*fft*/, short /*size*/, short /*sampfreq*/>? NotifyFftManaged;

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

        // Fast filters first
        if (dispIdMember != 11) return S_OK;          // FFT event only
        if (dp.cArgs < 3) return S_OK;
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

        //Following calls Form control Spec.Draw
        //NotifyFftManaged?.Invoke(fft, size, sampfreq);

        //Following calls the Form function to handle raw data
        NotifyFftRaw?.Invoke(fft, size, sampfreq);

        return S_OK;
    }

}




internal static class VariantUtil
{
    public const ushort VT_BYREF = 0x4000;
    public const ushort VT_I2 = 0x0002;
    public const ushort VT_I4 = 0x0003;

    public static ushort ReadVt(IntPtr pVariant) => (ushort)Marshal.ReadInt16(pVariant, 0);

    public static bool IsByRefI2(IntPtr pVariant)
    {
        ushort vt = ReadVt(pVariant);
        return vt == (VT_BYREF | VT_I2);
    }

    public static short ReadI2(IntPtr pVariant)
    {
        // For non-byref VT_I2: value is stored in union at offset 8
        return (short)Marshal.ReadInt16(pVariant, 8);
    }

    public static IntPtr ReadByrefPtr(IntPtr pVariant)
    {
        // For BYREF types: pointer is stored at offset 8 as IntPtr
        return Marshal.ReadIntPtr(pVariant, 8);
    }

    public static IntPtr ArgVariantPtr(IntPtr rgvarg, int index)
    {
        // VARIANT size is 16 bytes on 32-bit, 24 bytes on 64-bit
        int variantSize = (IntPtr.Size == 4) ? 16 : 24;
        return rgvarg + index * variantSize;
    }
}


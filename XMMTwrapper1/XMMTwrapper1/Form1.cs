using AxXMMTLib;
using System.Runtime.InteropServices;


namespace XMMTwrapper1
{
    public partial class Form1 : Form
    {

        // --- NMMR Data Structure and Indexes ---

        // VB6: Private m_nmmr(63) As Long
        // C#: A private array of 64 integers (0 to 63) to store the MMTTY engine status block.
        private int[] m_nmmr = new int[64];

        // VB6: Private m_CurrentSwitch As Long
        private int m_CurrentSwitch = -1;

        // VB6: Private m_CurrentSwitchReady As Long
        private int m_CurrentSwitchReady = 0;

        // --- Critical Constants (Indices for accessing m_nmmr) ---
        // These need to be defined based on the MMTTY interface documentation.
        // Use the values determined by your specific MMTTY/XMMT control interface.

        private const int xr_codeview = 4;      // Index for the View Status flags (Used for the 0x1000 check)
        private const int xr_codeswitch = 3;    // Index for the Mode/Switch flags (Used by UpdateButtonState)
        private const int xr_markfreq = 1;      // Index for Mark Frequency (Example)
        private const int xr_spacefreq = 2;     // Index for Space Frequency (Example)
                                                // ... define other indices (xr_signallevel, xr_data, etc.) as needed ...

        // --- Control and Status Constants ---
        private const int NMMR_VIEW_FLAG = 0x1000; // The flag checked in XMMR_OnNotifyNMMR

        private readonly System.Windows.Forms.Timer _specTimer = new() { Interval = 50 }; // 20 fps
        private short[] _fft = new short[2048];
        private int _phase = 0;
        private XmmrRawEventHook? _hook;
        private readonly object _fftLock = new();

        public Form1()
        {
            Log.Mark("Form1 before init");
            InitializeComponent();
            Log.Mark("Form1 after init");
            this.Shown += Form1_Shown;
            //this.Shown += (_, __) => TestRamp();
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            Log.Mark("Form1_Shown entered");
            Log.LogThreadDetails("Form1_Shown");


            //axXMMR1.CreateControl();
            //Log.Mark("Form1_Shown after CreateControl");

            if (_hook != null)
            {
                Log.Mark("Form1_Shown _hook not null");
                return;
            }

            _hook = new XmmrRawEventHook(axXMMR1);
            _hook.OnNotifyFftRaw += OnNotifyFftRaw;
            _hook.OnNotifyGetXY += OnNotifyGetXY;
            _hook.AcceptFft = false;      // ignore early noise
            _hook.Advise();

            StartMmttyEngine();

            DumpSpecDrawSignature();

        }

        void StartMmttyEngine()
        {
            Log.LogThreadDetails("StartMmttyEngine()", axXMMR1);
            axXMMR1.InvokeCommand = "C:\\Ham\\MMTTY\\MMTTY.exe -m";

            // Connect to the MMTTY engine
            axXMMR1.bActive = true;

            // Request notification events for the FFT/Spectra data
            axXMMR1.bNotifyFFT = true;

            //Log.Mark("MMTTY engine invoke , axXMMR1 sent");
            Log.LogThreadDetails("MMTTY engine invoke instruction sent", axXMMR1);

            //ShowStatus();
        }

        // [id(0x00000007)]  XY.Draw(long* pXY);
        private void OnNotifyGetXY(int[] XY)
        {
            //Log.LogThreadDetails("[[OnNotifyGetXY no action]]");

            IntPtr pXY = AllocXYBuffer(XY);
            try
            {
                XMMSpecComDrawDispatch.InvokeSendIntArray(axxmmxy1.GetOcx(), pXY, 0x00000007 /* XY.Draw */);
            }
            finally
            {
                Marshal.FreeHGlobal(pXY);
            }

            return;

        }

        private static IntPtr AllocXYBuffer(int[] xy1024)
        {
            if (xy1024 is null) throw new ArgumentNullException(nameof(xy1024));
            if (xy1024.Length != 1024) throw new ArgumentException("Must be 1024 elements.");

            IntPtr p = Marshal.AllocHGlobal(1024 * sizeof(int));
            Marshal.Copy(xy1024, 0, p, 1024);
            return p;
        }


        // Form fields
        //private readonly object _specLock = new();
        private volatile int _specDrawPending; // coalesce flood of FFT events

        // Call this from your wrapper when FFT arrives
        private void OnNotifyFftRaw(short[] fftRaw, short size, short sampfreq)
        {
            if (fftRaw == null || fftRaw.Length == 0) return;
            if (size <= 0) return;

            // Clamp to available data (defensive)
            int n = Math.Min(size, (short)fftRaw.Length);

            // Pin the managed array so we can pass a stable pointer into COM
            var handle = GCHandle.Alloc(fftRaw, GCHandleType.Pinned);
            try
            {
                IntPtr pFirst = handle.AddrOfPinnedObject(); // points to fftRaw[0] as short*
                //send data to Spectrum control
                XMMSpecComDrawDispatch.InvokeSpecDraw(
                    axXMMSpec1.GetOcx(),   // you can also try axXMMSpec1 itself if needed
                    pFirst,
                    (short)n,
                    sampfreq,
                    XMMSpecComDrawDispatch.DISPID_SpecDraw
                );

                //do same for waterfall
                XMMSpecComDrawDispatch.InvokeSpecDraw(
                    axxmmWater1.GetOcx(),   // you can also try axxmmWater1 itself if needed
                    pFirst,
                    (short)n,
                    sampfreq,
                    XMMSpecComDrawDispatch.DISPID_WaterfallDraw
                );

            }
            finally
            {
                handle.Free();
            }
        }


        private void panelSpec_Resize(object? sender, EventArgs e)
        {
            //XMMNativeDLL.XMM_SpecResize(panelSpec.Width, panelSpec.Height);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // Connect to the MMTTY engine
            //axXMMR1.bActive = true;

            // Request notification events for the FFT/Spectra data
            //axXMMR1.bNotifyFFT = true;

            //ShowStatus();
        }


        // C# method equivalent to your VB6 ShowStatus() Sub
        private void ShowStatus()
        {
            // VB6: If XMMR.Item(1).bActive Then ...
            if (axXMMR1.bActive)
            {
                // axXMMR1.verMMTTY will provide the version string
                LStatus.Text = $"Connected to MMTTY {axXMMR1.verMMTTY}";
            }
            else if (axXMMR1.bInvoking)
            {
                LStatus.Text = "Invoking...";
            }
            else
            {
                LStatus.Text = "Disconnected";
            }
        }

        // Equivalent to VB6: Private Sub XMMR_OnConnected(Index As Integer)
        private void axXMMR1_OnConnected_1(object sender, EventArgs e)
        {
            // VB6: XYscope.DemSampleFreq = XMMR.Item(1).smpDemFreq
            // Set the sample frequency property on your visual display control
            // axXYScope1.DemSampleFreq = axXMMR1.smpDemFreq; 


            Log.Mark("MMTTY engine started and in event axXMMR1_OnConnected_1");
            ShowStatus();

            Log.Mark("[UI] Engine connected — enabling FFT");
            _hook.AcceptFft = true;


            // Call UpdateMenus if you implement it (to enable/disable profile menu items)
            // UpdateMenus(); 
        }

        // Equivalent to VB6: Private Sub XMMR_OnDisconnected(Index As Integer, ByVal status As Integer
        private void axXMMR1_OnDisconnected(object sender, _DXMMREvents_OnDisconnectedEvent e)
        {
            switch (e.status)
            {
                case 0:
                    LStatus.Text = "MMTTY closing failed.";
                    break;
                case 2:
                    LStatus.Text = "MMTTY invoking failed.";
                    // If invoking failed, prompt the user for the path as in VB6
                    string prompt = "Probably, MMTTY was not invoked.\r\n\r\n" +
                                    "Enter full path name.\r\n" +
                                    "Or copy mmtty.exe and userpara.ini to the current folder.";

                    // This calls the method to prompt for a new invoke command
                    //InputInvokeCommand(prompt);
                    break;
                default:
                    LStatus.Text = "Disconnected (Status: " + e.status + ")";
                    break;
            }
        }





        // Assuming the event arguments (e) are structured based on your AxInterop definition:
        private void axXMMR1_OnNotifyNMMR(object sender, _DXMMREvents_OnNotifyNMMREvent e)
        {
            return;
            unsafe
            {
                fixed (int* dst = m_nmmr)
                fixed (int* src = &e.pNMMR)
                {
                    Buffer.MemoryCopy(
                        source: src,
                        destination: dst,
                        destinationSizeInBytes: 64 * sizeof(int),
                        sourceBytesToCopy: 64 * sizeof(int));
                }
            }


            // 2. Trigger updates for the visual controls, passing the raw pointer
            // This allows the visual controls to access the data directly without another copy.
            // Assuming axLevel1 and axSpectram1 are your controls.

            // VB6: Call Level.DrawByNMMR(pNMMR)
            axXMMLevel1.DrawByNMMR(ref e.pNMMR);

            // VB6: Call Spectram.UpdateByNMMR(pNMMR)
            axXMMSpec1.UpdateByNMMR(ref e.pNMMR);

            // 3. Update the state of all operating mode buttons (AFC, NET, etc.)
            // VB6: Call UpdateButtonState
            //UpdateButtonState();

            // 4. Ensure a specific view flag is enabled in the MMTTY engine
            // This is often done to guarantee the engine continues to output the detailed status needed.
            // VB6: If (m_nmmr(xr_codeview) And &H1000) = 0 Then
            // C#: if ((m_nmmr[xr_codeview] & 0x1000) == 0)

            const int NMMR_VIEW_FLAG = 0x1000; // The flag constant

            if ((m_nmmr[xr_codeview] & NMMR_VIEW_FLAG) == 0)
            {
                // If the flag is not set (result is 0), set it by ORing ( | ) the flag
                // and send the new view status back to the engine.

                // VB6: Call XMMR.Item(1).SetMmttyView(m_nmmr(xr_codeview) Or &H1000)
                axXMMR1.SetMmttyView(m_nmmr[xr_codeview] | NMMR_VIEW_FLAG);
            }
        }


        private void axXMMSpec1_OnLMouseDown(object sender, _DXMMSpecEvents_OnLMouseDownEvent e)
        {

        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hook?.Dispose();
            base.OnFormClosed(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Log.LogThreadDetails("button click");
            StartMmttyEngine();
        }

        ///*
        private void axXMMR1_OnNotifyFFT(object sender, _DXMMREvents_OnNotifyFFTEvent e)
        {
            Log.Mark("axXMMR1_OnNotifyFFT form control event");
        }

        private void axXMMSpec1_OnLMouseDown_1(object sender, _DXMMSpecEvents_OnLMouseDownEvent e)
        {
            Log.Mark("axXMMSpec1_OnLMouseDown_1 form control event");
        }
        //*/


        private void DumpSpecDrawSignature()
        {
            var t = axXMMSpec1.GetType();
            foreach (var m in t.GetMethods(System.Reflection.BindingFlags.Instance |
                                           System.Reflection.BindingFlags.Public))
            {
                if (!m.Name.Contains("Draw", StringComparison.OrdinalIgnoreCase)) continue;

                var ps = string.Join(", ", m.GetParameters()
                    .Select(p => $"{p.ParameterType.FullName} {p.Name}"));

                Log.Mark($"[Spec] {m.ReturnType.FullName} {m.Name}({ps})");
            }
        }

        private void btnTestDraw_Click(object sender, EventArgs e)
        {
            //NON-WORKING Interop method of Drawing to Spectrum Control
            //Hence the need for Dispatch methods

            const int n = 2048;
            const short fs = 11025;

            // Build a deterministic test pattern (easy to recognize)
            short[] data = new short[n];
            for (int i = 0; i < n; i++)
                data[i] = (short)(i % 256); // or a sine, ramp, impulses, etc.


            //ref short r0 = ref MemoryMarshal.GetArrayDataReference(data);
            ref short r0 = ref data[0];
            axXMMSpec1.Draw(ref r0, (short)128, (short)11025);
        }

        int t = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            //Form1.ActiveForm.Text = "Tick " + t++;
        }

        private void axxmmxy1_OnLButtonClick(object sender, _DXMMXYEvents_OnLButtonClickEvent e)
        {

        }

        private void axXMMR1_OnNotifyXY(object sender, _DXMMREvents_OnNotifyXYEvent e)
        {
            // TODO This doesnt work for same reasons 
            //axxmmxy1.Draw(ref e.pXY);
        }
    }
}

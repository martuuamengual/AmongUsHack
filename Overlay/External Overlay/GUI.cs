using System;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using SharpDX;
using SharpDX.DirectWrite;
using System.Threading;
using System.Runtime.InteropServices;
using DirectX_Renderer.Interfaces;

namespace DirectX_Renderer.GUI
{
    /// <summary>
    /// this class can be used to overlay DirectX graphs
    /// <para>Use <seealso cref="drawCallBack">drawCallBack</seealso> to draw DirectX graphs</para>
    /// </summary>
    public partial class GUI : BaseGUI, IExeCloseHandler
    {

        #region variables

        /// <summary>
        /// use this callback to draw what you want in the scren device using DirectX drivers
        /// </summary>
        public Action<WindowRenderTarget> drawCallBack = null;

        #endregion

        #region directx needed variables

        //Styles
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush;
        private Factory factory;

        //text fonts to test DirectX direct draw text
        private TextFormat font;
        private FontFactory fontFactory;
        private const string fontFamily = "Arial";
        private const float fontSize = 25.0f;

        private Thread threadDX = null;

        #endregion

        #region dll imports

        [DllImport("dwmapi.dll")]
        private static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

        #endregion


        public GUI()
        {
            InitializeComponent();
        }

        // we need this function to dont have an StackOverflowException
        /// <summary>
        /// This event is called when game process is closed
        /// </summary>
        public void OnExeClose() { }

        public new void OnLoad(object sender, EventArgs e)
        {
            // You can write your own dimensions of the auto-mode if doesn't work properly.

            this.Location = new System.Drawing.Point(-10, 0);

            System.Drawing.Rectangle screen = Utilities.GetScreen(this);
            this.Width = screen.Width + 20;
            this.Height = screen.Height + 20;

            this.DoubleBuffered = true; // reduce the flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |// reduce the flicker too
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            this.Visible = true;

            factory = new Factory();
            fontFactory = new FontFactory();
            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(this.Width, this.Height),
                PresentOptions = PresentOptions.None
            };

            //Init DirectX
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            // if you want use DirectX direct renderer, you can use this brush and fonts.
            // of course you can change this as you want.
            solidColorBrush = new SolidColorBrush(device, Color.Red);
            font = new TextFormat(fontFactory, fontFamily, fontSize);


            threadDX = new Thread(new ParameterizedThreadStart(_loop_DXThread));

            threadDX.Priority = ThreadPriority.Highest;
            threadDX.IsBackground = true;
            threadDX.Start();
        }

        private void _loop_DXThread(object sender)
        {
            while (true)
            {
                device.BeginDraw();
                device.Clear(Color.Transparent);
                device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Aliased;

                //device.DrawBitmap(_bitmap, 1, BitmapInterpolationMode.Linear, new SharpDX.Mathematics.Interop.RawRectangleF(600, 400, 0, 0));
                //place your rendering things here

                // Draw callback form dx
                drawCallBack?.Invoke(device);

                device.EndDraw();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            int[] marg = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

    }
}

// Copyright (c) 2016-2017 DirectX_Renderer - DoxCode - https://github.com/DoxCode
//
// DxRender - xDasEinhorn
//
// The overlay window use SHARPDX as wrapper of the DirectX API.
// Used version 4.01, of:
//
// SharpDX, SharpDX.Desktop, SharpDX.Direct2D1, SharpDX.Direct3D11, SharpDX.DXGI, SharpDX.Mathematics
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN

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
using System.Diagnostics;

namespace DirectX_Renderer
{
        public partial class Overlay_SharpDX : Form
        {
            public Action<WindowRenderTarget> drawCallBack = null;


            private WindowRenderTarget device;
            private HwndRenderTargetProperties renderProperties;
            private SolidColorBrush solidColorBrush;
            private Factory factory;

            //text fonts to test DirectX direct draw text
            private TextFormat font;
            private FontFactory fontFactory;
            private const string fontFamily = "Arial";
            private const float fontSize = 25.0f;
            private Bitmap _bitmap;

            private Thread threadDX = null;
            //DllImports
            [DllImport("user32.dll")]
            public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll")]
            static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

            [DllImport("dwmapi.dll")]
            public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

            [DllImport("user32.dll")]
            private static extern IntPtr SetActiveWindow(IntPtr handle);

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            private static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


            //Styles
            public const UInt32 SWP_NOSIZE = 0x0001;
            public const UInt32 SWP_NOMOVE = 0x0002;
            public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
            private const int WS_EX_NOACTIVATE = 0x08000000;


            delegate void OverlayIsFocused();
            private System.Threading.Timer timer = null;
            private Process currentprocess = null;

            public Overlay_SharpDX(Process process)
            {
                SetWindowLong(Handle, -8, process.Handle);
                OnResize(null);

                InitializeComponent();

                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMilliseconds(1);

                currentprocess = process;

                process.EnableRaisingEvents = true;
                process.Exited += this.HandleClosed;

                timer = new System.Threading.Timer((e) =>
                {
                    try
                    {
                        OverlayIsFocused mydel = new OverlayIsFocused(CheckOverlayStatus);
                        object v = this.Invoke(mydel);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        timer.Dispose();
                    }
                    catch (InvalidOperationException ex) { 
                    }
                }, null, startTimeSpan, periodTimeSpan);
            }

            private void HandleClosed(object sender, EventArgs e)
            {
                this.Invoke((MethodInvoker) delegate {
                    this.Close();
                });
            }

            private void CheckOverlayStatus()
            {
                if (this == null)
                {
                    return;
                }

                var isFocused = this.ApplicationIsActivated(this.currentprocess);

                if (isFocused)
                {
                    SetWindowPos(Handle, new IntPtr(-1), 0, 0, 0, 0, TOPMOST_FLAGS);
                }
                else
                {
                    SetWindowPos(Handle, new IntPtr(-2), 0, 0, 0, 0, TOPMOST_FLAGS);
                }
            }

            /// <summary>Returns true if the current application has focus, false otherwise</summary>
            private bool ApplicationIsActivated(Process process)
            {
                var activatedHandle = GetForegroundWindow(); //get the current window that user is focused
                if (activatedHandle == IntPtr.Zero)
                {
                    return false;       // No window is currently activated
                }

                var procId = process.Id;
                int activeProcId;
                GetWindowThreadProcessId(activatedHandle, out activeProcId);

                return activeProcId == procId;
            }

            // Remember change the values of the form in the designer.
            public void Overlay_SharpDX_Load(object sender, EventArgs e)
            {
                // You can write your own dimensions of the auto-mode if doesn't work properly.
                System.Drawing.Rectangle screen = Utilities.GetScreen(this);
                this.Width = screen.Width;
                this.Height = screen.Height;

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
                _bitmap = Utilities.LoadFromFile(device, "sprite_example.png");


                threadDX = new Thread(new ParameterizedThreadStart(_loop_DXThread));

                threadDX.Priority = ThreadPriority.Highest;
                threadDX.IsBackground = true;
                threadDX.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                int[] marg = new int[] { 0, 0, Width, Height };
                DwmExtendFrameIntoClientArea(this.Handle, ref marg);
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
                    drawCallBack.Invoke(device);

                    device.EndDraw();
                }
            }

            /// <summary>
            /// Used to not show up the form in alt-tab window. 
            /// Tested on Windows 7 - 64bit and Windows 10 64bit
            /// </summary>
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams pm = base.CreateParams;
                    pm.ExStyle |= WS_EX_NOACTIVATE; // prevent the form from being activated
                    return pm;
                }
            }

        }
    }

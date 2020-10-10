using Microsoft.CSharp.RuntimeBinder;
using SharpDX.Direct2D1;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{

    public static class BaseGUI_Constants {
        private static Process process;
        private static bool ExeWasClosed;

        public static void SetProcess(Process process)
        {
            if (process != null)
            {
                BaseGUI_Constants.process = process;
            }
        }

        public static Process GetProcess()
        {
            return process;
        }

        public static void CleanProcess()
        {
            process = null;
        }

        public static void SetExeWasClosedValue() {
            ExeWasClosed = true;
        }

        public static bool GetExeWasClosedValue() {
            return ExeWasClosed;
        }

        public static void CleanExeWasClosedValue()
        {
            ExeWasClosed = false;
        }

    }

    public partial class BaseGUI : Form
    {

        #region variables

        delegate void OverlayIsFocused(IntPtr handle, Process process);
        private System.Threading.Timer timer = null;

        #endregion

        #region directx needed variables

        //Styles
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        #region dll imports

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        #endregion

        public BaseGUI()
        {
            InitializeComponent();
        }

        public void OnLoad(object sender, EventArgs e) {
            OnStartConstructor();
            OnFinishConstructor();
        }

        private void OnStartConstructor() {
            if (BaseGUI_Constants.GetProcess() != null) { 
                SetWindowLong(Handle, -8, BaseGUI_Constants.GetProcess().Handle);
                OnResize(null);
            }
        }

        private void OnFinishConstructor() {

            if (BaseGUI_Constants.GetProcess() != null)
            {
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMilliseconds(1);

                BaseGUI_Constants.GetProcess().EnableRaisingEvents = true;
                BaseGUI_Constants.GetProcess().Exited += this.HandleClosed;

                timer = new System.Threading.Timer((e) =>
                {
                    try
                    {
                        OverlayIsFocused mydel = new OverlayIsFocused(Utilities.CheckOverlayStatus);
                        object v = this.Invoke((MethodInvoker)delegate { mydel(Handle, BaseGUI_Constants.GetProcess()); });
                    }
                    catch (ObjectDisposedException ex)
                    {
                        timer.Dispose();
                    }
                    catch (InvalidOperationException ex)
                    {
                    }
                }, null, startTimeSpan, periodTimeSpan);
            }
        }

        private void HandleClosed(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate {
                this.OnExeClose();
                this.Close();
            });
        }

        private void OnExeClose()
        {
            BaseGUI_Constants.SetExeWasClosedValue();
            // Using dynamic for more performance and more simplicity code
            dynamic derived = this;
            try
            {
                derived.OnExeClose();
            }
            catch (RuntimeBinderException e) { }
            return;
        }

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

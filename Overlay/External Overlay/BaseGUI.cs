using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{
    /// <summary>
    /// Use this class to get process game atached or if the game was closed.
    /// <para>This class is used to, for example, hide the UI when the game is minimized</para>
    /// </summary>
    public static class BaseGUI_Constants {
        private static Process process;
        private static bool ExeWasClosed;

        /// <summary>
        /// Sets the game process
        /// </summary>
        public static void SetProcess(Process process)
        {
            if (process != null)
            {
                BaseGUI_Constants.process = process;
            }
        }

        /// <summary>
        /// Gets the current game process atached
        /// </summary>
        public static Process GetProcess()
        {
            return process;
        }

        /// <summary>
        /// Puts process variable equals to <seealso cref="Nullable">null</seealso>
        /// </summary>
        public static void CleanProcess()
        {
            process = null;
        }

        /// <summary>
        /// Sets the exe flag closed to <seealso cref="Boolean">true</seealso>
        /// </summary>
        public static void SetExeWasClosedValue() {
            ExeWasClosed = true;
        }

        /// <summary>
        /// Gets the exe flag <seealso cref="Boolean">true</seealso> or <seealso cref="Boolean">false</seealso>
        /// </summary>
        public static bool GetExeWasClosedValue() {
            return ExeWasClosed;
        }

        /// <summary>
        /// Sets the exe flag closed to <seealso cref="Boolean">false</seealso>
        /// </summary>
        public static void CleanExeWasClosedValue()
        {
            ExeWasClosed = false;
        }

    }

    /// <summary>
    /// This class is the base of the overlay DirectX graphs but without the transparent background.
    /// </summary>
    public partial class BaseGUI : Form
    {

        #region variables

        private delegate void OverlayIsFocused(IntPtr handle, Process process);
        private System.Threading.Timer timer = null;

        #endregion

        #region directx needed variables

        //Styles
        private const int WS_EX_NOACTIVATE = 0x08000000;

        #endregion

        public BaseGUI()
        {
            InitializeComponent();
        }

        public void OnLoad(object sender, EventArgs e)
        {
            OnStartConstructor();
            OnFinishConstructor();
        }

        private void OnStartConstructor() {
            if (BaseGUI_Constants.GetProcess() != null) {
                Console.WriteLine("Starting GUI!");
                OnResize(null);
            }
        }

        private void OnFinishConstructor() {

            if (BaseGUI_Constants.GetProcess() != null)
            {

                BaseGUI_Constants.GetProcess().EnableRaisingEvents = true;
                BaseGUI_Constants.GetProcess().Exited += this.HandleClosed;

                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMilliseconds(1);

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

                Console.WriteLine("Finish GUI!");
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

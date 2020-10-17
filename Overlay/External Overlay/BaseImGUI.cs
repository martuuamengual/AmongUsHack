
using DirectX_Renderer.Handler;
using DirectX_Renderer.Interfaces;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{
    public partial class BaseImGUI : GUI, IDragHandler, IExeCloseHandler
    {

        private bool isDraging;

        public Point mouseDownPoint = Point.Empty;
        public Point formLocation = Point.Empty;

        private Panel MainPanel = null;

        public BaseImGUI()
        {
            InitializeComponent();
        }

        public new virtual void OnLoad(object snder, EventArgs e) {
            if (BaseGUI_Constants.GetProcess() != null) {
                if (this.MainPanel == null)
                {
                    throw new Exception("MainPanel is null, please use SetPanel()");
                }
                else { 
                    KeyHandler key = new KeyHandler(Keys.LButton);
                    key.OnMouseMove += OnMouseMoveE;
                    key.OnKeyDown += OnMouseDownE;
                    key.OnKeyUp += OnMouseUpE;
                }
            }
        }

        /// <summary>
        /// Gets the current UI base panel
        /// </summary>
        public Panel GetPanel() {
            return this.MainPanel;
        }

        /// <summary>
        /// Sets the current UI base panel
        /// </summary>
        public void SetPanel(Panel panel)
        {
            this.MainPanel = panel;
        }

        /// <summary>
        ///  Sets the base panel position to the new given position
        /// </summary>
        public void SetPanelPosition(Point position) {
            this.Invoke((MethodInvoker)delegate {
                this.GetPanel().Location = position;
            });
        }

        // we need this function to dont have an StackOverflowException
        /// <summary>
        ///  This event fires when mouse is left-click hold and mouse moves throw screen
        ///  <para>to include this event in youre class use <seealso cref="IDragHandler">IDragHandler</seealso> interface</para>
        /// </summary>
        public void OnDrag(object sender, Keys e)
        {
            bool isVisible = false;
            this.Invoke((MethodInvoker)delegate {
                isVisible = this.GetPanel().Visible;
            });
            if (!isVisible) {
                return;
            }

            Console.WriteLine(isVisible);

            Panel mainPanel = null;

            this.Invoke((MethodInvoker)delegate {
                mainPanel = this.GetPanel();
            });

            Point cursorPosition = Cursor.Position;

            Rectangle screen = Utilities.GetScreenInvoke(this);

            Point positionForm = new Point(cursorPosition.X - formLocation.X, cursorPosition.Y - formLocation.Y);

            bool isInsideX = (positionForm.X < cursorPosition.X && cursorPosition.X < positionForm.X + mainPanel.Width);
            bool isInsideY = (positionForm.Y < cursorPosition.Y && cursorPosition.Y < positionForm.Y + mainPanel.Height);
            bool isInsideForm = isInsideX && isInsideY;


            // Move Form
            if (isInsideForm)
            {
                this.SetPanelPosition(positionForm);
            }

        }

        // we need this function to dont have an StackOverflowException
        /// <summary>
        /// This event is called when game process is closed
        /// </summary>
        public new void OnExeClose() { base.OnExeClose(); }

        // Using dynamic for more performance and more simplicity code
        private void OnDragEvent(object sender, Keys key)
        {
            // Using dynamic for more performance and more simplicity code
            dynamic derived = this;
            try
            {
                derived.OnDrag(sender, key);
            }
            catch (RuntimeBinderException e) { }
        }

        private void OnMouseDownE(object sender, Keys e)
        {
            if (mouseDownPoint != Point.Empty) {
                if (!isDraging) {
                    this.Invoke((MethodInvoker)delegate {
                        Panel mainPanel = this.GetPanel();
                        formLocation = new Point(mouseDownPoint.X - mainPanel.Location.X, mouseDownPoint.Y - mainPanel.Location.Y);
                        isDraging = true;
                    });
                }

                OnDragEvent(sender, e);
            }
        }

        private void OnMouseUpE(object sender, Keys e)
        {
            this.Invoke((MethodInvoker)delegate {
                this.mouseDownPoint = Point.Empty;
                this.isDraging = false;
            });
        }

        private void OnMouseMoveE(object sender, Point e)
        {
            this.Invoke((MethodInvoker)delegate {
                mouseDownPoint = new Point(e.X, e.Y);
            });
        }
    }
}

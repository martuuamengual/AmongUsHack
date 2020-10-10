
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{
    public partial class BaseImGUI : BaseGUI
    {

        #region variables

        public Point mouseDownPoint = Point.Empty;

        #endregion

        #region directx needed variables

        #endregion


        public BaseImGUI()
        {
            InitializeComponent();

            MouseMove += new MouseEventHandler(OnMouseMove);
            MouseDown += new MouseEventHandler(OnMouseDown);
            MouseUp += new MouseEventHandler(OnMouseUp);
        }

        public void OnKeyDown(object sender, KeyEventArgs ev)
        {
            // Using dynamic for more performance and more simplicity code
            dynamic derived = this;
            try
            {
                derived.OnKeyDown();
            }
            catch (RuntimeBinderException e) { }
        }

        // Using dynamic for more performance and more simplicity code
        private void OnDrag(object sender, MouseEventArgs mouseEvent)
        {

            Location = new Point(this.Left + mouseEvent.X - mouseDownPoint.X, this.Top + mouseEvent.Y);

            // Using dynamic for more performance and more simplicity code
            dynamic derived = this;
            try
            {
                derived.OnDrag(sender, mouseEvent);
            }
            catch (RuntimeBinderException e) { }
        }

        public void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
            {
                mouseDownPoint = new Point(e.X, e.Y);
            }
        }

        public void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button.Equals(MouseButtons.Left))
                mouseDownPoint = Point.Empty;
        }

        public void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownPoint != Point.Empty)
                OnDrag(sender, e);
        }

    }
}

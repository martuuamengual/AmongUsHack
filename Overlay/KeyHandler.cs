using DirectX_Renderer.Threads;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DirectX_Renderer.Handler
{
    public class KeyHandler
    {
        #region events

        public EventHandler<Keys> OnKeyPressed;
        public EventHandler<Keys> OnKeyDown;
        public EventHandler<Keys> OnKeyUp;
        public EventHandler<Point> OnMouseMove;

        #endregion

        #region variables

        private System.Threading.Timer timerKeyPressed;
        private System.Threading.Timer timerKeyDown;
        private System.Threading.Timer timerKeyUp;
        private System.Threading.Timer timerMouseMove;

        private Keys vKey;
        private Point lastPositionMouse = new Point(0, 0);

        private bool _keyDown;

        #endregion

        #region dll imports

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        #endregion


        public KeyHandler(Keys key) {

            vKey = key;

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMilliseconds(1);

            // is needed to use timer to performance the usage of cpu
            timerKeyPressed = new System.Threading.Timer((e) =>
            {
                try
                {
                    KeyPressedTask();
                }
                catch (ObjectDisposedException ex)
                {
                    timerKeyPressed.Dispose();
                }
                catch (InvalidOperationException ex)
                {
                }
            }, null, startTimeSpan, periodTimeSpan);

            // is needed to use timer to performance the usage of cpu
            timerKeyDown = new System.Threading.Timer((e) =>
            {
                try
                {
                    KeyDownTask();
                }
                catch (ObjectDisposedException ex)
                {
                    timerKeyDown.Dispose();
                }
                catch (InvalidOperationException ex)
                {
                }
            }, null, startTimeSpan, periodTimeSpan);

            // is needed to use timer to performance the usage of cpu
            timerKeyUp = new System.Threading.Timer((e) =>
            {
                try
                {
                    KeyUpTask();
                }
                catch (ObjectDisposedException ex)
                {
                    timerKeyUp.Dispose();
                }
                catch (InvalidOperationException ex)
                {
                }
            }, null, startTimeSpan, periodTimeSpan);

            // is needed to use timer to performance the usage of cpu
            timerMouseMove = new System.Threading.Timer((e) =>
            {
                try
                {
                    MouseMoveTask();
                }
                catch (ObjectDisposedException ex)
                {
                    timerMouseMove.Dispose();
                }
                catch (InvalidOperationException ex)
                {
                }
            }, null, startTimeSpan, periodTimeSpan);

        }

        public void DisposeAll() {
            timerKeyPressed.Dispose();
            timerKeyDown.Dispose();
            timerKeyUp.Dispose();
            timerMouseMove.Dispose();
        }

        private bool isKeyDown()
        {
            int keystroke = (int)vKey;
            if (GetAsyncKeyState(keystroke) < 0)
            {
                return true;
            }
            return false;
        }

        private bool isKeyUp()
        {
            int keystroke = (int)vKey;
            if (GetAsyncKeyState(keystroke) == 0)
            {
                return true;
            }
            return false;
        }

        private bool isMouseMoving() {
            Point currentPositionMouse = Cursor.Position;
            if ((currentPositionMouse.X != lastPositionMouse.X || 
                currentPositionMouse.Y != lastPositionMouse.Y) && 
                (currentPositionMouse.X > 0 && currentPositionMouse.Y > 0)) {
                lastPositionMouse = currentPositionMouse;
                return true;
            }
            return false;
        }

        private void KeyPressedTask()
        {
            if (OnKeyPressed != null)
            {
                if (isKeyDown())
                {
                    _keyDown = true;
                }
                if (isKeyUp() && _keyDown)
                {
                    _keyDown = false;
                    OnKeyPressed.Invoke(this, vKey);
                }
            }
        }

        private void KeyDownTask()
        {
            if (OnKeyDown != null) { 
                if (isKeyDown())
                {
                    OnKeyDown.Invoke(this, vKey);
                }
            }
        }

        private void KeyUpTask()
        {
            if (OnKeyUp != null) { 
                if (isKeyUp())
                {
                    OnKeyUp.Invoke(this, vKey);
                }
            }
        }

        private void MouseMoveTask() {
            if (OnMouseMove != null) {
                if (isMouseMoving()) {
                    OnMouseMove.Invoke(this, Cursor.Position);
                }
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectX_Renderer
{
    public class KeyHandler
    {
        #region events

        public EventHandler<Keys> OnKeyPressed;

        #endregion

        #region variables

        private Keys vKey;

        private Dictionary<string, CancellationTokenSource> Tokens = new Dictionary<string, CancellationTokenSource>();
        private bool _keyDown;

        #endregion

        #region dll imports

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        #endregion


        public KeyHandler(Keys key) {
            vKey = key;
            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Factory.StartNew(KeyPressedTask, cts.Token);
            Tokens.Add("KeyPressed", cts);
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

        public void KeyPressedTask() {
            while (Tokens.ContainsKey("KeyPressed") && !Tokens["KeyPressed"].IsCancellationRequested) {
                if (isKeyDown()) {
                    _keyDown = true;
                }
                if (isKeyUp() && _keyDown) {
                    _keyDown = false;
                    OnKeyPressed.Invoke(this, vKey);
                }
            }
        }

    }
}

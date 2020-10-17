using DirectX_Renderer.GUI;
using DirectX_Renderer.Handler;
using DirectX_Renderer.Interfaces;
using System;
using System.Windows.Forms;

namespace YourCheat
{
    public partial class MenuDx3 : ImGUI, IKeyHandlerPressed
    {

        private bool isCurrentActive = true;

        public MenuDx3()
        {
            InitializeComponent();
        }

        public override void OnLoad(object sender, EventArgs e) {
            // we need to check if the process is active or not because if we dont checkit VS crash.
            if (BaseGUI_Constants.GetProcess() != null) {

                this.SetPanel(ConstructPanel());

                KeyHandler keyHandler = new KeyHandler(Keys.Insert);
                keyHandler.OnKeyPressed += OnKeyPressed;
            }
            base.OnLoad(sender, e);
        }

        private Panel ConstructPanel() {
            return panel1;
        }

        public void OnKeyPressed(object sender, Keys e)
        {
            if (isCurrentActive)
            {
                this.Invoke((MethodInvoker)delegate {
                    this.GetPanel().Hide();
                });
                isCurrentActive = false;
            }
            else {
                this.Invoke((MethodInvoker)delegate {
                    this.GetPanel().Show();
                });
                isCurrentActive = true;
            }
        }
    }

}

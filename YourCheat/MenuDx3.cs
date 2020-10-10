using DirectX_Renderer;
using DirectX_Renderer.GUI;
using DirectX_Renderer.Interfaces;
using System;
using System.Threading;
using System.Windows.Forms;

namespace YourCheat
{
    public partial class MenuDx3 : ImGUI, IKeyHandlerPressed
    {

        private bool isCurrentActive = false;

        public MenuDx3()
        {
            InitializeComponent();

            KeyHandler keyHandler = new KeyHandler(Keys.Insert);
            keyHandler.OnKeyPressed += OnKeyPressed;
        }

        public void OnKeyPressed(object sender, Keys e)
        {
            Console.WriteLine("Key Pressed!!!");
        }
    }

    public static class MenuDx3_Controller
    {

        public static void StartGUI()
        {
            Thread thread = new Thread(() => {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MenuDx3());
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }


    }
}

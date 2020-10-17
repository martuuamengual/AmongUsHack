using System;
using System.Threading;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{
    public static class GUIStarter
    {

        public static T Init<T>() where T : Form, new()
        {
            /*Type type = typeof(T);
            T form = (T)Activator.CreateInstance(type);*/

            T instance = new T();

            Thread thread = new Thread(() => {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(instance);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return instance;
        }

    }
}

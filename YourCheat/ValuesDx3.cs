
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FontFactory = SharpDX.DirectWrite.Factory;
using SharpDX;
using System;
using DirectX_Renderer.Interfaces;
using DirectX_Renderer.GUI;

namespace YourCheat
{
    public partial class ValuesDx3 : GUI, IExeCloseHandler
    {

        public static string impostorName = "none";

        public ValuesDx3() {
            this.drawCallBack += (WindowRenderTarget device) => {
                FontFactory fontFactory = new FontFactory();
                SolidColorBrush solidColorBrush = new SolidColorBrush(device, Color.Red);
                TextFormat font = new TextFormat(fontFactory, "Arial", 18.0f);
                device.DrawText("Impostor: " + impostorName, font, new SharpDX.Mathematics.Interop.RawRectangleF(15, 0, 136, 0), solidColorBrush);
                device.Transform = new SharpDX.Mathematics.Interop.RawMatrix3x2(1, 0, 0, 1, 0, 500);
            };
        }

        public new void OnExeClose() {
            Console.WriteLine("Among Us.exe was closed!");
        }

    }
}

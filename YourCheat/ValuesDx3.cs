using System.Diagnostics;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FontFactory = SharpDX.DirectWrite.Factory;
using SharpDX;
using System;

namespace DirectX_Renderer
{
    public partial class ValuesDx3 : Overlay_SharpDX
    {

        public static string impostorName = "none";

        public ValuesDx3(Process process) : base(process) {
            this.drawCallBack += (device) => {
                FontFactory fontFactory = new FontFactory();
                SolidColorBrush solidColorBrush = new SolidColorBrush(device, Color.Red);
                TextFormat font = new TextFormat(fontFactory, "Arial", 18.0f);
                device.DrawText("Impostor: " + impostorName, font, new SharpDX.Mathematics.Interop.RawRectangleF(5, 500, 500, 500), solidColorBrush);
            };
        }

        public override void OnExeClose() {
            base.OnExeClose();
            Console.WriteLine("Among Us.exe was closed!");
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FontFactory = SharpDX.DirectWrite.Factory;
using SharpDX;

namespace DirectX_Renderer
{
    public partial class ValuesDx3 : Overlay_SharpDX
    {

        public ValuesDx3(Process process) : base(process) {
            this.drawCallBack += (device) => {
                FontFactory fontFactory = new FontFactory();
                SolidColorBrush solidColorBrush = new SolidColorBrush(device, Color.Red);
                TextFormat font = new TextFormat(fontFactory, "Arial", 25.0f);
                device.DrawText(this.labelDX, font, new SharpDX.Mathematics.Interop.RawRectangleF(100, 100, 500, 30), solidColorBrush);
            };
        }

        public void Test(WindowRenderTarget device) {
        }

    }
}

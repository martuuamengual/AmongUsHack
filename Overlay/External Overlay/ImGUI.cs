using DirectX_Renderer.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DirectX_Renderer.GUI
{
    public partial class ImGUI : BaseImGUI, IDragHandler, IExeCloseHandler
    {

        public ImGUI()
        {
            InitializeComponent();
        }

        // we need this function to dont have an StackOverflowException
        public void OnDrag(object sender, MouseEventArgs e) { }

        // we need this function to dont have an StackOverflowException
        public void OnExeClose() { }
    }
}


using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DirectX_Renderer
{
    class Utilities
    {

        #region dll imports

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        #endregion

        #region directx needed variables

        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        #endregion

        public static System.Drawing.Rectangle GetScreen(Control referenceControl)
        {
            return Screen.FromControl(referenceControl).Bounds;
        }

        public static Bitmap LoadFromFile(RenderTarget renderTarget, string file)
        {
            // Loads from file using System.Drawing.Image
            using (var bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(file))
            {
                var sourceArea = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
                var bitmapProperties = new BitmapProperties(new SharpDX.Direct2D1.PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied));
                var size = new Size2(bitmap.Width, bitmap.Height);

                // Transform pixels from BGRA to RGBA
                int stride = bitmap.Width * sizeof(int);
                using (var tempStream = new DataStream(bitmap.Height * stride, true, true))
                {
                    // Lock System.Drawing.Bitmap
                    var bitmapData = bitmap.LockBits(sourceArea, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

                    // Convert all pixels 
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        int offset = bitmapData.Stride * y;
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            // Not optimized 
                            byte B = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte G = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte R = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            byte A = Marshal.ReadByte(bitmapData.Scan0, offset++);
                            int rgba = R | (G << 8) | (B << 16) | (A << 24);
                            tempStream.Write(rgba);
                        }

                    }
                    bitmap.UnlockBits(bitmapData);
                    tempStream.Position = 0;

                    return new Bitmap(renderTarget, size, tempStream, stride, bitmapProperties);
                }
            }
        }

        public static void CheckOverlayStatus(IntPtr handle, Process process)
        {

            var isFocused = ApplicationIsActivated(process);

            if (isFocused)
            {
                SetWindowPos(handle, new IntPtr(-1), 0, 0, 0, 0, TOPMOST_FLAGS);
            }
            else
            {
                SetWindowPos(handle, new IntPtr(-2), 0, 0, 0, 0, TOPMOST_FLAGS);
            }
        }

        private static bool ApplicationIsActivated(Process process)
        {
            var activatedHandle = GetForegroundWindow(); //get the current window that user is focused
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = process.Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

    }
}

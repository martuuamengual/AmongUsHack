namespace DirectX_Renderer.GUI
{
    partial class BaseGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Overlay_SharpDX
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(850, 330);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Overlay_SharpDX_movable";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Overlay Movable";

            this.Load += new System.EventHandler(this.OnLoad);


            /*this.panel1 = new System.Windows.Forms.Panel();
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right);
            this.panel1.BackColor = System.Drawing.Color.Red;
            this.panel1.Location = new System.Drawing.Point(16, 160);
            this.panel1.Size = new System.Drawing.Size(500, 300);*/

            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
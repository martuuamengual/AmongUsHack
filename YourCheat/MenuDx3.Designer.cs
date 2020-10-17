using System.Windows.Forms;

namespace YourCheat
{
    partial class MenuDx3
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkGray;
            this.panel1.Location = new System.Drawing.Point(482, 214);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(325, 198);
            this.panel1.TabIndex = 0;
            // 
            // MenuDx3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(1380, 784);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.Name = "MenuDx3";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel panel1;
    }
}

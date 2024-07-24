namespace CamSimulator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtLicensePlate;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.ComboBox cmbParkings;

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
            this.txtLicensePlate = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.cmbParkings = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtLicensePlate
            // 
            this.txtLicensePlate.Location = new System.Drawing.Point(15, 15);
            this.txtLicensePlate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtLicensePlate.Name = "txtLicensePlate";
            this.txtLicensePlate.Size = new System.Drawing.Size(302, 23);
            this.txtLicensePlate.TabIndex = 0;
            this.txtLicensePlate.TextChanged += new System.EventHandler(this.txtLicensePlate_TextChanged);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(15, 76);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(302, 27);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Invia";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // cmbParkings
            // 
            this.cmbParkings.Location = new System.Drawing.Point(15, 46);
            this.cmbParkings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cmbParkings.Name = "cmbParkings";
            this.cmbParkings.Size = new System.Drawing.Size(302, 23);
            this.cmbParkings.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 113);
            this.Controls.Add(this.cmbParkings);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtLicensePlate);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Simulazione Telecamera Parcheggio";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}

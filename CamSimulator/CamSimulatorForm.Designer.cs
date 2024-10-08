namespace CamSimulator
{
    partial class CamSimulatorForm
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
            txtLicensePlate = new TextBox();
            btnSend = new Button();
            cmbParkings = new ComboBox();
            SuspendLayout();
            // 
            // txtLicensePlate
            // 
            txtLicensePlate.Location = new Point(15, 15);
            txtLicensePlate.Margin = new Padding(4, 3, 4, 3);
            txtLicensePlate.Name = "txtLicensePlate";
            txtLicensePlate.Size = new Size(302, 23);
            txtLicensePlate.TabIndex = 0;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(15, 76);
            btnSend.Margin = new Padding(4, 3, 4, 3);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(302, 27);
            btnSend.TabIndex = 1;
            btnSend.Text = "Invia";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // cmbParkings
            // 
            cmbParkings.Location = new Point(15, 46);
            cmbParkings.Margin = new Padding(4, 3, 4, 3);
            cmbParkings.Name = "cmbParkings";
            cmbParkings.Size = new Size(302, 23);
            cmbParkings.TabIndex = 2;
            // 
            // CamSimulatorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(331, 113);
            Controls.Add(cmbParkings);
            Controls.Add(btnSend);
            Controls.Add(txtLicensePlate);
            Margin = new Padding(4, 3, 4, 3);
            Name = "CamSimulatorForm";
            Text = "Simulazione Telecamera Parcheggio";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }
    }
}

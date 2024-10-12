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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CamSimulatorForm));
            txtLicensePlate = new TextBox();
            btnSend = new Button();
            cmbParkings = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // txtLicensePlate
            // 
            txtLicensePlate.Location = new Point(13, 71);
            txtLicensePlate.Margin = new Padding(4, 3, 4, 3);
            txtLicensePlate.Name = "txtLicensePlate";
            txtLicensePlate.Size = new Size(166, 23);
            txtLicensePlate.TabIndex = 0;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(189, 12);
            btnSend.Margin = new Padding(4, 3, 4, 3);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(114, 82);
            btnSend.TabIndex = 1;
            btnSend.Text = "Rileva";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // cmbParkings
            // 
            cmbParkings.Location = new Point(15, 27);
            cmbParkings.Margin = new Padding(4, 3, 4, 3);
            cmbParkings.Name = "cmbParkings";
            cmbParkings.Size = new Size(166, 23);
            cmbParkings.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 53);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 3;
            label1.Text = "Targa rilevata";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 9);
            label2.Name = "label2";
            label2.Size = new Size(67, 15);
            label2.TabIndex = 4;
            label2.Text = "Parcheggio";
            // 
            // CamSimulatorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(314, 106);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(cmbParkings);
            Controls.Add(btnSend);
            Controls.Add(txtLicensePlate);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "CamSimulatorForm";
            Text = "Simulatore telecamera";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        private Label label1;
        private Label label2;
    }
}

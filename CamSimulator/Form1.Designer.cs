namespace CamSimulator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtLicensePlate;
        private System.Windows.Forms.Button btnSend;

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
            SuspendLayout();
            // 
            // txtLicensePlate
            // 
            txtLicensePlate.Location = new Point(15, 15);
            txtLicensePlate.Margin = new Padding(4, 3, 4, 3);
            txtLicensePlate.Name = "txtLicensePlate";
            txtLicensePlate.Size = new Size(302, 23);
            txtLicensePlate.TabIndex = 0;
            txtLicensePlate.TextChanged += txtLicensePlate_TextChanged;
            // 
            // btnSend
            // 
            btnSend.Location = new Point(15, 46);
            btnSend.Margin = new Padding(4, 3, 4, 3);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(302, 27);
            btnSend.TabIndex = 1;
            btnSend.Text = "Invia";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(331, 93);
            Controls.Add(btnSend);
            Controls.Add(txtLicensePlate);
            Margin = new Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Simulazione Telecamera Parcheggio";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}

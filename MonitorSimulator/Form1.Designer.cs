namespace MonitorSimulator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxParkingStatus = new System.Windows.Forms.ComboBox();
            this.comboBoxParking = new System.Windows.Forms.ComboBox();
            this.textBoxSlotNumber = new System.Windows.Forms.TextBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.dataGridViewParkingSlots = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParkingSlots)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxParkingStatus
            // 
            this.comboBoxParkingStatus.FormattingEnabled = true;
            this.comboBoxParkingStatus.Location = new System.Drawing.Point(12, 12);
            this.comboBoxParkingStatus.Name = "comboBoxParkingStatus";
            this.comboBoxParkingStatus.Size = new System.Drawing.Size(200, 23);
            this.comboBoxParkingStatus.TabIndex = 0;
            // 
            // comboBoxParking
            // 
            this.comboBoxParking.FormattingEnabled = true;
            this.comboBoxParking.Location = new System.Drawing.Point(12, 41);
            this.comboBoxParking.Name = "comboBoxParking";
            this.comboBoxParking.Size = new System.Drawing.Size(200, 23);
            this.comboBoxParking.TabIndex = 1;
            // 
            // textBoxSlotNumber
            // 
            this.textBoxSlotNumber.Location = new System.Drawing.Point(12, 70);
            this.textBoxSlotNumber.Name = "textBoxSlotNumber";
            this.textBoxSlotNumber.Size = new System.Drawing.Size(200, 23);
            this.textBoxSlotNumber.TabIndex = 2;
            this.textBoxSlotNumber.PlaceholderText = "Ricerca per Slot Number";
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(12, 99);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(200, 23);
            this.buttonSearch.TabIndex = 3;
            this.buttonSearch.Text = "Cerca";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // dataGridViewParkingSlots
            // 
            this.dataGridViewParkingSlots.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewParkingSlots.Location = new System.Drawing.Point(12, 128);
            this.dataGridViewParkingSlots.Name = "dataGridViewParkingSlots";
            this.dataGridViewParkingSlots.RowTemplate.Height = 25;
            this.dataGridViewParkingSlots.Size = new System.Drawing.Size(776, 310);
            this.dataGridViewParkingSlots.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dataGridViewParkingSlots);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.textBoxSlotNumber);
            this.Controls.Add(this.comboBoxParking);
            this.Controls.Add(this.comboBoxParkingStatus);
            this.Name = "Form1";
            this.Text = "Monitor Simulator";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewParkingSlots)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Aggiungi qui i controlli come campi privati
        private System.Windows.Forms.ComboBox comboBoxParkingStatus;
        private System.Windows.Forms.ComboBox comboBoxParking;
        private System.Windows.Forms.TextBox textBoxSlotNumber;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.DataGridView dataGridViewParkingSlots;
    }
}

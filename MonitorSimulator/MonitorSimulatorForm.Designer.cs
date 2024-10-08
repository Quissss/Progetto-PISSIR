namespace MonitorSimulator
{
    partial class MonitorSimulatorForm
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
            comboBoxParkingStatus = new ComboBox();
            comboBoxParking = new ComboBox();
            textBoxSlotNumber = new TextBox();
            buttonSearch = new Button();
            dataGridViewParkingSlots = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dataGridViewParkingSlots).BeginInit();
            SuspendLayout();
            // 
            // comboBoxParkingStatus
            // 
            comboBoxParkingStatus.FormattingEnabled = true;
            comboBoxParkingStatus.Location = new Point(12, 12);
            comboBoxParkingStatus.Name = "comboBoxParkingStatus";
            comboBoxParkingStatus.Size = new Size(200, 23);
            comboBoxParkingStatus.TabIndex = 0;
            // 
            // comboBoxParking
            // 
            comboBoxParking.FormattingEnabled = true;
            comboBoxParking.Location = new Point(12, 41);
            comboBoxParking.Name = "comboBoxParking";
            comboBoxParking.Size = new Size(200, 23);
            comboBoxParking.TabIndex = 1;
            // 
            // textBoxSlotNumber
            // 
            textBoxSlotNumber.Location = new Point(12, 70);
            textBoxSlotNumber.Name = "textBoxSlotNumber";
            textBoxSlotNumber.PlaceholderText = "Ricerca per Slot Number";
            textBoxSlotNumber.Size = new Size(200, 23);
            textBoxSlotNumber.TabIndex = 2;
            // 
            // buttonSearch
            // 
            buttonSearch.Location = new Point(12, 99);
            buttonSearch.Name = "buttonSearch";
            buttonSearch.Size = new Size(200, 23);
            buttonSearch.TabIndex = 3;
            buttonSearch.Text = "Cerca";
            buttonSearch.UseVisualStyleBackColor = true;
            buttonSearch.Click += buttonSearch_Click;
            // 
            // dataGridViewParkingSlots
            // 
            dataGridViewParkingSlots.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewParkingSlots.Location = new Point(12, 128);
            dataGridViewParkingSlots.Name = "dataGridViewParkingSlots";
            dataGridViewParkingSlots.Size = new Size(776, 310);
            dataGridViewParkingSlots.TabIndex = 4;
            // 
            // MonitorSimulatorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dataGridViewParkingSlots);
            Controls.Add(buttonSearch);
            Controls.Add(textBoxSlotNumber);
            Controls.Add(comboBoxParking);
            Controls.Add(comboBoxParkingStatus);
            Name = "MonitorSimulatorForm";
            Text = "Monitor Simulator";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewParkingSlots).EndInit();
            ResumeLayout(false);
            PerformLayout();
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

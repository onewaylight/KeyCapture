namespace KeyCapture
{
    partial class KeyCapture
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
            selectAreaButton = new Button();
            changeSaveLocationButton = new Button();
            instructionLabel = new Label();
            saveLocationLabel = new Label();
            tbTotalCapturedCnt = new TextBox();
            label1 = new Label();
            tbMessage = new TextBox();
            btnOpenFileLocation = new Button();
            lblSavedLocation = new Label();
            SuspendLayout();
            // 
            // selectAreaButton
            // 
            selectAreaButton.Location = new Point(43, 15);
            selectAreaButton.Name = "selectAreaButton";
            selectAreaButton.Size = new Size(140, 50);
            selectAreaButton.TabIndex = 0;
            selectAreaButton.Text = "📷 Select Area";
            selectAreaButton.Click += SelectAreaButton_Click;
            // 
            // changeSaveLocationButton
            // 
            changeSaveLocationButton.Location = new Point(216, 15);
            changeSaveLocationButton.Name = "changeSaveLocationButton";
            changeSaveLocationButton.Size = new Size(140, 50);
            changeSaveLocationButton.TabIndex = 1;
            changeSaveLocationButton.Text = "📂 File Location";
            changeSaveLocationButton.Click += ChangeSaveLocationButton_Click;
            // 
            // instructionLabel
            // 
            instructionLabel.Location = new Point(0, 0);
            instructionLabel.Name = "instructionLabel";
            instructionLabel.Size = new Size(100, 23);
            instructionLabel.TabIndex = 2;
            // 
            // saveLocationLabel
            // 
            saveLocationLabel.Location = new Point(0, 0);
            saveLocationLabel.Name = "saveLocationLabel";
            saveLocationLabel.Size = new Size(100, 23);
            saveLocationLabel.TabIndex = 8;
            // 
            // tbTotalCapturedCnt
            // 
            tbTotalCapturedCnt.Location = new Point(120, 81);
            tbTotalCapturedCnt.Name = "tbTotalCapturedCnt";
            tbTotalCapturedCnt.ReadOnly = true;
            tbTotalCapturedCnt.Size = new Size(42, 27);
            tbTotalCapturedCnt.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(168, 84);
            label1.Name = "label1";
            label1.Size = new Size(116, 20);
            label1.TabIndex = 5;
            label1.Text = "File(s) Captured";
            // 
            // tbMessage
            // 
            tbMessage.Location = new Point(12, 165);
            tbMessage.Name = "tbMessage";
            tbMessage.ReadOnly = true;
            tbMessage.Size = new Size(385, 27);
            tbMessage.TabIndex = 6;
            // 
            // btnOpenFileLocation
            // 
            btnOpenFileLocation.Location = new Point(352, 115);
            btnOpenFileLocation.Name = "btnOpenFileLocation";
            btnOpenFileLocation.Size = new Size(45, 33);
            btnOpenFileLocation.TabIndex = 7;
            btnOpenFileLocation.Text = "📁";
            btnOpenFileLocation.UseVisualStyleBackColor = true;
            btnOpenFileLocation.Click += btnOpenFileLocation_Click;
            // 
            // lblSavedLocation
            // 
            lblSavedLocation.AutoSize = true;
            lblSavedLocation.Location = new Point(12, 121);
            lblSavedLocation.Name = "lblSavedLocation";
            lblSavedLocation.Size = new Size(30, 20);
            lblSavedLocation.TabIndex = 9;
            lblSavedLocation.Text = "💾";
            // 
            // KeyCapture
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(419, 204);
            Controls.Add(lblSavedLocation);
            Controls.Add(btnOpenFileLocation);
            Controls.Add(tbMessage);
            Controls.Add(label1);
            Controls.Add(tbTotalCapturedCnt);
            Controls.Add(selectAreaButton);
            Controls.Add(changeSaveLocationButton);
            Controls.Add(instructionLabel);
            Controls.Add(saveLocationLabel);
            Name = "KeyCapture";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Key Capture";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button selectAreaButton;
        private Button changeSaveLocationButton;
        private Label instructionLabel;
        private Label saveLocationLabel;
        private TextBox tbTotalCapturedCnt;
        private Label label1;
        private TextBox tbMessage;
        private Button btnOpenFileLocation;
        private Label lblSavedLocation;
    }
}

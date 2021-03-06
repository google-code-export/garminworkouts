namespace GarminFitnessPlugin.View
{
    partial class SelectDeviceDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OKButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.DevicesComboBox = new System.Windows.Forms.ComboBox();
            this.IntroLabel = new System.Windows.Forms.Label();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(52, 162);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel_Button.Location = new System.Drawing.Point(133, 162);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 1;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
            // 
            // DevicesComboBox
            // 
            this.DevicesComboBox.FormattingEnabled = true;
            this.DevicesComboBox.Location = new System.Drawing.Point(38, 89);
            this.DevicesComboBox.Name = "DevicesComboBox";
            this.DevicesComboBox.Size = new System.Drawing.Size(184, 21);
            this.DevicesComboBox.TabIndex = 2;
            this.DevicesComboBox.SelectionChangeCommitted += new System.EventHandler(this.DevicesComboBox_SelectionChangeCommitted);
            // 
            // IntroLabel
            // 
            this.IntroLabel.Location = new System.Drawing.Point(12, 9);
            this.IntroLabel.Name = "IntroLabel";
            this.IntroLabel.Size = new System.Drawing.Size(237, 77);
            this.IntroLabel.TabIndex = 3;
            this.IntroLabel.Text = "Select your device below.  If your device doesn\'t appear, make sure it is connect" +
                "ed to your PC and hit the refresh button.";
            // 
            // RefreshButton
            // 
            this.RefreshButton.AutoSize = true;
            this.RefreshButton.Location = new System.Drawing.Point(83, 116);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(94, 23);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "Refresh devices";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // SelectDeviceDialog
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(261, 197);
            this.Controls.Add(this.IntroLabel);
            this.Controls.Add(this.DevicesComboBox);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SelectDeviceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select Device";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button Cancel_Button;
        private System.Windows.Forms.ComboBox DevicesComboBox;
        private System.Windows.Forms.Label IntroLabel;
        private System.Windows.Forms.Button RefreshButton;
    }
}
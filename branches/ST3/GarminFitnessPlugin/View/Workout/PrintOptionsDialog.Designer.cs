namespace GarminFitnessPlugin.View
{
    partial class PrintOptionsDialog
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
            this.InkFriendlyCheckBox = new System.Windows.Forms.CheckBox();
            this.PrintButton = new System.Windows.Forms.Button();
            this.PrintPreviewButton = new System.Windows.Forms.Button();
            this.Cancel_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // InkFriendlyCheckBox
            // 
            this.InkFriendlyCheckBox.AutoSize = true;
            this.InkFriendlyCheckBox.Checked = true;
            this.InkFriendlyCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.InkFriendlyCheckBox.Location = new System.Drawing.Point(13, 13);
            this.InkFriendlyCheckBox.Name = "InkFriendlyCheckBox";
            this.InkFriendlyCheckBox.Size = new System.Drawing.Size(106, 17);
            this.InkFriendlyCheckBox.TabIndex = 0;
            this.InkFriendlyCheckBox.Text = "Ink friendly mode";
            this.InkFriendlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(12, 38);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(75, 23);
            this.PrintButton.TabIndex = 1;
            this.PrintButton.Text = "Print";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // PrintPreviewButton
            // 
            this.PrintPreviewButton.Location = new System.Drawing.Point(93, 38);
            this.PrintPreviewButton.Name = "PrintPreviewButton";
            this.PrintPreviewButton.Size = new System.Drawing.Size(89, 23);
            this.PrintPreviewButton.TabIndex = 1;
            this.PrintPreviewButton.Text = "Print preview";
            this.PrintPreviewButton.UseVisualStyleBackColor = true;
            this.PrintPreviewButton.Click += new System.EventHandler(this.PrintPreviewButton_Click);
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Location = new System.Drawing.Point(202, 38);
            this.Cancel_Button.Name = "Cancel_Button";
            this.Cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.Cancel_Button.TabIndex = 1;
            this.Cancel_Button.Text = "Cancel";
            this.Cancel_Button.UseVisualStyleBackColor = true;
            // 
            // PrintOptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel_Button;
            this.ClientSize = new System.Drawing.Size(290, 73);
            this.Controls.Add(this.Cancel_Button);
            this.Controls.Add(this.PrintPreviewButton);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.InkFriendlyCheckBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrintOptionsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Print settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox InkFriendlyCheckBox;
        private System.Windows.Forms.Button PrintButton;
        private System.Windows.Forms.Button PrintPreviewButton;
        private System.Windows.Forms.Button Cancel_Button;
    }
}
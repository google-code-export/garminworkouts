namespace GarminFitnessPlugin.View
{
    partial class SetupWizardImportOrEditProfileControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ImportProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.EditProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // ImportProfileRadioButton
            // 
            this.ImportProfileRadioButton.AutoSize = true;
            this.ImportProfileRadioButton.Location = new System.Drawing.Point(4, 103);
            this.ImportProfileRadioButton.Name = "ImportProfileRadioButton";
            this.ImportProfileRadioButton.Size = new System.Drawing.Size(54, 17);
            this.ImportProfileRadioButton.TabIndex = 0;
            this.ImportProfileRadioButton.TabStop = true;
            this.ImportProfileRadioButton.Text = "Import";
            this.ImportProfileRadioButton.UseVisualStyleBackColor = true;
            this.ImportProfileRadioButton.CheckedChanged += new System.EventHandler(this.ImportProfileRadioButton_CheckedChanged);
            // 
            // EditProfileRadioButton
            // 
            this.EditProfileRadioButton.AutoSize = true;
            this.EditProfileRadioButton.Location = new System.Drawing.Point(4, 126);
            this.EditProfileRadioButton.Name = "EditProfileRadioButton";
            this.EditProfileRadioButton.Size = new System.Drawing.Size(43, 17);
            this.EditProfileRadioButton.TabIndex = 0;
            this.EditProfileRadioButton.TabStop = true;
            this.EditProfileRadioButton.Text = "Edit";
            this.EditProfileRadioButton.UseVisualStyleBackColor = true;
            this.EditProfileRadioButton.CheckedChanged += new System.EventHandler(this.EditProfileRadioButton_CheckedChanged);
            // 
            // SetupWizardImportOrEditProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.EditProfileRadioButton);
            this.Controls.Add(this.ImportProfileRadioButton);
            this.Name = "SetupWizardImportOrEditProfileControl";
            this.Size = new System.Drawing.Size(500, 299);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton ImportProfileRadioButton;
        private System.Windows.Forms.RadioButton EditProfileRadioButton;
    }
}

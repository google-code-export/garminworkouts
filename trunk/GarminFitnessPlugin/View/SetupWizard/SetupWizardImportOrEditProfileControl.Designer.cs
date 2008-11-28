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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizardImportOrEditProfileControl));
            this.ImportProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.EditProfileRadioButton = new System.Windows.Forms.RadioButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // ImportProfileRadioButton
            // 
            this.ImportProfileRadioButton.AutoSize = true;
            this.ImportProfileRadioButton.Checked = true;
            this.ImportProfileRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ImportProfileRadioButton.Location = new System.Drawing.Point(4, 100);
            this.ImportProfileRadioButton.Name = "ImportProfileRadioButton";
            this.ImportProfileRadioButton.Size = new System.Drawing.Size(264, 24);
            this.ImportProfileRadioButton.TabIndex = 0;
            this.ImportProfileRadioButton.TabStop = true;
            this.ImportProfileRadioButton.Text = "Import my profile from my unit";
            this.ImportProfileRadioButton.UseVisualStyleBackColor = true;
            this.ImportProfileRadioButton.CheckedChanged += new System.EventHandler(this.ImportProfileRadioButton_CheckedChanged);
            // 
            // EditProfileRadioButton
            // 
            this.EditProfileRadioButton.AutoSize = true;
            this.EditProfileRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditProfileRadioButton.Location = new System.Drawing.Point(4, 140);
            this.EditProfileRadioButton.Name = "EditProfileRadioButton";
            this.EditProfileRadioButton.Size = new System.Drawing.Size(216, 24);
            this.EditProfileRadioButton.TabIndex = 0;
            this.EditProfileRadioButton.Text = "Edit my profile manually";
            this.EditProfileRadioButton.UseVisualStyleBackColor = true;
            this.EditProfileRadioButton.CheckedChanged += new System.EventHandler(this.EditProfileRadioButton_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Properties.Resources.GarminLogo;
            this.pictureBox1.Location = new System.Drawing.Point(3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(184, 70);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(193, 6);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(323, 70);
            this.ExplanationLabel.TabIndex = 2;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // SetupWizardImportOrEditProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Controls.Add(this.EditProfileRadioButton);
            this.Controls.Add(this.ImportProfileRadioButton);
            this.Name = "SetupWizardImportOrEditProfileControl";
            this.Size = new System.Drawing.Size(520, 177);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton ImportProfileRadioButton;
        private System.Windows.Forms.RadioButton EditProfileRadioButton;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
    }
}

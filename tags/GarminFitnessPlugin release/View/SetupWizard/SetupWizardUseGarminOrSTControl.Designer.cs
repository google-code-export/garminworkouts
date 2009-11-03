namespace GarminFitnessPlugin.View
{
    partial class SetupWizardUseGarminOrSTControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWizardUseGarminOrSTControl));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.STModeRadioButton = new System.Windows.Forms.RadioButton();
            this.GarminModeRadioButton = new System.Windows.Forms.RadioButton();
            this.IndependentModeRadioButton = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(184, 70);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // ExplanationLabel
            // 
            this.ExplanationLabel.Location = new System.Drawing.Point(194, 6);
            this.ExplanationLabel.Name = "ExplanationLabel";
            this.ExplanationLabel.Size = new System.Drawing.Size(403, 84);
            this.ExplanationLabel.TabIndex = 2;
            this.ExplanationLabel.Text = resources.GetString("ExplanationLabel.Text");
            // 
            // STModeRadioButton
            // 
            this.STModeRadioButton.AutoSize = true;
            this.STModeRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.STModeRadioButton.Location = new System.Drawing.Point(4, 100);
            this.STModeRadioButton.Name = "STModeRadioButton";
            this.STModeRadioButton.Size = new System.Drawing.Size(313, 24);
            this.STModeRadioButton.TabIndex = 4;
            this.STModeRadioButton.TabStop = true;
            this.STModeRadioButton.Text = "I want to use the SportTracks mode";
            this.STModeRadioButton.UseVisualStyleBackColor = true;
            this.STModeRadioButton.CheckedChanged += new System.EventHandler(this.STZonesRadioButton_CheckedChanged);
            // 
            // GarminModeRadioButton
            // 
            this.GarminModeRadioButton.AutoSize = true;
            this.GarminModeRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GarminModeRadioButton.Location = new System.Drawing.Point(4, 140);
            this.GarminModeRadioButton.Name = "GarminModeRadioButton";
            this.GarminModeRadioButton.Size = new System.Drawing.Size(274, 24);
            this.GarminModeRadioButton.TabIndex = 4;
            this.GarminModeRadioButton.TabStop = true;
            this.GarminModeRadioButton.Text = "I want to use the Garmin mode";
            this.GarminModeRadioButton.UseVisualStyleBackColor = true;
            this.GarminModeRadioButton.CheckedChanged += new System.EventHandler(this.GarminZonesRadioButton_CheckedChanged);
            // 
            // IndependentModeRadioButton
            // 
            this.IndependentModeRadioButton.AutoSize = true;
            this.IndependentModeRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IndependentModeRadioButton.Location = new System.Drawing.Point(4, 180);
            this.IndependentModeRadioButton.Name = "IndependentModeRadioButton";
            this.IndependentModeRadioButton.Size = new System.Drawing.Size(378, 24);
            this.IndependentModeRadioButton.TabIndex = 4;
            this.IndependentModeRadioButton.TabStop = true;
            this.IndependentModeRadioButton.Text = "I want to configure the zones independently";
            this.IndependentModeRadioButton.UseVisualStyleBackColor = true;
            this.IndependentModeRadioButton.CheckedChanged += new System.EventHandler(this.IndependentModeRadioButton_CheckedChanged);
            // 
            // SetupWizardUseGarminOrSTControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.IndependentModeRadioButton);
            this.Controls.Add(this.GarminModeRadioButton);
            this.Controls.Add(this.STModeRadioButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardUseGarminOrSTControl";
            this.Size = new System.Drawing.Size(600, 214);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.RadioButton STModeRadioButton;
        private System.Windows.Forms.RadioButton GarminModeRadioButton;
        private System.Windows.Forms.RadioButton IndependentModeRadioButton;
    }
}

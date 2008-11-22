namespace GarminFitnessPlugin.View
{
    partial class SetupWizardSetupSTGarminZonesControl
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ExplanationLabel = new System.Windows.Forms.Label();
            this.DefaultPowerZonesLabel = new System.Windows.Forms.Label();
            this.SpeedSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.SpeedSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.DefaultSpeedZoneLabel = new System.Windows.Forms.Label();
            this.SpeedGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.HRGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.DefaultHeartRateZonesLabel = new System.Windows.Forms.Label();
            this.HRSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.HRSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerSettingsGroupBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SpeedSettingsGroupBox.SuspendLayout();
            this.HRSettingsGroupBox.SuspendLayout();
            this.PowerSettingsGroupBox.SuspendLayout();
            this.SuspendLayout();
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
            this.ExplanationLabel.Text = "You have chosen to configure your zone usage manually.  You can do so on this pag" +
                "e.  Note that these settings are always available in the Garmin fitness Plugin \"" +
                "Settings\" page.";
            // 
            // DefaultPowerZonesLabel
            // 
            this.DefaultPowerZonesLabel.AutoSize = true;
            this.DefaultPowerZonesLabel.Location = new System.Drawing.Point(6, 16);
            this.DefaultPowerZonesLabel.Name = "DefaultPowerZonesLabel";
            this.DefaultPowerZonesLabel.Size = new System.Drawing.Size(136, 13);
            this.DefaultPowerZonesLabel.TabIndex = 0;
            this.DefaultPowerZonesLabel.Text = "Use the power zones from :";
            // 
            // SpeedSettingsGroupBox
            // 
            this.SpeedSettingsGroupBox.Controls.Add(this.SpeedSportTracksRadioButton);
            this.SpeedSettingsGroupBox.Controls.Add(this.DefaultSpeedZoneLabel);
            this.SpeedSettingsGroupBox.Controls.Add(this.SpeedGarminRadioButton);
            this.SpeedSettingsGroupBox.Location = new System.Drawing.Point(3, 141);
            this.SpeedSettingsGroupBox.Name = "SpeedSettingsGroupBox";
            this.SpeedSettingsGroupBox.Size = new System.Drawing.Size(513, 37);
            this.SpeedSettingsGroupBox.TabIndex = 7;
            this.SpeedSettingsGroupBox.TabStop = false;
            this.SpeedSettingsGroupBox.Text = "Speed Settings";
            // 
            // SpeedSportTracksRadioButton
            // 
            this.SpeedSportTracksRadioButton.AutoSize = true;
            this.SpeedSportTracksRadioButton.Checked = true;
            this.SpeedSportTracksRadioButton.Location = new System.Drawing.Point(394, 11);
            this.SpeedSportTracksRadioButton.Name = "SpeedSportTracksRadioButton";
            this.SpeedSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.SpeedSportTracksRadioButton.TabIndex = 1;
            this.SpeedSportTracksRadioButton.TabStop = true;
            this.SpeedSportTracksRadioButton.Text = "SportTracks";
            this.SpeedSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.SpeedSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.SpeedSportTracksRadioButton_CheckedChanged);
            // 
            // DefaultSpeedZoneLabel
            // 
            this.DefaultSpeedZoneLabel.AutoSize = true;
            this.DefaultSpeedZoneLabel.Location = new System.Drawing.Point(6, 16);
            this.DefaultSpeedZoneLabel.Name = "DefaultSpeedZoneLabel";
            this.DefaultSpeedZoneLabel.Size = new System.Drawing.Size(136, 13);
            this.DefaultSpeedZoneLabel.TabIndex = 0;
            this.DefaultSpeedZoneLabel.Text = "Use the speed zones from :";
            // 
            // SpeedGarminRadioButton
            // 
            this.SpeedGarminRadioButton.AutoSize = true;
            this.SpeedGarminRadioButton.Location = new System.Drawing.Point(262, 13);
            this.SpeedGarminRadioButton.Name = "SpeedGarminRadioButton";
            this.SpeedGarminRadioButton.Size = new System.Drawing.Size(58, 17);
            this.SpeedGarminRadioButton.TabIndex = 1;
            this.SpeedGarminRadioButton.Text = "Garmin";
            this.SpeedGarminRadioButton.UseVisualStyleBackColor = true;
            this.SpeedGarminRadioButton.CheckedChanged += new System.EventHandler(this.SpeedGarminRadioButton_CheckedChanged);
            // 
            // PowerGarminRadioButton
            // 
            this.PowerGarminRadioButton.AutoSize = true;
            this.PowerGarminRadioButton.Location = new System.Drawing.Point(262, 13);
            this.PowerGarminRadioButton.Name = "PowerGarminRadioButton";
            this.PowerGarminRadioButton.Size = new System.Drawing.Size(58, 17);
            this.PowerGarminRadioButton.TabIndex = 1;
            this.PowerGarminRadioButton.TabStop = true;
            this.PowerGarminRadioButton.Text = "Garmin";
            this.PowerGarminRadioButton.UseVisualStyleBackColor = true;
            this.PowerGarminRadioButton.CheckedChanged += new System.EventHandler(this.PowerGarminRadioButton_CheckedChanged);
            // 
            // PowerSportTracksRadioButton
            // 
            this.PowerSportTracksRadioButton.AutoSize = true;
            this.PowerSportTracksRadioButton.Location = new System.Drawing.Point(394, 13);
            this.PowerSportTracksRadioButton.Name = "PowerSportTracksRadioButton";
            this.PowerSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.PowerSportTracksRadioButton.TabIndex = 1;
            this.PowerSportTracksRadioButton.TabStop = true;
            this.PowerSportTracksRadioButton.Text = "SportTracks";
            this.PowerSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.PowerSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.PowerSportTracksRadioButton_CheckedChanged);
            // 
            // HRGarminRadioButton
            // 
            this.HRGarminRadioButton.AutoSize = true;
            this.HRGarminRadioButton.Checked = true;
            this.HRGarminRadioButton.Location = new System.Drawing.Point(262, 13);
            this.HRGarminRadioButton.Name = "HRGarminRadioButton";
            this.HRGarminRadioButton.Size = new System.Drawing.Size(58, 17);
            this.HRGarminRadioButton.TabIndex = 1;
            this.HRGarminRadioButton.TabStop = true;
            this.HRGarminRadioButton.Text = "Garmin";
            this.HRGarminRadioButton.UseVisualStyleBackColor = true;
            this.HRGarminRadioButton.CheckedChanged += new System.EventHandler(this.HRGarminRadioButton_CheckedChanged);
            // 
            // DefaultHeartRateZonesLabel
            // 
            this.DefaultHeartRateZonesLabel.AutoSize = true;
            this.DefaultHeartRateZonesLabel.Location = new System.Drawing.Point(6, 16);
            this.DefaultHeartRateZonesLabel.Name = "DefaultHeartRateZonesLabel";
            this.DefaultHeartRateZonesLabel.Size = new System.Drawing.Size(152, 13);
            this.DefaultHeartRateZonesLabel.TabIndex = 0;
            this.DefaultHeartRateZonesLabel.Text = "Use the heart rate zones from :";
            // 
            // HRSettingsGroupBox
            // 
            this.HRSettingsGroupBox.Controls.Add(this.DefaultHeartRateZonesLabel);
            this.HRSettingsGroupBox.Controls.Add(this.HRGarminRadioButton);
            this.HRSettingsGroupBox.Controls.Add(this.HRSportTracksRadioButton);
            this.HRSettingsGroupBox.Location = new System.Drawing.Point(3, 93);
            this.HRSettingsGroupBox.Name = "HRSettingsGroupBox";
            this.HRSettingsGroupBox.Size = new System.Drawing.Size(513, 37);
            this.HRSettingsGroupBox.TabIndex = 6;
            this.HRSettingsGroupBox.TabStop = false;
            this.HRSettingsGroupBox.Text = "Heart Rate Settings";
            // 
            // HRSportTracksRadioButton
            // 
            this.HRSportTracksRadioButton.AutoSize = true;
            this.HRSportTracksRadioButton.Location = new System.Drawing.Point(394, 13);
            this.HRSportTracksRadioButton.Name = "HRSportTracksRadioButton";
            this.HRSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.HRSportTracksRadioButton.TabIndex = 1;
            this.HRSportTracksRadioButton.Text = "SportTracks";
            this.HRSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.HRSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.HRSportTracksRadioButton_CheckedChanged);
            // 
            // PowerSettingsGroupBox
            // 
            this.PowerSettingsGroupBox.Controls.Add(this.DefaultPowerZonesLabel);
            this.PowerSettingsGroupBox.Controls.Add(this.PowerGarminRadioButton);
            this.PowerSettingsGroupBox.Controls.Add(this.PowerSportTracksRadioButton);
            this.PowerSettingsGroupBox.Location = new System.Drawing.Point(3, 188);
            this.PowerSettingsGroupBox.Name = "PowerSettingsGroupBox";
            this.PowerSettingsGroupBox.Size = new System.Drawing.Size(513, 37);
            this.PowerSettingsGroupBox.TabIndex = 8;
            this.PowerSettingsGroupBox.TabStop = false;
            this.PowerSettingsGroupBox.Text = "Power Settings";
            // 
            // SetupWizardSetupSTGarminZonesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SpeedSettingsGroupBox);
            this.Controls.Add(this.HRSettingsGroupBox);
            this.Controls.Add(this.PowerSettingsGroupBox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardSetupSTGarminZonesControl";
            this.Size = new System.Drawing.Size(520, 229);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.SpeedSettingsGroupBox.ResumeLayout(false);
            this.SpeedSettingsGroupBox.PerformLayout();
            this.HRSettingsGroupBox.ResumeLayout(false);
            this.HRSettingsGroupBox.PerformLayout();
            this.PowerSettingsGroupBox.ResumeLayout(false);
            this.PowerSettingsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.Label DefaultPowerZonesLabel;
        private System.Windows.Forms.GroupBox SpeedSettingsGroupBox;
        private System.Windows.Forms.RadioButton SpeedSportTracksRadioButton;
        private System.Windows.Forms.Label DefaultSpeedZoneLabel;
        private System.Windows.Forms.RadioButton SpeedGarminRadioButton;
        private System.Windows.Forms.RadioButton PowerGarminRadioButton;
        private System.Windows.Forms.RadioButton PowerSportTracksRadioButton;
        private System.Windows.Forms.RadioButton HRGarminRadioButton;
        private System.Windows.Forms.Label DefaultHeartRateZonesLabel;
        private System.Windows.Forms.GroupBox HRSettingsGroupBox;
        private System.Windows.Forms.RadioButton HRSportTracksRadioButton;
        private System.Windows.Forms.GroupBox PowerSettingsGroupBox;
    }
}

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
            this.SpeedSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.DefaultSpeedZoneLabel = new System.Windows.Forms.Label();
            this.SpeedGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.HRGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.DefaultHeartRateZonesLabel = new System.Windows.Forms.Label();
            this.HRSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.PercentMaxRadioButton = new System.Windows.Forms.RadioButton();
            this.BPMRadioButton = new System.Windows.Forms.RadioButton();
            this.ExportSTHRZonesAsLabel = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.PercentFTPRadioButton = new System.Windows.Forms.RadioButton();
            this.WattsRadioButton = new System.Windows.Forms.RadioButton();
            this.ExportSTPowerZonesAsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
            this.pictureBox1.InitialImage = global::GarminFitnessPlugin.Resources.Resources.GarminLogo;
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
            this.ExplanationLabel.Size = new System.Drawing.Size(404, 70);
            this.ExplanationLabel.TabIndex = 2;
            this.ExplanationLabel.Text = "You have chosen to configure your zone usage manually.  You can do so on this pag" +
                "e.  This will determine what heart rate, speed and power zones will be available" +
                " in your workouts.";
            // 
            // DefaultPowerZonesLabel
            // 
            this.DefaultPowerZonesLabel.AutoSize = true;
            this.DefaultPowerZonesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefaultPowerZonesLabel.Location = new System.Drawing.Point(4, 9);
            this.DefaultPowerZonesLabel.Name = "DefaultPowerZonesLabel";
            this.DefaultPowerZonesLabel.Size = new System.Drawing.Size(229, 20);
            this.DefaultPowerZonesLabel.TabIndex = 0;
            this.DefaultPowerZonesLabel.Text = "Use the power zones from :";
            // 
            // SpeedSportTracksRadioButton
            // 
            this.SpeedSportTracksRadioButton.AutoSize = true;
            this.SpeedSportTracksRadioButton.Checked = true;
            this.SpeedSportTracksRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpeedSportTracksRadioButton.Location = new System.Drawing.Point(423, 30);
            this.SpeedSportTracksRadioButton.Name = "SpeedSportTracksRadioButton";
            this.SpeedSportTracksRadioButton.Size = new System.Drawing.Size(124, 24);
            this.SpeedSportTracksRadioButton.TabIndex = 1;
            this.SpeedSportTracksRadioButton.TabStop = true;
            this.SpeedSportTracksRadioButton.Text = "SportTracks";
            this.SpeedSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.SpeedSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.SpeedSportTracksRadioButton_CheckedChanged);
            // 
            // DefaultSpeedZoneLabel
            // 
            this.DefaultSpeedZoneLabel.AutoSize = true;
            this.DefaultSpeedZoneLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefaultSpeedZoneLabel.Location = new System.Drawing.Point(4, 9);
            this.DefaultSpeedZoneLabel.Name = "DefaultSpeedZoneLabel";
            this.DefaultSpeedZoneLabel.Size = new System.Drawing.Size(230, 20);
            this.DefaultSpeedZoneLabel.TabIndex = 0;
            this.DefaultSpeedZoneLabel.Text = "Use the speed zones from :";
            // 
            // SpeedGarminRadioButton
            // 
            this.SpeedGarminRadioButton.AutoSize = true;
            this.SpeedGarminRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpeedGarminRadioButton.Location = new System.Drawing.Point(270, 30);
            this.SpeedGarminRadioButton.Name = "SpeedGarminRadioButton";
            this.SpeedGarminRadioButton.Size = new System.Drawing.Size(85, 24);
            this.SpeedGarminRadioButton.TabIndex = 1;
            this.SpeedGarminRadioButton.Text = "Garmin";
            this.SpeedGarminRadioButton.UseVisualStyleBackColor = true;
            this.SpeedGarminRadioButton.CheckedChanged += new System.EventHandler(this.SpeedGarminRadioButton_CheckedChanged);
            // 
            // PowerGarminRadioButton
            // 
            this.PowerGarminRadioButton.AutoSize = true;
            this.PowerGarminRadioButton.Checked = true;
            this.PowerGarminRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerGarminRadioButton.Location = new System.Drawing.Point(270, 32);
            this.PowerGarminRadioButton.Name = "PowerGarminRadioButton";
            this.PowerGarminRadioButton.Size = new System.Drawing.Size(85, 24);
            this.PowerGarminRadioButton.TabIndex = 1;
            this.PowerGarminRadioButton.TabStop = true;
            this.PowerGarminRadioButton.Text = "Garmin";
            this.PowerGarminRadioButton.UseVisualStyleBackColor = true;
            this.PowerGarminRadioButton.CheckedChanged += new System.EventHandler(this.PowerGarminRadioButton_CheckedChanged);
            // 
            // PowerSportTracksRadioButton
            // 
            this.PowerSportTracksRadioButton.AutoSize = true;
            this.PowerSportTracksRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PowerSportTracksRadioButton.Location = new System.Drawing.Point(423, 31);
            this.PowerSportTracksRadioButton.Name = "PowerSportTracksRadioButton";
            this.PowerSportTracksRadioButton.Size = new System.Drawing.Size(124, 24);
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
            this.HRGarminRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HRGarminRadioButton.Location = new System.Drawing.Point(269, 31);
            this.HRGarminRadioButton.Name = "HRGarminRadioButton";
            this.HRGarminRadioButton.Size = new System.Drawing.Size(85, 24);
            this.HRGarminRadioButton.TabIndex = 1;
            this.HRGarminRadioButton.TabStop = true;
            this.HRGarminRadioButton.Text = "Garmin";
            this.HRGarminRadioButton.UseVisualStyleBackColor = true;
            this.HRGarminRadioButton.CheckedChanged += new System.EventHandler(this.HRGarminRadioButton_CheckedChanged);
            // 
            // DefaultHeartRateZonesLabel
            // 
            this.DefaultHeartRateZonesLabel.AutoSize = true;
            this.DefaultHeartRateZonesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefaultHeartRateZonesLabel.Location = new System.Drawing.Point(4, 9);
            this.DefaultHeartRateZonesLabel.Name = "DefaultHeartRateZonesLabel";
            this.DefaultHeartRateZonesLabel.Size = new System.Drawing.Size(260, 20);
            this.DefaultHeartRateZonesLabel.TabIndex = 0;
            this.DefaultHeartRateZonesLabel.Text = "Use the heart rate zones from :";
            // 
            // HRSportTracksRadioButton
            // 
            this.HRSportTracksRadioButton.AutoSize = true;
            this.HRSportTracksRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HRSportTracksRadioButton.Location = new System.Drawing.Point(422, 31);
            this.HRSportTracksRadioButton.Name = "HRSportTracksRadioButton";
            this.HRSportTracksRadioButton.Size = new System.Drawing.Size(124, 24);
            this.HRSportTracksRadioButton.TabIndex = 1;
            this.HRSportTracksRadioButton.Text = "SportTracks";
            this.HRSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.HRSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.HRSportTracksRadioButton_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.ExportSTHRZonesAsLabel);
            this.panel1.Controls.Add(this.DefaultHeartRateZonesLabel);
            this.panel1.Controls.Add(this.HRGarminRadioButton);
            this.panel1.Controls.Add(this.HRSportTracksRadioButton);
            this.panel1.Location = new System.Drawing.Point(0, 91);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(597, 88);
            this.panel1.TabIndex = 4;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.PercentMaxRadioButton);
            this.panel5.Controls.Add(this.BPMRadioButton);
            this.panel5.Location = new System.Drawing.Point(258, 57);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(336, 28);
            this.panel5.TabIndex = 4;
            // 
            // PercentMaxRadioButton
            // 
            this.PercentMaxRadioButton.AutoSize = true;
            this.PercentMaxRadioButton.Checked = true;
            this.PercentMaxRadioButton.Enabled = false;
            this.PercentMaxRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PercentMaxRadioButton.Location = new System.Drawing.Point(12, 3);
            this.PercentMaxRadioButton.Name = "PercentMaxRadioButton";
            this.PercentMaxRadioButton.Size = new System.Drawing.Size(110, 24);
            this.PercentMaxRadioButton.TabIndex = 1;
            this.PercentMaxRadioButton.TabStop = true;
            this.PercentMaxRadioButton.Text = "% HR Max";
            this.PercentMaxRadioButton.UseVisualStyleBackColor = true;
            this.PercentMaxRadioButton.CheckedChanged += new System.EventHandler(this.PercentMaxRadioButton_CheckedChanged);
            // 
            // BPMRadioButton
            // 
            this.BPMRadioButton.AutoSize = true;
            this.BPMRadioButton.Enabled = false;
            this.BPMRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BPMRadioButton.Location = new System.Drawing.Point(164, 3);
            this.BPMRadioButton.Name = "BPMRadioButton";
            this.BPMRadioButton.Size = new System.Drawing.Size(64, 24);
            this.BPMRadioButton.TabIndex = 1;
            this.BPMRadioButton.Text = "BPM";
            this.BPMRadioButton.UseVisualStyleBackColor = true;
            this.BPMRadioButton.CheckedChanged += new System.EventHandler(this.BPMRadioButton_CheckedChanged);
            // 
            // ExportSTHRZonesAsLabel
            // 
            this.ExportSTHRZonesAsLabel.AutoSize = true;
            this.ExportSTHRZonesAsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportSTHRZonesAsLabel.Location = new System.Drawing.Point(4, 60);
            this.ExportSTHRZonesAsLabel.Name = "ExportSTHRZonesAsLabel";
            this.ExportSTHRZonesAsLabel.Size = new System.Drawing.Size(232, 20);
            this.ExportSTHRZonesAsLabel.TabIndex = 3;
            this.ExportSTHRZonesAsLabel.Text = "Export heart rate zones as :";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.SpeedSportTracksRadioButton);
            this.panel2.Controls.Add(this.DefaultSpeedZoneLabel);
            this.panel2.Controls.Add(this.SpeedGarminRadioButton);
            this.panel2.Location = new System.Drawing.Point(0, 179);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(598, 65);
            this.panel2.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.ExportSTPowerZonesAsLabel);
            this.panel3.Controls.Add(this.DefaultPowerZonesLabel);
            this.panel3.Controls.Add(this.PowerGarminRadioButton);
            this.panel3.Controls.Add(this.PowerSportTracksRadioButton);
            this.panel3.Location = new System.Drawing.Point(0, 244);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(598, 93);
            this.panel3.TabIndex = 4;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.PercentFTPRadioButton);
            this.panel4.Controls.Add(this.WattsRadioButton);
            this.panel4.Location = new System.Drawing.Point(258, 61);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(337, 28);
            this.panel4.TabIndex = 6;
            // 
            // PercentFTPRadioButton
            // 
            this.PercentFTPRadioButton.AutoSize = true;
            this.PercentFTPRadioButton.Checked = true;
            this.PercentFTPRadioButton.Enabled = false;
            this.PercentFTPRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.PercentFTPRadioButton.Location = new System.Drawing.Point(12, 3);
            this.PercentFTPRadioButton.Name = "PercentFTPRadioButton";
            this.PercentFTPRadioButton.Size = new System.Drawing.Size(79, 24);
            this.PercentFTPRadioButton.TabIndex = 1;
            this.PercentFTPRadioButton.TabStop = true;
            this.PercentFTPRadioButton.Text = "% FTP";
            this.PercentFTPRadioButton.UseVisualStyleBackColor = true;
            this.PercentFTPRadioButton.CheckedChanged += new System.EventHandler(this.PercentFTPRadioButton_CheckedChanged);
            // 
            // WattsRadioButton
            // 
            this.WattsRadioButton.AutoSize = true;
            this.WattsRadioButton.Enabled = false;
            this.WattsRadioButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WattsRadioButton.Location = new System.Drawing.Point(164, 3);
            this.WattsRadioButton.Name = "WattsRadioButton";
            this.WattsRadioButton.Size = new System.Drawing.Size(74, 24);
            this.WattsRadioButton.TabIndex = 1;
            this.WattsRadioButton.Text = "Watts";
            this.WattsRadioButton.UseVisualStyleBackColor = true;
            this.WattsRadioButton.CheckedChanged += new System.EventHandler(this.WattsRadioButton_CheckedChanged);
            // 
            // ExportSTPowerZonesAsLabel
            // 
            this.ExportSTPowerZonesAsLabel.AutoSize = true;
            this.ExportSTPowerZonesAsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportSTPowerZonesAsLabel.Location = new System.Drawing.Point(4, 61);
            this.ExportSTPowerZonesAsLabel.Name = "ExportSTPowerZonesAsLabel";
            this.ExportSTPowerZonesAsLabel.Size = new System.Drawing.Size(201, 20);
            this.ExportSTPowerZonesAsLabel.TabIndex = 5;
            this.ExportSTPowerZonesAsLabel.Text = "Export power zones as :";
            // 
            // SetupWizardSetupSTGarminZonesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "SetupWizardSetupSTGarminZonesControl";
            this.Size = new System.Drawing.Size(600, 353);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.Label DefaultPowerZonesLabel;
        private System.Windows.Forms.RadioButton SpeedSportTracksRadioButton;
        private System.Windows.Forms.Label DefaultSpeedZoneLabel;
        private System.Windows.Forms.RadioButton SpeedGarminRadioButton;
        private System.Windows.Forms.RadioButton PowerGarminRadioButton;
        private System.Windows.Forms.RadioButton PowerSportTracksRadioButton;
        private System.Windows.Forms.RadioButton HRGarminRadioButton;
        private System.Windows.Forms.Label DefaultHeartRateZonesLabel;
        private System.Windows.Forms.RadioButton HRSportTracksRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.RadioButton PercentFTPRadioButton;
        private System.Windows.Forms.RadioButton WattsRadioButton;
        private System.Windows.Forms.Label ExportSTPowerZonesAsLabel;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.RadioButton PercentMaxRadioButton;
        private System.Windows.Forms.RadioButton BPMRadioButton;
        private System.Windows.Forms.Label ExportSTHRZonesAsLabel;
    }
}

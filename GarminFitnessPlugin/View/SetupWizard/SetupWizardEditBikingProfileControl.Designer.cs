namespace GarminFitnessPlugin.View
{
    partial class SetupWizardEditBikingProfileControl
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.BikeProfileActionBanner = new ZoneFiveSoftware.Common.Visuals.ActionBanner();
            this.WheelSizeGroupBox = new System.Windows.Forms.GroupBox();
            this.AutoWheelSizeCheckBox = new System.Windows.Forms.CheckBox();
            this.WheelSizeLabel = new System.Windows.Forms.Label();
            this.WheelSizeUnitLabel = new System.Windows.Forms.Label();
            this.WheelSizeTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.BikeNameLabel = new System.Windows.Forms.Label();
            this.OdometerUnitsLabel = new System.Windows.Forms.Label();
            this.HasCadenceCheckBox = new System.Windows.Forms.CheckBox();
            this.BikeWeightUnitLabel = new System.Windows.Forms.Label();
            this.HasPowerCheckBox = new System.Windows.Forms.CheckBox();
            this.OdometerTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.BikeNameTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.BikeWeightTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.BikeWeightLabel = new System.Windows.Forms.Label();
            this.OdometerLabel = new System.Windows.Forms.Label();
            this.PowerZonesGroupBox = new System.Windows.Forms.GroupBox();
            this.HighPowerTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.FTPTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.LowPowerTextBox = new GarminFitnessPlugin.View.ExtendedTextBox();
            this.FTPUnitsLabel = new System.Windows.Forms.Label();
            this.FTPLabel = new System.Windows.Forms.Label();
            this.HighPowerLabel = new System.Windows.Forms.Label();
            this.LowPowerLabel = new System.Windows.Forms.Label();
            this.PowerZonesTreeList = new GarminFitnessPlugin.View.ExtendedTreeList();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            this.WheelSizeGroupBox.SuspendLayout();
            this.PowerZonesGroupBox.SuspendLayout();
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
            this.ExplanationLabel.Text = "Some of the data in the profile in biking specific.  This data includes the diffe" +
                "rent power zones as well as the bike profiles.  If you don\'t have a biking compa" +
                "tible unit you can skip this section.";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BikeProfileActionBanner);
            this.panel1.Controls.Add(this.WheelSizeGroupBox);
            this.panel1.Controls.Add(this.BikeNameLabel);
            this.panel1.Controls.Add(this.OdometerUnitsLabel);
            this.panel1.Controls.Add(this.HasCadenceCheckBox);
            this.panel1.Controls.Add(this.BikeWeightUnitLabel);
            this.panel1.Controls.Add(this.HasPowerCheckBox);
            this.panel1.Controls.Add(this.OdometerTextBox);
            this.panel1.Controls.Add(this.BikeNameTextBox);
            this.panel1.Controls.Add(this.BikeWeightTextBox);
            this.panel1.Controls.Add(this.BikeWeightLabel);
            this.panel1.Controls.Add(this.OdometerLabel);
            this.panel1.Location = new System.Drawing.Point(6, 203);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(591, 120);
            this.panel1.TabIndex = 8;
            // 
            // BikeProfileActionBanner
            // 
            this.BikeProfileActionBanner.BackColor = System.Drawing.Color.Transparent;
            this.BikeProfileActionBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.BikeProfileActionBanner.HasMenuButton = true;
            this.BikeProfileActionBanner.Location = new System.Drawing.Point(0, 0);
            this.BikeProfileActionBanner.Name = "BikeProfileActionBanner";
            this.BikeProfileActionBanner.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.BikeProfileActionBanner.Size = new System.Drawing.Size(591, 31);
            this.BikeProfileActionBanner.Style = ZoneFiveSoftware.Common.Visuals.ActionBanner.BannerStyle.Header1;
            this.BikeProfileActionBanner.TabIndex = 5;
            this.BikeProfileActionBanner.Text = "Bike 1 (Name)";
            this.BikeProfileActionBanner.UseStyleFont = true;
            this.BikeProfileActionBanner.MenuClicked += new System.EventHandler(this.BikeProfileActionBanner_MenuClicked);
            // 
            // WheelSizeGroupBox
            // 
            this.WheelSizeGroupBox.Controls.Add(this.AutoWheelSizeCheckBox);
            this.WheelSizeGroupBox.Controls.Add(this.WheelSizeLabel);
            this.WheelSizeGroupBox.Controls.Add(this.WheelSizeUnitLabel);
            this.WheelSizeGroupBox.Controls.Add(this.WheelSizeTextBox);
            this.WheelSizeGroupBox.Location = new System.Drawing.Point(210, 70);
            this.WheelSizeGroupBox.Name = "WheelSizeGroupBox";
            this.WheelSizeGroupBox.Size = new System.Drawing.Size(378, 43);
            this.WheelSizeGroupBox.TabIndex = 9;
            this.WheelSizeGroupBox.TabStop = false;
            this.WheelSizeGroupBox.Text = "Wheel size";
            // 
            // AutoWheelSizeCheckBox
            // 
            this.AutoWheelSizeCheckBox.AutoSize = true;
            this.AutoWheelSizeCheckBox.Location = new System.Drawing.Point(7, 19);
            this.AutoWheelSizeCheckBox.Name = "AutoWheelSizeCheckBox";
            this.AutoWheelSizeCheckBox.Size = new System.Drawing.Size(48, 17);
            this.AutoWheelSizeCheckBox.TabIndex = 0;
            this.AutoWheelSizeCheckBox.Text = "Auto";
            this.AutoWheelSizeCheckBox.UseVisualStyleBackColor = true;
            this.AutoWheelSizeCheckBox.CheckedChanged += new System.EventHandler(this.AutoWheelSizeCheckBox_CheckedChanged);
            // 
            // WheelSizeLabel
            // 
            this.WheelSizeLabel.AutoSize = true;
            this.WheelSizeLabel.Location = new System.Drawing.Point(137, 20);
            this.WheelSizeLabel.Name = "WheelSizeLabel";
            this.WheelSizeLabel.Size = new System.Drawing.Size(65, 13);
            this.WheelSizeLabel.TabIndex = 6;
            this.WheelSizeLabel.Text = "Wheel size :";
            // 
            // WheelSizeUnitLabel
            // 
            this.WheelSizeUnitLabel.AutoSize = true;
            this.WheelSizeUnitLabel.Location = new System.Drawing.Point(316, 20);
            this.WheelSizeUnitLabel.Name = "WheelSizeUnitLabel";
            this.WheelSizeUnitLabel.Size = new System.Drawing.Size(23, 13);
            this.WheelSizeUnitLabel.TabIndex = 8;
            this.WheelSizeUnitLabel.Text = "mm";
            // 
            // WheelSizeTextBox
            // 
            this.WheelSizeTextBox.Location = new System.Drawing.Point(265, 16);
            this.WheelSizeTextBox.MaxLength = 4;
            this.WheelSizeTextBox.Name = "WheelSizeTextBox";
            this.WheelSizeTextBox.Size = new System.Drawing.Size(47, 20);
            this.WheelSizeTextBox.TabIndex = 7;
            this.WheelSizeTextBox.Validated += new System.EventHandler(this.WheelSizeTextBox_Validated);
            this.WheelSizeTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.WheelSizeTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.WheelSizeTextBox_Validating);
            // 
            // BikeNameLabel
            // 
            this.BikeNameLabel.AutoSize = true;
            this.BikeNameLabel.Location = new System.Drawing.Point(9, 44);
            this.BikeNameLabel.Name = "BikeNameLabel";
            this.BikeNameLabel.Size = new System.Drawing.Size(41, 13);
            this.BikeNameLabel.TabIndex = 3;
            this.BikeNameLabel.Text = "Name :";
            this.BikeNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OdometerUnitsLabel
            // 
            this.OdometerUnitsLabel.AutoSize = true;
            this.OdometerUnitsLabel.Location = new System.Drawing.Point(138, 70);
            this.OdometerUnitsLabel.Name = "OdometerUnitsLabel";
            this.OdometerUnitsLabel.Size = new System.Drawing.Size(21, 13);
            this.OdometerUnitsLabel.TabIndex = 8;
            this.OdometerUnitsLabel.Text = "km";
            // 
            // HasCadenceCheckBox
            // 
            this.HasCadenceCheckBox.AutoSize = true;
            this.HasCadenceCheckBox.Location = new System.Drawing.Point(223, 44);
            this.HasCadenceCheckBox.Name = "HasCadenceCheckBox";
            this.HasCadenceCheckBox.Size = new System.Drawing.Size(124, 17);
            this.HasCadenceCheckBox.TabIndex = 0;
            this.HasCadenceCheckBox.Text = "Has cadence sensor";
            this.HasCadenceCheckBox.UseVisualStyleBackColor = true;
            this.HasCadenceCheckBox.CheckedChanged += new System.EventHandler(this.HasCadenceCheckBox_CheckedChanged);
            // 
            // BikeWeightUnitLabel
            // 
            this.BikeWeightUnitLabel.AutoSize = true;
            this.BikeWeightUnitLabel.Location = new System.Drawing.Point(138, 96);
            this.BikeWeightUnitLabel.Name = "BikeWeightUnitLabel";
            this.BikeWeightUnitLabel.Size = new System.Drawing.Size(15, 13);
            this.BikeWeightUnitLabel.TabIndex = 8;
            this.BikeWeightUnitLabel.Text = "lb";
            // 
            // HasPowerCheckBox
            // 
            this.HasPowerCheckBox.AutoSize = true;
            this.HasPowerCheckBox.Location = new System.Drawing.Point(417, 43);
            this.HasPowerCheckBox.Name = "HasPowerCheckBox";
            this.HasPowerCheckBox.Size = new System.Drawing.Size(111, 17);
            this.HasPowerCheckBox.TabIndex = 0;
            this.HasPowerCheckBox.Text = "Has power sensor";
            this.HasPowerCheckBox.UseVisualStyleBackColor = true;
            this.HasPowerCheckBox.CheckedChanged += new System.EventHandler(this.HasPowerCheckBox_CheckedChanged);
            // 
            // OdometerTextBox
            // 
            this.OdometerTextBox.Location = new System.Drawing.Point(68, 67);
            this.OdometerTextBox.Name = "OdometerTextBox";
            this.OdometerTextBox.Size = new System.Drawing.Size(64, 20);
            this.OdometerTextBox.TabIndex = 7;
            this.OdometerTextBox.Validated += new System.EventHandler(this.OdometerTextBox_Validated);
            this.OdometerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.OdometerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.OdometerTextBox_Validating);
            // 
            // BikeNameTextBox
            // 
            this.BikeNameTextBox.Location = new System.Drawing.Point(68, 41);
            this.BikeNameTextBox.MaxLength = 15;
            this.BikeNameTextBox.Name = "BikeNameTextBox";
            this.BikeNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.BikeNameTextBox.TabIndex = 2;
            this.BikeNameTextBox.Validated += new System.EventHandler(this.BikeNameTextBox_Validated);
            this.BikeNameTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            // 
            // BikeWeightTextBox
            // 
            this.BikeWeightTextBox.Location = new System.Drawing.Point(68, 93);
            this.BikeWeightTextBox.Name = "BikeWeightTextBox";
            this.BikeWeightTextBox.Size = new System.Drawing.Size(64, 20);
            this.BikeWeightTextBox.TabIndex = 7;
            this.BikeWeightTextBox.Validated += new System.EventHandler(this.BikeWeightTextBox_Validated);
            this.BikeWeightTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.BikeWeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.BikeWeightTextBox_Validating);
            // 
            // BikeWeightLabel
            // 
            this.BikeWeightLabel.AutoSize = true;
            this.BikeWeightLabel.Location = new System.Drawing.Point(9, 96);
            this.BikeWeightLabel.Name = "BikeWeightLabel";
            this.BikeWeightLabel.Size = new System.Drawing.Size(47, 13);
            this.BikeWeightLabel.TabIndex = 6;
            this.BikeWeightLabel.Text = "Weight :";
            // 
            // OdometerLabel
            // 
            this.OdometerLabel.AutoSize = true;
            this.OdometerLabel.Location = new System.Drawing.Point(9, 70);
            this.OdometerLabel.Name = "OdometerLabel";
            this.OdometerLabel.Size = new System.Drawing.Size(59, 13);
            this.OdometerLabel.TabIndex = 6;
            this.OdometerLabel.Text = "Odometer :";
            // 
            // PowerZonesGroupBox
            // 
            this.PowerZonesGroupBox.Controls.Add(this.HighPowerTextBox);
            this.PowerZonesGroupBox.Controls.Add(this.FTPTextBox);
            this.PowerZonesGroupBox.Controls.Add(this.LowPowerTextBox);
            this.PowerZonesGroupBox.Controls.Add(this.FTPUnitsLabel);
            this.PowerZonesGroupBox.Controls.Add(this.FTPLabel);
            this.PowerZonesGroupBox.Controls.Add(this.HighPowerLabel);
            this.PowerZonesGroupBox.Controls.Add(this.LowPowerLabel);
            this.PowerZonesGroupBox.Controls.Add(this.PowerZonesTreeList);
            this.PowerZonesGroupBox.Location = new System.Drawing.Point(3, 80);
            this.PowerZonesGroupBox.Name = "PowerZonesGroupBox";
            this.PowerZonesGroupBox.Size = new System.Drawing.Size(594, 120);
            this.PowerZonesGroupBox.TabIndex = 7;
            this.PowerZonesGroupBox.TabStop = false;
            this.PowerZonesGroupBox.Text = "Power Zones";
            // 
            // HighPowerTextBox
            // 
            this.HighPowerTextBox.Location = new System.Drawing.Point(396, 66);
            this.HighPowerTextBox.MaxLength = 4;
            this.HighPowerTextBox.Name = "HighPowerTextBox";
            this.HighPowerTextBox.Size = new System.Drawing.Size(129, 20);
            this.HighPowerTextBox.TabIndex = 3;
            this.HighPowerTextBox.Validated += new System.EventHandler(this.HighPowerTextBox_Validated);
            this.HighPowerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.HighPowerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HighPowerTextBox_Validating);
            // 
            // FTPTextBox
            // 
            this.FTPTextBox.Location = new System.Drawing.Point(220, 18);
            this.FTPTextBox.MaxLength = 4;
            this.FTPTextBox.Name = "FTPTextBox";
            this.FTPTextBox.Size = new System.Drawing.Size(46, 20);
            this.FTPTextBox.TabIndex = 0;
            this.FTPTextBox.Validated += new System.EventHandler(this.FTPTextBox_Validated);
            this.FTPTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.FTPTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.FTPTextBox_Validating);
            // 
            // LowPowerTextBox
            // 
            this.LowPowerTextBox.Location = new System.Drawing.Point(396, 40);
            this.LowPowerTextBox.MaxLength = 4;
            this.LowPowerTextBox.Name = "LowPowerTextBox";
            this.LowPowerTextBox.Size = new System.Drawing.Size(129, 20);
            this.LowPowerTextBox.TabIndex = 2;
            this.LowPowerTextBox.Validated += new System.EventHandler(this.LowPowerTextBox_Validated);
            this.LowPowerTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnValidatedKeyDown);
            this.LowPowerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.LowPowerTextBox_Validating);
            // 
            // FTPUnitsLabel
            // 
            this.FTPUnitsLabel.AutoSize = true;
            this.FTPUnitsLabel.Location = new System.Drawing.Point(272, 21);
            this.FTPUnitsLabel.Name = "FTPUnitsLabel";
            this.FTPUnitsLabel.Size = new System.Drawing.Size(32, 13);
            this.FTPUnitsLabel.TabIndex = 8;
            this.FTPUnitsLabel.Text = "watts";
            // 
            // FTPLabel
            // 
            this.FTPLabel.AutoSize = true;
            this.FTPLabel.Location = new System.Drawing.Point(6, 21);
            this.FTPLabel.Name = "FTPLabel";
            this.FTPLabel.Size = new System.Drawing.Size(145, 13);
            this.FTPLabel.TabIndex = 1;
            this.FTPLabel.Text = "Functional Threshold Power :";
            // 
            // HighPowerLabel
            // 
            this.HighPowerLabel.AutoSize = true;
            this.HighPowerLabel.Location = new System.Drawing.Point(339, 69);
            this.HighPowerLabel.Name = "HighPowerLabel";
            this.HighPowerLabel.Size = new System.Drawing.Size(35, 13);
            this.HighPowerLabel.TabIndex = 2;
            this.HighPowerLabel.Text = "High :";
            // 
            // LowPowerLabel
            // 
            this.LowPowerLabel.AutoSize = true;
            this.LowPowerLabel.Location = new System.Drawing.Point(339, 43);
            this.LowPowerLabel.Name = "LowPowerLabel";
            this.LowPowerLabel.Size = new System.Drawing.Size(33, 13);
            this.LowPowerLabel.TabIndex = 1;
            this.LowPowerLabel.Text = "Low :";
            // 
            // PowerZonesTreeList
            // 
            this.PowerZonesTreeList.BackColor = System.Drawing.Color.Transparent;
            this.PowerZonesTreeList.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.PowerZonesTreeList.CheckBoxes = false;
            this.PowerZonesTreeList.DefaultIndent = 15;
            this.PowerZonesTreeList.DefaultRowHeight = -1;
            this.PowerZonesTreeList.DragAutoScrollSize = ((byte)(20));
            this.PowerZonesTreeList.HeaderRowHeight = 21;
            this.PowerZonesTreeList.Location = new System.Drawing.Point(6, 43);
            this.PowerZonesTreeList.MultiSelect = false;
            this.PowerZonesTreeList.Name = "PowerZonesTreeList";
            this.PowerZonesTreeList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.One;
            this.PowerZonesTreeList.NumLockedColumns = 0;
            this.PowerZonesTreeList.RowAlternatingColors = true;
            this.PowerZonesTreeList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(10)))), ((int)(((byte)(36)))), ((int)(((byte)(106)))));
            this.PowerZonesTreeList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.PowerZonesTreeList.RowHotlightMouse = true;
            this.PowerZonesTreeList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.PowerZonesTreeList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.PowerZonesTreeList.RowSeparatorLines = true;
            this.PowerZonesTreeList.ShowLines = false;
            this.PowerZonesTreeList.ShowPlusMinus = false;
            this.PowerZonesTreeList.Size = new System.Drawing.Size(298, 72);
            this.PowerZonesTreeList.TabIndex = 1;
            this.PowerZonesTreeList.SelectedChanged += new System.EventHandler(this.PowerZonesTreeList_SelectedChanged);
            // 
            // SetupWizardEditBikingProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.PowerZonesGroupBox);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.ExplanationLabel);
            this.Name = "SetupWizardEditBikingProfileControl";
            this.Size = new System.Drawing.Size(600, 329);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.WheelSizeGroupBox.ResumeLayout(false);
            this.WheelSizeGroupBox.PerformLayout();
            this.PowerZonesGroupBox.ResumeLayout(false);
            this.PowerZonesGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label ExplanationLabel;
        private System.Windows.Forms.Panel panel1;
        private ZoneFiveSoftware.Common.Visuals.ActionBanner BikeProfileActionBanner;
        private System.Windows.Forms.GroupBox WheelSizeGroupBox;
        private System.Windows.Forms.CheckBox AutoWheelSizeCheckBox;
        private System.Windows.Forms.Label WheelSizeLabel;
        private System.Windows.Forms.Label WheelSizeUnitLabel;
        private GarminFitnessPlugin.View.ExtendedTextBox WheelSizeTextBox;
        private System.Windows.Forms.Label BikeNameLabel;
        private System.Windows.Forms.Label OdometerUnitsLabel;
        private System.Windows.Forms.CheckBox HasCadenceCheckBox;
        private System.Windows.Forms.Label BikeWeightUnitLabel;
        private System.Windows.Forms.CheckBox HasPowerCheckBox;
        private GarminFitnessPlugin.View.ExtendedTextBox OdometerTextBox;
        private GarminFitnessPlugin.View.ExtendedTextBox BikeNameTextBox;
        private GarminFitnessPlugin.View.ExtendedTextBox BikeWeightTextBox;
        private System.Windows.Forms.Label BikeWeightLabel;
        private System.Windows.Forms.Label OdometerLabel;
        private System.Windows.Forms.GroupBox PowerZonesGroupBox;
        private GarminFitnessPlugin.View.ExtendedTextBox HighPowerTextBox;
        private GarminFitnessPlugin.View.ExtendedTextBox FTPTextBox;
        private GarminFitnessPlugin.View.ExtendedTextBox LowPowerTextBox;
        private System.Windows.Forms.Label FTPUnitsLabel;
        private System.Windows.Forms.Label FTPLabel;
        private System.Windows.Forms.Label HighPowerLabel;
        private System.Windows.Forms.Label LowPowerLabel;
        private ExtendedTreeList PowerZonesTreeList;
    }
}

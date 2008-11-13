namespace GarminFitnessPlugin.View
{
    partial class GarminProfileControl
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
            this.ActivitiesPanel = new System.Windows.Forms.Panel();
            this.ActivityPanel = new System.Windows.Forms.Panel();
            this.GarminActivityBanner = new ZoneFiveSoftware.Common.Visuals.ActionBanner();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.BirthDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.RestBPMLabel = new System.Windows.Forms.Label();
            this.WeightUnitLabel = new System.Windows.Forms.Label();
            this.RestHRTextBox = new System.Windows.Forms.TextBox();
            this.WeightTextBox = new System.Windows.Forms.TextBox();
            this.FemaleRadioButton = new System.Windows.Forms.RadioButton();
            this.MaleRadioButton = new System.Windows.Forms.RadioButton();
            this.BirthDateLabel = new System.Windows.Forms.Label();
            this.RestingHeartRateLabel = new System.Windows.Forms.Label();
            this.GenderLabel = new System.Windows.Forms.Label();
            this.WeightLabel = new System.Windows.Forms.Label();
            this.ProfileNameLabel = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.MaxHRLabel = new System.Windows.Forms.Label();
            this.MaxHRTextBox = new System.Windows.Forms.TextBox();
            this.MaxHRBPMLabel = new System.Windows.Forms.Label();
            this.GearWeightLabel = new System.Windows.Forms.Label();
            this.GearWeightTextBox = new System.Windows.Forms.TextBox();
            this.GearWeightUnitLabel = new System.Windows.Forms.Label();
            this.HRZonesGroupBox = new System.Windows.Forms.GroupBox();
            this.LowHRLabel = new System.Windows.Forms.Label();
            this.HighHRLabel = new System.Windows.Forms.Label();
            this.LowHRTextBox = new System.Windows.Forms.TextBox();
            this.HighHRTextBox = new System.Windows.Forms.TextBox();
            this.BPMRadioButton = new System.Windows.Forms.RadioButton();
            this.PercentMaxRadioButton = new System.Windows.Forms.RadioButton();
            this.SpeedZonesGroupBox = new System.Windows.Forms.GroupBox();
            this.PaceRadioButton = new System.Windows.Forms.RadioButton();
            this.SpeedRadioButton = new System.Windows.Forms.RadioButton();
            this.HighSpeedTextBox = new System.Windows.Forms.TextBox();
            this.LowSpeedTextBox = new System.Windows.Forms.TextBox();
            this.HighSpeedLabel = new System.Windows.Forms.Label();
            this.LowSpeedLabel = new System.Windows.Forms.Label();
            this.PowerZonesGroupBox = new System.Windows.Forms.GroupBox();
            this.HighPowerTextBox = new System.Windows.Forms.TextBox();
            this.LowPowerTextBox = new System.Windows.Forms.TextBox();
            this.HighPowerLabel = new System.Windows.Forms.Label();
            this.LowPowerLabel = new System.Windows.Forms.Label();
            this.NameSpeedLabel = new System.Windows.Forms.Label();
            this.SpeedNameTextBox = new System.Windows.Forms.TextBox();
            this.PowerZonesTreeList = new GarminFitnessPlugin.View.ExtendedTreeList();
            this.SpeedZonesTreeList = new GarminFitnessPlugin.View.ExtendedTreeList();
            this.HRZonesTreeList = new GarminFitnessPlugin.View.ExtendedTreeList();
            this.ActivitiesPanel.SuspendLayout();
            this.ActivityPanel.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.HRZonesGroupBox.SuspendLayout();
            this.SpeedZonesGroupBox.SuspendLayout();
            this.PowerZonesGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ActivitiesPanel
            // 
            this.ActivitiesPanel.Controls.Add(this.ActivityPanel);
            this.ActivitiesPanel.Controls.Add(this.GarminActivityBanner);
            this.ActivitiesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivitiesPanel.Location = new System.Drawing.Point(0, 0);
            this.ActivitiesPanel.Name = "ActivitiesPanel";
            this.ActivitiesPanel.Size = new System.Drawing.Size(500, 512);
            this.ActivitiesPanel.TabIndex = 1;
            // 
            // ActivityPanel
            // 
            this.ActivityPanel.Controls.Add(this.PowerZonesGroupBox);
            this.ActivityPanel.Controls.Add(this.SpeedZonesGroupBox);
            this.ActivityPanel.Controls.Add(this.HRZonesGroupBox);
            this.ActivityPanel.Controls.Add(this.MaxHRLabel);
            this.ActivityPanel.Controls.Add(this.MaxHRBPMLabel);
            this.ActivityPanel.Controls.Add(this.GearWeightUnitLabel);
            this.ActivityPanel.Controls.Add(this.MaxHRTextBox);
            this.ActivityPanel.Controls.Add(this.GearWeightTextBox);
            this.ActivityPanel.Controls.Add(this.GearWeightLabel);
            this.ActivityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivityPanel.Location = new System.Drawing.Point(0, 31);
            this.ActivityPanel.Name = "ActivityPanel";
            this.ActivityPanel.Size = new System.Drawing.Size(500, 481);
            this.ActivityPanel.TabIndex = 1;
            // 
            // GarminActivityBanner
            // 
            this.GarminActivityBanner.BackColor = System.Drawing.Color.Transparent;
            this.GarminActivityBanner.Dock = System.Windows.Forms.DockStyle.Top;
            this.GarminActivityBanner.HasMenuButton = true;
            this.GarminActivityBanner.Location = new System.Drawing.Point(0, 0);
            this.GarminActivityBanner.Name = "GarminActivityBanner";
            this.GarminActivityBanner.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.GarminActivityBanner.Size = new System.Drawing.Size(500, 31);
            this.GarminActivityBanner.Style = ZoneFiveSoftware.Common.Visuals.ActionBanner.BannerStyle.Header1;
            this.GarminActivityBanner.TabIndex = 0;
            this.GarminActivityBanner.Text = "Running";
            this.GarminActivityBanner.UseStyleFont = true;
            this.GarminActivityBanner.MenuClicked += new System.EventHandler(this.GarminActivityBanner_MenuClicked);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.BirthDateTimePicker);
            this.splitContainer1.Panel1.Controls.Add(this.RestBPMLabel);
            this.splitContainer1.Panel1.Controls.Add(this.WeightUnitLabel);
            this.splitContainer1.Panel1.Controls.Add(this.RestHRTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.WeightTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.FemaleRadioButton);
            this.splitContainer1.Panel1.Controls.Add(this.MaleRadioButton);
            this.splitContainer1.Panel1.Controls.Add(this.BirthDateLabel);
            this.splitContainer1.Panel1.Controls.Add(this.RestingHeartRateLabel);
            this.splitContainer1.Panel1.Controls.Add(this.GenderLabel);
            this.splitContainer1.Panel1.Controls.Add(this.WeightLabel);
            this.splitContainer1.Panel1.Controls.Add(this.ProfileNameLabel);
            this.splitContainer1.Panel1.Controls.Add(this.NameTextBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.ActivitiesPanel);
            this.splitContainer1.Size = new System.Drawing.Size(500, 600);
            this.splitContainer1.SplitterDistance = 84;
            this.splitContainer1.TabIndex = 2;
            // 
            // BirthDateTimePicker
            // 
            this.BirthDateTimePicker.Location = new System.Drawing.Point(359, 27);
            this.BirthDateTimePicker.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.BirthDateTimePicker.Name = "BirthDateTimePicker";
            this.BirthDateTimePicker.Size = new System.Drawing.Size(138, 20);
            this.BirthDateTimePicker.TabIndex = 4;
            this.BirthDateTimePicker.Validated += new System.EventHandler(this.BirthDateTimePicker_Validated);
            // 
            // RestBPMLabel
            // 
            this.RestBPMLabel.AutoSize = true;
            this.RestBPMLabel.Location = new System.Drawing.Point(467, 58);
            this.RestBPMLabel.Name = "RestBPMLabel";
            this.RestBPMLabel.Size = new System.Drawing.Size(30, 13);
            this.RestBPMLabel.TabIndex = 5;
            this.RestBPMLabel.Text = "BPM";
            // 
            // WeightUnitLabel
            // 
            this.WeightUnitLabel.AutoSize = true;
            this.WeightUnitLabel.Location = new System.Drawing.Point(133, 58);
            this.WeightUnitLabel.Name = "WeightUnitLabel";
            this.WeightUnitLabel.Size = new System.Drawing.Size(15, 13);
            this.WeightUnitLabel.TabIndex = 5;
            this.WeightUnitLabel.Text = "lb";
            // 
            // RestHRTextBox
            // 
            this.RestHRTextBox.Location = new System.Drawing.Point(417, 55);
            this.RestHRTextBox.MaxLength = 3;
            this.RestHRTextBox.Name = "RestHRTextBox";
            this.RestHRTextBox.Size = new System.Drawing.Size(44, 20);
            this.RestHRTextBox.TabIndex = 5;
            this.RestHRTextBox.Validated += new System.EventHandler(this.RestHRTextBox_Validated);
            this.RestHRTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.RestHRTextBox_Validating);
            // 
            // WeightTextBox
            // 
            this.WeightTextBox.Location = new System.Drawing.Point(63, 55);
            this.WeightTextBox.Name = "WeightTextBox";
            this.WeightTextBox.Size = new System.Drawing.Size(64, 20);
            this.WeightTextBox.TabIndex = 3;
            this.WeightTextBox.Validated += new System.EventHandler(this.WeightTextBox_Validated);
            this.WeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.WeightTextBox_Validating);
            // 
            // FemaleRadioButton
            // 
            this.FemaleRadioButton.AutoSize = true;
            this.FemaleRadioButton.Location = new System.Drawing.Point(136, 31);
            this.FemaleRadioButton.Name = "FemaleRadioButton";
            this.FemaleRadioButton.Size = new System.Drawing.Size(59, 17);
            this.FemaleRadioButton.TabIndex = 2;
            this.FemaleRadioButton.TabStop = true;
            this.FemaleRadioButton.Text = "Female";
            this.FemaleRadioButton.UseVisualStyleBackColor = true;
            this.FemaleRadioButton.CheckedChanged += new System.EventHandler(this.FemaleRadioButton_CheckedChanged);
            // 
            // MaleRadioButton
            // 
            this.MaleRadioButton.AutoSize = true;
            this.MaleRadioButton.Location = new System.Drawing.Point(63, 31);
            this.MaleRadioButton.Name = "MaleRadioButton";
            this.MaleRadioButton.Size = new System.Drawing.Size(48, 17);
            this.MaleRadioButton.TabIndex = 1;
            this.MaleRadioButton.TabStop = true;
            this.MaleRadioButton.Text = "Male";
            this.MaleRadioButton.UseVisualStyleBackColor = true;
            this.MaleRadioButton.CheckedChanged += new System.EventHandler(this.MaleRadioButton_CheckedChanged);
            // 
            // BirthDateLabel
            // 
            this.BirthDateLabel.AutoSize = true;
            this.BirthDateLabel.Location = new System.Drawing.Point(283, 31);
            this.BirthDateLabel.Name = "BirthDateLabel";
            this.BirthDateLabel.Size = new System.Drawing.Size(60, 13);
            this.BirthDateLabel.TabIndex = 2;
            this.BirthDateLabel.Text = "Birth Date :";
            // 
            // RestingHeartRateLabel
            // 
            this.RestingHeartRateLabel.AutoSize = true;
            this.RestingHeartRateLabel.Location = new System.Drawing.Point(283, 58);
            this.RestingHeartRateLabel.Name = "RestingHeartRateLabel";
            this.RestingHeartRateLabel.Size = new System.Drawing.Size(104, 13);
            this.RestingHeartRateLabel.TabIndex = 2;
            this.RestingHeartRateLabel.Text = "Resting Heart Rate :";
            // 
            // GenderLabel
            // 
            this.GenderLabel.AutoSize = true;
            this.GenderLabel.Location = new System.Drawing.Point(4, 31);
            this.GenderLabel.Name = "GenderLabel";
            this.GenderLabel.Size = new System.Drawing.Size(48, 13);
            this.GenderLabel.TabIndex = 2;
            this.GenderLabel.Text = "Gender :";
            // 
            // WeightLabel
            // 
            this.WeightLabel.AutoSize = true;
            this.WeightLabel.Location = new System.Drawing.Point(4, 58);
            this.WeightLabel.Name = "WeightLabel";
            this.WeightLabel.Size = new System.Drawing.Size(47, 13);
            this.WeightLabel.TabIndex = 2;
            this.WeightLabel.Text = "Weight :";
            // 
            // ProfileNameLabel
            // 
            this.ProfileNameLabel.AutoSize = true;
            this.ProfileNameLabel.Location = new System.Drawing.Point(4, 7);
            this.ProfileNameLabel.Name = "ProfileNameLabel";
            this.ProfileNameLabel.Size = new System.Drawing.Size(41, 13);
            this.ProfileNameLabel.TabIndex = 1;
            this.ProfileNameLabel.Text = "Name :";
            this.ProfileNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(63, 4);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(434, 20);
            this.NameTextBox.TabIndex = 0;
            this.NameTextBox.Validated += new System.EventHandler(this.NameTextBox_Validated);
            // 
            // MaxHRLabel
            // 
            this.MaxHRLabel.AutoSize = true;
            this.MaxHRLabel.Location = new System.Drawing.Point(7, 7);
            this.MaxHRLabel.Name = "MaxHRLabel";
            this.MaxHRLabel.Size = new System.Drawing.Size(112, 13);
            this.MaxHRLabel.TabIndex = 1;
            this.MaxHRLabel.Text = "Maximum Heart Rate :";
            // 
            // MaxHRTextBox
            // 
            this.MaxHRTextBox.Location = new System.Drawing.Point(125, 4);
            this.MaxHRTextBox.MaxLength = 3;
            this.MaxHRTextBox.Name = "MaxHRTextBox";
            this.MaxHRTextBox.Size = new System.Drawing.Size(44, 20);
            this.MaxHRTextBox.TabIndex = 0;
            this.MaxHRTextBox.Validated += new System.EventHandler(this.MaxHRTextBox_Validated);
            this.MaxHRTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.MaxHRTextBox_Validating);
            // 
            // MaxHRBPMLabel
            // 
            this.MaxHRBPMLabel.AutoSize = true;
            this.MaxHRBPMLabel.Location = new System.Drawing.Point(175, 7);
            this.MaxHRBPMLabel.Name = "MaxHRBPMLabel";
            this.MaxHRBPMLabel.Size = new System.Drawing.Size(30, 13);
            this.MaxHRBPMLabel.TabIndex = 5;
            this.MaxHRBPMLabel.Text = "BPM";
            // 
            // GearWeightLabel
            // 
            this.GearWeightLabel.AutoSize = true;
            this.GearWeightLabel.Location = new System.Drawing.Point(283, 7);
            this.GearWeightLabel.Name = "GearWeightLabel";
            this.GearWeightLabel.Size = new System.Drawing.Size(73, 13);
            this.GearWeightLabel.TabIndex = 2;
            this.GearWeightLabel.Text = "Gear Weight :";
            // 
            // GearWeightTextBox
            // 
            this.GearWeightTextBox.Location = new System.Drawing.Point(384, 4);
            this.GearWeightTextBox.Name = "GearWeightTextBox";
            this.GearWeightTextBox.Size = new System.Drawing.Size(64, 20);
            this.GearWeightTextBox.TabIndex = 1;
            this.GearWeightTextBox.Validated += new System.EventHandler(this.GearWeightTextBox_Validated);
            this.GearWeightTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.GearWeightTextBox_Validating);
            // 
            // GearWeightUnitLabel
            // 
            this.GearWeightUnitLabel.AutoSize = true;
            this.GearWeightUnitLabel.Location = new System.Drawing.Point(454, 7);
            this.GearWeightUnitLabel.Name = "GearWeightUnitLabel";
            this.GearWeightUnitLabel.Size = new System.Drawing.Size(15, 13);
            this.GearWeightUnitLabel.TabIndex = 5;
            this.GearWeightUnitLabel.Text = "lb";
            // 
            // HRZonesGroupBox
            // 
            this.HRZonesGroupBox.Controls.Add(this.PercentMaxRadioButton);
            this.HRZonesGroupBox.Controls.Add(this.BPMRadioButton);
            this.HRZonesGroupBox.Controls.Add(this.HighHRTextBox);
            this.HRZonesGroupBox.Controls.Add(this.LowHRTextBox);
            this.HRZonesGroupBox.Controls.Add(this.HighHRLabel);
            this.HRZonesGroupBox.Controls.Add(this.LowHRLabel);
            this.HRZonesGroupBox.Controls.Add(this.HRZonesTreeList);
            this.HRZonesGroupBox.Location = new System.Drawing.Point(10, 39);
            this.HRZonesGroupBox.Name = "HRZonesGroupBox";
            this.HRZonesGroupBox.Size = new System.Drawing.Size(478, 137);
            this.HRZonesGroupBox.TabIndex = 2;
            this.HRZonesGroupBox.TabStop = false;
            this.HRZonesGroupBox.Text = "Heart Rate Zones";
            // 
            // LowHRLabel
            // 
            this.LowHRLabel.AutoSize = true;
            this.LowHRLabel.Location = new System.Drawing.Point(311, 45);
            this.LowHRLabel.Name = "LowHRLabel";
            this.LowHRLabel.Size = new System.Drawing.Size(33, 13);
            this.LowHRLabel.TabIndex = 1;
            this.LowHRLabel.Text = "Low :";
            // 
            // HighHRLabel
            // 
            this.HighHRLabel.AutoSize = true;
            this.HighHRLabel.Location = new System.Drawing.Point(311, 71);
            this.HighHRLabel.Name = "HighHRLabel";
            this.HighHRLabel.Size = new System.Drawing.Size(35, 13);
            this.HighHRLabel.TabIndex = 2;
            this.HighHRLabel.Text = "High :";
            // 
            // LowHRTextBox
            // 
            this.LowHRTextBox.Location = new System.Drawing.Point(368, 42);
            this.LowHRTextBox.Name = "LowHRTextBox";
            this.LowHRTextBox.Size = new System.Drawing.Size(104, 20);
            this.LowHRTextBox.TabIndex = 2;
            this.LowHRTextBox.Validated += new System.EventHandler(this.LowHRTextBox_Validated);
            this.LowHRTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.LowHRTextBox_Validating);
            // 
            // HighHRTextBox
            // 
            this.HighHRTextBox.Location = new System.Drawing.Point(368, 68);
            this.HighHRTextBox.Name = "HighHRTextBox";
            this.HighHRTextBox.Size = new System.Drawing.Size(104, 20);
            this.HighHRTextBox.TabIndex = 3;
            this.HighHRTextBox.Validated += new System.EventHandler(this.HighHRTextBox_Validated);
            this.HighHRTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HighHRTextBox_Validating);
            // 
            // BPMRadioButton
            // 
            this.BPMRadioButton.AutoSize = true;
            this.BPMRadioButton.Location = new System.Drawing.Point(11, 19);
            this.BPMRadioButton.Name = "BPMRadioButton";
            this.BPMRadioButton.Size = new System.Drawing.Size(48, 17);
            this.BPMRadioButton.TabIndex = 0;
            this.BPMRadioButton.TabStop = true;
            this.BPMRadioButton.Text = "BPM";
            this.BPMRadioButton.UseVisualStyleBackColor = true;
            this.BPMRadioButton.CheckedChanged += new System.EventHandler(this.BPMRadioButton_CheckedChanged);
            // 
            // PercentMaxRadioButton
            // 
            this.PercentMaxRadioButton.AutoSize = true;
            this.PercentMaxRadioButton.Location = new System.Drawing.Point(120, 19);
            this.PercentMaxRadioButton.Name = "PercentMaxRadioButton";
            this.PercentMaxRadioButton.Size = new System.Drawing.Size(75, 17);
            this.PercentMaxRadioButton.TabIndex = 5;
            this.PercentMaxRadioButton.TabStop = true;
            this.PercentMaxRadioButton.Text = "% Max HR";
            this.PercentMaxRadioButton.UseVisualStyleBackColor = true;
            this.PercentMaxRadioButton.CheckedChanged += new System.EventHandler(this.PercentMaxRadioButton_CheckedChanged);
            // 
            // SpeedZonesGroupBox
            // 
            this.SpeedZonesGroupBox.Controls.Add(this.PaceRadioButton);
            this.SpeedZonesGroupBox.Controls.Add(this.SpeedRadioButton);
            this.SpeedZonesGroupBox.Controls.Add(this.SpeedNameTextBox);
            this.SpeedZonesGroupBox.Controls.Add(this.HighSpeedTextBox);
            this.SpeedZonesGroupBox.Controls.Add(this.NameSpeedLabel);
            this.SpeedZonesGroupBox.Controls.Add(this.LowSpeedTextBox);
            this.SpeedZonesGroupBox.Controls.Add(this.HighSpeedLabel);
            this.SpeedZonesGroupBox.Controls.Add(this.LowSpeedLabel);
            this.SpeedZonesGroupBox.Controls.Add(this.SpeedZonesTreeList);
            this.SpeedZonesGroupBox.Location = new System.Drawing.Point(10, 191);
            this.SpeedZonesGroupBox.Name = "SpeedZonesGroupBox";
            this.SpeedZonesGroupBox.Size = new System.Drawing.Size(478, 137);
            this.SpeedZonesGroupBox.TabIndex = 3;
            this.SpeedZonesGroupBox.TabStop = false;
            this.SpeedZonesGroupBox.Text = "Speed Zones";
            // 
            // PaceRadioButton
            // 
            this.PaceRadioButton.AutoSize = true;
            this.PaceRadioButton.Location = new System.Drawing.Point(120, 19);
            this.PaceRadioButton.Name = "PaceRadioButton";
            this.PaceRadioButton.Size = new System.Drawing.Size(50, 17);
            this.PaceRadioButton.TabIndex = 5;
            this.PaceRadioButton.TabStop = true;
            this.PaceRadioButton.Text = "Pace";
            this.PaceRadioButton.UseVisualStyleBackColor = true;
            this.PaceRadioButton.CheckedChanged += new System.EventHandler(this.PaceRadioButton_CheckedChanged);
            // 
            // SpeedRadioButton
            // 
            this.SpeedRadioButton.AutoSize = true;
            this.SpeedRadioButton.Location = new System.Drawing.Point(11, 19);
            this.SpeedRadioButton.Name = "SpeedRadioButton";
            this.SpeedRadioButton.Size = new System.Drawing.Size(56, 17);
            this.SpeedRadioButton.TabIndex = 0;
            this.SpeedRadioButton.TabStop = true;
            this.SpeedRadioButton.Text = "Speed";
            this.SpeedRadioButton.UseVisualStyleBackColor = true;
            this.SpeedRadioButton.CheckedChanged += new System.EventHandler(this.SpeedRadioButton_CheckedChanged);
            // 
            // HighSpeedTextBox
            // 
            this.HighSpeedTextBox.Location = new System.Drawing.Point(368, 68);
            this.HighSpeedTextBox.Name = "HighSpeedTextBox";
            this.HighSpeedTextBox.Size = new System.Drawing.Size(104, 20);
            this.HighSpeedTextBox.TabIndex = 3;
            this.HighSpeedTextBox.Validated += new System.EventHandler(this.HighSpeedTextBox_Validated);
            this.HighSpeedTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HighSpeedTextBox_Validating);
            // 
            // LowSpeedTextBox
            // 
            this.LowSpeedTextBox.Location = new System.Drawing.Point(368, 42);
            this.LowSpeedTextBox.Name = "LowSpeedTextBox";
            this.LowSpeedTextBox.Size = new System.Drawing.Size(104, 20);
            this.LowSpeedTextBox.TabIndex = 2;
            this.LowSpeedTextBox.Validated += new System.EventHandler(this.LowSpeedTextBox_Validated);
            this.LowSpeedTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.LowSpeedTextBox_Validating);
            // 
            // HighSpeedLabel
            // 
            this.HighSpeedLabel.AutoSize = true;
            this.HighSpeedLabel.Location = new System.Drawing.Point(311, 71);
            this.HighSpeedLabel.Name = "HighSpeedLabel";
            this.HighSpeedLabel.Size = new System.Drawing.Size(35, 13);
            this.HighSpeedLabel.TabIndex = 2;
            this.HighSpeedLabel.Text = "High :";
            // 
            // LowSpeedLabel
            // 
            this.LowSpeedLabel.AutoSize = true;
            this.LowSpeedLabel.Location = new System.Drawing.Point(311, 45);
            this.LowSpeedLabel.Name = "LowSpeedLabel";
            this.LowSpeedLabel.Size = new System.Drawing.Size(33, 13);
            this.LowSpeedLabel.TabIndex = 1;
            this.LowSpeedLabel.Text = "Low :";
            // 
            // PowerZonesGroupBox
            // 
            this.PowerZonesGroupBox.Controls.Add(this.HighPowerTextBox);
            this.PowerZonesGroupBox.Controls.Add(this.LowPowerTextBox);
            this.PowerZonesGroupBox.Controls.Add(this.HighPowerLabel);
            this.PowerZonesGroupBox.Controls.Add(this.LowPowerLabel);
            this.PowerZonesGroupBox.Controls.Add(this.PowerZonesTreeList);
            this.PowerZonesGroupBox.Location = new System.Drawing.Point(10, 341);
            this.PowerZonesGroupBox.Name = "PowerZonesGroupBox";
            this.PowerZonesGroupBox.Size = new System.Drawing.Size(478, 116);
            this.PowerZonesGroupBox.TabIndex = 4;
            this.PowerZonesGroupBox.TabStop = false;
            this.PowerZonesGroupBox.Text = "Power Zones";
            // 
            // HighPowerTextBox
            // 
            this.HighPowerTextBox.Location = new System.Drawing.Point(368, 45);
            this.HighPowerTextBox.Name = "HighPowerTextBox";
            this.HighPowerTextBox.Size = new System.Drawing.Size(104, 20);
            this.HighPowerTextBox.TabIndex = 2;
            this.HighPowerTextBox.Validated += new System.EventHandler(this.HighPowerTextBox_Validated);
            this.HighPowerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.HighPowerTextBox_Validating);
            // 
            // LowPowerTextBox
            // 
            this.LowPowerTextBox.Location = new System.Drawing.Point(368, 19);
            this.LowPowerTextBox.Name = "LowPowerTextBox";
            this.LowPowerTextBox.Size = new System.Drawing.Size(104, 20);
            this.LowPowerTextBox.TabIndex = 1;
            this.LowPowerTextBox.Validated += new System.EventHandler(this.LowPowerTextBox_Validated);
            this.LowPowerTextBox.Validating += new System.ComponentModel.CancelEventHandler(this.LowPowerTextBox_Validating);
            // 
            // HighPowerLabel
            // 
            this.HighPowerLabel.AutoSize = true;
            this.HighPowerLabel.Location = new System.Drawing.Point(311, 48);
            this.HighPowerLabel.Name = "HighPowerLabel";
            this.HighPowerLabel.Size = new System.Drawing.Size(35, 13);
            this.HighPowerLabel.TabIndex = 2;
            this.HighPowerLabel.Text = "High :";
            // 
            // LowPowerLabel
            // 
            this.LowPowerLabel.AutoSize = true;
            this.LowPowerLabel.Location = new System.Drawing.Point(311, 22);
            this.LowPowerLabel.Name = "LowPowerLabel";
            this.LowPowerLabel.Size = new System.Drawing.Size(33, 13);
            this.LowPowerLabel.TabIndex = 1;
            this.LowPowerLabel.Text = "Low :";
            // 
            // NameSpeedLabel
            // 
            this.NameSpeedLabel.AutoSize = true;
            this.NameSpeedLabel.Location = new System.Drawing.Point(311, 97);
            this.NameSpeedLabel.Name = "NameSpeedLabel";
            this.NameSpeedLabel.Size = new System.Drawing.Size(41, 13);
            this.NameSpeedLabel.TabIndex = 2;
            this.NameSpeedLabel.Text = "Name :";
            // 
            // SpeedNameTextBox
            // 
            this.SpeedNameTextBox.Location = new System.Drawing.Point(368, 94);
            this.SpeedNameTextBox.MaxLength = 15;
            this.SpeedNameTextBox.Name = "SpeedNameTextBox";
            this.SpeedNameTextBox.Size = new System.Drawing.Size(104, 20);
            this.SpeedNameTextBox.TabIndex = 4;
            this.SpeedNameTextBox.Validated += new System.EventHandler(this.SpeedNameTextBox_Validated);
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
            this.PowerZonesTreeList.Location = new System.Drawing.Point(6, 19);
            this.PowerZonesTreeList.MultiSelect = false;
            this.PowerZonesTreeList.Name = "PowerZonesTreeList";
            this.PowerZonesTreeList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
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
            this.PowerZonesTreeList.Size = new System.Drawing.Size(298, 89);
            this.PowerZonesTreeList.TabIndex = 0;
            this.PowerZonesTreeList.SelectedChanged += new System.EventHandler(this.PowerZonesTreeList_SelectedChanged);
            // 
            // SpeedZonesTreeList
            // 
            this.SpeedZonesTreeList.BackColor = System.Drawing.Color.Transparent;
            this.SpeedZonesTreeList.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.SpeedZonesTreeList.CheckBoxes = false;
            this.SpeedZonesTreeList.DefaultIndent = 15;
            this.SpeedZonesTreeList.DefaultRowHeight = -1;
            this.SpeedZonesTreeList.DragAutoScrollSize = ((byte)(20));
            this.SpeedZonesTreeList.HeaderRowHeight = 21;
            this.SpeedZonesTreeList.Location = new System.Drawing.Point(6, 42);
            this.SpeedZonesTreeList.MultiSelect = false;
            this.SpeedZonesTreeList.Name = "SpeedZonesTreeList";
            this.SpeedZonesTreeList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
            this.SpeedZonesTreeList.NumLockedColumns = 0;
            this.SpeedZonesTreeList.RowAlternatingColors = true;
            this.SpeedZonesTreeList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(10)))), ((int)(((byte)(36)))), ((int)(((byte)(106)))));
            this.SpeedZonesTreeList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.SpeedZonesTreeList.RowHotlightMouse = true;
            this.SpeedZonesTreeList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.SpeedZonesTreeList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.SpeedZonesTreeList.RowSeparatorLines = true;
            this.SpeedZonesTreeList.ShowLines = false;
            this.SpeedZonesTreeList.ShowPlusMinus = false;
            this.SpeedZonesTreeList.Size = new System.Drawing.Size(298, 89);
            this.SpeedZonesTreeList.TabIndex = 1;
            this.SpeedZonesTreeList.SelectedChanged += new System.EventHandler(this.SpeedZonesTreeList_SelectedChanged);
            // 
            // HRZonesTreeList
            // 
            this.HRZonesTreeList.BackColor = System.Drawing.Color.Transparent;
            this.HRZonesTreeList.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.HRZonesTreeList.CheckBoxes = false;
            this.HRZonesTreeList.DefaultIndent = 15;
            this.HRZonesTreeList.DefaultRowHeight = -1;
            this.HRZonesTreeList.DragAutoScrollSize = ((byte)(20));
            this.HRZonesTreeList.HeaderRowHeight = 21;
            this.HRZonesTreeList.Location = new System.Drawing.Point(6, 42);
            this.HRZonesTreeList.MultiSelect = false;
            this.HRZonesTreeList.Name = "HRZonesTreeList";
            this.HRZonesTreeList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
            this.HRZonesTreeList.NumLockedColumns = 0;
            this.HRZonesTreeList.RowAlternatingColors = true;
            this.HRZonesTreeList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(10)))), ((int)(((byte)(36)))), ((int)(((byte)(106)))));
            this.HRZonesTreeList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.HRZonesTreeList.RowHotlightMouse = true;
            this.HRZonesTreeList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.HRZonesTreeList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.HRZonesTreeList.RowSeparatorLines = true;
            this.HRZonesTreeList.ShowLines = false;
            this.HRZonesTreeList.ShowPlusMinus = false;
            this.HRZonesTreeList.Size = new System.Drawing.Size(298, 89);
            this.HRZonesTreeList.TabIndex = 1;
            this.HRZonesTreeList.SelectedChanged += new System.EventHandler(this.HRZonesTreeList_SelectedChanged);
            // 
            // GarminProfileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.DoubleBuffered = true;
            this.MaximumSize = new System.Drawing.Size(500, 600);
            this.Name = "GarminProfileControl";
            this.Size = new System.Drawing.Size(500, 600);
            this.ActivitiesPanel.ResumeLayout(false);
            this.ActivityPanel.ResumeLayout(false);
            this.ActivityPanel.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.HRZonesGroupBox.ResumeLayout(false);
            this.HRZonesGroupBox.PerformLayout();
            this.SpeedZonesGroupBox.ResumeLayout(false);
            this.SpeedZonesGroupBox.PerformLayout();
            this.PowerZonesGroupBox.ResumeLayout(false);
            this.PowerZonesGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel ActivitiesPanel;
        private System.Windows.Forms.Panel ActivityPanel;
        private ZoneFiveSoftware.Common.Visuals.ActionBanner GarminActivityBanner;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label ProfileNameLabel;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.Label WeightLabel;
        private System.Windows.Forms.Label GenderLabel;
        private System.Windows.Forms.RadioButton FemaleRadioButton;
        private System.Windows.Forms.RadioButton MaleRadioButton;
        private System.Windows.Forms.Label WeightUnitLabel;
        private System.Windows.Forms.TextBox WeightTextBox;
        private System.Windows.Forms.Label RestBPMLabel;
        private System.Windows.Forms.TextBox RestHRTextBox;
        private System.Windows.Forms.Label RestingHeartRateLabel;
        private System.Windows.Forms.Label BirthDateLabel;
        private System.Windows.Forms.DateTimePicker BirthDateTimePicker;
        private ExtendedTreeList HRZonesTreeList;
        private System.Windows.Forms.Label MaxHRLabel;
        private System.Windows.Forms.Label MaxHRBPMLabel;
        private System.Windows.Forms.TextBox MaxHRTextBox;
        private System.Windows.Forms.Label GearWeightUnitLabel;
        private System.Windows.Forms.TextBox GearWeightTextBox;
        private System.Windows.Forms.Label GearWeightLabel;
        private System.Windows.Forms.GroupBox HRZonesGroupBox;
        private System.Windows.Forms.TextBox LowHRTextBox;
        private System.Windows.Forms.Label HighHRLabel;
        private System.Windows.Forms.Label LowHRLabel;
        private System.Windows.Forms.RadioButton PercentMaxRadioButton;
        private System.Windows.Forms.RadioButton BPMRadioButton;
        private System.Windows.Forms.TextBox HighHRTextBox;
        private System.Windows.Forms.GroupBox PowerZonesGroupBox;
        private System.Windows.Forms.TextBox HighPowerTextBox;
        private System.Windows.Forms.TextBox LowPowerTextBox;
        private System.Windows.Forms.Label HighPowerLabel;
        private System.Windows.Forms.Label LowPowerLabel;
        private ExtendedTreeList PowerZonesTreeList;
        private System.Windows.Forms.GroupBox SpeedZonesGroupBox;
        private System.Windows.Forms.RadioButton PaceRadioButton;
        private System.Windows.Forms.RadioButton SpeedRadioButton;
        private System.Windows.Forms.TextBox SpeedNameTextBox;
        private System.Windows.Forms.TextBox HighSpeedTextBox;
        private System.Windows.Forms.Label NameSpeedLabel;
        private System.Windows.Forms.TextBox LowSpeedTextBox;
        private System.Windows.Forms.Label HighSpeedLabel;
        private System.Windows.Forms.Label LowSpeedLabel;
        private ExtendedTreeList SpeedZonesTreeList;



    }
}

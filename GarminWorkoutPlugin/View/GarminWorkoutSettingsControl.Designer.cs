namespace GarminWorkoutPlugin.View
{
    partial class GarminWorkoutSettingsControl
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
            this.DefaultHeartRateZonesLabel = new System.Windows.Forms.Label();
            this.DefaultSpeedZoneLabel = new System.Windows.Forms.Label();
            this.DefaultPowerZonesLabel = new System.Windows.Forms.Label();
            this.HRGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.HRSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.SpeedGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.SpeedSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerGarminRadioButton = new System.Windows.Forms.RadioButton();
            this.PowerSportTracksRadioButton = new System.Windows.Forms.RadioButton();
            this.HRSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.SpeedSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.CadenceSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.CadenceZoneComboBox = new System.Windows.Forms.ComboBox();
            this.CadenceZoneSelectionLabel = new System.Windows.Forms.Label();
            this.PowerSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.PowerZoneComboBox = new System.Windows.Forms.ComboBox();
            this.PowerZoneSelectionLabel = new System.Windows.Forms.Label();
            this.ExportDirectoryGroupBox = new System.Windows.Forms.GroupBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.ExportDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.CategoriesGroupBox = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CategorySelectionPanel = new System.Windows.Forms.Panel();
            this.GarminCategoriesPanel = new System.Windows.Forms.Panel();
            this.OtherRadioButton = new System.Windows.Forms.RadioButton();
            this.CyclingRadioButton = new System.Windows.Forms.RadioButton();
            this.RunningRadioButton = new System.Windows.Forms.RadioButton();
            this.CustomCategoryRadioButton = new System.Windows.Forms.RadioButton();
            this.ParentCategoryRadioButton = new System.Windows.Forms.RadioButton();
            this.ActivityCategoryList = new GarminWorkoutPlugin.View.AutoExpandTreeList();
            this.HRSettingsGroupBox.SuspendLayout();
            this.SpeedSettingsGroupBox.SuspendLayout();
            this.CadenceSettingsGroupBox.SuspendLayout();
            this.PowerSettingsGroupBox.SuspendLayout();
            this.ExportDirectoryGroupBox.SuspendLayout();
            this.CategoriesGroupBox.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.CategorySelectionPanel.SuspendLayout();
            this.GarminCategoriesPanel.SuspendLayout();
            this.SuspendLayout();
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
            // DefaultSpeedZoneLabel
            // 
            this.DefaultSpeedZoneLabel.AutoSize = true;
            this.DefaultSpeedZoneLabel.Location = new System.Drawing.Point(6, 16);
            this.DefaultSpeedZoneLabel.Name = "DefaultSpeedZoneLabel";
            this.DefaultSpeedZoneLabel.Size = new System.Drawing.Size(136, 13);
            this.DefaultSpeedZoneLabel.TabIndex = 0;
            this.DefaultSpeedZoneLabel.Text = "Use the speed zones from :";
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
            // HRGarminRadioButton
            // 
            this.HRGarminRadioButton.AutoSize = true;
            this.HRGarminRadioButton.Checked = true;
            this.HRGarminRadioButton.Location = new System.Drawing.Point(183, 13);
            this.HRGarminRadioButton.Name = "HRGarminRadioButton";
            this.HRGarminRadioButton.Size = new System.Drawing.Size(58, 17);
            this.HRGarminRadioButton.TabIndex = 1;
            this.HRGarminRadioButton.TabStop = true;
            this.HRGarminRadioButton.Text = "Garmin";
            this.HRGarminRadioButton.UseVisualStyleBackColor = true;
            this.HRGarminRadioButton.CheckedChanged += new System.EventHandler(this.HRGarminRadioButton_CheckedChanged);
            // 
            // HRSportTracksRadioButton
            // 
            this.HRSportTracksRadioButton.AutoSize = true;
            this.HRSportTracksRadioButton.Location = new System.Drawing.Point(276, 13);
            this.HRSportTracksRadioButton.Name = "HRSportTracksRadioButton";
            this.HRSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.HRSportTracksRadioButton.TabIndex = 1;
            this.HRSportTracksRadioButton.Text = "SportTracks";
            this.HRSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.HRSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.HRSportTracksRadioButton_CheckedChanged);
            // 
            // SpeedGarminRadioButton
            // 
            this.SpeedGarminRadioButton.AutoSize = true;
            this.SpeedGarminRadioButton.Location = new System.Drawing.Point(183, 13);
            this.SpeedGarminRadioButton.Name = "SpeedGarminRadioButton";
            this.SpeedGarminRadioButton.Size = new System.Drawing.Size(58, 17);
            this.SpeedGarminRadioButton.TabIndex = 1;
            this.SpeedGarminRadioButton.Text = "Garmin";
            this.SpeedGarminRadioButton.UseVisualStyleBackColor = true;
            this.SpeedGarminRadioButton.CheckedChanged += new System.EventHandler(this.SpeedGarminRadioButton_CheckedChanged);
            // 
            // SpeedSportTracksRadioButton
            // 
            this.SpeedSportTracksRadioButton.AutoSize = true;
            this.SpeedSportTracksRadioButton.Checked = true;
            this.SpeedSportTracksRadioButton.Location = new System.Drawing.Point(276, 13);
            this.SpeedSportTracksRadioButton.Name = "SpeedSportTracksRadioButton";
            this.SpeedSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.SpeedSportTracksRadioButton.TabIndex = 1;
            this.SpeedSportTracksRadioButton.TabStop = true;
            this.SpeedSportTracksRadioButton.Text = "SportTracks";
            this.SpeedSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.SpeedSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.SpeedSportTracksRadioButton_CheckedChanged);
            // 
            // PowerGarminRadioButton
            // 
            this.PowerGarminRadioButton.AutoSize = true;
            this.PowerGarminRadioButton.Location = new System.Drawing.Point(183, 15);
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
            this.PowerSportTracksRadioButton.Location = new System.Drawing.Point(277, 15);
            this.PowerSportTracksRadioButton.Name = "PowerSportTracksRadioButton";
            this.PowerSportTracksRadioButton.Size = new System.Drawing.Size(83, 17);
            this.PowerSportTracksRadioButton.TabIndex = 1;
            this.PowerSportTracksRadioButton.TabStop = true;
            this.PowerSportTracksRadioButton.Text = "SportTracks";
            this.PowerSportTracksRadioButton.UseVisualStyleBackColor = true;
            this.PowerSportTracksRadioButton.CheckedChanged += new System.EventHandler(this.PowerSportTracksRadioButton_CheckedChanged);
            // 
            // HRSettingsGroupBox
            // 
            this.HRSettingsGroupBox.Controls.Add(this.DefaultHeartRateZonesLabel);
            this.HRSettingsGroupBox.Controls.Add(this.HRGarminRadioButton);
            this.HRSettingsGroupBox.Controls.Add(this.HRSportTracksRadioButton);
            this.HRSettingsGroupBox.Location = new System.Drawing.Point(6, 3);
            this.HRSettingsGroupBox.Name = "HRSettingsGroupBox";
            this.HRSettingsGroupBox.Size = new System.Drawing.Size(383, 37);
            this.HRSettingsGroupBox.TabIndex = 2;
            this.HRSettingsGroupBox.TabStop = false;
            this.HRSettingsGroupBox.Text = "Heart Rate Settings";
            // 
            // SpeedSettingsGroupBox
            // 
            this.SpeedSettingsGroupBox.Controls.Add(this.SpeedSportTracksRadioButton);
            this.SpeedSettingsGroupBox.Controls.Add(this.DefaultSpeedZoneLabel);
            this.SpeedSettingsGroupBox.Controls.Add(this.SpeedGarminRadioButton);
            this.SpeedSettingsGroupBox.Location = new System.Drawing.Point(6, 47);
            this.SpeedSettingsGroupBox.Name = "SpeedSettingsGroupBox";
            this.SpeedSettingsGroupBox.Size = new System.Drawing.Size(383, 37);
            this.SpeedSettingsGroupBox.TabIndex = 3;
            this.SpeedSettingsGroupBox.TabStop = false;
            this.SpeedSettingsGroupBox.Text = "Speed Settings";
            // 
            // CadenceSettingsGroupBox
            // 
            this.CadenceSettingsGroupBox.Controls.Add(this.CadenceZoneComboBox);
            this.CadenceSettingsGroupBox.Controls.Add(this.CadenceZoneSelectionLabel);
            this.CadenceSettingsGroupBox.Location = new System.Drawing.Point(6, 91);
            this.CadenceSettingsGroupBox.Name = "CadenceSettingsGroupBox";
            this.CadenceSettingsGroupBox.Size = new System.Drawing.Size(383, 37);
            this.CadenceSettingsGroupBox.TabIndex = 4;
            this.CadenceSettingsGroupBox.TabStop = false;
            this.CadenceSettingsGroupBox.Text = "Cadence Settings";
            // 
            // CadenceZoneComboBox
            // 
            this.CadenceZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CadenceZoneComboBox.FormattingEnabled = true;
            this.CadenceZoneComboBox.Location = new System.Drawing.Point(212, 11);
            this.CadenceZoneComboBox.Name = "CadenceZoneComboBox";
            this.CadenceZoneComboBox.Size = new System.Drawing.Size(147, 21);
            this.CadenceZoneComboBox.TabIndex = 1;
            this.CadenceZoneComboBox.SelectionChangeCommitted += new System.EventHandler(this.CadenceZoneComboBox_SelectionChangedCommited);
            // 
            // CadenceZoneSelectionLabel
            // 
            this.CadenceZoneSelectionLabel.AutoSize = true;
            this.CadenceZoneSelectionLabel.Location = new System.Drawing.Point(6, 16);
            this.CadenceZoneSelectionLabel.Name = "CadenceZoneSelectionLabel";
            this.CadenceZoneSelectionLabel.Size = new System.Drawing.Size(199, 13);
            this.CadenceZoneSelectionLabel.TabIndex = 0;
            this.CadenceZoneSelectionLabel.Text = "Use this cadence zone for my workouts :";
            // 
            // PowerSettingsGroupBox
            // 
            this.PowerSettingsGroupBox.Controls.Add(this.PowerZoneComboBox);
            this.PowerSettingsGroupBox.Controls.Add(this.DefaultPowerZonesLabel);
            this.PowerSettingsGroupBox.Controls.Add(this.PowerZoneSelectionLabel);
            this.PowerSettingsGroupBox.Controls.Add(this.PowerGarminRadioButton);
            this.PowerSettingsGroupBox.Controls.Add(this.PowerSportTracksRadioButton);
            this.PowerSettingsGroupBox.Location = new System.Drawing.Point(6, 134);
            this.PowerSettingsGroupBox.Name = "PowerSettingsGroupBox";
            this.PowerSettingsGroupBox.Size = new System.Drawing.Size(383, 70);
            this.PowerSettingsGroupBox.TabIndex = 5;
            this.PowerSettingsGroupBox.TabStop = false;
            this.PowerSettingsGroupBox.Text = "Power Settings";
            // 
            // PowerZoneComboBox
            // 
            this.PowerZoneComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PowerZoneComboBox.FormattingEnabled = true;
            this.PowerZoneComboBox.Location = new System.Drawing.Point(212, 39);
            this.PowerZoneComboBox.Name = "PowerZoneComboBox";
            this.PowerZoneComboBox.Size = new System.Drawing.Size(147, 21);
            this.PowerZoneComboBox.TabIndex = 1;
            this.PowerZoneComboBox.SelectionChangeCommitted += new System.EventHandler(this.PowerZoneComboBox_SelectionChangedCommited);
            // 
            // PowerZoneSelectionLabel
            // 
            this.PowerZoneSelectionLabel.AutoSize = true;
            this.PowerZoneSelectionLabel.Location = new System.Drawing.Point(6, 44);
            this.PowerZoneSelectionLabel.Name = "PowerZoneSelectionLabel";
            this.PowerZoneSelectionLabel.Size = new System.Drawing.Size(199, 13);
            this.PowerZoneSelectionLabel.TabIndex = 0;
            this.PowerZoneSelectionLabel.Text = "Use this cadence zone for my workouts :";
            // 
            // ExportDirectoryGroupBox
            // 
            this.ExportDirectoryGroupBox.Controls.Add(this.BrowseButton);
            this.ExportDirectoryGroupBox.Controls.Add(this.ExportDirectoryTextBox);
            this.ExportDirectoryGroupBox.Location = new System.Drawing.Point(6, 368);
            this.ExportDirectoryGroupBox.Name = "ExportDirectoryGroupBox";
            this.ExportDirectoryGroupBox.Size = new System.Drawing.Size(383, 50);
            this.ExportDirectoryGroupBox.TabIndex = 6;
            this.ExportDirectoryGroupBox.TabStop = false;
            this.ExportDirectoryGroupBox.Text = "Default Export Directory";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BrowseButton.Location = new System.Drawing.Point(301, 17);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(76, 23);
            this.BrowseButton.TabIndex = 1;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // ExportDirectoryTextBox
            // 
            this.ExportDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ExportDirectoryTextBox.Location = new System.Drawing.Point(6, 19);
            this.ExportDirectoryTextBox.Name = "ExportDirectoryTextBox";
            this.ExportDirectoryTextBox.ReadOnly = true;
            this.ExportDirectoryTextBox.Size = new System.Drawing.Size(289, 20);
            this.ExportDirectoryTextBox.TabIndex = 0;
            // 
            // CategoriesGroupBox
            // 
            this.CategoriesGroupBox.Controls.Add(this.splitContainer1);
            this.CategoriesGroupBox.Location = new System.Drawing.Point(6, 210);
            this.CategoriesGroupBox.Name = "CategoriesGroupBox";
            this.CategoriesGroupBox.Size = new System.Drawing.Size(383, 152);
            this.CategoriesGroupBox.TabIndex = 7;
            this.CategoriesGroupBox.TabStop = false;
            this.CategoriesGroupBox.Text = "Categories";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 16);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.ActivityCategoryList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CategorySelectionPanel);
            this.splitContainer1.Size = new System.Drawing.Size(377, 133);
            this.splitContainer1.SplitterDistance = 238;
            this.splitContainer1.TabIndex = 6;
            // 
            // CategorySelectionPanel
            // 
            this.CategorySelectionPanel.Controls.Add(this.GarminCategoriesPanel);
            this.CategorySelectionPanel.Controls.Add(this.CustomCategoryRadioButton);
            this.CategorySelectionPanel.Controls.Add(this.ParentCategoryRadioButton);
            this.CategorySelectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CategorySelectionPanel.Enabled = false;
            this.CategorySelectionPanel.Location = new System.Drawing.Point(0, 0);
            this.CategorySelectionPanel.Name = "CategorySelectionPanel";
            this.CategorySelectionPanel.Size = new System.Drawing.Size(135, 133);
            this.CategorySelectionPanel.TabIndex = 0;
            // 
            // GarminCategoriesPanel
            // 
            this.GarminCategoriesPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.GarminCategoriesPanel.Controls.Add(this.OtherRadioButton);
            this.GarminCategoriesPanel.Controls.Add(this.CyclingRadioButton);
            this.GarminCategoriesPanel.Controls.Add(this.RunningRadioButton);
            this.GarminCategoriesPanel.Location = new System.Drawing.Point(12, 52);
            this.GarminCategoriesPanel.Name = "GarminCategoriesPanel";
            this.GarminCategoriesPanel.Size = new System.Drawing.Size(120, 78);
            this.GarminCategoriesPanel.TabIndex = 2;
            // 
            // OtherRadioButton
            // 
            this.OtherRadioButton.AutoSize = true;
            this.OtherRadioButton.Location = new System.Drawing.Point(3, 49);
            this.OtherRadioButton.Name = "OtherRadioButton";
            this.OtherRadioButton.Size = new System.Drawing.Size(51, 17);
            this.OtherRadioButton.TabIndex = 0;
            this.OtherRadioButton.Text = "Other";
            this.OtherRadioButton.UseVisualStyleBackColor = true;
            this.OtherRadioButton.CheckedChanged += new System.EventHandler(this.OtherRadioButton_CheckedChanged);
            // 
            // CyclingRadioButton
            // 
            this.CyclingRadioButton.AutoSize = true;
            this.CyclingRadioButton.Location = new System.Drawing.Point(3, 26);
            this.CyclingRadioButton.Name = "CyclingRadioButton";
            this.CyclingRadioButton.Size = new System.Drawing.Size(61, 17);
            this.CyclingRadioButton.TabIndex = 0;
            this.CyclingRadioButton.Text = "Cyclling";
            this.CyclingRadioButton.UseVisualStyleBackColor = true;
            this.CyclingRadioButton.CheckedChanged += new System.EventHandler(this.CyclingRadioButton_CheckedChanged);
            // 
            // RunningRadioButton
            // 
            this.RunningRadioButton.AutoSize = true;
            this.RunningRadioButton.Checked = true;
            this.RunningRadioButton.Location = new System.Drawing.Point(3, 3);
            this.RunningRadioButton.Name = "RunningRadioButton";
            this.RunningRadioButton.Size = new System.Drawing.Size(65, 17);
            this.RunningRadioButton.TabIndex = 0;
            this.RunningRadioButton.TabStop = true;
            this.RunningRadioButton.Text = "Running";
            this.RunningRadioButton.UseVisualStyleBackColor = true;
            this.RunningRadioButton.CheckedChanged += new System.EventHandler(this.RunningRadioButton_CheckedChanged);
            // 
            // CustomCategoryRadioButton
            // 
            this.CustomCategoryRadioButton.AutoSize = true;
            this.CustomCategoryRadioButton.Location = new System.Drawing.Point(4, 28);
            this.CustomCategoryRadioButton.Name = "CustomCategoryRadioButton";
            this.CustomCategoryRadioButton.Size = new System.Drawing.Size(125, 17);
            this.CustomCategoryRadioButton.TabIndex = 1;
            this.CustomCategoryRadioButton.Text = "Use custom category";
            this.CustomCategoryRadioButton.UseVisualStyleBackColor = true;
            this.CustomCategoryRadioButton.CheckedChanged += new System.EventHandler(this.CustomCategoryRadioButton_CheckedChanged);
            // 
            // ParentCategoryRadioButton
            // 
            this.ParentCategoryRadioButton.AutoSize = true;
            this.ParentCategoryRadioButton.Checked = true;
            this.ParentCategoryRadioButton.Location = new System.Drawing.Point(4, 4);
            this.ParentCategoryRadioButton.Name = "ParentCategoryRadioButton";
            this.ParentCategoryRadioButton.Size = new System.Drawing.Size(121, 17);
            this.ParentCategoryRadioButton.TabIndex = 0;
            this.ParentCategoryRadioButton.TabStop = true;
            this.ParentCategoryRadioButton.Text = "Use parent category";
            this.ParentCategoryRadioButton.UseVisualStyleBackColor = true;
            this.ParentCategoryRadioButton.CheckedChanged += new System.EventHandler(this.ParentCategoryRadioButton_CheckedChanged);
            // 
            // ActivityCategoryList
            // 
            this.ActivityCategoryList.AllowDrop = true;
            this.ActivityCategoryList.BackColor = System.Drawing.SystemColors.Window;
            this.ActivityCategoryList.Border = ZoneFiveSoftware.Common.Visuals.ControlBorder.Style.SmallRoundShadow;
            this.ActivityCategoryList.CheckBoxes = false;
            this.ActivityCategoryList.DefaultIndent = 15;
            this.ActivityCategoryList.DefaultRowHeight = -1;
            this.ActivityCategoryList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActivityCategoryList.HeaderRowHeight = 21;
            this.ActivityCategoryList.Location = new System.Drawing.Point(0, 0);
            this.ActivityCategoryList.MultiSelect = false;
            this.ActivityCategoryList.Name = "ActivityCategoryList";
            this.ActivityCategoryList.NumHeaderRows = ZoneFiveSoftware.Common.Visuals.TreeList.HeaderRows.None;
            this.ActivityCategoryList.NumLockedColumns = 0;
            this.ActivityCategoryList.RowAlternatingColors = true;
            this.ActivityCategoryList.RowHotlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(51)))), ((int)(((byte)(153)))), ((int)(((byte)(255)))));
            this.ActivityCategoryList.RowHotlightColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowHotlightMouse = false;
            this.ActivityCategoryList.RowSelectedColor = System.Drawing.SystemColors.Highlight;
            this.ActivityCategoryList.RowSelectedColorText = System.Drawing.SystemColors.HighlightText;
            this.ActivityCategoryList.RowSeparatorLines = true;
            this.ActivityCategoryList.ShowLines = false;
            this.ActivityCategoryList.ShowPlusMinus = false;
            this.ActivityCategoryList.Size = new System.Drawing.Size(238, 133);
            this.ActivityCategoryList.TabIndex = 5;
            this.ActivityCategoryList.TabStop = false;
            this.ActivityCategoryList.SelectedChanged += new System.EventHandler(this.ActivityCategoryList_SelectedChanged);
            // 
            // GarminWorkoutSettingsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CategoriesGroupBox);
            this.Controls.Add(this.ExportDirectoryGroupBox);
            this.Controls.Add(this.CadenceSettingsGroupBox);
            this.Controls.Add(this.SpeedSettingsGroupBox);
            this.Controls.Add(this.HRSettingsGroupBox);
            this.Controls.Add(this.PowerSettingsGroupBox);
            this.Name = "GarminWorkoutSettingsControl";
            this.Size = new System.Drawing.Size(396, 425);
            this.Load += new System.EventHandler(this.GarminWorkoutSettingsControl_Load);
            this.HRSettingsGroupBox.ResumeLayout(false);
            this.HRSettingsGroupBox.PerformLayout();
            this.SpeedSettingsGroupBox.ResumeLayout(false);
            this.SpeedSettingsGroupBox.PerformLayout();
            this.CadenceSettingsGroupBox.ResumeLayout(false);
            this.CadenceSettingsGroupBox.PerformLayout();
            this.PowerSettingsGroupBox.ResumeLayout(false);
            this.PowerSettingsGroupBox.PerformLayout();
            this.ExportDirectoryGroupBox.ResumeLayout(false);
            this.ExportDirectoryGroupBox.PerformLayout();
            this.CategoriesGroupBox.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.CategorySelectionPanel.ResumeLayout(false);
            this.CategorySelectionPanel.PerformLayout();
            this.GarminCategoriesPanel.ResumeLayout(false);
            this.GarminCategoriesPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label DefaultHeartRateZonesLabel;
        private System.Windows.Forms.Label DefaultSpeedZoneLabel;
        private System.Windows.Forms.Label DefaultPowerZonesLabel;
        private System.Windows.Forms.RadioButton HRGarminRadioButton;
        private System.Windows.Forms.RadioButton HRSportTracksRadioButton;
        private System.Windows.Forms.RadioButton SpeedGarminRadioButton;
        private System.Windows.Forms.RadioButton SpeedSportTracksRadioButton;
        private System.Windows.Forms.RadioButton PowerGarminRadioButton;
        private System.Windows.Forms.RadioButton PowerSportTracksRadioButton;
        private System.Windows.Forms.GroupBox HRSettingsGroupBox;
        private System.Windows.Forms.GroupBox SpeedSettingsGroupBox;
        private System.Windows.Forms.GroupBox CadenceSettingsGroupBox;
        private System.Windows.Forms.ComboBox CadenceZoneComboBox;
        private System.Windows.Forms.Label CadenceZoneSelectionLabel;
        private System.Windows.Forms.GroupBox PowerSettingsGroupBox;
        private System.Windows.Forms.ComboBox PowerZoneComboBox;
        private System.Windows.Forms.Label PowerZoneSelectionLabel;
        private System.Windows.Forms.GroupBox ExportDirectoryGroupBox;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox ExportDirectoryTextBox;
        private System.Windows.Forms.GroupBox CategoriesGroupBox;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private GarminWorkoutPlugin.View.AutoExpandTreeList ActivityCategoryList;
        private System.Windows.Forms.Panel CategorySelectionPanel;
        private System.Windows.Forms.Panel GarminCategoriesPanel;
        private System.Windows.Forms.RadioButton RunningRadioButton;
        private System.Windows.Forms.RadioButton CustomCategoryRadioButton;
        private System.Windows.Forms.RadioButton ParentCategoryRadioButton;
        private System.Windows.Forms.RadioButton OtherRadioButton;
        private System.Windows.Forms.RadioButton CyclingRadioButton;
    }
}

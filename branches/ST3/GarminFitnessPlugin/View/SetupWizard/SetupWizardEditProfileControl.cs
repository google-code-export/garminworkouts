using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Globalization;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardEditProfileControl : ExtendedWizardPageControl
    {
        public SetupWizardEditProfileControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            GarminActivityBanner.ThemeChanged(PluginMain.GetApplication().VisualTheme);
            HRZonesTreeList.ThemeChanged(PluginMain.GetApplication().VisualTheme);
            SpeedZonesTreeList.ThemeChanged(PluginMain.GetApplication().VisualTheme);

            m_CurrentCategory = GarminCategories.Running;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            BuildTreeLists();
            RefreshProfileInfo();
            RefreshUIFromCategory();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("EditProfileExplanationText");
            BirthDateTimePicker.CustomFormat = CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name).DateTimeFormat.ShortDatePattern;

            // User data
            ProfileNameLabel.Text = GarminFitnessView.GetLocalizedString("NameLabelText");
            GenderLabel.Text = GarminFitnessView.GetLocalizedString("GenderLabelText");
            MaleRadioButton.Text = GarminFitnessView.GetLocalizedString("MaleText");
            FemaleRadioButton.Text = GarminFitnessView.GetLocalizedString("FemaleText");
            WeightLabel.Text = GarminFitnessView.GetLocalizedString("WeightLabelText");
            WeightUnitLabel.Text = Weight.LabelAbbr(PluginMain.GetApplication().SystemPreferences.WeightUnits);
            BirthDateLabel.Text = GarminFitnessView.GetLocalizedString("BirthDateLabelText");
            RestingHeartRateLabel.Text = GarminFitnessView.GetLocalizedString("RestingHeartRateLabelText");
            RestBPMLabel.Text = CommonResources.Text.LabelBPM;

            // Activity data
            MaxHRLabel.Text = GarminFitnessView.GetLocalizedString("MaxHRLabelText");
            MaxHRBPMLabel.Text = CommonResources.Text.LabelBPM;
            GearWeightLabel.Text = GarminFitnessView.GetLocalizedString("GearWeightLabelText");
            GearWeightUnitLabel.Text = Weight.LabelAbbr(PluginMain.GetApplication().SystemPreferences.WeightUnits);

            // HR zones
            HRZonesGroupBox.Text = GarminFitnessView.GetLocalizedString("HRZonesGroupBoxText");
            BPMRadioButton.Text = CommonResources.Text.LabelBPM;
            PercentMaxRadioButton.Text = CommonResources.Text.LabelPercentOfMax;
            LowHRLabel.Text = GarminFitnessView.GetLocalizedString("LowLabelText");
            HighHRLabel.Text = GarminFitnessView.GetLocalizedString("HighLabelText");

            // Speed zones
            SpeedZonesGroupBox.Text = GarminFitnessView.GetLocalizedString("SpeedZonesGroupBoxText");
            SpeedRadioButton.Text = Length.LabelAbbr(m_CurrentProfile.BaseSpeedUnit) +
                                    GarminFitnessView.GetLocalizedString("PerHourText");
            PaceRadioButton.Text = GarminFitnessView.GetLocalizedString("MinuteAbbrText") + "/" +
                                   Length.LabelAbbr(m_CurrentProfile.BaseSpeedUnit);
            LowSpeedLabel.Text = GarminFitnessView.GetLocalizedString("LowLabelText");
            HighSpeedLabel.Text = GarminFitnessView.GetLocalizedString("HighLabelText");
            NameSpeedLabel.Text = GarminFitnessView.GetLocalizedString("NameLabelText");
        }

        void OnProfileChanged(object sender, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            RefreshProfileInfo();
        }

        void OnActivityProfileChanged(GarminActivityProfile profileModified, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            if (profileModified.Category == m_CurrentCategory)
            {
                // make sure we have the right profile, when deserializing from
                //  XML, this is not valid
                m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

                RefreshUIFromProfile();
            }
        }

#region UI callbacks
        private void GarminActivityBanner_MenuClicked(object sender, System.EventArgs e)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem menuItem;

            menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("RunningText"),
                                    new EventHandler(RunningProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("BikingText"),
                                    new EventHandler(BikingProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("OtherText"),
                                    new EventHandler(OtherProfileEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(GarminActivityBanner, GarminActivityBanner.PointToClient(MousePosition));
        }

        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.ProfileName = NameTextBox.Text;
        }

        private void MaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.IsMale = MaleRadioButton.Checked;
        }

        private void FemaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.IsMale = !FemaleRadioButton.Checked;
        }

        private void WeightTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(WeightTextBox.Text, Constants.MinWeight, Constants.MaxWeight);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), Constants.MinWeight, Constants.MaxWeight),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                WeightTextBox.Text = GarminProfileManager.Instance.UserProfile.GetWeightInUnits(PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
            }
        }

        private void WeightTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.SetWeightInUnits(float.Parse(WeightTextBox.Text), PluginMain.GetApplication().SystemPreferences.WeightUnits);
        }

        private void BirthDateTimePicker_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.BirthDate = BirthDateTimePicker.Value;
        }

        private void RestHRTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(RestHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                RestHRTextBox.Text = GarminProfileManager.Instance.UserProfile.RestingHeartRate.ToString("0");
            }
        }

        private void RestHRTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.UserProfile.RestingHeartRate = Byte.Parse(RestHRTextBox.Text);
        }

        private void MaxHRTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(MaxHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                MaxHRTextBox.Text = m_CurrentProfile.MaximumHeartRate.ToString("0");
            }
        }

        private void MaxHRTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory).MaximumHeartRate = Byte.Parse(MaxHRTextBox.Text);
        }

        private void GearWeightTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(GearWeightTextBox.Text, Constants.MinWeight, Constants.MaxWeight);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), Constants.MinWeight, Constants.MaxWeight),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                GearWeightTextBox.Text = m_CurrentProfile.GearWeight.ToString("0.0");
            }
        }

        private void GearWeightTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory).SetGearWeightInUnits(double.Parse(GearWeightTextBox.Text), PluginMain.GetApplication().SystemPreferences.WeightUnits);
        }

        private void BPMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentProfile.HRIsInPercentMax = !BPMRadioButton.Checked;
        }

        private void PercentMaxRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentProfile.HRIsInPercentMax = PercentMaxRadioButton.Checked;
        }

        private void HRZonesTreeList_SelectedItemsChanged(object sender, EventArgs e)
        {
            if (HRZonesTreeList.Selected.Count == 1)
            {
                m_SelectedHRZone = (GarminHeartRateZoneWrapper)HRZonesTreeList.Selected[0];
            }
            else
            {
                m_SelectedHRZone = null;
            }

            RefreshUIFromProfile();
        }

        private void LowHRTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GarminActivityProfile profile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            if (profile.HRIsInPercentMax)
            {
                e.Cancel = !Utils.IsTextIntegerInRange(LowHRTextBox.Text, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    LowHRTextBox.Text = m_CurrentProfile.GetHeartRateLowLimit(m_SelectedHRZone.Index).ToString("0");
                }
            }
            else
            {
                e.Cancel = !Utils.IsTextIntegerInRange(LowHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    LowHRTextBox.Text = m_CurrentProfile.GetHeartRateLowLimit(m_SelectedHRZone.Index).ToString("0");
                }
            }
        }

        private void LowHRTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentProfile.SetHeartRateLowLimit(m_SelectedHRZone.Index, Byte.Parse(LowHRTextBox.Text));
        }

        private void HighHRTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GarminActivityProfile profile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            if (profile.HRIsInPercentMax)
            {
                e.Cancel = !Utils.IsTextIntegerInRange(HighHRTextBox.Text, Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    HighHRTextBox.Text = m_CurrentProfile.GetHeartRateHighLimit(m_SelectedHRZone.Index).ToString("0");
                }
            }
            else
            {
                e.Cancel = !Utils.IsTextIntegerInRange(HighHRTextBox.Text, Constants.MinHRInBPM, profile.MaximumHeartRate);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinHRInBPM, profile.MaximumHeartRate),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    HighHRTextBox.Text = m_CurrentProfile.GetHeartRateHighLimit(m_SelectedHRZone.Index).ToString("0");
                }
            }
        }

        private void HighHRTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentProfile.SetHeartRateHighLimit(m_SelectedHRZone.Index, Byte.Parse(HighHRTextBox.Text));
        }

        private void SpeedRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentProfile.SpeedIsInPace = !SpeedRadioButton.Checked;
        }

        private void PaceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentProfile.SpeedIsInPace = PaceRadioButton.Checked;
        }

        private void SpeedZonesTreeList_SelectedItemsChanged(object sender, EventArgs e)
        {
            if (SpeedZonesTreeList.Selected.Count == 1)
            {
                m_SelectedSpeedZone = (GarminSpeedZoneWrapper)SpeedZonesTreeList.Selected[0];
            }
            else
            {
                m_SelectedSpeedZone = null;
            }

            RefreshUIFromProfile();
        }

        private void LowSpeedTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double min, max;

            if (m_CurrentProfile.SpeedIsInPace)
            {
                if (Utils.IsStatute(PluginMain.GetApplication().SystemPreferences.DistanceUnits))
                {
                    min = Constants.MinPaceStatute;
                    max = Constants.MaxPaceStatute;
                }
                else
                {
                    min = Constants.MinPaceMetric;
                    max = Constants.MaxPaceMetric;
                }

                e.Cancel = !Utils.IsTextTimeInRange(LowSpeedTextBox.Text, min, max);
                if (e.Cancel)
                {
                    UInt16 minMinutes, minSeconds;
                    UInt16 maxMinutes, maxSeconds;

                    Utils.DoubleToTime(min, out minMinutes, out minSeconds);
                    Utils.DoubleToTime(max, out maxMinutes, out maxSeconds);
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("TimeRangeValidationText"),
                                                  minMinutes, minSeconds,
                                                  maxMinutes, maxSeconds),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    LowSpeedTextBox.Text = Utils.DoubleToTimeString(m_CurrentProfile.GetSpeedLowLimit(m_SelectedSpeedZone.Index));
                }
            }
            else
            {
                if (Utils.IsStatute(PluginMain.GetApplication().SystemPreferences.DistanceUnits))
                {
                    min = Constants.MinSpeedStatute;
                    max = Constants.MaxSpeedStatute;
                }
                else
                {
                    min = Constants.MinSpeedMetric;
                    max = Constants.MaxSpeedMetric;
                }

                e.Cancel = !Utils.IsTextFloatInRange(LowSpeedTextBox.Text, min, max);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), min, max),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    LowSpeedTextBox.Text = m_CurrentProfile.GetSpeedLowLimit(m_SelectedSpeedZone.Index).ToString("0.0");
                }
            }
        }

        private void LowSpeedTextBox_Validated(object sender, EventArgs e)
        {
            if (m_CurrentProfile.SpeedIsInPace)
            {
                m_CurrentProfile.SetSpeedHighLimit(m_SelectedSpeedZone.Index, Utils.TimeToFloat(LowSpeedTextBox.Text));
            }
            else
            {
                m_CurrentProfile.SetSpeedLowLimit(m_SelectedSpeedZone.Index, double.Parse(LowSpeedTextBox.Text));
            }
        }

        private void HighSpeedTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double min, max;

            if (m_CurrentProfile.SpeedIsInPace)
            {
                if (Utils.IsStatute(m_CurrentProfile.BaseSpeedUnit))
                {
                    min = Constants.MinPaceStatute;
                    max = Constants.MaxPaceStatute;
                }
                else
                {
                    min = Constants.MinPaceMetric;
                    max = Constants.MaxPaceMetric;
                }

                e.Cancel = !Utils.IsTextTimeInRange(LowSpeedTextBox.Text, min, max);
                if (e.Cancel)
                {
                    UInt16 minMinutes, minSeconds;
                    UInt16 maxMinutes, maxSeconds;

                    Utils.DoubleToTime(min, out minMinutes, out minSeconds);
                    Utils.DoubleToTime(max, out maxMinutes, out maxSeconds);
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("TimeRangeValidationText"),
                                                  minMinutes, minSeconds,
                                                  maxMinutes, maxSeconds),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    HighSpeedTextBox.Text = Utils.DoubleToTimeString(m_CurrentProfile.GetSpeedHighLimit(m_SelectedSpeedZone.Index));
                }
            }
            else
            {
                if (Utils.IsStatute(PluginMain.GetApplication().SystemPreferences.DistanceUnits))
                {
                    min = Constants.MinSpeedStatute;
                    max = Constants.MaxSpeedStatute;
                }
                else
                {
                    min = Constants.MinSpeedMetric;
                    max = Constants.MaxSpeedMetric;
                }

                e.Cancel = !Utils.IsTextFloatInRange(HighSpeedTextBox.Text, min, max);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), min, max),
                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();

                    // Reset old valid value
                    HighSpeedTextBox.Text = m_CurrentProfile.GetSpeedHighLimit(m_SelectedSpeedZone.Index).ToString("0.0");
                }
            }
        }

        private void HighSpeedTextBox_Validated(object sender, EventArgs e)
        {
            if (m_CurrentProfile.SpeedIsInPace)
            {
                m_CurrentProfile.SetSpeedLowLimit(m_SelectedSpeedZone.Index, Utils.TimeToFloat(HighSpeedTextBox.Text));
            }
            else
            {
                m_CurrentProfile.SetSpeedHighLimit(m_SelectedSpeedZone.Index, double.Parse(HighSpeedTextBox.Text));
            }
        }

        private void SpeedNameTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentProfile.SetSpeedName(m_SelectedSpeedZone.Index, SpeedNameTextBox.Text);
        }
#endregion

        public void RunningProfileEventHandler(object sender, EventArgs args)
        {
            m_CurrentCategory = GarminCategories.Running;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            RefreshUIFromCategory();
        }

        public void BikingProfileEventHandler(object sender, EventArgs args)
        {
            m_CurrentCategory = GarminCategories.Biking;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            RefreshUIFromCategory();
        }

        public void OtherProfileEventHandler(object sender, EventArgs args)
        {
            m_CurrentCategory = GarminCategories.Other;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            RefreshUIFromCategory();
        }

        private void BuildTreeLists()
        {
            HRZonesTreeList.RowData = BuildTreeListDataForHeartRateZones();
            HRZonesTreeList.Columns.Clear();
            HRZonesTreeList.Columns.Add(new TreeList.Column("Name",
                                                            GarminFitnessView.GetLocalizedString("NumberText"),
                                                            150, System.Drawing.StringAlignment.Near));
            HRZonesTreeList.Columns.Add(new TreeList.Column("Low",
                                                            GarminFitnessView.GetLocalizedString("LowText"),
                                                            50, System.Drawing.StringAlignment.Near));
            HRZonesTreeList.Columns.Add(new TreeList.Column("High",
                                                            GarminFitnessView.GetLocalizedString("HighText"),
                                                            50, System.Drawing.StringAlignment.Near));

            SpeedZonesTreeList.RowData = BuildTreeListDataForSpeedZones();
            SpeedZonesTreeList.Columns.Clear();
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("Name",
                                                               GarminFitnessView.GetLocalizedString("NameText"),
                                                               150, System.Drawing.StringAlignment.Near));
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("Low",
                                                               GarminFitnessView.GetLocalizedString("LowText"),
                                                               50, System.Drawing.StringAlignment.Near));
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("High",
                                                               GarminFitnessView.GetLocalizedString("HighText"),
                                                               50, System.Drawing.StringAlignment.Near));
        }

        List<IGarminZoneWrapper> BuildTreeListDataForHeartRateZones()
        {
            List<IGarminZoneWrapper> result = new List<IGarminZoneWrapper>();

            for (int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                result.Add(new GarminHeartRateZoneWrapper(m_CurrentCategory, i));
            }

            return result;
        }

        List<IGarminZoneWrapper> BuildTreeListDataForSpeedZones()
        {
            List<IGarminZoneWrapper> result = new List<IGarminZoneWrapper>();

            for (int i = 0; i < Constants.GarminSpeedZoneCount; ++i)
            {
                result.Add(new GarminSpeedZoneWrapper(m_CurrentCategory, i));
            }

            return result;
        }

        private void RefreshTreeLists()
        {
            List<IGarminZoneWrapper> HRZones = (List<IGarminZoneWrapper>)HRZonesTreeList.RowData;
            List<IGarminZoneWrapper> speedZones = (List<IGarminZoneWrapper>)SpeedZonesTreeList.RowData;

            for (int i = 0; i < HRZones.Count; ++i)
            {
                HRZones[i].UpdateProfile(m_CurrentCategory);
            }

            for (int i = 0; i < speedZones.Count; ++i)
            {
                speedZones[i].UpdateProfile(m_CurrentCategory);
            }
        }

        private void RefreshProfileInfo()
        {
            NameTextBox.Text = GarminProfileManager.Instance.UserProfile.ProfileName;
            MaleRadioButton.Checked = GarminProfileManager.Instance.UserProfile.IsMale;
            FemaleRadioButton.Checked = !GarminProfileManager.Instance.UserProfile.IsMale;
            BirthDateTimePicker.Value = GarminProfileManager.Instance.UserProfile.BirthDate;
            RestHRTextBox.Text = GarminProfileManager.Instance.UserProfile.RestingHeartRate.ToString();
            WeightTextBox.Text = GarminProfileManager.Instance.UserProfile.GetWeightInUnits(PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
        }

        private void RefreshUIFromCategory()
        {
            switch (m_CurrentCategory)
            {
                case GarminCategories.Running:
                    {
                        GarminActivityBanner.Text = GarminFitnessView.GetLocalizedString("RunningText");
                        break;
                    }
                case GarminCategories.Biking:
                    {
                        GarminActivityBanner.Text = GarminFitnessView.GetLocalizedString("BikingText");
                        break;
                    }
                case GarminCategories.Other:
                    {
                        GarminActivityBanner.Text = GarminFitnessView.GetLocalizedString("OtherText");
                        break;
                    }
            }

            RefreshUIFromProfile();
        }

        private void RefreshUIFromProfile()
        {
            RefreshTreeLists();

            MaxHRTextBox.Text = m_CurrentProfile.MaximumHeartRate.ToString();
            GearWeightTextBox.Text = Weight.Convert(m_CurrentProfile.GearWeight, Weight.Units.Pound, PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");

            // HR Zones
            PercentMaxRadioButton.Checked = m_CurrentProfile.HRIsInPercentMax;
            BPMRadioButton.Checked = !m_CurrentProfile.HRIsInPercentMax;
            HRZonesTreeList.Invalidate();
            LowHRTextBox.Enabled = m_SelectedHRZone != null;
            HighHRTextBox.Enabled = m_SelectedHRZone != null;
            if (m_SelectedHRZone != null)
            {
                LowHRTextBox.Text = m_SelectedHRZone.Low;
                HighHRTextBox.Text = m_SelectedHRZone.High;
            }

            // Speed Zones
            PaceRadioButton.Checked = m_CurrentProfile.SpeedIsInPace;
            SpeedRadioButton.Checked = !m_CurrentProfile.SpeedIsInPace;
            SpeedZonesTreeList.Invalidate();
            LowSpeedTextBox.Enabled = m_SelectedSpeedZone != null;
            HighSpeedTextBox.Enabled = m_SelectedSpeedZone != null;
            SpeedNameTextBox.Enabled = m_SelectedSpeedZone != null;
            if (m_SelectedSpeedZone != null)
            {
                LowSpeedTextBox.Text = m_SelectedSpeedZone.Low;
                HighSpeedTextBox.Text = m_SelectedSpeedZone.High;
                SpeedNameTextBox.Text = m_SelectedSpeedZone.Name;
            }
        }

        private GarminCategories m_CurrentCategory;
        private GarminActivityProfile m_CurrentProfile;
        private IGarminZoneWrapper m_SelectedHRZone = null;
        private IGarminZoneWrapper m_SelectedSpeedZone = null;

        private void OnValidatedKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
    }
}

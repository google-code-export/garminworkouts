using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class GarminProfileControl : UserControl, IGarminFitnessPluginControl
    {
        public GarminProfileControl()
        {
            InitializeComponent();

            m_CurrentCategory = GarminCategories.Running;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);
            m_CurrentBikeProfile = null;

            GarminProfileManager.Instance.ProfileChanged += new GarminProfileManager.ProfileChangedEventHandler(OnProfileChanged);
            GarminProfileManager.Instance.ActivityProfileChanged += new GarminProfileManager.ActivityProfileChangedEventHandler(OnActivityProfileChanged);

            BuildTreeLists();
        }

        public void RefreshCalendar()
        {
            PluginMain.GetApplication().Calendar.SetHighlightedDates(null);
            PluginMain.GetApplication().Calendar.SetMarkedDates(null);
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            GarminActivityBanner.ThemeChanged(visualTheme);

            HRZonesTreeList.ThemeChanged(visualTheme);
            SpeedZonesTreeList.ThemeChanged(visualTheme);
            PowerZonesTreeList.ThemeChanged(visualTheme);

            BikeProfileActionBanner.ThemeChanged(visualTheme);
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            UpdateUIStrings();
            RefreshProfileInfo();
            RefreshUIFromCategory();
        }

        public void RefreshUIFromLogbook()
        {
            // Update profile from new logbook
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            RefreshProfileInfo();
            RefreshUIFromCategory();
        }

        void OnProfileChanged(object sender, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            RefreshProfileInfo();
        }

        void OnActivityProfileChanged(GarminActivityProfile profileModified, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            if (profileModified.Category == m_CurrentCategory)
            {
                // Make sure we have the right profile, when deserializing from
                //  XML, this is not valid
                m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

                RefreshUIFromProfile();
            }
        }

#region UI callbacks
        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.ProfileName = NameTextBox.Text;
        }

        private void MaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.IsMale = MaleRadioButton.Checked;
        }

        private void FemaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.IsMale = !FemaleRadioButton.Checked;
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
                WeightTextBox.Text = Weight.Convert(GarminProfileManager.Instance.WeightInPounds, Weight.Units.Pound, PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
            }
        }

        private void WeightTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.SetWeightInUnits(float.Parse(WeightTextBox.Text), PluginMain.GetApplication().SystemPreferences.WeightUnits);
        }

        private void BirthDateTimePicker_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.BirthDate = BirthDateTimePicker.Value;
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
                RestHRTextBox.Text = GarminProfileManager.Instance.RestingHeartRate.ToString("0");
            }
        }

        private void RestHRTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.RestingHeartRate = Byte.Parse(RestHRTextBox.Text);
        }

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

        private void HRZonesTreeList_SelectedChanged(object sender, EventArgs e)
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

        private void SpeedZonesTreeList_SelectedChanged(object sender, EventArgs e)
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

        private void BPMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (BPMRadioButton.Checked)
            {
                m_CurrentProfile.HRIsInPercentMax = false;
            }
        }

        private void PercentMaxRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PercentMaxRadioButton.Checked)
            {
                m_CurrentProfile.HRIsInPercentMax = true;
            }
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
            if (SpeedRadioButton.Checked)
            {
                m_CurrentProfile.SpeedIsInPace = false;
            }
        }

        private void PaceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PaceRadioButton.Checked)
            {
                m_CurrentProfile.SpeedIsInPace = true;
            }
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

        private void FTPTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(FTPTextBox.Text, Constants.MinPower, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinPower, Constants.MaxPowerProfile),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
                FTPTextBox.Text = concreteProfile.FTP.ToString("0");
            }
        }

        private void FTPTextBox_Validated(object sender, EventArgs e)
        {
            GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
            concreteProfile.FTP = UInt16.Parse(FTPTextBox.Text);
        }

        private void PowerZonesTreeList_SelectedChanged(object sender, EventArgs e)
        {
            if (PowerZonesTreeList.Selected.Count == 1)
            {
                m_SelectedPowerZone = (GarminPowerZoneWrapper)PowerZonesTreeList.Selected[0];
            }
            else
            {
                m_SelectedPowerZone = null;
            }

            RefreshUIFromProfile();
        }

        private void LowPowerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(LowPowerTextBox.Text, Constants.MinPower, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinPower, Constants.MaxPowerProfile),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
                LowPowerTextBox.Text = concreteProfile.GetPowerLowLimit(m_SelectedPowerZone.Index).ToString("0");
            }
        }

        private void LowPowerTextBox_Validated(object sender, EventArgs e)
        {
            Debug.Assert(m_CurrentProfile.GetType() == typeof(GarminBikingActivityProfile));

            GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;

            concreteProfile.SetPowerLowLimit(m_SelectedPowerZone.Index, UInt16.Parse(LowPowerTextBox.Text));
        }

        private void HighPowerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(HighPowerTextBox.Text, Constants.MinPower, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinPower, Constants.MaxPowerProfile),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
                HighPowerTextBox.Text = concreteProfile.GetPowerHighLimit(m_SelectedPowerZone.Index).ToString("0");
            }
        }

        private void HighPowerTextBox_Validated(object sender, EventArgs e)
        {
            Debug.Assert(m_CurrentProfile.GetType() == typeof(GarminBikingActivityProfile));

            GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;

            concreteProfile.SetPowerHighLimit(m_SelectedPowerZone.Index, UInt16.Parse(HighPowerTextBox.Text));
        }

        private void BikeProfileActionBanner_MenuClicked(object sender, EventArgs e)
        {
            GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
            ContextMenu menu = new ContextMenu();
            MenuItem menuItem;
            string baseMenuItemName;

            baseMenuItemName = GarminFitnessView.GetLocalizedString("BikeProfileMenuItemText");

            menuItem = new MenuItem(String.Format(baseMenuItemName, 1.ToString(), concreteProfile.GetBikeName(0)),
                                    new EventHandler(Bike1ProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(String.Format(baseMenuItemName, 2.ToString(), concreteProfile.GetBikeName(1)),
                                    new EventHandler(Bike2ProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(String.Format(baseMenuItemName, 3.ToString(), concreteProfile.GetBikeName(2)),
                                    new EventHandler(Bike3ProfileEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(BikeProfileActionBanner, BikeProfileActionBanner.PointToClient(MousePosition));
        }

        private void BikeNameTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.Name = BikeNameTextBox.Text;
        }

        private void OdometerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(OdometerTextBox.Text, Constants.MinOdometer, Constants.MaxOdometer);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), Constants.MinOdometer, Constants.MaxOdometer),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                OdometerTextBox.Text = Length.Convert(m_CurrentBikeProfile.OdometerInMeters, Length.Units.Meter, m_CurrentProfile.BaseSpeedUnit).ToString("0.0");
            }
        }

        private void OdometerTextBox_Validated(object sender, EventArgs e)
        {
            double odometerValue = double.Parse(OdometerTextBox.Text);

            m_CurrentBikeProfile.OdometerInMeters = Length.Convert(odometerValue, m_CurrentProfile.BaseSpeedUnit, Length.Units.Meter);
        }

        private void BikeWeightTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(BikeWeightTextBox.Text, Constants.MinWeight, Constants.MaxWeight);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), Constants.MinWeight, Constants.MaxWeight),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                BikeWeightTextBox.Text = Weight.Convert(m_CurrentBikeProfile.WeightInPounds, Weight.Units.Pound, PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
            }
        }

        private void BikeWeightTextBox_Validated(object sender, EventArgs e)
        {
            double weight = double.Parse(BikeWeightTextBox.Text);

            m_CurrentBikeProfile.WeightInPounds = Weight.Convert(weight, PluginMain.GetApplication().SystemPreferences.WeightUnits, Weight.Units.Pound);
        }

        private void HasCadenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.HasCadenceSensor = HasCadenceCheckBox.Checked;
        }

        private void HasPowerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.HasPowerSensor = HasPowerCheckBox.Checked;
        }

        private void AutoWheelSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.AutoWheelSize = AutoWheelSizeCheckBox.Checked;
        }

        private void WheelSizeTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(WheelSizeTextBox.Text, Constants.MinWheelSize, Constants.MaxWheelSize);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinWheelSize, Constants.MaxWheelSize),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                WheelSizeTextBox.Text = m_CurrentBikeProfile.WheelSize.ToString();
            }
        }

        private void WheelSizeTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.WheelSize = UInt16.Parse(WheelSizeTextBox.Text);
        }

        private void OnValidatedKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
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
            m_CurrentBikeProfile = ((GarminBikingActivityProfile)m_CurrentProfile).GetBikeProfile(m_BikeProfileIndex);

            RefreshUIFromCategory();
        }

        public void OtherProfileEventHandler(object sender, EventArgs args)
        {
            m_CurrentCategory = GarminCategories.Other;
            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory);

            RefreshUIFromCategory();
        }

        public void Bike1ProfileEventHandler(object sender, EventArgs args)
        {
            m_BikeProfileIndex = 0;
            m_CurrentBikeProfile = ((GarminBikingActivityProfile)m_CurrentProfile).GetBikeProfile(m_BikeProfileIndex);

            RefreshUIFromProfile();
        }

        public void Bike2ProfileEventHandler(object sender, EventArgs args)
        {
            m_BikeProfileIndex = 1;
            m_CurrentBikeProfile = ((GarminBikingActivityProfile)m_CurrentProfile).GetBikeProfile(m_BikeProfileIndex);

            RefreshUIFromProfile();
        }

        public void Bike3ProfileEventHandler(object sender, EventArgs args)
        {
            m_BikeProfileIndex = 2;
            m_CurrentBikeProfile = ((GarminBikingActivityProfile)m_CurrentProfile).GetBikeProfile(m_BikeProfileIndex);

            RefreshUIFromProfile();
        }

        private void RefreshProfileInfo()
        {
            NameTextBox.Text = GarminProfileManager.Instance.ProfileName;
            MaleRadioButton.Checked = GarminProfileManager.Instance.IsMale;
            FemaleRadioButton.Checked = !GarminProfileManager.Instance.IsMale;
            BirthDateTimePicker.Value = GarminProfileManager.Instance.BirthDate;
            RestHRTextBox.Text = GarminProfileManager.Instance.RestingHeartRate.ToString();

            WeightTextBox.Text = Weight.Convert(GarminProfileManager.Instance.WeightInPounds, Weight.Units.Pound, PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
        }

        private void RefreshUIFromCategory()
        {
            switch(m_CurrentCategory)
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

            PowerZonesTreeList.RowData = BuildTreeListDataForPowerZones();
            PowerZonesTreeList.Columns.Clear();
            PowerZonesTreeList.Columns.Add(new TreeList.Column("Name",
                                                               GarminFitnessView.GetLocalizedString("NumberText"),
                                                               150, System.Drawing.StringAlignment.Near));
            PowerZonesTreeList.Columns.Add(new TreeList.Column("Low",
                                                               GarminFitnessView.GetLocalizedString("LowText"),
                                                               50, System.Drawing.StringAlignment.Near));
            PowerZonesTreeList.Columns.Add(new TreeList.Column("High",
                                                               GarminFitnessView.GetLocalizedString("HighText"),
                                                               50, System.Drawing.StringAlignment.Near));
        }

        private void RefreshTreeLists()
        {
            List<IGarminZoneWrapper> HRZones = (List<IGarminZoneWrapper>)HRZonesTreeList.RowData;
            List<IGarminZoneWrapper> speedZones = (List<IGarminZoneWrapper>)SpeedZonesTreeList.RowData;
            List<IGarminZoneWrapper> powerZones = (List<IGarminZoneWrapper>)PowerZonesTreeList.RowData;

            for (int i = 0; i < HRZones.Count; ++i)
            {
                HRZones[i].UpdateProfile(m_CurrentCategory);
            }

            for (int i = 0; i < speedZones.Count; ++i)
            {
                speedZones[i].UpdateProfile(m_CurrentCategory);
            }

            for (int i = 0; i < powerZones.Count; ++i)
            {
                powerZones[i].UpdateProfile(m_CurrentCategory);
            }
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

            // Power Zones
            BikingProfilePanel.Visible = m_CurrentProfile.GetType() == typeof(GarminBikingActivityProfile);
            PowerZonesTreeList.Invalidate();
            LowPowerTextBox.Enabled = m_SelectedPowerZone != null;
            HighPowerTextBox.Enabled = m_SelectedPowerZone != null;
            if (m_CurrentProfile.GetType() == typeof(GarminBikingActivityProfile))
            {
                GarminBikingActivityProfile concreteProfile = (GarminBikingActivityProfile)m_CurrentProfile;
                m_CurrentBikeProfile = ((GarminBikingActivityProfile)m_CurrentProfile).GetBikeProfile(m_BikeProfileIndex);

                FTPTextBox.Text = concreteProfile.FTP.ToString();

                if (m_SelectedPowerZone != null)
                {
                    LowPowerTextBox.Text = m_SelectedPowerZone.Low;
                    HighPowerTextBox.Text = m_SelectedPowerZone.High;
                }

                // Bike profiles
                string baseMenuItemName;

                baseMenuItemName = GarminFitnessView.GetLocalizedString("BikeProfileMenuItemText");
                BikeProfileActionBanner.Text = String.Format(baseMenuItemName, (m_BikeProfileIndex + 1).ToString(), m_CurrentBikeProfile.Name);

                BikeNameTextBox.Text = m_CurrentBikeProfile.Name;
                HasCadenceCheckBox.Checked = m_CurrentBikeProfile.HasCadenceSensor;
                HasPowerCheckBox.Checked = m_CurrentBikeProfile.HasPowerSensor;
                OdometerTextBox.Text = Length.Convert(m_CurrentBikeProfile.OdometerInMeters, Length.Units.Meter, m_CurrentProfile.BaseSpeedUnit).ToString("0.0");
                BikeWeightTextBox.Text = Weight.Convert(m_CurrentBikeProfile.WeightInPounds, Weight.Units.Pound, PluginMain.GetApplication().SystemPreferences.WeightUnits).ToString("0.0");
                WheelSizeGroupBox.Enabled = m_CurrentBikeProfile.HasCadenceSensor;
                AutoWheelSizeCheckBox.Checked = m_CurrentBikeProfile.AutoWheelSize;
                WheelSizeTextBox.Enabled = !m_CurrentBikeProfile.AutoWheelSize;
                WheelSizeTextBox.Text = m_CurrentBikeProfile.WheelSize.ToString();
            }
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

        List<IGarminZoneWrapper> BuildTreeListDataForPowerZones()
        {
            List<IGarminZoneWrapper> result = new List<IGarminZoneWrapper>();

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                result.Add(new GarminPowerZoneWrapper(m_CurrentCategory, i));
            }

            return result;
        }

        private void UpdateUIStrings()
        {
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

            // Power zones
            FTPLabel.Text = GarminFitnessView.GetLocalizedString("FTPLabelText");
            FTPUnitsLabel.Text = CommonResources.Text.LabelWatts;
            PowerZonesGroupBox.Text = GarminFitnessView.GetLocalizedString("PowerZonesGroupBoxText");
            LowPowerLabel.Text = GarminFitnessView.GetLocalizedString("LowLabelText");
            HighPowerLabel.Text = GarminFitnessView.GetLocalizedString("HighLabelText");

            // Bike profiles
            BikeNameLabel.Text = GarminFitnessView.GetLocalizedString("NameLabelText");
            OdometerLabel.Text = GarminFitnessView.GetLocalizedString("OdometerLabelText");
            OdometerUnitsLabel.Text = Length.LabelAbbr(m_CurrentProfile.BaseSpeedUnit);
            BikeWeightLabel.Text = GarminFitnessView.GetLocalizedString("WeightLabelText");
            BikeWeightUnitLabel.Text = Weight.LabelAbbr(PluginMain.GetApplication().SystemPreferences.WeightUnits);
            HasCadenceCheckBox.Text = GarminFitnessView.GetLocalizedString("HasCadenceText");
            HasPowerCheckBox.Text = GarminFitnessView.GetLocalizedString("HasPowerText");
            WheelSizeGroupBox.Text = GarminFitnessView.GetLocalizedString("WheelSizeGroupBoxText");
            AutoWheelSizeCheckBox.Text = GarminFitnessView.GetLocalizedString("AutoText");
            WheelSizeLabel.Text = GarminFitnessView.GetLocalizedString("WheelSizeLabelText");
            WheelSizeUnitLabel.Text = GarminFitnessView.GetLocalizedString("MillimeterText");
        }

        private GarminCategories m_CurrentCategory;
        private GarminActivityProfile m_CurrentProfile;
        private IGarminZoneWrapper m_SelectedHRZone = null;
        private IGarminZoneWrapper m_SelectedSpeedZone = null;
        private IGarminZoneWrapper m_SelectedPowerZone = null;
        private GarminBikeProfile m_CurrentBikeProfile = null;
        private int m_BikeProfileIndex = 0;
    }
}

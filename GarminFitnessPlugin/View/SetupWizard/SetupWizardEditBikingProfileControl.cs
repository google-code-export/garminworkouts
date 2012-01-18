using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using STCommon.Resources;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardEditBikingProfileControl : ExtendedWizardPageControl
    {
        public SetupWizardEditBikingProfileControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            GarminProfileManager.Instance.ActivityProfileChanged += new GarminProfileManager.ActivityProfileChangedEventHandler(OnActivityProfileChanged);

            BikeProfileActionBanner.ThemeChanged(PluginMain.GetApplication().VisualTheme);
            PowerZonesTreeList.ThemeChanged(PluginMain.GetApplication().VisualTheme);

            m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(GarminCategories.Biking);
            m_CurrentBikeProfile = null;

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("BikeProfileExplanationText");

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

            BuildTreeLists();
            RefreshUIFromProfile();
        }

        void OnActivityProfileChanged(GarminActivityProfile profileModified, System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            if (profileModified.Category == GarminCategories.Biking)
            {
                // Make sure we have the right profile, when deserializing from
                //  XML, this is not valid
                m_CurrentProfile = GarminProfileManager.Instance.GetProfileForActivity(GarminCategories.Biking);

                RefreshUIFromProfile();
            }
        }

#region UI callbacks
        private void FTPTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(FTPTextBox.Text, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(TextResourceManager.IntegerRangeValidationText, Constants.MinPowerInWatts, Constants.MaxPowerProfile),
                                TextResourceManager.ValueValidationTitleText,
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

        private void PowerZonesTreeList_SelectedItemsChanged(object sender, EventArgs e)
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

        private void LowPowerTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(LowPowerTextBox.Text, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(TextResourceManager.IntegerRangeValidationText, Constants.MinPowerInWatts, Constants.MaxPowerProfile),
                                TextResourceManager.ValueValidationTitleText,
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

        private void HighPowerTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(HighPowerTextBox.Text, Constants.MinPowerInWatts, Constants.MaxPowerProfile);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(TextResourceManager.IntegerRangeValidationText, Constants.MinPowerInWatts, Constants.MaxPowerProfile),
                                TextResourceManager.ValueValidationTitleText,
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

        private void HasCadenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.HasCadenceSensor = HasCadenceCheckBox.Checked;
        }

        private void HasPowerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.HasPowerSensor = HasPowerCheckBox.Checked;
        }

        private void OdometerTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(OdometerTextBox.Text, Constants.MinOdometer, Constants.MaxOdometer);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(TextResourceManager.DoubleRangeValidationText, Constants.MinOdometer, Constants.MaxOdometer),
                                TextResourceManager.ValueValidationTitleText,
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

        private void BikeWeightTextBox_Validating(object sender, CancelEventArgs e)
        {
            double enteredValue = 0;

            e.Cancel = !double.TryParse(BikeWeightTextBox.Text, out enteredValue);

            if (!e.Cancel)
            {
                GarminFitnessDoubleRange valueInKilos = new GarminFitnessDoubleRange(0, Constants.MinWeight, Constants.MaxWeightInKg);

                e.Cancel = !valueInKilos.IsInRange(Weight.Convert(enteredValue, PluginMain.GetApplication().SystemPreferences.WeightUnits, Weight.Units.Kilogram));
            }

            if (e.Cancel)
            {
                double minValue = Weight.Convert(Constants.MinWeight, Weight.Units.Kilogram, PluginMain.GetApplication().SystemPreferences.WeightUnits);
                double maxValue = Weight.Convert(Constants.MaxWeightInKg, Weight.Units.Kilogram, PluginMain.GetApplication().SystemPreferences.WeightUnits);

                MessageBox.Show(String.Format(TextResourceManager.DoubleRangeValidationText, minValue, maxValue),
                                TextResourceManager.ValueValidationTitleText,
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

        private void AutoWheelSizeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            m_CurrentBikeProfile.AutoWheelSize = AutoWheelSizeCheckBox.Checked;
        }

        private void WheelSizeTextBox_Validating(object sender, CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(WheelSizeTextBox.Text, Constants.MinWheelSize, Constants.MaxWheelSize);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(TextResourceManager.IntegerRangeValidationText, Constants.MinWheelSize, Constants.MaxWheelSize),
                                TextResourceManager.ValueValidationTitleText,
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

        private void BuildTreeLists()
        {
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

        List<IGarminZoneWrapper> BuildTreeListDataForPowerZones()
        {
            List<IGarminZoneWrapper> result = new List<IGarminZoneWrapper>();

            for (int i = 0; i < Constants.GarminPowerZoneCount; ++i)
            {
                result.Add(new GarminPowerZoneWrapper(GarminCategories.Biking, i));
            }

            return result;
        }

        private void RefreshUIFromProfile()
        {
            // Power Zones
            PowerZonesTreeList.Invalidate();
            PowerZonePanel.Enabled = m_SelectedPowerZone != null;
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
                WheelSizePanel.Enabled = !m_CurrentBikeProfile.AutoWheelSize;
                WheelSizeTextBox.Text = m_CurrentBikeProfile.WheelSize.ToString();
            }
        }

        private GarminActivityProfile m_CurrentProfile;
        private IGarminZoneWrapper m_SelectedPowerZone = null;
        private GarminBikeProfile m_CurrentBikeProfile = null;
        private int m_BikeProfileIndex = 0;
    }
}

using System;
using System.Collections.Generic;
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

            GarminProfileManager.Instance.ProfileChanged += new GarminProfileManager.ProfileChangedEventHandler(OnProfileChanged);
            GarminProfileManager.Instance.ActivityProfileChanged += new GarminProfileManager.ActivityProfileChangedEventHandler(OnActivityProfileChanged);
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            GarminActivityBanner.ThemeChanged(visualTheme);

            HRZonesTreeList.ThemeChanged(visualTheme);
            SpeedZonesTreeList.ThemeChanged(visualTheme);
            PowerZonesTreeList.ThemeChanged(visualTheme);
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            UpdateUIStrings();
            RefreshProfileInfo();
            RefreshUIFromCategory();
        }

        public void RefreshUIFromLogbook()
        {
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
                RefreshUIFromProfile();
            }
        }

#region UI callbacks
        private void WeightTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(WeightTextBox.Text, Constants.MinWeightLimit, Constants.MaxWeightLimit);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("DoubleRangeValidationText"), Constants.MinWeightLimit, Constants.MaxWeightLimit),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void WeightTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.SetWeightInUnits(float.Parse(WeightTextBox.Text), PluginMain.GetApplication().SystemPreferences.WeightUnits);
        }

        private void MaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.IsMale = MaleRadioButton.Checked;
        }

        private void FemaleRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.IsMale = !FemaleRadioButton.Checked;
        }

        private void NameTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.ProfileName = NameTextBox.Text;
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
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("RunningText", GarminFitnessView.UICulture),
                                    new EventHandler(RunningProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("BikingText", GarminFitnessView.UICulture),
                                    new EventHandler(BikingProfileEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("OtherText", GarminFitnessView.UICulture),
                                    new EventHandler(OtherProfileEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(GarminActivityBanner, GarminActivityBanner.PointToClient(MousePosition));
        }

        private void MaxHRTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextIntegerInRange(MaxHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MaxHRTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory).MaximumHeartRate = Byte.Parse(MaxHRTextBox.Text);
        }

        private void GearWeightTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !Utils.IsTextFloatInRange(GearWeightTextBox.Text, Constants.MinWeightLimit, Constants.MaxWeightLimit);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("DoubleRangeValidationText"), Constants.MinWeightLimit, Constants.MaxWeightLimit),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void GearWeightTextBox_Validated(object sender, EventArgs e)
        {
            GarminProfileManager.Instance.GetProfileForActivity(m_CurrentCategory).GearWeight = double.Parse(GearWeightTextBox.Text);
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

            LowSpeedTextBox.Enabled = m_SelectedSpeedZone != null && m_SelectedSpeedZone.Index != 0;
            HighSpeedTextBox.Enabled = m_SelectedSpeedZone != null && m_SelectedSpeedZone.Index != Constants.GarminSpeedZoneCount - 1;

            RefreshUIFromProfile();
        }

        private void PowerZonesTreeList_SelectedChanged(object sender, EventArgs e)
        {
            if (PowerZonesTreeList.Selected.Count == 1)
            {
                m_SelectedPowerZone = (GarminHeartRateZoneWrapper)PowerZonesTreeList.Selected[0];
            }
            else
            {
                m_SelectedPowerZone = null;
            }

            LowPowerTextBox.Enabled = m_SelectedPowerZone != null && m_SelectedPowerZone.Index != 0;
            HighPowerTextBox.Enabled = m_SelectedPowerZone != null && m_SelectedPowerZone.Index != Constants.GarminPowerZoneCount - 1;

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
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                e.Cancel = !Utils.IsTextIntegerInRange(LowHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInPercentMax, Constants.MaxHRInPercentMax),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                e.Cancel = !Utils.IsTextIntegerInRange(HighHRTextBox.Text, Constants.MinHRInBPM, Constants.MaxHRInBPM);
                if (e.Cancel)
                {
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), Constants.MinHRInBPM, Constants.MaxHRInBPM),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void HighHRTextBox_Validated(object sender, EventArgs e)
        {
            m_CurrentProfile.SetHeartRateHighLimit(m_SelectedHRZone.Index, Byte.Parse(LowHRTextBox.Text));
        }

        private void SpeedRadioButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void PaceRadioButton_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void LowSpeedTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void LowSpeedTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void HighSpeedTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void HighSpeedTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void SpeedNameTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void LowPowerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void LowPowerTextBox_Validated(object sender, EventArgs e)
        {

        }

        private void HighPowerTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void HighPowerTextBox_Validated(object sender, EventArgs e)
        {

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
                        GarminActivityBanner.Text = GarminFitnessView.ResourceManager.GetString("RunningText", GarminFitnessView.UICulture);
                        break;
                    }
                case GarminCategories.Biking:
                    {
                        GarminActivityBanner.Text = GarminFitnessView.ResourceManager.GetString("BikingText", GarminFitnessView.UICulture);
                        break;
                    }
                case GarminCategories.Other:
                    {
                        GarminActivityBanner.Text = GarminFitnessView.ResourceManager.GetString("OtherText", GarminFitnessView.UICulture);
                        break;
                    }
            }

            RebuildTreeLists();
            RefreshUIFromProfile();
        }

        private void RebuildTreeLists()
        {
            HRZonesTreeList.RowData = BuildTreeListDataForHeartRateZones();
            HRZonesTreeList.Columns.Clear();
            HRZonesTreeList.Columns.Add(new TreeList.Column("Name", "Name", 150, System.Drawing.StringAlignment.Near));
            HRZonesTreeList.Columns.Add(new TreeList.Column("Low", "Low", 50, System.Drawing.StringAlignment.Near));
            HRZonesTreeList.Columns.Add(new TreeList.Column("High", "High", 50, System.Drawing.StringAlignment.Near));

            SpeedZonesTreeList.RowData = BuildTreeListDataForSpeedZones();
            SpeedZonesTreeList.Columns.Clear();
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("Name", "Name", 150, System.Drawing.StringAlignment.Near));
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("Low", "Low", 50, System.Drawing.StringAlignment.Near));
            SpeedZonesTreeList.Columns.Add(new TreeList.Column("High", "High", 50, System.Drawing.StringAlignment.Near));

            PowerZonesGroupBox.Visible = m_CurrentProfile.GetType() == typeof(GarminExtendedActivityProfile);
            if (m_CurrentProfile.GetType() == typeof(GarminExtendedActivityProfile))
            {
                PowerZonesTreeList.RowData = BuildTreeListDataForPowerZones();
                PowerZonesTreeList.Columns.Clear();
                PowerZonesTreeList.Columns.Add(new TreeList.Column("Name", "Name", 150, System.Drawing.StringAlignment.Near));
                PowerZonesTreeList.Columns.Add(new TreeList.Column("Low", "Low", 50, System.Drawing.StringAlignment.Near));
                PowerZonesTreeList.Columns.Add(new TreeList.Column("High", "High", 50, System.Drawing.StringAlignment.Near));
            }
        }

        private void RefreshUIFromProfile()
        {
            MaxHRTextBox.Text = m_CurrentProfile.MaximumHeartRate.ToString();
            GearWeightTextBox.Text = m_CurrentProfile.GearWeight.ToString("0.0");
            PercentMaxRadioButton.Checked = m_CurrentProfile.HRIsInPercentMax;
            BPMRadioButton.Checked = !m_CurrentProfile.HRIsInPercentMax;
            SpeedRadioButton.Checked = !m_CurrentProfile.SpeedIsInPace;
            PaceRadioButton.Checked = m_CurrentProfile.SpeedIsInPace;

            // HR Zones
            HRZonesTreeList.Invalidate();
            LowHRTextBox.Enabled = m_SelectedHRZone != null && m_SelectedHRZone.Index != 0;
            HighHRTextBox.Enabled = m_SelectedHRZone != null && m_SelectedHRZone.Index != Constants.GarminHRZoneCount - 1;
            if (m_SelectedHRZone != null)
            {
                LowHRTextBox.Text = m_SelectedHRZone.Low;
                HighHRTextBox.Text = m_SelectedHRZone.High;
            }

            // Speed Zones
            SpeedZonesTreeList.Invalidate();
            LowSpeedTextBox.Enabled = m_SelectedSpeedZone != null && m_SelectedSpeedZone.Index != 0;
            HighSpeedTextBox.Enabled = m_SelectedSpeedZone != null && m_SelectedSpeedZone.Index != Constants.GarminSpeedZoneCount - 1;
            SpeedNameTextBox.Enabled = m_SelectedSpeedZone != null;
            if (m_SelectedSpeedZone != null)
            {
                LowSpeedTextBox.Text = m_SelectedSpeedZone.Low;
                HighSpeedTextBox.Text = m_SelectedSpeedZone.High;
                SpeedNameTextBox.Text = m_SelectedSpeedZone.Name;
            }

            // Power Zones
            PowerZonesTreeList.Invalidate();
            LowPowerTextBox.Enabled = m_SelectedPowerZone != null && m_SelectedPowerZone.Index != 0;
            HighPowerTextBox.Enabled = m_SelectedPowerZone != null && m_SelectedPowerZone.Index != Constants.GarminPowerZoneCount - 1;
            if (m_CurrentProfile.GetType() == typeof(GarminExtendedActivityProfile) &&
                m_SelectedPowerZone != null)
            {
                LowPowerTextBox.Text = m_SelectedPowerZone.Low;
                HighPowerTextBox.Text = m_SelectedPowerZone.High;
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

            for (int i = 0; i < Constants.GarminHRZoneCount; ++i)
            {
                result.Add(new GarminSpeedZoneWrapper(m_CurrentCategory, i));
            }

            return result;
        }

        private void UpdateUIStrings()
        {
            BirthDateTimePicker.CustomFormat = CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name).DateTimeFormat.ShortDatePattern;

            // User data
            ProfileNameLabel.Text = GarminFitnessView.ResourceManager.GetString("NameLabelText", GarminFitnessView.UICulture);
            GenderLabel.Text = GarminFitnessView.ResourceManager.GetString("GenderLabelText", GarminFitnessView.UICulture);
            MaleRadioButton.Text = GarminFitnessView.ResourceManager.GetString("MaleText", GarminFitnessView.UICulture);
            FemaleRadioButton.Text = GarminFitnessView.ResourceManager.GetString("FemaleText", GarminFitnessView.UICulture);
            WeightLabel.Text = GarminFitnessView.ResourceManager.GetString("WeightLabelText", GarminFitnessView.UICulture);
            WeightUnitLabel.Text = Weight.LabelAbbr(PluginMain.GetApplication().SystemPreferences.WeightUnits);
            BirthDateLabel.Text = GarminFitnessView.ResourceManager.GetString("BirthDateLabelText", GarminFitnessView.UICulture);
            RestingHeartRateLabel.Text = GarminFitnessView.ResourceManager.GetString("RestingHeartRateLabelText", GarminFitnessView.UICulture);
            RestBPMLabel.Text = CommonResources.Text.LabelBPM;

            // Activity data
            MaxHRLabel.Text = GarminFitnessView.ResourceManager.GetString("MaxHRLabelText", GarminFitnessView.UICulture);
            MaxHRBPMLabel.Text = CommonResources.Text.LabelBPM;
            GearWeightLabel.Text = GarminFitnessView.ResourceManager.GetString("GearWeightLabelText", GarminFitnessView.UICulture);
            GearWeightUnitLabel.Text = Weight.LabelAbbr(PluginMain.GetApplication().SystemPreferences.WeightUnits);

            // HR zones
            HRZonesGroupBox.Text = GarminFitnessView.ResourceManager.GetString("HRZonesGroupBoxText", GarminFitnessView.UICulture);
            BPMRadioButton.Text = CommonResources.Text.LabelBPM;
            PercentMaxRadioButton.Text = CommonResources.Text.LabelPercentOfMax;
            LowHRLabel.Text = GarminFitnessView.ResourceManager.GetString("LowLabelText", GarminFitnessView.UICulture);
            HighHRLabel.Text = GarminFitnessView.ResourceManager.GetString("HighLabelText", GarminFitnessView.UICulture);

            // Speed zones
            SpeedZonesGroupBox.Text = GarminFitnessView.ResourceManager.GetString("SpeedZonesGroupBoxText", GarminFitnessView.UICulture);
            SpeedRadioButton.Text = Length.LabelAbbr(PluginMain.GetApplication().SystemPreferences.DistanceUnits) +
                                    GarminFitnessView.ResourceManager.GetString("PerHourText", GarminFitnessView.UICulture);
            PaceRadioButton.Text = GarminFitnessView.ResourceManager.GetString("MinuteAbbrText", GarminFitnessView.UICulture) + "/" +
                                   Length.LabelAbbr(PluginMain.GetApplication().SystemPreferences.DistanceUnits);
            LowSpeedLabel.Text = GarminFitnessView.ResourceManager.GetString("LowLabelText", GarminFitnessView.UICulture);
            HighSpeedLabel.Text = GarminFitnessView.ResourceManager.GetString("HighLabelText", GarminFitnessView.UICulture);
            NameSpeedLabel.Text = GarminFitnessView.ResourceManager.GetString("NameLabelText", GarminFitnessView.UICulture);

            // Power zones
            PowerZonesGroupBox.Text = GarminFitnessView.ResourceManager.GetString("PowerZonesGroupBoxText", GarminFitnessView.UICulture);
            LowPowerLabel.Text = GarminFitnessView.ResourceManager.GetString("LowLabelText", GarminFitnessView.UICulture);
            HighPowerLabel.Text = GarminFitnessView.ResourceManager.GetString("HighLabelText", GarminFitnessView.UICulture);
        }

        private GarminCategories m_CurrentCategory;
        GarminActivityProfile m_CurrentProfile;
        private IGarminZoneWrapper m_SelectedHRZone = null;
        private IGarminZoneWrapper m_SelectedSpeedZone = null;
        private IGarminZoneWrapper m_SelectedPowerZone = null;
    }
}

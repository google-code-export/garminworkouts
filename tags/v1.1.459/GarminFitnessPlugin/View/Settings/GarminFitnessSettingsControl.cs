using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class GarminFitnessSettingsControl : UserControl
    {
        public GarminFitnessSettingsControl()
        {
            InitializeComponent();

            Options.Instance.OptionsChanged += new Options.OptionsChangedEventHandler(OnOptionsChanged);
            PluginMain.LogbookChanged += new PluginMain.LogbookChangedEventHandler(OnLogbookChanged);

            ExportWarmupAsComboBox.Format += new ListControlConvertEventHandler(ExportIntensityAsComboBox_Format);
            ExportCooldownAsComboBox.Format += new ListControlConvertEventHandler(ExportIntensityAsComboBox_Format);

            UpdateUIStrings();
            UpdateOptionsUI();
        }

        public void UICultureChanged(CultureInfo culture)
        {
            UpdateUIStrings();
            UpdateOptionsUI();
        }

        void OnOptionsChanged(System.ComponentModel.PropertyChangedEventArgs changedProperty)
        {
            UpdateOptionsUI();
        }

        void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            UpdateOptionsUI();
        }

        private void HRGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksHeartRateZones = !HRGarminRadioButton.Checked;
        }

        private void HRSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksHeartRateZones = HRSportTracksRadioButton.Checked;
        }

        private void PercentMaxRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = PercentMaxRadioButton.Checked;
        }

        private void BPMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.ExportSportTracksHeartRateAsPercentMax = !BPMRadioButton.Checked;
        }

        private void SpeedGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksSpeedZones = !SpeedGarminRadioButton.Checked;
        }

        private void SpeedSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksSpeedZones = SpeedSportTracksRadioButton.Checked;
        }

        private void CadenceZoneComboBox_SelectionChangedCommited(object sender, System.EventArgs e)
        {
            if (PluginMain.GetApplication().Logbook != null)
            {
                Debug.Assert(PluginMain.GetApplication().Logbook.CadenceZones.Count > CadenceZoneComboBox.SelectedIndex);

                Options.Instance.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[CadenceZoneComboBox.SelectedIndex];
            }
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksPowerZones = !PowerGarminRadioButton.Checked;
        }

        private void PercentFTPRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.ExportSportTracksPowerAsPercentFTP = PercentFTPRadioButton.Checked;
        }

        private void WattsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.ExportSportTracksPowerAsPercentFTP = !WattsRadioButton.Checked;
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksPowerZones = PowerSportTracksRadioButton.Checked;
        }

        private void PowerZoneComboBox_SelectionChangedCommited(object sender, System.EventArgs e)
        {
            if (PluginMain.GetApplication().Logbook != null)
            {
                Debug.Assert(PluginMain.GetApplication().Logbook.PowerZones.Count > PowerZoneComboBox.SelectedIndex);

                Options.Instance.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[PowerZoneComboBox.SelectedIndex];
            }
        }

        private void ParentCategoryRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            CustomCategoryRadioButton.Checked = !ParentCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (ParentCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                Options.Instance.RemoveGarminCategory(selectedCategory);
                ActivityCategoryList.Invalidate();
            }
        }

        private void CustomCategoryRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (CustomCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;
                GarminCategories parentCategory = Options.Instance.GetGarminCategory(selectedCategory);

                Options.Instance.SetGarminCategory(selectedCategory, parentCategory);

                switch (parentCategory)
                {
                    case GarminCategories.Running:
                        RunningRadioButton.Checked = true;
                        break;
                    case GarminCategories.Biking:
                        CyclingRadioButton.Checked = true;
                        break;
                    case GarminCategories.Other:
                        OtherRadioButton.Checked = true;
                        break;
                }
            }
        }

        private void RunningRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CustomCategoryRadioButton.Checked && RunningRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Running);
            }
        }

        private void CyclingRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CustomCategoryRadioButton.Checked && CyclingRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Biking);
            }
        }

        private void ForceConsecutiveSpeedZonesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.ForceConsecutiveProfileSpeedZones = ForceConsecutiveSpeedZonesCheckBox.Checked;
        }

        private void OtherRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CustomCategoryRadioButton.Checked && OtherRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Other);
            }
        }

        private void HideInWorkoutListCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            Options.Instance.SetVisibleInWorkoutList(selectedCategory, !HideInWorkoutListCheckBox.Checked);
        }

        private void BrowseButton_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = Options.Instance.DefaultExportDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Options.Instance.DefaultExportDirectory = dlg.SelectedPath;
                ExportDirectoryTextBox.Text = Options.Instance.DefaultExportDirectory;
            }
        }

        private void AutoSplitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Options.Instance.AllowSplitWorkouts = AutoSplitCheckBox.Checked;
        }

        private void ActivityCategoryList_SelectedItemsChanged(object sender, System.EventArgs e)
        {
            if (ActivityCategoryList.Selected.Count == 1)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                CategorySelectionPanel.Enabled = true;

                if (Options.Instance.IsCustomGarminCategory(selectedCategory))
                {
                    CustomCategoryRadioButton.Checked = true;
                }
                else
                {
                    CustomCategoryRadioButton.Checked = false;
                }

                switch (Options.Instance.GetGarminCategory(selectedCategory))
                {
                    case GarminCategories.Running:
                        RunningRadioButton.Checked = true;
                        break;
                    case GarminCategories.Biking:
                        CyclingRadioButton.Checked = true;
                        break;
                    case GarminCategories.Other:
                        OtherRadioButton.Checked = true;
                        break;
                    default:
                        break;
                }

                ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
                GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

                ParentCategoryRadioButton.Enabled = selectedCategory.Parent != null;

                HideInWorkoutListCheckBox.Checked = !Options.Instance.GetVisibleInWorkoutList(selectedCategory);
            }
            else
            {
                CategorySelectionPanel.Enabled = false;
            }
        }

        void ExportIntensityAsComboBox_Format(object sender, ListControlConvertEventArgs e)
        {
            switch ((RegularStep.StepIntensity)e.ListItem)
            {
                case RegularStep.StepIntensity.Active:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("ActiveText");
                        break;
                    }
                case RegularStep.StepIntensity.Rest:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("RestText");
                        break;
                    }
                case RegularStep.StepIntensity.Warmup:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("WarmupText");
                        break;
                    }
                case RegularStep.StepIntensity.Cooldown:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("CooldownText");
                        break;
                    }
            }
        }

        private void ExportWarmupAsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Instance.TCXExportWarmupAs = (RegularStep.StepIntensity)ExportWarmupAsComboBox.SelectedItem;
        }

        private void ExportCooldownAsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Options.Instance.TCXExportCooldownAs = (RegularStep.StepIntensity)ExportCooldownAsComboBox.SelectedItem;
        }

        private void RunWizardLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GarminFitnessSetupWizard wizard = new GarminFitnessSetupWizard();

            wizard.ShowDialog();
        }

        private void DonateImageLabel_Click(object sender, System.EventArgs e)
        {
            Options.Instance.DonationReminderDate = new DateTime(0);

            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=H3VUJCWFVH2J2&lc=CA&item_name=PissedOffCil%20ST%20Plugins&item_number=Garmin%20Fitness&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

        private void UpdateUIStrings()
        {
            // HR settings
            HRSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("HRSettingsGroupBoxText");
            HRGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            HRSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            DefaultHeartRateZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultHeartRateZoneLabelText");
            ExportSTHRZonesAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportSTHRZonesAsLabelText");
            PercentMaxRadioButton.Text = CommonResources.Text.LabelPercentOfMax;
            BPMRadioButton.Text = CommonResources.Text.LabelBPM;

            // Speed settings
            ForceConsecutiveSpeedZonesCheckBox.Text = GarminFitnessView.GetLocalizedString("ForceConsecutiveSpeedZonesText");
            SpeedSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("SpeedSettingsGroupBoxText");
            DefaultSpeedZoneLabel.Text = GarminFitnessView.GetLocalizedString("DefaultSpeedZoneLabelText");
            SpeedGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            SpeedSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");

            // Cadence settings
            CadenceSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("CadenceSettingsGroupBoxText");
            CadenceZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("CadenceZoneSelectionLabelText");

            // Power settings
            PowerSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("PowerSettingsGroupBoxText");
            PowerGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            PowerSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            DefaultPowerZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultPowerZoneLabelText");
            PowerZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("PowerZoneSelectionLabelText");
            ExportSTPowerZonesAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportSTPowerZonesAsLabelText");
            PercentFTPRadioButton.Text = GarminFitnessView.GetLocalizedString("PercentFTPText");
            WattsRadioButton.Text = CommonResources.Text.LabelWatts;

            AutoSplitCheckBox.Text = GarminFitnessView.GetLocalizedString("AutoSplitCheckBoxText");

            CategoriesGroupBox.Text = GarminFitnessView.GetLocalizedString("CategoriesText");

            DeviceCommGroupBox.Text = GarminFitnessView.GetLocalizedString("DeviceCommGroupBoxText");
            DefaultExportDirectoryLabel.Text = GarminFitnessView.GetLocalizedString("DefaultExportDirectoryGroupBoxText");
            BrowseButton.Text = GarminFitnessView.GetLocalizedString("BrowseButtonText");

            TCXExportWarmupAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportWarmupAsText") +
                                          " (" + GarminFitnessView.GetLocalizedString("TCXFileText") + ") :";
            ExportWarmupAsComboBox.Items.Clear();
            ExportWarmupAsComboBox.Items.Add(RegularStep.StepIntensity.Active);
            ExportWarmupAsComboBox.Items.Add(RegularStep.StepIntensity.Rest);
            TCXExportCooldownAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportCooldownAsText") +
                                          " (" + GarminFitnessView.GetLocalizedString("TCXFileText") + ") :";
            ExportCooldownAsComboBox.Items.Clear();
            ExportCooldownAsComboBox.Items.Add(RegularStep.StepIntensity.Active);
            ExportCooldownAsComboBox.Items.Add(RegularStep.StepIntensity.Rest);

            RunWizardLinkLabel.Text = GarminFitnessView.GetLocalizedString("RunWizardText");

            ParentCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseParentCategoryText");
            CustomCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseCustomCategoryText");
            RunningRadioButton.Text = GarminFitnessView.GetLocalizedString("RunningText");
            CyclingRadioButton.Text = GarminFitnessView.GetLocalizedString("BikingText");
            OtherRadioButton.Text = GarminFitnessView.GetLocalizedString("OtherText");
            HideInWorkoutListCheckBox.Text = GarminFitnessView.GetLocalizedString("HideInWorkoutListText");

            // Fill category list
            UpdateCategoriesTreeList();
        }

        private void UpdateCategoriesTreeList()
        {
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();

            if (PluginMain.GetApplication().Logbook != null)
            {
                for (int i = 0; i < PluginMain.GetApplication().Logbook.ActivityCategories.Count; ++i)
                {
                    IActivityCategory currentCategory = PluginMain.GetApplication().Logbook.ActivityCategories[i];
                    STToGarminActivityCategoryWrapper newNode = new STToGarminActivityCategoryWrapper(null, currentCategory);

                    categories.Add(newNode);
                    AddCategoryNode(newNode, null);
                }
            }

            ActivityCategoryList.RowData = categories;
            ActivityCategoryList.Columns.Clear();
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.GetLocalizedString("CategoryText"),
                                                                 150, StringAlignment.Near));
            ActivityCategoryList.Columns.Add(new TreeList.Column("GarminCategory", "", 110, StringAlignment.Near));
        }

        private void UpdateComboBoxes()
        {
            CadenceZoneComboBox.Items.Clear();

            if (PluginMain.GetApplication().Logbook != null)
            {
                for (int i = 0; i < PluginMain.GetApplication().Logbook.CadenceZones.Count; ++i)
                {
                    IZoneCategory currentZone = PluginMain.GetApplication().Logbook.CadenceZones[i];

                    CadenceZoneComboBox.Items.Add(currentZone.Name);
                }
            }

            PowerZoneComboBox.Items.Clear();
            if (PluginMain.GetApplication().Logbook != null)
            {
                for (int i = 0; i < PluginMain.GetApplication().Logbook.PowerZones.Count; ++i)
                {
                    IZoneCategory currentZone = PluginMain.GetApplication().Logbook.PowerZones[i];

                    PowerZoneComboBox.Items.Add(currentZone.Name);
                }
            }
        }

        private void UpdateOptionsUI()
        {
            this.Enabled = PluginMain.GetApplication().Logbook != null;

            UpdateCategoriesTreeList();
            UpdateComboBoxes();

            // HR
            HRGarminRadioButton.Checked = !Options.Instance.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.Instance.UseSportTracksHeartRateZones;
            ExportHRAsPanel.Enabled = Options.Instance.UseSportTracksHeartRateZones;
            PercentMaxRadioButton.Checked = Options.Instance.ExportSportTracksHeartRateAsPercentMax;
            BPMRadioButton.Checked = !Options.Instance.ExportSportTracksHeartRateAsPercentMax;

            // Speed
            ForceConsecutiveSpeedZonesCheckBox.Checked = Options.Instance.ForceConsecutiveProfileSpeedZones;
            SpeedGarminRadioButton.Checked = !Options.Instance.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.Instance.UseSportTracksSpeedZones;

            // Cadence
            if (PluginMain.GetApplication().Logbook != null)
            {
                int cadenceSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.CadenceZones, Options.Instance.CadenceZoneCategory);
                CadenceZoneComboBox.SelectedIndex = cadenceSelectedIndex;
            }

            // Power
            if (PluginMain.GetApplication().Logbook != null)
            {
                int powerSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.PowerZones, Options.Instance.PowerZoneCategory);
                PowerZoneComboBox.SelectedIndex = powerSelectedIndex;
            }

            PowerGarminRadioButton.Checked = !Options.Instance.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.Instance.UseSportTracksPowerZones;
            ExportPowerAsPanel.Enabled = Options.Instance.UseSportTracksPowerZones;
            PercentFTPRadioButton.Checked = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            WattsRadioButton.Checked = !Options.Instance.ExportSportTracksPowerAsPercentFTP;

            // Auto-split workouts
            AutoSplitCheckBox.Checked = Options.Instance.AllowSplitWorkouts;

            // Default directory
            ExportDirectoryTextBox.Text = Options.Instance.DefaultExportDirectory;

            // Export intensities
            ExportWarmupAsComboBox.SelectedItem = Options.Instance.TCXExportWarmupAs;
            ExportCooldownAsComboBox.SelectedItem = Options.Instance.TCXExportCooldownAs;
        }

        private void AddCategoryNode(STToGarminActivityCategoryWrapper categoryNode, STToGarminActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                IActivityCategory currentCategory = category.SubCategories[i];
                STToGarminActivityCategoryWrapper newNode = new STToGarminActivityCategoryWrapper(categoryNode, currentCategory);

                AddCategoryNode(newNode, categoryNode);
            }
        }
    }
}

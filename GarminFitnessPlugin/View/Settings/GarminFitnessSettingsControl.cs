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
            Debug.Assert(PluginMain.GetApplication().Logbook.CadenceZones.Count > CadenceZoneComboBox.SelectedIndex);

            Options.Instance.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[CadenceZoneComboBox.SelectedIndex];
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksPowerZones = !PowerGarminRadioButton.Checked;
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.Instance.UseSportTracksPowerZones = PowerSportTracksRadioButton.Checked;
        }

        private void PowerZoneComboBox_SelectionChangedCommited(object sender, System.EventArgs e)
        {
            Debug.Assert(PluginMain.GetApplication().Logbook.PowerZones.Count > PowerZoneComboBox.SelectedIndex);

            Options.Instance.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[PowerZoneComboBox.SelectedIndex];
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

            if (RunningRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Running);
            }
        }

        private void CyclingRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CyclingRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Biking);
            }
        }

        private void OtherRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (OtherRadioButton.Checked)
            {
                Options.Instance.SetGarminCategory(selectedCategory, GarminCategories.Other);
            }
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

        private void ActivityCategoryList_SelectedChanged(object sender, System.EventArgs e)
        {
            if (ActivityCategoryList.Selected.Count == 1)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                CategorySelectionPanel.Enabled = true;

                if (Options.Instance.IsCustomGarminCategory(selectedCategory))
                {
                    CustomCategoryRadioButton.Checked = true;

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
                    }
                }
                else
                {
                    CustomCategoryRadioButton.Checked = false;
                }

                ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
                GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

                ParentCategoryRadioButton.Enabled = selectedCategory.Parent != null;
            }
            else
            {
                CategorySelectionPanel.Enabled = false;
            }
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
            HRGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            HRSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            SpeedGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            SpeedSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            PowerGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            PowerSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");

            HRSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("HRSettingsGroupBoxText");
            SpeedSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("SpeedSettingsGroupBoxText");
            CadenceSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("CadenceSettingsGroupBoxText");
            PowerSettingsGroupBox.Text = GarminFitnessView.GetLocalizedString("PowerSettingsGroupBoxText");
            ExportDirectoryGroupBox.Text = GarminFitnessView.GetLocalizedString("DefaultExportDirectoryGroupBoxText");

            DefaultHeartRateZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultHeartRateZoneLabelText");
            DefaultSpeedZoneLabel.Text = GarminFitnessView.GetLocalizedString("DefaultSpeedZoneLabelText");
            CadenceZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("CadenceZoneSelectionLabelText");
            DefaultPowerZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultPowerZoneLabelText");
            PowerZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("PowerZoneSelectionLabelText");
            BrowseButton.Text = GarminFitnessView.GetLocalizedString("BrowseButtonText");

            RunWizardLinkLabel.Text = GarminFitnessView.GetLocalizedString("RunWizardText");

            AutoSplitCheckBox.Text = GarminFitnessView.GetLocalizedString("AutoSplitCheckBoxText");

            CadenceZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.CadenceZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.CadenceZones[i];

                CadenceZoneComboBox.Items.Add(currentZone.Name);
            }

            PowerZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.PowerZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.PowerZones[i];

                PowerZoneComboBox.Items.Add(currentZone.Name);
            }

            ParentCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseParentCategoryText");
            CustomCategoryRadioButton.Text = GarminFitnessView.GetLocalizedString("UseCustomCategoryText");
            RunningRadioButton.Text = GarminFitnessView.GetLocalizedString("RunningText");
            CyclingRadioButton.Text = GarminFitnessView.GetLocalizedString("BikingText");
            OtherRadioButton.Text = GarminFitnessView.GetLocalizedString("OtherText");

            // Fill category list
            IApplication app = PluginMain.GetApplication();
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();

            for (int i = 0; i < app.Logbook.ActivityCategories.Count; ++i)
            {
                IActivityCategory currentCategory = app.Logbook.ActivityCategories[i];
                STToGarminActivityCategoryWrapper newNode = new STToGarminActivityCategoryWrapper(null, currentCategory);

                categories.Add(newNode);
                AddCategoryNode(newNode, null);
            }

            ActivityCategoryList.RowData = categories;
            ActivityCategoryList.Columns.Clear();
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.GetLocalizedString("CategoryText"),
                                                                 150, StringAlignment.Near));
            ActivityCategoryList.Columns.Add(new TreeList.Column("GarminCategory", "", 110, StringAlignment.Near));
        }

        private void UpdateOptionsUI()
        {
            // HR
            HRGarminRadioButton.Checked = !Options.Instance.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.Instance.UseSportTracksHeartRateZones;

            // Speed
            SpeedGarminRadioButton.Checked = !Options.Instance.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.Instance.UseSportTracksSpeedZones;

            // Cadence
            int cadenceSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.CadenceZones, Options.Instance.CadenceZoneCategory);
            CadenceZoneComboBox.SelectedIndex = cadenceSelectedIndex;

            // Power
            int powerSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.PowerZones, Options.Instance.PowerZoneCategory);
            PowerGarminRadioButton.Checked = !Options.Instance.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.Instance.UseSportTracksPowerZones;
            PowerZoneComboBox.SelectedIndex = powerSelectedIndex;

            // Auto-split workouts
            AutoSplitCheckBox.Checked = Options.Instance.AllowSplitWorkouts;

            // Default directory
            ExportDirectoryTextBox.Text = Options.Instance.DefaultExportDirectory;
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

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
        }

        void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            UpdateUIStrings();
        }

        private void GarminFitnessSettingsControl_Load(object sender, System.EventArgs e)
        {
            UpdateUIStrings();

            PluginMain.LogbookChanged += new PluginMain.LogbookChangedEventHandler(OnLogbookChanged);
        }

        private void HRGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksHeartRateZones = !HRGarminRadioButton.Checked;
        }

        private void HRSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksHeartRateZones = HRSportTracksRadioButton.Checked;
        }

        private void SpeedGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksSpeedZones = !SpeedGarminRadioButton.Checked;
        }

        private void SpeedSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksSpeedZones = SpeedSportTracksRadioButton.Checked;
        }

        private void CadenceZoneComboBox_SelectionChangedCommited(object sender, System.EventArgs e)
        {
            Trace.Assert(PluginMain.GetApplication().Logbook.CadenceZones.Count > CadenceZoneComboBox.SelectedIndex);

            Options.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[CadenceZoneComboBox.SelectedIndex];
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksPowerZones = !PowerGarminRadioButton.Checked;
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksPowerZones = PowerSportTracksRadioButton.Checked;
        }

        private void PowerZoneComboBox_SelectionChangedCommited(object sender, System.EventArgs e)
        {
            Trace.Assert(PluginMain.GetApplication().Logbook.PowerZones.Count > PowerZoneComboBox.SelectedIndex);

            Options.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[PowerZoneComboBox.SelectedIndex];
        }

        private void ParentCategoryRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            CustomCategoryRadioButton.Checked = !ParentCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (ParentCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;
                if (Options.STToGarminCategoryMap.ContainsKey(selectedCategory))
                {
                    Options.STToGarminCategoryMap.Remove(selectedCategory);

                    ActivityCategoryList.Invalidate();
                }
            }
        }

        private void CustomCategoryRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            ParentCategoryRadioButton.Checked = !CustomCategoryRadioButton.Checked;
            GarminCategoriesPanel.Enabled = CustomCategoryRadioButton.Checked;

            if (CustomCategoryRadioButton.Checked)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;
                GarminCategories parentCategory = Options.GetGarminCategory(selectedCategory);

                Options.STToGarminCategoryMap[selectedCategory] = parentCategory;

                switch (parentCategory)
                {
                    case GarminCategories.Running:
                        RunningRadioButton.Checked = true;
                        break;
                    case GarminCategories.Cycling:
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

            if (RunningRadioButton.Checked &&
                Options.STToGarminCategoryMap[selectedCategory] != GarminCategories.Running)
            {
                Options.STToGarminCategoryMap[selectedCategory] = GarminCategories.Running;

                CyclingRadioButton.Checked = false;
                OtherRadioButton.Checked = false;

                ActivityCategoryList.Invalidate();
            }
        }

        private void CyclingRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (CyclingRadioButton.Checked &&
                Options.STToGarminCategoryMap[selectedCategory] != GarminCategories.Cycling)
            {
                Options.STToGarminCategoryMap[selectedCategory] = GarminCategories.Cycling;

                RunningRadioButton.Checked = false;
                OtherRadioButton.Checked = false;

                ActivityCategoryList.Invalidate();
            }
        }

        private void OtherRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

            if (OtherRadioButton.Checked &&
                Options.STToGarminCategoryMap[selectedCategory] != GarminCategories.Other)
            {
                Options.STToGarminCategoryMap[selectedCategory] = GarminCategories.Other;

                RunningRadioButton.Checked = false;
                CyclingRadioButton.Checked = false;

                ActivityCategoryList.Invalidate();
            }
        }

        private void BrowseButton_Click(object sender, System.EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = Options.DefaultExportDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Options.DefaultExportDirectory = dlg.SelectedPath;
                ExportDirectoryTextBox.Text = Options.DefaultExportDirectory;
            }
        }

        private void ActivityCategoryList_SelectedChanged(object sender, System.EventArgs e)
        {
            if (ActivityCategoryList.Selected.Count == 1)
            {
                IActivityCategory selectedCategory = (IActivityCategory)((STToGarminActivityCategoryWrapper)ActivityCategoryList.Selected[0]).Element;

                CategorySelectionPanel.Enabled = true;

                if (Options.STToGarminCategoryMap.ContainsKey(selectedCategory))
                {
                    CustomCategoryRadioButton.Checked = true;

                    switch (Options.STToGarminCategoryMap[selectedCategory])
                    {
                        case GarminCategories.Running:
                            RunningRadioButton.Checked = true;
                            break;
                        case GarminCategories.Cycling:
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

        public void UICultureChanged(CultureInfo culture)
        {
            UpdateUIStrings();
        }

        private void UpdateUIStrings()
        {
            HRGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            HRSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);
            SpeedGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            SpeedSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);
            PowerGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            PowerSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);

            HRSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("HRSettingsGroupBoxText", GarminFitnessView.UICulture);
            SpeedSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("SpeedSettingsGroupBoxText", GarminFitnessView.UICulture);
            CadenceSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("CadenceSettingsGroupBoxText", GarminFitnessView.UICulture);
            PowerSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("PowerSettingsGroupBoxText", GarminFitnessView.UICulture);
            ExportDirectoryGroupBox.Text = GarminFitnessView.ResourceManager.GetString("DefaultExportDirectoryGroupBoxText", GarminFitnessView.UICulture);

            DefaultHeartRateZonesLabel.Text = GarminFitnessView.ResourceManager.GetString("DefaultHeartRateZoneLabelText", GarminFitnessView.UICulture);
            DefaultSpeedZoneLabel.Text = GarminFitnessView.ResourceManager.GetString("DefaultSpeedZoneLabelText", GarminFitnessView.UICulture);
            CadenceZoneSelectionLabel.Text = GarminFitnessView.ResourceManager.GetString("CadenceZoneSelectionLabelText", GarminFitnessView.UICulture);
            DefaultPowerZonesLabel.Text = GarminFitnessView.ResourceManager.GetString("DefaultPowerZoneLabelText", GarminFitnessView.UICulture);
            PowerZoneSelectionLabel.Text = GarminFitnessView.ResourceManager.GetString("PowerZoneSelectionLabelText", GarminFitnessView.UICulture);
            BrowseButton.Text = GarminFitnessView.ResourceManager.GetString("BrowseButtonText", GarminFitnessView.UICulture);

            int cadenceSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.CadenceZones, Options.CadenceZoneCategory);
            CadenceZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.CadenceZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.CadenceZones[i];

                CadenceZoneComboBox.Items.Add(currentZone.Name);
            }

            int powerSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.PowerZones, Options.PowerZoneCategory);
            PowerZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.PowerZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.PowerZones[i];

                PowerZoneComboBox.Items.Add(currentZone.Name);
            }

            ParentCategoryRadioButton.Text = GarminFitnessView.ResourceManager.GetString("UseParentCategoryText", GarminFitnessView.UICulture);
            CustomCategoryRadioButton.Text = GarminFitnessView.ResourceManager.GetString("UseCustomCategoryText", GarminFitnessView.UICulture);
            RunningRadioButton.Text = GarminFitnessView.ResourceManager.GetString("RunningText", GarminFitnessView.UICulture);
            CyclingRadioButton.Text = GarminFitnessView.ResourceManager.GetString("BikingText", GarminFitnessView.UICulture);
            OtherRadioButton.Text = GarminFitnessView.ResourceManager.GetString("OtherText", GarminFitnessView.UICulture);

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
            ActivityCategoryList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.ResourceManager.GetString("CategoryText", GarminFitnessView.UICulture),
                                                                 140, StringAlignment.Near));
            ActivityCategoryList.Columns.Add(new TreeList.Column("GarminCategory", "", 75, StringAlignment.Near));

            // HR
            HRGarminRadioButton.Checked = !Options.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.UseSportTracksHeartRateZones;

            // Speed
            SpeedGarminRadioButton.Checked = !Options.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.UseSportTracksSpeedZones;

            // Cadence
            CadenceZoneComboBox.SelectedIndex = cadenceSelectedIndex;

            // Power
            PowerGarminRadioButton.Checked = !Options.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.UseSportTracksPowerZones;
            PowerZoneComboBox.SelectedIndex = powerSelectedIndex;

            // Default directory
            ExportDirectoryTextBox.Text = Options.DefaultExportDirectory;
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

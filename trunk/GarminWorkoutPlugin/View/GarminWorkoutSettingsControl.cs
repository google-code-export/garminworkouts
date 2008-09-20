using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminWorkoutPlugin.Data;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.View
{
    public partial class GarminWorkoutSettingsControl : UserControl
    {
        public GarminWorkoutSettingsControl()
        {
            InitializeComponent();
        }

        void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            UpdateUIStrings();
        }

        private void GarminWorkoutSettingsControl_Load(object sender, System.EventArgs e)
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
            Utils.SaveWorkoutsToLogbook();
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            Options.UseSportTracksPowerZones = !PowerGarminRadioButton.Checked;
            Utils.SaveWorkoutsToLogbook();
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

        public void UICultureChanged(CultureInfo culture)
        {
            m_CurrentCulture = culture;

            UpdateUIStrings();
        }

        private void UpdateUIStrings()
        {
            HRSettingsGroupBox.Text = m_ResourceManager.GetString("HRSettingsGroupBoxText", m_CurrentCulture);
            SpeedSettingsGroupBox.Text = m_ResourceManager.GetString("SpeedSettingsGroupBoxText", m_CurrentCulture);
            CadenceSettingsGroupBox.Text = m_ResourceManager.GetString("CadenceSettingsGroupBoxText", m_CurrentCulture);
            PowerSettingsGroupBox.Text = m_ResourceManager.GetString("PowerSettingsGroupBoxText", m_CurrentCulture);
            ExportDirectoryGroupBox.Text = m_ResourceManager.GetString("DefaultExportDirectoryGroupBoxText", m_CurrentCulture);

            DefaultHeartRateZonesLabel.Text = m_ResourceManager.GetString("DefaultHeartRateZoneLabelText", m_CurrentCulture);
            DefaultSpeedZoneLabel.Text = m_ResourceManager.GetString("DefaultSpeedZoneLabelText", m_CurrentCulture);
            CadenceZoneSelectionLabel.Text = m_ResourceManager.GetString("CadenceZoneSelectionLabelText", m_CurrentCulture);
            DefaultPowerZonesLabel.Text = m_ResourceManager.GetString("DefaultPowerZoneLabelText", m_CurrentCulture);
            PowerZoneSelectionLabel.Text = m_ResourceManager.GetString("PowerZoneSelectionLabelText", m_CurrentCulture);
            BrowseButton.Text = m_ResourceManager.GetString("BrowseButtonText", m_CurrentCulture);

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

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
        private CultureInfo m_CurrentCulture;
    }
}

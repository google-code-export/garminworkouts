using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class SetupWizardSetupSTGarminZonesControl : UserControl
    {
        public SetupWizardSetupSTGarminZonesControl()
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.ResourceManager.GetString("ManualSTGarminZonesExplanationText", GarminFitnessView.UICulture);

            HRGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            HRSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);
            SpeedGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            SpeedSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);
            PowerGarminRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminText", GarminFitnessView.UICulture);
            PowerSportTracksRadioButton.Text = GarminFitnessView.ResourceManager.GetString("SportTracksText", GarminFitnessView.UICulture);

            HRSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("HRSettingsGroupBoxText", GarminFitnessView.UICulture);
            SpeedSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("SpeedSettingsGroupBoxText", GarminFitnessView.UICulture);
            PowerSettingsGroupBox.Text = GarminFitnessView.ResourceManager.GetString("PowerSettingsGroupBoxText", GarminFitnessView.UICulture);

            // HR
            HRGarminRadioButton.Checked = !Options.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.UseSportTracksHeartRateZones;

            // Speed
            SpeedGarminRadioButton.Checked = !Options.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.UseSportTracksSpeedZones;

            // Power
            PowerGarminRadioButton.Checked = !Options.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.UseSportTracksPowerZones;
        }

        private void HRGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRGarminRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = false;
            }
        }

        private void HRSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRSportTracksRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = true;
            }
        }

        private void SpeedGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedGarminRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = false;
            }
        }

        private void SpeedSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedSportTracksRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = true;
            }
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PowerGarminRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = false;
            }
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PowerSportTracksRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = true;
            }
        }
    }
}

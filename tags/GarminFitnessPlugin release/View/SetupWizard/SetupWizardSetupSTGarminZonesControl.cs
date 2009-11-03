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
    partial class SetupWizardSetupSTGarminZonesControl : ExtendedWizardPageControl
    {
        public SetupWizardSetupSTGarminZonesControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("ManualSTGarminZonesExplanationText");

            DefaultHeartRateZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultHeartRateZoneLabelText");
            DefaultSpeedZoneLabel.Text = GarminFitnessView.GetLocalizedString("DefaultSpeedZoneLabelText");
            DefaultPowerZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultPowerZoneLabelText");

            HRGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            HRSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            SpeedGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            SpeedSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            PowerGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            PowerSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");

            // HR
            HRGarminRadioButton.Checked = !Options.Instance.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.Instance.UseSportTracksHeartRateZones;

            // Speed
            SpeedGarminRadioButton.Checked = !Options.Instance.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.Instance.UseSportTracksSpeedZones;

            // Power
            PowerGarminRadioButton.Checked = !Options.Instance.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.Instance.UseSportTracksPowerZones;
        }

        private void HRGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRGarminRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = false;
            }
        }

        private void HRSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRSportTracksRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = true;
            }
        }

        private void SpeedGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedGarminRadioButton.Checked)
            {
                Options.Instance.UseSportTracksSpeedZones = false;
            }
        }

        private void SpeedSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (SpeedSportTracksRadioButton.Checked)
            {
                Options.Instance.UseSportTracksSpeedZones = true;
            }
        }

        private void PowerGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PowerGarminRadioButton.Checked)
            {
                Options.Instance.UseSportTracksPowerZones = false;
            }
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PowerSportTracksRadioButton.Checked)
            {
                Options.Instance.UseSportTracksPowerZones = true;
            }
        }
    }
}

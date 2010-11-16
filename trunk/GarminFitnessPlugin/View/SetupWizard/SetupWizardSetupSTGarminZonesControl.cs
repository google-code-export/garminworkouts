using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
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

            HRGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            HRSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            ExportSTHRZonesAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportSTHRZonesAsLabelText");
            PercentMaxRadioButton.Text = CommonResources.Text.LabelPercentOfMax;
            BPMRadioButton.Text = CommonResources.Text.LabelBPM;

            DefaultSpeedZoneLabel.Text = GarminFitnessView.GetLocalizedString("DefaultSpeedZoneLabelText");
            SpeedGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            SpeedSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");

            DefaultPowerZonesLabel.Text = GarminFitnessView.GetLocalizedString("DefaultPowerZoneLabelText");
            PowerGarminRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminText");
            PowerSportTracksRadioButton.Text = GarminFitnessView.GetLocalizedString("SportTracksText");
            ExportSTPowerZonesAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportSTPowerZonesAsLabelText");
            PercentFTPRadioButton.Text = GarminFitnessView.GetLocalizedString("PercentFTPText");
            WattsRadioButton.Text = CommonResources.Text.LabelWatts;

            // HR
            HRGarminRadioButton.Checked = !Options.Instance.UseSportTracksHeartRateZones;
            HRSportTracksRadioButton.Checked = Options.Instance.UseSportTracksHeartRateZones;
            PercentMaxRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
            BPMRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
            PercentMaxRadioButton.Checked = Options.Instance.ExportSportTracksHeartRateAsPercentMax;
            BPMRadioButton.Checked = !Options.Instance.ExportSportTracksHeartRateAsPercentMax;

            // Speed
            SpeedGarminRadioButton.Checked = !Options.Instance.UseSportTracksSpeedZones;
            SpeedSportTracksRadioButton.Checked = Options.Instance.UseSportTracksSpeedZones;

            // Power
            PowerGarminRadioButton.Checked = !Options.Instance.UseSportTracksPowerZones;
            PowerSportTracksRadioButton.Checked = Options.Instance.UseSportTracksPowerZones;
            PercentFTPRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
            WattsRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
            PercentFTPRadioButton.Checked = Options.Instance.ExportSportTracksPowerAsPercentFTP;
            WattsRadioButton.Checked = !Options.Instance.ExportSportTracksPowerAsPercentFTP;
        }

        private void HRGarminRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRGarminRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = false;

                PercentMaxRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
                BPMRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
            }
        }

        private void HRSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (HRSportTracksRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = true;

                PercentMaxRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
                BPMRadioButton.Enabled = Options.Instance.UseSportTracksHeartRateZones;
            }
        }

        private void PercentMaxRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PercentMaxRadioButton.Checked)
            {
                Options.Instance.ExportSportTracksHeartRateAsPercentMax = true;
            }
        }

        private void BPMRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (BPMRadioButton.Checked)
            {
                Options.Instance.ExportSportTracksHeartRateAsPercentMax = false;
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

                PercentFTPRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
                WattsRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
            }
        }

        private void PowerSportTracksRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PowerSportTracksRadioButton.Checked)
            {
                Options.Instance.UseSportTracksPowerZones = true;

                PercentFTPRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
                WattsRadioButton.Enabled = Options.Instance.UseSportTracksPowerZones;
            }
        }

        private void PercentFTPRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (PercentFTPRadioButton.Checked)
            {
                Options.Instance.ExportSportTracksPowerAsPercentFTP = true;
            }
        }

        private void WattsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (WattsRadioButton.Checked)
            {
                Options.Instance.ExportSportTracksPowerAsPercentFTP = false;
            }
        }
    }
}

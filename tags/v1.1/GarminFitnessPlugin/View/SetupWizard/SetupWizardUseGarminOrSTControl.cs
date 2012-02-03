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
    partial class SetupWizardUseGarminOrSTControl : ExtendedWizardPageControl
    {
        public SetupWizardUseGarminOrSTControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("ModeExplanationText");
            STModeRadioButton.Text = GarminFitnessView.GetLocalizedString("STModeRadioButtonText");
            GarminModeRadioButton.Text = GarminFitnessView.GetLocalizedString("GarminModeRadioButtonText");
            IndependentModeRadioButton.Text = GarminFitnessView.GetLocalizedString("IndependentModeRadioButtonText");

            // Find the current settings and setup the radio buttons accordingly
            if (Options.Instance.UseSportTracksHeartRateZones == Options.Instance.UseSportTracksSpeedZones &&
                Options.Instance.UseSportTracksSpeedZones == Options.Instance.UseSportTracksPowerZones)
            {
                // All have the same setting
                STModeRadioButton.Checked = Options.Instance.UseSportTracksHeartRateZones;
                GarminModeRadioButton.Checked = !Options.Instance.UseSportTracksHeartRateZones;
            }
            else
            {
                // Different options
                IndependentModeRadioButton.Checked = true;
            }
        }

        private void STZonesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (STModeRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = true;
                Options.Instance.UseSportTracksPowerZones = true;
                Options.Instance.UseSportTracksSpeedZones = true;
            }
        }

        private void GarminZonesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (GarminModeRadioButton.Checked)
            {
                Options.Instance.UseSportTracksHeartRateZones = false;
                Options.Instance.UseSportTracksPowerZones = false;
                Options.Instance.UseSportTracksSpeedZones = false;
            }
        }

        private void IndependentModeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            ((GarminFitnessSetupWizard)Wizard).IsIndependentZonesSetupSelected = IndependentModeRadioButton.Checked;
        }
    }
}

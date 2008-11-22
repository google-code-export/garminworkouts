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
    public partial class SetupWizardUseGarminOrSTControl : UserControl
    {
        public SetupWizardUseGarminOrSTControl()
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.ResourceManager.GetString("ModeExplanationText", GarminFitnessView.UICulture);
            STModeRadioButton.Text = GarminFitnessView.ResourceManager.GetString("STModeRadioButtonText", GarminFitnessView.UICulture);
            GarminModeRadioButton.Text = GarminFitnessView.ResourceManager.GetString("GarminModeRadioButtonText", GarminFitnessView.UICulture);
            IndependentModeRadioButton.Text = GarminFitnessView.ResourceManager.GetString("IndependentModeRadioButtonText", GarminFitnessView.UICulture);

            // Find the current settings and setup the radio buttons accordingly
            if (Options.UseSportTracksHeartRateZones == Options.UseSportTracksSpeedZones &&
                Options.UseSportTracksSpeedZones == Options.UseSportTracksPowerZones)
            {
                // All have the same setting
                STModeRadioButton.Checked = Options.UseSportTracksHeartRateZones;
                GarminModeRadioButton.Checked = !Options.UseSportTracksHeartRateZones;
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
                Options.UseSportTracksHeartRateZones = true;
                Options.UseSportTracksPowerZones = true;
                Options.UseSportTracksSpeedZones = true;
            }
        }

        private void GarminZonesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (GarminModeRadioButton.Checked)
            {
                Options.UseSportTracksHeartRateZones = false;
                Options.UseSportTracksPowerZones = false;
                Options.UseSportTracksSpeedZones = false;
            }
        }

        public bool IsIndependentSetupSelected
        {
            get { return IndependentModeRadioButton.Checked;  }
        }
    }
}

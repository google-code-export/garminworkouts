using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardImportOrEditProfileControl : ExtendedWizardPageControl
    {
        public SetupWizardImportOrEditProfileControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("ImportEditExplanationText");
            ImportProfileRadioButton.Text = GarminFitnessView.GetLocalizedString("ImportRadioButtonText");
            EditProfileRadioButton.Text = GarminFitnessView.GetLocalizedString("EditRadioButtonText");

            ImportProfileRadioButton.Checked = ((GarminFitnessSetupWizard)Wizard).ImportProfile;
            EditProfileRadioButton.Checked = !((GarminFitnessSetupWizard)Wizard).ImportProfile;
        }

        private void EditProfileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EditProfileRadioButton.Checked)
            {
                ((GarminFitnessSetupWizard)Wizard).ImportProfile = false;
            }
        }

        private void ImportProfileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (ImportProfileRadioButton.Checked)
            {
                ((GarminFitnessSetupWizard)Wizard).ImportProfile = true;
            }
        }
    }
}

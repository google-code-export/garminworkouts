using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardWelcomeControl : ExtendedWizardPageControl
    {
        public SetupWizardWelcomeControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            WelcomeLabel.Text = GarminFitnessView.GetLocalizedString("WelcomeLabelText");
            ContinueLabel.Text = GarminFitnessView.GetLocalizedString("ContinueLabelText");
            FinishLabel.Text = GarminFitnessView.GetLocalizedString("FinishLabelText");
        }
    }
}

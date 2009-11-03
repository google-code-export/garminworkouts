using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardCompletedControl : ExtendedWizardPageControl
    {
        public SetupWizardCompletedControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("CompletedExplanationText");
            FinishLabel.Text = GarminFitnessView.GetLocalizedString("CompletedLabelText");
        }
    }
}

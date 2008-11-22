using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    public partial class SetupWizardWelcomeControl : UserControl
    {
        public SetupWizardWelcomeControl()
        {
            InitializeComponent();

            WelcomeLabel.Text = GarminFitnessView.ResourceManager.GetString("WelcomeLabelText", GarminFitnessView.UICulture);
            ContinueLabel.Text = GarminFitnessView.ResourceManager.GetString("ContinueLabelText", GarminFitnessView.UICulture);
            FinishLabel.Text = GarminFitnessView.ResourceManager.GetString("FinishLabelText", GarminFitnessView.UICulture);
        }
    }
}

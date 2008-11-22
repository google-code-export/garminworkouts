using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    public partial class SetupWizardImportOrEditProfileControl : UserControl
    {
        public SetupWizardImportOrEditProfileControl()
        {
            InitializeComponent();
        }

        private void EditProfileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EditProfileRadioButton.Checked)
            {
                m_EditProfile = true;
            }
        }

        private void ImportProfileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (ImportProfileRadioButton.Checked)
            {
                m_EditProfile = false;
            }
        }

        public bool EditProfile
        {
            get { return m_EditProfile; }
        }

        public bool ImportProfile
        {
            get { return !m_EditProfile; }
        }

        private bool m_EditProfile = false;
    }
}

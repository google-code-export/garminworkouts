using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using System.Diagnostics;

namespace GarminFitnessPlugin.View
{
    partial class CancelOperationDialog : Form
    {
        public CancelOperationDialog(GarminDeviceManager.CancelOperationDelegate cancelDelegate)
        {
            Debug.Assert(cancelDelegate != null);

            InitializeComponent();

            m_CancelDelegate = cancelDelegate;

            ProgressBar.MarqueeAnimationSpeed = 10;

            Cancel_Button.Text = CommonResources.Text.ActionCancel;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            m_CancelDelegate();
        }

        GarminDeviceManager.CancelOperationDelegate m_CancelDelegate = null;
    }
}

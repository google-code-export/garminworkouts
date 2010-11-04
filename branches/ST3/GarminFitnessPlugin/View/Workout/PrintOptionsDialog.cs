using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    partial class PrintOptionsDialog : Form
    {
        public PrintOptionsDialog(List<Workout> workoutsToPrint)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.GetLocalizedString("PrintOptionsText");
            InkFriendlyCheckBox.Text = GarminFitnessView.GetLocalizedString("InkFriendlyModeText");
            PrintButton.Text = GarminFitnessView.GetLocalizedString("PrintText");
            PrintPreviewButton.Text = GarminFitnessView.GetLocalizedString("PrintPreviewText");
            Cancel_Button.Text = CommonResources.Text.ActionCancel;

            m_WorkoutsToPrint = workoutsToPrint;
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            WorkoutPrintDocument printDocument = new WorkoutPrintDocument(m_WorkoutsToPrint,
                                                                          InkFriendlyCheckBox.Checked);
            PrintDialog printDialog = new PrintDialog();

            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
                Close();
            }
        }

        private void PrintPreviewButton_Click(object sender, EventArgs e)
        {
            WorkoutPrintDocument printDocument = new WorkoutPrintDocument(m_WorkoutsToPrint,
                                                                          InkFriendlyCheckBox.Checked);
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();

            previewDialog.Document = printDocument;

            previewDialog.ShowDialog();
        }

        List<Workout> m_WorkoutsToPrint = null;
    }
}

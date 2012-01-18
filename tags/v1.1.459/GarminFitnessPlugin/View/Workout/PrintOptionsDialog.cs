using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
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
            UnrollRepeatsCheckBox.Text = GarminFitnessView.GetLocalizedString("UnrollRepeatsText");
            PageSetupButton.Text = GarminFitnessView.GetLocalizedString("PageSetupText") + "...";
            PrintButton.Text = GarminFitnessView.GetLocalizedString("PrintText") + "...";
            PrintPreviewButton.Text = GarminFitnessView.GetLocalizedString("PrintPreviewText") + "...";
            Cancel_Button.Text = CommonResources.Text.ActionCancel;

            m_WorkoutsToPrint = workoutsToPrint;
        }

        private void PageSetupButton_Click(object sender, EventArgs e)
        {
            PageSetupDialog pageSetupDialog = new PageSetupDialog();

            pageSetupDialog.PageSettings = m_DocumentPageSettings;

            if (pageSetupDialog.ShowDialog() == DialogResult.OK)
            {
                m_DocumentPageSettings = pageSetupDialog.PageSettings;
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            WorkoutPrintDocument printDocument = new WorkoutPrintDocument(m_WorkoutsToPrint,
                                                                          InkFriendlyCheckBox.Checked,
                                                                          UnrollRepeatsCheckBox.Checked);
            PrintDialog printDialog = new PrintDialog();

            printDocument.DefaultPageSettings = m_DocumentPageSettings;
            printDialog.Document = printDocument;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
                Close();

                if (m_PrintPreviewDialog != null)
                {
                    m_PrintPreviewDialog.Close();
                    m_PrintPreviewDialog = null;
                }
            }
        }

        private void PrintPreviewButton_Click(object sender, EventArgs e)
        {
            m_WorkoutDocument = new WorkoutPrintDocument(m_WorkoutsToPrint,
                                                         InkFriendlyCheckBox.Checked,
                                                         UnrollRepeatsCheckBox.Checked);
            m_PrintPreviewDialog = new PrintPreviewDialog();
            ToolStrip previewStrip = m_PrintPreviewDialog.Controls["ToolStrip1"] as ToolStrip;

            if (previewStrip != null)
            {
                m_PrintPreviewToolStripButton = previewStrip.Items["PrintToolStripButton"] as ToolStripButton;

                if (m_PrintPreviewToolStripButton != null)
                {
                    m_PrintPreviewToolStripButton.MouseDown += new MouseEventHandler(printButton_MouseDown);
                    m_PrintPreviewToolStripButton.MouseUp += new MouseEventHandler(PrintButton_Click);
                }
            }

            m_WorkoutDocument.DefaultPageSettings = m_DocumentPageSettings;

            m_PrintPreviewDialog.Document = m_WorkoutDocument;

            if (m_DocumentPageSettings.Landscape)
            {
                m_PrintPreviewDialog.MinimumSize = new Size(600, 400);
            }
            else
            {
                m_PrintPreviewDialog.MinimumSize = new Size(400, 600);
            }

            m_PrintPreviewDialog.ShowDialog();
        }

        void printButton_MouseDown(object sender, MouseEventArgs e)
        {
            m_PrintPreviewDialog.Document = null;
            m_PrintPreviewToolStripButton.PerformClick();
        }

        PrintPreviewDialog m_PrintPreviewDialog = null;
        ToolStripButton m_PrintPreviewToolStripButton = null;
        WorkoutPrintDocument m_WorkoutDocument = null;
        PageSettings m_DocumentPageSettings = new PageSettings();
        List<Workout> m_WorkoutsToPrint = null;
    }
}

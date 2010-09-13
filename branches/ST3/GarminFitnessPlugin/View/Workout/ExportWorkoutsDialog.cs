using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class ExportWorkoutsDialog : Form
    {
        public ExportWorkoutsDialog(bool FITFormatOnly)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.GetLocalizedString("SelectFolderText");
            ExportAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportAsLabelText");
            ExportButton.Text = CommonResources.Text.ActionExport;
            Cancel_Button.Text = CommonResources.Text.ActionCancel;

//            if (!FITFormatOnly)
            {
                FileFormatsComboBox.Items.Add(GarminFitnessView.GetLocalizedString("TCXFileText"));
            }
            //FileFormatsComboBox.Items.Add(GarminFitnessView.GetLocalizedString("FitFileText"));

            FileFormatsComboBox.SelectedIndex = 0;

            DirectoryTree.SelectedPath = Options.Instance.DefaultExportDirectory;
        }

        private void Cancel_Button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public string SelectedPath
        {
            get { return DirectoryTree.SelectedPath; }
            set { DirectoryTree.SelectedPath = value; }
        }
    }
}

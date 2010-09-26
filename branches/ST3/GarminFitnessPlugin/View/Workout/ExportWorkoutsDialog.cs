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
    partial class ExportWorkoutsDialog : Form
    {
        public ExportWorkoutsDialog(bool FITFormatOnly)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.GetLocalizedString("SelectFolderText");
            ExportAsLabel.Text = GarminFitnessView.GetLocalizedString("ExportAsLabelText");
            ExportButton.Text = CommonResources.Text.ActionExport;
            Cancel_Button.Text = CommonResources.Text.ActionCancel;

            if (!FITFormatOnly)
            {
                FileFormatsComboBox.Items.Add(GarminWorkoutManager.FileFormats.FileFormat_TCX);
            }
            FileFormatsComboBox.Items.Add(GarminWorkoutManager.FileFormats.FileFormat_FIT);
            FileFormatsComboBox.Format += new ListControlConvertEventHandler(OnFileFormatsComboBoxFormat);

            FileFormatsComboBox.SelectedIndex = 0;

            DirectoryTree.SelectedPath = Options.Instance.DefaultExportDirectory;
        }

        void OnFileFormatsComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            GarminWorkoutManager.FileFormats fileFormat = (GarminWorkoutManager.FileFormats)e.ListItem;

            switch(fileFormat)
            {
                case GarminWorkoutManager.FileFormats.FileFormat_TCX:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("TCXFileText");
                        break;
                    }
                case GarminWorkoutManager.FileFormats.FileFormat_FIT:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("FitFileText");
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
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

        public GarminWorkoutManager.FileFormats SelectedFormat
        {
            get { return (GarminWorkoutManager.FileFormats)FileFormatsComboBox.SelectedItem; }
        }
    }
}

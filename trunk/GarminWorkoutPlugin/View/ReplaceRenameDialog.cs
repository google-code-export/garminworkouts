using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    public partial class ReplaceRenameDialog : Form
    {
        public ReplaceRenameDialog(string newName)
        {
            CultureInfo uiCulture = GarminFitnessView.UICulture;

            InitializeComponent();

            ReplaceArrowPictureBox.Click += new EventHandler(ReplacePanel_Click);
            ReplaceLabel.Click += new EventHandler(ReplacePanel_Click);
            ReplaceExplanationLabel.Click += new EventHandler(ReplacePanel_Click);

            RenameArrowPictureBox.Click += new EventHandler(RenamePanel_Click);
            RenameLabel.Click += new EventHandler(RenamePanel_Click);
            RenameExplanationLabel.Click += new EventHandler(RenamePanel_Click);
            NewNameLabel.Click += new EventHandler(RenamePanel_Click);

            ReplaceRenameIntroLabel.Text = GarminFitnessView.ResourceManager.GetString("ReplaceRenameIntroLabelText", uiCulture);
            ReplaceExplanationLabel.Text = GarminFitnessView.ResourceManager.GetString("ReplaceExplanationLabelText", uiCulture);
            RenameExplanationLabel.Text = GarminFitnessView.ResourceManager.GetString("RenameExplanationLabelText", uiCulture);
            NewNameTextBox.Text = newName;
        }

        private void ReplacePanel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void RenamePanel_Click(object sender, EventArgs e)
        {
            CultureInfo uiCulture = GarminFitnessView.UICulture;

            if(!WorkoutManager.Instance.IsWorkoutNameValid(NewName))
            {
                MessageBox.Show(GarminFitnessView.ResourceManager.GetString("InvalidWorkoutNameText", uiCulture),
                                GarminFitnessView.ResourceManager.GetString("ErrorText", uiCulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            }
        }

        public string NewName
        {
            get { return NewNameTextBox.Text; }
        }
    }
}
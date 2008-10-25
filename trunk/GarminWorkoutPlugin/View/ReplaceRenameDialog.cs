using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using GarminWorkoutPlugin.Data;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.View
{
    public partial class ReplaceRenameDialog : Form
    {
        public ReplaceRenameDialog(string newName)
        {
            CultureInfo uiCulture = ((GarminWorkoutView)PluginMain.GetApplication().ActiveView).UICulture;

            InitializeComponent();

            ReplaceArrowPictureBox.Click += new EventHandler(ReplacePanel_Click);
            ReplaceLabel.Click += new EventHandler(ReplacePanel_Click);
            ReplaceExplanationLabel.Click += new EventHandler(ReplacePanel_Click);

            RenameArrowPictureBox.Click += new EventHandler(RenamePanel_Click);
            RenameLabel.Click += new EventHandler(RenamePanel_Click);
            RenameExplanationLabel.Click += new EventHandler(RenamePanel_Click);
            NewNameLabel.Click += new EventHandler(RenamePanel_Click);

            ReplaceRenameIntroLabel.Text = m_ResourceManager.GetString("ReplaceRenameIntroLabelText", uiCulture);
            ReplaceExplanationLabel.Text = m_ResourceManager.GetString("ReplaceExplanationLabelText", uiCulture);
            RenameExplanationLabel.Text = m_ResourceManager.GetString("RenameExplanationLabelText", uiCulture);
            NewNameTextBox.Text = newName;
        }

        private void ReplacePanel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void RenamePanel_Click(object sender, EventArgs e)
        {
            CultureInfo uiCulture = ((GarminWorkoutView)PluginMain.GetApplication().ActiveView).UICulture;

            if(!WorkoutManager.Instance.IsWorkoutNameValid(NewName))
            {
                MessageBox.Show(m_ResourceManager.GetString("InvalidWorkoutNameText", uiCulture),
                                m_ResourceManager.GetString("ErrorText", uiCulture),
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

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    partial class SelectDeviceDialog : Form
    {
        public SelectDeviceDialog()
        {
            CultureInfo UICulture = GarminFitnessView.UICulture;

            InitializeComponent();

            GarminDeviceManager.GetInstance().TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnManagerTaskCompleted);

            this.Text = GarminFitnessView.ResourceManager.GetString("SelectDeviceText", UICulture);
            IntroLabel.Text = GarminFitnessView.ResourceManager.GetString("SelectDeviceIntroText", UICulture);
            RefreshButton.Text = GarminFitnessView.ResourceManager.GetString("RefreshButtonText", UICulture);
            Cancel_Button.Text = CommonResources.Text.ActionCancel;
            OKButton.Text = CommonResources.Text.ActionOk;

            RefreshDeviceComboBox();
        }

        ~SelectDeviceDialog()
        {
            GarminDeviceManager.GetInstance().TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnManagerTaskCompleted);
        }

        private void RefreshButton_Click(object sender, System.EventArgs e)
        {
            OKButton.Enabled = false;
            Cancel_Button.Enabled = false;
            RefreshButton.Enabled = false;
            DevicesComboBox.Enabled = false;
            Cursor = Cursors.WaitCursor;

            GarminDeviceManager.GetInstance().RefreshDevices();
        }

        private void DevicesComboBox_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            m_SelectedDevice = GarminDeviceManager.GetInstance().Devices[DevicesComboBox.SelectedIndex];
        }

        private void OKButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Button_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void RefreshDeviceComboBox()
        {
            DevicesComboBox.Items.Clear();
            for (int i = 0; i < GarminDeviceManager.GetInstance().Devices.Count; ++i)
            {
                DevicesComboBox.Items.Add(GarminDeviceManager.GetInstance().Devices[i].DisplayName);
            }

            if (GarminDeviceManager.GetInstance().Devices.Count > 0)
            {
                m_SelectedDevice = GarminDeviceManager.GetInstance().Devices[0];
                DevicesComboBox.SelectedIndex = 0;
            }

            OKButton.Enabled = (m_SelectedDevice != null);
        }

        private void OnManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded)
        {
            if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_RefreshDevices)
            {
                OKButton.Enabled = true;
                Cancel_Button.Enabled = true;
                RefreshButton.Enabled = true;
                DevicesComboBox.Enabled = true;
                Cursor = Cursors.Default;

                RefreshDeviceComboBox();
            }
        }

        public Device SelectedDevice
        {
            get { return m_SelectedDevice; }
        }

        private Device m_SelectedDevice = null;
    }
}
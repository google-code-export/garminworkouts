using System.Diagnostics;
using System.Windows.Forms;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
{
    partial class SelectDeviceDialog : Form
    {
        public SelectDeviceDialog(GarminDeviceManager manager)
        {
            InitializeComponent();

            m_Manager = manager;
            m_Manager.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnManagerTaskCompleted);

            RefreshDeviceComboBox();
        }

        private void RefreshButton_Click(object sender, System.EventArgs e)
        {
            OKButton.Enabled = false;
            Cancel_Button.Enabled = false;
            RefreshButton.Enabled = false;
            DevicesComboBox.Enabled = false;
            Cursor = Cursors.WaitCursor;

            m_Manager.RefreshDevices();
        }

        private void DevicesComboBox_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            m_SelectedDevice = m_Manager.Devices[DevicesComboBox.SelectedIndex];
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
            for (int i = 0; i < m_Manager.Devices.Count; ++i)
            {
                DevicesComboBox.Items.Add(m_Manager.Devices[i].DisplayName);
            }

            if (m_Manager.Devices.Count > 0)
            {
                m_SelectedDevice = m_Manager.Devices[0];
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

        private GarminDeviceManager m_Manager;
        private Device m_SelectedDevice = null;
    }
}
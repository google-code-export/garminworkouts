using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.View
{
    partial class SelectDeviceDialog : Form
    {
        public SelectDeviceDialog()
        {
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            CultureInfo UICulture = currentView.UICulture;

            InitializeComponent();

            GarminDeviceManager.GetInstance().TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnManagerTaskCompleted);

            this.Text = m_ResourceManager.GetString("SelectDeviceText", UICulture);
            IntroLabel.Text = m_ResourceManager.GetString("SelectDeviceIntroText", UICulture);
            RefreshButton.Text = m_ResourceManager.GetString("RefreshButtonText", UICulture);
            Cancel_Button.Text = m_ResourceManager.GetString("CancelButtonText", UICulture);
            OKButton.Text = m_ResourceManager.GetString("OKButtonText", UICulture);

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

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}
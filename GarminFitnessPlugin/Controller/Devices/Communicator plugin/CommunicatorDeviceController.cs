using System;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminFitnessPlugin.Data;
using System.IO;

namespace GarminFitnessPlugin.Controller
{
    class CommunicatorDeviceController : IGarminDeviceController
    {
        public CommunicatorDeviceController()
        {
            m_Controller.ReadyChanged += new EventHandler(OnControllerReadyChanged);
            m_Controller.FinishFindDevices += new GarminDeviceControl.FinishFindDevicesEventHandler(OnControllerFinishFindDevices);
        }

        ~CommunicatorDeviceController()
        {
            m_Controller.ReadyChanged -= new EventHandler(OnControllerReadyChanged);
            m_Controller.FinishFindDevices -= new GarminDeviceControl.FinishFindDevicesEventHandler(OnControllerFinishFindDevices);
        }

        private void OnControllerReadyChanged(object sender, EventArgs e)
        {
            Logger.Instance.LogText("Comm. : Controller ready");

            if (InitializationCompleted != null)
            {
                InitializationCompleted(this, m_Controller.Ready && m_Controller.Initialize());
            }
        }

        private void OnControllerFinishFindDevices(object sender, GarminDeviceControl.FinishFindDevicesEventArgs e)
        {
            Logger.Instance.LogText(String.Format("Comm. : Devices found({0})", e.Devices.GetLength(0)));

            m_Devices.Clear();

            foreach(Device currentDevice in e.Devices)
            {
                m_Devices.Add(new CommunicatorDevice(this, currentDevice));
            }

            if (FindDevicesCompleted != null)
            {
                FindDevicesCompleted(this, true);
            }
        }

#region IGarminDeviceController Members

        public bool IsInitialized
        {
            get { return m_Controller.Ready && m_Controller.Initialized; }
        }

        public IList<IGarminDevice> Devices
        {
            get { return m_Devices; }
        }

        public void Initialize()
        {
            Logger.Instance.LogText("Comm. : Initializing");

            if (m_Controller.Ready)
            {
                if (!m_Controller.Initialized)
                {
                    if (InitializationCompleted != null)
                    {
                        InitializationCompleted(this, m_Controller.Initialize());
                    }
                }
                else
                {
                    if (InitializationCompleted != null)
                    {
                        InitializationCompleted(this, true);
                    }
                }
            }
            else
            {
                // Do nothing, device is not yet ready to start
            }
        }

        public void FindDevices()
        {
            Logger.Instance.LogText("Comm. : Finding devices");

            m_Controller.FindDevices();
        }

        public event DeviceControllerOperationCompletedEventHandler InitializationCompleted;
        public event DeviceControllerOperationCompletedEventHandler FindDevicesCompleted;

#endregion

        public GarminDeviceControl CommunicatorController
        {
            get { return m_Controller; }
        }

        private GarminDeviceControl m_Controller = new GarminDeviceControl();
        private List<IGarminDevice> m_Devices = new List<IGarminDevice>();
    }
}

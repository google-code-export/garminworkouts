using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class GarminFitnessCommunicatorDeviceController : IGarminDeviceController
    {
        public GarminFitnessCommunicatorDeviceController()
        {
            m_CommunicatorBridge = new GarminFitnessCommunicatorBridge();

            m_CommunicatorBridge.ControllerReady += new EventHandler(OnCommunicatorBridgeControllerReady);
            m_CommunicatorBridge.InitializeCompleted += new EventHandler<GarminFitnessCommunicatorBridge.InitializeCompletedEventArgs>(OnCommunicatorBridgeInitializeCompleted);
            m_CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnCommunicatorBridgeExceptionTriggered);
            m_CommunicatorBridge.FinishFindDevices += new EventHandler<GarminFitnessCommunicatorBridge.FinishFindDevicesEventArgs>(OnCommunicatorBridgeFinishFindDevices);

            m_CommunicatorBridge.Start();
        }

        private void OnCommunicatorBridgeExceptionTriggered(object sender, GarminFitnessCommunicatorBridge.ExceptionEventArgs e)
        {
            if (ExceptionTriggered != null)
            {
                ExceptionTriggered(sender, e);
            }
        }

        void OnCommunicatorBridgeControllerReady(object sender, EventArgs e)
        {
            Logger.Instance.LogText("Bridge controller ready");

            if (m_CommunicatorBridge.IsControllerReady && m_PendingInitialize)
            {
                Logger.Instance.LogText("Controller pending initialize start");

                m_CommunicatorBridge.Initialize();
                m_PendingInitialize = false;
            }
        }

        private void OnCommunicatorBridgeInitializeCompleted(object sender, GarminFitnessCommunicatorBridge.InitializeCompletedEventArgs e)
        {
            Logger.Instance.LogText("Bridge initialization complete");

            m_Initialized = e.Success;

            if (InitializationCompleted != null)
            {
                InitializationCompleted(this, IsInitialized, e.ErrorText);
            }
        }

        private void OnCommunicatorBridgeFinishFindDevices(object sender, GarminFitnessCommunicatorBridge.FinishFindDevicesEventArgs e)
        {
            XmlDocument devicesDocument = new XmlDocument();

            foreach (IGarminDevice device in m_Devices)
            {
                device.Dispose();
            }
            m_Devices.Clear();

            try
            {
                devicesDocument.LoadXml(e.DevicesString);

                foreach (XmlNode currentDeviceNode in devicesDocument.FirstChild.ChildNodes)
                {
                    GarminFitnessCommunicatorDevice newDevice = new GarminFitnessCommunicatorDevice(this, currentDeviceNode);

                    // Ignore & dispose invalid devices
                    if (newDevice.SoftwareVersion.Equals("0") ||
                        newDevice.DeviceId.Equals("4294967295"))    // 0xFFFFFFF
                    {
                        newDevice.Dispose();
                    }
                    else
                    {
                        m_Devices.Add(newDevice);
                    }
                }
            }
            catch
            {
                if (FindDevicesCompleted != null)
                {
                    FindDevicesCompleted(this, false, String.Empty);
                }
            }

            if (FindDevicesCompleted != null)
            {
                FindDevicesCompleted(this, true, String.Empty);
            }
        }

#region IGarminDeviceController Members

        public bool IsInitialized
        {
            get { return m_Initialized; }
        }

        public IList<IGarminDevice> Devices
        {
            get { return m_Devices; }
        }

        public void Initialize()
        {
            if (!m_CommunicatorBridge.IsControllerReady)
            {
                Logger.Instance.LogText("Controller initialize queued");
                m_PendingInitialize = true;
            }
            else
            {
                Logger.Instance.LogText("Controller initialize start");

                m_Initialized = false;
                m_CommunicatorBridge.Initialize();
            }
        }

        public void FindDevices()
        {
            if (IsInitialized)
            {
                m_CommunicatorBridge.FindDevices();
            }
        }

        public event DeviceControllerOperationCompletedEventHandler InitializationCompleted;
        public event DeviceControllerOperationCompletedEventHandler FindDevicesCompleted;
        public event EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs> ExceptionTriggered;

#endregion

        public GarminFitnessCommunicatorBridge CommunicatorBridge
        {
            get { return m_CommunicatorBridge; }
        }


        private GarminFitnessCommunicatorBridge m_CommunicatorBridge = null;
        private List<IGarminDevice> m_Devices = new List<IGarminDevice>();
        private bool m_PendingInitialize = false;
        private bool m_Initialized = false;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class GarXFaceDeviceController : IGarminDeviceController
    {
        public GarXFaceDeviceController()
        {
        }

#region IGarminDeviceController Members

        public bool IsInitialized
        {
            get { return true; }
        }

        public IList<IGarminDevice> Devices
        {
            get { return m_Devices; }
        }

        public void Initialize()
        {
            if (InitializationCompleted != null)
            {
                InitializationCompleted(this, true);
            }
        }

        public void FindDevices()
        {
            GarXFaceNet._UsbDeviceNameList usbDevices = new GarXFaceNet._UsbDeviceNameList();

            m_Devices.Clear();
            for (UInt32 i = 0; i < usbDevices.GetCount(); ++i)
            {
                String name = usbDevices.GetAtIndex(i);

                m_Devices.Add(new GarXFaceDevice(this, new GarXFaceNet._GpsUsbDevice(name)));
            }

            if (FindDevicesCompleted != null)
            {
                FindDevicesCompleted(this, true);
            }
        }

        public event DeviceControllerOperationCompletedEventHandler InitializationCompleted;
        public event DeviceControllerOperationCompletedEventHandler FindDevicesCompleted;

#endregion

        public GarXFaceNet._Gps GarXFaceController
        {
            get { return m_Controller; }
        }

        private GarXFaceNet._Gps m_Controller = new GarXFaceNet._Gps();
        private List<IGarminDevice> m_Devices = new List<IGarminDevice>();
    }
}

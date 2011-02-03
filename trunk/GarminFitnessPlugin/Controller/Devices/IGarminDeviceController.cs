using System;
using System.Collections.Generic;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    delegate void DeviceControllerOperationCompletedEventHandler(IGarminDeviceController controller, Boolean succeeded);

    interface IGarminDeviceController
    {
        Boolean IsInitialized { get; }
        IList<IGarminDevice> Devices { get; }

        void Initialize();
        void FindDevices();

        event DeviceControllerOperationCompletedEventHandler InitializationCompleted;
        event DeviceControllerOperationCompletedEventHandler FindDevicesCompleted;
        event EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs> ExceptionTriggered;
    }
}

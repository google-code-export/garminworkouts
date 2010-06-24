using System;
using System.Collections.Generic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public enum DeviceOperations
    {
        Operation_Idle = 0,
        Operation_ReadWorkout,
        Operation_WriteWorkout,
        Operation_ReadProfile,
        Operation_WriteProfile
    }

    delegate void DeviceOperationCompletedEventHandler(IGarminDevice device, DeviceOperations operation, Boolean succeeded);

    interface IGarminDevice
    {
        void CancelWrite();
        void CancelRead();

        void WriteWorkouts(List<IWorkout> workouts);
        void ReadWorkouts();
        void WriteProfile(GarminProfile profile);
        void ReadProfile(GarminProfile profile);

        IGarminDeviceController Controller { get; }

        Boolean SupportsReadWorkout { get; }
        Boolean SupportsWriteWorkout { get; }
        Boolean SupportsReadProfile { get; }
        Boolean SupportsWriteProfile { get; }

        String DeviceId { get; }
        String SoftwareVersion { get; }
        String DisplayName { get; }

        event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;
    }
}

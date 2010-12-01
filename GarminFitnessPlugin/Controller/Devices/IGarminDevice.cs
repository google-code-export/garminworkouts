using System;
using System.Collections.Generic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public enum DeviceOperations
    {
        Idle = 0,
        ReadWorkout,
        ReadMassStorageWorkouts,
        ReadFITWorkouts,
        WriteWorkout,
        ReadProfile,
        ReadFITProfile,
        WriteProfile
    }

    delegate void DeviceOperationCompletedEventHandler(IGarminDevice device, DeviceOperations operation, Boolean succeeded);
    delegate void DeviceOperationProgressedEventHandler(IGarminDevice device, DeviceOperations operation, int progress);

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
        Boolean SupportsFITWorkouts { get; }
        Boolean SupportsFITProfile { get; }

        String DeviceId { get; }
        String SoftwareVersion { get; }
        String DisplayName { get; }

        event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        event DeviceOperationProgressedEventHandler OperationProgressed;
        event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;
    }
}

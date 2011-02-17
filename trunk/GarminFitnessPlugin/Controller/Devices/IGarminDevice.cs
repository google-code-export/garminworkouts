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

    delegate void DeviceOperationWillCompleteEventHandler(IGarminDevice device, DeviceOperations operation);
    delegate void DeviceOperationCompletedEventHandler(IGarminDevice device, DeviceOperations operation, Boolean succeeded, String failureMessage);
    delegate void DeviceOperationProgressedEventHandler(IGarminDevice device, DeviceOperations operation, int progress);

    interface IGarminDevice : IDisposable
    {
        void Initialize();
        void Uninitialize();

        void CancelWrite(String reason);
        void CancelRead(String reason);

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

        event DeviceOperationWillCompleteEventHandler OperationWillComplete;
        event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        event DeviceOperationProgressedEventHandler OperationProgressed;
        event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;
    }
}

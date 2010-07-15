using System;
using System.Collections.Generic;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class GarXFaceDevice : IGarminDevice
    {
        enum WorkoutTransferSubSteps
        {
            Step_None = 0,
            Step_WorkoutList,
            Step_WorkoutOccuranceList
        }

        public GarXFaceDevice(GarXFaceDeviceController controller, GarXFaceNet._GpsDevice device)
        {
            m_Controller = controller;
            m_Device = device;
        }

#region IGarminDevice Members

        public void CancelWrite()
        {
        }

        public void CancelRead()
        {
        }

        public void WriteWorkouts(List<IWorkout> workouts)
        {
            Logger.Instance.LogText(String.Format("GarXFace : Writing workouts (%i)", workouts.Count));

            int result;
            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            Logger.Instance.LogText("GarXFace : Opening");
            controller.Open(m_Device);
            Logger.Instance.LogText("GarXFace : Open");
            controller.GetWorkoutList().Clear();
            controller.GetWorkoutOccuranceList().Clear();
            Logger.Instance.LogText("GarXFace : Lists cleared");

            foreach (IWorkout workout in workouts)
            {
                Logger.Instance.LogText(String.Format("GarXFace : Writing workout {0)", workout.Name));

                workout.Serialize(controller.GetWorkoutList());
                workout.SerializeOccurances(controller.GetWorkoutOccuranceList());
            }

            m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;

            Logger.Instance.LogText("GarXFace : Transfer workouts");
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutList;
            result = controller.TxWorkoutList();
            Logger.Instance.LogText(String.Format("GarXFace : Transfer workouts result = %i", result));

            Logger.Instance.LogText("GarXFace : Transfer workouts occurances");
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutOccuranceList;
            result = controller.TxWorkoutOccuranceList();
            Logger.Instance.LogText(String.Format("GarXFace : Transfer workouts occurances result = %i", result));

            Logger.Instance.LogText("GarXFace : Write workouts completed");

            // Completed
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
            controller.Close();
            if (WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, m_CurrentOperation, true);
            }

            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

        public void ReadWorkouts()
        {
            Logger.Instance.LogText("GarXFace : Reading workouts");

            int result;
            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            controller.Open(m_Device);

            m_CurrentOperation = DeviceOperations.Operation_ReadWorkout;

            Logger.Instance.LogText("GarXFace : Reading workouts");
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutList;
            result = controller.RxWorkoutList();
            Logger.Instance.LogText(String.Format("GarXFace : Reading workouts result = %i", result));

            Logger.Instance.LogText("GarXFace : Reading workouts occurances");
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutOccuranceList;
            result = controller.RxWorkoutOccuranceList();
            Logger.Instance.LogText(String.Format("GarXFace : Reading workouts occurances result = %i", result));

            Logger.Instance.LogText(String.Format("GarXFace : Read workout completed({0})", controller.GetWorkoutList().GetCount()));

            for (UInt32 i = 0; i < controller.GetWorkoutList().GetCount(); ++i)
            {
                GarXFaceNet._Workout workout = controller.GetWorkoutList().GetAtIndex(i);

                WorkoutImporter.ImportWorkout(workout, controller.GetWorkoutOccuranceList());

                Logger.Instance.LogText(String.Format("GarXFace : Read workout {0}", workout.GetName()));
            }

            Logger.Instance.LogText("GarXFace : Write workouts completed");

            // Completed
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
            controller.Close();

            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, m_CurrentOperation, true);
            }

            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

        public void WriteProfile(GarminProfile profile)
        {
            Logger.Instance.LogText("GarXFace : Writing profile");

            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            controller.Open(m_Device);

            profile.Serialize(controller.GetFitnessUserProfile());

            m_CurrentOperation = DeviceOperations.Operation_WriteProfile;
            controller.TxFitnessUserProfile();
            controller.Close();

            if (WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, m_CurrentOperation, true);
            }

            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

        public void ReadProfile(GarminProfile profile)
        {
            Logger.Instance.LogText("GarXFace : Reading profile");

            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            controller.Open(m_Device);
            m_CurrentOperation = DeviceOperations.Operation_ReadProfile;
            controller.RxFitnessUserProfile();

            GarminProfileManager.Instance.UserProfile.Deserialize(controller.GetFitnessUserProfile());

            controller.Close();

            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, m_CurrentOperation, true);
            }

            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

        public IGarminDeviceController Controller
        {
            get { return m_Controller; }
        }

        public bool SupportsReadWorkout
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                Int16 workoutProtocol = controller.GetProductData().GetProtocolCapabilities().GetWorkoutProtocol();

                Logger.Instance.LogText(String.Format("GarXFace : Workout protocol id={0}", workoutProtocol));

                return workoutProtocol == 1002 || workoutProtocol == 1008;
            }
        }

        public bool SupportsWriteWorkout
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                Int16 workoutProtocol = controller.GetProductData().GetProtocolCapabilities().GetWorkoutProtocol();

                Logger.Instance.LogText(String.Format("GarXFace : Workout protocol id={0}", workoutProtocol));

                return workoutProtocol == 1002 || workoutProtocol == 1008;
            }
        }

        public bool SupportsReadProfile
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                Int16 profileProtocol = controller.GetProductData().GetProtocolCapabilities().GetFitnessUserProfileProtocol();

                Logger.Instance.LogText(String.Format("GarXFace : Profile protocol id={0}", profileProtocol));

                return profileProtocol == 1004;
            }
        }

        public bool SupportsWriteProfile
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                Int16 profileProtocol = controller.GetProductData().GetProtocolCapabilities().GetFitnessUserProfileProtocol();

                Logger.Instance.LogText(String.Format("GarXFace : Profile protocol id={0}", profileProtocol));

                return profileProtocol == 1004;
            }
        }

        public string DeviceId
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                return controller.GetProductData().GetProductID().ToString();
            }
        }

        public string SoftwareVersion
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                return controller.GetProductData().GetSwVersion().ToString();
            }
        }

        public string DisplayName
        {
            get
            {
                GarXFaceNet._Gps controller = new GarXFaceNet._Gps();

                controller.Close();
                controller.Open(m_Device);

                controller.RxProductData();
                return controller.GetProductData().GetDescription();
            }
        }

        public event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        public event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;

#endregion

        private GarXFaceDeviceController m_Controller = null;
        private GarXFaceNet._GpsDevice m_Device = null;
        private DeviceOperations m_CurrentOperation = DeviceOperations.Operation_Idle;
        private WorkoutTransferSubSteps m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
    }
}

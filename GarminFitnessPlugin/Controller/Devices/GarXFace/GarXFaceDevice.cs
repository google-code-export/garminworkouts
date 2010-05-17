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

        void OnTransferProgressChanged(bool inProgress, uint progressTotalPackets, uint progressCurrentPacket, bool completed)
        {
            if (!inProgress)
            {
                // Tranfer completed
                switch (m_CurrentOperation)
                {
                    case DeviceOperations.Operation_ReadWorkout:
                        {
                            if (m_WorkoutSubStep == WorkoutTransferSubSteps.Step_WorkoutList)
                            {
                                m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutOccuranceList;
                                m_Controller.GarXFaceController.TxWorkoutOccuranceList(m_OperationProgressNotifier);
                            }
                            else
                            {
                                for (UInt32 i = 0; i < m_Controller.GarXFaceController.GetWorkoutList().GetCount(); ++i)
                                {
                                    GarXFaceNet._Workout workout = m_Controller.GarXFaceController.GetWorkoutList().GetAtIndex(i);

                                    WorkoutImporter.ImportWorkout(workout, m_Controller.GarXFaceController.GetWorkoutOccuranceList());
                                }

                                m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
                                m_Controller.GarXFaceController.Close();
                                if (ReadFromDeviceCompleted != null)
                                {
                                    ReadFromDeviceCompleted(this, m_CurrentOperation, completed);
                                }

                                m_OperationProgressNotifier.TransferProgressChanged -= new GarXFaceNet._ProgressNotifier.ProgressChangedEventHandler(OnTransferProgressChanged);
                                m_CurrentOperation = DeviceOperations.Operation_Idle;
                            }
                            break;
                        }
                    case DeviceOperations.Operation_WriteWorkout:
                        {
                            if (m_WorkoutSubStep == WorkoutTransferSubSteps.Step_WorkoutList)
                            {
                                // Send occurances
                                m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutOccuranceList;
                                m_Controller.GarXFaceController.TxWorkoutOccuranceList(m_OperationProgressNotifier);
                            }
                            else
                            {
                                // Completed
                                m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
                                m_Controller.GarXFaceController.Close();
                                if (WriteToDeviceCompleted != null)
                                {
                                    WriteToDeviceCompleted(this, m_CurrentOperation, completed);
                                }

                                m_OperationProgressNotifier.TransferProgressChanged -= new GarXFaceNet._ProgressNotifier.ProgressChangedEventHandler(OnTransferProgressChanged);
                                m_CurrentOperation = DeviceOperations.Operation_Idle;
                            }
                            break;
                        }
                }
            }
        }

#region IGarminDevice Members

        public void CancelWrite()
        {
            m_OperationProgressNotifier.SetAbort(true);
        }

        public void CancelRead()
        {
            m_OperationProgressNotifier.SetAbort(true);
        }

        public void WriteWorkouts(List<Workout> workouts)
        {
            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            controller.Open(m_Device);

            controller.GetWorkoutList().Clear();
            controller.GetWorkoutOccuranceList().Clear();
            foreach (Workout workout in workouts)
            {
                workout.Serialize(m_Controller.GarXFaceController.GetWorkoutList());
                workout.SerializeOccurances(m_Controller.GarXFaceController.GetWorkoutOccuranceList());
            }

            m_OperationProgressNotifier.TransferProgressChanged += new GarXFaceNet._ProgressNotifier.ProgressChangedEventHandler(OnTransferProgressChanged);
            m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutList;
            controller.TxWorkoutList(m_OperationProgressNotifier);
        }

        public void ReadWorkouts()
        {
            GarXFaceNet._Gps controller = m_Controller.GarXFaceController;

            controller.Open(m_Device);

            m_OperationProgressNotifier.TransferProgressChanged += new GarXFaceNet._ProgressNotifier.ProgressChangedEventHandler(OnTransferProgressChanged);
            m_CurrentOperation = DeviceOperations.Operation_ReadWorkout;
            m_WorkoutSubStep = WorkoutTransferSubSteps.Step_WorkoutList;
            controller.RxWorkoutList(m_OperationProgressNotifier);
        }

        public void WriteProfile(GarminProfile profile)
        {
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
        private GarXFaceNet._ProgressNotifier m_OperationProgressNotifier = new GarXFaceNet._ProgressNotifier();
        private DeviceOperations m_CurrentOperation = DeviceOperations.Operation_Idle;
        private WorkoutTransferSubSteps m_WorkoutSubStep = WorkoutTransferSubSteps.Step_None;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class CommunicatorDevice : IGarminDevice
    {
        public CommunicatorDevice(CommunicatorDeviceController controller, Device device)
        {
            m_Controller = controller;
            m_Device = device;
        }

        void OnControllerFinishReadFromDevice(object sender, GarminDeviceControl.TransferCompleteEventArgs e)
        {
            bool success = e.Success;

            switch(m_CurrentOperation)
            {
                case DeviceOperations.Operation_ReadProfile:
                    {
                        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(e.XmlData));

                        success = ProfileImporter.ImportProfile(stream);
                        stream.Close();

                        break;
                    }
                case DeviceOperations.Operation_ReadWorkout:
                    {
                        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(e.XmlData));

                        success = WorkoutImporter.ImportWorkout(stream);
                        stream.Close();
                        
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        success = false;
                        break;
                    }
            }

            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, m_CurrentOperation, success);
            }

            m_Controller.CommunicatorController.FinishReadFromDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);
            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

        void OnControllerFinishWriteToDevice(object sender, GarminDeviceControl.TransferCompleteEventArgs e)
        {
            bool success = e.Success;

            switch (m_CurrentOperation)
            {
                case DeviceOperations.Operation_WriteProfile:
                    {
                        break;
                    }
                case DeviceOperations.Operation_WriteWorkout:
                    {
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        success = false;
                        break;
                    }
            }

            if (WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, m_CurrentOperation, success);
            }

            m_Controller.CommunicatorController.FinishWriteToDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
            m_CurrentOperation = DeviceOperations.Operation_Idle;
        }

#region IDevice Members

        public void CancelWrite()
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorController.FireFinishWriteToDevice(false, "");
        }

        public void CancelRead()
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorController.FireFinishReadFromDevice(false, "");
        }

        public void WriteWorkouts(List<Workout> workouts)
        {
            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            string fileName;

            if (workouts.Count == 1)
            {
                fileName = Utils.GetWorkoutFilename(workouts[0]);
            }
            else
            {
                fileName = "Workouts.tcx";
            }
            MemoryStream textStream = new MemoryStream();

            WorkoutExporter.ExportWorkout(workouts, textStream);
            string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

            m_Controller.CommunicatorController.FinishWriteToDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
            m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;
            m_Controller.CommunicatorController.WriteWorkouts(m_Device,
                                                              xmlCode,
                                                              fileName);
        }

        public void ReadWorkouts()
        {
            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorController.FinishReadFromDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);
            m_CurrentOperation = DeviceOperations.Operation_ReadWorkout;
            m_Controller.CommunicatorController.ReadWktWorkouts(m_Device);
        }

        public void WriteProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            string fileName = "Profile.tcx";
            MemoryStream textStream = new MemoryStream();

            ProfileExporter.ExportProfile(profile, textStream);
            string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

            m_Controller.CommunicatorController.FinishWriteToDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
            m_CurrentOperation = DeviceOperations.Operation_WriteProfile;
            m_Controller.CommunicatorController.WriteUserProfile(m_Device,
                                                                 xmlCode,
                                                                 fileName);
        }

        public void ReadProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorController.FinishReadFromDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);
            m_CurrentOperation = DeviceOperations.Operation_ReadProfile;
            m_Controller.CommunicatorController.ReadTcxUserProfile(m_Device);
        }

        public IGarminDeviceController Controller
        {
            get { return m_Controller; }
        }

        public bool SupportsReadWorkout
        {
            get
            {
                bool isSupported = false;

                // Check supported data types
                foreach (DataType currentDataType in m_Device.DataTypes.DataType)
                {
                    if (currentDataType.Type.Equals("FitnessWorkouts"))
                    {
                        isSupported = currentDataType.CanRead;
                        break;
                    }
                }

                return isSupported;
            }
        }

        public bool SupportsWriteWorkout
        {
            get
            {
                bool isSupported = false;

                // Check supported data types
                foreach (DataType currentDataType in m_Device.DataTypes.DataType)
                {
                    if (currentDataType.Type.Equals("FitnessWorkouts"))
                    {
                        isSupported = currentDataType.CanWrite;
                        break;
                    }
                }

                return isSupported;
            }
        }

        public bool SupportsReadProfile
        {
            get
            {
                bool isSupported = false;

                // Check supported data types
                foreach (DataType currentDataType in m_Device.DataTypes.DataType)
                {
                    if (currentDataType.Type.Equals("FitnessUserProfile"))
                    {
                        isSupported = currentDataType.CanRead;
                        break;
                    }
                }

                return isSupported;
            }
        }

        public bool SupportsWriteProfile
        {
            get
            {
                bool isSupported = false;

                // Check supported data types
                foreach (DataType currentDataType in m_Device.DataTypes.DataType)
                {
                    if (currentDataType.Type.Equals("FitnessUserProfile"))
                    {
                        isSupported = currentDataType.CanWrite;
                        break;
                    }
                }

                return isSupported;
            }
        }

        public string DeviceId
        {
            get { return m_Device.Id; }
        }

        public string SoftwareVersion
        {
            get { return m_Device.SoftwareVersion; }
        }

        public string DisplayName
        {
            get { return m_Device.DisplayName; }
        }

        public event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        public event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;

#endregion

        private CommunicatorDeviceController m_Controller = null;
        private Device m_Device = null;
        private DeviceOperations m_CurrentOperation = DeviceOperations.Operation_Idle;
    }
}

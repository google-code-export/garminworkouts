using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class GarminFitnessCommunicatorDevice : IGarminDevice
    {
        public GarminFitnessCommunicatorDevice(GarminFitnessCommunicatorDeviceController controller, XmlNode deviceXml)
        {
            m_Controller = controller;

            foreach (XmlAttribute attribute in deviceXml.Attributes)
            {
                if (attribute.Name.Equals("Number"))
                {
                    m_DeviceNumber = Int32.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("DisplayName"))
                {
                    m_DisplayName = attribute.Value;
                }
                else if (attribute.Name.Equals("Id"))
                {
                    m_Id = attribute.Value;
                }
                else if (attribute.Name.Equals("SoftwareVersion"))
                {
                    m_SoftwareVersion = attribute.Value;
                }
                else if (attribute.Name.Equals("SupportReadWorkout"))
                {
                    m_SupportsReadWorkout = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("SupportWriteWorkout"))
                {
                    m_SupportsWriteWorkout = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("WorkoutFileTransferPath"))
                {
                    m_WorkoutFileTransferPath = attribute.Value;
                }
                else if (attribute.Name.Equals("SupportReadProfile"))
                {
                    m_SupportsReadProfile = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("SupportWriteProfile"))
                {
                    m_SupportsWriteProfile = Boolean.Parse(attribute.Value);
                }
            }
        }

        void OnBridgeExceptionTriggered(object sender, GarminFitnessCommunicatorBridge.ExceptionEventArgs e)
        {
            switch(m_CurrentOperation)
            {
                case DeviceOperations.Operation_ReadProfile:
                case DeviceOperations.Operation_ReadWorkout:
                {
                    CancelRead();
                    break;
                }
                case DeviceOperations.Operation_WriteProfile:
                case DeviceOperations.Operation_WriteWorkout:
                {
                    CancelWrite();
                    break;
                }
            }
        }

        private void OnBridgeReadFromDeviceCompleted(object sender, GarminFitnessCommunicatorBridge.TranferCompletedEventArgs e)
        {
            bool success = e.Success;
            bool operationCompleted = true;
            DeviceOperations lastOperation = m_CurrentOperation;

            switch(m_CurrentOperation)
            {
                case DeviceOperations.Operation_ReadProfile:
                {
                    Logger.Instance.LogText("Comm. : Profile read");

                    if (success)
                    {
                        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(e.DataString));

                        success = ProfileImporter.ImportProfile(stream);
                        stream.Close();
                    }

                    break;
                }
                case DeviceOperations.Operation_ReadMassStorageWorkouts:
                {
                    Logger.Instance.LogText("Comm. : Mass storage workouts read");

                    if (success)
                    {
                        if (OperationProgressed != null)
                        {
                            OperationProgressed(this, m_CurrentOperation, ++m_WorkoutsReadCount);
                        }

                        success = ImportWorkoutFileResult(e.DataString);

                        if (success &&
                            m_WorkoutFilesToDownload != null &&
                            m_WorkoutFilesToDownload.Count > 0)
                        {
                            String workoutFilename = m_WorkoutFilesToDownload[0];

                            operationCompleted = false;
                            m_WorkoutFilesToDownload.RemoveAt(0);
                            m_Controller.CommunicatorBridge.GetBinaryFile(workoutFilename);
                        }
                    }
                    else
                    {
                        m_WorkoutFilesToDownload.Clear();
                    }

                    break;
                }
                case DeviceOperations.Operation_ReadWorkout:
                {
                    Logger.Instance.LogText("Comm. : Workouts read");

                    if (success)
                    {
                        ImportWorkoutFileResult(e.DataString);
                    }

                    break;
                }
                default:
                {
                    Debug.Assert(false);
                    success = false;
                    break;
                }
            }

            if (operationCompleted)
            {
                m_Controller.CommunicatorBridge.ExceptionTriggered -= new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                m_Controller.CommunicatorBridge.ReadFromDeviceCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
                m_CurrentOperation = DeviceOperations.Operation_Idle;

                if (ReadFromDeviceCompleted != null)
                {
                    ReadFromDeviceCompleted(this, lastOperation, success);
                }
            }
        }

        private void OnBridgeWriteToDeviceCompleted(object sender, GarminFitnessCommunicatorBridge.TranferCompletedEventArgs e)
        {
            bool success = e.Success;
            DeviceOperations lastOperation = m_CurrentOperation;

            switch (m_CurrentOperation)
            {
                case DeviceOperations.Operation_WriteProfile:
                    {
                        Logger.Instance.LogText("Comm. : Profile written");
                        break;
                    }
                case DeviceOperations.Operation_WriteWorkout:
                    {
                        Logger.Instance.LogText("Comm. : Workouts written");
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        success = false;
                        break;
                    }
            }

            m_Controller.CommunicatorBridge.ExceptionTriggered -= new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
            m_CurrentOperation = DeviceOperations.Operation_Idle;

            if (WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, lastOperation, success);
            }
        }

#region IDevice Members

        private bool ImportWorkoutFileResult(String workoutsXml)
        {
            try
            {
                bool result = false;

                // UU encoduded base 64, decode first
                if (workoutsXml.StartsWith("begin-base64"))
                {
                    Byte[] decodedBytes;

                    workoutsXml = workoutsXml.Substring(workoutsXml.IndexOf('\n') + 1);
                    workoutsXml = workoutsXml.Substring(0, workoutsXml.LastIndexOf("===="));

                    decodedBytes = System.Convert.FromBase64String(workoutsXml);
                    workoutsXml = Encoding.UTF8.GetString(decodedBytes);
                }

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(workoutsXml));

                result = WorkoutImporter.ImportWorkout(stream);
                stream.Close();

                return result;
            }
            catch
            {
                return false;
            }
        }

        public void CancelWrite()
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.CancelWriteToDevice();
        }

        public void CancelRead()
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.CancelReadFromDevice();
        }

        public void WriteWorkout(IWorkout workout)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
            m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsWorkoutMassStorageTransfer)
            {
                List<String> filenames = new List<String>();
                string fileDestination = typeof(PluginMain).Assembly.Location;

                fileDestination = fileDestination.Substring(0, fileDestination.LastIndexOf('\\'));
                fileDestination = fileDestination.Substring(0, fileDestination.LastIndexOf('\\'));
                fileDestination = fileDestination  + "\\Communicator\\temp\\";

                Directory.CreateDirectory(fileDestination);

                string fileName = Utils.GetWorkoutFilename(workout);
                FileStream fileStream = File.Create(fileDestination + fileName);

                WorkoutExporter.ExportWorkout(workout, fileStream);
                fileStream.Close();
                filenames.Add(fileName);

                m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;

                m_Controller.CommunicatorBridge.WriteWorkoutsToFile(filenames, WorkoutFileTransferPath);
            }
            else
            {
                string fileName = Utils.GetWorkoutFilename(workout);
                MemoryStream textStream = new MemoryStream();

                WorkoutExporter.ExportWorkout(workout, textStream);
                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

                m_Controller.CommunicatorBridge.WriteWorkoutsToFitnessDevice(xmlCode, fileName);
            }
        }

        public void WriteWorkouts(List<IWorkout> workouts)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
            m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsWorkoutMassStorageTransfer)
            {
                List<IWorkout> concreteWorkouts = new List<IWorkout>();
                List<String> filenames = new List<String>();
                string fileDestination = typeof(PluginMain).Assembly.Location;

                fileDestination = fileDestination.Substring(0, fileDestination.LastIndexOf('\\'));
                fileDestination = fileDestination.Substring(0, fileDestination.LastIndexOf('\\'));
                fileDestination = fileDestination + "\\Communicator\\temp\\";

                Directory.CreateDirectory(fileDestination);

                foreach (IWorkout currentWorkout in workouts)
                {
                    if (currentWorkout.GetSplitPartsCount() == 1)
                    {
                        concreteWorkouts.Add(currentWorkout);
                    }
                    else
                    {
                        List<WorkoutPart> parts = currentWorkout.SplitInSeperateParts();

                        foreach (WorkoutPart part in parts)
                        {
                            concreteWorkouts.Add(part);
                        }
                    }
                }

                foreach (IWorkout concreteWorkout in concreteWorkouts)
                {
                    string fileName = Utils.GetWorkoutFilename(concreteWorkout);
                    FileStream fileStream = File.Create(fileDestination + fileName);

                    WorkoutExporter.ExportWorkout(concreteWorkout, fileStream);
                    fileStream.Close();
                    filenames.Add(fileName);
                }

                m_CurrentOperation = DeviceOperations.Operation_WriteWorkout;

                m_Controller.CommunicatorBridge.WriteWorkoutsToFile(filenames, WorkoutFileTransferPath);
            }
            else
            {
                string fileName = "Default.tcx";
                MemoryStream textStream = new MemoryStream();

                if (workouts.Count == 1)
                {
                    fileName = Utils.GetWorkoutFilename(workouts[0]);
                }

                WorkoutExporter.ExportWorkouts(workouts, textStream);

                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());
                m_Controller.CommunicatorBridge.WriteWorkoutsToFitnessDevice(xmlCode, fileName);
            }
        }

        public void ReadWorkouts()
        {
            Logger.Instance.LogText("Comm. : Reading workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);
            if (SupportsWorkoutMassStorageTransfer)
            {
                m_CurrentOperation = DeviceOperations.Operation_ReadMassStorageWorkouts;
                m_WorkoutsReadCount = 0;

                m_WorkoutFilesToDownload = m_Controller.CommunicatorBridge.GetWorkoutFiles();

                if (m_WorkoutFilesToDownload != null)
                {
                    String workoutFilename = m_WorkoutFilesToDownload[0];

                    m_WorkoutFilesToDownload.RemoveAt(0);
                    m_Controller.CommunicatorBridge.GetBinaryFile(workoutFilename);
                }
                else
                {
                    m_Controller.CommunicatorBridge.ReadWorkoutsFromFitnessDevice();
                }
            }
            else
            {
                m_CurrentOperation = DeviceOperations.Operation_ReadWorkout;
                m_Controller.CommunicatorBridge.ReadWorkoutsFromFitnessDevice();
            }
        }

        public void WriteProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            string fileName = "Profile.tcx";
            MemoryStream textStream = new MemoryStream();

            ProfileExporter.ExportProfile(profile, textStream);
            string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
            m_CurrentOperation = DeviceOperations.Operation_WriteProfile;

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);
            m_Controller.CommunicatorBridge.WriteProfileToFitnessDevice(xmlCode, fileName);
        }

        public void ReadProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Reading profile");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Operation_Idle);

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
            m_CurrentOperation = DeviceOperations.Operation_ReadProfile;

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);
            m_Controller.CommunicatorBridge.ReadProfileFromFitnessDevice();
        }

        public IGarminDeviceController Controller
        {
            get { return m_Controller; }
        }

        public bool SupportsReadWorkout
        {
            get { return m_SupportsReadWorkout; }
        }

        public bool SupportsWriteWorkout
        {
            get { return m_SupportsWriteWorkout; }
        }

        public bool SupportsWorkoutMassStorageTransfer
        {
            get { return Options.Instance.EnableMassStorageMode &&
                         m_WorkoutFileTransferPath != String.Empty; }
        }

        public String WorkoutFileTransferPath
        {
            get { return m_WorkoutFileTransferPath; }
        }

        public bool SupportsReadProfile
        {
            get { return m_SupportsReadProfile; }
        }

        public bool SupportsWriteProfile
        {
            get { return m_SupportsWriteProfile; }
        }

        public string DeviceId
        {
            get { return m_Id; }
        }

        public string SoftwareVersion
        {
            get { return m_SoftwareVersion; }
        }

        public string DisplayName
        {
            get { return m_DisplayName; }
        }

        public event DeviceOperationCompletedEventHandler WriteToDeviceCompleted;
        public event DeviceOperationCompletedEventHandler ReadFromDeviceCompleted;
        public event DeviceOperationProgressedEventHandler OperationProgressed;

#endregion

        private GarminFitnessCommunicatorDeviceController m_Controller = null;
        private DeviceOperations m_CurrentOperation = DeviceOperations.Operation_Idle;
        private Int32 m_DeviceNumber = -1;
        private string m_DisplayName = String.Empty;
        private string m_Id = string.Empty;
        private string m_SoftwareVersion = string.Empty;
        private bool m_SupportsReadWorkout = false;
        private bool m_SupportsWriteWorkout = false;
        private string m_WorkoutFileTransferPath = String.Empty;
        private bool m_SupportsReadProfile = false;
        private bool m_SupportsWriteProfile = false;
        private List<String> m_WorkoutFilesToDownload = null;
        private int m_WorkoutsReadCount = 0;
    }
}

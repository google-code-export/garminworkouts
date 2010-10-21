using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Controller
{
    class GarminFitnessCommunicatorDevice : IGarminDevice, WorkoutImporter.AsyncImportDelegate
    {
        public GarminFitnessCommunicatorDevice(GarminFitnessCommunicatorDeviceController controller, XmlNode deviceXml)
        {
            m_TempDirectoryLocation = typeof(PluginMain).Assembly.Location;
            m_TempDirectoryLocation = m_TempDirectoryLocation.Substring(0, m_TempDirectoryLocation.LastIndexOf('\\'));
            m_TempDirectoryLocation = m_TempDirectoryLocation.Substring(0, m_TempDirectoryLocation.LastIndexOf('\\'));
            m_TempDirectoryLocation = m_TempDirectoryLocation + "\\Communicator\\temp\\";

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
                else if (attribute.Name.Equals("SupportsFITWorkouts"))
                {
                    m_SupportsFITWorkouts = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("FITWorkoutFileWriteTransferPath"))
                {
                    m_FITWorkoutFileWritePath = attribute.Value;
                }
                else if (attribute.Name.Equals("FITWorkoutFileReadTransferPath"))
                {
                    m_FITWorkoutFileReadPath = attribute.Value;
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
                case DeviceOperations.ReadProfile:
                case DeviceOperations.ReadWorkout:
                {
                    CancelRead();
                    break;
                }
                case DeviceOperations.WriteProfile:
                case DeviceOperations.WriteWorkout:
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
                case DeviceOperations.ReadProfile:
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
                case DeviceOperations.ReadMassStorageWorkouts:
                case DeviceOperations.ReadFITWorkouts:
                {
                    Logger.Instance.LogText("Comm. : Mass storage workout read");

                    if (success)
                    {
                        if (OperationProgressed != null)
                        {
                            OperationProgressed(this, m_CurrentOperation, ++m_WorkoutsReadCount);
                        }

                        if (m_WorkoutFilesToDownload != null)
                        {
                            operationCompleted = false;

                            Debug.Assert(m_WorkoutFilesToDownload.Count > 0);

                            String workoutFilename = m_TempDirectoryLocation + m_WorkoutFilesToDownload[0].Replace('/', '\\');
                            String workoutDirectory = workoutFilename.Substring(0, workoutFilename.LastIndexOf('\\'));
                            Directory.CreateDirectory(workoutDirectory);
                            FileStream newWorkout = File.Create(workoutFilename);
                            Byte[] decodedData = UUDecode(e.DataString);

                            m_WorkoutFilesToDownload.RemoveAt(0);
                            newWorkout.Write(decodedData, 0, decodedData.Length);
                            newWorkout.Close();

                            if (m_WorkoutFilesToDownload.Count > 0)
                            {
                                m_Controller.CommunicatorBridge.GetBinaryFile(m_WorkoutFilesToDownload[0]);
                            }
                            else
                            {
                                m_Controller.CommunicatorBridge.ExceptionTriggered -= new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                                m_Controller.CommunicatorBridge.ReadFromDeviceCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);

                                if (!WorkoutImporter.AsyncImportDirectory(m_TempDirectoryLocation, this))
                                {
                                    operationCompleted = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        m_WorkoutFilesToDownload.Clear();
                    }

                    break;
                }
                case DeviceOperations.ReadWorkout:
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
                m_CurrentOperation = DeviceOperations.Idle;

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
                case DeviceOperations.WriteProfile:
                    {
                        Logger.Instance.LogText("Comm. : Profile written");
                        break;
                    }
                case DeviceOperations.WriteWorkout:
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
            m_CurrentOperation = DeviceOperations.Idle;

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
                    Byte[] decodedBytes = UUDecode(workoutsXml);
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

        private bool ImportFITWorkoutFileResult(String workoutData)
        {
            try
            {
                Byte[] decodedBytes = null;
                bool result = false;

                // UU encoduded base 64, decode first
                if (workoutData.StartsWith("begin-base64"))
                {
                    decodedBytes = UUDecode(workoutData);
                }
                else
                {
                    decodedBytes = Encoding.UTF8.GetBytes(workoutData);
                }

                MemoryStream stream = new MemoryStream(decodedBytes);

                result = WorkoutImporter.ImportWorkoutFromFIT(stream);
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
            Debug.Assert(m_CurrentOperation != DeviceOperations.Idle);

            m_Controller.CommunicatorBridge.CancelWriteToDevice();
        }

        public void CancelRead()
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Idle);

            m_Controller.CommunicatorBridge.CancelReadFromDevice();
        }

        public void WriteWorkouts(List<IWorkout> workouts)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            List<IWorkout> concreteWorkouts = new List<IWorkout>();
            bool exportExtended = false;
            bool exportToFIT = false;

            // Split workouts for the right export method
            foreach (IWorkout currentWorkout in workouts)
            {
                if (currentWorkout.ContainsFITOnlyFeatures)
                {
                    exportToFIT = true;
                }
                else if (currentWorkout.ContainsTCXExtensionFeatures)
                {
                    exportExtended = true;
                }

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

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (!exportExtended && !exportToFIT && !SupportsFITWorkouts)
            {
                // Basic TCX export
                string fileName = "Default.tcx";
                MemoryStream textStream = new MemoryStream();

                if (concreteWorkouts.Count == 1)
                {
                    fileName = Utils.GetWorkoutFilename(concreteWorkouts[0], GarminWorkoutManager.FileFormats.TCX);
                }

                WorkoutExporter.ExportWorkouts(concreteWorkouts, textStream);

                m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
                m_CurrentOperation = DeviceOperations.WriteWorkout;

                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());
                m_Controller.CommunicatorBridge.WriteWorkoutsToFitnessDevice(xmlCode, fileName);
            }
            else
            {
                List<String> filenames = new List<String>();
                string exportPath = String.Empty;

                ClearTempDirectory();

                if (exportToFIT || SupportsFITWorkouts)
                {
                    // FIT export
                    if (SupportsFITWorkouts)
                    {
                        foreach (IWorkout currentWorkout in concreteWorkouts)
                        {
                            string fileName = Utils.GetWorkoutFilename(currentWorkout, GarminWorkoutManager.FileFormats.FIT);
                            FileStream fileStream = File.Create(m_TempDirectoryLocation + fileName);

                            WorkoutExporter.ExportWorkoutToFIT(currentWorkout, fileStream);
                            fileStream.Close();
                            filenames.Add(fileName);
                        }

                        exportPath = FITWorkoutFileWritePath;
                    }
                    else
                    {
                        throw new NoDeviceSupportException(this, GarminFitnessView.GetLocalizedString("FITExportText"));
                    }
                }
                else
                {
                    // Extended TCX export
                    if (SupportsWorkoutMassStorageTransfer)
                    {
                        foreach (IWorkout currentWorkout in concreteWorkouts)
                        {
                            string fileName = Utils.GetWorkoutFilename(currentWorkout, GarminWorkoutManager.FileFormats.TCX);
                            FileStream fileStream = File.Create(m_TempDirectoryLocation + fileName);

                            WorkoutExporter.ExportWorkout(currentWorkout, fileStream);
                            fileStream.Close();
                            filenames.Add(fileName);
                        }

                        exportPath = WorkoutFileTransferPath;
                    }
                    else
                    {
                        throw new NoDeviceSupportException(this, GarminFitnessView.GetLocalizedString("NoMassStorageSupportText"));
                    }
                }

                m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
                m_CurrentOperation = DeviceOperations.WriteWorkout;

                m_Controller.CommunicatorBridge.WriteWorkoutsToFile(filenames, exportPath);
            }
        }

        public void ReadWorkouts()
        {
            Logger.Instance.LogText("Comm. : Reading workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsFITWorkouts)
            {
                m_CurrentOperation = DeviceOperations.ReadFITWorkouts;
                m_WorkoutsReadCount = 0;
                List<String> fitFilesOnDevice;

                ClearTempDirectory();
                m_WorkoutFilesToDownload = new List<String>();
                fitFilesOnDevice = m_Controller.CommunicatorBridge.GetWorkoutFiles();

                foreach(string fitFile in fitFilesOnDevice)
                {
                    if(fitFile.StartsWith(m_FITWorkoutFileReadPath))
                    {
                        m_WorkoutFilesToDownload.Add(fitFile);
                    }
                }

                if (m_WorkoutFilesToDownload.Count > 0)
                {
                    String workoutFilename = m_WorkoutFilesToDownload[0];

                    m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                    m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);

                    m_Controller.CommunicatorBridge.GetBinaryFile(workoutFilename);
                }
                else
                {
                    DeviceOperations lastOperation = m_CurrentOperation;
                    m_CurrentOperation = DeviceOperations.Idle;

                    if (ReadFromDeviceCompleted != null)
                    {
                        ReadFromDeviceCompleted(this, lastOperation, true);
                    }
                }
            }
            else if (SupportsWorkoutMassStorageTransfer)
            {
                m_CurrentOperation = DeviceOperations.ReadMassStorageWorkouts;
                m_WorkoutsReadCount = 0;

                ClearTempDirectory();

                m_WorkoutFilesToDownload = m_Controller.CommunicatorBridge.GetWorkoutFiles();

                if (m_WorkoutFilesToDownload != null && m_WorkoutFilesToDownload.Count > 0)
                {
                    String workoutFilename = m_WorkoutFilesToDownload[0];

                    m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                    m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);

                    m_Controller.CommunicatorBridge.GetBinaryFile(workoutFilename);
                }
                else
                {
                    DeviceOperations lastOperation = m_CurrentOperation;
                    m_CurrentOperation = DeviceOperations.Idle;

                    if (ReadFromDeviceCompleted != null)
                    {
                        ReadFromDeviceCompleted(this, lastOperation, true);
                    }
                }
            }
            else
            {
                m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
                m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
                m_CurrentOperation = DeviceOperations.ReadWorkout;
                m_Controller.CommunicatorBridge.ReadWorkoutsFromFitnessDevice();
            }
        }

        public void WriteProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            string fileName = "Profile.tcx";
            MemoryStream textStream = new MemoryStream();

            ProfileExporter.ExportProfile(profile, textStream);
            string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
            m_CurrentOperation = DeviceOperations.WriteProfile;

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);
            m_Controller.CommunicatorBridge.WriteProfileToFitnessDevice(xmlCode, fileName);
        }

        public void ReadProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Reading profile");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TranferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
            m_CurrentOperation = DeviceOperations.ReadProfile;

            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);
            m_Controller.CommunicatorBridge.ReadProfileFromFitnessDevice();
        }

        private void ClearTempDirectory()
        {
            if (Directory.Exists(m_TempDirectoryLocation))
            {
                Directory.Delete(m_TempDirectoryLocation, true);
            }
            Directory.CreateDirectory(m_TempDirectoryLocation);
        }

        private Byte[] UUDecode(String data)
        {
            Byte[] decodedBytes;

            data = data.Substring(data.IndexOf('\n') + 1);
            data = data.Substring(0, data.LastIndexOf("===="));

            decodedBytes = System.Convert.FromBase64String(data);

            Logger.Instance.LogText(String.Format("UU decoded result = {0}", Encoding.UTF8.GetString(decodedBytes)));

            return decodedBytes;
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

        public bool SupportsFITWorkouts
        {
            get { return m_SupportsFITWorkouts; }
        }

        public String FITWorkoutFileWritePath
        {
            get { return m_FITWorkoutFileWritePath; }
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

#region AsyncImportDelegate Members

        public void OnAsyncImportCompleted(bool success)
        {
            DeviceOperations lastOperation = m_CurrentOperation;
            m_CurrentOperation = DeviceOperations.Idle;

            if (ReadFromDeviceCompleted != null)
            {
                ReadFromDeviceCompleted(this, lastOperation, success);
            }
        }

        public void OnProgressChanged(int progressPercent)
        {
            if (OperationProgressed != null)
            {
                OperationProgressed(this, m_CurrentOperation, m_WorkoutsReadCount + progressPercent);
            }
        }

#endregion

        private GarminFitnessCommunicatorDeviceController m_Controller = null;
        private DeviceOperations m_CurrentOperation = DeviceOperations.Idle;
        private List<String> m_WorkoutFilesToDownload = null;
        private Int32 m_DeviceNumber = -1;
        private readonly string m_TempDirectoryLocation;
        private string m_DisplayName = String.Empty;
        private string m_Id = string.Empty;
        private string m_SoftwareVersion = string.Empty;
        private string m_WorkoutFileTransferPath = String.Empty;
        private string m_FITWorkoutFileWritePath = String.Empty;
        private string m_FITWorkoutFileReadPath = String.Empty;
        private int m_WorkoutsReadCount = 0;
        private bool m_SupportsReadWorkout = false;
        private bool m_SupportsWriteWorkout = false;
        private bool m_SupportsReadProfile = false;
        private bool m_SupportsWriteProfile = false;
        private bool m_SupportsFITWorkouts = false;
    }
}

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
    class GarminFitnessCommunicatorDevice : IGarminDevice, WorkoutImporter.AsyncImportDelegate, ProfileImporter.AsyncImportDelegate, IDisposable
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
                else if (attribute.Name.Equals("SupportsFITWorkoutSchedules"))
                {
                    m_SupportsFITWorkoutSchedules = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("FITWorkoutSchedulesFileWriteTransferPath"))
                {
                    m_FITWorkoutSchedulesFileWritePath = attribute.Value;
                }
                else if (attribute.Name.Equals("FITWorkoutSchedulesFileReadTransferPath"))
                {
                    m_FITWorkoutSchedulesFileReadPath = attribute.Value;
                }
                else if (attribute.Name.Equals("SupportReadProfile"))
                {
                    m_SupportsReadProfile = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("SupportWriteProfile"))
                {
                    m_SupportsWriteProfile = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("SupportsFITSettings"))
                {
                    m_SupportsFITSettings = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("FITSettingsFileWriteTransferPath"))
                {
                    m_FITSettingsFileWritePath = attribute.Value;
                }
                else if (attribute.Name.Equals("FITSettingsFileReadTransferPath"))
                {
                    m_FITSettingsFileReadPath = attribute.Value;
                }
                else if (attribute.Name.Equals("SupportsFITSports"))
                {
                    m_SupportsFITSports = Boolean.Parse(attribute.Value);
                }
                else if (attribute.Name.Equals("FITSportFileWriteTransferPath"))
                {
                    m_FITSportFileWritePath = attribute.Value;
                }
                else if (attribute.Name.Equals("FITSportFileReadTransferPath"))
                {
                    m_FITSportFileReadPath = attribute.Value;
                }
            }
        }

#region IDisposable Members

        public void Dispose()
        {
            m_Controller.CommunicatorBridge.ExceptionTriggered -= new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.ProgressChanged -= new EventHandler<GarminFitnessCommunicatorBridge.TransferProgressedEventArgs>(OnBridgeProgressChanged);
            m_Controller.CommunicatorBridge.ReadFromDeviceCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
            m_Controller.CommunicatorBridge.ReadDirectoryCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeReadDirectoryCompleted);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted -= new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
        }
    
#endregion

        void OnBridgeExceptionTriggered(object sender, GarminFitnessCommunicatorBridge.ExceptionEventArgs e)
        {
            DeviceOperations lastOperation = m_CurrentOperation;

            m_CancellationError = e.ExceptionText;
            m_CurrentOperation = DeviceOperations.Idle;

            Logger.Instance.LogText(String.Format("Bridge exception caught in device : {0}\nStack:{1}", m_CancellationError, new StackTrace(true).ToString()));

            switch (lastOperation)
            {
                case DeviceOperations.ReadProfile:
                case DeviceOperations.ReadWorkout:
                {
                    if (ReadFromDeviceCompleted != null)
                    {
                        ReadFromDeviceCompleted(this, lastOperation, false, m_CancellationError);
                    }
                    break;
                }
                case DeviceOperations.WriteProfile:
                case DeviceOperations.WriteWorkout:
                {
                    if (WriteToDeviceCompleted != null)
                    {
                        WriteToDeviceCompleted(this, lastOperation, false, m_CancellationError);
                    }
                    break;
                }
            }
        }

        private void OnBridgeReadFromDeviceCompleted(object sender, GarminFitnessCommunicatorBridge.TransferCompletedEventArgs e)
        {
            bool success = e.Success;
            bool operationCompleted = true;
            DeviceOperations lastOperation = m_CurrentOperation;

            switch(m_CurrentOperation)
            {
                case DeviceOperations.ReadProfile:
                {
                    Logger.Instance.LogText("Comm. : Profile read");

                    TriggerOperationWillCompleteEvent();

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
                case DeviceOperations.ReadFITProfile:
                {
                    if (success)
                    {
                        if (m_CurrentOperation == DeviceOperations.ReadFITProfile)
                        {
                            Logger.Instance.LogText("Comm. : Mass storage profile read");
                        }
                        else
                        {
                            Logger.Instance.LogText("Comm. : Mass storage workout read");
                        }

                        ++m_MassStorageFileReadCount;
                        if (OperationProgressed != null)
                        {
                            OperationProgressed(this, m_CurrentOperation, m_MassStorageFileReadCount);
                        }

                        if (m_MassStorageFilesToDownload != null)
                        {
                            Debug.Assert(m_MassStorageFilesToDownload.Count > 0);
                            Logger.Instance.LogText(String.Format("Mass storage files remaining = {0}", m_MassStorageFilesToDownload.Count));

                            if (m_MassStorageFilesToDownload.Count > 0)
                            {
                                String currentFilename = m_TempDirectoryLocation + m_MassStorageFilesToDownload[0].Replace('/', '\\');
                                String fileDirectory = currentFilename.Substring(0, currentFilename.LastIndexOf('\\'));
                                Directory.CreateDirectory(fileDirectory);
                                FileStream newFile = File.Create(currentFilename);
                                Byte[] decodedData = UUDecode(e.DataString);

                                newFile.Write(decodedData, 0, decodedData.Length);
                                newFile.Close();
                                m_MassStorageFilesToDownload.RemoveAt(0);
                                operationCompleted = false;

                                Logger.Instance.LogText(String.Format("Mass storage files remaining after delete = {0}", m_MassStorageFilesToDownload.Count));

                                if (m_MassStorageFilesToDownload.Count > 0)
                                {
                                    m_Controller.CommunicatorBridge.GetBinaryFile(m_MassStorageFilesToDownload[0]);
                                }
                                else
                                {
                                    TriggerOperationWillCompleteEvent();

                                    if (m_CurrentOperation == DeviceOperations.ReadFITProfile)
                                    {
                                        success = ProfileImporter.AsyncImportDirectory(m_TempDirectoryLocation, this);
                                    }
                                    else
                                    {
                                        success = WorkoutImporter.AsyncImportDirectory(m_TempDirectoryLocation, this);
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    throw new Exception();
                                }
                                catch (Exception exception)
                                {
                                    Logger.Instance.LogText("Caught empty mass storage list\n" + exception.StackTrace);
                                }
                            }
                        }
                    }
                    else
                    {
                        TriggerOperationWillCompleteEvent();
                        m_MassStorageFilesToDownload.Clear();
                    }

                    break;
                }
                case DeviceOperations.ReadWorkout:
                {
                    Logger.Instance.LogText("Comm. : Workouts read");

                    TriggerOperationWillCompleteEvent();

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
                m_CurrentOperation = DeviceOperations.Idle;

                if (ReadFromDeviceCompleted != null)
                {
                    ReadFromDeviceCompleted(this, lastOperation, success, m_CancellationError);
                }
            }
        }

        void OnBridgeProgressChanged(object sender, GarminFitnessCommunicatorBridge.TransferProgressedEventArgs e)
        {
            Logger.Instance.LogText(String.Format("Comm. : Operation progress : {0}", e.Progress));

            if (OperationProgressed != null)
            {
                OperationProgressed(this, m_CurrentOperation, e.Progress);
            }
        }

        private void OnBridgeWriteToDeviceCompleted(object sender, GarminFitnessCommunicatorBridge.TransferCompletedEventArgs e)
        {
            bool success = e.Success;
            DeviceOperations lastOperation = m_CurrentOperation;

            TriggerOperationWillCompleteEvent();

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

            m_CurrentOperation = DeviceOperations.Idle;

            if (WriteToDeviceCompleted != null)
            {
                WriteToDeviceCompleted(this, lastOperation, success, m_CancellationError);
            }
        }

        void OnBridgeReadDirectoryCompleted(object sender, GarminFitnessCommunicatorBridge.TransferCompletedEventArgs e)
        {
            bool operationComplete = true;

            m_IsReadingDirectory = false;

            if (e.Success)
            {
                if (OperationProgressed != null)
                {
                    OperationProgressed(this, m_CurrentOperation, 0);
                }

                if (m_CurrentOperation == DeviceOperations.ReadMassStorageWorkouts)
                {
                    m_MassStorageFilesToDownload = GetFilePaths(e.DataString);

                    Logger.Instance.LogText(String.Format("Comm. : Mass storage workout file count = {0}", m_MassStorageFilesToDownload.Count));
                }
                else if (m_CurrentOperation == DeviceOperations.ReadFITWorkouts)
                {
                    List<String> fitFilesOnDevice = GetFilePaths(e.DataString);

                    m_MassStorageFilesToDownload = new List<String>();

                    foreach (string fitFile in fitFilesOnDevice)
                    {
                        if (fitFile.StartsWith(m_FITWorkoutFileReadPath) ||
                            fitFile.StartsWith(m_FITWorkoutSchedulesFileReadPath))
                        {
                            m_MassStorageFilesToDownload.Add(fitFile);
                        }
                    }

                    Logger.Instance.LogText(String.Format("Comm. : Workout file count = {0}", m_MassStorageFilesToDownload.Count));
                }
                else if (m_CurrentOperation == DeviceOperations.ReadFITProfile)
                {
                    Logger.Instance.LogText("Comm. : Profile directory read");

                    List<String> fitFilesOnDevice = GetFilePaths(e.DataString);

                    m_MassStorageFilesToDownload = new List<String>();

                    foreach (string fitFile in fitFilesOnDevice)
                    {
                        if (fitFile.StartsWith(m_FITSettingsFileReadPath) ||
                            fitFile.StartsWith(m_FITSportFileReadPath))
                        {
                            m_MassStorageFilesToDownload.Add(fitFile);
                        }
                    }

                    Logger.Instance.LogText(String.Format("Comm. : Profile file count = {0}", m_MassStorageFilesToDownload.Count));
                }

                if (m_MassStorageFilesToDownload.Count > 0)
                {
                    operationComplete = false;

                    m_Controller.CommunicatorBridge.GetBinaryFile(m_MassStorageFilesToDownload[0]);
                }
            }

            if (operationComplete)
            {
                DeviceOperations lastOperation = m_CurrentOperation;
                m_CurrentOperation = DeviceOperations.Idle;

                if (ReadFromDeviceCompleted != null)
                {
                    ReadFromDeviceCompleted(this, lastOperation, e.Success, m_CancellationError);
                }
            }
        }

#region IDevice Members

        public void Initialize()
        {
            m_Controller.CommunicatorBridge.ExceptionTriggered += new EventHandler<GarminFitnessCommunicatorBridge.ExceptionEventArgs>(OnBridgeExceptionTriggered);
            m_Controller.CommunicatorBridge.ProgressChanged += new EventHandler<GarminFitnessCommunicatorBridge.TransferProgressedEventArgs>(OnBridgeProgressChanged);
            m_Controller.CommunicatorBridge.ReadFromDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeReadFromDeviceCompleted);
            m_Controller.CommunicatorBridge.ReadDirectoryCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeReadDirectoryCompleted);
            m_Controller.CommunicatorBridge.WriteToDeviceCompleted += new EventHandler<GarminFitnessCommunicatorBridge.TransferCompletedEventArgs>(OnBridgeWriteToDeviceCompleted);
        }

        public void Uninitialize()
        {
            Dispose();
        }

        public void CancelWrite(String reason)
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Idle);

            m_CancellationError = reason;

            m_Controller.CommunicatorBridge.CancelWriteToDevice();
        }

        public void CancelRead(String reason)
        {
            Debug.Assert(m_CurrentOperation != DeviceOperations.Idle);

            m_CancellationError = reason;

            if (m_IsReadingDirectory)
            {
                m_Controller.CommunicatorBridge.CancelReadFitDirectory();
            }
            else
            {
                m_Controller.CommunicatorBridge.CancelReadFromDevice();
            }
        }

        public void WriteWorkouts(List<IWorkout> workouts)
        {
            Logger.Instance.LogText("Comm. : Writing workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            List<IWorkout> concreteWorkouts = new List<IWorkout>();
            bool exportToFIT = false;

            // Split workouts for the right export method
            foreach (IWorkout currentWorkout in workouts)
            {
                if (currentWorkout.ContainsFITOnlyFeatures)
                {
                    exportToFIT = true;
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

            m_CancellationError = "";
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (!exportToFIT && !SupportsFITWorkouts && !SupportsWorkoutMassStorageTransfer)
            {
                // Basic TCX export
                string fileName = "Default.tcx";
                MemoryStream textStream = new MemoryStream();

                if (concreteWorkouts.Count == 1)
                {
                    fileName = Utils.GetWorkoutFilename(concreteWorkouts[0], GarminWorkoutManager.FileFormats.TCX);
                }

                WorkoutExporter.ExportWorkouts(concreteWorkouts, textStream);

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
                        MemoryStream schedulesDataStream = new MemoryStream();
                        UInt16 fileIdNumber = 0;

                        foreach (IWorkout currentWorkout in concreteWorkouts)
                        {
                            string fileName = Utils.GetWorkoutFilename(currentWorkout, GarminWorkoutManager.FileFormats.FIT);
                            FileStream fileStream = File.Create(m_TempDirectoryLocation + fileName);

                            // Serialize workout & schedules
                            WorkoutExporter.ExportWorkoutToFIT(currentWorkout, fileStream, fileIdNumber);
                            currentWorkout.SerializetoFITSchedule(schedulesDataStream);

                            fileStream.Close();
                            filenames.Add(fileName);

                            Logger.Instance.LogText(String.Format("Export workout : {0}", fileName));

                            ++fileIdNumber;
                        }

                        if (Options.Instance.EnableFITScheduling)
                        {
                            FileStream schedulesFileStream = File.Create(m_TempDirectoryLocation + "\\" + "Schedules.fit");
                            WorkoutExporter.ExportSchedulesFITFile(schedulesFileStream, schedulesDataStream, fileIdNumber);
                            schedulesFileStream.Close();
                            filenames.Add("Schedules.fit");
                            Logger.Instance.LogText("Export schedules");
                        }

                        Debug.Assert(m_FITWorkoutFileWritePath.Equals(m_FITWorkoutSchedulesFileWritePath));

                        exportPath = m_FITWorkoutFileWritePath;
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

                            Logger.Instance.LogText(String.Format("Export workout : {0}", fileName));
                        }

                        exportPath = m_WorkoutFileTransferPath;
                    }
                    else
                    {
                        throw new NoDeviceSupportException(this, GarminFitnessView.GetLocalizedString("NoMassStorageSupportText"));
                    }
                }

                m_CurrentOperation = DeviceOperations.WriteWorkout;

                m_Controller.CommunicatorBridge.WriteFilesToDevice(filenames, exportPath);
            }
        }

        public void ReadWorkouts()
        {
            Logger.Instance.LogText("Comm. : Reading workouts");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            m_CancellationError = "";
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsFITWorkouts ||
                SupportsWorkoutMassStorageTransfer)
            {
                m_MassStorageFileReadCount = 0;
                m_IsReadingDirectory = true;
                ClearTempDirectory();

                if (SupportsFITWorkouts)
                {
                    m_CurrentOperation = DeviceOperations.ReadFITWorkouts;
                    m_Controller.CommunicatorBridge.GetFITDirectoryInfo();
                }
                else
                {
                    m_CurrentOperation = DeviceOperations.ReadMassStorageWorkouts;
                    m_Controller.CommunicatorBridge.GetWorkoutFiles();
                }
            }
            else
            {
                m_CurrentOperation = DeviceOperations.ReadWorkout;
                m_Controller.CommunicatorBridge.ReadWorkoutsFromFitnessDevice();
            }
        }

        public void WriteProfile(string profileXml)
        {
            Logger.Instance.LogText("Comm. : Writing string profile");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            string fileName = "UserProfile.tcx";

            m_CancellationError = "";
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            m_CurrentOperation = DeviceOperations.WriteProfile;

            m_Controller.CommunicatorBridge.WriteProfileToFitnessDevice(profileXml, fileName);
        }

        public void WriteProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Writing profile");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            m_CancellationError = "";
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsFITProfile)
            {
                if (m_FITSportFileWritePath.Equals(m_FITSettingsFileWritePath))
                {
                    Logger.Instance.LogText("Comm. : Writing FIT profile");

                    IList<String> filenames;

                    ClearTempDirectory();

                    filenames = ProfileExporter.ExportProfileToFIT(m_TempDirectoryLocation);

                    Logger.Instance.LogText(String.Format("Comm. : {0} FIT profile files", filenames.Count));

                    m_CurrentOperation = DeviceOperations.WriteProfile;

                    m_Controller.CommunicatorBridge.WriteFilesToDevice(filenames, m_FITSportFileWritePath);
                }
                else
                {
                    throw new Exception("Different export path for setting & sport FIT files.  Please report this problem to PissedOffCil on the SportTracks forum.");
                }
            }
            else
            {
                string fileName = "UserProfile.tcx";
                MemoryStream textStream = new MemoryStream();

                ProfileExporter.ExportProfile(profile, textStream);
                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());

                m_CurrentOperation = DeviceOperations.WriteProfile;

                m_Controller.CommunicatorBridge.WriteProfileToFitnessDevice(xmlCode, fileName);
            }
        }

        public void ReadProfile(GarminFitnessPlugin.Data.GarminProfile profile)
        {
            Logger.Instance.LogText("Comm. : Reading profile");

            Debug.Assert(m_CurrentOperation == DeviceOperations.Idle);

            m_CancellationError = "";
            m_Controller.CommunicatorBridge.SetDeviceNumber(m_DeviceNumber);

            if (SupportsFITProfile)
            {
                Logger.Instance.LogText("Comm. : Reading FIT directory for profile");

                m_MassStorageFileReadCount = 0;
                m_IsReadingDirectory = true;
                ClearTempDirectory();

                m_CurrentOperation = DeviceOperations.ReadFITProfile;
                m_Controller.CommunicatorBridge.GetFITDirectoryInfo();
            }
            else
            {
                m_CurrentOperation = DeviceOperations.ReadProfile;
                m_Controller.CommunicatorBridge.ReadProfileFromFitnessDevice();
            }
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

            Logger.Instance.LogText(String.Format("UU decoded result length = {0}", Encoding.UTF8.GetString(decodedBytes).Length));

            return decodedBytes;
        }

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

        private List<string> GetFilePaths(String directoryXml)
        {
            List<string> result = new List<string>();
            XmlDocument directoryDocument = new XmlDocument();

            try
            {
                // Akward bug fix : Remove last character if it's a non-printing character
                for (int i = 0; i < 32; ++i)
                {
                    char currentCharacter = (char)i;

                    if (directoryXml.EndsWith(currentCharacter.ToString()))
                    {
                        directoryXml = directoryXml.Substring(0, directoryXml.Length - 1);
                        break;
                    }
                }

                directoryDocument.LoadXml(directoryXml);

                // Extract all file names and path
                if (directoryDocument.ChildNodes.Count >= 2 &&
                    directoryDocument.ChildNodes.Item(1).Name == "DirectoryListing")
                {
                    foreach (XmlElement fileNode in directoryDocument.ChildNodes.Item(1).ChildNodes)
                    {
                        if (fileNode.Name == "File" && !String.IsNullOrEmpty(fileNode.GetAttribute("Path")))
                        {
                            result.Add(fileNode.GetAttribute("Path"));
                        }
                    }

                    return result;
                }

                return null;
            }
            catch (Exception e)
            {
                return null;

                throw e;
            }
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

        public bool SupportsFITWorkoutSchedules
        {
            get { return m_SupportsFITWorkoutSchedules; }
        }

        public bool SupportsFITProfile
        {
            get { return m_SupportsFITSettings && m_SupportsFITSports; }
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

        public event DeviceOperationWillCompleteEventHandler OperationWillComplete;
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
                ReadFromDeviceCompleted(this, lastOperation, success, m_CancellationError);
            }
        }

        public void OnProgressChanged(int progressPercent)
        {
            if (OperationProgressed != null)
            {
                OperationProgressed(this, m_CurrentOperation, m_MassStorageFileReadCount + progressPercent);
            }
        }

#endregion

        private void TriggerOperationWillCompleteEvent()
        {
            if (OperationWillComplete != null)
            {
                OperationWillComplete(this, m_CurrentOperation);
            }
        }

        private GarminFitnessCommunicatorDeviceController m_Controller = null;
        private DeviceOperations m_CurrentOperation = DeviceOperations.Idle;
        private List<String> m_MassStorageFilesToDownload = null;
        private Int32 m_DeviceNumber = -1;
        private readonly string m_TempDirectoryLocation;
        private string m_DisplayName = String.Empty;
        private string m_Id = string.Empty;
        private string m_SoftwareVersion = string.Empty;
        private string m_WorkoutFileTransferPath = String.Empty;
        private string m_FITWorkoutFileWritePath = String.Empty;
        private string m_FITWorkoutFileReadPath = String.Empty;
        private string m_FITWorkoutSchedulesFileWritePath = String.Empty;
        private string m_FITWorkoutSchedulesFileReadPath = String.Empty;
        private string m_FITSettingsFileWritePath = String.Empty;
        private string m_FITSettingsFileReadPath = String.Empty;
        private string m_FITSportFileWritePath = String.Empty;
        private string m_FITSportFileReadPath = String.Empty;
        private string m_CancellationError = "";
        private int m_MassStorageFileReadCount = 0;
        private bool m_SupportsReadWorkout = false;
        private bool m_SupportsWriteWorkout = false;
        private bool m_SupportsReadProfile = false;
        private bool m_SupportsWriteProfile = false;
        private bool m_SupportsFITWorkouts = false;
        private bool m_SupportsFITWorkoutSchedules = false;
        private bool m_SupportsFITSettings = false;
        private bool m_SupportsFITSports = false;
        private bool m_IsReadingDirectory = false;
    }
}

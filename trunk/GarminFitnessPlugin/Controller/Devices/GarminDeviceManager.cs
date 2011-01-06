using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class GarminDeviceManager
    {
        private GarminDeviceManager()
        {
            try
            {
                Logger.Instance.LogText("Creating GarminDeviceManager");

                m_TimeoutTimer.Tick += new EventHandler(OnTimeoutTimerTick);
                m_TimeoutTimer.Interval = 30000;

                Logger.Instance.LogText("Adding Communicator controller");

                IGarminDeviceController newController = new GarminFitnessCommunicatorDeviceController();
                newController.InitializationCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerInitializationCompleted);
                newController.FindDevicesCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerFindDevicesCompleted);
                m_Controllers.Add(newController);

                Initialize();
            }
            catch (Exception e)
            {
                Logger.Instance.LogText(String.Format("Error in GarminDeviceManager constructor, {0}",
                                                      e.Message));

                MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
            }
        }

        ~GarminDeviceManager()
        {
            Logger.Instance.LogText("Destroying GarminDeviceManager");

            foreach(IGarminDeviceController controller in m_Controllers)
            {
                controller.InitializationCompleted -= new DeviceControllerOperationCompletedEventHandler(OnControllerInitializationCompleted);
                controller.FindDevicesCompleted -= new DeviceControllerOperationCompletedEventHandler(OnControllerFindDevicesCompleted);
            }

            m_Controllers.Clear();
        }

        private void Initialize()
        {
            Logger.Instance.LogText("Initializing first controller");
            m_Controllers[0].Initialize();
        }

        public void RefreshDevices()
        {
            if (m_Controllers.Count > 0)
            {
                Logger.Instance.LogText("Refreshing devices");

                m_Devices.Clear();

                // Refresh devices is an optional sub-step of SetOperatingDevice, so insert it
                //  first and trigger it manually.
                m_TaskQueue.Insert(0, new BasicTask(BasicTask.TaskTypes.RefreshDevices));

                m_Controllers[0].FindDevices();
            }
            else
            {
                Logger.Instance.LogText("Refreshing devices without controllers");

                CancelAllTasks();
            }
        }

        public void SetOperatingDevice()
        {
            Logger.Instance.LogText("Setting operating devices");

            m_Devices.Clear();
            OperatingDevice = null;

            AddTask(new SetOperationDeviceTask());
        }

        public void ExportWorkouts(List<IWorkout> workouts)
        {
            String workoutsText = String.Empty;
            List<IWorkout> workoutsToExport = new List<IWorkout>();

            foreach(Workout current in workouts)
            {
                workoutsText += String.Format(", {0}", current.Name);

                if (current.GetSplitPartsCount() == 1)
                {
                    workoutsToExport.Add(current);
                }
                else
                {
                    List<WorkoutPart> parts = current.SplitInSeperateParts();

                    foreach (WorkoutPart part in parts)
                    {
                        workoutsToExport.Add(part);
                    }
                }
            }

            Logger.Instance.LogText(String.Format("Exporting workouts({0}){1}", workouts.Count, workoutsText));

            if (workoutsToExport.Count > 0)
            {
                AddTask(new ExportWorkoutTask(workoutsToExport));
            }
        }

        public void ImportWorkouts()
        {
            Logger.Instance.LogText("Importing workouts");

            AddTask(new ImportWorkoutsTask());
        }

        public void ImportProfile()
        {
            Logger.Instance.LogText("Importing profile");

            AddTask(new ImportProfileTask());
        }

        public void ExportProfile()
        {
            Logger.Instance.LogText("Exporting profile");

            AddTask(new ExportProfileTask());
        }

        public void CancelAllTasks()
        {
            Logger.Instance.LogText("Cancelling all tasks");

            m_TaskQueue.Clear();
        }

        private void CancelPendingTasks()
        {
            Logger.Instance.LogText("Cancelling pending tasks");

            m_TaskQueue.RemoveRange(1, m_TaskQueue.Count - 1);
        }

        private void AddTask(BasicTask task)
        {
            Logger.Instance.LogText(String.Format("Adding task {0}", task.Type.ToString()));

            m_TaskQueue.Add(task);

            if (IsInitialized && m_TaskQueue.Count == 1)
            {
                // Start the task since none are pending
                StartNextTask();
            }
        }

        private void StartNextTask()
        {
            try
            {
                Logger.Instance.LogText(String.Format("Starting task {0}", CurrentTask.Type.ToString()));

                Debug.Assert(m_TaskQueue.Count > 0);
                Debug.Assert(IsInitialized);

                m_TimeoutTimer.Start();
                m_LastProgressValue = 0;
                CurrentTask.ExecuteTask(OperatingDevice);
            }
            catch (NoDeviceSupportException e)
            {
                Logger.Instance.LogText("Comm : No device support");

                CompleteCurrentTask(false, e.Message);
            }
        }

        private void CompleteCurrentTask(bool success, String errorText)
        {
            Logger.Instance.LogText(String.Format("Task {0} completed with result {1}", CurrentTask.Type.ToString(), success));

            BasicTask task = CurrentTask;

            m_TimeoutTimer.Stop();
            m_TaskQueue.RemoveAt(0);

            if (TaskCompleted != null)
            {
                TaskCompleted(this, task, success, errorText);
            }

            if (success)
            {
                if (PendingTaskCount > 0 && task.Type != BasicTask.TaskTypes.RefreshDevices)
                {
                    StartNextTask();
                }
            }
        }

        private void CompleteCurrentTask(bool success)
        {
            CompleteCurrentTask(success, String.Empty);
        }

        private void OnControllerInitializationCompleted(object sender, Boolean succeeded)
        {
            if (succeeded)
            {
                IGarminDeviceController controller = sender as IGarminDeviceController;
                int currentControllerIndex = m_Controllers.IndexOf(controller);

                Logger.Instance.LogText(String.Format("Controller {0} completed initialization", currentControllerIndex + 1));

                if (currentControllerIndex != m_Controllers.Count - 1)
                {
                    Logger.Instance.LogText(String.Format("Initializing controller {0}", currentControllerIndex + 2));

                    // Not the last controller, start the next one.
                    m_Controllers[currentControllerIndex + 1].Initialize();
                }
                else
                {
                    Logger.Instance.LogText("All initializations completed");

                    if (PendingTaskCount > 0)
                    {
                        StartNextTask();
                    }
                }
            }
            else
            {
                Logger.Instance.LogText("Controller initialization failed");

                if (TaskCompleted != null)
                {
                    TaskCompleted(this, new BasicTask(BasicTask.TaskTypes.Initialize), false, String.Empty);
                }

                CancelAllTasks();
            }
        }

        private void OnControllerFindDevicesCompleted(object sender, Boolean succeeded)
        {
            bool setDeviceSucceeded = succeeded;

            if (succeeded)
            {
                IGarminDeviceController controller = sender as IGarminDeviceController;
                int currentControllerIndex = m_Controllers.IndexOf(controller);

                Logger.Instance.LogText(String.Format("Controller {0} finished finding devices", currentControllerIndex + 1));

                foreach (IGarminDevice currentDevice in controller.Devices)
                {
                    Logger.Instance.LogText(String.Format("Device {0} found (id={1}, version={2})", currentDevice.DisplayName, currentDevice.DeviceId, currentDevice.SoftwareVersion));

                    // Don't add invalid devices or add them twice
                    if (currentDevice.SoftwareVersion != "0")
                    {
                        if (!m_Devices.ContainsKey(currentDevice.DeviceId))
                        {
                            Logger.Instance.LogText(String.Format("Device {0} added", currentDevice.DeviceId));

                            m_Devices.Add(currentDevice.DeviceId, currentDevice);
                        }
                        else
                        {
                            Logger.Instance.LogText(String.Format("Device {0} duplicate, ignored", currentDevice.DeviceId));
                        }
                    }
                    else
                    {
                        Logger.Instance.LogText(String.Format("Device {0} ignored", currentDevice.DeviceId));
                    }
                }

                if (currentControllerIndex != m_Controllers.Count - 1)
                {
                    Logger.Instance.LogText(String.Format("Finding devices on next controller {0}", currentControllerIndex + 2));

                    // Not the last controller, start the next one.
                    m_Controllers[currentControllerIndex + 1].FindDevices();
                }
                else
                {
                    Logger.Instance.LogText(String.Format("All controllers found devices({0})", m_Devices.Count));

                    if (CurrentTask.Type == BasicTask.TaskTypes.SetOperatingDevice)
                    {
                        if (Devices.Count == 1)
                        {
                            Logger.Instance.LogText(String.Format("Using only device found({0})", Devices[0].DeviceId));

                            // Use the first and only device
                            OperatingDevice = Devices[0];
                        }
                        else
                        {
                            Logger.Instance.LogText("Multiple devices found");

                            SelectDeviceDialog dlg = new SelectDeviceDialog();

                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Logger.Instance.LogText(String.Format("Selected device in dialog : {0}", dlg.SelectedDevice.DeviceId));

                                OperatingDevice = dlg.SelectedDevice;
                            }
                            else
                            {
                                CancelPendingTasks();
                                setDeviceSucceeded = false;
                            }
                        }
                    }

                    CompleteCurrentTask(setDeviceSucceeded);
                }
            }
            else
            {
                CompleteCurrentTask(setDeviceSucceeded);
            }
        }

        private void OnWriteToDeviceCompleted(IGarminDevice device, DeviceOperations operation, Boolean succeeded, String failureMessage)
        {
            String errorText = String.Empty;

            if (CurrentTask.Type == BasicTask.TaskTypes.ExportWorkout)
            {
                Logger.Instance.LogText("Completed export workouts");

                Debug.Assert(operation == DeviceOperations.WriteWorkout);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ExportWorkoutsErrorText");

                    if (!String.IsNullOrEmpty(failureMessage))
                    {
                        errorText += "\n\nReason :\n" + failureMessage;
                    }
                }
            }
            else if (CurrentTask.Type == BasicTask.TaskTypes.ExportProfile)
            {
                Logger.Instance.LogText("Completed export profile");

                Debug.Assert(operation == DeviceOperations.WriteProfile);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ExportProfileErrorText");

                    if (!String.IsNullOrEmpty(failureMessage))
                    {
                        errorText += "\n\nReason :\n" + failureMessage;
                    }
                }
            }

            CompleteCurrentTask(succeeded, errorText);
        }

        private void OnReadFromDeviceCompleted(IGarminDevice device, DeviceOperations operation, Boolean succeeded, String failureMessage)
        {
            String errorText = String.Empty;

            if (CurrentTask.Type == BasicTask.TaskTypes.ImportWorkouts)
            {
                Logger.Instance.LogText("Completed import workouts");

                Debug.Assert(operation == DeviceOperations.ReadWorkout ||
                             operation == DeviceOperations.ReadMassStorageWorkouts ||
                             operation == DeviceOperations.ReadFITWorkouts);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ImportWorkoutsErrorText");

                    if(!String.IsNullOrEmpty(failureMessage))
                    {
                        errorText += "\n\nReason :\n" + failureMessage;
                    }
                }
            }
            else if (CurrentTask.Type == BasicTask.TaskTypes.ImportProfile)
            {
                Logger.Instance.LogText("Completed import profile");

                Debug.Assert(operation == DeviceOperations.ReadProfile ||
                             operation == DeviceOperations.ReadFITProfile);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ImportProfileErrorText");

                    if (!String.IsNullOrEmpty(failureMessage))
                    {
                        errorText += "\n\nReason :\n" + failureMessage;
                    }
                }
            }

            CompleteCurrentTask(succeeded, errorText);
        }

        private void OnOperationToDeviceWillComplete(IGarminDevice device, DeviceOperations operation)
        {
            m_TimeoutTimer.Stop();
        }

        private void OnOperationProgressed(IGarminDevice device, DeviceOperations operation, int progress)
        {
            if (progress > m_LastProgressValue)
            {
                // Restart timer
                m_TimeoutTimer.Stop();
                m_TimeoutTimer.Start();

                m_LastProgressValue = progress;
            }
        }

        private void OnTimeoutTimerTick(object sender, EventArgs e)
        {
            Logger.Instance.LogText("Operation timeout");

            m_TimeoutTimer.Stop();

            if (CurrentTask.Type == BasicTask.TaskTypes.ExportWorkout ||
                CurrentTask.Type == BasicTask.TaskTypes.ExportProfile)
            {
                OperatingDevice.CancelWrite();
            }
            else if (CurrentTask.Type == BasicTask.TaskTypes.ImportWorkouts ||
                     CurrentTask.Type == BasicTask.TaskTypes.ImportProfile)
            {
                OperatingDevice.CancelRead();
            }
            else
            {
                CancelPendingTasks();
                CompleteCurrentTask(false);
            }
        }

        public class BasicTask
        {
            public BasicTask(TaskTypes type)
            {
                m_Type = type;
            }

            public enum TaskTypes
            {
                Initialize,
                RefreshDevices,
                SetOperatingDevice,
                ExportWorkout,
                ImportWorkouts,
                ImportProfile,
                ExportProfile
            };

            public TaskTypes Type
            {
                get { return m_Type; }
            }

            public virtual void ExecuteTask(IGarminDevice device)
            {
            }

            private TaskTypes m_Type;
        }

        public class SetOperationDeviceTask : BasicTask
        {
            public SetOperationDeviceTask() :
                base(TaskTypes.SetOperatingDevice)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                if(GarminDeviceManager.Instance.m_Controllers.Count > 0)
                {
                    GarminDeviceManager.Instance.m_Controllers[0].FindDevices();
                }
            }
        }

        public class ExportWorkoutTask : BasicTask
        {
            public ExportWorkoutTask(List<IWorkout> workouts) :
                base(TaskTypes.ExportWorkout)
            {
                m_Workouts = workouts;
            }

            public ExportWorkoutTask(IWorkout workout) :
                base(TaskTypes.ExportWorkout)
            {
                m_Workouts = new List<IWorkout>();
                m_Workouts.Add(workout);
            }

            public List<IWorkout> Workouts
            {
                get { return m_Workouts; }
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsWriteWorkout)
                {
                    throw new NoDeviceSupportException(device, GarminFitnessView.GetLocalizedString("ExportWorkoutsText"));
                }
                else
                {
                    device.WriteWorkouts(m_Workouts);
                }
            }

            private List<IWorkout> m_Workouts;
        }

        public class ExportProfileTask : BasicTask
        {
            public ExportProfileTask() :
                base(TaskTypes.ExportProfile)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsWriteProfile && !device.SupportsFITProfile)
                {
                    throw new NoDeviceSupportException(device, GarminFitnessView.GetLocalizedString("ExportProfileText"));
                }
                else
                {
                    device.WriteProfile(GarminProfileManager.Instance.UserProfile);
                }
            }
        }

        public class ImportWorkoutsTask : BasicTask
        {
            public ImportWorkoutsTask() :
                base(TaskTypes.ImportWorkouts)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadWorkout && !device.SupportsFITWorkouts)
                {
                    throw new NoDeviceSupportException(device, GarminFitnessView.GetLocalizedString("ImportWorkoutsText"));
                }
                else
                {
                    device.ReadWorkouts();
                }
            }
        }

        public class ImportProfileTask : BasicTask
        {
            public ImportProfileTask() :
                base(TaskTypes.ImportProfile)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadProfile && !device.SupportsFITProfile)
                {
                    throw new NoDeviceSupportException(device, GarminFitnessView.GetLocalizedString("ImportProfileText"));
                }
                else
                {
                    device.ReadProfile(GarminProfileManager.Instance.UserProfile);
                }
            }
        }

        public int PendingTaskCount
        {
            get { return m_TaskQueue.Count; }
        }

        public BasicTask CurrentTask
        {
            get { return m_TaskQueue[0]; }
        }

        public static GarminDeviceManager Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = new GarminDeviceManager();
                }

                return m_Instance;
            }
        }

        public bool IsInitialized
        {
            get
            {
                foreach (IGarminDeviceController controller in m_Controllers)
                {
                    if (!controller.IsInitialized)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool AreAllTasksFinished
        {
            get { return m_TaskQueue.Count == 0; }
        }

        public List<IGarminDevice> Devices
        {
            get
            {
                List<IGarminDevice> devices = new List<IGarminDevice>();
                Dictionary<String, IGarminDevice>.ValueCollection.Enumerator enumerator = m_Devices.Values.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    devices.Add(enumerator.Current);
                }

                return devices;
            }
        }

        public IGarminDevice OperatingDevice
        {
            get { return m_OperatingDevice; }
            private set
            {
                if(OperatingDevice != value)
                {
                    if (OperatingDevice != null)
                    {
                        OperatingDevice.ReadFromDeviceCompleted -= new DeviceOperationCompletedEventHandler(OnReadFromDeviceCompleted);
                        OperatingDevice.WriteToDeviceCompleted -= new DeviceOperationCompletedEventHandler(OnWriteToDeviceCompleted);
                        OperatingDevice.OperationWillComplete -= new DeviceOperationWillCompleteEventHandler(OnOperationToDeviceWillComplete);
                        OperatingDevice.OperationProgressed -= new DeviceOperationProgressedEventHandler(OnOperationProgressed);
                    }

                    m_OperatingDevice = value;

                    if (OperatingDevice != null)
                    {
                        OperatingDevice.ReadFromDeviceCompleted += new DeviceOperationCompletedEventHandler(OnReadFromDeviceCompleted);
                        OperatingDevice.WriteToDeviceCompleted += new DeviceOperationCompletedEventHandler(OnWriteToDeviceCompleted);
                        OperatingDevice.OperationWillComplete += new DeviceOperationWillCompleteEventHandler(OnOperationToDeviceWillComplete);
                        OperatingDevice.OperationProgressed += new DeviceOperationProgressedEventHandler(OnOperationProgressed);
                    }
                }
            }
        }

        public delegate void TaskCompletedEventHandler(GarminDeviceManager manager, BasicTask task, bool succeeded, String errorText);
        public event TaskCompletedEventHandler TaskCompleted;

        private List<IGarminDeviceController> m_Controllers = new List<IGarminDeviceController>();
        private List<BasicTask> m_TaskQueue = new List<BasicTask>();
        private Dictionary<String, IGarminDevice> m_Devices = new Dictionary<String, IGarminDevice>();
        private IGarminDevice m_OperatingDevice = null;
        private System.Windows.Forms.Timer m_TimeoutTimer = new System.Windows.Forms.Timer();
        private int m_LastProgressValue = 0;

        private static GarminDeviceManager m_Instance = null;
    }
}

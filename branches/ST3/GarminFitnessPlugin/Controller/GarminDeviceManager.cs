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

/*                Logger.Instance.LogText("Adding GarXFace controller");

                IGarminDeviceController newController = new GarXFaceDeviceController();
                newController.InitializationCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerInitializationCompleted);
                newController.FindDevicesCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerFindDevicesCompleted);
                m_Controllers.Add(newController);*/

                Logger.Instance.LogText("Adding Communicator controller");

                IGarminDeviceController newController = new CommunicatorDeviceController();
                newController.InitializationCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerInitializationCompleted);
                newController.FindDevicesCompleted += new DeviceControllerOperationCompletedEventHandler(OnControllerFindDevicesCompleted);
                m_Controllers.Add(newController);

                Logger.Instance.LogText("Initializing first controller");
                m_Controllers[0].Initialize();
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

        public void RefreshDevices()
        {
            if (m_Controllers.Count > 0)
            {
                Logger.Instance.LogText("Refreshing devices");

                m_Devices.Clear();

                // Refresh devices is an optional sub-step of SetOperatingDevice, so insert it
                //  first and trigger it manually.
                m_TaskQueue.Insert(0, new BasicTask(BasicTask.TaskTypes.TaskType_RefreshDevices));

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

        public void ExportWorkout(List<Workout> workouts)
        {
            String workoutsText = String.Empty;

            foreach(Workout current in workouts)
            {
                workoutsText += String.Format(", {0}", current.Name);
            }

            Logger.Instance.LogText(String.Format("Exporting workouts({0}){1}", workouts.Count, workoutsText));

            AddTask(new ExportWorkoutTask(workouts));
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
                Logger.Instance.LogText(String.Format("Starting task {0}", GetCurrentTask().Type.ToString()));

                Debug.Assert(m_TaskQueue.Count > 0);
                Debug.Assert(IsInitialized);

                m_TimeoutTimer.Start();
                GetCurrentTask().ExecuteTask(OperatingDevice);
            }
            catch (NoDeviceSupportException e)
            {
                Logger.Instance.LogText("Error in task execution");

                CompleteCurrentTask(false, e.Message);
            }
        }

        private void CompleteCurrentTask(bool success, String errorText)
        {
            Logger.Instance.LogText(String.Format("Task {0} completed with result {1}", GetCurrentTask().Type.ToString(), success));

            BasicTask task = GetCurrentTask();

            m_TimeoutTimer.Stop();
            m_TaskQueue.RemoveAt(0);

            if (TaskCompleted != null)
            {
                TaskCompleted(this, task, success, errorText);
            }

            if (success)
            {
                if (GetPendingTaskCount() > 0 && task.Type != BasicTask.TaskTypes.TaskType_RefreshDevices)
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

                    if (GetPendingTaskCount() > 0)
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
                    TaskCompleted(this, new BasicTask(BasicTask.TaskTypes.TaskType_Initialize), false, String.Empty);
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

                    if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_SetOperatingDevice)
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
        }

        void OnWriteToDeviceCompleted(IGarminDevice device, DeviceOperations operation, Boolean succeeded)
        {
            String errorText = String.Empty;

            if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ExportWorkout)
            {
                Logger.Instance.LogText("Completed export workouts");

                Debug.Assert(operation == DeviceOperations.Operation_WriteWorkout);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ExportWorkoutsErrorText");
                }
            }
            else if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ExportProfile)
            {
                Logger.Instance.LogText("Completed export profile");

                Debug.Assert(operation == DeviceOperations.Operation_WriteProfile);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ExportProfileErrorText");
                }
            }

            CompleteCurrentTask(succeeded, errorText);
        }

        void OnReadFromDeviceCompleted(IGarminDevice device, DeviceOperations operation, Boolean succeeded)
        {
            String errorText = String.Empty;

            if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ImportWorkouts)
            {
                Logger.Instance.LogText("Completed import workouts");

                Debug.Assert(operation == DeviceOperations.Operation_ReadWorkout);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ImportWorkoutsErrorText");
                }

            }
            else if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ImportProfile)
            {
                Logger.Instance.LogText("Completed import profile");

                Debug.Assert(operation == DeviceOperations.Operation_ReadProfile);

                if (!succeeded)
                {
                    errorText = GarminFitnessView.GetLocalizedString("ImportProfileErrorText");
                }
            }

            CompleteCurrentTask(succeeded, errorText);
        }

        void OnTimeoutTimerTick(object sender, EventArgs e)
        {
            Logger.Instance.LogText("Operation timeout");

            m_TimeoutTimer.Stop();

            if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ExportWorkout ||
                GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ExportProfile)
            {
                OperatingDevice.CancelWrite();
            }
            else if (GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ImportWorkouts ||
                     GetCurrentTask().Type == BasicTask.TaskTypes.TaskType_ImportProfile)
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
                TaskType_Initialize,
                TaskType_RefreshDevices,
                TaskType_SetOperatingDevice,
                TaskType_ExportWorkout,
                TaskType_ImportWorkouts,
                TaskType_ImportProfile,
                TaskType_ExportProfile
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
                base(TaskTypes.TaskType_SetOperatingDevice)
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
            public ExportWorkoutTask(List<Workout> workouts) :
                base(TaskTypes.TaskType_ExportWorkout)
            {
                m_Workouts = workouts;
            }

            public List<Workout> Workouts
            {
                get { return m_Workouts; }
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadWorkout)
                {
                    throw new NoDeviceSupportException(device, "Export Workout");
                }
                else
                {
                    List<IWorkout> allWorkouts = new List<IWorkout>();

                    foreach (IWorkout workout in m_Workouts)
                    {
                        allWorkouts.Add(workout);
                    }

                    device.WriteWorkouts(allWorkouts);
                }
            }

            private List<Workout> m_Workouts;
        }

        public class ExportProfileTask : BasicTask
        {
            public ExportProfileTask()
                :
                base(TaskTypes.TaskType_ExportProfile)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadProfile)
                {
                    throw new NoDeviceSupportException(device, "Export Profile");
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
                base(TaskTypes.TaskType_ImportWorkouts)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadWorkout)
                {
                    throw new NoDeviceSupportException(device, "Import Workouts");
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
                base(TaskTypes.TaskType_ImportProfile)
            {
            }

            public override void ExecuteTask(IGarminDevice device)
            {
                // This function is not supported on the device
                if (!device.SupportsReadProfile)
                {
                    throw new NoDeviceSupportException(device, "Import Profile");
                }
                else
                {
                    device.ReadProfile(GarminProfileManager.Instance.UserProfile);
                }
            }
        }

        public int GetPendingTaskCount()
        {
            return m_TaskQueue.Count;
        }

        public BasicTask GetCurrentTask()
        {
            return m_TaskQueue[0];
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

        private IGarminDevice OperatingDevice
        {
            get { return m_OperatingDevice; }
            set
            {
                if(OperatingDevice != value)
                {
                    if (OperatingDevice != null)
                    {
                        OperatingDevice.ReadFromDeviceCompleted -= new DeviceOperationCompletedEventHandler(OnReadFromDeviceCompleted);
                        OperatingDevice.WriteToDeviceCompleted -= new DeviceOperationCompletedEventHandler(OnWriteToDeviceCompleted);
                    }

                    m_OperatingDevice = value;

                    if (OperatingDevice != null)
                    {
                        OperatingDevice.ReadFromDeviceCompleted += new DeviceOperationCompletedEventHandler(OnReadFromDeviceCompleted);
                        OperatingDevice.WriteToDeviceCompleted += new DeviceOperationCompletedEventHandler(OnWriteToDeviceCompleted);
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
        private static Timer m_TimeoutTimer = new Timer();

        private static GarminDeviceManager m_Instance = null;
    }
}

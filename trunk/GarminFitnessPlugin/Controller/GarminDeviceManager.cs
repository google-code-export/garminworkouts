using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
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
                m_TimeoutTimer.Tick += new EventHandler(OnTimeoutTimerTick);
                m_TimeoutTimer.Interval = 30000;

                m_Controller = new GarminDeviceControl();
                m_Controller.ReadyChanged += new EventHandler(OnControllerReadyChanged);
                m_Controller.FinishFindDevices += new GarminDeviceControl.FinishFindDevicesEventHandler(OnControllerFinishFindDevices);
                m_Controller.FinishWriteToDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
                m_Controller.FinishReadFromDevice += new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);

                AddTask(new BasicTask(BasicTask.TaskTypes.TaskType_Initialize));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\n" + e.StackTrace);
            }
        }

        ~GarminDeviceManager()
        {
            m_Controller.ReadyChanged -= new EventHandler(OnControllerReadyChanged);
            m_Controller.FinishFindDevices -= new GarminDeviceControl.FinishFindDevicesEventHandler(OnControllerFinishFindDevices);
            m_Controller.FinishWriteToDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
            m_Controller.FinishReadFromDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);
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
            // refresh devices is an optional sub-step of SetOperatingDevice, so insert it
            //  first and trigger it manually.
            m_TaskQueue.Insert(0, new BasicTask(BasicTask.TaskTypes.TaskType_RefreshDevices));
            m_Controller.FindDevices();
        }

        public void SetOperatingDevice()
        {
            AddTask(new SetOperationDeviceTask());
        }

        public void ExportWorkout(List<Workout> workouts)
        {
            AddTask(new ExportWorkoutTask(workouts));
        }

        public void ImportWorkouts()
        {
            AddTask(new ImportWorkoutsTask());
        }

        public void ImportProfile()
        {
            AddTask(new ImportProfileTask());
        }

        public void ExportProfile()
        {
            AddTask(new ExportProfileTask());
        }

        public void CancelAllTasks()
        {
            m_TaskQueue.Clear();
        }

        private void CancelPendingTasks()
        {
            m_TaskQueue.RemoveRange(1, m_TaskQueue.Count - 1);
        }

        private void AddTask(BasicTask task)
        {
            m_TaskQueue.Add(task);

            if (m_TaskQueue.Count == 1)
            {
                // Start the task since none are pending
                StartNextTask();
            }
        }

        private void StartNextTask()
        {
            Trace.Assert(m_TaskQueue.Count > 0);
            Trace.Assert(m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_Initialize || IsInitialized);

            m_TaskQueue[0].ExecuteTask(m_Controller, m_OperatingDevice);
            m_TimeoutTimer.Start();
        }

        private void CompleteCurrentTask(bool success)
        {
            BasicTask task = m_TaskQueue[0];

            m_TimeoutTimer.Stop();
            m_TaskQueue.RemoveAt(0);

            if (TaskCompleted != null)
            {
                TaskCompleted(this, task, success);
            }

            if (success)
            {
                if (m_TaskQueue.Count > 0 && task.Type != BasicTask.TaskTypes.TaskType_RefreshDevices)
                {
                    StartNextTask();
                }
            }
        }

        private void OnControllerReadyChanged(object sender, EventArgs e)
        {
            if (m_Controller.Ready && m_Controller.Initialize())
            {
                CompleteCurrentTask(true);
            }
            else
            {
                CancelPendingTasks();

                CompleteCurrentTask(false);
            }
        }

        private void OnControllerFinishFindDevices(object sender, GarminDeviceControl.FinishFindDevicesEventArgs e)
        {
            bool succeeded = true;

            m_Devices.Clear();

            for (int i = 0; i < e.Devices.Length; ++i)
            {
                Device currentDevice = e.Devices[i];

                if (currentDevice.SoftwareVersion != "0")
                {
                    m_Devices.Add(currentDevice);
                }
            }

            if (m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_SetOperatingDevice)
            {
                if (m_Devices.Count == 1)
                {
                    m_OperatingDevice = m_Devices[0];
                }
                else
                {
                    SelectDeviceDialog dlg = new SelectDeviceDialog();

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        m_OperatingDevice = dlg.SelectedDevice;
                    }
                    else
                    {
                        CancelPendingTasks();
                        succeeded = false;
                    }
                }
            }

            CompleteCurrentTask(succeeded);
        }

        void OnControllerFinishWriteToDevice(object sender, GarminDeviceControl.TransferCompleteEventArgs e)
        {
            CompleteCurrentTask(e.Success);
        }

        void OnControllerFinishReadFromDevice(object sender, GarminDeviceControl.TransferCompleteEventArgs e)
        {
            if(m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportWorkouts)
            {
                ImportWorkoutsTask task = (ImportWorkoutsTask)m_TaskQueue[0];

                task.WorkoutsXML = e.XmlData;
                CompleteCurrentTask(e.Success);
            }
            else if(m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportProfile)
            {
                ImportProfileTask task = (ImportProfileTask)m_TaskQueue[0];

                task.ProfileXML = e.XmlData;
                CompleteCurrentTask(e.Success);
            }
        }

        void OnTimeoutTimerTick(object sender, EventArgs e)
        {
            m_TimeoutTimer.Stop();

            if (m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ExportWorkout ||
                m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ExportProfile)
            {
                m_Controller.FireFinishWriteToDevice(false, "");
            }
            else if (m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportWorkouts ||
                     m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportProfile)
            {
                m_Controller.FireFinishReadFromDevice(false, "");
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

            public virtual void ExecuteTask(GarminDeviceControl controller, Device device)
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

            public string WorkoutsXML
            {
                get { return m_WorkoutsXML; }
                set { m_WorkoutsXML = value; }
            }

            public override void ExecuteTask(GarminDeviceControl controller, Device device)
            {
                controller.FindDevices();
            }

            private string m_WorkoutsXML;
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

            public override void ExecuteTask(GarminDeviceControl controller, Device device)
            {
                string fileName;

                if (Workouts.Count == 1)
                {
                    fileName = Utils.GetWorkoutFilename(Workouts[0]);
                }
                else
                {
                    fileName = "Workouts.tcx";
                }
                MemoryStream textStream = new MemoryStream();

                WorkoutExporter.ExportWorkout(Workouts, textStream);
                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());
                controller.WriteWorkouts(device,
                                         xmlCode,
                                         fileName);
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

            public override void ExecuteTask(GarminDeviceControl controller, Device device)
            {
                string fileName = "Profile.tcx";
                MemoryStream textStream = new MemoryStream();

                ProfileExporter.ExportProfile(textStream);
                string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());
                controller.WriteUserProfile(device,
                                            xmlCode,
                                            fileName);
            }
        }

        public class ImportWorkoutsTask : BasicTask
        {
            public ImportWorkoutsTask() :
                base(TaskTypes.TaskType_ImportWorkouts)
            {
            }

            public string WorkoutsXML
            {
                get { return m_WorkoutsXML; }
                set { m_WorkoutsXML = value; }
            }

            public override void ExecuteTask(GarminDeviceControl controller, Device device)
            {
                controller.ReadWktWorkouts(device);
            }

            private string m_WorkoutsXML;
        }

        public class ImportProfileTask : BasicTask
        {
            public ImportProfileTask() :
                base(TaskTypes.TaskType_ImportProfile)
            {
            }

            public string ProfileXML
            {
                get { return m_ProfileXML; }
                set { m_ProfileXML = value; }
            }

            public override void ExecuteTask(GarminDeviceControl controller, Device device)
            {
                controller.ReadTcxUserProfile(device);
            }

            private string m_ProfileXML;
        }

        public int GetPendingTaskCount()
        {
            return m_TaskQueue.Count;
        }

        public bool IsInitialized
        {
            get { return m_Controller.Ready && m_Controller.Initialized; }
        }

        public bool AreAllTasksFinished
        {
            get { return m_TaskQueue.Count == 0; }
        }

        public List<Device> Devices
        {
            get { return m_Devices; }
        }

        public delegate void TaskCompletedEventHandler(GarminDeviceManager manager, BasicTask task, bool succeeded);
        public event TaskCompletedEventHandler TaskCompleted;

        private GarminDeviceControl m_Controller;
        private List<BasicTask> m_TaskQueue = new List<BasicTask>();
        private List<Device> m_Devices = new List<Device>();
        private Device m_OperatingDevice = null;
        private Timer m_TimeoutTimer = new Timer();

        private static GarminDeviceManager m_Instance = null;
    }
}

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.SportTracks.Device.GarminGPS;
using GarminWorkoutPlugin.View;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.Controller
{
    class GarminDeviceManager
    {
        private GarminDeviceManager()
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

        ~GarminDeviceManager()
        {
            m_Controller.ReadyChanged -= new EventHandler(OnControllerReadyChanged);
            m_Controller.FinishFindDevices -= new GarminDeviceControl.FinishFindDevicesEventHandler(OnControllerFinishFindDevices);
            m_Controller.FinishWriteToDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishWriteToDevice);
            m_Controller.FinishReadFromDevice -= new GarminDeviceControl.TransferCompleteEventHandler(OnControllerFinishReadFromDevice);
        }

        public static GarminDeviceManager GetInstance()
        {
            return m_Instance;
        }

        public void RefreshDevices()
        {
            m_TaskQueue.Insert(0, new BasicTask(BasicTask.TaskTypes.TaskType_RefreshDevices));
            m_Controller.FindDevices();
        }

        public void SetOperatingDevice()
        {
            AddTask(new BasicTask(BasicTask.TaskTypes.TaskType_SetOperatingDevice));
        }

        public void ExportWorkout(Workout workout)
        {
            AddTask(new ExportWorkoutTask(workout));
        }

        public void ImportWorkouts()
        {
            AddTask(new ImportWorkoutsTask());
        }

        public void CancelAllPendingTasks()
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

            switch(m_TaskQueue[0].Type)
            {
                case BasicTask.TaskTypes.TaskType_Initialize:
                    {
                        break;
                    }
                case BasicTask.TaskTypes.TaskType_RefreshDevices:
                    {
                        ValidateManagerState();
                        break;
                    }
                case BasicTask.TaskTypes.TaskType_SetOperatingDevice:
                    {
                        m_Controller.FindDevices();
                        break;
                    }
                case BasicTask.TaskTypes.TaskType_ExportWorkout:
                    {
                        ValidateManagerState();

                        ExportWorkoutTask task = (ExportWorkoutTask)m_TaskQueue[0];

                        string fileName = task.Workout.Name;
                        MemoryStream textStream = new MemoryStream();

                        fileName = fileName.Replace('\\', '_');
                        fileName = fileName.Replace('/', '_');
                        fileName += ".tcx";

                        WorkoutExporter.ExportWorkout(task.Workout, textStream);
                        string xmlCode = Encoding.UTF8.GetString(textStream.GetBuffer());
                        m_Controller.WriteWorkouts(m_OperatingDevice,
                                                   xmlCode,
                                                   fileName);
 
                        break;
                    }
                case BasicTask.TaskTypes.TaskType_ImportWorkouts:
                    {
                        ValidateManagerState();

                        m_Controller.ReadWktWorkouts(m_OperatingDevice);
                        break;
                    }
            }

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

            if (m_TaskQueue.Count > 0 && task.Type != BasicTask.TaskTypes.TaskType_RefreshDevices)
            {
                StartNextTask();
            }
        }

        private void ValidateManagerState()
        {
            Trace.Assert(IsInitialized);
        }

        private void OnControllerReadyChanged(object sender, EventArgs e)
        {
            if (m_Controller.Ready && m_Controller.Initialize())
            {
                CompleteCurrentTask(true);
            }
            else
            {
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
                    SelectDeviceDialog dlg = new SelectDeviceDialog(this);

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        m_OperatingDevice = dlg.SelectedDevice;
                    }
                    else
                    {
                        CancelAllPendingTasks();
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
            Trace.Assert(m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportWorkouts);
            ImportWorkoutsTask task = (ImportWorkoutsTask)m_TaskQueue[0];

            task.WorkoutsXML = e.XmlData;
            CompleteCurrentTask(e.Success);
        }

        void OnTimeoutTimerTick(object sender, EventArgs e)
        {
            m_TimeoutTimer.Stop();

            if (m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ExportWorkout)
            {
                m_Controller.FireFinishWriteToDevice(false, "");
            }
            else if (m_TaskQueue[0].Type == BasicTask.TaskTypes.TaskType_ImportWorkouts)
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
                TaskType_ImportWorkouts
            };

            public TaskTypes Type
            {
                get { return m_Type; }
            }

            private TaskTypes m_Type;
        }

        public class ExportWorkoutTask : BasicTask
        {
            public ExportWorkoutTask(Workout workout) :
                base(TaskTypes.TaskType_ExportWorkout)
            {
                m_Workout = workout;
            }

            public Workout Workout
            {
                get { return m_Workout; }
            }

            private Workout m_Workout;
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

            private string m_WorkoutsXML;
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

        private static GarminDeviceManager m_Instance = new GarminDeviceManager();
    }
}

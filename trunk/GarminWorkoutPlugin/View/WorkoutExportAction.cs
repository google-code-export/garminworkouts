using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminWorkoutPlugin.Data;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.View
{
    class WorkoutExportAction : IAction
    {
        public WorkoutExportAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkoutExportAction_PropertyChanged);
        }

        #region IAction Members

        public bool Enabled
        {
            get { return true; }
        }

        public bool HasMenuArrow
        {
            get { return true; }
        }

        public System.Drawing.Image Image
        {
            get
            {
                return global::GarminWorkoutPlugin.Properties.Resources.Export;
            }
        }

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if (m_IsEnabled)
            {
                GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
                Control control = currentView.CreatePageControl();
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem(m_ResourceManager.GetString("ToDeviceText", currentView.UICulture),
                                        new EventHandler(ToDeviceEventHandler));
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(m_ResourceManager.GetString("ToFileText", currentView.UICulture),
                                        new EventHandler(ToFileEventHandler));
                menu.MenuItems.Add(menuItem);

                menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
            }
        }

        public string Title
        {
            get
            {
                Trace.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminWorkoutView));

                GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
                return m_ResourceManager.GetString("ExportText", currentView.UICulture);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        void WorkoutExportAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        public void ToDeviceEventHandler(object sender, EventArgs args)
        {
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;

            try
            {
                Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
                GarminDeviceManager deviceManager = new GarminDeviceManager();

                deviceManager.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                for (int i = 0; i < viewControl.Controls.Count; ++i)
                {
                    viewControl.Controls[i].Enabled = false;
                }
                m_IsEnabled = false;
                viewControl.Cursor = Cursors.WaitCursor;

                for (int i = 0; i < WorkoutManager.Instance.Workouts.Count; ++i)
                {
                    deviceManager.ExportWorkout(WorkoutManager.Instance.Workouts[i]);
                }
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(m_ResourceManager.GetString("DeviceCommunicationErrorText", currentView.UICulture),
                                m_ResourceManager.GetString("ErrorText", currentView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(m_ResourceManager.GetString("ExportFailedText", currentView.UICulture), m_ResourceManager.GetString("ErrorText", currentView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                throw e;
            }
        }

        public void ToFileEventHandler(object sender, EventArgs args)
        {
            FileStream file = null;
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = Options.DefaultExportDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < WorkoutManager.Instance.Workouts.Count; ++i)
                    {
                        Workout currentWorkout = WorkoutManager.Instance.Workouts[i];
                        string fileName = currentWorkout.Name;

                        fileName = fileName.Replace('\\', '_');
                        fileName = fileName.Replace('/', '_');
                        file = File.Create(dlg.SelectedPath + "\\" + fileName + ".tcx");

                        if (file != null)
                        {
                            WorkoutExporter.ExportWorkout(currentWorkout, file);
                            file.Close();
                        }
                        else
                        {
                            // Error creating file, throw error to display message below
                            throw new Exception();
                        }
                    }

                    MessageBox.Show(String.Format(m_ResourceManager.GetString("ExportSuccessText", currentView.UICulture), dlg.SelectedPath),
                                    m_ResourceManager.GetString("SuccessText", currentView.UICulture), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e)
                {
                    MessageBox.Show(m_ResourceManager.GetString("ExportFailedText", currentView.UICulture), m_ResourceManager.GetString("ErrorText", currentView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    throw e;
                }
                finally
                {
                    file.Close();
                }
            }
        }

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded)
        {
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            bool exportCancelled = false;

            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    manager.CancelAllPendingTasks();

                    MessageBox.Show(m_ResourceManager.GetString("DeviceCommunicationErrorText", currentView.UICulture),
                                    m_ResourceManager.GetString("ErrorText", currentView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_SetOperatingDevice)
                {
                    exportCancelled = true;
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ExportWorkout)
                {
                    GarminDeviceManager.ExportWorkoutTask concreteTask = (GarminDeviceManager.ExportWorkoutTask)task;

                    m_FailedExportList.Add(concreteTask.Workout);
                }
            }

            if (manager.AreAllTasksFinished)
            {
                Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();

                for (int i = 0; i < viewControl.Controls.Count; ++i)
                {
                    viewControl.Controls[i].Enabled = true;
                }
                m_IsEnabled = true;
                viewControl.Cursor = Cursors.Default;

                if (!exportCancelled)
                {
                    if (m_FailedExportList.Count == 0)
                    {
                        manager.TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                        MessageBox.Show(String.Format(m_ResourceManager.GetString("ExportSuccessText", currentView.UICulture), m_ResourceManager.GetString("DeviceText", currentView.UICulture)),
                                        m_ResourceManager.GetString("SuccessText", currentView.UICulture), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        m_FailedExportList.Clear();

                        MessageBox.Show(m_ResourceManager.GetString("ExportFailedText", currentView.UICulture), m_ResourceManager.GetString("ErrorText", currentView.UICulture),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private static bool m_IsEnabled = true;
        private List<Workout> m_FailedExportList = new List<Workout>();

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}

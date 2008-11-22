using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    class ExportAllWorkoutsAction : IAction
    {
        public ExportAllWorkoutsAction()
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
                return global::GarminFitnessPlugin.Properties.Resources.Export;
            }
        }

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if ((!GarminDeviceManager.GetInstance().IsInitialized && GarminDeviceManager.GetInstance().GetPendingTaskCount() == 1) ||
                GarminDeviceManager.GetInstance().AreAllTasksFinished)
            {
                GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;
                Control control = currentView.CreatePageControl();
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("ToDeviceText", GarminFitnessView.UICulture),
                                        new EventHandler(ToDeviceEventHandler));
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("ToFileText", GarminFitnessView.UICulture),
                                        new EventHandler(ToFileEventHandler));
                menu.MenuItems.Add(menuItem);

                menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
            }
        }

        public string Title
        {
            get
            {
                Trace.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView));

                return GarminFitnessView.ResourceManager.GetString("ExportAllText", GarminFitnessView.UICulture);
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
            try
            {
                GarminDeviceManager.GetInstance().TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();

                // Export using Communicator Plugin
                GarminDeviceManager.GetInstance().SetOperatingDevice();
                GarminDeviceManager.GetInstance().ExportWorkout(GarminWorkoutManager.Instance.Workouts);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(GarminFitnessView.ResourceManager.GetString("DeviceCommunicationErrorText", GarminFitnessView.UICulture),
                                GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportWorkoutsFailedText", GarminFitnessView.UICulture),
                                GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                throw e;
            }
        }

        public void ToFileEventHandler(object sender, EventArgs args)
        {
            FileStream file = null;
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = Options.DefaultExportDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
                    {
                        Workout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];
                        string fileName = Utils.GetWorkoutFilename(currentWorkout);

                        file = File.Create(dlg.SelectedPath + "\\" + fileName);
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

                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("ExportWorkoutsSuccessText", GarminFitnessView.UICulture), dlg.SelectedPath),
                                    GarminFitnessView.ResourceManager.GetString("SuccessText", GarminFitnessView.UICulture), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e)
                {
                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportWorkoutsFailedText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
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
            bool exportCancelled = false;

            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    exportCancelled = true;

                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("DeviceCommunicationErrorText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_SetOperatingDevice)
                {
                    exportCancelled = true;
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ExportWorkout)
                {
                    GarminDeviceManager.ExportWorkoutTask concreteTask = (GarminDeviceManager.ExportWorkoutTask)task;

                    m_FailedExportList.AddRange(concreteTask.Workouts);
                }
            }
            
            if (manager.AreAllTasksFinished)
            {
                Utils.ReleaseMainWindow();

                manager.TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                if (!exportCancelled)
                {
                    if (m_FailedExportList.Count == 0)
                    {
                        MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportWorkoutsSuccessText", GarminFitnessView.UICulture),
                                        GarminFitnessView.ResourceManager.GetString("SuccessText", GarminFitnessView.UICulture),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        m_FailedExportList.Clear();

                        MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportWorkoutsFailedText", GarminFitnessView.UICulture),
                                        GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private List<Workout> m_FailedExportList = new List<Workout>();

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

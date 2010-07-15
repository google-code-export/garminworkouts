using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Action"));
            }
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
                return global::GarminFitnessPlugin.Resources.Resources.Export;
            }
        }

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if ((!GarminDeviceManager.Instance.IsInitialized && GarminDeviceManager.Instance.GetPendingTaskCount() == 1) ||
                GarminDeviceManager.Instance.AreAllTasksFinished)
            {
                GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;
                Control control = currentView.CreatePageControl();
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("ToDeviceText"),
                                        new EventHandler(ToDeviceEventHandler));
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("ToFileText"),
                                        new EventHandler(ToFileEventHandler));
                menu.MenuItems.Add(menuItem);

                menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
            }
        }

        public string Title
        {
            get
            {
                Debug.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView));

                return GarminFitnessView.GetLocalizedString("ExportAllText");
            }
        }

        #endregion

        public void ToDeviceEventHandler(object sender, EventArgs args)
        {
            try
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();

                // Export using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ExportWorkout(GarminWorkoutManager.Instance.Workouts);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsFailedText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                throw e;
            }
        }

        public void ToFileEventHandler(object sender, EventArgs args)
        {
            FileStream file = null;
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            dlg.SelectedPath = Options.Instance.DefaultExportDirectory;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach (Workout currentWorkout in GarminWorkoutManager.Instance.Workouts)
                    {
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

                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("ExportWorkoutsSuccessText"), dlg.SelectedPath),
                                    GarminFitnessView.GetLocalizedString("SuccessText"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception e)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsFailedText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    throw e;
                }
                finally
                {
                    file.Close();
                }
            }
        }

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded, String errorText)
        {
            bool exportCancelled = false;

            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    exportCancelled = true;

                    MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
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
                else
                {
                    MessageBox.Show(errorText,
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsSuccessText"),
                                        GarminFitnessView.GetLocalizedString("SuccessText"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        m_FailedExportList.Clear();

                        MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsFailedText"),
                                        GarminFitnessView.GetLocalizedString("ErrorText"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private List<Workout> m_FailedExportList = new List<Workout>();

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

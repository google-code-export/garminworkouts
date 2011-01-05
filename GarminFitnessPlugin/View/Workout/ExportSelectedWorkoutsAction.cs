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
    class ExportSelectedWorkoutsAction : IAction
    {
        public ExportSelectedWorkoutsAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkoutExportAction_PropertyChanged);
        }

#region IAction Members

        public bool Enabled
        {
            get
            {
                GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;
                GarminWorkoutControl viewControl = (GarminWorkoutControl)currentView.GetCurrentView();

                return viewControl.SelectedConcreteWorkouts.Count > 0;
            }
        }

        public bool HasMenuArrow
        {
            get { return true; }
        }

        public System.Drawing.Image Image
        {
            get { return global::GarminFitnessPlugin.Resources.Resources.Export; }
        }

        public bool Visible
        {
            get { return true; }
        }

        public IList<string> MenuPath
        {
            get { return null; }
        }

        public void Refresh()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Enabled"));
            }
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if ((!GarminDeviceManager.Instance.IsInitialized && GarminDeviceManager.Instance.PendingTaskCount == 1) ||
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

                return GarminFitnessView.GetLocalizedString("ExportSelectedText");
            }
        }

#endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        void WorkoutExportAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        public void ToDeviceEventHandler(object sender, EventArgs args)
        {
            GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;

            try
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);
                GarminWorkoutControl viewControl = (GarminWorkoutControl)currentView.GetCurrentView();
                List<IWorkout> workoutsToExport = new List<IWorkout>();

                Utils.HijackMainWindow();

                foreach (IWorkout currentWorkout in viewControl.SelectedConcreteWorkouts)
                {
                    workoutsToExport.Add(currentWorkout);
                }

                // Export using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ExportWorkouts(workoutsToExport);
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
            try
            {
                GarminWorkoutControl viewControl = (GarminWorkoutControl)((GarminFitnessView)PluginMain.GetApplication().ActiveView).GetCurrentView();
                List<IWorkout> workoutsToExport = new List<IWorkout>();
                ExportWorkoutsDialog dlg;
                bool containsFITOnlyFeatures = false;

                // Populate list of workouts to export
                foreach (Workout currentWorkout in viewControl.SelectedConcreteWorkouts)
                {
                    containsFITOnlyFeatures = containsFITOnlyFeatures || currentWorkout.ContainsFITOnlyFeatures;

                    if (currentWorkout.GetSplitPartsCount() > 1)
                    {
                        List<WorkoutPart> splitParts = currentWorkout.SplitInSeperateParts();

                        // Replace the workout by it's parts
                        foreach (WorkoutPart currentPart in splitParts)
                        {
                            workoutsToExport.Add(currentPart);
                        }
                    }
                    else
                    {
                        workoutsToExport.Add(currentWorkout);
                    }
                }

                dlg = new ExportWorkoutsDialog(containsFITOnlyFeatures);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    UInt16 fileIdNumber = 0;

                    foreach (IWorkout currentWorkout in workoutsToExport)
                    {
                        string fileName = Utils.GetWorkoutFilename(currentWorkout, dlg.SelectedFormat);

                        file = File.Create(dlg.SelectedPath + "\\" + fileName);
                        if (file != null)
                        {
                            if (dlg.SelectedFormat == GarminWorkoutManager.FileFormats.FIT)
                            {
                                WorkoutExporter.ExportWorkoutToFIT(currentWorkout, file, fileIdNumber);
                            }
                            else
                            {
                                WorkoutExporter.ExportWorkout(currentWorkout, file);
                            }
                            file.Close();
                        }
                        else
                        {
                            // Error creating file, throw error to display message below
                            throw new Exception();
                        }

                        ++fileIdNumber;
                    }

                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("ExportWorkoutsSuccessText"), dlg.SelectedPath),
                                    GarminFitnessView.GetLocalizedString("SuccessText"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsFailedText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                if (file != null)
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
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.Initialize)
                {
                    exportCancelled = true;

                    MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.SetOperatingDevice)
                {
                    exportCancelled = true;
                }
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.ExportWorkout)
                {
                    GarminDeviceManager.ExportWorkoutTask concreteTask = (GarminDeviceManager.ExportWorkoutTask)task;

                    m_FailedExportList.AddRange(concreteTask.Workouts);
                    m_FailedExportErrors.Add(errorText);
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
                        String allErrors = String.Empty;

                        if (m_FailedExportErrors.Count > 0)
                        {
                            allErrors = "\n\n";

                            foreach (String error in m_FailedExportErrors)
                            {
                                allErrors += error + "\n";
                            }
                        }

                        m_FailedExportList.Clear();
                        m_FailedExportErrors.Clear();

                        MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportWorkoutsFailedText") + allErrors,
                                        GarminFitnessView.GetLocalizedString("ErrorText"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private List<IWorkout> m_FailedExportList = new List<IWorkout>();
        private List<String> m_FailedExportErrors = new List<String>();
    }
}

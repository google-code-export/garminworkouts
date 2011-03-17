using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class ImportWorkoutsAction : IAction
    {
        public ImportWorkoutsAction()
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
                return global::GarminFitnessPlugin.Resources.Resources.Import;
            }
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
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if ((!GarminDeviceManager.Instance.IsInitialized && GarminDeviceManager.Instance.PendingTaskCount == 1) ||
                GarminDeviceManager.Instance.AreAllTasksFinished)
            {
                Control control = PluginMain.GetApplication().ActiveView.CreatePageControl();
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("FromDeviceText"),
                                        new EventHandler(FromDeviceEventHandler));
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("FromFileText"),
                                        new EventHandler(FromFileEventHandler));
                menu.MenuItems.Add(menuItem);

                menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
            }
        }

        public string Title
        {
            get
            {
                return CommonResources.Text.ActionImport;
            }
        }

#endregion


        public void FromDeviceEventHandler(object sender, EventArgs args)
        {
            try
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();
                Options.Instance.LastImportCategory = null;
                Options.Instance.UseLastCategoryForAllImportedWorkout = false;

                // Import using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ImportWorkouts();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void FromFileEventHandler(object sender, EventArgs args)
        {
            // Ask for file to import
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult result;
            bool importError = false;

            dlg.Title = GarminFitnessView.GetLocalizedString("OpenFileText");
            dlg.Filter = GarminFitnessView.GetLocalizedString("FileDescriptionText") + " (*.tcx;*.wkt;*.fit)|*.tcx;*.wkt;*.fit";
            dlg.Multiselect = true;
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<UInt32, Workout> workoutIdMap = new Dictionary<UInt32, Workout>();
                String FITSchedulesFileName = String.Empty;
                Options.Instance.LastImportCategory = null;
                Options.Instance.UseLastCategoryForAllImportedWorkout = false;

                foreach (String filename in dlg.FileNames)
                {
                    Stream workoutStream = File.OpenRead(filename);

                    // Check if this is a FIT file or not
                    if (WorkoutImporter.IsFITFileStream(workoutStream))
                    {
                        if (WorkoutImporter.PeekFITFileType(workoutStream) == FITFileTypes.Schedules)
                        {
                            // Import schedules last since it refers data contained in the workouts.
                            //  Keep the filename to delay the loading of this file
                            FITSchedulesFileName = filename;
                        }
                        else
                        {
                            UInt32 workoutId;
                            Workout resultWorkout = WorkoutImporter.ImportWorkoutFromFIT(workoutStream, out workoutId);

                            if (resultWorkout == null)
                            {
                                importError = true;
                            }
                            else
                            {
                                workoutIdMap.Add(workoutId, resultWorkout);
                            }
                        }
                    }
                    else
                    {
                        if (!WorkoutImporter.ImportWorkout(workoutStream))
                        {
                            importError = true;
                        }
                    }
                    workoutStream.Close();
                }

                // Import schedules last since it refers data contained in the workouts
                if (!String.IsNullOrEmpty(FITSchedulesFileName))
                {
                    Stream workoutStream = File.OpenRead(FITSchedulesFileName);

                    if (!WorkoutImporter.ImportSchedulesFromFIT(workoutStream, workoutIdMap))
                    {
                        importError = true;
                    }
                }

                if (importError)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ImportWorkoutsErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded, String errorText)
        {
            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.Initialize)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if(errorText != String.Empty)
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
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

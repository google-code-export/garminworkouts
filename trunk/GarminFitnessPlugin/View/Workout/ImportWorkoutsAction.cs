using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    class ImportWorkoutsAction : IAction
    {
        public ImportWorkoutsAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkoutImportWorkoutsAction_PropertyChanged);
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

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            if ((!GarminDeviceManager.Instance.IsInitialized && GarminDeviceManager.Instance.GetPendingTaskCount() == 1) ||
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

        #region INotifyPropertyChanged Members

        void WorkoutImportWorkoutsAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        public void FromDeviceEventHandler(object sender, EventArgs args)
        {
            try
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();

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

            dlg.Title = GarminFitnessView.GetLocalizedString("OpenFileText");
            dlg.Filter = GarminFitnessView.GetLocalizedString("FileDescriptionText") + " (*.tcx;*.wkt)|*.tcx;*.wkt";
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream workoutStream = dlg.OpenFile();

                if (!WorkoutImporter.ImportWorkout(workoutStream))
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ImportWorkoutsErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                workoutStream.Close();
            }
        }

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded, String errorText)
        {
            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(errorText,
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ImportWorkouts)
                {
                    GarminDeviceManager.ImportWorkoutsTask concreteTask = (GarminDeviceManager.ImportWorkoutsTask)task;
                    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(concreteTask.WorkoutsXML));

                    if (!WorkoutImporter.ImportWorkout(stream))
                    {
                        MessageBox.Show(GarminFitnessView.GetLocalizedString("ImportWorkoutsErrorText"),
                                        GarminFitnessView.GetLocalizedString("ErrorText"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    stream.Close();
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

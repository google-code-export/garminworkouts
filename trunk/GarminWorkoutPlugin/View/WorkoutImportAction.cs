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
    class WorkoutImportAction : IAction
    {
        public WorkoutImportAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkoutImportAction_PropertyChanged);
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
                return global::GarminFitnessPlugin.Properties.Resources.Import;
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
                Control control = PluginMain.GetApplication().ActiveView.CreatePageControl();
                ContextMenu menu = new ContextMenu();
                MenuItem menuItem;

                menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("FromDeviceText", GarminFitnessView.UICulture),
                                        new EventHandler(FromDeviceEventHandler));
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(GarminFitnessView.ResourceManager.GetString("FromFileText", GarminFitnessView.UICulture),
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

        void WorkoutImportAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

        public void FromDeviceEventHandler(object sender, EventArgs args)
        {
            try
            {
                GarminDeviceManager.GetInstance().TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
                Control mainWindow = viewControl.Parent.Parent.Parent.Parent;

                for (int i = 0; i < mainWindow.Controls.Count; ++i)
                {
                    mainWindow.Controls[i].Enabled = false;
                }
                mainWindow.Cursor = Cursors.WaitCursor;

                GarminDeviceManager.GetInstance().SetOperatingDevice();
                GarminDeviceManager.GetInstance().ImportWorkouts();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(GarminFitnessView.ResourceManager.GetString("DeviceCommunicationErrorText", GarminFitnessView.UICulture),
                                GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void FromFileEventHandler(object sender, EventArgs args)
        {
            // Ask for file to import
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult result;

            dlg.Title = GarminFitnessView.ResourceManager.GetString("OpenFileText", GarminFitnessView.UICulture);
            dlg.Filter = GarminFitnessView.ResourceManager.GetString("FileDescriptionText", GarminFitnessView.UICulture) + " (*.tcx;*.wkt)|*.tcx;*.wkt";
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                Stream workoutStream = dlg.OpenFile();

                if (!WorkoutImporter.ImportWorkout(workoutStream))
                {
                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ImportErrorText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Utils.SaveWorkoutsToLogbook();
                }

                PluginMain.GetApplication().ActiveView.ShowPage("");
                workoutStream.Close();
            }
        }

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded)
        {
            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    manager.CancelAllPendingTasks();

                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("DeviceCommunicationErrorText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
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
                        MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ImportErrorText", GarminFitnessView.UICulture),
                                        GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        Utils.SaveWorkoutsToLogbook();
                    }

                    PluginMain.GetApplication().ActiveView.ShowPage("");
                    stream.Close();
                }
            }

            if (manager.AreAllTasksFinished)
            {
                Control viewControl = PluginMain.GetApplication().ActiveView.CreatePageControl();
                Control mainWindow = viewControl.Parent.Parent.Parent.Parent;

                for (int i = 0; i < mainWindow.Controls.Count; ++i)
                {
                    mainWindow.Controls[i].Enabled = true;
                }
                mainWindow.Cursor = Cursors.Default;

                manager.TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

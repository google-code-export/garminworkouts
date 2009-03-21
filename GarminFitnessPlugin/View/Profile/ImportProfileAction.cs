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
    class ImportProfileAction : IAction
    {
        public ImportProfileAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(WorkoutImportProfileAction_PropertyChanged);
        }

        #region IAction Members

        public bool Enabled
        {
            get { return true; }
        }

        public bool HasMenuArrow
        {
            get { return false; }
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
            try
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();

                // Import using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ImportProfile();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        void WorkoutImportProfileAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

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
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ImportProfile)
                {
                    GarminDeviceManager.ImportProfileTask concreteTask = (GarminDeviceManager.ImportProfileTask)task;
                    MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(concreteTask.ProfileXML));

                    if(!ProfileImporter.ImportProfile(stream))
                    {
                        MessageBox.Show(GarminFitnessView.GetLocalizedString("ImportProfileErrorText"),
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

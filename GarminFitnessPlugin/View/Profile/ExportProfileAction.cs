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
    class ExportProfileAction : IAction
    {
        public ExportProfileAction()
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ExportProfileAction_PropertyChanged);
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
                return global::GarminFitnessPlugin.Properties.Resources.Export;
            }
        }

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            try
            {
                GarminDeviceManager.GetInstance().TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Utils.HijackMainWindow();

                // Export using Communicator Plugin
                GarminDeviceManager.GetInstance().SetOperatingDevice();
                GarminDeviceManager.GetInstance().ExportProfile();
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

        public string Title
        {
            get
            {
                Trace.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView));

                return GarminFitnessView.ResourceManager.GetString("ExportText", GarminFitnessView.UICulture);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        void ExportProfileAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

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
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ExportProfile)
                {
                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportProfileFailedText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            
            if (manager.AreAllTasksFinished)
            {
                Utils.ReleaseMainWindow();

                manager.TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                if (!exportCancelled && succeeded)
                {
                    MessageBox.Show(GarminFitnessView.ResourceManager.GetString("ExportProfileSuccessText", GarminFitnessView.UICulture),
                                    GarminFitnessView.ResourceManager.GetString("SuccessText", GarminFitnessView.UICulture),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

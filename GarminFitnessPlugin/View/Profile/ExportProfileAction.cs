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
                return global::GarminFitnessPlugin.Resources.Resources.Export;
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

                // Export using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ExportProfile();
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

        public string Title
        {
            get
            {
                Debug.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView));

                return GarminFitnessView.GetLocalizedString("ExportText");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        void ExportProfileAction_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

        #endregion

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
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ExportProfile)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportProfileFailedText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (errorText != String.Empty)
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

                if (!exportCancelled && succeeded)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportProfileSuccessText"),
                                    GarminFitnessView.GetLocalizedString("SuccessText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

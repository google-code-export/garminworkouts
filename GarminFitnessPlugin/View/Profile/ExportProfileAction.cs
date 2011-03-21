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
    class ExportProfileAction : IAction
    {
        public ExportProfileAction()
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

                return GarminFitnessView.GetLocalizedString("ExportText");
            }
        }

#endregion

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
                else if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.ExportProfile)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportProfileFailedText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (!String.IsNullOrEmpty(errorText))
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

        
        public void ToDeviceEventHandler(object sender, EventArgs args)
        {
            GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;

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
                MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportProfileFailedText"),
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
                ExportFilesDialog dlg = new ExportFilesDialog(false);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    MemoryStream schedulesDataStream = new MemoryStream();

                    if (dlg.SelectedFormat == GarminWorkoutManager.FileFormats.FIT)
                    {
                        ProfileExporter.ExportProfileToFIT(dlg.SelectedPath);
                    }
                    else
                    {
                        file = File.Create(dlg.SelectedPath + "\\Profile.tcx");

                        ProfileExporter.ExportProfile(GarminProfileManager.Instance.UserProfile, file);
                        file.Close();
                    }

                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("ExportProfileSuccessText"), dlg.SelectedPath),
                                    GarminFitnessView.GetLocalizedString("SuccessText"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("ExportProfileFailedText"),
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

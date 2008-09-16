using System;
using System.Diagnostics;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
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
                return global::GarminWorkoutPlugin.Properties.Resources.Import;
            }
        }

        public void Refresh()
        {
        }

        public void Run(System.Drawing.Rectangle rectButton)
        {
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            Control control = PluginMain.GetApplication().ActiveView.CreatePageControl();
            ContextMenu menu = new ContextMenu();
            MenuItem menuItem;

            menuItem = new MenuItem(m_ResourceManager.GetString("FromDeviceText", currentView.UICulture),
                                    new EventHandler(FromDeviceEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(m_ResourceManager.GetString("FromFileText", currentView.UICulture),
                                    new EventHandler(FromFileEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
        }

        public string Title
        {
            get
            {
                Trace.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminWorkoutView));

                GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
                return m_ResourceManager.GetString("ImportText", currentView.UICulture);
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
        }

        public void FromFileEventHandler(object sender, EventArgs args)
        {
            // Ask for file to import
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult result;

            dlg.Title = m_ResourceManager.GetString("OpenFileText", currentView.UICulture);
            dlg.Filter = m_ResourceManager.GetString("FileDescriptionText", currentView.UICulture) + " (*.tcx)|*.tcx";
            dlg.DefaultExt = "tcx";
            dlg.CheckFileExists = true;
            result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                SelectCategoryDialog categoryDlg = new SelectCategoryDialog(currentView.UICulture);

                categoryDlg.ShowDialog();
                Stream workoutStream = dlg.OpenFile();

                if (!WorkoutImporter.ImportWorkout(workoutStream, categoryDlg.SelectedCategory))
                {
                    MessageBox.Show(m_ResourceManager.GetString("ImportErrorText", currentView.UICulture),
                                    m_ResourceManager.GetString("ErrorText", currentView.UICulture),
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

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
    }
}

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
    class PrintWorkoutsAction : IAction
    {
        public PrintWorkoutsAction()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Action"));
            }
        }

#region IAction Members

        public bool Enabled
        {
            get { return GarminWorkoutManager.Instance.Workouts.Count > 0; }
        }

        public bool HasMenuArrow
        {
            get { return true; }
        }

        public System.Drawing.Image Image
        {
            get { return global::GarminFitnessPlugin.Resources.Resources.Print; }
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

                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("PrintAllText"),
                                        new EventHandler(PrintAllEventHandler));
                menuItem.Enabled = GarminWorkoutManager.Instance.Workouts.Count > 0;
                menu.MenuItems.Add(menuItem);
                menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("PrintSelectedText"),
                                        new EventHandler(PrintSelectedEventHandler));
                menuItem.Enabled = (currentView.GetCurrentView() as GarminWorkoutControl).SelectedWorkouts.Count > 0;
                menu.MenuItems.Add(menuItem);

                menu.Show(control, control.PointToClient(new Point(rectButton.Right, rectButton.Top)));
            }
        }

        public string Title
        {
            get
            {
                Debug.Assert(PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView));
                
                return GarminFitnessView.GetLocalizedString("PrintText");
            }
        }

#endregion

        public void PrintAllEventHandler(object sender, EventArgs args)
        {
            try
            {
                PrintWorkouts(GarminWorkoutManager.Instance.Workouts);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void PrintSelectedEventHandler(object sender, EventArgs args)
        {
            try
            {
                GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;

                PrintWorkouts((currentView.GetCurrentView() as GarminWorkoutControl).SelectedConcreteWorkouts);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void PrintWorkouts(List<Workout> workoutsToPrint)
        {
            PrintOptionsDialog printDialog = new PrintOptionsDialog(workoutsToPrint);

            printDialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

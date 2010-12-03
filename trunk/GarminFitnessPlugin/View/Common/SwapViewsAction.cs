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

namespace GarminFitnessPlugin.View
{
    class SwapViewsAction : IAction
    {
        public SwapViewsAction()
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
            get { return false; }
        }

        public System.Drawing.Image Image
        {
            get
            {
                GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;

                switch (pluginView.CurrentView)
                {
                    case PluginViews.Workouts:
                        {
                            return CommonResources.Images.Redo16;
                        }
                    case PluginViews.Profile:
                        {
                            return CommonResources.Images.Undo16;
                        }
                }

                return null;
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
            GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;

            pluginView.SwapViews();
        }

        public string Title
        {
            get
            {
                GarminFitnessView pluginView = PluginMain.GetApplication().ActiveView as GarminFitnessView;
                
                switch(pluginView.CurrentView)
                {
                    case PluginViews.Workouts:
                    {
                        return GarminFitnessView.GetLocalizedString("GoToProfileText");
                    }
                    case PluginViews.Profile:
                    {
                        return GarminFitnessView.GetLocalizedString("GoToWorkoutsText");
                    }
                }

                return String.Empty;
            }
        }

#endregion

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}

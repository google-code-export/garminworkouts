using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using GarminWorkoutPlugin.Data;

namespace GarminWorkoutPlugin.View
{
    class GarminWorkoutView : IView
    {
        #region IView Members

        public System.Collections.Generic.IList<IAction> Actions
        {
            get
            {
                switch (m_CurrentView)
                {
                    case PluginViews.Workouts:
                        {
                            return m_WorkoutsViewActions;
                        }
                    case PluginViews.Profile:
                        {
                            return m_ProfileViewActions;
                        }
                }

                Trace.Assert(false);
                return null;
            }
        }

        public System.Guid Id
        {
            get { return GarminWorkoutPlugin.GUIDs.GarminWorkoutView; }
        }

        public string SubTitle
        {
            get
            {
                switch(m_CurrentView)
                {
                    case PluginViews.Workouts:
                        {
                            return m_ResourceManager.GetString("WorkoutsText", m_CurrentCulture);
                        }
                    case PluginViews.Profile:
                        {
                            return m_ResourceManager.GetString("ProfileText", m_CurrentCulture);
                        }
                    default:
                        {
                            Trace.Assert(false);
                            return "";
                        }
                }
            }
        }

        public void SubTitleClicked(System.Drawing.Rectangle subTitleRect)
        {
            GarminWorkoutView currentView = (GarminWorkoutView)PluginMain.GetApplication().ActiveView;
            Control control = currentView.CreatePageControl();
            ContextMenu menu = new ContextMenu();
            MenuItem menuItem;

            menuItem = new MenuItem(m_ResourceManager.GetString("WorkoutsText", m_CurrentCulture),
                                    new EventHandler(WorkoutsViewEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(m_ResourceManager.GetString("ProfileText", m_CurrentCulture),
                                    new EventHandler(ProfileViewEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(control, control.PointToClient(new Point(subTitleRect.X, subTitleRect.Bottom)));
        }

        public bool SubTitleHyperlink
        {
            get { return true; }
        }

        public string TasksHeading
        {
            get { return m_ResourceManager.GetString("GarminFitnessText", m_CurrentCulture); }
        }

        #endregion

        #region IDialogPage Members

        public System.Windows.Forms.Control CreatePageControl()
        {
            if (m_ViewControls[(int)m_CurrentView] == null)
            {
                switch (m_CurrentView)
                {
                    case PluginViews.Workouts:
                        {
                            m_ViewControls[(int)m_CurrentView] = new GarminWorkoutControl();
                            break;
                        }
                    case PluginViews.Profile:
                        {
                            m_ViewControls[(int)m_CurrentView] = new GarminProfileControl();
                            break;
                        }
                }
            }

            return (UserControl)m_ViewControls[(int)m_CurrentView];
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public bool HidePage()
        {
            return true;
        }

        public string PageName
        {
            get { return m_ResourceManager.GetString("GarminFitnessText", m_CurrentCulture); }
        }

        public void ShowPage(string bookmark)
        {
            CreatePageControl();

            // This is to make sure that the categories are up to date
            m_ViewControls[(int)m_CurrentView].RefreshUIFromLogbook();
        }

        public IPageStatus Status
        {
            get { throw new System.Exception("The method or operation is not implemented."); }
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            CreatePageControl();

            m_ViewControls[(int)m_CurrentView].ThemeChanged(visualTheme);
        }

        public string Title
        {
            get { return m_ResourceManager.GetString("GarminFitnessText", m_CurrentCulture); }
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            m_CurrentCulture = culture;
            m_ViewControls[(int)m_CurrentView].UICultureChanged(culture);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public GarminWorkoutView()
        {
            PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            PluginMain.LogbookChanged += new PluginMain.LogbookChangedEventHandler(OnLogbookChanged);

            m_ViewControls = new IGarminPluginControl[]
                    {
                        null,
                        null,
                    };
        }

        private void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            CreatePageControl();

            m_ViewControls[(int)m_CurrentView].RefreshUIFromLogbook();
        }

        public void WorkoutsViewEventHandler(object sender, EventArgs args)
        {
            if (m_CurrentView != PluginViews.Workouts)
            {
                SwapViews();
            }
        }

        public void ProfileViewEventHandler(object sender, EventArgs args)
        {
            if (m_CurrentView != PluginViews.Profile)
            {
                SwapViews();
            }
        }

        private void SwapViews()
        {
            Control oldControl = CreatePageControl();
            Control currentControl;

            // Swap them
            switch(m_CurrentView)
            {
                case PluginViews.Workouts:
                    m_CurrentView = PluginViews.Profile;
                    break;
                case PluginViews.Profile:
                    m_CurrentView = PluginViews.Workouts;
                    break;
                default:
                    Trace.Assert(false);
                    break;
            }

            // Make sure the other control is created
            currentControl = CreatePageControl();
            // Modify UI hierarchy
            if (currentControl.Parent == null)
            {
                currentControl.Parent = oldControl.Parent;
                oldControl.Parent = null;
            }

            // Refresh UI
            currentControl.Parent.Invalidate();
            PropertyChanged(this, new PropertyChangedEventArgs("SubTitle"));

            // Refresh the lsit of actions also, inelegant, but working
            PluginMain.GetApplication().ShowView(GUIDs.DailyActivityView, "");
            PluginMain.GetApplication().ShowView(GUIDs.GarminWorkoutView, "");
        }

        public static CultureInfo UICulture
        {
            get { return m_CurrentCulture; }
        }

        private IAction[] m_WorkoutsViewActions = new IAction[]
            {
                new WorkoutExportAllAction(),
                new WorkoutExportSelectedAction(),
                new WorkoutImportAction()
            };

        private IAction[] m_ProfileViewActions = new IAction[]
            {
                new WorkoutImportAction()
            };

        private IGarminPluginControl[] m_ViewControls = null;
        private PluginViews m_CurrentView = PluginViews.Workouts;

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
        private static CultureInfo m_CurrentCulture;
    }
}

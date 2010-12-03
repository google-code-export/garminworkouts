using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using ZoneFiveSoftware.Common.Visuals;
using SportTracksPluginFramework;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessView : STFrameworkView
    {
        public GarminFitnessView()
        {
            PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            PluginMain.LogbookChanged += new PluginMain.LogbookChangedEventHandler(OnLogbookChanged);
            Options.Instance.OptionsChanged += new Options.OptionsChangedEventHandler(OnOptionsChanged);

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Instance"));
            }

            m_ViewControls = new IGarminFitnessPluginControl[]
                    {
                        null,
                        null,
                    };
        }

#region STFrameworkView Members

        public override System.Collections.Generic.IList<IAction> Actions
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

                Debug.Assert(false);
                return null;
            }
        }

        public override System.Guid Id
        {
            get { return GarminFitnessPlugin.GUIDs.GarminFitnessView; }
        }

        public override string SubTitle
        {
            get
            {
                switch(m_CurrentView)
                {
                    case PluginViews.Workouts:
                        {
                            return GarminFitnessView.GetLocalizedString("WorkoutsText");
                        }
                    case PluginViews.Profile:
                        {
                            return GarminFitnessView.GetLocalizedString("ProfileText");
                        }
                    default:
                        {
                            Debug.Assert(false);
                            return "";
                        }
                }
            }
        }

        public override void SubTitleClicked(System.Drawing.Rectangle subTitleRect)
        {
            GarminFitnessView currentView = (GarminFitnessView)PluginMain.GetApplication().ActiveView;
            Control control = currentView.CreatePageControl();
            ContextMenu menu = new ContextMenu();
            MenuItem menuItem;

            menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("WorkoutsText"),
                                    new EventHandler(WorkoutsViewEventHandler));
            menu.MenuItems.Add(menuItem);
            menuItem = new MenuItem(GarminFitnessView.GetLocalizedString("ProfileText"),
                                    new EventHandler(ProfileViewEventHandler));
            menu.MenuItems.Add(menuItem);

            menu.Show(control, control.PointToClient(new Point(subTitleRect.X, subTitleRect.Bottom)));
        }

        public override bool SubTitleHyperlink
        {
            get { return true; }
        }

        public override string TasksHeading
        {
            get { return GarminFitnessView.GetLocalizedString("GarminFitnessText"); }
        }

        public override Control ViewControl
        {
            get
            {
                if (m_MainControl == null)
                {
                    m_MainControl = new GarminFitnessMainControl();

                    SetupCurrentView();
                    GetCurrentView().RefreshUIFromLogbook();
                }

                if (PluginMain.GetApplication().Logbook != null)
                {
                    byte[] extensionData = PluginMain.GetApplication().Logbook.GetExtensionData(GUIDs.PluginMain);

                    if (extensionData == null || extensionData.Length == 0)
                    {
                        GarminWorkoutManager.Instance.RemoveAllWorkouts();
                        GarminProfileManager.Instance.UserProfile.Cleanup();
                        Options.Instance.ResetLogbookSettings();

                        // Show the wizard on first run
                        GarminFitnessSetupWizard wizard = new GarminFitnessSetupWizard();

                        wizard.ShowDialog();
                    }
                }

                return m_MainControl;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public override bool HidePage()
        {
            return true;
        }

        public override string PageName
        {
            get { return GarminFitnessView.GetLocalizedString("GarminFitnessText"); }
        }

        public override void ShowPage(string bookmark)
        {
            GetCurrentView().RefreshCalendar();
        }

        public override IPageStatus Status
        {
            get { throw new System.Exception("The method or operation is not implemented."); }
        }

        public override void ThemeChanged(ITheme visualTheme)
        {
            GetCurrentView().ThemeChanged(visualTheme);
        }

        public override string Title
        {
            get { return GarminFitnessView.GetLocalizedString("GarminFitnessText"); }
        }

        public override void UICultureChanged(CultureInfo culture)
        {
            GetCurrentView().UICultureChanged(culture);
        }

        public override Guid MainPluginId
        {
            get { return GUIDs.PluginMain; }
        }

        public override event PropertyChangedEventHandler PropertyChanged;

#endregion

        private void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            if (m_ViewControls[(int)m_CurrentView] != null)
            {
                GetCurrentView().RefreshUIFromLogbook();
            }
        }

        void OnOptionsChanged(PropertyChangedEventArgs changedProperty)
        {
            if (changedProperty.PropertyName == "EnableAutoSplitWorkouts")
            {
                if (!Options.Instance.AllowSplitWorkouts)
                {
                    List<Workout> workoutsToSplit = new List<Workout>();

                    // Check if all workouts can handle the change
                    for(int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; i++)
                    {
                        IWorkout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];

                        if (currentWorkout is Workout && currentWorkout.GetSplitPartsCount() > 1)
                        {
                            workoutsToSplit.Add(currentWorkout as Workout);
                        }
                    }

                    // Go ahead, split them up
                    if (workoutsToSplit.Count > 0)
                    {
                        if (MessageBox.Show(GarminFitnessView.GetLocalizedString("WorkoutsWereSplitText"),
                                            GarminFitnessView.GetLocalizedString("WarningText"),
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            foreach (Workout workout in workoutsToSplit)
                            {
                                List<WorkoutPart> splitParts = workout.SplitInSeperateParts();

                                foreach (WorkoutPart part in splitParts)
                                {
                                    part.ConvertToWorkout();
                                }
                            }
                            GarminWorkoutManager.Instance.RemoveWorkouts(workoutsToSplit);
                        }
                        else
                        {
                            Options.Instance.AllowSplitWorkouts = true;
                        }
                    }
                }
            }
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

        public IGarminFitnessPluginControl GetCurrentView()
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

                m_ViewControls[(int)m_CurrentView].RefreshUIFromLogbook();
            }

            return m_ViewControls[(int)m_CurrentView];
        }

        public PluginViews CurrentView
        {
            get { return m_CurrentView; }
        }

        public void SwapViews()
        {
            ((UserControl)GetCurrentView()).Visible = false;

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
                    Debug.Assert(false);
                    break;
            }

            SetupCurrentView();

            // Refresh the list of actions, ugly but working
            PluginMain.GetApplication().ShowView(GUIDs.DailyActivityView, "");
            PluginMain.GetApplication().ShowView(GUIDs.GarminFitnessView, "");

            GetCurrentView().RefreshCalendar();
        }

        private void SetupCurrentView()
        {
            // Update UI to view
            UserControl currentControl = (UserControl)GetCurrentView();

            if (currentControl.Parent == null)
            {
                currentControl.Parent = CreatePageControl();
                currentControl.Dock = DockStyle.Fill;
            }

            currentControl.Visible = true;
        }

        public static string GetLocalizedString(string name)
        {
            try
            {
                return ResourceManager.GetString(name);
            }
            catch
            {
                Debug.Assert(false, "Unable to find string resource named " + name);

                return String.Empty;
            }
        }

        public static ResourceManager ResourceManager
        {
            get { return m_ResourceManager; }
        }

        public static CultureInfo UICulture
        {
            get
            {
                return Thread.CurrentThread.CurrentCulture;
            }
        }

        private IAction[] m_WorkoutsViewActions = new IAction[]
            {
                new SwapViewsAction(),
                new ExportAllWorkoutsAction(),
                new ExportSelectedWorkoutsAction(),
                new ImportWorkoutsAction(),
                new PrintWorkoutsAction()
            };

        private IAction[] m_ProfileViewActions = new IAction[]
            {
                new SwapViewsAction(),
                new ExportProfileAction(),
                new ImportProfileAction()
            };

        private UserControl m_MainControl = null;
        private IGarminFitnessPluginControl[] m_ViewControls = null;
        private PluginViews m_CurrentView = PluginViews.Workouts;

        private static ResourceManager m_ResourceManager = new ResourceManager("GarminFitnessPlugin.Resources.StringResources",
                                                                               Assembly.GetExecutingAssembly());
    }
}

using System;
using System.ComponentModel;
using System.Diagnostics;
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
                return new IAction[]
                {
                    new WorkoutExportAction(),
                    new WorkoutImportAction()
                };
            }
        }

        public System.Guid Id
        {
            get { return GarminWorkoutPlugin.GUIDs.GarminWorkoutView; }
        }

        public string SubTitle
        {
            get { return null; }
        }

        public void SubTitleClicked(System.Drawing.Rectangle subTitleRect)
        {
        }

        public bool SubTitleHyperlink
        {
            get { return false; }
        }

        public string TasksHeading
        {
            get { return m_ResourceManager.GetString("WorkoutsText", m_CurrentCulture); }
        }

        #endregion

        #region IDialogPage Members

        public System.Windows.Forms.Control CreatePageControl()
        {
            PropertyChanged += new PropertyChangedEventHandler(OnPropertyChanged);
            PluginMain.LogbookChanged += new PluginMain.LogbookChangedEventHandler(OnLogbookChanged);

            if (m_ViewControl == null)
            {
                m_ViewControl = new GarminWorkoutControl();
            }

            return m_ViewControl;
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
            get { return m_ResourceManager.GetString("GarminWorkoutsText", m_CurrentCulture); }
        }

        public void ShowPage(string bookmark)
        {
            Trace.Assert(m_ViewControl != null);

            // This is to make sure that the categories are up to date
            m_ViewControl.BuildWorkoutsList();
        }

        public IPageStatus Status
        {
            get { throw new System.Exception("The method or operation is not implemented."); }
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            m_ViewControl.ThemeChanged(visualTheme);
        }

        public string Title
        {
            get { return m_ResourceManager.GetString("WorkoutsText", m_CurrentCulture); }
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            m_CurrentCulture = culture;
            m_ViewControl.UICultureChanged(culture);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public GarminWorkoutView()
        {
            PropertyChanged = null;
        }

        private void OnLogbookChanged(object sender, ILogbook oldLogbook, ILogbook newLogbook)
        {
            // Ewwww should call another function but this does the job (and some extra work)...
            m_ViewControl.UICultureChanged(m_CurrentCulture);
        }

        public CultureInfo UICulture
        {
            get { return m_CurrentCulture; }
        }

        private GarminWorkoutControl m_ViewControl = null;
        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
        private CultureInfo m_CurrentCulture;
    }
}

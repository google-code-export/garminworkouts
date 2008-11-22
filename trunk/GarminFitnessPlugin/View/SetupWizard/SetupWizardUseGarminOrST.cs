using ZoneFiveSoftware.Common.Visuals;
using System.ComponentModel;

namespace GarminFitnessPlugin.View
{
    class SetupWizardUseGarminOrST : IExtendedWizardPage
    {
        public SetupWizardUseGarminOrST(ExtendedWizard parentWizard)
            :
            base(parentWizard)
        {
            PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(SetupWizardUseGarminOrST_PropertyChanged);
        }

        void SetupWizardUseGarminOrST_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
        }

#region IExtendedWizardPage Members

        public override void FinishClicked(CancelEventArgs e)
        {
        }

        public override void NextClicked(CancelEventArgs e)
        {
            if (m_Control.IsIndependentSetupSelected)
            {
                IExtendedWizardPage nextPage = Wizard.GetPageByType(typeof(SetupWizardSetupSTGarminZones));

                Wizard.ShowPage(nextPage);

                e.Cancel = true;
            }
        }

        public override void PrevClicked(CancelEventArgs e)
        {
        }

#endregion

#region IWizardPage Members

        public override bool CanFinish
        {
            get { return true; }
        }

        public override bool CanNext
        {
            get { return true; }
        }

        public override bool CanPrev
        {
            get { return true; }
        }

#endregion

#region IDialogPage Members

        public override System.Windows.Forms.Control CreatePageControl()
        {
            if (m_Control == null)
            {
                m_Control = new SetupWizardUseGarminOrSTControl();
            }

            return m_Control;
        }

        public override bool HidePage()
        {
            return true;
        }

        public override string PageName
        {
            get { throw new System.Exception("The method or operation is not implemented."); }
        }

        public override void ShowPage(string bookmark)
        {
            
        }

        public override IPageStatus Status
        {
            get { throw new System.Exception("The method or operation is not implemented."); }
        }

        public override void ThemeChanged(ITheme visualTheme)
        {
        }

        public override string Title
        {
            get { return GarminFitnessView.ResourceManager.GetString("GarminOrSTText", GarminFitnessView.UICulture); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

#endregion

#region INotifyPropertyChanged Members

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardUseGarminOrSTControl m_Control = null;
    }
}

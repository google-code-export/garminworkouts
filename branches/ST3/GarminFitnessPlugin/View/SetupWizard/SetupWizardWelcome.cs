using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class SetupWizardWelcome : IExtendedWizardPage
    {
        public SetupWizardWelcome(ExtendedWizard parentWizard) :
            base(parentWizard)
        {
        }


#region IExtendedWizardPage Members

        public override void FinishClicked(CancelEventArgs e)
        {
            
        }

        public override void NextClicked(CancelEventArgs e)
        {
        }

        public override void PrevClicked(CancelEventArgs e)
        {
        }

#endregion

#region IWizardPage Members

        public override bool CanFinish
        {
            get { return false; }
        }

        public override bool CanNext
        {
            get { return true; }
        }

        public override bool CanPrev
        {
            get { return false; }
        }

#endregion

#region IDialogPage Members

        public override ExtendedWizardPageControl CreatePageControl(ExtendedWizard wizard)
        {
            if(m_Control == null)
            {
                m_Control = new SetupWizardWelcomeControl(wizard);

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Control"));
                }
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
            get { return GarminFitnessView.GetLocalizedString("WelcomeText"); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

#endregion

#region INotifyPropertyChanged Members

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardWelcomeControl m_Control = null;
    }
}

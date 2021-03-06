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
        }

#region IExtendedWizardPage Members

        public override void FinishClicked(CancelEventArgs e)
        {
        }

        public override void NextClicked(CancelEventArgs e)
        {
            if (((GarminFitnessSetupWizard)Wizard).IsIndependentZonesSetupSelected)
            {
                IExtendedWizardPage nextPage = Wizard.GetPageByType(typeof(SetupWizardSetupSTGarminZones));

                Wizard.ShowPage(nextPage);

                e.Cancel = true;
            }
        }

        public override void PrevClicked(CancelEventArgs e)
        {
        }

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
            get { return true; }
        }
        public override ExtendedWizardPageControl CreatePageControl(ExtendedWizard wizard)
        {
            if (m_Control == null)
            {
                m_Control = new SetupWizardUseGarminOrSTControl(wizard);

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
            get { return GarminFitnessView.GetLocalizedString("GarminOrSTText"); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardUseGarminOrSTControl m_Control = null;
    }
}

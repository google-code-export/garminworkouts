using System;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class SetupWizardSetupSTGarminZones : IExtendedWizardPage
    {
        public SetupWizardSetupSTGarminZones(ExtendedWizard parentWizard) :
            base(parentWizard)
        {
        }

#region IExtendedWizardPage implementation

        public override void FinishClicked(CancelEventArgs e)
        {
        }

        public override void NextClicked(CancelEventArgs e)
        {
            IExtendedWizardPage nextPage = Wizard.GetPageByType(typeof(SetupWizardSportTracksZones));

            Wizard.ShowPage(nextPage);

            e.Cancel = true;
        }

        public override void PrevClicked(CancelEventArgs e)
        {
            IExtendedWizardPage prevPage = Wizard.GetPageByType(typeof(SetupWizardUseGarminOrST));

            Wizard.ShowPage(prevPage);

            e.Cancel = true;
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
                m_Control = new SetupWizardSetupSTGarminZonesControl(wizard);

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
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override void ShowPage(string bookmark)
        {
        }

        public override ZoneFiveSoftware.Common.Visuals.IPageStatus Status
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public override void ThemeChanged(ZoneFiveSoftware.Common.Visuals.ITheme visualTheme)
        {
        }

        public override string Title
        {
            get { return GarminFitnessView.GetLocalizedString("SetupGarminOrSTZonesText"); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardSetupSTGarminZonesControl m_Control = null;
    }
}

using System;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class SetupWizardEditBikingProfile : IExtendedWizardPage
    {
        public SetupWizardEditBikingProfile(ExtendedWizard parentWizard)
            :
            base(parentWizard)
        {
        }

#region IExtendedWizardPage implementation

        public override void FinishClicked(CancelEventArgs e)
        {
        }


        public override void NextClicked(CancelEventArgs e)
        {
            IExtendedWizardPage nextPage = Wizard.GetPageByType(typeof(SetupWizardCompleted));

            Wizard.ShowPage(nextPage);

            e.Cancel = true;
        }

        public override void PrevClicked(CancelEventArgs e)
        {
        }

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

        public override ExtendedWizardPageControl CreatePageControl(ExtendedWizard wizard)
        {
            if (m_Control == null)
            {
                m_Control = new SetupWizardEditBikingProfileControl(wizard);

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
            get { return GarminFitnessView.GetLocalizedString("EditBikingProfileText"); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardEditBikingProfileControl m_Control = null;
    }
}

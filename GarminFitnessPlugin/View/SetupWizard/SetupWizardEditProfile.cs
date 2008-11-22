using System;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class SetupWizardEditProfile : IExtendedWizardPage
    {
        public SetupWizardEditProfile(ExtendedWizard parentWizard)
            :
            base(parentWizard)
        {
            PropertyChanged += new PropertyChangedEventHandler(SetupWizardEditProfile_PropertyChanged);
        }

#region IExtendedWizardPage implementation

        void SetupWizardEditProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

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
            IExtendedWizardPage prevPage = Wizard.GetPageByType(typeof(SetupWizardImportOrEditProfile));

            Wizard.ShowPage(prevPage);

            e.Cancel = true;
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

        public override System.Windows.Forms.Control CreatePageControl()
        {
            if (m_Control == null)
            {
                m_Control = new SetupWizardEditProfileControl();
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
            get { return "EditProfile"; }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardEditProfileControl m_Control = null;
    }
}

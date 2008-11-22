using System;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class SetupWizardCompleted : IExtendedWizardPage
    {
        public SetupWizardCompleted(ExtendedWizard parentWizard)
            :
            base(parentWizard)
        {
            PropertyChanged += new PropertyChangedEventHandler(SetupWizardCompleted_PropertyChanged);
        }

#region IExtendedWizardPage implementation

        void SetupWizardCompleted_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public override void FinishClicked(CancelEventArgs e)
        {
        }

        public override void NextClicked(CancelEventArgs e)
        {
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
            get { return false; }
        }

        public override bool CanPrev
        {
            get { return true; }
        }

        public override System.Windows.Forms.Control CreatePageControl()
        {
            if (m_Control == null)
            {
                m_Control = new SetupWizardCompletedControl();
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
            get { return "Completed"; }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        SetupWizardCompletedControl m_Control = null;
    }
}

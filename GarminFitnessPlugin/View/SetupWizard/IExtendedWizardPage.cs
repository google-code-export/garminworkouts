using System.ComponentModel;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    abstract class IExtendedWizardPage : IWizardPage
    {
        public IExtendedWizardPage(ExtendedWizard parent)
        {
            m_Wizard = parent;
        }

        public abstract void FinishClicked(CancelEventArgs e);
        public abstract void NextClicked(CancelEventArgs e);
        public abstract void PrevClicked(CancelEventArgs e);

#region IWizardPage Members
        public abstract bool CanFinish { get; }
        public abstract bool CanNext { get; }
        public abstract bool CanPrev { get; }
#endregion

#region IDialogPage Members
        public abstract ExtendedWizardPageControl CreatePageControl(ExtendedWizard wizard);
        public abstract bool HidePage();
        public abstract string PageName { get; }
        public abstract void ShowPage(string bookmark);
        public abstract IPageStatus Status { get; }
        public abstract void ThemeChanged(ITheme visualTheme);
        public abstract string Title { get; }
        public abstract void UICultureChanged(System.Globalization.CultureInfo culture);
#endregion

#region INotifyPropertyChanged Members
        public abstract event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
#endregion

        public Control CreatePageControl()
        {
            return CreatePageControl(Wizard);
        }

        public ExtendedWizard Wizard
        {
            get { return m_Wizard; }
        }

        ExtendedWizard m_Wizard = null;
    }
}

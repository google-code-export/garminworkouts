using System.Diagnostics;
using System.Windows.Forms;

namespace GarminFitnessPlugin.View
{
    class ExtendedWizardPageControl : UserControl
    {
        public ExtendedWizardPageControl()
        {
            m_Wizard = null;
        }

        public ExtendedWizardPageControl(ExtendedWizard wizard)
        {
            m_Wizard = wizard;
        }

        public ExtendedWizard Wizard
        {
            get { return m_Wizard; }
        }

        ExtendedWizard m_Wizard = null;
    }
}

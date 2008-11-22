using System;
using System.Collections.Generic;
using System.ComponentModel;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class ExtendedWizard : Wizard
    {
        public IExtendedWizardPage GetPageByType(Type type)
        {
            for (int i = 0; i < m_Pages.Count; ++i)
            {
                if (m_Pages[i].GetType() == type)
                {
                    return m_Pages[i];
                }
            }

            return null;
        }

        protected override void OnShown(System.EventArgs e)
        {
            bannerPage.ThemeChanged(PluginMain.GetApplication().VisualTheme);

            base.OnShown(e);
        }

        protected override void FinishClicked()
        {
            CancelEventArgs e = new CancelEventArgs(false);

            // Notify current page
            if (ActivePage != null && ActivePage.GetType().IsSubclassOf(typeof(IExtendedWizardPage)))
            {
                IExtendedWizardPage concretePage = (IExtendedWizardPage)ActivePage;

                concretePage.FinishClicked(e);
            }

            if (!e.Cancel)
            {
                base.FinishClicked();
            }
        }

        protected override void NextClicked()
        {
            CancelEventArgs e = new CancelEventArgs(false);

            // Notify current page
            if (ActivePage != null && ActivePage.GetType().IsSubclassOf(typeof(IExtendedWizardPage)))
            {
                IExtendedWizardPage concretePage = (IExtendedWizardPage)ActivePage;

                concretePage.NextClicked(e);
            }

            if (!e.Cancel)
            {
                base.NextClicked();
            }
        }

        protected override void PrevClicked()
        {
            CancelEventArgs e = new CancelEventArgs(false);

            // Notify current page
            if (ActivePage != null && ActivePage.GetType().IsSubclassOf(typeof(IExtendedWizardPage)))
            {
                IExtendedWizardPage concretePage = (IExtendedWizardPage)ActivePage;

                concretePage.PrevClicked(e);
            }

            if (!e.Cancel)
            {
                base.PrevClicked();
            }
        }

        protected override bool CanFinish
        {
            get
            {
                if (ActivePage != null)
                {
                    return ActivePage.CanFinish;
                }

                return base.CanFinish;
            }
        }

        protected override bool CanNext
        {
            get
            {
                if (ActivePage != null)
                {
                    return ActivePage.CanNext;
                }

                return base.CanNext;
            }
        }

        protected override bool CanPrev
        {
            get
            {
                if (ActivePage != null)
                {
                    return ActivePage.CanPrev;
                }

                return base.CanPrev;
            }
        }

        public new IList<IExtendedWizardPage> Pages
        {
            get
            {
                return m_Pages;
            }
            set
            {
                m_Pages = value;

                List<IWizardPage> pages = new List<IWizardPage>();
                for (int i = 0; i < m_Pages.Count; ++i)
                {
                    pages.Add(m_Pages[i]);
                }
                base.Pages = pages;
            }
        }

        IList<IExtendedWizardPage> m_Pages;
    }
}

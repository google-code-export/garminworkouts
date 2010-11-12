using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class ExtendedWizard : Wizard
    {
        public ExtendedWizard()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            m_BaseWizardHeight = this.Height;
            m_ExcessWizardHeight = m_BaseWizardHeight - panelMain.Height;
        }

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
            if (PluginMain.GetApplication() != null)
            {
                bannerPage.ThemeChanged(PluginMain.GetApplication().VisualTheme);
            }

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
                GoNext();
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
                GoPrev();
            }
        }

        public new void GoNext()
        {
            Debug.Assert((ActivePageNum + 1) < Pages.Count);

            ShowPage(Pages[ActivePageNum + 1]);
        }

        public new void GoPrev()
        {
            Debug.Assert((ActivePageNum - 1) >= 0);

            ShowPage(Pages[ActivePageNum - 1]);
        }

        public new void ShowPage(IWizardPage page)
        {
            int requiredHeight = page.CreatePageControl().Height + m_ExcessWizardHeight;

            base.ShowPage(page);

            if (requiredHeight > m_BaseWizardHeight)
            {
                // Realign the center
                this.Top += (this.Height - requiredHeight) / 2;

                this.Height = requiredHeight;
            }
            else if(this.Height != m_BaseWizardHeight)
            {
                // Realign the center
                this.Top += (this.Height - m_BaseWizardHeight) / 2;

                // Set back to our default value
                this.Height = m_BaseWizardHeight;
            }

            this.Invalidate();
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
        int m_BaseWizardHeight;
        int m_ExcessWizardHeight;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ExtendedWizard
            // 
            this.ClientSize = new System.Drawing.Size(553, 410);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ExtendedWizard";
            this.ResumeLayout(false);

        }
    }
}

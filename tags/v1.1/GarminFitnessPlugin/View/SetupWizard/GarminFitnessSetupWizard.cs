using System.Collections.Generic;
using System.Drawing;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    class GarminFitnessSetupWizard : ExtendedWizard
    {
        public GarminFitnessSetupWizard()
        {
            InitializeComponent();

            List<IExtendedWizardPage> pages = new List<IExtendedWizardPage>();

            pages.Add(new SetupWizardWelcome(this));
            pages.Add(new SetupWizardUseGarminOrST(this));
            pages.Add(new SetupWizardSportTracksZones(this));
            pages.Add(new SetupWizardGarminCategoryAssociation(this));
            pages.Add(new SetupWizardImportOrEditProfile(this));
            pages.Add(new SetupWizardCompleted(this));

            // These pages are not directly accessible. They are used only when needed.
            //  This means the last page just above this should have it's CanNext property
            //  returning false
            pages.Add(new SetupWizardSetupSTGarminZones(this));

            // Don't change the order of these two, they are meant to follow each other
            pages.Add(new SetupWizardEditProfile(this));
            pages.Add(new SetupWizardEditBikingProfile(this));

            Pages = pages;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(532, 367);
            this.btnCancel.Visible = false;
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(451, 367);
            // 
            // panelMain
            // 
            this.panelMain.Location = new System.Drawing.Point(10, 47);
            this.panelMain.Size = new System.Drawing.Size(600, 300);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(367, 367);
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(532, 367);
            // 
            // bannerPage
            // 
            this.bannerPage.Size = new System.Drawing.Size(614, 40);
            // 
            // GarminFitnessSetupWizard
            // 
            this.AcceptButton = null;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.CancelButton = null;
            this.ClientSize = new System.Drawing.Size(619, 395);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "GarminFitnessSetupWizard";
            this.ResumeLayout(false);

        }

        protected override void FinishClicked()
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            base.FinishClicked();
        }

        public bool IsIndependentZonesSetupSelected
        {
            get { return m_IsIndependentZonesSetupSelected; }
            set { m_IsIndependentZonesSetupSelected = value; }
        }

        public bool ImportProfile
        {
            get { return m_ImportProfile; }
            set { m_ImportProfile = value; }
        }

        private bool m_IsIndependentZonesSetupSelected = false;
        private bool m_ImportProfile = true;
    }
}

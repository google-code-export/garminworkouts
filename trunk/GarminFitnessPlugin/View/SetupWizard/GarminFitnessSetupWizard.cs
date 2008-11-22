using System.Collections.Generic;
using System.Drawing;

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
            pages.Add(new SetupWizardEditProfile(this));

            Pages = pages;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Visible = false;
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(385, 382);
            // 
            // btnPrev
            // 
            this.btnPrev.Location = new System.Drawing.Point(301, 382);
            // 
            // btnFinish
            // 
            this.btnFinish.Location = new System.Drawing.Point(466, 382);
            // 
            // GarminFitnessSetupWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(553, 410);
            this.Name = "GarminFitnessSetupWizard";
            this.ResumeLayout(false);
        }
    }
}

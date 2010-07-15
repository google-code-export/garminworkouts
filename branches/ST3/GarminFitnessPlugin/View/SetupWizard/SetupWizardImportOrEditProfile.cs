using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    class SetupWizardImportOrEditProfile : IExtendedWizardPage
    {
        public SetupWizardImportOrEditProfile(ExtendedWizard parentWizard)
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
            if (!((GarminFitnessSetupWizard)Wizard).ImportProfile)
            {
                IExtendedWizardPage nextPage = Wizard.GetPageByType(typeof(SetupWizardEditProfile));

                Wizard.ShowPage(nextPage);

                e.Cancel = true;
            }
            else
            {
                GarminDeviceManager.Instance.TaskCompleted += new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);

                Wizard.Enabled = false;
                Wizard.Cursor = Cursors.WaitCursor;

                // Import using Communicator Plugin
                GarminDeviceManager.Instance.SetOperatingDevice();
                GarminDeviceManager.Instance.ImportProfile();

                e.Cancel = true;
            }
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
                m_Control = new SetupWizardImportOrEditProfileControl(wizard);

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
            get { return GarminFitnessView.GetLocalizedString("ImportOrEditProfileText"); }
        }

        public override void UICultureChanged(System.Globalization.CultureInfo culture)
        {
        }

        public override event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

#endregion

        void OnDeviceManagerTaskCompleted(GarminDeviceManager manager, GarminDeviceManager.BasicTask task, bool succeeded, String errorText)
        {
            if (!succeeded)
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_Initialize)
                {
                    MessageBox.Show(GarminFitnessView.GetLocalizedString("DeviceCommunicationErrorText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(errorText,
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (task.Type == GarminDeviceManager.BasicTask.TaskTypes.TaskType_ImportProfile)
                {
                    Wizard.GoNext();
                }
            }

            if (manager.AreAllTasksFinished)
            {
                Wizard.Enabled = true;
                Wizard.Cursor = Cursors.Default;

                manager.TaskCompleted -= new GarminDeviceManager.TaskCompletedEventHandler(OnDeviceManagerTaskCompleted);
            }
        }

        SetupWizardImportOrEditProfileControl m_Control = null;
    }
}

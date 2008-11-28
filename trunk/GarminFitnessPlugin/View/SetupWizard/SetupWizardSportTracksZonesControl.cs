using System;
using System.Diagnostics;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    partial class SetupWizardSportTracksZonesControl : ExtendedWizardPageControl
    {
        public SetupWizardSportTracksZonesControl(ExtendedWizard wizard)
            : base(wizard)
        {
            InitializeComponent();

            ExplanationLabel.Text = GarminFitnessView.GetLocalizedString("STZonesExplanationText");
            CadenceZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("CadenceZoneSelectionLabelText");
            PowerZoneSelectionLabel.Text = GarminFitnessView.GetLocalizedString("PowerZoneSelectionLabelText");

            int cadenceSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.CadenceZones, Options.Instance.CadenceZoneCategory);
            CadenceZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.CadenceZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.CadenceZones[i];

                CadenceZoneComboBox.Items.Add(currentZone.Name);
            }
            CadenceZoneComboBox.SelectedIndex = cadenceSelectedIndex;

            int powerSelectedIndex = Utils.FindIndexForZoneCategory(PluginMain.GetApplication().Logbook.PowerZones, Options.Instance.PowerZoneCategory);
            PowerZoneComboBox.Items.Clear();
            for (int i = 0; i < PluginMain.GetApplication().Logbook.PowerZones.Count; ++i)
            {
                IZoneCategory currentZone = PluginMain.GetApplication().Logbook.PowerZones[i];

                PowerZoneComboBox.Items.Add(currentZone.Name);
            }
            PowerZoneComboBox.SelectedIndex = powerSelectedIndex;
        }

        private void CadenceZoneComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Trace.Assert(PluginMain.GetApplication().Logbook.CadenceZones.Count > CadenceZoneComboBox.SelectedIndex);

            Options.Instance.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[CadenceZoneComboBox.SelectedIndex];
        }

        private void PowerZoneComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Trace.Assert(PluginMain.GetApplication().Logbook.PowerZones.Count > PowerZoneComboBox.SelectedIndex);

            Options.Instance.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[PowerZoneComboBox.SelectedIndex];
        }
    }
}

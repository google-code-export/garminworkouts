using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    partial class WorkoutLinkSelectionDialog : Form
    {
        public WorkoutLinkSelectionDialog(Workout parentWorkout, IStep destinationStep)
        {
            InitializeComponent();

            this.Text = GarminFitnessView.GetLocalizedString("SelectWorkoutText");
            OKButton.Text = CommonResources.Text.ActionOk;
            Cancel_Button.Text = CommonResources.Text.ActionCancel;

            WorkoutsListBox.Format += new ListControlConvertEventHandler(WorkoutsListBox_Format);
            foreach (Workout workout in GarminWorkoutManager.Instance.Workouts)
            {
                // Don't allow recursive workouts or the same workout link twice since
                //  we can't tell the difference between substeps
                if (workout != parentWorkout &&
                    !workout.ContainsWorkoutLink(parentWorkout) &&
                    parentWorkout.CanAcceptNewStep(workout.StepCount, destinationStep))
                {
                    WorkoutsListBox.Items.Add(workout);
                }
            }

            if (WorkoutsListBox.Items.Count > 0)
            {
                WorkoutsListBox.SelectedIndex = 0;
            }
            else
            {
                OKButton.Enabled = false;
            }
        }

        void WorkoutsListBox_Format(object sender, ListControlConvertEventArgs e)
        {
            Workout workout = e.ListItem as Workout;

            e.Value = workout.Name;
        }

        public Workout SelectedWorkout
        {
            get { return WorkoutsListBox.SelectedItem as Workout; }
        }
    }
}

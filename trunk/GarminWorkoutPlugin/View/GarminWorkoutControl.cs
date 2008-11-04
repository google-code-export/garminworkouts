using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;
using System.Collections;

namespace GarminFitnessPlugin.View
{
    partial class GarminWorkoutControl : UserControl, IGarminFitnessPluginControl
    {
        public GarminWorkoutControl()
        {
            InitializeComponent();

            WorkoutsList.RowDataRenderer = new WorkoutRowDataRenderer(WorkoutsList);
            WorkoutsList.LabelProvider = new WorkoutIconLabelProvider();

            StepsList.RowDataRenderer = new StepRowDataRenderer(StepsList);
            StepsList.LabelProvider = new StepIconLabelProvider();

            m_DurationPanels = new ZoneFiveSoftware.Common.Visuals.Panel[]
            {
                null,
                DistanceDurationPanel,
                TimeDurationPanel,
                HeartRateDurationPanel,
                HeartRateDurationPanel,
                CaloriesDurationPanel
            };

            // Register on controller events
            GarminWorkoutManager.Instance.WorkoutListChanged += new GarminWorkoutManager.WorkoutListChangedEventHandler(OnGarminWorkoutManagerWorkoutListChanged);
            GarminWorkoutManager.Instance.WorkoutChanged += new GarminWorkoutManager.WorkoutChangedEventHandler(OnWorkoutChanged);
            GarminWorkoutManager.Instance.WorkoutStepChanged += new GarminWorkoutManager.WorkoutStepChangedEventHandler(OnWorkoutStepChanged);
        }

        private void OnGarminWorkoutManagerWorkoutListChanged()
        {
            // This callback handles creation & deletion of workouts
            BuildWorkoutsList();
        }

        private void OnWorkoutChanged(Workout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            // When a property changes in a workout, this is triggered
            if (changedProperty.PropertyName == "Name" ||
                changedProperty.PropertyName == "Category")
            {
                BuildWorkoutsList();
            }

            if (changedProperty.PropertyName == "Schedule")
            {
                RefreshCalendarView();
            }

            if (SelectedWorkout == modifiedWorkout)
            {
                UpdateUIFromWorkouts();

                if (changedProperty.PropertyName == "Steps")
                {
                    UpdateUIFromSteps();
                }
            }
        }

        private void OnWorkoutStepChanged(Workout modifiedWorkout, IStep stepChanged, PropertyChangedEventArgs changedProperty)
        {
            StepsList.Invalidate();

            // When a property changes in a workout's step, this callback executes
            if (SelectedStep == stepChanged)
            {
                UpdateUIFromStep();
            }
        }

        #region UI Callbacks
        private void GarminWorkoutControl_Load(object sender, EventArgs e)
        {
            PluginMain.GetApplication().Calendar.SelectedChanged += new EventHandler(OnCalendarSelectedChanged);
        }

        void OnCalendarSelectedChanged(object sender, EventArgs e)
        {
            m_SelectedWorkouts.Clear();
            // Find the workouts planned on the selected date
            for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
            {
                Workout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];

                if (currentWorkout.ScheduledDates.Contains(PluginMain.GetApplication().Calendar.Selected))
                {
                    m_SelectedWorkouts.Add(currentWorkout);
                }
            }

            // Select them
            RefreshWorkoutSelection();
            WorkoutCalendar.SelectedDate = PluginMain.GetApplication().Calendar.Selected;
        }

        private void GarminWorkoutControl_SizeChanged(object sender, EventArgs e)
        {
            // Reset splitter distances
            CategoriesSplit.SplitterDistance = Options.CategoriesPanelSplitSize;
            WorkoutSplit.SplitterDistance = Options.WorkoutPanelSplitSize;
            StepsNotesSplitter.SplitterDistance = Options.StepNotesSplitSize;
            StepSplit.SplitterDistance = Math.Max(StepSplit.Panel1MinSize, StepSplit.Height - Options.StepPanelSplitSize);
            CalendarSplit.SplitterDistance = Math.Max(CalendarSplit.Panel1MinSize, CalendarSplit.Height - Options.CalendarPanelSplitSize);
        }

        private void CategoriesSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.CategoriesPanelSplitSize = e.SplitX;
        }

        private void WorkoutSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.WorkoutPanelSplitSize = e.SplitX;
        }

        private void StepSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.StepPanelSplitSize = StepSplit.Height - e.SplitY;
        }

        private void CalendarSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.CalendarPanelSplitSize = CalendarSplit.Height - e.SplitY;
        }

        private void StepsNotesSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.StepNotesSplitSize = e.SplitY;
        }

        private void DurationComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            IDuration.DurationType newType = (IDuration.DurationType)DurationComboBox.SelectedIndex;

            if (concreteStep.Duration.Type != newType)
            {
                DurationFactory.Create((IDuration.DurationType)DurationComboBox.SelectedIndex, concreteStep);
            }
        }

        private void TargetComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            ITarget.TargetType newType = (ITarget.TargetType)TargetComboBox.SelectedIndex;

            if (concreteStep.Target.Type != newType)
            {
                TargetFactory.Create((ITarget.TargetType)TargetComboBox.SelectedIndex, concreteStep);
            }
        }

        private void DistanceDurationPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (SelectedWorkout != null)
            {
                DistanceDurationUnitsLabel.Text = Length.LabelAbbr(SelectedWorkout.Category.DistanceUnits);
            }
        }

        private void NewWorkoutButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(SelectedCategory != null);

            SelectedWorkout = GarminWorkoutManager.Instance.CreateWorkout(SelectedCategory);
            RefreshWorkoutSelection();
        }

        private void RemoveWorkoutButton_Click(object sender, EventArgs e)
        {
            GarminWorkoutManager.Instance.RemoveWorkouts(m_SelectedWorkouts);
        }

        private void ScheduleWorkoutButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_SelectedWorkouts.Count; ++i)
            {
                if (!m_SelectedWorkouts[i].ScheduledDates.Contains(WorkoutCalendar.SelectedDate))
                {
                    m_SelectedWorkouts[i].ScheduleWorkout(WorkoutCalendar.SelectedDate);
                }
            }
        }

        private void RemoveScheduledDateButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < m_SelectedWorkouts.Count; ++i)
            {
                m_SelectedWorkouts[i].RemoveScheduledDate(WorkoutCalendar.SelectedDate);
            }
        }

        private void WorkoutCalendar_SelectionChanged(object sender, DateTime newSelection)
        {
            bool hasScheduledDate = false;
            bool areAllWorkoutsScheduledOnDate = true;

            // Check what controls we must enable or disable
            for (int i = 0; i < m_SelectedWorkouts.Count; ++i)
            {
                Workout currentWorkout = m_SelectedWorkouts[i];
                bool foundSelectedDatePlanned = false;

                for (int j = 0; j < currentWorkout.ScheduledDates.Count; ++j)
                {
                    if (newSelection == currentWorkout.ScheduledDates[j])
                    {
                        foundSelectedDatePlanned = true;
                        hasScheduledDate = true;
                    }
                }

                if (!foundSelectedDatePlanned)
                {
                    areAllWorkoutsScheduledOnDate = false;
                }
            }

            ScheduleWorkoutButton.Enabled = m_SelectedWorkouts.Count > 0 && newSelection >= DateTime.Today && !areAllWorkoutsScheduledOnDate;
            RemoveScheduledDateButton.Enabled = hasScheduledDate;
        }

        private void StepsList_SelectedChanged(object sender, EventArgs e)
        {
            m_CancelledStepSelection = false;

            if (StepsList.Selected.Count > 0)
            {
                if (StepsList.Selected.Count == 1)
                {
                    IStep selectedStep = (IStep)((StepWrapper)StepsList.Selected[0]).Element;

                    if (!SelectedSteps.Contains(selectedStep))
                    {
                        SelectedStep = selectedStep;
                    }
                    else
                    {
                        RefreshStepSelection();

                        m_CancelledStepSelection = true;
                        m_SelectedStepCancelled = selectedStep;
                    }
                }
                else
                {
                    List<IStep> newSelection = new List<IStep>();

                    // We have multiple items selected
                    for (int i = 0; i < StepsList.Selected.Count; ++i)
                    {
                        newSelection.Add((IStep)((StepWrapper)StepsList.Selected[i]).Element);
                    }

                    SelectedSteps = newSelection;
                }
            }
            else
            {
                SelectedStep = null;
            }

            RefreshActions();
            UpdateUIFromSteps();
        }

        private void WorkoutsList_SelectedChanged(object sender, EventArgs e)
        {
            m_CancelledWorkoutSelection = false;

            if (WorkoutsList.Selected.Count > 0)
            {
                if (WorkoutsList.Selected.Count == 1)
                {
                    if (WorkoutsList.Selected[0].GetType() == typeof(ActivityCategoryWrapper))
                    {
                        SelectedWorkout = null;
                        SelectedCategory = (IActivityCategory)((ActivityCategoryWrapper)WorkoutsList.Selected[0]).Element;
                    }
                    else if (WorkoutsList.Selected[0].GetType() == typeof(WorkoutWrapper))
                    {
                        Workout selectedWorkout = (Workout)((WorkoutWrapper)WorkoutsList.Selected[0]).Element;

                        if (!m_SelectedWorkouts.Contains(selectedWorkout))
                        {
                            SelectedWorkout = selectedWorkout;
                            SelectedCategory = SelectedWorkout.Category;
                        }
                        else
                        {
                            RefreshWorkoutSelection();

                            m_CancelledWorkoutSelection = true;
                            m_SelectedWorkoutCancelled = selectedWorkout;
                        }
                    }
                    else
                    {
                        Trace.Assert(false);
                    }
                }
                else
                {
                    // We have multiple items selected, keep only the workouts
                    m_SelectedWorkouts.Clear();
                    for (int i = 0; i < WorkoutsList.Selected.Count; ++i)
                    {
                        if (WorkoutsList.Selected[i].GetType() == typeof(WorkoutWrapper))
                        {
                            m_SelectedWorkouts.Add((Workout)((WorkoutWrapper)WorkoutsList.Selected[i]).Element);
                        }
                    }
                    SelectedCategory = null;
                }
            }
            else
            {
                SelectedWorkout = null;
                SelectedCategory = null;
            }

            RefreshActions();
            UpdateUIFromWorkouts();
        }

        private void StepNameText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            Trace.Assert(StepNameText.Text.Length <= 15);
            RegularStep concreteStep = (RegularStep)SelectedStep;

            concreteStep.Name = StepNameText.Text;
        }

        private void RestingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;

            concreteStep.IsRestingStep = RestingCheckBox.Checked;
        }

        private void CaloriesDurationText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            if (Utils.IsTextIntegerInRange(CaloriesDurationText.Text, 1, 65535))
            {
                e.Cancel = false;
            }
            else
            {
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), 1, 65535),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();
                CaloriesDurationText.Text = concreteDuration.CaloriesToSpend.ToString();
                e.Cancel = true;
            }
        }

        private void CaloriesDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            concreteDuration.CaloriesToSpend = UInt16.Parse(CaloriesDurationText.Text);
        }

        private void CaloriesDurationText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void HeartRateReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null);

            if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove)
            {
                HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;
                bool isPercentMax = HeartRateReferenceComboBox.SelectedIndex == 1;

                if (isPercentMax && concreteDuration.MaxHeartRate > 100)
                {
                    concreteDuration.MaxHeartRate = 100;
                    HeartRateDurationText.Text = "100";
                }
                else if (!isPercentMax && concreteDuration.MaxHeartRate < 30)
                {
                    concreteDuration.MaxHeartRate = 30;
                    HeartRateDurationText.Text = "30";
                }

                concreteDuration.IsPercentageMaxHeartRate = isPercentMax;
            }
            else if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow)
            {
                HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;
                bool isPercentMax = HeartRateReferenceComboBox.SelectedIndex == 1;

                if (isPercentMax && concreteDuration.MinHeartRate > 100)
                {
                    concreteDuration.MinHeartRate = 100;
                    HeartRateDurationText.Text = "100";
                }
                else if (!isPercentMax && concreteDuration.MinHeartRate < 30)
                {
                    concreteDuration.MinHeartRate = 30;
                    HeartRateDurationText.Text = "30";
                }

                concreteDuration.IsPercentageMaxHeartRate = isPercentMax;
            }
            else
            {
                Trace.Assert(false);
            }
        }

        private void HeartRateDurationText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove || concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow));
            bool isPercentMax;
            UInt16 HRValue;

            if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove)
            {
                HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;

                isPercentMax = concreteDuration.IsPercentageMaxHeartRate;
                HRValue = concreteDuration.MaxHeartRate;
            }
            else
            {
                HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;

                isPercentMax = concreteDuration.IsPercentageMaxHeartRate;
                HRValue = concreteDuration.MinHeartRate;
            }

            if (isPercentMax)
            {
                if (Utils.IsTextIntegerInRange(HeartRateDurationText.Text, 1, 100))
                {
                    e.Cancel = false;
                }
                else
                {
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), 1, 100),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();
                    HeartRateDurationText.Text = HRValue.ToString();
                    e.Cancel = true;
                }
            }
            else
            {
                if (Utils.IsTextIntegerInRange(HeartRateDurationText.Text, 30, 240))
                {
                    e.Cancel = false;
                }
                else
                {
                    MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), 30, 240),
                                    GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();
                    HeartRateDurationText.Text = HRValue.ToString();
                    e.Cancel = true;
                }
            }
        }

        private void HeartRateDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove || concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow));

            if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove)
            {
                HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;

                concreteDuration.MaxHeartRate = Byte.Parse(HeartRateDurationText.Text);
            }
            else if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow)
            {
                HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;

                concreteDuration.MinHeartRate = Byte.Parse(HeartRateDurationText.Text);
            }
            else
            {
                Trace.Assert(false);
            }
        }

        private void HeartRateDurationText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void DistanceDurationText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;
            float minDistance, maxDistance;

            if (SelectedWorkout.Category.DistanceUnits == Length.Units.Mile)
            {
                minDistance = 0.01f;
                maxDistance = 40.00f;
            }
            else if (SelectedWorkout.Category.DistanceUnits == Length.Units.Kilometer)
            {
                minDistance = 0.01f;
                maxDistance = 65.00f;
            }
            else
            {
                minDistance = 1.0f;
                maxDistance = 65000.0f;
            }

            if (Utils.IsTextFloatInRange(DistanceDurationText.Text, minDistance, maxDistance))
            {
                e.Cancel = false;
            }
            else
            {
                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("DoubleRangeValidationText"), minDistance, maxDistance),
                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();
                DistanceDurationText.Text = String.Format("{0:0.00}", concreteDuration.GetDistanceInUnits(SelectedWorkout.Category.DistanceUnits));
                e.Cancel = true;
            }
        }

        private void DistanceDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;

            concreteDuration.SetDistanceInUnits(float.Parse(DistanceDurationText.Text), SelectedWorkout.Category.DistanceUnits);
            DistanceDurationText.Text = String.Format("{0:0.00}", concreteDuration.GetDistanceInUnits(SelectedWorkout.Category.DistanceUnits));
        }

        private void DistanceDurationText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void TimeDurationUpDown_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Time);
            TimeDuration concreteDuration = (TimeDuration)concreteStep.Duration;

            concreteDuration.TimeInSeconds = TimeDurationUpDown.Duration;
        }

        private void TimeDurationUpDown_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void ZoneComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);
            int selectedIndex = ZoneComboBox.SelectedIndex;

            switch (concreteStep.Target.Type)
            {
                case ITarget.TargetType.HeartRate:
                    {
                        UpdateHeartRateTargetFromComboBox(concreteStep, selectedIndex);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        UpdateCadenceTargetFromComboBox(concreteStep, selectedIndex);
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        UpdateSpeedTargetFromComboBox(concreteStep, selectedIndex);
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        UpdatePowerTargetFromComboBox(concreteStep, selectedIndex);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            ZoneComboBox.SelectedIndex = selectedIndex;
            TargetDirtyPictureBox.Visible = concreteStep.Target.IsDirty;
        }

        private void LowRangeTargetText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null);

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                UInt16 intMin = 0;
                UInt16 intMax = 0;
                double doubleMin = 0;
                double doubleMax = 0;
                string oldValue = "";
                RangeValidationInputType inputType = RangeValidationInputType.Integer;

                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MinHeartRate.ToString();
                            inputType = RangeValidationInputType.Integer;

                            if (concreteTarget.IsPercentageMaxHeartRate)
                            {
                                intMin = 1;
                                intMax = 100;
                            }
                            else
                            {
                                intMin = 30;
                                intMax = 240;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MinCadence.ToString();
                            intMin = 0;
                            intMax = 254;
                            inputType = RangeValidationInputType.Integer;

                            break;
                        }
                    case ITarget.TargetType.Speed:
                        {
                            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                double paceTime = concreteTarget.GetMaxSpeedInMinutesPerUnit(SelectedWorkout.Category.DistanceUnits);
                                UInt16 minutes, seconds;

                                Utils.FloatToTime(paceTime, out minutes, out seconds);
                                oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                                doubleMin = 1.0 / (Length.Convert(60, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                                doubleMax = 1.0 / (Length.Convert(1.0002, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                                inputType = RangeValidationInputType.Time;
                            }
                            else
                            {
                                oldValue = String.Format("{0:0.00}", concreteTarget.GetMinSpeedInUnitsPerHour(SelectedWorkout.Category.DistanceUnits));
                                doubleMin = Length.Convert(1, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits);
                                doubleMax = Length.Convert(60, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits);
                                inputType = RangeValidationInputType.Float;
                            }

                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MinPower.ToString();
                            intMin = 20;
                            intMax = 999;
                            inputType = RangeValidationInputType.Integer;

                            break;
                        }
                    default:
                        {
                            Trace.Assert(false);
                            break;
                        }
                }

                switch (inputType)
                {
                    case RangeValidationInputType.Integer:
                        {
                            if (Utils.IsTextIntegerInRange(LowRangeTargetText.Text, intMin, intMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), intMin, intMax),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                LowRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                    case RangeValidationInputType.Float:
                        {
                            if (Utils.IsTextFloatInRange(LowRangeTargetText.Text, doubleMin, doubleMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                LowRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                    case RangeValidationInputType.Time:
                        {
                            if (Utils.IsTextTimeInRange(LowRangeTargetText.Text, doubleMin, doubleMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                UInt16 minMinutes, minSeconds;
                                UInt16 maxMinutes, maxSeconds;

                                Utils.FloatToTime(doubleMin, out minMinutes, out minSeconds);
                                Utils.FloatToTime(doubleMax, out maxMinutes, out maxSeconds);
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("TimeRangeValidationText"),
                                                              minMinutes, minSeconds,
                                                              maxMinutes, maxSeconds),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                LowRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                }
            }
        }

        private void LowRangeTargetText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null);
            bool forceSelectHighTargetText = false;

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(LowRangeTargetText.Text);

                            if (newValue <= concreteTarget.MaxHeartRate)
                            {
                                concreteTarget.MinHeartRate = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentageMaxHeartRate);
                                forceSelectHighTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(LowRangeTargetText.Text);

                            if (newValue < concreteTarget.MaxCadence)
                            {
                                concreteTarget.MinCadence = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue);
                                forceSelectHighTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Speed:
                        {
                            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                double time = Utils.TimeToFloat(LowRangeTargetText.Text);

                                if (time <= concreteTarget.GetMinSpeedInMinutesPerUnit(SelectedWorkout.Category.DistanceUnits))
                                {
                                    concreteTarget.SetMaxSpeedInMinutesPerUnit(time, SelectedWorkout.Category.DistanceUnits);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInMinutesPerUnit(time, time, SelectedWorkout.Category.DistanceUnits);
                                    forceSelectHighTargetText = true;
                                }
                            }
                            else
                            {
                                double newValue = double.Parse(LowRangeTargetText.Text);

                                if (newValue <= concreteTarget.GetMaxSpeedInUnitsPerHour(SelectedWorkout.Category.DistanceUnits))
                                {
                                    concreteTarget.SetMinSpeedInUnitsPerHour(newValue, SelectedWorkout.Category.DistanceUnits);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInUnitsPerHour(newValue, newValue, SelectedWorkout.Category.DistanceUnits);
                                    forceSelectHighTargetText = true;
                                }
                            }
                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;
                            UInt16 newValue = UInt16.Parse(LowRangeTargetText.Text);

                            if (newValue < concreteTarget.MaxPower)
                            {
                                concreteTarget.MinPower = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue);
                                forceSelectHighTargetText = true;
                            }
                            break;
                        }
                    default:
                        {
                            Trace.Assert(false);
                            break;
                        }
                }

                if (forceSelectHighTargetText)
                {
                    HighRangeTargetText.Focus();
                    HighRangeTargetText.SelectAll();
                }
            }
        }

        private void LowRangeTargetText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void HighRangeTargetText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null);

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                UInt16 intMin = 0;
                UInt16 intMax = 0;
                double doubleMin = 0;
                double doubleMax = 0;
                string oldValue = "";
                RangeValidationInputType inputType = RangeValidationInputType.Integer;

                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MaxHeartRate.ToString();
                            inputType = RangeValidationInputType.Integer;

                            if (concreteTarget.IsPercentageMaxHeartRate)
                            {
                                intMin = 1;
                                intMax = 100;
                            }
                            else
                            {
                                intMin = 30;
                                intMax = 240;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MaxCadence.ToString();
                            intMin = 0;
                            intMax = 254;
                            inputType = RangeValidationInputType.Integer;

                            break;
                        }
                    case ITarget.TargetType.Speed:
                        {
                            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                double paceTime = concreteTarget.GetMinSpeedInMinutesPerUnit(SelectedWorkout.Category.DistanceUnits);
                                UInt16 minutes, seconds;

                                Utils.FloatToTime(paceTime, out minutes, out seconds);
                                oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                                doubleMin = 1.0 / (Length.Convert(60, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                                doubleMax = 1.0 / (Length.Convert(1.0002, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                                inputType = RangeValidationInputType.Time;
                            }
                            else
                            {
                                oldValue = String.Format("{0:0.00}", concreteTarget.GetMaxSpeedInUnitsPerHour(SelectedWorkout.Category.DistanceUnits));
                                doubleMin = Length.Convert(1, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits);
                                doubleMax = Length.Convert(60, Length.Units.Mile, SelectedWorkout.Category.DistanceUnits);
                                inputType = RangeValidationInputType.Float;
                            }

                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                            oldValue = concreteTarget.MaxPower.ToString();
                            intMin = 20;
                            intMax = 999;
                            inputType = RangeValidationInputType.Integer;

                            break;
                        }
                    default:
                        {
                            Trace.Assert(false);
                            break;
                        }
                }

                switch (inputType)
                {
                    case RangeValidationInputType.Integer:
                        {
                            if (Utils.IsTextIntegerInRange(HighRangeTargetText.Text, intMin, intMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("IntegerRangeValidationText"), intMin, intMax),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                HighRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                    case RangeValidationInputType.Float:
                        {
                            if (Utils.IsTextFloatInRange(HighRangeTargetText.Text, doubleMin, doubleMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                HighRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                    case RangeValidationInputType.Time:
                        {
                            if (Utils.IsTextTimeInRange(HighRangeTargetText.Text, doubleMin, doubleMax))
                            {
                                e.Cancel = false;
                            }
                            else
                            {
                                UInt16 minMinutes, minSeconds;
                                UInt16 maxMinutes, maxSeconds;

                                Utils.FloatToTime(doubleMin, out minMinutes, out minSeconds);
                                Utils.FloatToTime(doubleMax, out maxMinutes, out maxSeconds);
                                MessageBox.Show(String.Format(GarminFitnessView.ResourceManager.GetString("TimeRangeValidationText"),
                                                              minMinutes, minSeconds,
                                                              maxMinutes, maxSeconds),
                                                GarminFitnessView.ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                System.Media.SystemSounds.Asterisk.Play();
                                HighRangeTargetText.Text = oldValue;
                                e.Cancel = true;
                            }
                            break;
                        }
                }
            }
        }

        private void HighRangeTargetText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null);
            bool forceSelectLowTargetText = false;

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.MinHeartRate)
                            {
                                concreteTarget.MaxHeartRate = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentageMaxHeartRate);
                                forceSelectLowTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.MinCadence)
                            {
                                concreteTarget.MaxCadence = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue);
                                forceSelectLowTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Speed:
                        {
                            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                float time = Utils.TimeToFloat(HighRangeTargetText.Text);

                                if (time >= concreteTarget.GetMaxSpeedInMinutesPerUnit(SelectedWorkout.Category.DistanceUnits))
                                {
                                    concreteTarget.SetMinSpeedInMinutesPerUnit(time, SelectedWorkout.Category.DistanceUnits);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInMinutesPerUnit(time, time, SelectedWorkout.Category.DistanceUnits);
                                    forceSelectLowTargetText = true;
                                }
                            }
                            else
                            {
                                double newValue = double.Parse(HighRangeTargetText.Text);

                                if (newValue >= concreteTarget.GetMinSpeedInUnitsPerHour(SelectedWorkout.Category.DistanceUnits))
                                {
                                    concreteTarget.SetMaxSpeedInUnitsPerHour(newValue, SelectedWorkout.Category.DistanceUnits);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInUnitsPerHour(newValue, newValue, SelectedWorkout.Category.DistanceUnits);
                                    forceSelectLowTargetText = true;
                                }
                            }
                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;
                            UInt16 newValue = UInt16.Parse(HighRangeTargetText.Text);

                            if (newValue > concreteTarget.MinPower)
                            {
                                concreteTarget.MaxPower = newValue;
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue);
                                forceSelectLowTargetText = true;
                            }
                            break;
                        }
                    default:
                        {
                            Trace.Assert(false);
                            break;
                        }
                }

                if (forceSelectLowTargetText)
                {
                    LowRangeTargetText.Focus();
                    LowRangeTargetText.SelectAll();
                }
            }
        }

        private void HighRangeTargetText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void WorkoutsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && m_SelectedWorkouts.Count > 0)
            {
                GarminWorkoutManager.Instance.RemoveWorkouts(m_SelectedWorkouts);
            }
            else if (e.Control)
            {
                if (e.KeyCode == Keys.C || e.KeyCode == Keys.X)
                {
                    // Copy data to clipboard
                    MemoryStream workoutsData = new MemoryStream();

                    // Number of workouts to deserialize
                    workoutsData.WriteByte((Byte)m_SelectedWorkouts.Count);
                    for (int i = 0; i < m_SelectedWorkouts.Count; ++i)
                    {
                        m_SelectedWorkouts[i].Serialize(workoutsData);
                    }

                    Clipboard.SetData("GWP_WorkoutsList", workoutsData);

                    if (e.KeyCode == Keys.X)
                    {
                        // Cut, so remove from workout
                        GarminWorkoutManager.Instance.RemoveWorkouts(m_SelectedWorkouts);
                    }
                }
                else if (e.KeyCode == Keys.V)
                {
                    if (Clipboard.ContainsData("GWP_WorkoutsList"))
                    {
                        MemoryStream pasteResult = (MemoryStream)Clipboard.GetData("GWP_WorkoutsList");

                        if (SelectedCategory == null)
                        {
                            SelectedCategory = PluginMain.GetApplication().Logbook.ActivityCategories[0];
                        }

                        if (pasteResult != null)
                        {
                            // Set back to start
                            pasteResult.Seek(0, SeekOrigin.Begin);

                            SelectedWorkouts = GarminWorkoutManager.Instance.DeserializeWorkouts(pasteResult, SelectedCategory);
                        }
                    }
                }
            }
        }

        private void WorkoutsList_DragDrop(object sender, DragEventArgs e)
        {
            TreeList.RowHitState type;
            Point mouseLocation = new Point(e.X, e.Y);
            List<Workout> workoutsToMove = (List<Workout>)e.Data.GetData(typeof(List<Workout>));
            object item = WorkoutsList.RowHitTest(WorkoutsList.PointToClient(mouseLocation), out type);

            if (item != null && workoutsToMove != null)
            {
                IActivityCategory category = null;

                if (item.GetType() == typeof(WorkoutWrapper))
                {
                    WorkoutWrapper wrapper = (WorkoutWrapper)item;

                    category = ((Workout)wrapper.Element).Category;
                }
                else if (item.GetType() == typeof(ActivityCategoryWrapper))
                {
                    ActivityCategoryWrapper wrapper = (ActivityCategoryWrapper)item;

                    category = (IActivityCategory)wrapper.Element;
                }
                else
                {
                    // What is this doing here?
                    Trace.Assert(false);
                }

                if (e.Effect == DragDropEffects.Copy)
                {
                    // Make a copy of workouts
                    SelectedWorkouts = GarminWorkoutManager.Instance.CopyWorkouts(workoutsToMove, category);
                    SelectedSteps = null;
                }
                else
                {
                    GarminWorkoutManager.Instance.MoveWorkouts(workoutsToMove, category);
                }
            }

            m_IsMouseDownInWorkoutsList = false;
        }

        private void WorkoutsList_DragOver(object sender, DragEventArgs e)
        {
            List<Workout> workoutsToMove = (List<Workout>)e.Data.GetData(typeof(List<Workout>));

            if (workoutsToMove != null)
            {
                if ((e.KeyState & CTRL_KEY_CODE) != 0)
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.Move;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void WorkoutsList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeList.RowHitState type;
                object futureSelection = WorkoutsList.RowHitTest(e.Location, out type);

                if (futureSelection != null)
                {
                    if (futureSelection.GetType() == typeof(WorkoutWrapper))
                    {
                        m_IsMouseDownInWorkoutsList = true;
                        m_MouseMovedPixels = 0;
                        m_LastMouseDownLocation = e.Location;
                    }
                }
            }
        }

        private void WorkoutsList_MouseUp(object sender, MouseEventArgs e)
        {
            m_IsMouseDownInWorkoutsList = false;

            if (m_CancelledWorkoutSelection)
            {
                m_SelectedWorkouts.Clear();
                m_SelectedWorkouts.Add(m_SelectedWorkoutCancelled);

                RefreshWorkoutSelection();
            }
        }

        private void WorkoutsList_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsMouseDownInWorkoutsList && m_MouseMovedPixels < 5)
            {
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.X - e.X);
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.Y - e.Y);

                if (m_MouseMovedPixels >= 5)
                {
                    // Make a copy, because it might change because of selection
                    //  callbacks
                    List<Workout> currentSelection = new List<Workout>();

                    currentSelection.AddRange(m_SelectedWorkouts);

                    // Start drag & drop operation
                    DoDragDrop(currentSelection, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        private void StepsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && SelectedSteps.Count > 0)
            {
                DeleteSelectedSteps();
            }
            else if (e.Control)
            {
                if (e.KeyCode == Keys.C || e.KeyCode == Keys.X)
                {
                    // Copy data to clipboard
                    MemoryStream stepsData = new MemoryStream();
                    List<IStep> baseSteps;

                    baseSteps = GetMinimalStepsBase(SelectedSteps);
                    // Number of steps to deserialize
                    stepsData.WriteByte((Byte)baseSteps.Count);
                    for (int i = 0; i < baseSteps.Count; ++i)
                    {
                        baseSteps[i].Serialize(stepsData);
                    }

                    Clipboard.SetData("GWP_StepsList", stepsData);

                    if (e.KeyCode == Keys.X)
                    {
                        // Cut, so remove from workout
                        DeleteSelectedSteps();
                    }
                }
                else if (e.KeyCode == Keys.V)
                {
                    if (Clipboard.ContainsData("GWP_StepsList"))
                    {
                        MemoryStream pasteResult = (MemoryStream)Clipboard.GetData("GWP_StepsList");

                        if (pasteResult != null && SelectedWorkout != null)
                        {
                            // Set back to start
                            pasteResult.Seek(0, SeekOrigin.Begin);

                            List<IStep> stepsPasted = new List<IStep>();
                            byte[] intBuffer = new byte[sizeof(Int32)];
                            Byte stepCount = (Byte)pasteResult.ReadByte();

                            for (int i = 0; i < stepCount; i++)
                            {
                                IStep.StepType type;

                                pasteResult.Read(intBuffer, 0, sizeof(Int32));
                                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                                if (type == IStep.StepType.Regular)
                                {
                                    stepsPasted.Add(new RegularStep(pasteResult, Constants.CurrentVersion, SelectedWorkout));
                                }
                                else
                                {
                                    stepsPasted.Add(new RepeatStep(pasteResult, Constants.CurrentVersion, SelectedWorkout));
                                }
                            }

                            // Now that we deserialized, paste in the current workout
                            SelectedWorkout.AddStepsToRoot(stepsPasted);

                            SelectedSteps = stepsPasted;
                        }
                    }
                }
            }
        }

        private void StepsList_DragDrop(object sender, DragEventArgs e)
        {
            int rowNumber;
            bool isInUpperHalf;
            object destination;
            Point clientPoint = StepsList.PointToClient(new Point(e.X, e.Y));
            IStep destinationStep;
            List<IStep> newSelection = new List<IStep>();
            List<IStep> stepsToMove = (List<IStep>)e.Data.GetData(typeof(List<IStep>));
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

            destination = renderer.RowHitTest(clientPoint, out rowNumber, out isInUpperHalf);
            if (destination == null)
            {
                // Insert as the last item in the workout
                destinationStep = SelectedWorkout.Steps[SelectedWorkout.Steps.Count - 1];
            }
            else
            {
                destinationStep = (IStep)((StepWrapper)destination).Element;
            }

            // Make a copy
            List<IStep> newSteps = new List<IStep>();

            for (int i = 0; i < stepsToMove.Count; ++i)
            {
                newSteps.Add(stepsToMove[i].Clone());
            }

            // Add new items
            if (isInUpperHalf)
            {
                SelectedWorkout.InsertStepsBeforeStep(destinationStep, newSteps);
            }
            else
            {
                SelectedWorkout.InsertStepsAfterStep(destinationStep, newSteps);
            }

            // Remove the old ones
            if (e.Effect == DragDropEffects.Move)
            {
                SelectedWorkout.RemoveSteps(stepsToMove);
            }

            SelectedSteps = newSteps;
            m_IsMouseDownInStepsList = false;
            renderer.IsInDrag = false;
        }

        private void StepsList_DragOver(object sender, DragEventArgs e)
        {
            bool isCtrlKeyDown = (e.KeyState & CTRL_KEY_CODE) != 0;
            List<IStep> stepsToMove = (List<IStep>)e.Data.GetData(typeof(List<IStep>));

            if (stepsToMove != null)
            {
                StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;
                Point mouseLocation = StepsList.PointToClient(new Point(e.X, e.Y));
                object item = renderer.RowHitTest(mouseLocation);
                IStep destinationStep;
                int stepsDragged = 0;

                if (item == null)
                {
                    // Insert as the last item in the workout
                    destinationStep = SelectedWorkout.Steps[SelectedWorkout.Steps.Count - 1];
                }
                else
                {
                    destinationStep = (IStep)((StepWrapper)item).Element;
                }

                // We need to count the number of items being moved
                for (int i = 0; i < stepsToMove.Count; ++i)
                {
                    stepsDragged += stepsToMove[i].GetStepCount();
                }

                if (!isCtrlKeyDown ||
                    (isCtrlKeyDown && SelectedWorkout.GetStepCount() + stepsDragged <= Constants.MaxStepsPerWorkout))
                {
                    // Make sure we are not moving a repeat inside itself
                    if (!isCtrlKeyDown)
                    {
                        for (int i = 0; i < stepsToMove.Count; ++i)
                        {
                            if (stepsToMove[i].Type == IStep.StepType.Repeat)
                            {
                                RepeatStep repeatStep = (RepeatStep)stepsToMove[i];

                                if (repeatStep.IsChildStep(destinationStep))
                                {
                                    renderer.IsInDrag = false;
                                    e.Effect = DragDropEffects.None;
                                    StepsList.Invalidate();

                                    return;
                                }
                            }
                        }
                    }

                    renderer.IsInDrag = true;
                    renderer.DragOverClientPosition = mouseLocation;

                    if (isCtrlKeyDown)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                    }

                    StepsList.Invalidate();
                }
            }
        }

        private void StepsList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeList.RowHitState type;
                object futureSelection = StepsList.RowHitTest(e.Location, out type);

                if (futureSelection != null)
                {
                    if (futureSelection.GetType() == typeof(StepWrapper))
                    {
                        m_IsMouseDownInStepsList = true;
                        m_MouseMovedPixels = 0;
                        m_LastMouseDownLocation = e.Location;
                    }
                }
            }
        }

        private void StepsList_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if (e.EscapePressed)
            {
                StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

                renderer.IsInDrag = false;
            }
        }

        private void StepsList_DragEnter(object sender, DragEventArgs e)
        {
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

            renderer.IsInDrag = true;
            StepsList.Invalidate();
        }

        private void StepsList_DragLeave(object sender, EventArgs e)
        {
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

            renderer.IsInDrag = false;
            StepsList.Invalidate();
        }

        private void StepsList_MouseUp(object sender, MouseEventArgs e)
        {
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

            m_IsMouseDownInStepsList = false;
            renderer.IsInDrag = false;

            if (m_CancelledStepSelection)
            {
                SelectedStep = m_SelectedStepCancelled;
            }
        }

        private void StepsList_MouseMove(object sender, MouseEventArgs e)
        {
            if (StepsList.SelectedItems.Count == 0)
            {
                m_IsMouseDownInStepsList = false;
                return;
            }

            if (m_IsMouseDownInStepsList && m_MouseMovedPixels < 5)
            {
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.X - e.X);
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.Y - e.Y);

                if (m_MouseMovedPixels >= 5)
                {
                    // Start drag & drop operation
                    StepsList.DoDragDrop(GetMinimalStepsBase(SelectedSteps), DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        private void RepetitionCountText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)SelectedStep;

            if (Utils.IsTextIntegerInRange(RepetitionCountText.Text, 2, 99))
            {
                e.Cancel = false;
            }
            else
            {
                MessageBox.Show("Value must be an integer number between 2 and 99", "Invalid value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();
                RepetitionCountText.Text = concreteStep.RepetitionCount.ToString();
                e.Cancel = true;
            }
        }

        private void RepetitionCountText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)SelectedStep;

            concreteStep.RepetitionCount = Byte.Parse(RepetitionCountText.Text);
        }

        private void RepetitionCountText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void NotesText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedWorkout != null);

            SelectedWorkout.Notes = WorkoutNotesText.Text;
        }


        private void StepNotesText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null);

            SelectedStep.Notes = StepNotesText.Text;
        }

        private void WorkoutNameText_Validating(object sender, CancelEventArgs e)
        {
            Workout workoutWithSameName = GarminWorkoutManager.Instance.GetWorkoutWithName(WorkoutNameText.Text);

            if (WorkoutNameText.Text == String.Empty || (workoutWithSameName != null && workoutWithSameName != SelectedWorkout))
            {
                e.Cancel = true;

                MessageBox.Show(GarminFitnessView.ResourceManager.GetString("InvalidWorkoutNameText", GarminFitnessView.UICulture),
                                GarminFitnessView.ResourceManager.GetString("ErrorText", GarminFitnessView.UICulture),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WorkoutNameText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(SelectedWorkout != null);

            SelectedWorkout.Name = WorkoutNameText.Text;
            WorkoutsList.Invalidate();
        }

        private void WorkoutNameText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void HRRangeReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type == ITarget.TargetType.HeartRate);
            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
            Trace.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range);
            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
            bool isPercentMax = HRRangeReferenceComboBox.SelectedIndex == 1;
            Byte newMin = concreteTarget.MinHeartRate;
            Byte newMax = concreteTarget.MaxHeartRate;

            if (isPercentMax && newMin > 100)
            {
                newMin = 100;
            }
            else if (!isPercentMax && newMin < 30)
            {
                newMin = 30;
            }

            if (isPercentMax && newMax > 100)
            {
                newMax = 100;
            }
            else if (!isPercentMax && newMax < 30)
            {
                newMax = 30;
            }

            LowRangeTargetText.Text = newMin.ToString();
            HighRangeTargetText.Text = newMax.ToString();

            concreteTarget.SetValues(newMin, newMax, isPercentMax);
        }

        private void AddStepButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RegularStep(SelectedWorkout));
        }

        private void AddRepeatButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RepeatStep(SelectedWorkout));
        }

        private void RemoveItemButton_Click(object sender, EventArgs e)
        {
            DeleteSelectedSteps();
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(SelectedWorkout != null && SelectedStep != null);

            SelectedWorkout.MoveStepUp(SelectedStep);
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(SelectedWorkout != null && SelectedStep != null);

            SelectedWorkout.MoveStepDown(SelectedStep);
        }

        #endregion

        public void ThemeChanged(ITheme visualTheme)
        {
            m_CurrentTheme = visualTheme;

            CategoriesBanner.ThemeChanged(visualTheme);
            WorkoutsList.ThemeChanged(visualTheme);
            DetailsBanner.ThemeChanged(visualTheme);
            StepDetailsBanner.ThemeChanged(visualTheme);
            ScheduleBanner.ThemeChanged(visualTheme);
            StepsList.ThemeChanged(visualTheme);
            WorkoutCalendar.ThemeChanged(visualTheme);

            StepSplit.Panel1.BackColor = visualTheme.Control;
            splitContainer10.Panel2.BackColor = visualTheme.Control;
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            UpdateUIStrings();
            BuildWorkoutsList();
            UpdateUIFromWorkouts();
        }

        public void RefreshUIFromLogbook()
        {
            SelectedCategory = null;
            SelectedWorkout = null;
            SelectedSteps = null;

            RefreshCalendarView();
        }

        private ActivityCategoryWrapper AddCategoryNode(IActivityCategory category)
        {
            ActivityCategoryWrapper wrapper;

            // If we already have a wrapper for this workout, use it
            if (m_CategoryWrapperMap.ContainsKey(category))
            {
                wrapper = m_CategoryWrapperMap[category];

                wrapper.Children.Clear();
            }
            else
            {
                // Create a new wrapper
                wrapper = new ActivityCategoryWrapper(null, category);

                m_CategoryWrapperMap[category] = wrapper;
            }

            AddCategoryNode(wrapper, null);

            return wrapper;
        }

        private void AddCategoryNode(ActivityCategoryWrapper categoryNode, ActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }
            categoryNode.Parent = parent;

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                IActivityCategory currentCategory = category.SubCategories[i];
                ActivityCategoryWrapper wrapper;

                // If we already have a wrapper for this category, use it
                if (m_CategoryWrapperMap.ContainsKey(currentCategory))
                {
                    wrapper = m_CategoryWrapperMap[currentCategory];

                    wrapper.Children.Clear();
                }
                else
                {
                    // Create a new wrapper
                    wrapper = new ActivityCategoryWrapper(categoryNode, currentCategory);

                    m_CategoryWrapperMap[currentCategory] = wrapper;
                }

                AddCategoryNode(wrapper, categoryNode);
            }
        }

        private void UpdateUIStrings()
        {
            CategoriesBanner.Text = GarminFitnessView.ResourceManager.GetString("CategoriesText", GarminFitnessView.UICulture);
            DetailsBanner.Text = GarminFitnessView.ResourceManager.GetString("DetailsText", GarminFitnessView.UICulture);
            StepDetailsBanner.Text = GarminFitnessView.ResourceManager.GetString("StepDetailsText", GarminFitnessView.UICulture);
            ScheduleBanner.Text = GarminFitnessView.ResourceManager.GetString("ScheduleBannerText", GarminFitnessView.UICulture);

            NameLabel.Text = GarminFitnessView.ResourceManager.GetString("NameLabelText", GarminFitnessView.UICulture);
            WorkoutNotesLabel.Text = GarminFitnessView.ResourceManager.GetString("NotesLabelText", GarminFitnessView.UICulture);
            StepNotesLabel.Text = GarminFitnessView.ResourceManager.GetString("NotesLabelText", GarminFitnessView.UICulture);
            StepNameLabel.Text = GarminFitnessView.ResourceManager.GetString("StepNameLabelText", GarminFitnessView.UICulture);
            RestingCheckBox.Text = GarminFitnessView.ResourceManager.GetString("RestingCheckBoxText", GarminFitnessView.UICulture);
            StepDurationGroup.Text = GarminFitnessView.ResourceManager.GetString("StepDurationGroupText", GarminFitnessView.UICulture);
            StepDurationLabel.Text = GarminFitnessView.ResourceManager.GetString("StepDurationLabelText", GarminFitnessView.UICulture);
            StepTargetGroup.Text = GarminFitnessView.ResourceManager.GetString("StepTargetGroupText", GarminFitnessView.UICulture);
            StepTargetLabel.Text = GarminFitnessView.ResourceManager.GetString("StepTargetLabelText", GarminFitnessView.UICulture);
            CaloriesDurationLabel.Text = GarminFitnessView.ResourceManager.GetString("CaloriesDurationLabelText", GarminFitnessView.UICulture);
            DistanceDurationLabel.Text = GarminFitnessView.ResourceManager.GetString("DistanceDurationLabelText", GarminFitnessView.UICulture);
            HeartRateDurationLabel.Text = GarminFitnessView.ResourceManager.GetString("HeartRateDurationLabelText", GarminFitnessView.UICulture);
            TimeDurationLabel.Text = GarminFitnessView.ResourceManager.GetString("TimeDurationLabelText", GarminFitnessView.UICulture);
            ZoneLabel.Text = GarminFitnessView.ResourceManager.GetString("WhichZoneText", GarminFitnessView.UICulture);
            LowRangeTargetLabel.Text = GarminFitnessView.ResourceManager.GetString("BetweenText", GarminFitnessView.UICulture);
            MiddleRangeTargetLabel.Text = GarminFitnessView.ResourceManager.GetString("AndText", GarminFitnessView.UICulture);
            RepetitionCountLabel.Text = GarminFitnessView.ResourceManager.GetString("RepetitionCountLabelText", GarminFitnessView.UICulture);
            ExportDateTextLabel.Text = GarminFitnessView.ResourceManager.GetString("LastExportDateText", GarminFitnessView.UICulture);

            // Update duration heart rate reference combo box text
            HeartRateReferenceComboBox.Items.Clear();
            HeartRateReferenceComboBox.Items.Add(CommonResources.Text.LabelBPM);
            HeartRateReferenceComboBox.Items.Add(CommonResources.Text.LabelPercentOfMax);

            // Update target heart rate reference combo box text
            HRRangeReferenceComboBox.Items.Clear();
            HRRangeReferenceComboBox.Items.Add(CommonResources.Text.LabelBPM);
            HRRangeReferenceComboBox.Items.Add(CommonResources.Text.LabelPercentOfMax);

            // Update duration combo box
            int currentSelection = DurationComboBox.SelectedIndex;
            DurationComboBox.Items.Clear();
            for (int i = 0; i < (int)IDuration.DurationType.DurationTypeCount; ++i)
            {
                IDuration.DurationType currentDuration = (IDuration.DurationType)i;
                FieldInfo durationFieldInfo = currentDuration.GetType().GetField(Enum.GetName(currentDuration.GetType(), currentDuration));
                ComboBoxStringProviderAttribute providerAttribute = (ComboBoxStringProviderAttribute)Attribute.GetCustomAttribute(durationFieldInfo, typeof(ComboBoxStringProviderAttribute));

                DurationComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture));

                if (currentSelection == i)
                {
                    DurationComboBox.Text = GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture);
                }
            }

            // Update target combo box
            currentSelection = TargetComboBox.SelectedIndex;
            TargetComboBox.Items.Clear();
            for (int i = 0; i < (int)ITarget.TargetType.TargetTypeCount; ++i)
            {
                ITarget.TargetType currentTarget = (ITarget.TargetType)i;
                FieldInfo targetFieldInfo = currentTarget.GetType().GetField(Enum.GetName(currentTarget.GetType(), currentTarget));
                ComboBoxStringProviderAttribute providerAttribute = (ComboBoxStringProviderAttribute)Attribute.GetCustomAttribute(targetFieldInfo, typeof(ComboBoxStringProviderAttribute));

                TargetComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture));

                if (currentSelection == i)
                {
                    TargetComboBox.Text = GarminFitnessView.ResourceManager.GetString(providerAttribute.StringName, GarminFitnessView.UICulture);
                }
            }
        }

        private void UpdateDurationPanelVisibility(IDuration duration)
        {
            if (m_CurrentDurationPanel != null)
            {
                m_CurrentDurationPanel.Visible = false;
            }

            m_CurrentDurationPanel = m_DurationPanels[(int)duration.Type];

            if (m_CurrentDurationPanel != null)
            {
                m_CurrentDurationPanel.Visible = true;
            }
        }

        private void UpdateTargetPanelVisibility(ITarget target)
        {
            ZoneTargetPanel.Visible = (target.Type != ITarget.TargetType.Null);
            TargetDirtyPictureBox.Visible = target.IsDirty;

            switch (target.Type)
            {
                case ITarget.TargetType.Null:
                    {
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        BaseSpeedTarget baseTarget = (BaseSpeedTarget)target;

                        RangeTargetUnitsLabel.Visible = true;
                        HRRangeReferenceComboBox.Visible = false;

                        BuildSpeedComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    break;
                                }
                            case IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    break;
                                }
                            case IConcreteSpeedTarget.SpeedTargetType.Range:
                                {
                                    LowRangeTargetLabel.Visible = true;
                                    LowRangeTargetText.Visible = true;
                                    MiddleRangeTargetLabel.Visible = true;
                                    HighRangeTargetText.Visible = true;
                                    RangeTargetUnitsLabel.Visible = true;
                                    break;
                                }
                        }
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)target;

                        RangeTargetUnitsLabel.Visible = true;
                        HRRangeReferenceComboBox.Visible = false;

                        BuildCadenceComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    break;
                                }
                            case IConcreteCadenceTarget.CadenceTargetType.Range:
                                {
                                    LowRangeTargetLabel.Visible = true;
                                    LowRangeTargetText.Visible = true;
                                    MiddleRangeTargetLabel.Visible = true;
                                    HighRangeTargetText.Visible = true;
                                    RangeTargetUnitsLabel.Visible = true;
                                    break;
                                }
                        }
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)target;

                        RangeTargetUnitsLabel.Visible = false;
                        HRRangeReferenceComboBox.Visible = true;

                        BuildHRComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    HRRangeReferenceComboBox.Visible = false;
                                    break;
                                }
                            case IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    HRRangeReferenceComboBox.Visible = false;
                                    break;
                                }
                            case IConcreteHeartRateTarget.HeartRateTargetType.Range:
                                {
                                    LowRangeTargetLabel.Visible = true;
                                    LowRangeTargetText.Visible = true;
                                    MiddleRangeTargetLabel.Visible = true;
                                    HighRangeTargetText.Visible = true;
                                    RangeTargetUnitsLabel.Visible = false;
                                    HRRangeReferenceComboBox.Visible = true;
                                    break;
                                }
                        }
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)target;

                        RangeTargetUnitsLabel.Visible = true;
                        HRRangeReferenceComboBox.Visible = false;

                        BuildPowerComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case IConcretePowerTarget.PowerTargetType.ZoneGTC:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    break;
                                }
                            case IConcretePowerTarget.PowerTargetType.ZoneST:
                                {
                                    LowRangeTargetLabel.Visible = false;
                                    LowRangeTargetText.Visible = false;
                                    MiddleRangeTargetLabel.Visible = false;
                                    HighRangeTargetText.Visible = false;
                                    RangeTargetUnitsLabel.Visible = false;
                                    break;
                                }
                            case IConcretePowerTarget.PowerTargetType.Range:
                                {
                                    LowRangeTargetLabel.Visible = true;
                                    LowRangeTargetText.Visible = true;
                                    MiddleRangeTargetLabel.Visible = true;
                                    HighRangeTargetText.Visible = true;
                                    RangeTargetUnitsLabel.Visible = true;
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        Trace.Assert(false);
                        break;
                    }
            }
        }

        private void UpdateUIFromWorkouts()
        {
            if (SelectedWorkouts.Count <= 1)
            {
                UpdateUIFromWorkout(SelectedSteps);
            }
            else
            {
                bool hasScheduledDate = false;
                bool areAllWorkoutsScheduledOnDate = true;

                if (SelectedCategory == null)
                {
                    NewWorkoutButton.Enabled = false;
                }
                else
                {
                    NewWorkoutButton.Enabled = true;
                }

                StepSplit.Panel1.Enabled = false;
                RemoveWorkoutButton.Enabled = true;
                StepSplit.Panel2.Enabled = false;
                AddStepButton.Enabled = false;
                AddRepeatButton.Enabled = false;
                RemoveItemButton.Enabled = false;
                WorkoutCalendar.Enabled = true;
                ExportDateTextLabel.Enabled = false;
                ExportDateLabel.Enabled = false;

                if (StepsList.RowData != null)
                {
                    ((List<TreeList.TreeListNode>)StepsList.RowData).Clear();
                    StepsList.RowData = StepsList.RowData;
                }

                // Highlight scheduled dates
                WorkoutCalendar.RemoveAllMarkedDatesStyle(ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                for (int i = 0; i < SelectedWorkouts.Count; ++i)
                {
                    bool foundSelectedDatePlanned = false;
                    Workout currentWorkout = SelectedWorkouts[i];

                    for (int j = 0; j < currentWorkout.ScheduledDates.Count; ++j)
                    {
                        if (WorkoutCalendar.SelectedDate == currentWorkout.ScheduledDates[j])
                        {
                            foundSelectedDatePlanned = true;
                            hasScheduledDate = true;
                        }

                        WorkoutCalendar.AddMarkedDateStyle(currentWorkout.ScheduledDates[j], ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                    }

                    if (!foundSelectedDatePlanned)
                    {
                        areAllWorkoutsScheduledOnDate = false;
                    }
                }

                ScheduleWorkoutButton.Enabled = WorkoutCalendar.SelectedDate >= DateTime.Today && !areAllWorkoutsScheduledOnDate;
                RemoveScheduledDateButton.Enabled = hasScheduledDate;
            }
        }

        private void UpdateUIFromWorkout()
        {
            UpdateUIFromWorkout(null);
        }

        private void UpdateUIFromWorkout(List<IStep> forcedSelection)
        {
            if (SelectedCategory == null)
            {
                NewWorkoutButton.Enabled = false;
            }
            else
            {
                NewWorkoutButton.Enabled = true;
            }

            if (SelectedWorkout == null)
            {
                StepSplit.Panel1.Enabled = false;
                RemoveWorkoutButton.Enabled = false;
                StepSplit.Panel2.Enabled = false;
                AddStepButton.Enabled = false;
                AddRepeatButton.Enabled = false;
                RemoveItemButton.Enabled = false;
                WorkoutCalendar.Enabled = false;
                ScheduleWorkoutButton.Enabled = false;
                RemoveScheduledDateButton.Enabled = false;
                ExportDateTextLabel.Enabled = false;
                ExportDateLabel.Enabled = false;

                if (StepsList.RowData != null)
                {
                    ((List<TreeList.TreeListNode>)StepsList.RowData).Clear();
                    StepsList.RowData = StepsList.RowData;
                }

                WorkoutCalendar.RemoveAllMarkedDatesStyle(ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
            }
            else
            {
                RemoveWorkoutButton.Enabled = true;
                ScheduleWorkoutButton.Enabled = true;
                StepSplit.Panel1.Enabled = true;
                AddStepButton.Enabled = SelectedWorkout.GetStepCount() < 20;
                AddRepeatButton.Enabled = SelectedWorkout.GetStepCount() < 19;
                RemoveItemButton.Enabled = true;
                WorkoutCalendar.Enabled = true;
                ExportDateTextLabel.Enabled = true;
                ExportDateLabel.Enabled = true;

                if (SelectedWorkout.LastExportDate.Ticks == 0)
                {
                    ExportDateLabel.Text = GarminFitnessView.ResourceManager.GetString("NeverExportedText", GarminFitnessView.UICulture);
                }
                else
                {
                    ExportDateLabel.Text = SelectedWorkout.LastExportDate.ToString(CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name).DateTimeFormat.ShortDatePattern) +
                                            " " + SelectedWorkout.LastExportDate.ToString(CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name).DateTimeFormat.ShortTimePattern);
                }

                ScheduleWorkoutButton.Enabled = WorkoutCalendar.SelectedDate >= DateTime.Today && !SelectedWorkout.ScheduledDates.Contains(WorkoutCalendar.SelectedDate);
                RemoveScheduledDateButton.Enabled = SelectedWorkout.ScheduledDates.Contains(WorkoutCalendar.SelectedDate);

                // Update control with workout data
                WorkoutNameText.Text = SelectedWorkout.Name;
                WorkoutNotesText.Text = SelectedWorkout.Notes;

                BuildStepsList();

                // Highlight scheduled dates
                WorkoutCalendar.RemoveAllMarkedDatesStyle(ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                for (int i = 0; i < SelectedWorkout.ScheduledDates.Count; ++i)
                {
                    WorkoutCalendar.AddMarkedDateStyle(SelectedWorkout.ScheduledDates[i], ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                }

                // Force selection
                if (forcedSelection != null)
                {
                    List<StepWrapper> newSelection = new List<StepWrapper>();

                    for (int i = 0; i < forcedSelection.Count; ++i)
                    {
                        StepWrapper wrapper = GetStepWrapper((List<TreeList.TreeListNode>)StepsList.RowData, forcedSelection[i]);

                        if (wrapper != null)
                        {
                            newSelection.Add(wrapper);
                        }
                    }

                    // Force update
                    StepsList.Selected = newSelection;
                }
            }
        }

        private void UpdateUIFromSteps()
        {
            if (SelectedWorkout != null && SelectedSteps.Count <= 1)
            {
                AddStepButton.Enabled = SelectedWorkout.GetStepCount() < Constants.MaxStepsPerWorkout;
                AddRepeatButton.Enabled = SelectedWorkout.GetStepCount() < Constants.MaxStepsPerWorkout - 1;
                StepsNotesSplitter.Enabled = true;

                UpdateUIFromStep();
            }
            else
            {
                AddStepButton.Enabled = false;
                AddRepeatButton.Enabled = false;
                MoveDownButton.Enabled = false;
                MoveUpButton.Enabled = false;
                StepsNotesSplitter.Enabled = false;
            }
        }

        private void UpdateUIFromStep()
        {
            if (SelectedStep == null)
            {
                StepSplit.Panel2.Enabled = false;
                MoveUpButton.Enabled = false;
                MoveDownButton.Enabled = false;
                RemoveItemButton.Enabled = false;
            }
            else
            {
                UInt16 selectedPosition = 0;
                List<IStep> selectedList = null;

                StepSplit.Panel2.Enabled = true;
                RemoveItemButton.Enabled = true;

                if (Utils.GetStepInfo(SelectedStep, SelectedWorkout.Steps, out selectedList, out selectedPosition))
                {
                    MoveUpButton.Enabled = selectedPosition != 0; // Not the first step
                    MoveDownButton.Enabled = selectedPosition < selectedList.Count - 1; // Not the last step

                    StepNotesText.Text = SelectedStep.Notes;

                    switch (SelectedStep.Type)
                    {
                        case IStep.StepType.Regular:
                            {
                                // Show correct panels/controls
                                RepetitionPropertiesGroup.Visible = false;
                                StepDurationGroup.Visible = true;
                                StepTargetGroup.Visible = true;
                                StepNameLabel.Visible = true;
                                StepNameText.Visible = true;
                                RestingCheckBox.Visible = true;

                                RegularStep concreteStep = (RegularStep)SelectedStep;

                                if (concreteStep.Name != null && concreteStep.Name != String.Empty)
                                {
                                    StepNameText.Text = concreteStep.Name;
                                }
                                else
                                {
                                    StepNameText.Text = "";
                                }
                                RestingCheckBox.Checked = concreteStep.IsRestingStep;
                                DurationComboBox.SelectedIndex = (int)concreteStep.Duration.Type;
                                TargetComboBox.SelectedIndex = (int)concreteStep.Target.Type;

                                // Update correct duration UI elements based on type
                                UpdateDurationPanelVisibility(concreteStep.Duration);
                                switch (concreteStep.Duration.Type)
                                {
                                    case IDuration.DurationType.LapButton:
                                        {
                                            break;
                                        }
                                    case IDuration.DurationType.Distance:
                                        {
                                            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;
                                            double distance = concreteDuration.GetDistanceInUnits(SelectedWorkout.Category.DistanceUnits);
                                            DistanceDurationText.Text = String.Format("{0:0.00}", distance);
                                            DistanceDurationUnitsLabel.Text = Length.LabelAbbr(SelectedWorkout.Category.DistanceUnits);
                                            break;
                                        }
                                    case IDuration.DurationType.Time:
                                        {
                                            TimeDuration concreteDuration = (TimeDuration)concreteStep.Duration;
                                            TimeDurationUpDown.Duration = concreteDuration.TimeInSeconds;
                                            break;
                                        }
                                    case IDuration.DurationType.HeartRateAbove:
                                        {
                                            HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;
                                            HeartRateDurationText.Text = concreteDuration.MaxHeartRate.ToString();
                                            if (concreteDuration.IsPercentageMaxHeartRate)
                                            {
                                                HeartRateReferenceComboBox.SelectedIndex = 1;
                                            }
                                            else
                                            {
                                                HeartRateReferenceComboBox.SelectedIndex = 0;
                                            }
                                            break;
                                        }
                                    case IDuration.DurationType.HeartRateBelow:
                                        {
                                            HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;
                                            HeartRateDurationText.Text = concreteDuration.MinHeartRate.ToString();
                                            if (concreteDuration.IsPercentageMaxHeartRate)
                                            {
                                                HeartRateReferenceComboBox.SelectedIndex = 1;
                                            }
                                            else
                                            {
                                                HeartRateReferenceComboBox.SelectedIndex = 0;
                                            }
                                            break;
                                        }
                                    case IDuration.DurationType.Calories:
                                        {
                                            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;
                                            CaloriesDurationText.Text = concreteDuration.CaloriesToSpend.ToString();
                                            break;
                                        }
                                    default:
                                        {
                                            Trace.Assert(false);
                                            break;
                                        }
                                }

                                UpdateTargetPanelVisibility(concreteStep.Target);
                                switch (concreteStep.Target.Type)
                                {
                                    case ITarget.TargetType.Null:
                                        {
                                            break;
                                        }
                                    case ITarget.TargetType.Speed:
                                        {
                                            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;

                                            BuildSpeedComboBox(baseTarget);
                                            switch (baseTarget.ConcreteTarget.Type)
                                            {
                                                case IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                                                    {
                                                        SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                                        break;
                                                    }
                                                case IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                                                    {
                                                        SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(baseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones,
                                                                                                            concreteTarget.Zone) + 1;
                                                        break;
                                                    }
                                                case IConcreteSpeedTarget.SpeedTargetType.Range:
                                                    {
                                                        Length.Units unit = SelectedWorkout.Category.DistanceUnits;
                                                        SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                                                        if (concreteTarget.ViewAsPace)
                                                        {
                                                            RangeTargetUnitsLabel.Text = GarminFitnessView.ResourceManager.GetString("MinuteAbbrText", GarminFitnessView.UICulture) + "/" + Length.LabelAbbr(unit);
                                                            double min = concreteTarget.GetMinSpeedInMinutesPerUnit(unit);
                                                            double max = concreteTarget.GetMaxSpeedInMinutesPerUnit(unit);
                                                            UInt16 minutes;
                                                            UInt16 seconds;

                                                            Utils.FloatToTime(min, out minutes, out seconds);
                                                            HighRangeTargetText.Text = String.Format("{0:00}:{1:00}", minutes, seconds);
                                                            Utils.FloatToTime(max, out minutes, out seconds);
                                                            LowRangeTargetText.Text = String.Format("{0:00}:{1:00}", minutes, seconds);
                                                        }
                                                        else
                                                        {
                                                            RangeTargetUnitsLabel.Text = Length.LabelAbbr(unit) + GarminFitnessView.ResourceManager.GetString("PerHourText", GarminFitnessView.UICulture);
                                                            LowRangeTargetText.Text = String.Format("{0:0.00}", concreteTarget.GetMinSpeedInUnitsPerHour(unit));
                                                            HighRangeTargetText.Text = String.Format("{0:0.00}", concreteTarget.GetMaxSpeedInUnitsPerHour(unit));
                                                        }

                                                        ZoneComboBox.SelectedIndex = 0;

                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case ITarget.TargetType.Cadence:
                                        {
                                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;

                                            BuildCadenceComboBox(baseTarget);
                                            switch (baseTarget.ConcreteTarget.Type)
                                            {
                                                case IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                                                    {
                                                        CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(Options.CadenceZoneCategory.Zones,
                                                                                                                           concreteTarget.Zone) + 1;
                                                        break;
                                                    }
                                                case IConcreteCadenceTarget.CadenceTargetType.Range:
                                                    {
                                                        CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                                                        RangeTargetUnitsLabel.Text = CommonResources.Text.LabelRPM;
                                                        ZoneComboBox.SelectedIndex = 0;
                                                        LowRangeTargetText.Text = concreteTarget.MinCadence.ToString();
                                                        HighRangeTargetText.Text = concreteTarget.MaxCadence.ToString();
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case ITarget.TargetType.HeartRate:
                                        {
                                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;

                                            BuildHRComboBox(baseTarget);
                                            switch (baseTarget.ConcreteTarget.Type)
                                            {
                                                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                                                    {
                                                        HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                                        break;
                                                    }
                                                case IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                                                    {
                                                        HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(baseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones,
                                                                                                                           concreteTarget.Zone) + 1;
                                                        break;
                                                    }
                                                case IConcreteHeartRateTarget.HeartRateTargetType.Range:
                                                    {
                                                        HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = 0;
                                                        LowRangeTargetText.Text = concreteTarget.MinHeartRate.ToString();
                                                        HighRangeTargetText.Text = concreteTarget.MaxHeartRate.ToString();
                                                        HRRangeReferenceComboBox.SelectedIndex = concreteTarget.IsPercentageMaxHeartRate ? 1 : 0;
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    case ITarget.TargetType.Power:
                                        {
                                            HRRangeReferenceComboBox.Visible = false;

                                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;

                                            BuildPowerComboBox(baseTarget);
                                            switch (baseTarget.ConcreteTarget.Type)
                                            {
                                                case IConcretePowerTarget.PowerTargetType.ZoneGTC:
                                                    {
                                                        PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                                        break;
                                                    }
                                                case IConcretePowerTarget.PowerTargetType.ZoneST:
                                                    {
                                                        PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;
                                                        ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(Options.PowerZoneCategory.Zones,
                                                                                                            concreteTarget.Zone) + 1;
                                                        break;
                                                    }
                                                case IConcretePowerTarget.PowerTargetType.Range:
                                                    {
                                                        PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                                                        RangeTargetUnitsLabel.Text = CommonResources.Text.LabelWatts;
                                                        ZoneComboBox.SelectedIndex = 0;
                                                        LowRangeTargetText.Text = concreteTarget.MinPower.ToString();
                                                        HighRangeTargetText.Text = concreteTarget.MaxPower.ToString();
                                                        break;
                                                    }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            Trace.Assert(false);
                                            break;
                                        }
                                }

                                break;
                            }
                        case IStep.StepType.Repeat:
                            {
                                // Show correct panels/controls
                                RepetitionPropertiesGroup.Visible = true;
                                StepDurationGroup.Visible = false;
                                StepTargetGroup.Visible = false;
                                StepNameLabel.Visible = false;
                                StepNameText.Visible = false;
                                RestingCheckBox.Visible = false;

                                RepeatStep concreteStep = (RepeatStep)SelectedStep;

                                RepetitionCountText.Text = concreteStep.RepetitionCount.ToString();

                                break;
                            }
                        default:
                            {
                                Trace.Assert(false);
                                break;
                            }
                    }
                }
            }
        }

        private void BuildCadenceComboBox(BaseCadenceTarget target)
        {
            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString("CustomText"));

            IList<INamedLowHighZone> zones = Options.CadenceZoneCategory.Zones;
            for (byte i = 0; i < zones.Count; ++i)
            {
                ZoneComboBox.Items.Add(zones[i].Name);
            }
        }

        private void BuildHRComboBox(BaseHeartRateTarget target)
        {
            IConcreteHeartRateTarget.HeartRateTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString("CustomText"));

            if (type == IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC ||
               (type == IConcreteHeartRateTarget.HeartRateTargetType.Range && !Options.UseSportTracksHeartRateZones))
            {
                for (byte i = 1; i <= 5; ++i)
                {
                    ZoneComboBox.Items.Add(i.ToString());
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = target.ParentStep.ParentWorkout.Category.HeartRateZone.Zones;
                for (byte i = 0; i < zones.Count; ++i)
                {
                    ZoneComboBox.Items.Add(zones[i].Name);
                }
            }
        }

        private void BuildPowerComboBox(BasePowerTarget target)
        {
            IConcretePowerTarget.PowerTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString("CustomText"));

            if (type == IConcretePowerTarget.PowerTargetType.ZoneGTC ||
               (type == IConcretePowerTarget.PowerTargetType.Range && !Options.UseSportTracksPowerZones))
            {
                for (byte i = 1; i <= 7; ++i)
                {
                    ZoneComboBox.Items.Add(i.ToString());
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = Options.PowerZoneCategory.Zones;
                for (byte i = 0; i < zones.Count; ++i)
                {
                    ZoneComboBox.Items.Add(zones[i].Name);
                }
            }
        }

        private void BuildSpeedComboBox(BaseSpeedTarget target)
        {
            IConcreteSpeedTarget.SpeedTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString("CustomText"));

            // Use GTC zones
            if (type == IConcreteSpeedTarget.SpeedTargetType.ZoneGTC ||
               (type == IConcreteSpeedTarget.SpeedTargetType.Range && !Options.UseSportTracksHeartRateZones))
            {
                for (byte i = 1; i <= 10; ++i)
                {
                    ZoneComboBox.Items.Add(GarminFitnessView.ResourceManager.GetString("GTCSpeedZone" + i.ToString() + "Text", GarminFitnessView.UICulture));
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = target.ParentStep.ParentWorkout.Category.SpeedZone.Zones;
                for (byte i = 0; i < zones.Count; ++i)
                {
                    ZoneComboBox.Items.Add(zones[i].Name);
                }
            }
        }

        private void RefreshCalendarView()
        {
            List<DateTime> highlights = new List<DateTime>();

            for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
            {
                Workout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];

                for (int j = 0; j < currentWorkout.ScheduledDates.Count; ++j)
                {
                    highlights.Add(currentWorkout.ScheduledDates[j]);
                }
            }

            PluginMain.GetApplication().Calendar.SetHighlightedDates(highlights);
            PluginMain.GetApplication().Calendar.Selected = PluginMain.GetApplication().Calendar.Selected;
        }

        private void RefreshWorkoutSelection()
        {
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
            {
                if (m_SelectedWorkouts.Contains(GarminWorkoutManager.Instance.Workouts[i]))
                {
                    selection.Add(GetWorkoutWrapper((List<TreeList.TreeListNode>)WorkoutsList.RowData, GarminWorkoutManager.Instance.Workouts[i]));
                }
            }

            WorkoutsList.Selected = selection;
        }

        private void AddNewStep(IStep newStep)
        {
            Trace.Assert(SelectedWorkout != null);

            if (SelectedStep != null)
            {
                // Insert after selected
                SelectedWorkout.InsertStepAfterStep(SelectedStep, newStep);
            }
            else
            {
                // Insert as 1st element
                SelectedWorkout.InsertStepInRoot(0, newStep);
            }

            SelectedStep = newStep;
        }

        private StepWrapper GetStepWrapper(List<TreeList.TreeListNode> list, IStep step)
        {
            for (UInt16 i = 0; i < list.Count; ++i)
            {
                StepWrapper currentStep = (StepWrapper)list[i];

                if (currentStep.Element == step)
                {
                    return currentStep;
                }
                else if (((IStep)currentStep.Element).Type == IStep.StepType.Repeat)
                {
                    StepWrapper childWrapper = GetStepWrapper((List<TreeList.TreeListNode>)currentStep.Children, step);

                    if (childWrapper != null)
                    {
                        return childWrapper;
                    }
                }
            }

            return null;
        }

        private WorkoutWrapper GetWorkoutWrapper(List<TreeList.TreeListNode> list, Workout workout)
        {
            for (UInt16 i = 0; i < list.Count; ++i)
            {
                if (list[i].GetType() == typeof(WorkoutWrapper))
                {
                    WorkoutWrapper currentWorkout = (WorkoutWrapper)list[i];

                    if (currentWorkout.Element == workout)
                    {
                        return currentWorkout;
                    }
                }
                else
                {
                    Trace.Assert(list[i].GetType() == typeof(ActivityCategoryWrapper));

                    ActivityCategoryWrapper currentCategory = (ActivityCategoryWrapper)list[i];
                    WorkoutWrapper childWrapper = GetWorkoutWrapper((List<TreeList.TreeListNode>)currentCategory.Children, workout);

                    if (childWrapper != null)
                    {
                        return childWrapper;
                    }

                }
            }

            return null;
        }

        public void BuildWorkoutsList()
        {
            IApplication app = PluginMain.GetApplication();
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();

            for (int i = 0; i < app.Logbook.ActivityCategories.Count; ++i)
            {
                ActivityCategoryWrapper newNode;

                newNode = AddCategoryNode(app.Logbook.ActivityCategories[i]);
                categories.Add(newNode);
            }

            for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
            {
                WorkoutWrapper newItem = AddWorkoutToList(categories, GarminWorkoutManager.Instance.Workouts[i]);
            }

            WorkoutsList.RowData = categories;
            WorkoutsList.Columns.Clear();
            WorkoutsList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.ResourceManager.GetString("CategoryText", GarminFitnessView.UICulture),
                                                         150, StringAlignment.Near));
            WorkoutsList.SetExpanded(WorkoutsList.RowData, true, true);
        }

        public void BuildStepsList()
        {
            if (SelectedWorkout != null)
            {
                List<TreeList.TreeListNode> stepsList = new List<TreeList.TreeListNode>();
                AddStepsToList(stepsList, SelectedWorkout.Steps, null);

                StepsList.RowData = stepsList;
                StepsList.Columns.Clear();
                StepsList.Columns.Add(new TreeList.Column("DisplayString", "Description", 350,
                                                          StringAlignment.Near));
            }
        }

        private WorkoutWrapper AddWorkoutToList(List<TreeList.TreeListNode> list, Workout workout)
        {
            // Go through category list
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].GetType() == typeof(ActivityCategoryWrapper))
                {
                    ActivityCategoryWrapper currentCategory = (ActivityCategoryWrapper)list[i];

                    if (currentCategory.Element == workout.Category)
                    {
                        WorkoutWrapper wrapper;

                        // If we already have a wrapper for this workout, use it
                        if (m_WorkoutWrapperMap.ContainsKey(workout))
                        {
                            wrapper = m_WorkoutWrapperMap[workout];
                        }
                        else
                        {
                            // Create a new wrapper
                            wrapper = new WorkoutWrapper(currentCategory, workout);

                            m_WorkoutWrapperMap[workout] = wrapper;
                        }

                        int index = 0;
                        while (index < currentCategory.Children.Count &&
                              (currentCategory.Children[index].GetType() != typeof(WorkoutWrapper) ||
                              ((Workout)((WorkoutWrapper)currentCategory.Children[index]).Element).Name.CompareTo(wrapper.Name) < 0))
                        {
                            index++;
                        }

                        currentCategory.Children.Insert(index, wrapper);
                        wrapper.Parent = currentCategory;

                        return wrapper;
                    }
                    else if (currentCategory.Children.Count > 0)
                    {
                        WorkoutWrapper result;

                        result = AddWorkoutToList((List<TreeList.TreeListNode>)currentCategory.Children, workout);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
        }

        private void AddStepsToList(List<TreeList.TreeListNode> list, List<IStep> steps, StepWrapper parent)
        {
            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];
                StepWrapper wrapper;

                // If we already have a wrapper for this workout, use it
                if (m_StepWrapperMap.ContainsKey(currentStep))
                {
                    wrapper = m_StepWrapperMap[currentStep];

                    wrapper.Children.Clear();
                    wrapper.Parent = null;
                }
                else
                {
                    // Create a new wrapper
                    wrapper = new StepWrapper(parent, currentStep);

                    m_StepWrapperMap[currentStep] = wrapper;
                }

                if (parent != null)
                {
                    parent.Children.Add(wrapper);
                }
                else
                {
                    list.Add(wrapper);
                }
                wrapper.Parent = parent;

                if (steps[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;

                    AddStepsToList(list, concreteStep.StepsToRepeat, wrapper);
                }
            }
        }

        private void UpdateHeartRateTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != IConcreteHeartRateTarget.HeartRateTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new HeartRateRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range &&
                    selectedIndex != 0)
            {
                if (Options.UseSportTracksHeartRateZones)
                {
                    baseTarget.ConcreteTarget = new HeartRateZoneSTTarget(baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new HeartRateZoneGTCTarget(baseTarget);
                }
            }

            if (baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.Range)
            {
                HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

                LowRangeTargetText.Text = concreteTarget.MinHeartRate.ToString();
                HighRangeTargetText.Text = concreteTarget.MaxHeartRate.ToString();
                HRRangeReferenceComboBox.SelectedIndex = concreteTarget.IsPercentageMaxHeartRate ? 1 : 0;
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.ZoneST)
            {
                HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = baseTarget.ParentStep.ParentWorkout.Category.HeartRateZone.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC)
            {
                HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)(selectedIndex);
            }
        }

        private void UpdateSpeedTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != IConcreteSpeedTarget.SpeedTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new SpeedRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range &&
                    selectedIndex != 0)
            {
                if (Options.UseSportTracksHeartRateZones)
                {
                    baseTarget.ConcreteTarget = new SpeedZoneSTTarget(baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new SpeedZoneGTCTarget(baseTarget);
                }
            }

            if (baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.Range)
            {
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.ZoneST)
            {
                SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = baseTarget.ParentStep.ParentWorkout.Category.SpeedZone.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteSpeedTarget.SpeedTargetType.ZoneGTC)
            {
                SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)(selectedIndex);
            }
        }

        private void UpdateCadenceTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != IConcreteCadenceTarget.CadenceTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new CadenceRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range &&
                     selectedIndex != 0)
            {
                // ST zone
                baseTarget.ConcreteTarget = new CadenceZoneSTTarget(baseTarget);
            }

            if (baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range)
            {
                CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                LowRangeTargetText.Text = concreteTarget.MinCadence.ToString();
                HighRangeTargetText.Text = concreteTarget.MaxCadence.ToString();
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.ZoneST)
            {
                CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = Options.CadenceZoneCategory.Zones[selectedIndex - 1];
            }
        }

        private void UpdatePowerTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != IConcretePowerTarget.PowerTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new PowerRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range &&
                     selectedIndex != 0)
            {
                // ST zone
                if (Options.UseSportTracksHeartRateZones)
                {
                    baseTarget.ConcreteTarget = new PowerZoneSTTarget(baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new PowerZoneGTCTarget(baseTarget);
                }
            }

            if (baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.Range)
            {
                PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                LowRangeTargetText.Text = concreteTarget.MinPower.ToString();
                HighRangeTargetText.Text = concreteTarget.MaxPower.ToString();
            }
            else if (baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.ZoneST)
            {
                PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = Options.PowerZoneCategory.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == IConcretePowerTarget.PowerTargetType.ZoneGTC)
            {
                PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)selectedIndex;
            }
        }

        private void RefreshStepSelection()
        {
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            for (int i = 0; i < SelectedSteps.Count; ++i)
            {
                IStep currentStep = SelectedSteps[i];

                selection.Add(GetStepWrapper((List<TreeList.TreeListNode>)StepsList.RowData, currentStep));
            }

            StepsList.Selected = selection;
        }

        private void RefreshActions()
        {
            if (PluginMain.GetApplication().ActiveView.GetType() == typeof(GarminFitnessView))
            {
                for (int i = 0; i < PluginMain.GetApplication().ActiveView.Actions.Count; ++i)
                {
                    PluginMain.GetApplication().ActiveView.Actions[i].Refresh();
                }
            }
        }

        private void DeleteSelectedSteps()
        {
            List<IStep> stepsTodelete = new List<IStep>();

            stepsTodelete.AddRange(SelectedSteps);
            SelectedSteps = null;

            DeleteSteps(stepsTodelete);
        }

        private void DeleteSteps(List<IStep> stepsTodelete)
        {
            for (int i = 0; i < stepsTodelete.Count; ++i)
            {
                SelectedWorkout.RemoveStep(stepsTodelete[i]);
            }
        }

        private void DeleteSelectedStep()
        {
            DeleteStep(SelectedStep);
        }

        private void DeleteStep(IStep step)
        {
            List<IStep> stepsTodelete = new List<IStep>();

            stepsTodelete.Add(step);

            DeleteSteps(stepsTodelete);
        }

        private List<IStep> GetMinimalStepsBase(List<IStep> steps)
        {
            List<IStep> result = new List<IStep>();
            List<RepeatStep> baseRepeatSteps = new List<RepeatStep>();
            List<RepeatStep> repeatSteps = new List<RepeatStep>();

            // 1st pass, add all the base repeat steps to the result list
            for (int i = 0; i < steps.Count; ++i)
            {
                if (steps[i].Type == IStep.StepType.Repeat)
                {
                    repeatSteps.Add((RepeatStep)steps[i]);
                }
            }

            for (int i = 0; i < repeatSteps.Count; ++i)
            {
                RepeatStep currentRepeat = repeatSteps[i];
                bool isChild = false;

                // We must check if this repeat is a base, or a child of another repeat
                for (int j = 0; j < repeatSteps.Count; j++)
                {
                    if (i != j && repeatSteps[j].IsChildStep(currentRepeat))
                    {
                        isChild = true;
                        break;
                    }
                }

                if (!isChild)
                {
                    baseRepeatSteps.Add(currentRepeat);
                }
            }

            // We now have all base repeat steps in our result, check all regular steps
            //  for inheritance against that base
            for (int i = 0; i < steps.Count; ++i)
            {
                if (steps[i].Type == IStep.StepType.Regular)
                {
                    RegularStep currentStep = (RegularStep)steps[i];
                    bool isChild = false;

                    for (int j = 0; j < baseRepeatSteps.Count; ++j)
                    {
                        // We must check if this repeat is a base, or a child of another repeat
                        if (baseRepeatSteps[j].IsChildStep(currentStep))
                        {
                            isChild = true;
                            break;
                        }
                    }

                    if (!isChild)
                    {
                        result.Add(currentStep);
                    }
                }
                else if (baseRepeatSteps.Contains((RepeatStep)steps[i]))
                {
                    // Add repeats in the right order
                    result.Add(steps[i]);
                }
            }

            return result;
        }

        private int SelectedStepComparison(IStep x, IStep y)
        {
            int xId, yId;

            xId = Utils.GetStepExportId(x);
            yId = Utils.GetStepExportId(y);

            if (xId < yId)
            {
                return -1;
            }
            else if (xId == yId)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private Workout SelectedWorkout
        {
            get
            {
                if (m_SelectedWorkouts.Count == 1)
                {
                    return m_SelectedWorkouts[0];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    if (SelectedWorkouts.Count != 1 || SelectedWorkouts[0] != value)
                    {
                        List<Workout> selection = new List<Workout>();

                        selection.Add(value);
                        SelectedWorkouts = selection;
                    }
                }
                else
                {
                    SelectedWorkouts = null;
                }

            }
        }

        public List<Workout> SelectedWorkouts
        {
            get { return m_SelectedWorkouts; }
            private set
            {
                if (m_SelectedWorkouts != value)
                {
                    if (value != null)
                    {
                        m_SelectedWorkouts = value;
                    }
                    else
                    {
                        m_SelectedWorkouts.Clear();
                    }
                    RefreshWorkoutSelection();
                }
            }
        }

        private IStep SelectedStep
        {
            get
            {
                if (m_SelectedSteps.Count == 1)
                {
                    return m_SelectedSteps[0];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    if (SelectedSteps.Count != 1 || SelectedSteps[0] != value)
                    {
                        List<IStep> selection = new List<IStep>();

                        selection.Add(value);
                        SelectedSteps = selection;
                    }
                }
                else
                {
                    SelectedSteps = null;
                }
            }
        }

        public List<IStep> SelectedSteps
        {
            get { return m_SelectedSteps; }
            private set
            {
                if (m_SelectedSteps != value)
                {
                    if (value != null)
                    {
                        m_SelectedSteps = value;
                    }
                    else
                    {
                        m_SelectedSteps.Clear();
                    }

                    // Sort selected steps to prevent re-ordering when drag & dropping
                    m_SelectedSteps.Sort(SelectedStepComparison);
                    RefreshStepSelection();
                }
            }
        }

        public IActivityCategory SelectedCategory
        {
            get
            {
                if (m_SelectedCategory == null)
                {
                    // We probably have multiple workouts selected, find the common category
                    IActivityCategory selection = null;

                    for (int i = 0; i < SelectedWorkouts.Count; ++i)
                    {
                        if (selection == null)
                        {
                            selection = SelectedWorkouts[i].Category;
                        }
                        else if (selection != SelectedWorkouts[i].Category)
                        {
                            return null;
                        }
                    }

                    return selection;
                }

                return m_SelectedCategory;
            }
            set { m_SelectedCategory = value; }
        }

        private enum RangeValidationInputType
        {
            Integer,
            Float,
            Time
        }

        private ITheme m_CurrentTheme;
        private ZoneFiveSoftware.Common.Visuals.Panel m_CurrentDurationPanel;
        private readonly ZoneFiveSoftware.Common.Visuals.Panel[] m_DurationPanels;
        private const int CTRL_KEY_CODE = 8;

        private List<Workout> m_SelectedWorkouts = new List<Workout>();
        private List<IStep> m_SelectedSteps = new List<IStep>();
        private IActivityCategory m_SelectedCategory;

        private Dictionary<Workout, WorkoutWrapper> m_WorkoutWrapperMap = new Dictionary<Workout, WorkoutWrapper>();
        private Dictionary<IActivityCategory, ActivityCategoryWrapper> m_CategoryWrapperMap = new Dictionary<IActivityCategory, ActivityCategoryWrapper>();
        private Dictionary<IStep, StepWrapper> m_StepWrapperMap = new Dictionary<IStep, StepWrapper>();

        private bool m_IsMouseDownInWorkoutsList = false;
        private bool m_IsMouseDownInStepsList = false;
        private Point m_LastMouseDownLocation;
        private int m_MouseMovedPixels = 0;

        private bool m_CancelledWorkoutSelection = false;
        private bool m_CancelledStepSelection = false;
        private Workout m_SelectedWorkoutCancelled;
        private IStep m_SelectedStepCancelled;
    }
}

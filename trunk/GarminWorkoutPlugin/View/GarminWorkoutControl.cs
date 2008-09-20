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
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Data.Fitness;
using ZoneFiveSoftware.Common.Data.Measurement;
using ZoneFiveSoftware.Common.Visuals;
using ZoneFiveSoftware.Common.Visuals.Fitness;
using GarminWorkoutPlugin.Data;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.View
{
    public partial class GarminWorkoutControl : UserControl
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
        }

#region UI Callbacks

        protected override void OnPaint(PaintEventArgs e)
        {
            if (PaintEnabled)
            {
                base.OnPaint(e);
            }
        }

        private void GarminWorkoutControl_Load(object sender, EventArgs e)
        {
            BuildWorkoutsList();
            UpdateUIFromWorkout(m_SelectedWorkout);

            // Reset splitter distances
            CategoriesSplit.SplitterDistance = Options.CategoriesPanelSplitSize;
            WorkoutSplit.SplitterDistance = Options.WorkoutPanelSplitSize;
            StepSplit.SplitterDistance = Options.StepPanelSplitSize;
        }

        private void CategoriesSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Options.CategoriesPanelSplitSize = CategoriesSplit.SplitterDistance;
        }

        private void WorkoutSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Options.WorkoutPanelSplitSize = WorkoutSplit.SplitterDistance;
        }

        private void StepSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            Options.StepPanelSplitSize = StepSplit.SplitterDistance;
        }

        private void DurationComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            IDuration.DurationType newType = (IDuration.DurationType)DurationComboBox.SelectedIndex;

            if (concreteStep.Duration.Type != newType)
            {
                concreteStep.Duration = DurationFactory.Create((IDuration.DurationType)DurationComboBox.SelectedIndex, concreteStep);

                UpdateUIFromStep(m_SelectedStep);
                StepsList.Invalidate();

                Utils.SaveWorkoutsToLogbook();
            }
        }

        private void TargetComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            ITarget.TargetType newType = (ITarget.TargetType)TargetComboBox.SelectedIndex;

            if (concreteStep.Target.Type != newType)
            {
                concreteStep.Target = TargetFactory.Create((ITarget.TargetType)TargetComboBox.SelectedIndex, concreteStep);

                UpdateUIFromStep(m_SelectedStep);
                StepsList.Invalidate();

                Utils.SaveWorkoutsToLogbook();
            }
        }

        private void DistanceDurationPanel_VisibleChanged(object sender, EventArgs e)
        {
            if (m_SelectedWorkout != null)
            {
                DistanceDurationUnitsLabel.Text = Length.LabelAbbr(m_SelectedWorkout.Category.DistanceUnits);
            }
        }

        private void NewWorkoutButton_Click(object sender, EventArgs e)
        {
            AddNewWorkout();
            Utils.SaveWorkoutsToLogbook();
        }

        private void RemoveWorkoutButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedWorkout != null);

            WorkoutManager.Instance.Workouts.Remove(m_SelectedWorkout);
            m_SelectedWorkout = null;

            BuildWorkoutsList();
            UpdateUIFromWorkout(m_SelectedWorkout);

            Utils.SaveWorkoutsToLogbook();
        }

        private void StepsList_SelectedChanged(object sender, EventArgs e)
        {
            if (StepsList.Selected.Count > 0)
            {
                m_SelectedStep = (IStep)((StepWrapper)StepsList.Selected[0]).Element;
            }
            else
            {
                m_SelectedStep = null;
            }

            UpdateUIFromStep(m_SelectedStep);
        }
        
        private void WorkoutsList_SelectedChanged(object sender, EventArgs e)
        {
            if (WorkoutsList.Selected.Count > 0)
            {
                if(WorkoutsList.Selected[0].GetType() == typeof(ActivityCategoryWrapper))
                {
                    m_SelectedWorkout = null;
                    m_SelectedCategory = (IActivityCategory)((ActivityCategoryWrapper)WorkoutsList.Selected[0]).Element;
                }
                else if (WorkoutsList.Selected[0].GetType() == typeof(WorkoutWrapper))
                {
                    m_SelectedWorkout = (Workout)((WorkoutWrapper)WorkoutsList.Selected[0]).Element;
                    m_SelectedCategory = m_SelectedWorkout.Category;
                }
                else
                {
                    Trace.Assert(false);
                }
            }
            else
            {
                m_SelectedWorkout = null;
                m_SelectedCategory = null;
            }

            UpdateUIFromWorkout(m_SelectedWorkout);
        }

        private void StepNameText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            Trace.Assert(StepNameText.Text.Length <= 15);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;

            concreteStep.Name = StepNameText.Text;
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
        }

        private void RestingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;

            concreteStep.IsRestingStep = RestingCheckBox.Checked;

            Utils.SaveWorkoutsToLogbook();
        }

        private void CaloriesDurationText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            if (Utils.IsTextIntegerInRange(CaloriesDurationText.Text, 1, 65535))
            {
                e.Cancel = false;
            }
            else
            {
                MessageBox.Show(String.Format(m_ResourceManager.GetString("IntegerRangeValidationText"), 1, 65535),
                                m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();
                CaloriesDurationText.Text = concreteDuration.CaloriesToSpend.ToString();
                e.Cancel = true;
            }
        }

        private void CaloriesDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            concreteDuration.CaloriesToSpend = UInt16.Parse(CaloriesDurationText.Text);
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
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

            StepsList.Invalidate();
        }

        private void HeartRateDurationText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
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
                    MessageBox.Show(String.Format(m_ResourceManager.GetString("IntegerRangeValidationText"), 1, 100),
                                    m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    MessageBox.Show(String.Format(m_ResourceManager.GetString("IntegerRangeValidationText"), 30, 240),
                                    m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    System.Media.SystemSounds.Asterisk.Play();
                    HeartRateDurationText.Text = HRValue.ToString();
                    e.Cancel = true;
                }
            }
        }

        private void HeartRateDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
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

            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;
            float minDistance, maxDistance;

            if (m_SelectedWorkout.Category.DistanceUnits == Length.Units.Mile)
            {
                minDistance = 0.01f;
                maxDistance = 40.00f;
            }
            else if (m_SelectedWorkout.Category.DistanceUnits == Length.Units.Kilometer)
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
                MessageBox.Show(String.Format(m_ResourceManager.GetString("DoubleRangeValidationText"), minDistance, maxDistance),
                                m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();
                DistanceDurationText.Text = String.Format("{0:0.00}", concreteDuration.GetDistanceInUnits(m_SelectedWorkout.Category.DistanceUnits));
                e.Cancel = true;
            }
        }

        private void DistanceDurationText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;

            concreteDuration.SetDistanceInUnits(float.Parse(DistanceDurationText.Text), m_SelectedWorkout.Category.DistanceUnits);
            DistanceDurationText.Text = String.Format("{0:0.00}", concreteDuration.GetDistanceInUnits(m_SelectedWorkout.Category.DistanceUnits));
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Time);
            TimeDuration concreteDuration = (TimeDuration)concreteStep.Duration;

            concreteDuration.TimeInSeconds = TimeDurationUpDown.Duration;
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
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
            StepsList.Invalidate();
            WorkoutsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
        }

        private void LowRangeTargetText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);

            UInt16 intMin = 0;
            UInt16 intMax = 0;
            double doubleMin = 0;
            double doubleMax = 0;
            string oldValue = "";
            RangeValidationInputType inputType = RangeValidationInputType.Integer;

            switch(concreteStep.Target.Type)
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

                        if (baseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            double paceTime = concreteTarget.GetMaxSpeedInMinutesPerUnit(m_SelectedWorkout.Category.DistanceUnits);
                            UInt16 minutes, seconds;

                            Utils.FloatToTime(paceTime, out minutes, out seconds);
                            oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                            doubleMin = 1.0 / (Length.Convert(60, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                            doubleMax = 1.0 / (Length.Convert(1.0002, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                            inputType = RangeValidationInputType.Time;
                        }
                        else
                        {
                            oldValue = String.Format("{0:0.00}", concreteTarget.GetMinSpeedInUnitsPerHour(m_SelectedWorkout.Category.DistanceUnits));
                            doubleMin = Length.Convert(1, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits);
                            doubleMax = Length.Convert(60, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits);
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

            switch(inputType)
            {
                case RangeValidationInputType.Integer:
                    {
                        if (Utils.IsTextIntegerInRange(LowRangeTargetText.Text, intMin, intMax))
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("IntegerRangeValidationText"), intMin, intMax),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("TimeRangeValidationText"),
                                                          minMinutes, minSeconds,
                                                          maxMinutes, maxSeconds),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            System.Media.SystemSounds.Asterisk.Play();
                            LowRangeTargetText.Text = oldValue;
                            e.Cancel = true;
                        }
                        break;
                    }
            }
        }

        private void LowRangeTargetText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);
            bool forceSelectHighTargetText = false;

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

                        if (baseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            double time = Utils.TimeToFloat(LowRangeTargetText.Text);

                            if (time <= concreteTarget.GetMinSpeedInMinutesPerUnit(m_SelectedWorkout.Category.DistanceUnits))
                            {
                                concreteTarget.SetMaxSpeedInMinutesPerUnit(time, m_SelectedWorkout.Category.DistanceUnits);
                            }
                            else
                            {
                                concreteTarget.SetRangeInMinutesPerUnit(time, time, m_SelectedWorkout.Category.DistanceUnits);
                                forceSelectHighTargetText = true;
                            }
                        }
                        else
                        {
                            double newValue = double.Parse(LowRangeTargetText.Text);

                            if (newValue <= concreteTarget.GetMaxSpeedInUnitsPerHour(m_SelectedWorkout.Category.DistanceUnits))
                            {
                                concreteTarget.SetMinSpeedInUnitsPerHour(newValue, m_SelectedWorkout.Category.DistanceUnits);
                            }
                            else
                            {
                                concreteTarget.SetRangeInUnitsPerHour(newValue, newValue, m_SelectedWorkout.Category.DistanceUnits);
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

            StepsList.Invalidate();
            UpdateUIFromStep(m_SelectedStep);
            Utils.SaveWorkoutsToLogbook();

            if (forceSelectHighTargetText)
            {
                HighRangeTargetText.Focus();
                HighRangeTargetText.SelectAll();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);

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

                        if (baseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            double paceTime = concreteTarget.GetMinSpeedInMinutesPerUnit(m_SelectedWorkout.Category.DistanceUnits);
                            UInt16 minutes, seconds;

                            Utils.FloatToTime(paceTime, out minutes, out seconds);
                            oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                            doubleMin = 1.0 / (Length.Convert(60, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                            doubleMax = 1.0 / (Length.Convert(1.0002, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits) / Constants.MinutesPerHour);
                            inputType = RangeValidationInputType.Time;
                        }
                        else
                        {
                            oldValue = String.Format("{0:0.00}", concreteTarget.GetMaxSpeedInUnitsPerHour(m_SelectedWorkout.Category.DistanceUnits));
                            doubleMin = Length.Convert(1, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits);
                            doubleMax = Length.Convert(60, Length.Units.Mile, m_SelectedWorkout.Category.DistanceUnits);
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

            switch(inputType)
            {
                case RangeValidationInputType.Integer:
                    {
                        if (Utils.IsTextIntegerInRange(HighRangeTargetText.Text, intMin, intMax))
                        {
                            e.Cancel = false;
                        }
                        else
                        {
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("IntegerRangeValidationText"), intMin, intMax),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                            MessageBox.Show(String.Format(m_ResourceManager.GetString("TimeRangeValidationText"),
                                                          minMinutes, minSeconds,
                                                          maxMinutes, maxSeconds),
                                            m_ResourceManager.GetString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            System.Media.SystemSounds.Asterisk.Play();
                            HighRangeTargetText.Text = oldValue;
                            e.Cancel = true;
                        }
                        break;
                    }
            }
        }

        private void HighRangeTargetText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
            Trace.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);
            bool forceSelectLowTargetText = false;

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

                        if (newValue > concreteTarget.MinCadence)
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

                        if (baseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                        {
                            float time = Utils.TimeToFloat(HighRangeTargetText.Text);

                            if (time >= concreteTarget.GetMaxSpeedInMinutesPerUnit(m_SelectedWorkout.Category.DistanceUnits))
                            {
                                concreteTarget.SetMinSpeedInMinutesPerUnit(time, m_SelectedWorkout.Category.DistanceUnits);
                            }
                            else
                            {
                                concreteTarget.SetRangeInMinutesPerUnit(time, time, m_SelectedWorkout.Category.DistanceUnits);
                                forceSelectLowTargetText = true;
                            }
                        }
                        else
                        {
                            double newValue = double.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.GetMinSpeedInUnitsPerHour(m_SelectedWorkout.Category.DistanceUnits))
                            {
                                concreteTarget.SetMaxSpeedInUnitsPerHour(newValue, m_SelectedWorkout.Category.DistanceUnits);
                            }
                            else
                            {
                                concreteTarget.SetRangeInUnitsPerHour(newValue, newValue, m_SelectedWorkout.Category.DistanceUnits);
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

            StepsList.Invalidate();
            UpdateUIFromStep(m_SelectedStep);
            Utils.SaveWorkoutsToLogbook();

            if (forceSelectLowTargetText)
            {
                LowRangeTargetText.Focus();
                LowRangeTargetText.SelectAll();
            }
        }

        private void HighRangeTargetText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }
        private void WorkoutsList_DragDrop(object sender, DragEventArgs e)
        {
            TreeList.RowHitState type;
            Point mouseLocation = new Point(e.X, e.Y);
            Workout workoutToMove = (Workout)e.Data.GetData(typeof(Workout));
            object item = WorkoutsList.RowHitTest(WorkoutsList.PointToClient(mouseLocation), out type);

            if (item != null && workoutToMove != null)
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
                    // Clone the current object
                    MemoryStream tempStream = new MemoryStream();

                    // Serialize
                    workoutToMove.Serialize(tempStream);

                    // Deserialize to copy in a new object
                    tempStream.Seek(0, SeekOrigin.Begin);
                    workoutToMove = WorkoutManager.Instance.CreateWorkout(category);
                    workoutToMove.Deserialize(tempStream, Constants.CurrentVersion);
                    tempStream.Close();

                    // We must update the name to avoid duplicates
                    string tempName = workoutToMove.Name;

                    if (!Utils.IsTextInteger(tempName))
                    {
                        // Remove all trailing numbers
                        while (tempName.LastIndexOfAny("0123456789".ToCharArray()) == tempName.Length - 1)
                        {
                            tempName = tempName.Substring(0, tempName.Length - 1);
                        }
                    }

                    workoutToMove.Name = WorkoutManager.Instance.GetUniqueName(tempName);
                }

                if (e.Effect == DragDropEffects.Copy || workoutToMove.Category != category)
                {
                    workoutToMove.Category = category;

                    Utils.SaveWorkoutsToLogbook();
                    m_SelectedWorkout = workoutToMove;
                    m_SelectedStep = null;

                    BuildWorkoutsList();
                    UpdateUIFromWorkout(m_SelectedWorkout);
                }
            }

            m_IsMouseDownInWorkoutsList = false;
        }

        private void WorkoutsList_DragOver(object sender, DragEventArgs e)
        {
            Workout workoutToMove = (Workout)e.Data.GetData(typeof(Workout));

            if (workoutToMove != null)
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
        }

        private void WorkoutsList_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsMouseDownInWorkoutsList && m_MouseMovedPixels < 5)
            {
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.X - e.X);
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.Y - e.Y);

                if (m_MouseMovedPixels >= 5)
                {
                    // Start drag & drop operation
                    DoDragDrop(((WorkoutWrapper)WorkoutsList.SelectedItems[0]).Element, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        private void StepsList_DragDrop(object sender, DragEventArgs e)
        {
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;
            IStep stepToMove = (RegularStep)e.Data.GetData(typeof(RegularStep));

            // we can have either a regular or a repeat step
            if (stepToMove == null)
            {
                stepToMove = (RepeatStep)e.Data.GetData(typeof(RepeatStep));
            }

            if (stepToMove != null)
            {
                int rowNumber;
                bool isInUpperHalf;
                object destination;
                IStep destinationStep;
                Point clientPoint = StepsList.PointToClient(new Point(e.X, e.Y));

                destination = renderer.RowHitTest(clientPoint, out rowNumber, out isInUpperHalf);

                if(destination == null)
                {
                    // Insert as the last item in the workout
                    destinationStep = m_SelectedWorkout.Steps[m_SelectedWorkout.Steps.Count - 1];
                    isInUpperHalf = false;
                }
                else
                {
                    destinationStep = (IStep)((StepWrapper)destination).Element;
                }

                if (e.Effect == DragDropEffects.Copy || stepToMove != destinationStep)
                {
                    IStep movedStep = stepToMove.Clone();
                    List<IStep> parentList;
                    UInt16 index;

                    Utils.GetStepInfo(destinationStep, m_SelectedWorkout.Steps, out parentList, out index);

                    if(!isInUpperHalf)
                    {
                        index++;
                    }

                    parentList.Insert(index, movedStep);

                    if (e.Effect == DragDropEffects.Move)
                    {
                        Utils.GetStepInfo(stepToMove, m_SelectedWorkout.Steps, out parentList, out index);

                        parentList.RemoveAt(index);
                        CleanUpWorkoutAfterDelete(m_SelectedWorkout);
                    }

                    Utils.SaveWorkoutsToLogbook();
                    UpdateUIFromWorkout(m_SelectedWorkout, movedStep);
                }
            }

            m_IsMouseDownInStepsList = false;
            renderer.IsInDrag = false;
        }

        private void StepsList_DragOver(object sender, DragEventArgs e)
        {
            bool isCtrlKeyDown = (e.KeyState & CTRL_KEY_CODE) != 0;
            IStep stepToMove = (RegularStep)e.Data.GetData(typeof(RegularStep));

            // we can have either a regular or a repeat step
            if (stepToMove == null)
            {
                stepToMove = (RepeatStep)e.Data.GetData(typeof(RepeatStep));
            }

            e.Effect = DragDropEffects.None;

            if (stepToMove != null)
            {
                StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;
                Point mouseLocation = StepsList.PointToClient(new Point(e.X, e.Y));
                object item = renderer.RowHitTest(mouseLocation);
                IStep destinationStep;

                if (item == null)
                {
                    // Insert as the last item in the workout
                    destinationStep = m_SelectedWorkout.Steps[m_SelectedWorkout.Steps.Count - 1];
                }
                else
                {
                    destinationStep = (IStep)((StepWrapper)item).Element;
                }

                if (!isCtrlKeyDown ||
                    (isCtrlKeyDown && m_SelectedWorkout.GetStepCount() + stepToMove.GetStepCount() <= 20))
                {
                    // In case of a repeat make sure we're not moving the step inside itself
                    if (!isCtrlKeyDown && stepToMove.Type == IStep.StepType.Repeat)
                    {
                        RepeatStep repeatStep = (RepeatStep)stepToMove;

                        if (repeatStep.IsChildStep(destinationStep))
                        {
                            return;
                        }
                    }

                    renderer.IsInDrag = true;
                    renderer.DragOverClientPosition = mouseLocation;
                    StepsList.Invalidate();

                    if (isCtrlKeyDown)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Move;
                    }
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

        private void StepsList_MouseUp(object sender, MouseEventArgs e)
        {
            StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;

            m_IsMouseDownInStepsList = false;
            renderer.IsInDrag = false;
        }

        private void StepsList_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsMouseDownInStepsList && m_MouseMovedPixels < 5)
            {
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.X - e.X);
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.Y - e.Y);

                if (m_MouseMovedPixels >= 5)
                {
                    // Start drag & drop operation
                    DoDragDrop(((StepWrapper)StepsList.SelectedItems[0]).Element, DragDropEffects.Move | DragDropEffects.Copy);
                }
            }
        }

        private void RepetitionCountText_Validating(object sender, CancelEventArgs e)
        {
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)m_SelectedStep;

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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)m_SelectedStep;

            concreteStep.RepetitionCount = Byte.Parse(RepetitionCountText.Text);
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedWorkout != null);

            m_SelectedWorkout.Notes = NotesText.Text;

            Utils.SaveWorkoutsToLogbook();
        }

        private void WorkoutNameText_Validated(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedWorkout != null);

            m_SelectedWorkout.Name = WorkoutNameText.Text;
            WorkoutsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
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
            Trace.Assert(m_SelectedStep != null && m_SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)m_SelectedStep;
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
            StepsList.Invalidate();

            Utils.SaveWorkoutsToLogbook();
        }

        private void AddStepButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RegularStep(m_SelectedWorkout));

            Utils.SaveWorkoutsToLogbook();
        }

        private void AddRepeatButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RepeatStep(m_SelectedWorkout));

            Utils.SaveWorkoutsToLogbook();
        }

        private void RemoveItemButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedWorkout != null && m_SelectedStep != null);
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            if (Utils.GetStepInfo(m_SelectedStep, m_SelectedWorkout.Steps, out selectedList, out selectedPosition))
            {
                selectedList.RemoveAt(selectedPosition);

                CleanUpWorkoutAfterDelete(m_SelectedWorkout);
                if (selectedPosition < selectedList.Count)
                {
                    UpdateUIFromWorkout(m_SelectedWorkout, selectedList[selectedPosition]);
                }
                else
                {
                    if (selectedList.Count > 0)
                    {
                        UpdateUIFromWorkout(m_SelectedWorkout, selectedList[selectedList.Count - 1]);
                    }
                    else
                    {
                        UpdateUIFromWorkout(m_SelectedWorkout);
                    }
                }

                Utils.SaveWorkoutsToLogbook();
            }
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedWorkout != null && m_SelectedStep != null);
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            if (Utils.GetStepInfo(m_SelectedStep, m_SelectedWorkout.Steps, out selectedList, out selectedPosition))
            {
                Trace.Assert(selectedPosition > 0);

                selectedList.Reverse(selectedPosition - 1, 2);
                UpdateUIFromWorkout(m_SelectedWorkout, selectedList[selectedPosition - 1]);

                Utils.SaveWorkoutsToLogbook();
            }
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            Trace.Assert(m_SelectedWorkout != null && m_SelectedStep != null);
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            if (Utils.GetStepInfo(m_SelectedStep, m_SelectedWorkout.Steps, out selectedList, out selectedPosition))
            {
                Trace.Assert(selectedPosition < selectedList.Count - 1);

                selectedList.Reverse(selectedPosition, 2);
                UpdateUIFromWorkout(m_SelectedWorkout, selectedList[selectedPosition + 1]);

                Utils.SaveWorkoutsToLogbook();
            }
        }

#endregion

        public void ThemeChanged(ITheme visualTheme)
        {
            CategoriesBanner.ThemeChanged(visualTheme);
            WorkoutsList.ThemeChanged(visualTheme);
            DetailsBanner.ThemeChanged(visualTheme);
            StepDetailsBanner.ThemeChanged(visualTheme);
            StepsList.ThemeChanged(visualTheme);
            StepSplit.Panel1.BackColor = visualTheme.Control;
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            m_CurrentCulture = culture;

            StepDescriptionStringFormatter.ResourceManager = m_ResourceManager;
            StepDescriptionStringFormatter.CurrentCulture = m_CurrentCulture;

            UpdateUIStrings();
            BuildWorkoutsList();
            UpdateUIFromWorkout(m_SelectedWorkout);
        }

        private void AddCategoryNode(ActivityCategoryWrapper categoryNode, ActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                IActivityCategory currentCategory = category.SubCategories[i];
                ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(categoryNode, currentCategory);

                AddCategoryNode(newNode, categoryNode);
            }
        }

        private void UpdateUIStrings()
        {
            CategoriesBanner.Text = m_ResourceManager.GetString("CategoriesText", m_CurrentCulture);
            DetailsBanner.Text = m_ResourceManager.GetString("DetailsText", m_CurrentCulture);
            StepDetailsBanner.Text = m_ResourceManager.GetString("StepDetailsText", m_CurrentCulture);
            NameLabel.Text = m_ResourceManager.GetString("NameLabelText", m_CurrentCulture);
            NotesLabel.Text = m_ResourceManager.GetString("NotesLabelText", m_CurrentCulture);
            StepNameLabel.Text = m_ResourceManager.GetString("StepNameLabelText", m_CurrentCulture);
            RestingCheckBox.Text = m_ResourceManager.GetString("RestingCheckBoxText", m_CurrentCulture);
            StepDurationGroup.Text = m_ResourceManager.GetString("StepDurationGroupText", m_CurrentCulture);
            StepDurationLabel.Text = m_ResourceManager.GetString("StepDurationLabelText", m_CurrentCulture);
            StepTargetGroup.Text = m_ResourceManager.GetString("StepTargetGroupText", m_CurrentCulture);
            StepTargetLabel.Text = m_ResourceManager.GetString("StepTargetLabelText", m_CurrentCulture);
            CaloriesDurationLabel.Text = m_ResourceManager.GetString("CaloriesDurationLabelText", m_CurrentCulture);
            DistanceDurationLabel.Text = m_ResourceManager.GetString("DistanceDurationLabelText", m_CurrentCulture);
            HeartRateDurationLabel.Text = m_ResourceManager.GetString("HeartRateDurationLabelText", m_CurrentCulture);
            TimeDurationLabel.Text = m_ResourceManager.GetString("TimeDurationLabelText", m_CurrentCulture);
            ZoneLabel.Text = m_ResourceManager.GetString("WhichZoneText", m_CurrentCulture);
            LowRangeTargetLabel.Text = m_ResourceManager.GetString("BetweenText", m_CurrentCulture);
            MiddleRangeTargetLabel.Text = m_ResourceManager.GetString("AndText", m_CurrentCulture);
            RepetitionCountLabel.Text = m_ResourceManager.GetString("RepetitionCountLabelText", m_CurrentCulture);

            // Update duration heart rate reference combo box text
            HeartRateReferenceComboBox.Items.Clear();
            HeartRateReferenceComboBox.Items.Add(m_ResourceManager.GetString("BPMText", m_CurrentCulture));
            HeartRateReferenceComboBox.Items.Add(m_ResourceManager.GetString("PercentMaxHeartRateText", m_CurrentCulture));

            // Update target heart rate reference combo box text
            HRRangeReferenceComboBox.Items.Clear();
            HRRangeReferenceComboBox.Items.Add(m_ResourceManager.GetString("BPMText", m_CurrentCulture));
            HRRangeReferenceComboBox.Items.Add(m_ResourceManager.GetString("PercentMaxHeartRateText", m_CurrentCulture));

            // Update duration combo box
            int currentSelection = DurationComboBox.SelectedIndex;
            DurationComboBox.Items.Clear();
            for (int i = 0; i < (int)IDuration.DurationType.DurationTypeCount; ++i)
            {
                IDuration.DurationType currentDuration = (IDuration.DurationType)i;
                FieldInfo durationFieldInfo = currentDuration.GetType().GetField(Enum.GetName(currentDuration.GetType(), currentDuration));
                ComboBoxStringProviderAttribute providerAttribute = (ComboBoxStringProviderAttribute)Attribute.GetCustomAttribute(durationFieldInfo, typeof(ComboBoxStringProviderAttribute));

                DurationComboBox.Items.Add(m_ResourceManager.GetString(providerAttribute.StringName, m_CurrentCulture));

                if (currentSelection == i)
                {
                    DurationComboBox.Text = m_ResourceManager.GetString(providerAttribute.StringName, m_CurrentCulture);
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

                TargetComboBox.Items.Add(m_ResourceManager.GetString(providerAttribute.StringName, m_CurrentCulture));

                if (currentSelection == i)
                {
                    TargetComboBox.Text = m_ResourceManager.GetString(providerAttribute.StringName, m_CurrentCulture);
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

        private void UpdateUIFromWorkout(Workout workout)
        {
            UpdateUIFromWorkout(workout, null);
        }

        private void UpdateUIFromWorkout(Workout workout, IStep forcedSelection)
        {
            PaintEnabled = false;

            if (workout == null)
            {
                StepSplit.Panel1.Enabled = false;
                RemoveWorkoutButton.Enabled = false;
                StepSplit.Panel2.Enabled = false;
                AddStepButton.Enabled = false;
                AddRepeatButton.Enabled = false;
                RemoveItemButton.Enabled = false;

                if (StepsList.RowData != null)
                {
                    ((List<TreeList.TreeListNode>)StepsList.RowData).Clear();
                    StepsList.RowData = StepsList.RowData;
                }
            }
            else
            {
                RemoveWorkoutButton.Enabled = true;
                StepSplit.Panel1.Enabled = true;
                AddStepButton.Enabled = workout.GetStepCount() < 20;
                AddRepeatButton.Enabled = workout.GetStepCount() < 19;
                RemoveItemButton.Enabled = true;

                // Update control with workout data
                WorkoutNameText.Text = workout.Name;
                NotesText.Text = workout.Notes;

                if(StepsList.RowData == null)
                {
                    StepsList.RowData = new List<TreeList.TreeListNode>();
                }
                else
                {
                    ((List<TreeList.TreeListNode>)StepsList.RowData).Clear();
                }

                AddStepsToList((List<TreeList.TreeListNode>)StepsList.RowData, workout.Steps, null);
                StepsList.Columns.Clear();
                StepsList.Columns.Add(new TreeList.Column("DisplayString", "Description", 350,
                                                          StringAlignment.Near));

                // I don't know why but Invalidate() doesn't refresh the display resulting in
                //  an empty list.  So we force it by reassigning RowData with it's own value
                StepsList.RowData = StepsList.RowData;

                // Force selection
                if (forcedSelection != null)
                {
                    List<StepWrapper> newSelection = new List<StepWrapper>();

                    newSelection.Add(GetStepWrapper((List<TreeList.TreeListNode>)StepsList.RowData, forcedSelection));

                    // Force update
                    StepsList.Selected = newSelection;
                }

                UpdateUIFromStep(m_SelectedStep);
            }

            PaintEnabled = true;
        }

        private void UpdateUIFromStep(IStep step)
        {
            PaintEnabled = false;

            if (step == null)
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
                Utils.GetStepInfo(m_SelectedStep, m_SelectedWorkout.Steps, out selectedList, out selectedPosition);
                MoveUpButton.Enabled = selectedPosition != 0; // Not the first step
                MoveDownButton.Enabled = selectedPosition < selectedList.Count - 1; // Not the last step

                switch (step.Type)
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

                            RegularStep concreteStep = (RegularStep)step;

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
                                        double distance = concreteDuration.GetDistanceInUnits(m_SelectedWorkout.Category.DistanceUnits);
                                        DistanceDurationText.Text = String.Format("{0:0.00}", distance);
                                        DistanceDurationUnitsLabel.Text = Length.LabelAbbr(m_SelectedWorkout.Category.DistanceUnits);
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
                            switch(concreteStep.Target.Type)
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
                                                    Length.Units unit = m_SelectedWorkout.Category.DistanceUnits;
                                                    SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                                                    if (baseTarget.ParentStep.ParentWorkout.Category.SpeedUnits == Speed.Units.Pace)
                                                    {
                                                        RangeTargetUnitsLabel.Text = m_ResourceManager.GetString("MinuteAbbrText", m_CurrentCulture) + "/" + Length.LabelAbbr(unit);
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
                                                        RangeTargetUnitsLabel.Text = Length.LabelAbbr(unit) + m_ResourceManager.GetString("PerHourText", m_CurrentCulture);
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

                                                    RangeTargetUnitsLabel.Text = m_ResourceManager.GetString("RPMText", m_CurrentCulture);
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

                                                    RangeTargetUnitsLabel.Text = m_ResourceManager.GetString("WattsText", m_CurrentCulture);
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

                            RepeatStep concreteStep = (RepeatStep)step;

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

            PaintEnabled = true;
        }

        private void BuildCadenceComboBox(BaseCadenceTarget target)
        {
            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(m_ResourceManager.GetString("CustomText"));

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
            ZoneComboBox.Items.Add(m_ResourceManager.GetString("CustomText"));

            if(type == IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC ||
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
            ZoneComboBox.Items.Add(m_ResourceManager.GetString("CustomText"));

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
            ZoneComboBox.Items.Add(m_ResourceManager.GetString("CustomText"));

            // Use GTC zones
            if (type == IConcreteSpeedTarget.SpeedTargetType.ZoneGTC ||
               (type == IConcreteSpeedTarget.SpeedTargetType.Range && !Options.UseSportTracksHeartRateZones))
            {
                for (byte i = 1; i <= 10; ++i)
                {
                    ZoneComboBox.Items.Add(m_ResourceManager.GetString("GTCSpeedZone" + i.ToString() + "Text", m_CurrentCulture));
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

        private void AddStepsToList(List<TreeList.TreeListNode> list, List<IStep> steps, StepWrapper parent)
        {
            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];
                StepWrapper newStep = new StepWrapper(parent, currentStep);

                if (parent != null)
                {
                    parent.Children.Add(newStep);
                }
                else
                {
                    list.Add(newStep);
                }

                if (steps[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;

                    AddStepsToList(list, concreteStep.StepsToRepeat, newStep);
                }
            }
        }

        private void AddNewStep(IStep newStep)
        {
            Trace.Assert(m_SelectedWorkout != null);
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;
            bool selectionFound;

            selectionFound = Utils.GetStepInfo(m_SelectedStep, m_SelectedWorkout.Steps, out selectedList, out selectedPosition);
            m_SelectedStep = newStep;

            if (selectionFound)
            {
                // Insert after selected
                selectedList.Insert(selectedPosition + 1, m_SelectedStep);
            }
            else
            {
                // Insert as 1st element
                m_SelectedWorkout.Steps.Insert(0, m_SelectedStep);
            }

            Trace.Assert(StepsList.Selected != null);
            StepsList.Selected.Clear();
            StepsList.Selected.Add(m_SelectedStep);

            UpdateUIFromWorkout(m_SelectedWorkout, m_SelectedStep);
        }

        private void AddNewWorkout()
        {
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();
            WorkoutWrapper wrapper;
            Trace.Assert(m_SelectedCategory != null);

            m_SelectedWorkout = WorkoutManager.Instance.CreateWorkout(m_SelectedCategory);
            wrapper = AddWorkoutToList((List<TreeList.TreeListNode>)WorkoutsList.RowData, m_SelectedWorkout);
            selection.Add(wrapper);
            WorkoutsList.Selected = selection;

            // Force list update
            WorkoutsList.RowData = WorkoutsList.RowData;

            UpdateUIFromWorkout(m_SelectedWorkout);
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

        private void CleanUpWorkoutAfterDelete(Workout workout)
        {
            if (workout.Steps.Count == 0)
            {
                // Cannot have an empty workout, recreate a base step
                workout.Steps.Add(new RegularStep(m_SelectedWorkout));
            }
            else
            {
                // Go through repeat steps and delete the ones which have 0 substeps
                for (int i = 0; i < workout.Steps.Count; ++i)
                {
                    IStep currentStep = workout.Steps[i];

                    if (currentStep.Type == IStep.StepType.Repeat)
                    {
                        RepeatStep concreteStep = (RepeatStep)currentStep;

                        if (concreteStep.StepsToRepeat.Count == 0)
                        {
                            workout.Steps.RemoveAt(i);
                            i--;
                        }
                        else
                        {
                            CleanUpWorkoutAfterDelete(concreteStep);
                        }
                    }
                }
            }
        }

        private void CleanUpWorkoutAfterDelete(RepeatStep step)
        {
            // Go through repeat steps and delete the ones which have 0 substeps
            for (int i = 0; i < step.StepsToRepeat.Count; ++i)
            {
                IStep currentStep = step.StepsToRepeat[i];

                if (currentStep.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;

                    if (concreteStep.StepsToRepeat.Count == 0)
                    {
                        step.StepsToRepeat.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        CleanUpWorkoutAfterDelete(concreteStep);
                    }
                }
            }
        }

        public void BuildWorkoutsList()
        {
            IApplication app = PluginMain.GetApplication();
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            for (int i = 0; i < app.Logbook.ActivityCategories.Count; ++i)
            {
                IActivityCategory currentCategory = app.Logbook.ActivityCategories[i];
                ActivityCategoryWrapper newNode = new ActivityCategoryWrapper(null, currentCategory);

                categories.Add(newNode);
                AddCategoryNode(newNode, null);

                if (m_SelectedWorkout == null && i == 0)
                {
                    selection.Add(newNode);
                }
            }

            for (int i = 0; i < WorkoutManager.Instance.Workouts.Count; ++i)
            {
                WorkoutWrapper newItem = AddWorkoutToList(categories, WorkoutManager.Instance.Workouts[i]);

                if (m_SelectedWorkout == WorkoutManager.Instance.Workouts[i])
                {
                    selection.Add(newItem);
                }
            }

            if (selection.Count == 0)
            {
                selection.Add(categories[0]);
            }

            WorkoutsList.RowData = categories;
            WorkoutsList.Columns.Clear();
            WorkoutsList.Columns.Add(new TreeList.Column("Name", m_ResourceManager.GetString("CategoryText", m_CurrentCulture),
                                                         150, StringAlignment.Near));
            WorkoutsList.Selected = selection;
            WorkoutsList.SetExpanded(WorkoutsList.RowData, true, true);
        }

        private WorkoutWrapper AddWorkoutToList(List<TreeList.TreeListNode> list, Workout workout)
        {
            // Go throough category list
            for(int i = 0; i < list.Count; ++i)
            {
                if(list[i].GetType() == typeof(ActivityCategoryWrapper))
                {
                    ActivityCategoryWrapper currentCategory = (ActivityCategoryWrapper)list[i];

                    if (currentCategory.Element == workout.Category)
                    {
                        WorkoutWrapper wrapper = new WorkoutWrapper(currentCategory, workout);
                        currentCategory.Children.Add(wrapper);

                        return wrapper;
                    }
                    else if(currentCategory.Children.Count > 0)
                    {
                        WorkoutWrapper result;

                        result = AddWorkoutToList((List<TreeList.TreeListNode>)currentCategory.Children, workout);

                        if(result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            return null;
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
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
            }
            else if (baseTarget.ConcreteTarget.Type == IConcreteCadenceTarget.CadenceTargetType.Range &&
                     selectedIndex != 0)
            {
                // ST zone
                baseTarget.ConcreteTarget = new CadenceZoneSTTarget(baseTarget);
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
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
                UpdateUIFromStep(m_SelectedStep);
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

        [DesignerSerializationVisibility(0)]
        [Browsable(false)]
        private bool PaintEnabled
        {
            get { return m_PaintDisableCount == 0; }
            set
            {
                StepsList.PaintEnabled = value;
                if (value == true)
                {
                    Trace.Assert(m_PaintDisableCount > 0);
                    m_PaintDisableCount--;

                    if (m_PaintDisableCount == 0)
                    {
                        Invalidate();
                    }
                }
                else
                {
                    m_PaintDisableCount++;
                }
            }
        }

        private enum RangeValidationInputType
        {
            Integer,
            Float,
            Time
        }

        private ResourceManager m_ResourceManager = new ResourceManager("GarminWorkoutPlugin.Resources.StringResources",
                                                                        Assembly.GetExecutingAssembly());
        private CultureInfo m_CurrentCulture;
        private ZoneFiveSoftware.Common.Visuals.Panel m_CurrentDurationPanel;
        private readonly ZoneFiveSoftware.Common.Visuals.Panel[] m_DurationPanels;
        private const int CTRL_KEY_CODE = 8;

        private Workout m_SelectedWorkout = null;
        private IStep m_SelectedStep;
        private IActivityCategory m_SelectedCategory;
        private int m_PaintDisableCount = 0;

        private bool m_IsMouseDownInWorkoutsList = false;
        private bool m_IsMouseDownInStepsList = false;
        private Point m_LastMouseDownLocation;
        private int m_MouseMovedPixels = 0;
    }
}

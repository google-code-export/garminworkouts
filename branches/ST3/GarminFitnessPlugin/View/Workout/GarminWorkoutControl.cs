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

            Options.Instance.OptionsChanged += new Options.OptionsChangedEventHandler(OnOptionsChanged);

            WorkoutsList.RowDataRenderer = new WorkoutRowDataRenderer(WorkoutsList);
            WorkoutsList.LabelProvider = new WorkoutIconLabelProvider();

            StepsList.RowDataRenderer = new StepRowDataRenderer(StepsList);
            StepsList.LabelProvider = new StepIconLabelProvider();

            m_DurationPanels = new System.Windows.Forms.Panel[]
            {
                null,
                DistanceDurationPanel,
                TimeDurationPanel,
                HeartRateDurationPanel,
                HeartRateDurationPanel,
                CaloriesDurationPanel,
                PowerDurationPanel,
                PowerDurationPanel
            };

            SettingsButton.CenterImage = CommonResources.Images.Settings16;
            CopyButton.CenterImage = CommonResources.Images.DocumentCopy16;
            PasteButton.CenterImage = CommonResources.Images.Paste16;
            DonateButton.CenterImage = CommonResources.Images.Favorite16;

            RegisterChildrenControlsForFocus(this);

            // Register on controller events
            GarminWorkoutManager.Instance.WorkoutListChanged += new GarminWorkoutManager.WorkoutListChangedEventHandler(OnGarminWorkoutManagerWorkoutListChanged);
            GarminWorkoutManager.Instance.WorkoutChanged += new GarminWorkoutManager.WorkoutChangedEventHandler(OnWorkoutChanged);
            GarminWorkoutManager.Instance.WorkoutStepChanged += new GarminWorkoutManager.WorkoutStepChangedEventHandler(OnWorkoutStepChanged);
            GarminWorkoutManager.Instance.WorkoutStepDurationChanged += new GarminWorkoutManager.WorkoutStepDurationChangedEventHandler(OnWorkoutStepDurationChanged);
            GarminWorkoutManager.Instance.WorkoutStepTargetChanged += new GarminWorkoutManager.WorkoutStepTargetChangedEventHandler(OnWorkoutStepTargetChanged);

            WorkoutsList.Columns.Clear();
            WorkoutsList.Columns.Add(new TreeList.Column("Name", GarminFitnessView.GetLocalizedString("CategoryText"),
                                                         150, StringAlignment.Near));

            StepsList.Columns.Clear();
            StepsList.Columns.Add(new TreeList.Column("DisplayString", "Description", StepsList.Width - 60,
                                                      StringAlignment.Near));
            StepsList.Columns.Add(new TreeList.Column("AutoSplitPart", "Workout Part", 40,
                                                      StringAlignment.Near));

            AddLinkStepButton.Enabled = GarminWorkoutManager.Instance.Workouts.Count > 1;

            IntensityComboBox.Format += new ListControlConvertEventHandler(IntensityComboBoxFormat);
            for(int i = 0; i < (int)RegularStep.StepIntensity.IntensityCount; ++i)
            {
                IntensityComboBox.Items.Add((RegularStep.StepIntensity)i);
            }
        }

        void IntensityComboBoxFormat(object sender, ListControlConvertEventArgs e)
        {
            switch((RegularStep.StepIntensity)e.ListItem)
            {
                case RegularStep.StepIntensity.Active:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("ActiveText");
                        break;
                    }
                case RegularStep.StepIntensity.Rest:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("RestText");
                        break;
                    }
                case RegularStep.StepIntensity.Warmup:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("WarmupText");
                        break;
                    }
                case RegularStep.StepIntensity.Cooldown:
                    {
                        e.Value = GarminFitnessView.GetLocalizedString("CooldownText");
                        break;
                    }
            }
        }

        void OnOptionsChanged(PropertyChangedEventArgs changedProperty)
        {
            if (changedProperty.PropertyName == "EnableAutoSplitWorkouts")
            {
                RefreshWorkoutSelectionControls();
            }
        }

        private void OnGarminWorkoutManagerWorkoutListChanged()
        {
            // This callback handles creation & deletion of workouts
            BuildWorkoutsList();
            RefreshWorkoutSelection();

            AddLinkStepButton.Enabled = GarminWorkoutManager.Instance.Workouts.Count > 1;
        }

        private void OnWorkoutChanged(IWorkout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            // When a property changes in a workout, this is triggered
            if (changedProperty.PropertyName == "Name" ||
                changedProperty.PropertyName == "Category" ||
                changedProperty.PropertyName == "PartsCount")
            {
                int modifiedWorkoutChildrenCount = -1;
                WorkoutWrapper modifiedWrapper = GetWorkoutWrapper(modifiedWorkout.ConcreteWorkout);

                if (changedProperty.PropertyName == "PartsCount")
                {
                    CleanUpWorkoutPartsSelection();
                    modifiedWorkoutChildrenCount = modifiedWrapper.Children.Count;
                }

                BuildWorkoutsList();

                if (changedProperty.PropertyName == "PartsCount" && modifiedWorkoutChildrenCount == 0)
                {
                    WorkoutsList.SetExpanded(modifiedWrapper, true);
                }
            }

            if (changedProperty.PropertyName == "Schedule")
            {
                RefreshWorkoutCalendar();
                RefreshCalendarView();
            }

            if (SelectedWorkout == modifiedWorkout)
            {
                if (changedProperty.PropertyName == "Steps")
                {
                    BuildStepsList();

                    // Update the new step buttons
                    RefreshWorkoutSelectionControls();
                }
                else
                {
                    RefreshUIFromWorkouts();
                }
            }
        }

        private void OnWorkoutStepChanged(IWorkout modifiedWorkout, IStep stepChanged, PropertyChangedEventArgs changedProperty)
        {
            if (SelectedWorkout != null && SelectedWorkout.ConcreteWorkout == modifiedWorkout.ConcreteWorkout)
            {
                // Refresh the steps list so it updates the name/description
                WorkoutsList.Invalidate();
                StepsList.Invalidate();

                // When a property changes in a workout's step, this callback executes
                if (SelectedStep == stepChanged)
                {
                    if ((changedProperty.PropertyName == "ForceSplitOnStep" && SelectedWorkout is WorkoutPart))
                    {
                        BuildStepsList();
                    }

                    UpdateUIFromStep();
                }

                if (changedProperty.PropertyName == "LinkSteps")
                {
                    BuildStepsList();
                }
            }
            else if(SelectedStep is WorkoutLinkStep)
            {
                WorkoutLinkStep concreteStep = SelectedStep as WorkoutLinkStep;

                if (modifiedWorkout.ConcreteWorkout == concreteStep.LinkedWorkout)
                {
                    UpdateUIFromStep();
                }
            }
        }

        void OnWorkoutStepDurationChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            if (SelectedWorkout == modifiedWorkout)
            {
                // Refresh the steps list so it updates the name/description
                StepsList.Invalidate();

                if (modifiedStep == SelectedStep)
                {
                    UpdateUIFromDuration();
                }
            }
        }

        void OnWorkoutStepTargetChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty)
        {
            if (SelectedWorkout == modifiedWorkout)
            {
                // Refresh the steps list so it updates the name/description
                StepsList.Invalidate();

                if (changedProperty.PropertyName == "ConcreteTarget")
                {
                    UpdateTargetPanelVisibility();
                }

                if (modifiedStep == SelectedStep)
                {
                    UpdateUIFromTarget();
                }
            }
        }

        void OnCalendarSelectedChanged(object sender, EventArgs e)
        {
            List<IWorkout> newSelection = new List<IWorkout>();
            GarminFitnessDate selectedDate = new GarminFitnessDate(PluginMain.GetApplication().Calendar.Selected);

            // Find the workouts planned on the selected date
            for (int i = 0; i < GarminWorkoutManager.Instance.Workouts.Count; ++i)
            {
                IWorkout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];

                if (currentWorkout.ScheduledDates.Contains(selectedDate))
                {
                    newSelection.Add(currentWorkout);
                }
            }

            SelectedWorkouts = newSelection;
            WorkoutCalendar.SelectedDate = PluginMain.GetApplication().Calendar.Selected;
        }

#region UI Callbacks
        private void GarminWorkoutControl_Load(object sender, EventArgs e)
        {
            PluginMain.GetApplication().Calendar.SelectedChanged += new EventHandler(OnCalendarSelectedChanged);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            WorkoutCalendar.StartOfWeek = PluginMain.GetApplication().SystemPreferences.StartOfWeek;
        }

        private void GarminWorkoutControl_SizeChanged(object sender, EventArgs e)
        {
            // Reset splitter distances
            CategoriesSplit.SplitterDistance = Options.Instance.CategoriesPanelSplitSize;
            WorkoutSplit.SplitterDistance = Options.Instance.WorkoutPanelSplitSize;
            StepsNotesSplitter.SplitterDistance = Options.Instance.StepNotesSplitSize;
            StepSplit.SplitterDistance = Math.Max(StepSplit.Panel1MinSize, StepSplit.Height - Options.Instance.StepPanelSplitSize);
            CalendarSplit.SplitterDistance = Math.Max(CalendarSplit.Panel1MinSize, CalendarSplit.Height - Options.Instance.CalendarPanelSplitSize);

            RefreshClipboardControls();
        }

        private void CategoriesSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.Instance.CategoriesPanelSplitSize = e.SplitX;
        }

        private void WorkoutSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.Instance.WorkoutPanelSplitSize = e.SplitX;
        }

        private void StepSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.Instance.StepPanelSplitSize = StepSplit.Height - e.SplitY;
        }

        private void CalendarSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.Instance.CalendarPanelSplitSize = CalendarSplit.Height - e.SplitY;
        }

        private void StepsNotesSplit_SplitterMoving(object sender, SplitterCancelEventArgs e)
        {
            Options.Instance.StepNotesSplitSize = e.SplitY;
        }

        private void DurationComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            IDuration.DurationType newType = (IDuration.DurationType)DurationComboBox.SelectedIndex;

            if (concreteStep.Duration.Type != newType)
            {
                DurationFactory.Create(newType, concreteStep);
            }
        }

        private void TargetComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            ITarget.TargetType newType = (ITarget.TargetType)TargetComboBox.SelectedIndex;

            if (concreteStep.Target.Type != newType)
            {
                TargetFactory.Create(newType, concreteStep);
            }
        }

        private void NewWorkoutButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(SelectedCategory != null);

            SelectedWorkout = GarminWorkoutManager.Instance.CreateWorkout(SelectedCategory);
            SelectedCategory = null;
            WorkoutsList.Focus();
        }

        private void RemoveWorkoutButton_Click(object sender, EventArgs e)
        {
            if (SelectedConcreteWorkouts.Count > 0)
            {
                DeleteSelectedWorkouts();
            }
        }

        private void ScheduleWorkoutButton_Click(object sender, EventArgs e)
        {
            foreach (Workout workout in SelectedConcreteWorkouts)
            {
                if (workout.AddToDailyViewOnSchedule)
                {
                    // Create new activity from template
                    IActivity newActivity = PluginMain.GetApplication().Logbook.Activities.Add(new DateTime(WorkoutCalendar.SelectedDate.Year, WorkoutCalendar.SelectedDate.Month, WorkoutCalendar.SelectedDate.Day, 12, 0, 0));

                    newActivity.Category = workout.Category;
                    newActivity.Name = workout.Name;
                    newActivity.Notes = workout.Notes;
                    newActivity.HasStartTime = true;
                }

                // Schedule workout in plugin
                workout.ScheduleWorkout(WorkoutCalendar.SelectedDate);
            }
        }

        private void RemoveScheduledDateButton_Click(object sender, EventArgs e)
        {
            foreach (Workout workout in SelectedConcreteWorkouts)
            {
                workout.RemoveScheduledDate(WorkoutCalendar.SelectedDate);
            }
        }

        private void RemoveAllScheduledButton_Click(object sender, EventArgs e)
        {
            foreach (Workout workout in SelectedConcreteWorkouts)
            {
                workout.RemoveAllScheduledDates();
            }
        }

        private void AddDailyViewCheckBox_Validated(object sender, EventArgs e)
        {
            if (AddDailyViewCheckBox.CheckState != CheckState.Indeterminate)
            {
                foreach (Workout workout in SelectedConcreteWorkouts)
                {
                    workout.AddToDailyViewOnSchedule = AddDailyViewCheckBox.Checked;
                }
            }
        }

        private void WorkoutCalendar_SelectionChanged(object sender, DateTime newSelection)
        {
            RefreshWorkoutCalendar();
        }

        private void StepsList_SelectedItemsChanged(object sender, EventArgs e)
        {
            List<IStep> newSelection = new List<IStep>();

            // We have multiple items selected
            foreach (StepWrapper currentWrapper in StepsList.Selected)
            {
                newSelection.Add(currentWrapper.Element as IStep);
            }

            SelectedSteps = newSelection;
            UpdateUIFromSteps();
            RefreshClipboardControls();
        }

        private void StepsList_SizeChanged(object sender, EventArgs e)
        {
            if (StepsList.Columns.Count > 0)
            {
                StepsList.Columns[0].Width = StepsList.Width - 60;
            }
        }

        private void WorkoutsList_SelectedItemsChanged(object sender, EventArgs e)
        {
            List<IWorkout> newWorkoutSelection = new List<IWorkout>();
            List<IActivityCategory> newCategorySelection = new List<IActivityCategory>();

            // We have multiple items selected, keep only the workouts
            foreach (object curentSelection in WorkoutsList.Selected)
            {
                if (curentSelection.GetType() == typeof(ActivityCategoryWrapper))
                {
                    newCategorySelection.Add((IActivityCategory)((ActivityCategoryWrapper)curentSelection).Element);
                }
                else if (curentSelection.GetType() == typeof(WorkoutWrapper))
                {
                    newWorkoutSelection.Add((Workout)((WorkoutWrapper)curentSelection).Element);
                }
                else if (curentSelection.GetType() == typeof(WorkoutPartWrapper))
                {
                    newWorkoutSelection.Add((WorkoutPart)((WorkoutPartWrapper)curentSelection).Element);
                }
            }

            SelectedCategories = newCategorySelection;
            SelectedWorkouts = newWorkoutSelection;
            SelectedSteps = null;

            RefreshActions();
            UpdateUIFromWorkouts();
            RefreshClipboardControls();
        }

        private void ChildControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
            }
        }

        private void StepNameText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            Debug.Assert(StepNameText.Text.Length <= Constants.MaxNameLength);
            RegularStep concreteStep = (RegularStep)SelectedStep;

            concreteStep.Name = StepNameText.Text;
        }

        private void IntensityComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;

            concreteStep.Intensity = (RegularStep.StepIntensity)IntensityComboBox.SelectedItem;
        }

        private void ForceSplitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null);

            SelectedStep.ForceSplitOnStep = ForceSplitCheckBox.Checked;
        }

        private void HeartRateDurationReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null);
            bool isPercentMax = HeartRateDurationReferenceComboBox.SelectedIndex == 1;

            if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove)
            {
                HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;

                if (isPercentMax && concreteDuration.MaxHeartRate > Constants.MaxHRInPercentMax)
                {
                    concreteDuration.MaxHeartRate = Constants.MaxHRInPercentMax;
                }
                else if (!isPercentMax && concreteDuration.MaxHeartRate < Constants.MinHRInBPM)
                {
                    concreteDuration.MaxHeartRate = Constants.MinHRInBPM;
                }

                concreteDuration.IsPercentageMaxHeartRate = isPercentMax;
            }
            else if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow)
            {
                HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;

                if (isPercentMax && concreteDuration.MinHeartRate > Constants.MaxHRInPercentMax)
                {
                    concreteDuration.MinHeartRate = Constants.MaxHRInPercentMax;
                }
                else if (!isPercentMax && concreteDuration.MinHeartRate < Constants.MinHRInBPM)
                {
                    concreteDuration.MinHeartRate = Constants.MinHRInBPM;
                }

                concreteDuration.IsPercentageMaxHeartRate = isPercentMax;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        private void HeartRateDurationText_Validating(object sender, CancelEventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove || concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow));
            bool isPercentMax;
            UInt16 oldValue, minRange, maxRange;

            if (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove)
            {
                HeartRateAboveDuration concreteDuration = (HeartRateAboveDuration)concreteStep.Duration;

                isPercentMax = concreteDuration.IsPercentageMaxHeartRate;
                oldValue = concreteDuration.MaxHeartRate;
            }
            else
            {
                HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;

                isPercentMax = concreteDuration.IsPercentageMaxHeartRate;
                oldValue = concreteDuration.MinHeartRate;
            }

            if (isPercentMax)
            {
                minRange = Constants.MinHRInPercentMax;
                maxRange = Constants.MaxHRInPercentMax;
            }
            else
            {
                minRange = Constants.MinHRInBPM;
                maxRange = Constants.MaxHRInBPM;
            }

            e.Cancel = !Utils.IsTextIntegerInRange(HeartRateDurationText.Text, minRange, maxRange);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), minRange, maxRange),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                HeartRateDurationText.Text = oldValue.ToString();
            }
        }

        private void HeartRateDurationText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.HeartRateAbove || concreteStep.Duration.Type == IDuration.DurationType.HeartRateBelow));

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
                Debug.Assert(false);
            }
        }

        private void PowerDurationReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null);
            bool isPercentFTP = PowerDurationReferenceComboBox.SelectedIndex == 1;

            if (concreteStep.Duration.Type == IDuration.DurationType.PowerAbove)
            {
                PowerAboveDuration concreteDuration = (PowerAboveDuration)concreteStep.Duration;

                if (isPercentFTP && concreteDuration.MaxPower > Constants.MaxPowerInPercentFTP)
                {
                    concreteDuration.MaxPower = Constants.MaxPowerInPercentFTP;
                }
                else if (!isPercentFTP && concreteDuration.MaxPower < Constants.MinPowerInWatts)
                {
                    concreteDuration.MaxPower = Constants.MinPowerInWatts;
                }

                concreteDuration.IsPercentFTP = isPercentFTP;
            }
            else if (concreteStep.Duration.Type == IDuration.DurationType.PowerBelow)
            {
                PowerBelowDuration concreteDuration = (PowerBelowDuration)concreteStep.Duration;

                if (isPercentFTP && concreteDuration.MinPower > Constants.MaxPowerInPercentFTP)
                {
                    concreteDuration.MinPower = Constants.MaxPowerInPercentFTP;
                }
                else if (!isPercentFTP && concreteDuration.MinPower < Constants.MinPowerInWatts)
                {
                    concreteDuration.MinPower = Constants.MinPowerInWatts;
                }

                concreteDuration.IsPercentFTP = isPercentFTP;
            }
            else
            {
                Debug.Assert(false);
            }
        }

        private void PowerDurationText_Validating(object sender, CancelEventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.PowerAbove || concreteStep.Duration.Type == IDuration.DurationType.PowerBelow));
            bool isPercentFTP;
            UInt16 oldValue, minRange, maxRange;

            if (concreteStep.Duration.Type == IDuration.DurationType.PowerAbove)
            {
                PowerAboveDuration concreteDuration = (PowerAboveDuration)concreteStep.Duration;

                isPercentFTP = concreteDuration.IsPercentFTP;
                oldValue = concreteDuration.MaxPower;
            }
            else
            {
                PowerBelowDuration concreteDuration = (PowerBelowDuration)concreteStep.Duration;

                isPercentFTP = concreteDuration.IsPercentFTP;
                oldValue = concreteDuration.MinPower;
            }

            if (isPercentFTP)
            {
                minRange = Constants.MinPowerInPercentFTP;
                maxRange = Constants.MaxPowerInPercentFTP;
            }
            else
            {
                minRange = Constants.MinPowerInWatts;
                maxRange = Constants.MaxPowerWorkoutInWatts;
            }

            e.Cancel = !Utils.IsTextIntegerInRange(PowerDurationText.Text, minRange, maxRange);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), minRange, maxRange),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                PowerDurationText.Text = oldValue.ToString();
            }
        }

        private void PowerDurationText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && (concreteStep.Duration.Type == IDuration.DurationType.PowerAbove || concreteStep.Duration.Type == IDuration.DurationType.PowerBelow));

            if (concreteStep.Duration.Type == IDuration.DurationType.PowerAbove)
            {
                PowerAboveDuration concreteDuration = (PowerAboveDuration)concreteStep.Duration;

                concreteDuration.MaxPower = UInt16.Parse(PowerDurationText.Text);
            }
            else if (concreteStep.Duration.Type == IDuration.DurationType.PowerBelow)
            {
                PowerBelowDuration concreteDuration = (PowerBelowDuration)concreteStep.Duration;

                concreteDuration.MinPower = UInt16.Parse(PowerDurationText.Text);
            }
            else
            {
                Debug.Assert(false);
            }
        }

        private void DistanceDurationText_Validating(object sender, CancelEventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;
            double maxDistance;

            if (Utils.IsStatute(concreteDuration.BaseUnit))
            {
                maxDistance = Constants.MaxDistanceStatute;
            }
            else
            {
                maxDistance = Constants.MaxDistanceMetric;
            }

            e.Cancel = !Utils.IsTextFloatInRange(DistanceDurationText.Text, Constants.MinDistance, maxDistance);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), Constants.MinDistance, maxDistance),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                DistanceDurationText.Text = String.Format("{0:0.00}", concreteDuration.GetDistanceInBaseUnit());
            }
        }

        private void DistanceDurationText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Distance);
            DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;

            concreteDuration.SetDistanceInBaseUnit(double.Parse(DistanceDurationText.Text));
        }

        private void TimeDurationUpDown_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Time);
            TimeDuration concreteDuration = (TimeDuration)concreteStep.Duration;

            concreteDuration.TimeInSeconds = TimeDurationUpDown.Duration;
        }

        private void CaloriesDurationText_Validating(object sender, CancelEventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            e.Cancel = !Utils.IsTextIntegerInRange(CaloriesDurationText.Text, Constants.MinCalories, Constants.MaxCalories);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinCalories, Constants.MaxCalories),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                CaloriesDurationText.Text = concreteDuration.CaloriesToSpend.ToString();
            }
        }

        private void CaloriesDurationText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Duration != null && concreteStep.Duration.Type == IDuration.DurationType.Calories);
            CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;

            concreteDuration.CaloriesToSpend = UInt16.Parse(CaloriesDurationText.Text);
        }

        private void ZoneComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Target != null && concreteStep.Target.Type != ITarget.TargetType.Null);
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

            TargetDirtyPictureBox.Visible = concreteStep.Target.IsDirty;
        }

        private void LowRangeTargetText_Validating(object sender, CancelEventArgs e)
        {
            if (SelectedStep != null)
            {
                Debug.Assert(SelectedStep.Type == IStep.StepType.Regular);
                RegularStep concreteStep = (RegularStep)SelectedStep;
                Debug.Assert(concreteStep.Target != null);

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
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);
                                HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MinHeartRate.ToString();
                                inputType = RangeValidationInputType.Integer;

                                if (concreteTarget.IsPercentMaxHeartRate)
                                {
                                    intMin = Constants.MinHRInPercentMax;
                                    intMax = Constants.MaxHRInPercentMax;
                                }
                                else
                                {
                                    intMin = Constants.MinHRInBPM;
                                    intMax = Constants.MaxHRInBPM;
                                }
                                break;
                            }
                        case ITarget.TargetType.Cadence:
                            {
                                BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);
                                CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MinCadence.ToString();
                                intMin = Constants.MinCadence;
                                intMax = Constants.MaxCadence;
                                inputType = RangeValidationInputType.Integer;

                                break;
                            }
                        case ITarget.TargetType.Speed:
                            {
                                BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);
                                SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                                if (concreteTarget.ViewAsPace)
                                {
                                    double paceTime = concreteTarget.GetMaxSpeedInMinutesPerBaseUnit();
                                    UInt16 minutes, seconds;

                                    Utils.DoubleToTime(paceTime, out minutes, out seconds);
                                    oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                                    if (Utils.IsStatute(concreteTarget.BaseUnit))
                                    {
                                        doubleMin = Constants.MinPaceStatute;
                                        doubleMax = Constants.MaxPaceStatute;
                                    }
                                    else
                                    {
                                        doubleMin = Constants.MinPaceMetric;
                                        doubleMax = Constants.MaxPaceMetric;
                                    }
                                    inputType = RangeValidationInputType.Time;
                                }
                                else
                                {
                                    oldValue = String.Format("{0:0.00}", concreteTarget.GetMinSpeedInBaseUnitsPerHour());
                                    if (Utils.IsStatute(concreteTarget.BaseUnit))
                                    {
                                        doubleMin = Constants.MinSpeedStatute;
                                        doubleMax = Constants.MaxSpeedStatute;
                                    }
                                    else
                                    {
                                        doubleMin = Constants.MinSpeedMetric;
                                        doubleMax = Constants.MaxSpeedMetric;
                                    }
                                    inputType = RangeValidationInputType.Double;
                                }

                                break;
                            }
                        case ITarget.TargetType.Power:
                            {
                                BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);
                                PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MinPower.ToString();

                                if (concreteTarget.IsPercentFTP)
                                {
                                    intMin = Constants.MinPowerInPercentFTP;
                                    intMax = Constants.MaxPowerInPercentFTP;
                                }
                                else
                                {
                                    intMin = Constants.MinPowerInWatts;
                                    intMax = Constants.MaxPowerWorkoutInWatts;
                                }
                                inputType = RangeValidationInputType.Integer;

                                break;
                            }
                        default:
                            {
                                Debug.Assert(false);
                                break;
                            }
                    }

                    switch (inputType)
                    {
                        case RangeValidationInputType.Integer:
                            {
                                e.Cancel = !Utils.IsTextIntegerInRange(LowRangeTargetText.Text, intMin, intMax);
                                if (e.Cancel)
                                {
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), intMin, intMax),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    LowRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                        case RangeValidationInputType.Double:
                            {
                                e.Cancel = !Utils.IsTextFloatInRange(LowRangeTargetText.Text, doubleMin, doubleMax);
                                if (e.Cancel)
                                {
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    LowRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                        case RangeValidationInputType.Time:
                            {
                                e.Cancel = !Utils.IsTextTimeInRange(LowRangeTargetText.Text, doubleMin, doubleMax);
                                if (e.Cancel)
                                {
                                    UInt16 minMinutes, minSeconds;
                                    UInt16 maxMinutes, maxSeconds;

                                    Utils.DoubleToTime(doubleMin, out minMinutes, out minSeconds);
                                    Utils.DoubleToTime(doubleMax, out maxMinutes, out maxSeconds);
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("TimeRangeValidationText"),
                                                                  minMinutes, minSeconds,
                                                                  maxMinutes, maxSeconds),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    LowRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void LowRangeTargetText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Target != null);
            bool forceSelectHighTargetText = false;

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(LowRangeTargetText.Text);

                            if (newValue <= concreteTarget.MaxHeartRate)
                            {
                                concreteTarget.SetMinHeartRate(newValue);
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentMaxHeartRate);
                                forceSelectHighTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(LowRangeTargetText.Text);

                            if (newValue <= concreteTarget.MaxCadence)
                            {
                                concreteTarget.SetMinCadence(newValue);
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
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                double time = Utils.TimeToFloat(LowRangeTargetText.Text);

                                if (time <= concreteTarget.GetMinSpeedInMinutesPerBaseUnit())
                                {
                                    concreteTarget.SetMaxSpeedInMinutesPerBaseUnit(time);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInMinutesPerBaseUnit(time, time);
                                    forceSelectHighTargetText = true;
                                }
                            }
                            else
                            {
                                double newValue = double.Parse(LowRangeTargetText.Text);

                                if (newValue <= concreteTarget.GetMaxSpeedInBaseUnitsPerHour())
                                {
                                    concreteTarget.SetMinSpeedInBaseUnitsPerHour(newValue);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInBaseUnitsPerHour(newValue, newValue);
                                    forceSelectHighTargetText = true;
                                }
                            }
                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;
                            UInt16 newValue = UInt16.Parse(LowRangeTargetText.Text);

                            if (newValue <= concreteTarget.MaxPower)
                            {
                                concreteTarget.SetMinPower(newValue);
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentFTP);
                                forceSelectHighTargetText = true;
                            }
                            break;
                        }
                    default:
                        {
                            Debug.Assert(false);
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

        private void HighRangeTargetText_Validating(object sender, CancelEventArgs e)
        {
            if (SelectedStep != null)
            {
                Debug.Assert(SelectedStep.Type == IStep.StepType.Regular);
                RegularStep concreteStep = (RegularStep)SelectedStep;
                Debug.Assert(concreteStep.Target != null);

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
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);
                                HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MaxHeartRate.ToString();
                                inputType = RangeValidationInputType.Integer;

                                if (concreteTarget.IsPercentMaxHeartRate)
                                {
                                    intMin = Constants.MinHRInPercentMax;
                                    intMax = Constants.MaxHRInPercentMax;
                                }
                                else
                                {
                                    intMin = Constants.MinHRInBPM;
                                    intMax = Constants.MaxHRInBPM;
                                }
                                break;
                            }
                        case ITarget.TargetType.Cadence:
                            {
                                BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);
                                CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MaxCadence.ToString();
                                intMin = Constants.MinCadence;
                                intMax = Constants.MaxCadence;
                                inputType = RangeValidationInputType.Integer;

                                break;
                            }
                        case ITarget.TargetType.Speed:
                            {
                                BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);
                                SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                                if (concreteTarget.ViewAsPace)
                                {
                                    double paceTime = concreteTarget.GetMinSpeedInMinutesPerBaseUnit();
                                    UInt16 minutes, seconds;

                                    Utils.DoubleToTime(paceTime, out minutes, out seconds);
                                    oldValue = String.Format("{0:00}:{1:00}", minutes, seconds);
                                    if (Utils.IsStatute(concreteTarget.BaseUnit))
                                    {
                                        doubleMin = Constants.MinPaceStatute;
                                        doubleMax = Constants.MaxPaceStatute;
                                    }
                                    else
                                    {
                                        doubleMin = Constants.MinPaceMetric;
                                        doubleMax = Constants.MaxPaceMetric;
                                    }
                                    inputType = RangeValidationInputType.Time;
                                }
                                else
                                {
                                    oldValue = String.Format("{0:0.00}", concreteTarget.GetMaxSpeedInBaseUnitsPerHour());
                                    if (Utils.IsStatute(concreteTarget.BaseUnit))
                                    {
                                        doubleMin = Constants.MinSpeedStatute;
                                        doubleMax = Constants.MaxSpeedStatute;
                                    }
                                    else
                                    {
                                        doubleMin = Constants.MinSpeedMetric;
                                        doubleMax = Constants.MaxSpeedMetric;
                                    }
                                    inputType = RangeValidationInputType.Double;
                                }

                                break;
                            }
                        case ITarget.TargetType.Power:
                            {
                                BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                                Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);
                                PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                                oldValue = concreteTarget.MaxPower.ToString();

                                if (concreteTarget.IsPercentFTP)
                                {
                                    intMin = Constants.MinPowerInPercentFTP;
                                    intMax = Constants.MaxPowerInPercentFTP;
                                }
                                else
                                {
                                    intMin = Constants.MinPowerInWatts;
                                    intMax = Constants.MaxPowerWorkoutInWatts;
                                }
                                inputType = RangeValidationInputType.Integer;

                                break;
                            }
                        default:
                            {
                                Debug.Assert(false);
                                break;
                            }
                    }

                    switch (inputType)
                    {
                        case RangeValidationInputType.Integer:
                            {
                                e.Cancel = !Utils.IsTextIntegerInRange(HighRangeTargetText.Text, intMin, intMax);
                                if (e.Cancel)
                                {
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), intMin, intMax),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    HighRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                        case RangeValidationInputType.Double:
                            {
                                e.Cancel = !Utils.IsTextFloatInRange(HighRangeTargetText.Text, doubleMin, doubleMax);
                                if (e.Cancel)
                                {
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("DoubleRangeValidationText"), doubleMin, doubleMax),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    HighRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                        case RangeValidationInputType.Time:
                            {
                                e.Cancel = !Utils.IsTextTimeInRange(HighRangeTargetText.Text, doubleMin, doubleMax);
                                if (e.Cancel)
                                {
                                    UInt16 minMinutes, minSeconds;
                                    UInt16 maxMinutes, maxSeconds;

                                    Utils.DoubleToTime(doubleMin, out minMinutes, out minSeconds);
                                    Utils.DoubleToTime(doubleMax, out maxMinutes, out maxSeconds);
                                    MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("TimeRangeValidationText"),
                                                                  minMinutes, minSeconds,
                                                                  maxMinutes, maxSeconds),
                                                    GarminFitnessView.GetLocalizedString("ValueValidationTitleText"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    System.Media.SystemSounds.Asterisk.Play();

                                    // Reset old valid value
                                    HighRangeTargetText.Text = oldValue;
                                }
                                break;
                            }
                    }
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void HighRangeTargetText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Target != null);
            bool forceSelectLowTargetText = false;

            if (concreteStep.Target.Type != ITarget.TargetType.Null)
            {
                switch (concreteStep.Target.Type)
                {
                    case ITarget.TargetType.HeartRate:
                        {
                            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);
                            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.MinHeartRate)
                            {
                                concreteTarget.SetMaxHeartRate(newValue);
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentMaxHeartRate);
                                forceSelectLowTargetText = true;
                            }
                            break;
                        }
                    case ITarget.TargetType.Cadence:
                        {
                            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);
                            CadenceRangeTarget concreteTarget = (CadenceRangeTarget)baseTarget.ConcreteTarget;
                            Byte newValue = Byte.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.MinCadence)
                            {
                                concreteTarget.SetMaxCadence(newValue);
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
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);
                            SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                            if (concreteTarget.ViewAsPace)
                            {
                                float time = Utils.TimeToFloat(HighRangeTargetText.Text);

                                if (time >= concreteTarget.GetMaxSpeedInMinutesPerBaseUnit())
                                {
                                    concreteTarget.SetMinSpeedInMinutesPerBaseUnit(time);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInMinutesPerBaseUnit(time, time);
                                    forceSelectLowTargetText = true;
                                }
                            }
                            else
                            {
                                double newValue = double.Parse(HighRangeTargetText.Text);

                                if (newValue >= concreteTarget.GetMinSpeedInBaseUnitsPerHour())
                                {
                                    concreteTarget.SetMaxSpeedInBaseUnitsPerHour(newValue);
                                }
                                else
                                {
                                    concreteTarget.SetRangeInBaseUnitsPerHour(newValue, newValue);
                                    forceSelectLowTargetText = true;
                                }
                            }
                            break;
                        }
                    case ITarget.TargetType.Power:
                        {
                            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
                            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);
                            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;
                            UInt16 newValue = UInt16.Parse(HighRangeTargetText.Text);

                            if (newValue >= concreteTarget.MinPower)
                            {
                                concreteTarget.SetMaxPower(newValue);
                            }
                            else
                            {
                                concreteTarget.SetValues(newValue, newValue, concreteTarget.IsPercentFTP);
                                forceSelectLowTargetText = true;
                            }
                            break;
                        }
                    default:
                        {
                            Debug.Assert(false);
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

        private void RepetitionCountText_Validating(object sender, CancelEventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)SelectedStep;

            e.Cancel = !Utils.IsTextIntegerInRange(RepetitionCountText.Text, Constants.MinRepeats, Constants.MaxRepeats);
            if (e.Cancel)
            {
                MessageBox.Show(String.Format(GarminFitnessView.GetLocalizedString("IntegerRangeValidationText"), Constants.MinRepeats, Constants.MaxRepeats),
                                GarminFitnessView.GetLocalizedString("ValueValidationTitleText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                System.Media.SystemSounds.Asterisk.Play();

                // Reset old valid value
                RepetitionCountText.Text = concreteStep.RepetitionCount.ToString();
            }
        }

        private void RepetitionCountText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Repeat);
            RepeatStep concreteStep = (RepeatStep)SelectedStep;

            concreteStep.RepetitionCount = Byte.Parse(RepetitionCountText.Text);
        }

        private void WorkoutNotesText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedWorkout != null);

            if (!(SelectedWorkout is WorkoutPart))
            {
                SelectedWorkout.Notes = WorkoutNotesText.Text;
            }
        }

        private void StepNotesText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null);

            SelectedStep.Notes = StepNotesText.Text;
        }

        private void WorkoutNameText_Validating(object sender, CancelEventArgs e)
        {
            if (!(SelectedWorkout is WorkoutPart))
            {
                Workout workoutWithSameName = GarminWorkoutManager.Instance.GetWorkout(WorkoutNameText.Text);

                if (WorkoutNameText.Text == String.Empty || (workoutWithSameName != null && workoutWithSameName != SelectedWorkout))
                {
                    e.Cancel = true;

                    MessageBox.Show(GarminFitnessView.GetLocalizedString("InvalidWorkoutNameText"),
                                    GarminFitnessView.GetLocalizedString("ErrorText"),
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void WorkoutNameText_Validated(object sender, EventArgs e)
        {
            Debug.Assert(SelectedWorkout != null);

            if (!(SelectedWorkout is WorkoutPart))
            {
                Debug.Assert((SelectedWorkout as Workout) != null);

                Workout concreteWorkout = SelectedWorkout as Workout;

                concreteWorkout.Name = WorkoutNameText.Text;
            }
        }

        private void HRRangeReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Target != null && concreteStep.Target.Type == ITarget.TargetType.HeartRate);
            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;
            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);
            HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;

            concreteTarget.SetIsPercentMax(HRRangeReferenceComboBox.SelectedIndex == 1);
        }
        
        private void PowerRangeReferenceComboBox_SelectionChangedCommited(object sender, EventArgs e)
        {
            Debug.Assert(SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            Debug.Assert(concreteStep.Target != null && concreteStep.Target.Type == ITarget.TargetType.Power);
            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;
            Debug.Assert(baseTarget.ConcreteTarget != null && baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);
            PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

            concreteTarget.SetIsPercentFTP(PowerRangeReferenceComboBox.SelectedIndex == 1);
        }

        private void WorkoutsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && SelectedConcreteWorkouts.Count > 0)
            {
                DeleteSelectedWorkouts();
            }
            else if (e.Control)
            {
                if (SelectedConcreteWorkouts.Count > 0)
                {
                    if(e.KeyCode == Keys.C)
                    {
                        CopyWorkoutSelection();

                        RefreshClipboardControls();
                    }
                    else if (e.KeyCode == Keys.X)
                    {
                        CutWorkoutSelection();

                        RefreshClipboardControls();
                    }
                }
                
                if (e.KeyCode == Keys.V && Clipboard.ContainsData(Constants.WorkoutsClipboardID))
                {
                    PasteWorkoutsFromClipboard();

                    RefreshClipboardControls();
                }
                else if (e.KeyCode == Keys.N)
                {
                    // New regular step
                    if (SelectedCategory != null)
                    {
                        SelectedWorkout = GarminWorkoutManager.Instance.CreateWorkout(SelectedCategory);
                        SelectedCategory = null;
                    }
                }
            }
        }

        private void WorkoutsList_DragStart(object sender, EventArgs e)
        {
            if (SelectedConcreteWorkouts.Count > 0)
            {
                WorkoutsList.DoDragDrop(SelectedConcreteWorkouts, DragDropEffects.Move | DragDropEffects.Copy);
            }
        }

        private void WorkoutsList_DragDrop(object sender, DragEventArgs e)
        {
            TreeList.RowHitState type;
            Point mouseLocation = new Point(e.X, e.Y);
            List<Workout> workoutsToMove = (List<Workout>)e.Data.GetData(typeof(List<Workout>));
            object item = WorkoutsList.RowHitTest(WorkoutsList.PointToClient(mouseLocation), out type);

            if (item != null && workoutsToMove != null && workoutsToMove.Count > 0)
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
                    Debug.Assert(false);
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
                SelectedCategory = null;
            }
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

        private void StepsList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && SelectedSteps.Count > 0)
            {
                DeleteSelectedSteps();
            }
            else if (e.Control)
            {
                if (SelectedSteps.Count > 0)
                {
                    if (e.KeyCode == Keys.C)
                    {
                        CopyStepSelection();

                        RefreshClipboardControls();
                    }
                    else if (e.KeyCode == Keys.X)
                    {
                        CutStepSelection();

                        RefreshClipboardControls();
                    }
                }
                
                if (e.KeyCode == Keys.V && SelectedWorkout != null &&
                    Clipboard.ContainsData(Constants.StepsClipboardID))
                {
                    PasteStepsFromClipboard();

                    RefreshClipboardControls();
                }
                else if (e.KeyCode == Keys.N)
                {
                    // New regular step
                    AddNewStep(new RegularStep(SelectedWorkout.ConcreteWorkout));
                }
                else if (e.KeyCode == Keys.R)
                {
                    // New repeat step
                    AddNewStep(new RepeatStep(SelectedWorkout.ConcreteWorkout));
                }
                else if (e.KeyCode == Keys.Up)
                {
                    // Move step up
                    UInt16 selectedPosition = 0;
                    List<IStep> selectedList = null;

                    if (SelectedStep != null && Utils.GetStepInfo(SelectedStep, SelectedWorkout.Steps, out selectedList, out selectedPosition))
                    {
                        if (selectedPosition > 0)
                        {
                            SelectedWorkout.MoveStepUp(SelectedStep);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Down)
                {
                    // Move step down
                    UInt16 selectedPosition = 0;
                    List<IStep> selectedList = null;

                    if (SelectedStep != null && Utils.GetStepInfo(SelectedStep, SelectedWorkout.Steps, out selectedList, out selectedPosition))
                    {
                        if (selectedPosition < selectedList.Count - 1)
                        {
                            SelectedWorkout.MoveStepDown(SelectedStep);
                        }
                    }
                }
            }
        }

        private void StepsList_DragStart(object sender, EventArgs e)
        {
            if (SelectedSteps.Count > 0)
            {
                StepsList.DoDragDrop(GetMinimalStepsBase(SelectedSteps), DragDropEffects.Move | DragDropEffects.Copy);
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

            destination = StepsList.RowHitTest(clientPoint, out rowNumber, out isInUpperHalf);
            if (destination == null)
            {
                // Insert as the last item in the workout
                destinationStep = SelectedWorkout.Steps[SelectedWorkout.Steps.Count - 1];
            }
            else
            {
                destinationStep = (IStep)((StepWrapper)destination).Element;
            }

            if (e.Effect == DragDropEffects.Move)
            {
                if (isInUpperHalf)
                {
                    SelectedWorkout.Steps.MoveStepsBeforeStep(stepsToMove, destinationStep);
                }
                else
                {
                    SelectedWorkout.Steps.MoveStepsAfterStep(stepsToMove, destinationStep);
                }
            }
            else
            {
                // Make a copy
                List<IStep> newSteps = new List<IStep>();

                for (int i = 0; i < stepsToMove.Count; ++i)
                {
                    newSteps.Add(stepsToMove[i].Clone());
                }

                // Add new items
                if (isInUpperHalf)
                {
                    SelectedWorkout.Steps.InsertStepsBeforeStep(newSteps, destinationStep);
                }
                else
                {
                    SelectedWorkout.Steps.InsertStepsAfterStep(newSteps, destinationStep);
                }

                SelectedSteps = newSteps;
            }

            renderer.IsInDrag = false;
            UpdateUIFromSteps();
            StepsList.Invalidate();
        }

        private void StepsList_DragOver(object sender, DragEventArgs e)
        {
            List<IStep> stepsToMove = (List<IStep>)e.Data.GetData(typeof(List<IStep>));

            e.Effect = DragDropEffects.None;

            if (stepsToMove != null && stepsToMove.Count > 0)
            {
                bool isCtrlKeyDown = (e.KeyState & CTRL_KEY_CODE) != 0;
                StepRowDataRenderer renderer = (StepRowDataRenderer)StepsList.RowDataRenderer;
                Point mouseLocation = StepsList.PointToClient(new Point(e.X, e.Y));
                object item = StepsList.RowHitTest(mouseLocation);
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

                    // Make sure you only consider the topmost workout link when we're a link child
                    StepWrapper wrapper = GetStepWrapper(SelectedWorkout.ConcreteWorkout, destinationStep);
                    while (wrapper.IsWorkoutLinkChild)
                    {
                        wrapper = GetStepWrapper(SelectedWorkout.ConcreteWorkout, wrapper.Parent as IStep);
                    }
                }

                // We need to count the number of items being moved
                for (int i = 0; i < stepsToMove.Count; ++i)
                {
                    stepsDragged += stepsToMove[i].StepCount;
                }

                bool canMoveStep = true;
                if (isCtrlKeyDown)
                {
                    canMoveStep = SelectedWorkout.CanAcceptNewStep(stepsDragged, destinationStep);
                }
                else
                {
                    IStep destinationStepTopMostRepeat = SelectedWorkout.Steps.GetTopMostRepeatForStep(destinationStep);

                    for (int i = 0; i < stepsToMove.Count; ++i)
                    {
                        IStep currentMoveStep = stepsToMove[i];
                        IStep currentMoveStepTopMostRepeat = SelectedWorkout.Steps.GetTopMostRepeatForStep(currentMoveStep);

                        if (destinationStepTopMostRepeat != null &&
                            currentMoveStepTopMostRepeat != destinationStepTopMostRepeat)
                        {
                            // It is not allowed to move a repeat inside the top most if we bust
                            // the limit of 20 steps
                            if (!SelectedWorkout.CanAcceptNewStep(stepsDragged, destinationStep))
                            {
                                canMoveStep = false;
                                break;
                            }
                        }

                        // Make sure we are not moving a repeat inside itself
                        if (currentMoveStep.Type == IStep.StepType.Repeat)
                        {
                            RepeatStep repeatStep = (RepeatStep)currentMoveStep;

                            if (repeatStep.IsChildStep(destinationStep))
                            {
                                canMoveStep = false;
                                break;
                            }
                        }
                    }
                }

                if (canMoveStep)
                {
                    if (!isCtrlKeyDown)
                    {
                        e.Effect = DragDropEffects.Move;
                    }
                    else
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }

                renderer.IsInDrag = canMoveStep;
                renderer.DragOverClientPosition = mouseLocation;
                StepsList.Invalidate();
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

        private void AddStepButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RegularStep(SelectedWorkout.ConcreteWorkout));
        }

        private void AddRepeatButton_Click(object sender, EventArgs e)
        {
            AddNewStep(new RepeatStep(SelectedWorkout.ConcreteWorkout));
        }

        private void AddLinkStepButton_Click(object sender, EventArgs e)
        {
            WorkoutLinkSelectionDialog dlg = new WorkoutLinkSelectionDialog(SelectedWorkout.ConcreteWorkout, SelectedStep);
            DialogResult result = DialogResult.Cancel;

            result = dlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                AddNewStep(new WorkoutLinkStep(SelectedWorkout.ConcreteWorkout, dlg.SelectedWorkout));
            }
        }

        private void RemoveStepButton_Click(object sender, EventArgs e)
        {
            DeleteSelectedSteps();
        }

        private void MoveUpButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null);

            SelectedWorkout.MoveStepUp(SelectedStep);

            // Updates the Move Up & Move Down buttons
            RefreshStepSelection();
        }

        private void MoveDownButton_Click(object sender, EventArgs e)
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null);

            SelectedWorkout.MoveStepDown(SelectedStep);

            // Updates the Move Up & Move Down buttons
            RefreshStepSelection();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://code.google.com/p/garminworkouts/wiki/Welcome?tm=6");
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            PluginMain.GetApplication().ShowView(GUIDs.SettingsView, "PageId=" + GUIDs.GarminFitnessSettings.ToString());
        }

        private void StepsList_FocusEnter(object sender, EventArgs e)
        {
            m_LastClipboardControlFocused = ClipboardControls.StepsList;
        }

        private void WorkoutsList_FocusEnter(object sender, EventArgs e)
        {
            m_LastClipboardControlFocused = ClipboardControls.WorkoutsList;
        }

        private void GenericControl_FocusEnter(object sender, EventArgs e)
        {
            m_LastClipboardControlFocused = ClipboardControls.Invalid;
        }

        private void TextboxControl_FocusEnter(object sender, EventArgs e)
        {
            m_LastClipboardControlFocused = ClipboardControls.TextBox;
            m_LastFocusTextBox = (ExtendedTextBox)sender;
        }

        void ChildControl_GotFocus(object sender, EventArgs e)
        {
            RefreshClipboardControls();
        }

        private void PasteButton_Click(object sender, EventArgs e)
        {
            switch(m_LastClipboardControlFocused)
            {
                case ClipboardControls.WorkoutsList:
                    {
                        PasteWorkoutsFromClipboard();
                        break;
                    }
                case ClipboardControls.StepsList:
                    {
                        PasteStepsFromClipboard();
                        break;
                    }
                case ClipboardControls.TextBox:
                    {
                        m_LastFocusTextBox.Paste();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            RefreshClipboardControls();
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            switch (m_LastClipboardControlFocused)
            {
                case ClipboardControls.WorkoutsList:
                    {
                        CopyWorkoutSelection();
                        break;
                    }
                case ClipboardControls.StepsList:
                    {
                        CopyStepSelection();
                        break;
                    }
                case ClipboardControls.TextBox:
                    {
                        m_LastFocusTextBox.Copy();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            RefreshClipboardControls();
        }

        private void CutButton_Click(object sender, EventArgs e)
        {
            switch (m_LastClipboardControlFocused)
            {
                case ClipboardControls.WorkoutsList:
                    {
                        CutWorkoutSelection();
                        break;
                    }
                case ClipboardControls.StepsList:
                    {
                        CutStepSelection();
                        break;
                    }
                case ClipboardControls.TextBox:
                    {
                        m_LastFocusTextBox.Cut();
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            RefreshClipboardControls();
        }

        private void DonateButton_Click(object sender, EventArgs e)
        {
            Options.Instance.DonationReminderDate = new DateTime(0);

            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=H3VUJCWFVH2J2&lc=CA&item_name=PissedOffCil%20ST%20Plugins&item_number=Garmin%20Fitness&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted");
        }

#endregion

        public void RefreshCalendar()
        {
            RefreshCalendarView();
        }

        public void ThemeChanged(ITheme visualTheme)
        {
            CategoriesBanner.ThemeChanged(visualTheme);
            WorkoutsList.ThemeChanged(visualTheme);
            DetailsBanner.ThemeChanged(visualTheme);
            StepDetailsBanner.ThemeChanged(visualTheme);
            ScheduleBanner.ThemeChanged(visualTheme);
            StepsList.ThemeChanged(visualTheme);
            WorkoutCalendar.ThemeChanged(visualTheme);
            WorkoutNameText.ThemeChanged(visualTheme);
            WorkoutNotesText.ThemeChanged(visualTheme);
            RepetitionCountText.ThemeChanged(visualTheme);
            StepNameText.ThemeChanged(visualTheme);
            StepNotesText.ThemeChanged(visualTheme);
            CaloriesDurationText.ThemeChanged(visualTheme);
            HeartRateDurationText.ThemeChanged(visualTheme);
            DistanceDurationText.ThemeChanged(visualTheme);
            LowRangeTargetText.ThemeChanged(visualTheme);
            HighRangeTargetText.ThemeChanged(visualTheme);

            StepSplit.Panel1.BackColor = visualTheme.Control;
            splitContainer10.Panel2.BackColor = visualTheme.Control;
        }

        public void UICultureChanged(System.Globalization.CultureInfo culture)
        {
            UpdateUIStrings();

            if (PluginMain.GetApplication().Logbook != null)
            {
                BuildWorkoutsList();
            }
        }

        public void RefreshUIFromLogbook()
        {
            SelectedCategory = null;
            SelectedWorkout = null;
            SelectedSteps = null;

            RefreshCalendarView();
            if (PluginMain.GetApplication().Logbook != null)
            {
                BuildWorkoutsList();
            }

            if (WorkoutsList.RowData != null)
            {
                WorkoutsList.SetExpanded(WorkoutsList.RowData, true, true);
            }

            RefreshClipboardControls();
        }

        private void UpdateUIStrings()
        {
            CategoriesBanner.Text = GarminFitnessView.GetLocalizedString("CategoriesText");
            DetailsBanner.Text = GarminFitnessView.GetLocalizedString("DetailsText");
            StepDetailsBanner.Text = GarminFitnessView.GetLocalizedString("StepDetailsText");
            ScheduleBanner.Text = GarminFitnessView.GetLocalizedString("ScheduleBannerText");

            NameLabel.Text = GarminFitnessView.GetLocalizedString("NameLabelText");
            WorkoutNotesLabel.Text = GarminFitnessView.GetLocalizedString("NotesLabelText");
            StepNotesLabel.Text = GarminFitnessView.GetLocalizedString("NotesLabelText");
            StepNameLabel.Text = GarminFitnessView.GetLocalizedString("StepNameLabelText");
            StepDurationGroup.Text = GarminFitnessView.GetLocalizedString("StepDurationGroupText");
            StepDurationLabel.Text = GarminFitnessView.GetLocalizedString("StepDurationLabelText");
            StepTargetGroup.Text = GarminFitnessView.GetLocalizedString("StepTargetGroupText");
            StepTargetLabel.Text = GarminFitnessView.GetLocalizedString("StepTargetLabelText");
            CaloriesDurationLabel.Text = GarminFitnessView.GetLocalizedString("CaloriesDurationLabelText");
            DistanceDurationLabel.Text = GarminFitnessView.GetLocalizedString("DistanceDurationLabelText");
            HeartRateDurationLabel.Text = GarminFitnessView.GetLocalizedString("HeartRateDurationLabelText");
            PowerDurationLabel.Text = GarminFitnessView.GetLocalizedString("PowerDurationLabelText");
            TimeDurationLabel.Text = GarminFitnessView.GetLocalizedString("TimeDurationLabelText");
            ZoneLabel.Text = GarminFitnessView.GetLocalizedString("WhichZoneText");
            LowRangeTargetLabel.Text = GarminFitnessView.GetLocalizedString("BetweenText");
            MiddleRangeTargetLabel.Text = GarminFitnessView.GetLocalizedString("AndText");
            RepetitionPropertiesGroup.Text = GarminFitnessView.GetLocalizedString("RepetitionPropertiesGroupText");
            RepetitionCountLabel.Text = GarminFitnessView.GetLocalizedString("RepetitionCountLabelText");
            ExportDateTextLabel.Text = GarminFitnessView.GetLocalizedString("LastExportDateText");
            AddDailyViewCheckBox.Text = GarminFitnessView.GetLocalizedString("AddDailyViewCheckBoxText");
            ForceSplitCheckBox.Text = GarminFitnessView.GetLocalizedString("ForceSplitCheckBoxText");

            if (SelectedWorkout != null)
            {
                if (SelectedWorkout.LastExportDate.Ticks == 0)
                {
                    ExportDateLabel.Text = GarminFitnessView.GetLocalizedString("NeverExportedText");
                }
                else
                {
                    CultureInfo culture = CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name);

                    ExportDateLabel.Text = SelectedWorkout.LastExportDate.ToString(culture.DateTimeFormat.ShortDatePattern) +
                                            " " + SelectedWorkout.LastExportDate.ToString(culture.DateTimeFormat.ShortTimePattern);
                }
            }

            // Update duration heart rate reference combo box text
            HeartRateDurationReferenceComboBox.Items.Clear();
            HeartRateDurationReferenceComboBox.Items.Add(CommonResources.Text.LabelBPM);
            HeartRateDurationReferenceComboBox.Items.Add(CommonResources.Text.LabelPercentOfMax);

            // Update duration heart rate reference combo box text
            PowerDurationReferenceComboBox.Items.Clear();
            PowerDurationReferenceComboBox.Items.Add(CommonResources.Text.LabelWatts);
            PowerDurationReferenceComboBox.Items.Add(GarminFitnessView.GetLocalizedString("PercentFTPText"));

            // Update target heart rate reference combo box text
            HRRangeReferenceComboBox.Items.Clear();
            HRRangeReferenceComboBox.Items.Add(CommonResources.Text.LabelBPM);
            HRRangeReferenceComboBox.Items.Add(CommonResources.Text.LabelPercentOfMax);

            // Update target power reference combo box text
            PowerRangeReferenceComboBox.Items.Clear();
            PowerRangeReferenceComboBox.Items.Add(CommonResources.Text.LabelWatts);
            PowerRangeReferenceComboBox.Items.Add(GarminFitnessView.GetLocalizedString("PercentFTPText"));

            // Update duration combo box
            int currentSelection = DurationComboBox.SelectedIndex;
            DurationComboBox.Items.Clear();
            for (int i = 0; i < (int)IDuration.DurationType.DurationTypeCount; ++i)
            {
                IDuration.DurationType currentDuration = (IDuration.DurationType)i;
                FieldInfo durationFieldInfo = currentDuration.GetType().GetField(Enum.GetName(currentDuration.GetType(), currentDuration));
                ComboBoxStringProviderAttribute providerAttribute = (ComboBoxStringProviderAttribute)Attribute.GetCustomAttribute(durationFieldInfo, typeof(ComboBoxStringProviderAttribute));

                DurationComboBox.Items.Add(GarminFitnessView.GetLocalizedString(providerAttribute.StringName));

                if (currentSelection == i)
                {
                    DurationComboBox.Text = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);
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

                TargetComboBox.Items.Add(GarminFitnessView.GetLocalizedString(providerAttribute.StringName));

                if (currentSelection == i)
                {
                    TargetComboBox.Text = GarminFitnessView.GetLocalizedString(providerAttribute.StringName);
                }
            }
        }

        private void UpdateDurationPanelVisibility()
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            IDuration duration = concreteStep.Duration;

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

        private void UpdateTargetPanelVisibility()
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            ITarget target = concreteStep.Target;

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
                        PowerRangeReferenceComboBox.Visible = false;

                        BuildSpeedComboBox(baseTarget);
                        SetRangeTargetControlsVisibility(baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range);
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)target;

                        RangeTargetUnitsLabel.Visible = true;
                        HRRangeReferenceComboBox.Visible = false;
                        PowerRangeReferenceComboBox.Visible = false;

                        BuildCadenceComboBox(baseTarget);
                        SetRangeTargetControlsVisibility(baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range);
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)target;

                        BuildHRComboBox(baseTarget);
                        SetRangeTargetControlsVisibility(baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range);

                        RangeTargetUnitsLabel.Visible = false;
                        HRRangeReferenceComboBox.Visible = baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range;
                        PowerRangeReferenceComboBox.Visible = false;
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)target;

                        BuildPowerComboBox(baseTarget);
                        SetRangeTargetControlsVisibility(baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range);

                        RangeTargetUnitsLabel.Visible = false;
                        HRRangeReferenceComboBox.Visible = false;
                        PowerRangeReferenceComboBox.Visible = baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        private void UpdateUIFromWorkouts()
        {
            UpdateUIFromWorkout(SelectedSteps);
        }

        private void UpdateUIFromWorkout()
        {
            UpdateUIFromWorkout(null);
        }

        private void RefreshUIFromWorkouts()
        {
            if (SelectedWorkout != null)
            {
                if (SelectedWorkout.LastExportDate.Ticks == 0)
                {
                    ExportDateLabel.Text = GarminFitnessView.GetLocalizedString("NeverExportedText");
                }
                else
                {
                    CultureInfo culture = CultureInfo.CreateSpecificCulture(GarminFitnessView.UICulture.Name);

                    ExportDateLabel.Text = SelectedWorkout.LastExportDate.ToString(culture.DateTimeFormat.ShortDatePattern) +
                                            " " + SelectedWorkout.LastExportDate.ToString(culture.DateTimeFormat.ShortTimePattern);
                }

                // Update control with workout data
                WorkoutNameText.Text = SelectedWorkout.Name;
                WorkoutNameText.Enabled = SelectedWorkout is Workout;
                WorkoutNotesText.Text = SelectedWorkout.Notes;
                WorkoutNotesText.Enabled = SelectedWorkout is Workout;

                StepsList.Columns[1].Visible = SelectedWorkout is Workout;

                // Highlight scheduled dates
                WorkoutCalendar.RemoveAllMarkedDatesStyle(ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                for (int i = 0; i < SelectedWorkout.ScheduledDates.Count; ++i)
                {
                    WorkoutCalendar.AddMarkedDateStyle(SelectedWorkout.ScheduledDates[i], ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);
                }
                AddDailyViewCheckBox.Enabled = SelectedWorkout is Workout;
                AddDailyViewCheckBox.ThreeState = false;
                AddDailyViewCheckBox.CheckState = SelectedWorkout.AddToDailyViewOnSchedule ? CheckState.Checked : CheckState.Unchecked;
            }
        }

        private void UpdateUIFromWorkout(List<IStep> forcedSelection)
        {
            RefreshUIFromWorkouts();

            if (SelectedWorkout != null)
            {
                BuildStepsList();

                // Force selection
                if (forcedSelection != null)
                {
                    SelectedSteps = forcedSelection;
                }
            }
            else
            {
                StepsList.RowData = new List<TreeList.TreeListNode>();
            }
        }

        private void UpdateUIFromSteps()
        {
            if (SelectedWorkout != null && SelectedSteps.Count <= 1)
            {
                UpdateUIFromStep();
            }
        }

        private void UpdateUIFromStep()
        {
            if (SelectedStep != null)
            {
                StepNotesText.Text = SelectedStep.Notes;
                ForceSplitCheckBox.Visible = Options.Instance.AllowSplitWorkouts || SelectedWorkout.GetSplitPartsCount() > 1;
                ForceSplitCheckBox.Enabled = SelectedWorkout.Steps.GetTopMostRepeatForStep(SelectedStep) == null;
                ForceSplitCheckBox.Checked = SelectedStep.ForceSplitOnStep;

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
                        IntensityComboBox.Visible = true;

                        StepWrapper wrapper = GetStepWrapper(SelectedWorkout.ConcreteWorkout, SelectedStep);

                        StepSplit.Panel2.Enabled = !wrapper.IsWorkoutLinkChild;

                        RegularStep concreteStep = (RegularStep)SelectedStep;

                        if (concreteStep.Name != null && concreteStep.Name != String.Empty)
                        {
                            StepNameText.Text = concreteStep.Name;
                        }
                        else
                        {
                            StepNameText.Text = "";
                        }

                        IntensityComboBox.SelectedItem = concreteStep.Intensity;
                        DurationComboBox.SelectedIndex = (int)concreteStep.Duration.Type;
                        TargetComboBox.SelectedIndex = (int)concreteStep.Target.Type;

                        // Update duration
                        UpdateDurationPanelVisibility();
                        UpdateUIFromDuration();

                        // Update target
                        UpdateTargetPanelVisibility();
                        UpdateUIFromTarget();
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
                        IntensityComboBox.Visible = false;

                        StepWrapper wrapper = GetStepWrapper(SelectedWorkout.ConcreteWorkout, SelectedStep);

                        StepSplit.Panel2.Enabled = !wrapper.IsWorkoutLinkChild;

                        RepeatStep concreteStep = SelectedStep as RepeatStep;

                        RepetitionCountText.Text = concreteStep.RepetitionCount.ToString();

                        break;
                    }
                case IStep.StepType.Link:
                    {
                        // Show correct panels/controls
                        RepetitionPropertiesGroup.Visible = false;
                        StepDurationGroup.Visible = false;
                        StepTargetGroup.Visible = false;
                        StepNameLabel.Visible = false;
                        StepNameText.Visible = false;
                        IntensityComboBox.Visible = false;

                        StepSplit.Panel2.Enabled = true;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
                }

                RefreshWorkoutSelectionControls();
            }
        }

        private void UpdateUIFromDuration()
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            IDuration duration = concreteStep.Duration;

            switch (concreteStep.Duration.Type)
            {
                case IDuration.DurationType.LapButton:
                    {
                        break;
                    }
                case IDuration.DurationType.Distance:
                    {
                        DistanceDuration concreteDuration = (DistanceDuration)concreteStep.Duration;
                        double distance = concreteDuration.GetDistanceInBaseUnit();
                        DistanceDurationText.Text = String.Format("{0:0.00}", distance);
                        DistanceDurationUnitsLabel.Text = Length.LabelAbbr(concreteDuration.BaseUnit);
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
                        HeartRateDurationReferenceComboBox.SelectedIndex = concreteDuration.IsPercentageMaxHeartRate ? 1 : 0;
                        break;
                    }
                case IDuration.DurationType.HeartRateBelow:
                    {
                        HeartRateBelowDuration concreteDuration = (HeartRateBelowDuration)concreteStep.Duration;
                        HeartRateDurationText.Text = concreteDuration.MinHeartRate.ToString();
                        HeartRateDurationReferenceComboBox.SelectedIndex = concreteDuration.IsPercentageMaxHeartRate ? 1 : 0;
                        break;
                    }
                case IDuration.DurationType.Calories:
                    {
                        CaloriesDuration concreteDuration = (CaloriesDuration)concreteStep.Duration;
                        CaloriesDurationText.Text = concreteDuration.CaloriesToSpend.ToString();
                        break;
                    }
                case IDuration.DurationType.PowerAbove:
                    {
                        PowerAboveDuration concreteDuration = (PowerAboveDuration)concreteStep.Duration;
                        PowerDurationText.Text = concreteDuration.MaxPower.ToString();
                        PowerDurationReferenceComboBox.SelectedIndex = concreteDuration.IsPercentFTP ? 1 : 0;
                        break;
                    }
                case IDuration.DurationType.PowerBelow:
                    {
                        PowerBelowDuration concreteDuration = (PowerBelowDuration)concreteStep.Duration;
                        PowerDurationText.Text = concreteDuration.MinPower.ToString();
                        PowerDurationReferenceComboBox.SelectedIndex = concreteDuration.IsPercentFTP ? 1 : 0;
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        private void UpdateUIFromTarget()
        {
            Debug.Assert(SelectedWorkout != null && SelectedStep != null && SelectedStep.Type == IStep.StepType.Regular);
            RegularStep concreteStep = (RegularStep)SelectedStep;
            ITarget target = concreteStep.Target;

            switch (target.Type)
            {
                case ITarget.TargetType.Null:
                    {
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        BaseSpeedTarget baseTarget = (BaseSpeedTarget)target;

                        BuildSpeedComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneGTC:
                                {
                                    SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                    break;
                                }
                            case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneST:
                                {
                                    SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(baseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones,
                                                                                        concreteTarget.Zone) + 1;
                                    break;
                                }
                            case BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range:
                                {
                                    SpeedRangeTarget concreteTarget = (SpeedRangeTarget)baseTarget.ConcreteTarget;

                                    if (concreteTarget.ViewAsPace)
                                    {
                                        RangeTargetUnitsLabel.Text = GarminFitnessView.GetLocalizedString("MinuteAbbrText") + "/" + Length.LabelAbbr(concreteTarget.BaseUnit);
                                        double min = concreteTarget.GetMinSpeedInMinutesPerBaseUnit();
                                        double max = concreteTarget.GetMaxSpeedInMinutesPerBaseUnit();
                                        UInt16 minutes;
                                        UInt16 seconds;

                                        Utils.DoubleToTime(min, out minutes, out seconds);
                                        HighRangeTargetText.Text = String.Format("{0:00}:{1:00}", minutes, seconds);
                                        Utils.DoubleToTime(max, out minutes, out seconds);
                                        LowRangeTargetText.Text = String.Format("{0:00}:{1:00}", minutes, seconds);
                                    }
                                    else
                                    {
                                        RangeTargetUnitsLabel.Text = Length.LabelAbbr(concreteTarget.BaseUnit) + GarminFitnessView.GetLocalizedString("PerHourText");
                                        LowRangeTargetText.Text = String.Format("{0:0.00}", concreteTarget.GetMinSpeedInBaseUnitsPerHour());
                                        HighRangeTargetText.Text = String.Format("{0:0.00}", concreteTarget.GetMaxSpeedInBaseUnitsPerHour());
                                    }

                                    ZoneComboBox.SelectedIndex = 0;

                                    break;
                                }
                        }
                        break;
                    }
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)target;

                        BuildCadenceComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST:
                                {
                                    CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(Options.Instance.CadenceZoneCategory.Zones,
                                                                                                       concreteTarget.Zone) + 1;
                                    break;
                                }
                            case BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range:
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
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)target;

                        BuildHRComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC:
                                {
                                    HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                    break;
                                }
                            case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneST:
                                {
                                    HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(baseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones,
                                                                                                       concreteTarget.Zone) + 1;
                                    break;
                                }
                            case BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range:
                                {
                                    HeartRateRangeTarget concreteTarget = (HeartRateRangeTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = 0;
                                    LowRangeTargetText.Text = concreteTarget.MinHeartRate.ToString();
                                    HighRangeTargetText.Text = concreteTarget.MaxHeartRate.ToString();
                                    HRRangeReferenceComboBox.SelectedIndex = concreteTarget.IsPercentMaxHeartRate ? 1 : 0;
                                    break;
                                }
                        }
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)target;

                        BuildPowerComboBox(baseTarget);
                        switch (baseTarget.ConcreteTarget.Type)
                        {
                            case BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneGTC:
                                {
                                    PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = concreteTarget.Zone;
                                    break;
                                }
                            case BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneST:
                                {
                                    PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;
                                    ZoneComboBox.SelectedIndex = Utils.FindIndexForZone(Options.Instance.PowerZoneCategory.Zones,
                                                                                        concreteTarget.Zone) + 1;
                                    break;
                                }
                            case BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range:
                                {
                                    PowerRangeTarget concreteTarget = (PowerRangeTarget)baseTarget.ConcreteTarget;

                                    ZoneComboBox.SelectedIndex = 0;
                                    LowRangeTargetText.Text = concreteTarget.MinPower.ToString();
                                    HighRangeTargetText.Text = concreteTarget.MaxPower.ToString();
                                    PowerRangeReferenceComboBox.SelectedIndex = concreteTarget.IsPercentFTP ? 1 : 0;
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        Debug.Assert(false);
                        break;
                    }
            }
        }

        private void BuildCadenceComboBox(BaseCadenceTarget target)
        {
            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.GetLocalizedString("CustomText"));

            IList<INamedLowHighZone> zones = Options.Instance.CadenceZoneCategory.Zones;
            for (byte i = 0; i < zones.Count; ++i)
            {
                ZoneComboBox.Items.Add(zones[i].Name);
            }
        }

        private void BuildHRComboBox(BaseHeartRateTarget target)
        {
            BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.GetLocalizedString("CustomText"));

            if (type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC ||
               (type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range && !Options.Instance.UseSportTracksHeartRateZones))
            {
                for (byte i = 1; i <= Constants.GarminHRZoneCount; ++i)
                {
                    ZoneComboBox.Items.Add(i.ToString());
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = target.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones;
                for (byte i = 0; i < zones.Count; ++i)
                {
                    ZoneComboBox.Items.Add(zones[i].Name);
                }
            }
        }

        private void BuildPowerComboBox(BasePowerTarget target)
        {
            BasePowerTarget.IConcretePowerTarget.PowerTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.GetLocalizedString("CustomText"));

            if (type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneGTC ||
               (type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range && !Options.Instance.UseSportTracksPowerZones))
            {
                for (byte i = 1; i <= Constants.GarminPowerZoneCount; ++i)
                {
                    ZoneComboBox.Items.Add(i.ToString());
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = Options.Instance.PowerZoneCategory.Zones;
                for (byte i = 0; i < zones.Count; ++i)
                {
                    ZoneComboBox.Items.Add(zones[i].Name);
                }
            }
        }

        private void BuildSpeedComboBox(BaseSpeedTarget target)
        {
            BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType type = target.ConcreteTarget.Type;

            ZoneComboBox.Items.Clear();
            ZoneComboBox.Items.Add(GarminFitnessView.GetLocalizedString("CustomText"));

            // Use GTC zones
            if (type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneGTC ||
               (type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range && !Options.Instance.UseSportTracksHeartRateZones))
            {
                GarminCategories garminCategory = Options.Instance.GetGarminCategory(target.ParentStep.ParentConcreteWorkout.Category);
                GarminActivityProfile currentProfile = GarminProfileManager.Instance.GetProfileForActivity(garminCategory);

                for (byte i = 0; i < Constants.GarminSpeedZoneCount; ++i)
                {
                    ZoneComboBox.Items.Add(currentProfile.GetSpeedZoneName(i));
                }
            }
            // Use ST zones
            else
            {
                IList<INamedLowHighZone> zones = target.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones;
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
                IWorkout currentWorkout = GarminWorkoutManager.Instance.Workouts[i];

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

            foreach (Workout workout in SelectedConcreteWorkouts)
            {
                WorkoutWrapper wrapper = GetWorkoutWrapper(workout, null);

                if(wrapper != null)
                {
                    selection.Add(wrapper);
                }
            }

            foreach (WorkoutPart part in SelectedWorkoutParts)
            {
                if (part.IsValid())
                {
                    WorkoutPartWrapper wrapper = GetWorkoutPartWrapper(part, null);

                    if (wrapper != null)
                    {
                        selection.Add(wrapper);
                    }
                }
            }

            for (int i = 0; i < SelectedCategories.Count; ++i)
            {
                selection.Add(GetActivityCategoryWrapper(SelectedCategories[i], null));
            }

            WorkoutsList.Selected = selection;

            // Update UI elements enabled state based on selection
            if (SelectedCategory == null)
            {
                NewWorkoutButton.Enabled = false;
            }
            else
            {
                NewWorkoutButton.Enabled = true;
            }

            RefreshWorkoutSelectionControls();
        }

        private void CleanUpWorkoutPartsSelection()
        {
            List<WorkoutPart> selectionToRemove = new List<WorkoutPart>();

            foreach (WorkoutPart part in SelectedWorkoutParts)
            {
                if (!part.IsValid())
                {
                    selectionToRemove.Add(part);
                }
            }

            foreach(WorkoutPart part in selectionToRemove)
            {
                SelectedWorkouts.Remove(part);
            }
        }

        private void RefreshStepSelection()
        {
            List<TreeList.TreeListNode> selection = new List<TreeList.TreeListNode>();

            for (int i = 0; i < SelectedSteps.Count; ++i)
            {
                IStep currentStep = SelectedSteps[i];

                selection.Add(GetStepWrapper(SelectedWorkout.ConcreteWorkout, currentStep, null));
            }

            StepsList.Selected = selection;

            if (StepsList.Selected.Count > 0)
            {
                StepsList.EnsureVisible(StepsList.Selected[0]);
            }

            // Update UI elements enabled state based on selection
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            RemoveStepButton.Enabled = SelectedSteps.Count > 0;
            StepsNotesSplitter.Enabled = SelectedStep != null;
            if (SelectedStep != null && SelectedWorkout != null &&
                Utils.GetStepInfo(SelectedStep, SelectedWorkout.Steps, out selectedList, out selectedPosition))
            {
                MoveUpButton.Enabled = selectedPosition != 0; // Not the first step
                MoveDownButton.Enabled = selectedPosition < selectedList.Count - 1; // Not the last step
            }
            else
            {
                MoveUpButton.Enabled = false;
                MoveDownButton.Enabled = false;
            }

        }

        private void RefreshWorkoutCalendar()
        {
            List<DateTime> markedDates = new List<DateTime>();

            bool hasScheduledDate = false;
            bool areAllWorkoutsScheduledOnDate = true;
            bool areAllWorkoutsSameAddToDailyView = true;
            bool addToDailyView = false;
            bool checkBoxSet = false;

            // Highlight scheduled dates
            foreach (IWorkout currentWorkout in SelectedWorkouts)
            {
                bool foundSelectedDatePlanned = false;

                foreach (GarminFitnessDate date in currentWorkout.ScheduledDates)
                {
                    if (WorkoutCalendar.SelectedDate == date)
                    {
                        foundSelectedDatePlanned = true;
                        hasScheduledDate = true;
                    }

                    markedDates.Add(date);
                }

                if (!checkBoxSet || currentWorkout.AddToDailyViewOnSchedule == addToDailyView)
                {
                    addToDailyView = currentWorkout.AddToDailyViewOnSchedule;
                    checkBoxSet = true;
                }
                else
                {
                    areAllWorkoutsSameAddToDailyView = false;
                }

                if (!foundSelectedDatePlanned)
                {
                    areAllWorkoutsScheduledOnDate = false;
                }
            }
            WorkoutCalendar.SetMarkedDates(markedDates, ZoneFiveSoftware.Common.Visuals.Calendar.MarkerStyle.RedTriangle);

            ScheduleWorkoutButton.Enabled = SelectedConcreteWorkouts.Count > 0 && WorkoutCalendar.SelectedDate >= DateTime.Today && !areAllWorkoutsScheduledOnDate;
            RemoveScheduledDateButton.Enabled = SelectedConcreteWorkouts.Count > 0 && hasScheduledDate;
            RemoveAllScheduledButton.Enabled = markedDates.Count > 0;

            AddDailyViewCheckBox.Enabled = SelectedConcreteWorkouts.Count > 0;
            if (areAllWorkoutsSameAddToDailyView)
            {
                AddDailyViewCheckBox.ThreeState = false;
                AddDailyViewCheckBox.CheckState = addToDailyView ? CheckState.Checked : CheckState.Unchecked;
            }
            else
            {
                AddDailyViewCheckBox.ThreeState = true;
                AddDailyViewCheckBox.CheckState = CheckState.Indeterminate;
            }
        }

        private void RefreshWorkoutSelectionControls()
        {
            // None or multiple selection
            if (SelectedWorkout == null)
            {
                StepSplit.Enabled = false;
                ExportDateTextLabel.Enabled = false;
                ExportDateLabel.Enabled = false;

                if (SelectedConcreteWorkouts.Count == 0)
                {
                    RemoveWorkoutButton.Enabled = false;
                    CalendarSplit.Panel2.Enabled = false;
                }
                else
                {
                    RemoveWorkoutButton.Enabled = SelectedConcreteWorkouts.Count > 0;
                    CalendarSplit.Panel2.Enabled = SelectedConcreteWorkouts.Count > 0;
                }

            }
            else
            {
                GarminFitnessDate selectedDate = new GarminFitnessDate(WorkoutCalendar.SelectedDate);

                StepSplit.Enabled = true;
                AddStepButton.Enabled = SelectedWorkout.CanAcceptNewStep(1, SelectedStep);
                AddRepeatButton.Enabled = SelectedWorkout.CanAcceptNewStep(2, SelectedStep);
                AddLinkStepButton.Enabled = AddStepButton.Enabled &&
                                            GarminWorkoutManager.Instance.Workouts.Count > 1;
                CalendarSplit.Panel2.Enabled = true;
                ExportDateTextLabel.Enabled = true;
                ExportDateLabel.Enabled = true;

                RemoveWorkoutButton.Enabled = SelectedConcreteWorkouts.Count > 0;
                CalendarSplit.Panel2.Enabled = SelectedConcreteWorkouts.Count > 0;
            }

            RefreshWorkoutCalendar();
        }

        private void RefreshClipboardControls()
        {
            switch(m_LastClipboardControlFocused)
            {
                case ClipboardControls.WorkoutsList:
                    {
                        CutButton.Enabled = SelectedConcreteWorkouts.Count > 0;
                        CopyButton.Enabled = SelectedConcreteWorkouts.Count > 0;
                        PasteButton.Enabled = Clipboard.ContainsData(Constants.WorkoutsClipboardID);
                        break;
                    }
                case ClipboardControls.StepsList:
                    {
                        CutButton.Enabled = SelectedSteps.Count > 0;
                        CopyButton.Enabled = SelectedSteps.Count > 0;
                        PasteButton.Enabled = Clipboard.ContainsData(Constants.StepsClipboardID);
                        break;
                    }
                case ClipboardControls.TextBox:
                    {
                        CutButton.Enabled = true;
                        CopyButton.Enabled = true;
                        PasteButton.Enabled = Clipboard.ContainsText();
                        break;
                    }
                default:
                    {
                        CutButton.Enabled = false;
                        CopyButton.Enabled = false;
                        PasteButton.Enabled = false;
                        break;
                    }
            }
        }

        private void AddNewStep(IStep newStep)
        {
            Debug.Assert(SelectedWorkout != null);
            bool succeeded = false;

            if (SelectedStep != null)
            {
                // Insert after selected
                succeeded = SelectedWorkout.Steps.InsertStepAfterStep(newStep, SelectedStep);
            }
            else
            {
                // Insert as 1st element
                succeeded = SelectedWorkout.Steps.InsertStepInRoot(0, newStep);
            }

            if (succeeded)
            {
                SelectedStep = newStep;
                StepsList.Focus();
            }
        }

        private StepWrapper GetStepWrapper(Workout baseWorkout, IStep step)
        {
            if (m_StepWrapperMap.ContainsKey(baseWorkout) &&
                m_StepWrapperMap[baseWorkout].ContainsKey(step))
            {
                return m_StepWrapperMap[baseWorkout][step];
            }

            return null;
        }

        private StepWrapper GetStepWrapper(Workout baseWorkout, IStep step, StepWrapper parent)
        {
            StepWrapper wrapper;

            if (!m_StepWrapperMap.ContainsKey(baseWorkout))
            {
                m_StepWrapperMap.Add(baseWorkout, new Dictionary<IStep, StepWrapper>());
            }

            // If we already have a wrapper for this workout, use it
            if (m_StepWrapperMap[baseWorkout].ContainsKey(step))
            {
                wrapper = m_StepWrapperMap[baseWorkout][step];
            }
            else
            {
                bool isWorkoutLinkChild = parent != null &&
                                          (parent.IsWorkoutLinkChild ||
                                            (parent.Element as IStep) is WorkoutLinkStep);

                // Create a new wrapper
                wrapper = new StepWrapper(parent, step, baseWorkout, isWorkoutLinkChild);

                m_StepWrapperMap[baseWorkout][step] = wrapper;
            }

            return wrapper;
        }

        private WorkoutWrapper GetWorkoutWrapper(Workout workout)
        {
            if (m_WorkoutWrapperMap.ContainsKey(workout))
            {
                return m_WorkoutWrapperMap[workout];
            }

            return null;
        }

        private WorkoutWrapper GetWorkoutWrapper(Workout workout, ActivityCategoryWrapper parent)
        {
            WorkoutWrapper wrapper = null;

            if (GarminWorkoutManager.Instance.Workouts.Contains(workout))
            {
                // If we already have a wrapper for this workout, use it
                if (m_WorkoutWrapperMap.ContainsKey(workout))
                {
                    wrapper = m_WorkoutWrapperMap[workout];
                }
                else
                {
                    // Create a new wrapper
                    wrapper = new WorkoutWrapper(parent, workout);

                    m_WorkoutWrapperMap[workout] = wrapper;
                }
            }

            return wrapper;
        }

        private WorkoutPartWrapper GetWorkoutPartWrapper(WorkoutPart workoutPart, WorkoutWrapper parent)
        {
            WorkoutPartWrapper wrapper = null;

            // If we already have a wrapper for this workout, use it
            if (m_WorkoutPartWrapperMap.ContainsKey(workoutPart))
            {
                wrapper = m_WorkoutPartWrapperMap[workoutPart];
            }
            else
            {
                // Create a new wrapper
                wrapper = new WorkoutPartWrapper(parent, workoutPart);

                m_WorkoutPartWrapperMap[workoutPart] = wrapper;
            }

            return wrapper;
        }

        private ActivityCategoryWrapper GetActivityCategoryWrapper(IActivityCategory category, ActivityCategoryWrapper parent)
        {
            ActivityCategoryWrapper wrapper;

            // If we already have a wrapper for this category, use it
            if (m_CategoryWrapperMap.ContainsKey(category))
            {
                wrapper = m_CategoryWrapperMap[category];
            }
            else
            {
                // Create a new wrapper
                wrapper = new ActivityCategoryWrapper(parent, category);

                m_CategoryWrapperMap[category] = wrapper;
            }

            return wrapper;
        }

        public void BuildWorkoutsList()
        {
            IApplication app = PluginMain.GetApplication();
            List<TreeList.TreeListNode> categories = new List<TreeList.TreeListNode>();
            bool expandList = WorkoutsList.RowData == null;

            for (int i = 0; i < app.Logbook.ActivityCategories.Count; ++i)
            {
                ActivityCategoryWrapper newNode;

                newNode = CreateCategoryNode(app.Logbook.ActivityCategories[i]);
                categories.Add(newNode);
            }

            Dictionary<WorkoutWrapper, bool> expandedMap = new Dictionary<WorkoutWrapper, bool>(); 
            foreach (Workout currentWorkout in GarminWorkoutManager.Instance.Workouts)
            {
                WorkoutWrapper oldWrapper = GetWorkoutWrapper(currentWorkout);
                bool expandWorkout = false;

                if (oldWrapper != null)
                {
                    expandWorkout = WorkoutsList.IsExpanded(oldWrapper);
                }

                WorkoutWrapper newItem = AddWorkoutToList(categories, currentWorkout);

                if (newItem != null)
                {
                    expandedMap.Add(newItem, expandWorkout);
                }
            }

            WorkoutsList.RowData = categories;

            if (expandList)
            {
                WorkoutsList.SetExpanded(WorkoutsList.RowData, true, true);
            }
            else
            {
                // This fixes the Workouts losing the expanded state when not the
                //  first wrapper in the category
                Dictionary<WorkoutWrapper, bool>.Enumerator iter = expandedMap.GetEnumerator();
                while(iter.MoveNext())
                {
                    WorkoutsList.SetExpanded(iter.Current.Key, iter.Current.Value);
                }
            }
            RefreshWorkoutSelection();
        }

        public void BuildStepsList()
        {
            if (SelectedWorkout != null)
            {
                List<TreeList.TreeListNode> stepsList = new List<TreeList.TreeListNode>();
                AddStepsToList(stepsList, SelectedWorkout.Steps, null);

                StepsList.RowData = stepsList;

                if (StepsList.Selected.Count > 0)
                {
                    StepsList.EnsureVisible(StepsList.Selected[0]);
                }
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
                        WorkoutWrapper wrapper = GetWorkoutWrapper(workout, currentCategory);

                        currentCategory.Children.Add(wrapper);
                        wrapper.Parent = currentCategory;
                        wrapper.Children.Clear();

                        // If we have multiple parts, add them to the list
                        if (workout.GetSplitPartsCount() > 1)
                        {
                            AddWorkoutPartsToList(wrapper, workout.GetSplitParts());
                        }

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

        private void AddWorkoutPartsToList(WorkoutWrapper parent, List<WorkoutPart> parts)
        {
            foreach (WorkoutPart part in parts)
            {
                WorkoutPartWrapper wrapper = GetWorkoutPartWrapper(part, parent);

                parent.Children.Add(wrapper);
                wrapper.Parent = parent;
            }
        }

        private ActivityCategoryWrapper CreateCategoryNode(IActivityCategory category)
        {
            ActivityCategoryWrapper wrapper = GetActivityCategoryWrapper(category, null);
            wrapper.Children.Clear();

            CreateCategoryNode(wrapper, null);

            return wrapper;
        }

        private void CreateCategoryNode(ActivityCategoryWrapper categoryNode, ActivityCategoryWrapper parent)
        {
            IActivityCategory category = (IActivityCategory)categoryNode.Element;

            if (parent != null)
            {
                parent.Children.Add(categoryNode);
            }
            categoryNode.Parent = parent;

            for (int i = 0; i < category.SubCategories.Count; ++i)
            {
                ActivityCategoryWrapper wrapper = GetActivityCategoryWrapper(category.SubCategories[i], categoryNode);
                wrapper.Children.Clear();

                CreateCategoryNode(wrapper, categoryNode);
            }
        }

        private void AddStepsToList(List<TreeList.TreeListNode> list, List<IStep> steps, StepWrapper parent)
        {
            foreach (IStep currentStep in steps)
            {
                StepWrapper wrapper = GetStepWrapper(SelectedWorkout.ConcreteWorkout, currentStep, parent);

                // Reset hierarchy
                wrapper.Parent = null;
                wrapper.Children.Clear();

                if (parent != null)
                {
                    parent.Children.Add(wrapper);
                }
                else
                {
                    list.Add(wrapper);
                }
                wrapper.Parent = parent;

                if (currentStep is RepeatStep)
                {
                    RepeatStep concreteStep = currentStep as RepeatStep;

                    AddStepsToList(list, concreteStep.StepsToRepeat, wrapper);
                }
                else if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep concreteStep = currentStep as WorkoutLinkStep;

                    AddStepsToList(list, concreteStep.LinkedWorkoutSteps, wrapper);
                }
            }
        }

        private void UpdateHeartRateTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new HeartRateRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.Range &&
                    selectedIndex != 0)
            {
                if (Options.Instance.UseSportTracksHeartRateZones)
                {
                    baseTarget.ConcreteTarget = new HeartRateZoneSTTarget(baseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones[selectedIndex - 1], baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new HeartRateZoneGTCTarget((Byte)(selectedIndex), baseTarget);
                }
            }
            else if (baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneST)
            {
                HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = baseTarget.ParentStep.ParentConcreteWorkout.Category.HeartRateZone.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneGTC)
            {
                HeartRateZoneGTCTarget concreteTarget = (HeartRateZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)(selectedIndex);
            }
        }

        private void UpdateSpeedTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseSpeedTarget baseTarget = (BaseSpeedTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new SpeedRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.Range &&
                    selectedIndex != 0)
            {
                if (Options.Instance.UseSportTracksSpeedZones)
                {
                    baseTarget.ConcreteTarget = new SpeedZoneSTTarget(baseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones[selectedIndex - 1], baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new SpeedZoneGTCTarget((Byte)(selectedIndex), baseTarget);
                }
            }
            else if (baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneST)
            {
                SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = baseTarget.ParentStep.ParentConcreteWorkout.Category.SpeedZone.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneGTC)
            {
                SpeedZoneGTCTarget concreteTarget = (SpeedZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)(selectedIndex);
            }
        }

        private void UpdateCadenceTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new CadenceRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.Range &&
                     selectedIndex != 0)
            {
                // ST zone
                baseTarget.ConcreteTarget = new CadenceZoneSTTarget(Options.Instance.CadenceZoneCategory.Zones[selectedIndex - 1], baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST)
            {
                CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = Options.Instance.CadenceZoneCategory.Zones[selectedIndex - 1];
            }
        }

        private void UpdatePowerTargetFromComboBox(RegularStep concreteStep, int selectedIndex)
        {
            BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;

            // We might have to change from one target type to the other
            if (baseTarget.ConcreteTarget.Type != BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range &&
                selectedIndex == 0)
            {
                // Custom range
                baseTarget.ConcreteTarget = new PowerRangeTarget(baseTarget);
            }
            else if (baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.Range &&
                     selectedIndex != 0)
            {
                // ST zone
                if (Options.Instance.UseSportTracksPowerZones)
                {
                    baseTarget.ConcreteTarget = new PowerZoneSTTarget(Options.Instance.PowerZoneCategory.Zones[selectedIndex - 1], baseTarget);
                }
                else
                {
                    baseTarget.ConcreteTarget = new PowerZoneGTCTarget((Byte)selectedIndex, baseTarget);
                }
            }
            else if (baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneST)
            {
                PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = Options.Instance.PowerZoneCategory.Zones[selectedIndex - 1];
            }
            else if (baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneGTC)
            {
                PowerZoneGTCTarget concreteTarget = (PowerZoneGTCTarget)baseTarget.ConcreteTarget;

                concreteTarget.Zone = (Byte)selectedIndex;
            }
        }

        private void SetRangeTargetControlsVisibility(bool visible)
        {
            LowRangeTargetLabel.Visible = visible;
            LowRangeTargetText.Visible = visible;
            MiddleRangeTargetLabel.Visible = visible;
            HighRangeTargetText.Visible = visible;
            RangeTargetUnitsLabel.Visible = visible;
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

        private void DeleteSelectedWorkouts()
        {
            GarminWorkoutManager.Instance.RemoveWorkouts(SelectedConcreteWorkouts);

            SelectedWorkout = null;
            SelectedSteps = null;
            UpdateUIFromWorkouts();
        }

        private void DeleteSelectedSteps()
        {
            object nextSelection = StepsList.FindNextSelectedAfterDelete(GetStepWrapper(SelectedWorkout.ConcreteWorkout, SelectedSteps[0]));

            SelectedWorkout.Steps.RemoveSteps(SelectedSteps);

            if (nextSelection != null)
            {
                SelectedStep = (IStep)((StepWrapper)nextSelection).Element;
            }
            else
            {
                SelectedStep = null;
            }
        }

        private bool IsItemSelectedInWorkoutsList(object item)
        {
            if (item.GetType() == typeof(WorkoutWrapper))
            {
                return SelectedWorkouts.Contains((Workout)((WorkoutWrapper)item).Element);
            }
            else if(item.GetType() == typeof(ActivityCategoryWrapper))
            {
                return SelectedCategories.Contains((IActivityCategory)((ActivityCategoryWrapper)item).Element);
            }

            return false;
        }

        private void CopyWorkoutSelection()
        {
            Debug.Assert(SelectedConcreteWorkouts.Count > 0);

            // Copy data to clipboard
            MemoryStream workoutsData = new MemoryStream();

            // Number of workouts to deserialize
            workoutsData.WriteByte((Byte)SelectedConcreteWorkouts.Count);
            foreach (Workout workout in SelectedConcreteWorkouts)
            {
                workout.Serialize(workoutsData);
            }

            Clipboard.SetData(Constants.WorkoutsClipboardID, workoutsData);
        }

        private void CutWorkoutSelection()
        {
            CopyWorkoutSelection();
            
            // Cut, so remove from workout
            DeleteSelectedWorkouts();
        }

        private void PasteWorkoutsFromClipboard()
        {
            Debug.Assert(Clipboard.ContainsData(Constants.WorkoutsClipboardID));

            MemoryStream pasteResult = (MemoryStream)Clipboard.GetData(Constants.WorkoutsClipboardID);

            if (SelectedCategory == null)
            {
                SelectedCategory = Utils.GetDefaultCategory();
            }

            if (pasteResult != null)
            {
                // Set back to start
                pasteResult.Seek(0, SeekOrigin.Begin);

                SelectedWorkouts = GarminWorkoutManager.Instance.DeserializeWorkouts(pasteResult, SelectedCategory);
            }

            WorkoutsList.Focus();
        }

        private void CopyStepSelection()
        {
            Debug.Assert(SelectedSteps.Count > 0);

            // Copy data to clipboard
            MemoryStream stepsData = new MemoryStream();
            List<IStep> baseSteps;

            baseSteps = GetMinimalStepsBase(SelectedSteps);
            // Number of steps to deserialize
            stepsData.WriteByte((Byte)baseSteps.Count);
            foreach (IStep step in baseSteps)
            {
                step.Serialize(stepsData);
            }

            Clipboard.SetData(Constants.StepsClipboardID, stepsData);

            stepsData.Close();
        }

        private void CutStepSelection()
        {
            CopyStepSelection();

            DeleteSelectedSteps();
        }

        private void PasteStepsFromClipboard()
        {
            Debug.Assert(Clipboard.ContainsData(Constants.StepsClipboardID));

            MemoryStream pasteResult = (MemoryStream)Clipboard.GetData(Constants.StepsClipboardID);

            // Set back to start
            pasteResult.Seek(0, SeekOrigin.Begin);

            List<IStep> newSteps = SelectedWorkout.DeserializeSteps(pasteResult);

            if (newSteps != null)
            {
                SelectedSteps = newSteps;
            }
            else
            {
                MessageBox.Show(GarminFitnessView.GetLocalizedString("PasteStepErrorText"),
                                GarminFitnessView.GetLocalizedString("ErrorText"),
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            pasteResult.Close();

            StepsList.Focus();
        }

        private void RegisterChildrenControlsForFocus(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.GotFocus += new EventHandler(ChildControl_GotFocus);

                if (control.Controls.Count > 0)
                {
                    RegisterChildrenControlsForFocus(control);
                }
            }
        }

        private List<IStep> GetMinimalStepsBase(List<IStep> steps)
        {
            List<IStep> result = new List<IStep>();
            List<RepeatStep> baseRepeatSteps = new List<RepeatStep>();
            List<RepeatStep> repeatSteps = new List<RepeatStep>();

            // 1st pass, add all the base repeat steps to the result list
            foreach (IStep currentStep in steps)
            {
                if (currentStep is RepeatStep)
                {
                    repeatSteps.Add(currentStep as RepeatStep);
                }
            }

            foreach (RepeatStep currentRepeat in repeatSteps)
            {
                bool isChild = false;

                // We must check if this repeat is a base, or a child of another repeat
                foreach (RepeatStep currentRepeat2 in repeatSteps)
                {
                    if (currentRepeat != currentRepeat2 &&
                        currentRepeat2.IsChildStep(currentRepeat))
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
            foreach (IStep currentStep in steps)
            {
                if (!GetStepWrapper(SelectedWorkout.ConcreteWorkout, currentStep).IsWorkoutLinkChild)
                {
                    if (currentStep is RepeatStep &&
                             baseRepeatSteps.Contains(currentStep as RepeatStep))
                    {
                        // Add repeats in the right order
                        result.Add(currentStep);
                    }
                    else
                    {
                        // We cannot move steps that are inside another workout (workout link child step)
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
                }
            }

            return result;
        }

        private int StepComparison(IStep x, IStep y)
        {
            int xId, yId;

            xId = x.ParentConcreteWorkout.GetStepExportId(x);
            yId = x.ParentConcreteWorkout.GetStepExportId(y);

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

        private IWorkout SelectedWorkout
        {
            get
            {
                if (SelectedWorkouts.Count == 1)
                {
                    return SelectedWorkouts[0];
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    if (SelectedWorkouts.Count != 1 || SelectedWorkouts[0] != value)
                    {
                        List<IWorkout> selection = new List<IWorkout>();

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

        public List<Workout> SelectedConcreteWorkouts
        {
            get
            {
                List<Workout> result = new List<Workout>();

                foreach(IWorkout workout in SelectedWorkouts)
                {
                    if (workout is Workout)
                    {
                        result.Add(workout as Workout);
                    }
                }

                return result;
            }
        }

        public List<WorkoutPart> SelectedWorkoutParts
        {
            get
            {
                List<WorkoutPart> result = new List<WorkoutPart>();

                foreach(IWorkout workout in SelectedWorkouts)
                {
                    if (workout is WorkoutPart)
                    {
                        result.Add(workout as WorkoutPart);
                    }
                }

                return result;
            }
        }

        public List<IWorkout> SelectedWorkouts
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
                if (SelectedSteps.Count == 1)
                {
                    return SelectedSteps[0];
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
                    m_SelectedSteps.Sort(StepComparison);
                    RefreshStepSelection();
                }
            }
        }

        public IActivityCategory SelectedCategory
        {
            get
            {
                if (m_SelectedCategories.Count <= 1)
                {
                    if (m_SelectedCategories.Count == 1 && SelectedWorkouts.Count == 0)
                    {
                        return m_SelectedCategories[0];
                    }
                    else
                    {
                        // If we have multiple workouts selected, find the common category
                        IActivityCategory selection = null;

                        // The workouts category must be the same as the selected one
                        if (m_SelectedCategories.Count == 1)
                        {
                            selection = m_SelectedCategories[0];
                        }

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
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    List<IActivityCategory> newSelection = new List<IActivityCategory>();

                    newSelection.Add(value);

                    SelectedCategories = newSelection;
                }
                else
                {
                    SelectedCategories = null;
                }
            }
        }

        public List<IActivityCategory> SelectedCategories
        {
            get { return m_SelectedCategories; }
            private set
            {
                if (m_SelectedCategories != value)
                {
                    if (value != null)
                    {
                        m_SelectedCategories = value;
                    }
                    else
                    {
                        m_SelectedCategories.Clear();
                    }

                    RefreshWorkoutSelection();
                }
            }
        }

        private enum RangeValidationInputType
        {
            Integer,
            Double,
            Time
        }

        private enum ClipboardControls
        {
            WorkoutsList,
            StepsList,
            TextBox,
            Invalid
        }

        private System.Windows.Forms.Panel m_CurrentDurationPanel;
        private readonly System.Windows.Forms.Panel[] m_DurationPanels;
        private const int CTRL_KEY_CODE = 8;

        private List<IWorkout> m_SelectedWorkouts = new List<IWorkout>();
        private List<IStep> m_SelectedSteps = new List<IStep>();
        private List<IActivityCategory> m_SelectedCategories = new List<IActivityCategory>();

        private Dictionary<Workout, WorkoutWrapper> m_WorkoutWrapperMap = new Dictionary<Workout, WorkoutWrapper>();
        private Dictionary<WorkoutPart, WorkoutPartWrapper> m_WorkoutPartWrapperMap = new Dictionary<WorkoutPart, WorkoutPartWrapper>();
        private Dictionary<IActivityCategory, ActivityCategoryWrapper> m_CategoryWrapperMap = new Dictionary<IActivityCategory, ActivityCategoryWrapper>();
        private Dictionary<Workout, Dictionary<IStep, StepWrapper>> m_StepWrapperMap = new Dictionary<Workout, Dictionary<IStep, StepWrapper>>();
        private ClipboardControls m_LastClipboardControlFocused = ClipboardControls.Invalid;
        private ExtendedTextBox m_LastFocusTextBox;
    }
}

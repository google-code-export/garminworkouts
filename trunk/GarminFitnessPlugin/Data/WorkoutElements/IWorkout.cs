using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    abstract class IWorkout : IPluginSerializable, IXMLSerializable
    {
#region IXMLSerializable Members
        public void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            if (GetSplitPartsCount() > 1)
            {
                List<WorkoutPart> splitParts = ConcreteWorkout.SplitInSeperateParts();

                for (int j = 0; j < splitParts.Count; ++j)
                {
                    splitParts[j].Serialize(parentNode, nodeName, document);
                }
            }
            else
            {
                XmlNode childNode;
                XmlNode workoutNode = document.CreateElement(nodeName);
                XmlAttribute attribute;

                parentNode.AppendChild(workoutNode);

                // Sport attribute
                attribute = document.CreateAttribute(null, "Sport", null);
                attribute.Value = Constants.GarminCategoryTCXString[(int)Options.Instance.GetGarminCategory(Category)];
                workoutNode.Attributes.Append(attribute);

                // Name
                NameInternal.Serialize(workoutNode, "Name", document);

                // Export all steps
                for (int i = 0; i < Steps.Count; ++i)
                {
                    childNode = document.CreateElement("Step");

                    Steps[i].Serialize(childNode, "Step", document);
                    workoutNode.AppendChild(childNode);
                }

                // Scheduled dates
                for (int i = 0; i < ScheduledDates.Count; ++i)
                {
                    ScheduledDates[i].Serialize(workoutNode, "ScheduledOn", document);
                }

                // Notes
                if (Notes != String.Empty && Notes != null)
                {
                    NotesInternal.Serialize(workoutNode, "Notes", document);
                }

                // Extensions
                childNode = document.CreateElement(Constants.ExtensionsTCXString);
                workoutNode.AppendChild(childNode);

                // Steps extensions
                if (m_StepsExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("Steps");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.garmin.com/xmlschemas/WorkoutExtension/v1";
                    extensionsNode.Attributes.Append(attribute);

                    for (int i = 0; i < m_StepsExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = m_StepsExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    m_StepsExtensions.Clear();
                }

                // ST extension
                if (m_STExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("SportTracksExtensions");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.zonefivesoftware.com/SportTracks/Plugins/plugin_detail.php?id=97";
                    extensionsNode.Attributes.Append(attribute);

                    // Category
                    XmlNode categoryNode = document.CreateElement("SportTracksCategory");
                    categoryNode.AppendChild(document.CreateTextNode(Category.ReferenceId));
                    extensionsNode.AppendChild(categoryNode);

                    for (int i = 0; i < m_STExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = m_STExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    m_STExtensions.Clear();
                }
            }
        }

        public abstract void Deserialize(XmlNode parentNode);
#endregion

        public void AddSportTracksExtension(XmlNode extensionNode)
        {
            m_STExtensions.Add(extensionNode);
        }

        public void AddStepExtension(XmlNode extensionNode)
        {
            m_StepsExtensions.Add(extensionNode);
        }

        void OnStepChanged(IStep modifiedStep, PropertyChangedEventArgs changedProperty)
        {
            if (StepChanged != null)
            {
                StepChanged(this, modifiedStep, changedProperty);
            }
        }

        void OnDurationChanged(IStep modifiedStep, IDuration durationChanged, PropertyChangedEventArgs changedProperty)
        {
            Debug.Assert(modifiedStep.Type == IStep.StepType.Regular);

            if (StepDurationChanged != null)
            {
                StepDurationChanged(this, (RegularStep)modifiedStep, durationChanged, changedProperty);
            }
        }

        void OnTargetChanged(IStep modifiedStep, ITarget targetChanged, PropertyChangedEventArgs changedProperty)
        {
            Debug.Assert(modifiedStep.Type == IStep.StepType.Regular);

            if (StepTargetChanged != null)
            {
                StepTargetChanged(this, (RegularStep)modifiedStep, targetChanged, changedProperty);
            }
        }

        public bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory)
        {
            bool valueChanged = false;

            // Validate all steps
            foreach (IStep step in Steps)
            {
                if (step.ValidateAfterZoneCategoryChanged(changedCategory))
                {
                    valueChanged = true;
                }
            }

            return valueChanged;
        }

        public void MarkAllCadenceSTZoneTargetsAsDirty()
        {
            foreach (IStep step in Steps)
            {
                if (step.Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)step;

                    if (concreteStep.Target.Type == ITarget.TargetType.Cadence)
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)concreteStep.Target;

                        if (baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST)
                        {
                            CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;

                            concreteTarget.Zone = Options.Instance.CadenceZoneCategory.Zones[0];
                            concreteTarget.IsDirty = true;
                        }
                    }
                }
                else if (step.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)step;

                    concreteStep.MarkAllCadenceSTZoneTargetsAsDirty();
                }
            }
        }

        public void MarkAllPowerSTZoneTargetsAsDirty()
        {
            foreach (IStep step in Steps)
            {
                if (step.Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)step;

                    if (concreteStep.Target.Type == ITarget.TargetType.Power)
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)concreteStep.Target;

                        if (baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneST)
                        {
                            PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;

                            concreteTarget.Zone = Options.Instance.PowerZoneCategory.Zones[0];
                            concreteTarget.IsDirty = true;
                        }
                    }
                }
                else if (step.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)step;

                    concreteStep.MarkAllPowerSTZoneTargetsAsDirty();
                }
            }
        }

        public virtual int GetStepCount()
        {
            int result = 0;

            foreach (IStep step in Steps)
            {
                result += step.GetStepCount();
            }

            return result;
        }

        public virtual UInt16 GetSplitPartsCount()
        {
            return GetStepSplitPart(Steps[Steps.Count - 1]);
        }

        public UInt16 GetStepSplitPart(IStep step)
        {
            UInt16 partNumber = 1;
            UInt16 counter = 0;
            IStep topMostRepeat = GetTopMostRepeatForStep(step);
            IStep stepToFind = step;

            if (topMostRepeat != null)
            {
                stepToFind = topMostRepeat;
            }

            for (int i = 0; i < Steps.Count; ++i)
            {
                IStep currentStep = Steps[i];

                counter += currentStep.GetStepCount();

                if (i != 0 && (currentStep.ForceSplitOnStep || counter > Constants.MaxStepsPerWorkout))
                {
                    partNumber++;
                    counter = currentStep.GetStepCount();
                }

                if (currentStep == stepToFind)
                {
                    break;
                }
            }

            return partNumber;
        }

        public List<IStep> GetStepsForPart(int partNumber)
        {
            List<IStep> result = new List<IStep>();
            UInt16 currentPartNumber = 0;
            UInt16 counter = 0;

            for (int i = 0; i < Steps.Count; ++i)
            {
                IStep currentStep = Steps[i];

                counter += currentStep.GetStepCount();

                if (i != 0 && (currentStep.ForceSplitOnStep || counter > Constants.MaxStepsPerWorkout))
                {
                    currentPartNumber++;
                    counter = currentStep.GetStepCount();
                }

                if (currentPartNumber == partNumber)
                {
                    // Add step to result, it's in the right part
                    result.Add(currentStep);
                }
                else if (currentPartNumber > partNumber)
                {
                    break;
                }
            }

            return result;
        }

        public RepeatStep GetTopMostRepeatForStep(IStep step)
        {
            if (step != null)
            {
                for (int i = 0; i < Steps.Count; ++i)
                {
                    if (Steps[i].Type == IStep.StepType.Repeat)
                    {
                        if (((RepeatStep)Steps[i]).IsChildStep(step))
                        {
                            return (RepeatStep)Steps[i];
                        }
                    }
                }
            }

            return null;
        }

        public IStep GetNextStep(IStep previousStep)
        {
            UInt16 index;
            List<IStep> parentList;

            if (Utils.GetStepInfo(previousStep, Steps, out parentList, out index))
            {
                if (index != parentList.Count - 1)
                {
                    index++;
                }

                return parentList[index];
            }

            return null;
        }

        public IStep GetPreviousStep(IStep nextStep)
        {
            UInt16 index;
            List<IStep> parentList;

            if (Utils.GetStepInfo(nextStep, Steps, out parentList, out index))
            {
                if (index != 0)
                {
                    return parentList[index - 1];
                }
            }

            return null;
        }

        public void MoveStepUp(IStep step)
        {
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            if (Utils.GetStepInfo(step, Steps, out selectedList, out selectedPosition))
            {
                Debug.Assert(selectedPosition > 0);

                selectedList.Reverse(selectedPosition - 1, 2);

                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
            }
        }

        public void MoveStepDown(IStep step)
        {
            UInt16 selectedPosition = 0;
            List<IStep> selectedList = null;

            if (Utils.GetStepInfo(step, Steps, out selectedList, out selectedPosition))
            {
                Debug.Assert(selectedPosition < selectedList.Count - 1);

                selectedList.Reverse(selectedPosition, 2);

                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
            }
        }

        public bool CanAcceptNewStep(IStep newStep, IStep destinationStep)
        {
            return CanAcceptNewStep(newStep.GetStepCount(), destinationStep);
        }

        public bool AddStepToRoot(IStep stepToAdd)
        {
            List<IStep> newStep = new List<IStep>();

            newStep.Add(stepToAdd);

            return AddStepsToRoot(newStep);
        }

        public bool AddStepsToRoot(List<IStep> stepsToAdd)
        {
            if (Steps.Count > 0)
            {
                return InsertStepsAfterStep(stepsToAdd, Steps[Steps.Count - 1]);
            }
            else
            {
                return InsertStepsAfterStep(stepsToAdd, null);
            }
        }

        public bool InsertStepInRoot(int index, IStep stepToAdd)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStep(stepsToAdd, Steps[index]);
        }

        public bool InsertStepsInRoot(List<IStep> stepsToAdd, int index)
        {
            return InsertStepsBeforeStep(stepsToAdd, Steps[index]);
        }

        public bool InsertStepAfterStep(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsAfterStep(stepsToAdd, previousStep);
        }

        private bool InsertStepAfterStepInternal(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsAfterStepInternal(stepsToAdd, previousStep);
        }

        public bool InsertStepsAfterStep(List<IStep> stepsToAdd, IStep previousStep)
        {
            int insertStepsCount = 0;

            // We need to count the number of items being moved
            for (int i = 0; i < stepsToAdd.Count; ++i)
            {
                insertStepsCount += stepsToAdd[i].GetStepCount();
            }

            if (CanAcceptNewStep(insertStepsCount, previousStep))
            {
                return InsertStepsAfterStepInternal(stepsToAdd, previousStep);
            }

            return false;
        }

        private bool InsertStepsAfterStepInternal(List<IStep> stepsToAdd, IStep previousStep)
        {
            UInt16 previousPosition = 0;
            List<IStep> parentList = null;

            if (Utils.GetStepInfo(previousStep, Steps, out parentList, out previousPosition))
            {
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    IStep currentStep = stepsToAdd[i];

                    // Make sure we don't duplicate the step in the list
                    Debug.Assert(!Utils.GetStepInfo(currentStep, Steps, out tempList, out tempPosition));

                    RegisterStep(currentStep);

                    parentList.Insert(++previousPosition, currentStep);
                    stepAdded = true;
                }

                if (stepAdded)
                {
                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
                }

                return true;
            }
            else
            {
                // We haven't found the step, insert at root
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    IStep currentStep = stepsToAdd[i];

                    // Make sure we don't duplicate the step in the list
                    Debug.Assert(!Utils.GetStepInfo(currentStep, Steps, out tempList, out tempPosition));

                    RegisterStep(currentStep);

                    Steps.Add(currentStep);
                    stepAdded = true;
                }

                if (stepAdded)
                {
                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
                }
            }

            return false;
        }

        public bool InsertStepBeforeStep(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStep(stepsToAdd, previousStep);
        }

        private bool InsertStepBeforeStepInternal(IStep stepToAdd, IStep previousStep)
        {
            List<IStep> stepsToAdd = new List<IStep>();

            stepsToAdd.Add(stepToAdd);

            return InsertStepsBeforeStepInternal(stepsToAdd, previousStep);
        }

        public bool InsertStepsBeforeStep(List<IStep> stepsToAdd, IStep nextStep)
        {
            Debug.Assert(nextStep != null);

            int insertStepsCount = 0;

            // We need to count the number of items being moved
            for (int i = 0; i < stepsToAdd.Count; ++i)
            {
                insertStepsCount += stepsToAdd[i].GetStepCount();
            }

            if (CanAcceptNewStep(insertStepsCount, nextStep))
            {
                return InsertStepsBeforeStepInternal(stepsToAdd, nextStep);
            }

            return false;
        }

        private bool InsertStepsBeforeStepInternal(List<IStep> stepsToAdd, IStep nextStep)
        {
            UInt16 previousPosition = 0;
            List<IStep> parentList = null;

            if (Utils.GetStepInfo(nextStep, Steps, out parentList, out previousPosition))
            {
                List<IStep> tempList;
                UInt16 tempPosition = 0;
                bool stepAdded = false;

                for (int i = 0; i < stepsToAdd.Count; ++i)
                {
                    // Make sure we don't duplicate the step in the list
                    Debug.Assert(!Utils.GetStepInfo(stepsToAdd[i], Steps, out tempList, out tempPosition));

                    RegisterStep(stepsToAdd[i]);

                    parentList.Insert(previousPosition++, stepsToAdd[i]);
                    stepAdded = true;
                }

                if (stepAdded)
                {
                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
                }

                return true;
            }
            else
            {
                // We haven't found the right step, this shouldn't happen
                Debug.Assert(false);
            }

            return false;
        }

        public void MoveStepsAfterStep(List<IStep> stepsToMove, IStep previousStep)
        {
            if (stepsToMove != null && stepsToMove.Count > 0)
            {
                m_EventsActive = false;
                {
                    // Mark the destination
                    IStep positionMarker = new RegularStep(ConcreteWorkout);

                    // Mark position
                    InsertStepAfterStepInternal(positionMarker, previousStep);

                    // Remove items from their original position
                    RemoveSteps(stepsToMove);

                    // Add at new position
                    InsertStepsAfterStepInternal(stepsToMove, positionMarker);

                    // Remove our position marker
                    RemoveStep(positionMarker);

                    m_EventsActive = true;
                }

                // Trigger event
                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
            }
        }

        public void MoveStepsBeforeStep(List<IStep> stepsToMove, IStep nextStep)
        {
            if (stepsToMove != null && stepsToMove.Count > 0)
            {
                m_EventsActive = false;
                {
                    // Make a copy of the destination
                    IStep positionMarker = nextStep.Clone();

                    // Mark position
                    InsertStepBeforeStepInternal(positionMarker, nextStep);

                    // Remove items from their original position
                    RemoveSteps(stepsToMove);

                    // Add at new position
                    InsertStepsAfterStepInternal(stepsToMove, positionMarker);

                    // Remove our position marker
                    RemoveStep(positionMarker);

                    m_EventsActive = true;
                }

                // Trigger event
                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
            }
        }

        public void RemoveStep(IStep stepToRemove)
        {
            List<IStep> stepsToRemove = new List<IStep>(); ;

            stepsToRemove.Add(stepToRemove);

            RemoveSteps(stepsToRemove);
        }

        public void RemoveSteps(List<IStep> stepsToRemove)
        {
            for (int i = 0; i < stepsToRemove.Count; ++i)
            {
                UnregisterStep(stepsToRemove[i]);
            }

            CleanUpAfterDelete();

            TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Steps"));
        }

        private void CleanUpAfterDelete()
        {
            if (Steps.Count > 0)
            {
                // Go through repeat steps and delete the ones which have 0 substeps
                for (int i = 0; i < Steps.Count; ++i)
                {
                    IStep currentStep = Steps[i];

                    if (currentStep.Type == IStep.StepType.Repeat)
                    {
                        RepeatStep concreteStep = (RepeatStep)currentStep;

                        if (concreteStep.StepsToRepeat.Count > 0)
                        {
                            CleanUpAfterDelete(concreteStep);
                        }

                        if (concreteStep.StepsToRepeat.Count == 0)
                        {
                            UnregisterStep(currentStep);
                            i--;
                        }
                    }
                }
            }

            if (Steps.Count == 0)
            {
                // Cannot have an empty workout, recreate a base step
                AddStepToRoot(new RegularStep(ConcreteWorkout));
            }
        }

        private void CleanUpAfterDelete(RepeatStep step)
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
                        UnregisterStep(currentStep);
                        i--;
                    }
                    else
                    {
                        CleanUpAfterDelete(concreteStep);
                    }
                }
            }
        }

        private void RegisterStep(IStep stepToRegister)
        {
            stepToRegister.StepChanged += new IStep.StepChangedEventHandler(OnStepChanged);
            stepToRegister.DurationChanged += new IStep.StepDurationChangedEventHandler(OnDurationChanged);
            stepToRegister.TargetChanged += new IStep.StepTargetChangedEventHandler(OnTargetChanged);

            stepToRegister.ParentWorkout = ConcreteWorkout;

            if (stepToRegister.Type == IStep.StepType.Repeat)
            {
                RegisterRepeatStep((RepeatStep)stepToRegister);
            }
        }

        private void RegisterRepeatStep(RepeatStep stepToRegister)
        {
            foreach (IStep step in stepToRegister.StepsToRepeat)
            {
                RegisterStep(step);
            }
        }

        private void UnregisterStep(IStep stepToUnregister)
        {
            List<IStep> selectedList = null;
            UInt16 selectedPosition = 0;

            if (Utils.GetStepInfo(stepToUnregister, Steps, out selectedList, out selectedPosition))
            {
                stepToUnregister.StepChanged -= new IStep.StepChangedEventHandler(OnStepChanged);
                stepToUnregister.DurationChanged -= new IStep.StepDurationChangedEventHandler(OnDurationChanged);
                stepToUnregister.TargetChanged -= new IStep.StepTargetChangedEventHandler(OnTargetChanged);

                selectedList.RemoveAt(selectedPosition);
            }
        }

        public IStep GetStepById(int id)
        {
            return GetStepByIdInternal(Steps, id, 0);
        }

        private static IStep GetStepByIdInternal(IList<IStep> steps, int id, int baseId)
        {
            int currentId = baseId;

            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];

                if (currentId + currentStep.GetStepCount() == id)
                {
                    return currentStep;
                }
                else if (currentStep.Type == IStep.StepType.Repeat && id <= currentId + currentStep.GetStepCount())
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;
                    return GetStepByIdInternal(concreteStep.StepsToRepeat, id, currentId);
                }

                currentId += currentStep.GetStepCount();
            }

            return null;
        }

        public void ScheduleWorkout(DateTime date)
        {
            GarminFitnessDate temp = new GarminFitnessDate(date);

            if (!ScheduledDates.Contains(temp) && date.Ticks >= DateTime.Today.Ticks)
            {
                ScheduledDates.Add(temp);

                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Schedule"));
            }
        }

        public void RemoveScheduledDate(DateTime date)
        {
            GarminFitnessDate temp = new GarminFitnessDate(date);

            if (ScheduledDates.Contains(temp))
            {
                ScheduledDates.Remove(temp);

                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Schedule"));
            }
        }

        public List<WorkoutPart> SplitInSeperateParts()
        {
            UInt16 partsCount = GetSplitPartsCount();
            List<WorkoutPart> result = new List<WorkoutPart>(partsCount);

            if (partsCount > 1)
            {
                for (int i = 0; i < partsCount; ++i)
                {
                    result.Add(GarminWorkoutManager.Instance.CreateWorkoutPart(ConcreteWorkout, i));
                }
            }

            return result;
        }

        protected void TriggerWorkoutChangedEvent(PropertyChangedEventArgs args)
        {
            if (m_EventsActive && WorkoutChanged != null)
            {
                WorkoutChanged(this, args);
            }
        }

        public string Name
        {
            get { return NameInternal; }
            set
            {
                Debug.Assert(!(this is WorkoutPart));

                if (value != Name)
                {
                    NameInternal.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public string Notes
        {
            get { return NotesInternal; }
            set
            {
                Debug.Assert(!(this is WorkoutPart));

                if (value != Notes)
                {
                    NotesInternal.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Notes"));
                }
            }
        }

        // Abstract mthods
        public abstract bool CanAcceptNewStep(int newStepCount, IStep destinationStep);
        public abstract Workout ConcreteWorkout { get; }
        public abstract GarminFitnessString NameInternal { get; }
        public abstract GarminFitnessString NotesInternal { get; }
        public abstract DateTime LastExportDate { get; set; }
        public abstract List<GarminFitnessDate> ScheduledDates { get; }
        public abstract IActivityCategory Category { get; set; }
        public abstract List<IStep> Steps { get; }
        public abstract bool AddToDailyViewOnSchedule { get; set; }

        // Members
        public delegate void WorkoutChangedEventHandler(IWorkout modifiedWorkout, PropertyChangedEventArgs changedProperty);
        public event WorkoutChangedEventHandler WorkoutChanged;

        public delegate void StepChangedEventHandler(IWorkout modifiedWorkout, IStep modifiedStep, PropertyChangedEventArgs changedProperty);
        public event StepChangedEventHandler StepChanged;

        public delegate void StepDurationChangedEventHandler(IWorkout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event StepDurationChangedEventHandler StepDurationChanged;

        public delegate void StepTargetChangedEventHandler(IWorkout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty);
        public event StepTargetChangedEventHandler StepTargetChanged;

        private bool m_EventsActive = true;

        private List<XmlNode> m_STExtensions = new List<XmlNode>();
        private List<XmlNode> m_StepsExtensions = new List<XmlNode>();
    }
}

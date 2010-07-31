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

                foreach (WorkoutPart part in splitParts)
                {
                    part.Serialize(parentNode, nodeName, document);
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
                Steps.Serialize(workoutNode, "", document);

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

        public void Serialize(GarXFaceNet._WorkoutList workoutList)
        {
            if (GetSplitPartsCount() > 1)
            {
                List<WorkoutPart> splitParts = ConcreteWorkout.SplitInSeperateParts();

                foreach (WorkoutPart part in splitParts)
                {
                    part.Serialize(workoutList);
                }
            }
            else
            {
                GarXFaceNet._Workout workout = new GarXFaceNet._Workout();

                workout.SetName(Name);
                workout.SetNumValidSteps((UInt32)Steps.Count);
                workout.SetSportType((GarXFaceNet._Workout.SportTypes)Options.Instance.GetGarminCategory(Category));

                Steps.Serialize(workout, 0);

                workoutList.Add(workout);
            }
        }

        public void SerializeOccurances(GarXFaceNet._WorkoutOccuranceList occuranceList)
        {
            foreach (GarminFitnessDate scheduledDate in ScheduledDates)
            {
                GarXFaceNet._WorkoutOccurance newOccurance = new GarXFaceNet._WorkoutOccurance();
                TimeSpan timeSinceReference = (DateTime)scheduledDate - new DateTime(1989, 12, 31);

                newOccurance.SetWorkoutName(Name);
                newOccurance.SetDay((UInt32)(timeSinceReference.TotalSeconds));

                occuranceList.Add(newOccurance);
            }
        }

        public abstract void Deserialize(GarXFaceNet._Workout workout);
        public abstract void DeserializeOccurances(GarXFaceNet._WorkoutOccuranceList occuranceList);

        public List<IStep> DeserializeSteps(Stream stream)
        {
            List<IStep> deserializedSteps = new List<IStep>();
            byte[] intBuffer = new byte[sizeof(Int32)];
            Byte stepCount = (Byte)stream.ReadByte();

            for (int i = 0; i < stepCount; i++)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                if (type == IStep.StepType.Regular)
                {
                    deserializedSteps.Add(new RegularStep(stream, Constants.CurrentVersion, ConcreteWorkout));
                }
                else
                {
                    deserializedSteps.Add(new RepeatStep(stream, Constants.CurrentVersion, ConcreteWorkout));
                }
            }

            // Now that we deserialized, paste in the current workout
            if (Steps.AddStepsToRoot(deserializedSteps))
            {
                return deserializedSteps;
            }

            return null;
        }

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
            if (changedProperty.PropertyName == "ForceSplitOnStep")
            {
                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("PartsCount"));
            }

            if (ConcreteWorkout.StepChanged != null)
            {
                ConcreteWorkout.StepChanged(this, modifiedStep, changedProperty);
            }
        }

        void OnDurationChanged(IStep modifiedStep, IDuration durationChanged, PropertyChangedEventArgs changedProperty)
        {
            Debug.Assert(modifiedStep.Type == IStep.StepType.Regular);

            if (ConcreteWorkout.StepDurationChanged != null)
            {
                ConcreteWorkout.StepDurationChanged(this, (RegularStep)modifiedStep, durationChanged, changedProperty);
            }
        }

        void OnTargetChanged(IStep modifiedStep, ITarget targetChanged, PropertyChangedEventArgs changedProperty)
        {
            Debug.Assert(modifiedStep.Type == IStep.StepType.Regular);

            if (ConcreteWorkout.StepTargetChanged != null)
            {
                ConcreteWorkout.StepTargetChanged(this, (RegularStep)modifiedStep, targetChanged, changedProperty);
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
            if (Steps.Count > 0)
            {
                return (UInt16)(GetStepSplitPart(Steps[Steps.Count - 1]) + 1);
            }

            return 0;
        }

        public UInt16 GetStepSplitPart(IStep step)
        {
            UInt16 partNumber = 0;
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

        public WorkoutStepsList GetStepsForPart(int partNumber)
        {
            WorkoutStepsList result = new WorkoutStepsList(this);
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

        protected void RegisterStep(IStep stepToRegister)
        {
            stepToRegister.StepChanged += new IStep.StepChangedEventHandler(OnStepChanged);
            stepToRegister.DurationChanged += new IStep.StepDurationChangedEventHandler(OnDurationChanged);
            stepToRegister.TargetChanged += new IStep.StepTargetChangedEventHandler(OnTargetChanged);

            stepToRegister.ParentConcreteWorkout = ConcreteWorkout;

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

        protected void UnregisterStep(IStep stepToUnregister)
        {
            stepToUnregister.StepChanged -= new IStep.StepChangedEventHandler(OnStepChanged);
            stepToUnregister.DurationChanged -= new IStep.StepDurationChangedEventHandler(OnDurationChanged);
            stepToUnregister.TargetChanged -= new IStep.StepTargetChangedEventHandler(OnTargetChanged);
        }
        
        public int GetStepExportId(IStep step)
        {
            int result = GetStepExportIdInternal(step.ParentWorkout.Steps.List, step);

            Debug.Assert(result != -1);

            return result;
        }

        private int GetStepExportIdInternal(IList<IStep> steps, IStep step)
        {
            int currentId = 0;

            for (int i = 0; i < steps.Count; ++i)
            {
                IStep currentStep = steps[i];

                if (currentStep == step)
                {
                    return currentId + currentStep.GetStepCount();
                }
                else if (currentStep.Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)currentStep;
                    int temp = GetStepExportIdInternal(concreteStep.StepsToRepeat, step);

                    if (temp != -1)
                    {
                        return currentId + temp;
                    }
                }

                currentId += currentStep.GetStepCount();
            }

            return -1;
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

        public void RemoveAllScheduledDates()
        {
            if (ScheduledDates.Count > 0)
            {
                ScheduledDates.Clear();

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
            if (m_EventsActive && ConcreteWorkout.WorkoutChanged != null)
            {
                ConcreteWorkout.WorkoutChanged(this, args);
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

        public Guid Id
        {
            get { return IdInternal; }
            set
            {
                Debug.Assert(!(this is WorkoutPart));

                if (!value.Equals(Id))
                {
                    IdInternal.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Id"));
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
        public abstract GarminFitnessGuid IdInternal { get; }
        public abstract GarminFitnessString NotesInternal { get; }
        public abstract DateTime LastExportDate { get; set; }
        public abstract List<GarminFitnessDate> ScheduledDates { get; }
        public abstract IActivityCategory Category { get; set; }
        public abstract WorkoutStepsList Steps { get; }
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

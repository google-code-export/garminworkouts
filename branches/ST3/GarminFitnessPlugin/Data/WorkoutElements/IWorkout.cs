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

                StepsExtensions.Clear();
                STExtensions.Clear();

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
                if (StepsExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("Steps");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.garmin.com/xmlschemas/WorkoutExtension/v1";
                    extensionsNode.Attributes.Append(attribute);

                    for (int i = 0; i < StepsExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = StepsExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    StepsExtensions.Clear();
                }

                // ST extension
                if (STExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("SportTracksExtensions");
                    attribute = document.CreateAttribute("xmlns");
                    attribute.Value = "http://www.zonefivesoftware.com/SportTracks/Plugins/plugin_detail.php?id=97";
                    extensionsNode.Attributes.Append(attribute);

                    // Category
                    XmlNode categoryNode = document.CreateElement("SportTracksCategory");
                    categoryNode.AppendChild(document.CreateTextNode(Category.ReferenceId));
                    extensionsNode.AppendChild(categoryNode);

                    for (int i = 0; i < STExtensions.Count; ++i)
                    {
                        XmlNode currentExtension = STExtensions[i];

                        extensionsNode.AppendChild(currentExtension);
                    }
                    childNode.AppendChild(extensionsNode);

                    STExtensions.Clear();
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

        public virtual void SerializetoFIT(Stream stream)
        {
            if (GetSplitPartsCount() > 1)
            {
                List<WorkoutPart> splitParts = ConcreteWorkout.SplitInSeperateParts();

                foreach (WorkoutPart part in splitParts)
                {
                    part.SerializetoFIT(stream);
                }
            }
            else
            {
                FITMessage workoutMessage = new FITMessage(FITGlobalMessageIds.Workout);
                FITMessageField sportType = new FITMessageField((Byte)FITWorkoutFieldIds.SportType);
                FITMessageField numValidSteps = new FITMessageField((Byte)FITWorkoutFieldIds.NumSteps);
                FITMessageField workoutName = new FITMessageField((Byte)FITWorkoutFieldIds.WorkoutName);

                sportType.SetEnum((Byte)Options.Instance.GetFITSport(Category));
                workoutMessage.AddField(sportType);
                numValidSteps.SetUInt16(StepCount);
                workoutMessage.AddField(numValidSteps);

                if (!String.IsNullOrEmpty(Name))
                {
                    workoutName.SetString(Name);
                    workoutMessage.AddField(workoutName);
                }

                workoutMessage.Serialize(stream);

                foreach (IStep step in Steps)
                {
                    step.SerializetoFIT(stream);
                }
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
                else if (type == IStep.StepType.Repeat)
                {
                    deserializedSteps.Add(new RepeatStep(stream, Constants.CurrentVersion, ConcreteWorkout));
                }
                else
                {
                    WorkoutLinkStep tempLink = new WorkoutLinkStep(stream, Constants.CurrentVersion, ConcreteWorkout);

                    if (tempLink.LinkedWorkout != null)
                    {
                        deserializedSteps.Add(tempLink);
                    }
                    else
                    {
                        WorkoutStepsList linkSteps = new WorkoutStepsList(ConcreteWorkout);

                        linkSteps.Deserialize(stream, Constants.CurrentVersion);
                        deserializedSteps.AddRange(linkSteps);
                    }
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
            STExtensions.Add(extensionNode);
        }

        public void AddStepExtension(XmlNode extensionNode)
        {
            StepsExtensions.Add(extensionNode);
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

        public virtual UInt16 StepCount
        {
            get
            {
                UInt16 result = 0;

                foreach (IStep step in Steps)
                {
                    result += step.StepCount;
                }

                return result;
            }
        }

        public virtual UInt16 GetSplitPartsCount()
        {
            if (Steps.Count > 0)
            {
                return GetStepSplitPart(Steps[Steps.Count - 1]);
            }

            return 0;
        }

        public UInt16 GetStepSplitPart(IStep step)
        {
            UInt16 counter = 0;
            UInt16 partNumber = 1;

            if (GetStepSplitPart(step, Steps, ref partNumber, ref counter))
            {
                return partNumber;
            }

            return 0;
        }

        public WorkoutStepsList GetStepsForPart(int partNumber)
        {
            WorkoutStepsList result = new WorkoutStepsList(this);
            UInt16 currentPartNumber = 0;
            int counter = 0;

            for (int i = 0; i < Steps.Count; ++i)
            {
                IStep currentStep = Steps[i];

                counter += currentStep.StepCount;

                if (i != 0 && (currentStep.ForceSplitOnStep || counter > Constants.MaxStepsPerWorkout))
                {
                    currentPartNumber++;
                    counter = currentStep.StepCount;
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

        private bool GetStepSplitPart(IStep step, WorkoutStepsList stepsList, ref UInt16 stepPart, ref UInt16 stepCounter)
        {
            IStep topMostRepeat = Steps.GetTopMostRepeatForStep(step);
            IStep stepToFind = step;

            if (topMostRepeat != null)
            {
                stepToFind = topMostRepeat;
            }

            if (stepsList.Count > 0 &&
                stepsList[0].ForceSplitOnStep && stepsList[0] != Steps[0])
            {
                stepPart++;
                stepCounter = 0;
            }

            foreach (IStep currentStep in stepsList)
            {
                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;

                    if (currentStep == step)
                    {
                        if (linkStep.LinkedWorkoutSteps.Count > 0)
                        {
                            // The part # of a workout link is the part number of it's last step
                            return GetStepSplitPart(linkStep.LinkedWorkoutSteps[linkStep.LinkedWorkoutSteps.Count - 1],
                                                    linkStep.LinkedWorkoutSteps,
                                                    ref stepPart,
                                                    ref stepCounter);
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (GetStepSplitPart(step, linkStep.LinkedWorkoutSteps,
                                            ref stepPart, ref stepCounter))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    stepCounter += currentStep.StepCount;
                }

                if ((currentStep.ForceSplitOnStep && currentStep != Steps[0]) ||
                    stepCounter > Constants.MaxStepsPerWorkout)
                {
                    stepPart++;
                    stepCounter = 0;
                }

                if (currentStep == stepToFind)
                {
                    return true;
                }
            }

            return false;
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
            return CanAcceptNewStep(newStep.StepCount, destinationStep);
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
            UInt16 counter = 0;
            bool result = GetStepExportIdInternal(step, Steps, ref counter);

            Debug.Assert(result);

            return counter;
        }

        private bool GetStepExportIdInternal(IStep step, List<IStep> stepsList, ref UInt16 stepCounter)
        {
            foreach (IStep currentStep in stepsList)
            {
                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep concreteStep = currentStep as WorkoutLinkStep;
                    UInt16 tempCounter = 0;

                    if (GetStepExportIdInternal(step, concreteStep.LinkedWorkoutSteps, ref tempCounter))
                    {
                        stepCounter += tempCounter;
                        return true;
                    }
                }
                else if (currentStep is RepeatStep)
                {
                    RepeatStep concreteStep = currentStep as RepeatStep;
                    UInt16 tempCounter = 0;

                    if (GetStepExportIdInternal(step, concreteStep.StepsToRepeat, ref tempCounter))
                    {
                        stepCounter += tempCounter;
                        return true;
                    }
                }

                stepCounter += currentStep.StepCount;

                if (currentStep == step)
                {
                    return true;
                }
            }

            return false;
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

        public virtual List<XmlNode> StepsExtensions
        {
            get { return ConcreteWorkout.StepsExtensions; }
        }

        public virtual List<XmlNode> STExtensions
        {
            get { return ConcreteWorkout.STExtensions; }
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
    }
}

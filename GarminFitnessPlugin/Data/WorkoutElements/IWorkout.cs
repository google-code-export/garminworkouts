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
    public abstract class IWorkout : IPluginSerializable, IXMLSerializable
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
                foreach (DateTime scheduledDate in ScheduledDates)
                {
                    GarminFitnessDate currentSchedule = new GarminFitnessDate(scheduledDate.ToUniversalTime());
                    currentSchedule.Serialize(workoutNode, "ScheduledOn", document);
                }

                // Notes
                if (Notes != String.Empty && Notes != null)
                {
                    NotesInternal.Serialize(workoutNode, "Notes", document);
                }

                // Creator tag (required by 610XT)
                XmlNode creatorNode = document.CreateElement("Creator");
                XmlAttribute attributeNode = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);

                attributeNode.Value = "Device_t";
                creatorNode.Attributes.Append(attributeNode);
                workoutNode.AppendChild(creatorNode);
                creatorNode.AppendChild(document.CreateElement("Name"));

                XmlNode unitIdNode = document.CreateElement("UnitId");
                unitIdNode.AppendChild(document.CreateTextNode("1234567890"));
                creatorNode.AppendChild(unitIdNode);

                XmlNode productIdNode = document.CreateElement("ProductID");
                productIdNode.AppendChild(document.CreateTextNode("0"));
                creatorNode.AppendChild(productIdNode);

                XmlNode versionNode = document.CreateElement("Version");
                creatorNode.AppendChild(versionNode);

                XmlNode versionMajorNode = document.CreateElement("VersionMajor");
                versionMajorNode.AppendChild(document.CreateTextNode("0"));
                versionNode.AppendChild(versionMajorNode);
                XmlNode versionMinorNode = document.CreateElement("VersionMinor");
                versionMinorNode.AppendChild(document.CreateTextNode("0"));
                versionNode.AppendChild(versionMinorNode);
                XmlNode buildMajorNode = document.CreateElement("BuildMajor");
                buildMajorNode.AppendChild(document.CreateTextNode("0"));
                versionNode.AppendChild(buildMajorNode);
                XmlNode buildMinorNode = document.CreateElement("BuildMinor");
                buildMinorNode.AppendChild(document.CreateTextNode("0"));
                versionNode.AppendChild(buildMinorNode);

                // Extensions
                childNode = document.CreateElement(Constants.ExtensionsTCXString);
                workoutNode.AppendChild(childNode);

                // Steps extensions
                if (StepsExtensions.Count > 0)
                {
                    XmlNode extensionsNode = document.CreateElement("Steps");

                    // xmlns namespace attribute
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
                    attribute.Value = "http://www.zonefivesoftware.com/sporttracks/plugins/?p=garmin-fitness";
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

        public virtual void SerializeToFIT(Stream stream)
        {
            Debug.Assert(GetSplitPartsCount() == 1);

            FITMessage workoutMessage = new FITMessage(FITGlobalMessageIds.Workout);

            FillFITMessage(workoutMessage);
            workoutMessage.Serialize(stream);

            bool serializeDefinition = true;
            foreach (IStep step in Steps)
            {
                step.SerializeToFIT(stream, serializeDefinition);
                serializeDefinition = false;
            }
        }

        public void FillFITMessage(FITMessage workoutMessage)
        {
            FITMessageField capabilities = new FITMessageField((Byte)FITWorkoutFieldIds.Capabilities);
            FITMessageField sportType = new FITMessageField((Byte)FITWorkoutFieldIds.SportType);
            FITMessageField numValidSteps = new FITMessageField((Byte)FITWorkoutFieldIds.NumSteps);
            FITMessageField unknown = new FITMessageField((Byte)FITWorkoutFieldIds.Unknown);
            FITMessageField workoutName = new FITMessageField((Byte)FITWorkoutFieldIds.WorkoutName);

            sportType.SetEnum((Byte)Options.Instance.GetFITSport(Category));
            capabilities.SetUInt32z(32);
            numValidSteps.SetUInt16(StepCount);
            unknown.SetUInt16(0xFFFF);

            workoutMessage.AddField(capabilities);
            if (!String.IsNullOrEmpty(Name))
            {
                workoutName.SetString(Name, (Byte)(Constants.MaxNameLength + 1));
                workoutMessage.AddField(workoutName);
            }
            workoutMessage.AddField(numValidSteps);
            workoutMessage.AddField(unknown);
            workoutMessage.AddField(sportType);
        }

        public virtual void SerializeToFITSchedule(Stream stream, bool serializeDefiniton)
        {
            Debug.Assert(GetSplitPartsCount() == 1);

            FITMessage scheduleMessage = new FITMessage(FITGlobalMessageIds.WorkoutSchedules);

            foreach (DateTime scheduledDate in ScheduledDates)
            {
                FillFITMessageForScheduledDate(scheduleMessage, scheduledDate);

                scheduleMessage.Serialize(stream, serializeDefiniton);
                serializeDefiniton = false;
            }
        }

        public virtual void FillFITMessageForScheduledDate(FITMessage scheduleMessage, DateTime scheduledDate)
        {
            FITMessageField workoutManufacturer = new FITMessageField((Byte)FITScheduleFieldIds.WorkoutManufacturer);
            FITMessageField workoutProduct = new FITMessageField((Byte)FITScheduleFieldIds.WorkoutProduct);
            FITMessageField workoutSN = new FITMessageField((Byte)FITScheduleFieldIds.WorkoutSN);
            FITMessageField scheduleType = new FITMessageField((Byte)FITScheduleFieldIds.ScheduleType);
            FITMessageField workoutCompleted = new FITMessageField((Byte)FITScheduleFieldIds.WorkoutCompleted);
            FITMessageField workoutId = new FITMessageField((Byte)FITScheduleFieldIds.WorkoutId);
            FITMessageField scheduledField = new FITMessageField((Byte)FITScheduleFieldIds.ScheduledDate);
            DateTime midDaySchedule = new DateTime(scheduledDate.Date.Year, scheduledDate.Date.Month, scheduledDate.Day, 12, 0, 0);
            TimeSpan timeSinceReference = midDaySchedule - new DateTime(1989, 12, 31);

            // Hardcoded fields from the schedule file
            workoutManufacturer.SetUInt16(1);           // Always 1
            workoutProduct.SetUInt16(20119);            // Always 20119
            workoutSN.SetUInt32z(0);                    // Invalid
            workoutCompleted.SetEnum((Byte)0xFF);       // Invalid/Not completed

            // Real data
            scheduleType.SetEnum((Byte)FITScheduleType.Workout);
            workoutId.SetUInt32(CreationTimestamp);

            scheduleMessage.AddField(workoutSN);
            scheduleMessage.AddField(workoutId);
            scheduleMessage.AddField(scheduledField);
            scheduleMessage.AddField(workoutManufacturer);
            scheduleMessage.AddField(workoutProduct);
            scheduleMessage.AddField(workoutCompleted);
            scheduleMessage.AddField(scheduleType); 

            scheduledField.SetUInt32((UInt32)timeSinceReference.TotalSeconds);
        }

        public abstract void DeserializeFromFIT(FITMessage workoutMessage);

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
            if (changedProperty.PropertyName == "ForceSplitOnStep" ||
                changedProperty.PropertyName == "LinkSteps")
            {
                TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("PartsCount"));
            }

            if (ConcreteWorkout.StepChanged != null)
            {
                ConcreteWorkout.StepChanged(this, modifiedStep, changedProperty);
            }
        }

        void OnDurationChanged(RegularStep modifiedStep, IDuration durationChanged, PropertyChangedEventArgs changedProperty)
        {
            if (ConcreteWorkout.StepDurationChanged != null)
            {
                ConcreteWorkout.StepDurationChanged(this, modifiedStep, durationChanged, changedProperty);
            }
        }

        void OnRepeatDurationChanged(RepeatStep modifiedStep, IRepeatDuration durationChanged, PropertyChangedEventArgs changedProperty)
        {
            if (ConcreteWorkout.StepRepeatDurationChanged != null)
            {
                ConcreteWorkout.StepRepeatDurationChanged(this, modifiedStep, durationChanged, changedProperty);
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

        public virtual IStep ParentStep
        {
            get { return null; }
        }

        public WorkoutStepsList GetStepsForPart(int partNumber)
        {
            WorkoutStepsList result = new WorkoutStepsList(this);
            UInt16 currentPartNumber = 0;
            int counter = 0;

            GetStepsForPart(partNumber, ref result, ref currentPartNumber, ref counter);

            return result;
        }

        public WorkoutStepsList GetStepsForPart(int partNumber, ref WorkoutStepsList result,
                                                ref UInt16 partNumberCounter, ref int stepCounter)
        {
            for (int i = 0; i < Steps.Count; ++i)
            {
                IStep currentStep = Steps[i];

                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;

                    linkStep.GetStepsForPart(partNumber, ref result,
                                             ref partNumberCounter, ref stepCounter);
                }
                else
                {
                    stepCounter += currentStep.StepCount;

                    if (i != 0 && (currentStep.ForceSplitOnStep || stepCounter > Constants.MaxStepsPerWorkout))
                    {
                        partNumberCounter++;
                        stepCounter = currentStep.StepCount;
                    }

                    if (partNumberCounter == partNumber)
                    {
                        // Add step to result, it's in the right part
                        result.Add(currentStep);
                    }
                    else if (partNumberCounter > partNumber)
                    {
                        break;
                    }
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
                    if ((currentStep.ForceSplitOnStep && currentStep != Steps[0]) ||
                        stepCounter + currentStep.StepCount > Constants.MaxStepsPerWorkout)
                    {
                        stepPart++;
                        stepCounter = 0;
                    }

                    stepCounter += currentStep.StepCount;
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

            if (stepToRegister is RegularStep)
            {
                RegularStep regularStep = stepToRegister as RegularStep;

                regularStep.DurationChanged += new RegularStep.StepDurationChangedEventHandler(OnDurationChanged);
                regularStep.TargetChanged += new RegularStep.StepTargetChangedEventHandler(OnTargetChanged);
            }
            else if (stepToRegister is RepeatStep)
            {
                RepeatStep repeatStep = stepToRegister as RepeatStep;

                repeatStep.DurationChanged += new RepeatStep.StepDurationChangedEventHandler(OnRepeatDurationChanged);
            }

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

            if (stepToUnregister is RegularStep)
            {
                RegularStep regularStep = stepToUnregister as RegularStep;

                regularStep.DurationChanged -= new RegularStep.StepDurationChangedEventHandler(OnDurationChanged);
                regularStep.TargetChanged -= new RegularStep.StepTargetChangedEventHandler(OnTargetChanged);
            }
            else if (stepToUnregister is RepeatStep)
            {
                RepeatStep repeatStep = stepToUnregister as RepeatStep;

                repeatStep.DurationChanged -= new RepeatStep.StepDurationChangedEventHandler(OnRepeatDurationChanged);
            }
        }

        public int GetStepExportId(IStep step)
        {
            IWorkout containerWorkout = this;
            UInt16 counter = 0;

            if (step is WorkoutLinkStep)
            {
                step = (step as WorkoutLinkStep).LinkedWorkoutSteps[0];
            }

            if (GetSplitPartsCount() > 1)
            {
                ushort stepSplitPart = GetStepSplitPart(step);

                if (stepSplitPart == 0)
                {
                    throw new Exception("Step not found in workout");
                }

                containerWorkout = ConcreteWorkout.SplitInSeperateParts()[stepSplitPart - 1];
            }

            if(!containerWorkout.GetStepExportIdInternal(step, containerWorkout.Steps, ref counter))
            {
                throw new Exception("Step not found in workout");
            }

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
            // Force it to be at noon
            DateTime midDaySchedule = new DateTime(date.Date.Year, date.Date.Month, date.Day, 12, 0, 0);
            GarminFitnessDate temp = new GarminFitnessDate(midDaySchedule);

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

        public bool ContainsFITOnlyFeatures
        {
            get { return Steps.ContainsFITOnlyFeatures; }
        }

        public bool ContainsTCXExtensionFeatures
        {
            get { return Steps.ContainsTCXExtensionFeatures; }
        }

        public UInt32 CreationTimestamp
        {
            get
            {
                // Trick to get a unique valid timestamp per workout
                return (UInt32)((Name.GetHashCode() & 0x0FFFFFFF) | 0x10000000);
            }
        }

        // Abstract methods
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

        public delegate void StepRepeatDurationChangedEventHandler(IWorkout modifiedWorkout, RepeatStep modifiedStep, IRepeatDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event StepRepeatDurationChangedEventHandler StepRepeatDurationChanged;

        public delegate void StepTargetChangedEventHandler(IWorkout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty);
        public event StepTargetChangedEventHandler StepTargetChanged;

        private bool m_EventsActive = true;
    }
}

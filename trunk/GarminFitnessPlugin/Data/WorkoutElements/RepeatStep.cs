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
    public class RepeatStep : IStep
    {
        public RepeatStep(Workout parent)
            : this(2, parent)
        {
        }

        public RepeatStep(Byte numRepeats, Workout parent)
            : base(StepType.Repeat, parent)
        {
            Debug.Assert(numRepeats <= Constants.MaxRepeats);

            RepeatCountDuration repeatCountDuration = new RepeatCountDuration(this);
            repeatCountDuration.RepetitionCount = numRepeats;
            Duration = repeatCountDuration;

            m_StepsToRepeat.Add(new RegularStep(parent));
        }

        public RepeatStep(Stream stream, DataVersion version, Workout parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        private void OnDurationChanged(IRepeatDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            TriggerDurationChangedEvent(modifiedDuration, changedProperty);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(StepsToRepeat.Count), 0, sizeof(Int32));
            foreach (IStep currentStep in StepsToRepeat)
            {
                currentStep.Serialize(stream);
            }

            Duration.Serialize(stream);
        }


        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];
            Int32 stepCount;

            RepeatCountDuration repeatCountDuration = new RepeatCountDuration(this);
            repeatCountDuration.Deserialize(stream, version);
            Duration = repeatCountDuration;

            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);

            // In case the repeat was already registered on the workout
            ParentConcreteWorkout.Steps.RemoveSteps(StepsToRepeat, false);
            m_StepsToRepeat.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);
                if (type == IStep.StepType.Regular)
                {
                    m_StepsToRepeat.Add(new RegularStep(stream, version, ParentConcreteWorkout));
                }
                else if (type == IStep.StepType.Link)
                {
                    WorkoutLinkStep tempLink = new WorkoutLinkStep(stream, version, ParentConcreteWorkout);

                    if (tempLink.LinkedWorkout != null)
                    {
                        m_StepsToRepeat.Add(tempLink);
                    }
                    else
                    {
                        WorkoutStepsList linkSteps = new WorkoutStepsList(ParentConcreteWorkout);

                        linkSteps.Deserialize(stream, Constants.CurrentVersion);
                        m_StepsToRepeat.AddRange(linkSteps);
                    }
                }
                else
                {
                    m_StepsToRepeat.Add(new RepeatStep(stream, version, ParentConcreteWorkout));
                }
            }
        }

        public void Deserialize_V20(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];
            Int32 stepCount;

            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);

            // In case the repeat was already registered on the workout
            ParentConcreteWorkout.Steps.RemoveSteps(StepsToRepeat, false);
            m_StepsToRepeat.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);
                if (type == IStep.StepType.Regular)
                {
                    m_StepsToRepeat.Add(new RegularStep(stream, version, ParentConcreteWorkout));
                }
                else if (type == IStep.StepType.Link)
                {
                    WorkoutLinkStep tempLink = new WorkoutLinkStep(stream, version, ParentConcreteWorkout);

                    if (tempLink.LinkedWorkout != null)
                    {
                        m_StepsToRepeat.Add(tempLink);
                    }
                    else
                    {
                        WorkoutStepsList linkSteps = new WorkoutStepsList(ParentConcreteWorkout);

                        linkSteps.Deserialize(stream, Constants.CurrentVersion);
                        m_StepsToRepeat.AddRange(linkSteps);
                    }
                }
                else
                {
                    m_StepsToRepeat.Add(new RepeatStep(stream, version, ParentConcreteWorkout));
                }
            }

            stream.Read(intBuffer, 0, sizeof(Int32));
            IRepeatDuration.RepeatDurationType durationType = (IRepeatDuration.RepeatDurationType)(BitConverter.ToInt32(intBuffer, 0));
            Duration = DurationFactory.Create(durationType, stream, version, this);
        }

        public override void SerializetoFIT(Stream stream)
        {
            // Serialize children first, followed by the repeat
            foreach (IStep child in StepsToRepeat)
            {
                child.SerializetoFIT(stream);
            }

            base.SerializetoFIT(stream);
        }

        public override void DeserializeFromFIT(FITMessage stepMessage)
        {
            FITMessageField stepsToRepeatField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationValue);

            if (stepsToRepeatField != null)
            {
                Int32 precedingStepsToRepeat = (Int32)(ParentWorkout.StepCount - stepsToRepeatField.GetUInt32());
                List<IStep> stepsToRepeat = new List<IStep>();

                while (precedingStepsToRepeat > 0)
                {
                    Int32 precedingStepIndex = ParentWorkout.Steps.Count - 1;
                    Int32 precedingStepCounter = ParentWorkout.Steps[precedingStepIndex].StepCount;

                    while (precedingStepCounter < precedingStepsToRepeat)
                    {
                        precedingStepCounter += ParentWorkout.Steps[precedingStepIndex].StepCount;
                        precedingStepIndex--;
                    }

                    IStep precedingStep = ParentWorkout.Steps[precedingStepIndex];

                    stepsToRepeat.Add(precedingStep);

                    precedingStepsToRepeat -= precedingStep.StepCount;
                }

                // Officialize result in workout
                ParentConcreteWorkout.Steps.RemoveSteps(stepsToRepeat, false);
                // In case the repeat wasn't yet registered on the workout
                StepsToRepeat.Clear();
                foreach (IStep currentStep in stepsToRepeat)
                {
                    StepsToRepeat.Add(currentStep);
                }

                DurationFactory.Create(stepMessage, this);
            }
            else
            {
                throw new FITParserException("Missing steps to repeat");
            }
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField repeatFromStep = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationValue);
            FITMessageField targetType = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetType);

            repeatFromStep.SetUInt32((UInt32)(ParentWorkout.GetStepExportId(StepsToRepeat[0]) - 1));
            message.AddField(repeatFromStep);
            targetType.SetEnum((Byte)FITWorkoutStepTargetTypes.NoTarget);
            message.AddField(targetType);

            Duration.FillFITStepMessage(message);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            Debug.Assert(Duration is RepeatCountDuration);
            Duration.Serialize(parentNode.LastChild, "Repetitions", document);

            // Export all children
            foreach(IStep currentStep in StepsToRepeat)
            {
                currentStep.Serialize(parentNode.LastChild, "Child", document);
            }
        }

        public override void Deserialize(XmlNode parentNode)
        {
            bool repeatsRead = false;
            List<IStep> stepsToRepeat = new List<IStep>();

            base.Deserialize(parentNode);

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.Name == "Repetitions")
                {
                    RepeatCountDuration repeatCountDuration = new RepeatCountDuration(this);

                    repeatCountDuration.Deserialize(child);
                    Duration = repeatCountDuration;
                    repeatsRead = true;
                }
                else if (child.Name == "Child")
                {
                    string stepTypeString = child.Attributes[0].Value;
                    IStep newStep = null;

                    if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Regular])
                    {
                        newStep = new RegularStep(ParentConcreteWorkout);
                    }
                    else if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Repeat])
                    {
                        newStep = new RepeatStep(ParentConcreteWorkout);
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    stepsToRepeat.Add(newStep);
                    newStep.Deserialize(child);
                }
            }

            if (!repeatsRead || stepsToRepeat.Count == 0)
            {
                throw new GarminFitnessXmlDeserializationException("Information missing in the XML node", parentNode);
            }

            ParentConcreteWorkout.Steps.RemoveSteps(m_StepsToRepeat, false);
            // In case the repeat wasn't yet registered on the workout
            m_StepsToRepeat.Clear();
            for (int i = 0; i < stepsToRepeat.Count; ++i)
            {
                m_StepsToRepeat.Add(stepsToRepeat[i]);
            }
        }

        public override IStep Clone()
        {
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(sizeof(Int32), SeekOrigin.Begin);

            return new RepeatStep(stream, Constants.CurrentVersion, ParentConcreteWorkout);
        }

        public override bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory)
        {
            bool valueChanged = false;
            
            // Validate all steps
            for (int i = 0; i < m_StepsToRepeat.Count; ++i)
            {
                if (m_StepsToRepeat[i].ValidateAfterZoneCategoryChanged(changedCategory))
                {
                    valueChanged = true;
                }
            }

            return valueChanged;
        }

        public bool IsChildStep(IStep step)
        {
            if (StepsToRepeat.Contains(step))
            {
                return true;
            }

            foreach (IStep currentStep in StepsToRepeat)
            {
                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;

                    if (linkStep.LinkedWorkoutSteps.Contains(step))
                    {
                        return true;
                    }
                }
                else if (currentStep is RepeatStep)
                {
                    RepeatStep repeatStep = currentStep as RepeatStep;

                    if (repeatStep.IsChildStep(step))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void MarkAllCadenceSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_StepsToRepeat.Count; ++i)
            {
                if (m_StepsToRepeat[i].Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)m_StepsToRepeat[i];

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
                else if (m_StepsToRepeat[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)m_StepsToRepeat[i];

                    concreteStep.MarkAllCadenceSTZoneTargetsAsDirty();
                }
            }
        }

        public void MarkAllPowerSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_StepsToRepeat.Count; ++i)
            {
                if (m_StepsToRepeat[i].Type == IStep.StepType.Regular)
                {
                    RegularStep concreteStep = (RegularStep)m_StepsToRepeat[i];

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
                else if (m_StepsToRepeat[i].Type == IStep.StepType.Repeat)
                {
                    RepeatStep concreteStep = (RepeatStep)m_StepsToRepeat[i];

                    concreteStep.MarkAllPowerSTZoneTargetsAsDirty();
                }
            }
        }

        protected void TriggerDurationChangedEvent(IRepeatDuration duration, PropertyChangedEventArgs args)
        {
            if (DurationChanged != null)
            {
                DurationChanged(this, duration, args);
            }
        }

        public override Workout ParentConcreteWorkout
        {
            get { return base.ParentConcreteWorkout; }
            set
            {
                base.ParentConcreteWorkout = value;

                foreach (IStep currentStep in StepsToRepeat)
                {
                    currentStep.ParentConcreteWorkout = value;
                }
            }
        }

        public override UInt16 StepCount
        {
            get
            {
                UInt16 stepCount = base.StepCount;

                foreach (IStep currentStep in m_StepsToRepeat)
                {
                    stepCount += currentStep.StepCount;
                }

                return stepCount;
            }
        }

        public IRepeatDuration Duration
        {
            get { return m_Duration; }
            set
            {
                if (m_Duration != value)
                {
                    if (m_Duration != null)
                    {
                        m_Duration.DurationChanged -= new IRepeatDuration.DurationChangedEventHandler(OnDurationChanged);
                    }

                    m_Duration = value;
                    Debug.Assert(m_Duration != null);

                    m_Duration.DurationChanged += new IRepeatDuration.DurationChangedEventHandler(OnDurationChanged);

                    TriggerStepChanged(new PropertyChangedEventArgs("Duration"));
                }
            }
        }

        public List<IStep> StepsToRepeat
        {
            get { return m_StepsToRepeat; }
        }

        public bool IsEmpty
        {
            get { return m_StepsToRepeat.Count == 0; }
        }

        public override bool IsDirty
        {
            get
            {
                for (int i = 0; i < m_StepsToRepeat.Count; ++i)
                {
                    if (m_StepsToRepeat[i].IsDirty)
                    {
                        return true;
                    }
                }

                return false;
            }
            set { Debug.Assert(false); }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get
            {
                if (Duration.ContainsFITOnlyFeatures)
                {
                    return true;
                }
                else
                {
                    foreach (IStep step in m_StepsToRepeat)
                    {
                        if (step.ContainsFITOnlyFeatures)
                        {
                            return true;
                        }
                    }
                }

                return false; 
            }
        }

        public override bool ContainsTCXExtensionFeatures
        {
            get
            {
                foreach(IStep step in m_StepsToRepeat)
                {
                    if (step.ContainsTCXExtensionFeatures)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public delegate void StepDurationChangedEventHandler(RepeatStep modifiedStep, IRepeatDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event StepDurationChangedEventHandler DurationChanged;

        private IRepeatDuration m_Duration = null;
        private List<IStep> m_StepsToRepeat = new List<IStep>();
    }
}

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
    class RepeatStep : IStep
    {
        public RepeatStep(Workout parent)
            : this(2, parent)
        {
        }

        public RepeatStep(Byte numRepeats, Workout parent)
            : base(StepType.Repeat, parent)
        {
            Debug.Assert(numRepeats <= Constants.MaxRepeats);
            RepetitionCount = numRepeats;

            m_StepsToRepeat.Add(new RegularStep(parent));
        }

        public RepeatStep(Stream stream, DataVersion version, Workout parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_RepetitionCount.Serialize(stream);

            stream.Write(BitConverter.GetBytes(StepsToRepeat.Count), 0, sizeof(Int32));
            for (int i = 0; i < StepsToRepeat.Count; ++i)
            {
                StepsToRepeat[i].Serialize(stream);
            }
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] byteBuffer = new byte[sizeof(Byte)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            Int32 stepCount;

            m_RepetitionCount.Deserialize(stream, version);

            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);

            // In case the repeat was already registered on the workout
            ParentWorkout.RemoveSteps(StepsToRepeat);
            m_StepsToRepeat.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);
                if (type == IStep.StepType.Regular)
                {
                    m_StepsToRepeat.Add(new RegularStep(stream, version, ParentWorkout));
                }
                else
                {
                    m_StepsToRepeat.Add(new RepeatStep(stream, version, ParentWorkout));
                }
            }
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            m_RepetitionCount.Serialize(parentNode, "Repetitions", document);

            // Export all children
            for (int i = 0; i < StepsToRepeat.Count; ++i)
            {
                XmlNode childNode = document.CreateElement("Child");

                StepsToRepeat[i].Serialize(childNode, "Child", document);
                parentNode.AppendChild(childNode);
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
                    m_RepetitionCount.Deserialize(child);
                    repeatsRead = true;
                }
                else if (child.Name == "Child")
                {
                    string stepTypeString = child.Attributes[0].Value;
                    IStep newStep = null;

                    if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Regular])
                    {
                        newStep = new RegularStep(ParentWorkout);
                    }
                    else if (stepTypeString == Constants.StepTypeTCXString[(int)IStep.StepType.Repeat])
                    {
                        newStep = new RepeatStep(ParentWorkout);
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
                throw new GarminFitnesXmlDeserializationException("Information missing in the XML node", parentNode);
            }

            ParentWorkout.RemoveSteps(m_StepsToRepeat);
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

            return new RepeatStep(stream, Constants.CurrentVersion, ParentWorkout);
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

        public override byte GetStepCount()
        {
            byte stepCount = base.GetStepCount();

            for (int i = 0; i < m_StepsToRepeat.Count; ++i)
            {
                stepCount += m_StepsToRepeat[i].GetStepCount();
            }

            return stepCount;
        }

        public bool IsChildStep(IStep step)
        {
            for (int i = 0; i < StepsToRepeat.Count; ++i)
            {
                IStep currentStep = StepsToRepeat[i];

                if (currentStep == step)
                {
                    return true;
                }
                else if(currentStep.Type == StepType.Repeat)
                {
                    RepeatStep repeatStep = (RepeatStep)currentStep;

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

        public Byte RepetitionCount
        {
            get { return m_RepetitionCount; }
            set
            {
                if (m_RepetitionCount != value)
                {
                    m_RepetitionCount.Value = value;

                    TriggerStepChanged(new PropertyChangedEventArgs("RepetitionCount"));
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

        private GarminFitnessByteRange m_RepetitionCount = new GarminFitnessByteRange(Constants.MinRepeats, Constants.MinRepeats, Constants.MaxRepeats);
        private List<IStep> m_StepsToRepeat = new List<IStep>();
    }
}

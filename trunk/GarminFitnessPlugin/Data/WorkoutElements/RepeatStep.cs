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
            Debug.Assert(numRepeats <= 99);
            m_RepetitionCount = numRepeats;

            ParentWorkout.AddNewStep(new RegularStep(parent), this);
        }

        public RepeatStep(Stream stream, DataVersion version, Workout parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            stream.Write(BitConverter.GetBytes(RepetitionCount), 0, sizeof(Byte));

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

            stream.Read(byteBuffer, 0, sizeof(Byte));
            RepetitionCount = byteBuffer[0];

            stream.Read(intBuffer, 0, sizeof(Int32));
            stepCount = BitConverter.ToInt32(intBuffer, 0);
            StepsToRepeat.Clear();
            for (int i = 0; i < stepCount; ++i)
            {
                IStep.StepType type;

                stream.Read(intBuffer, 0, sizeof(Int32));
                type = (IStep.StepType)BitConverter.ToInt32(intBuffer, 0);

                if (type == IStep.StepType.Regular)
                {
                    ParentWorkout.AddNewStep(new RegularStep(stream, version, ParentWorkout), this);
                }
                else
                {
                    ParentWorkout.AddNewStep(new RepeatStep(stream, version, ParentWorkout), this);
                }
            }
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);
            XmlNode elementNode;
            
            elementNode = document.CreateElement("Repetitions");
            elementNode.AppendChild(document.CreateTextNode(RepetitionCount.ToString()));
            parentNode.AppendChild(elementNode);

            // Export all children
            for (int i = 0; i < StepsToRepeat.Count; ++i)
            {
                XmlNode childNode = document.CreateElement("Child");

                StepsToRepeat[i].Serialize(childNode, document);
                parentNode.AppendChild(childNode);
            }
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                bool repetitionsLoaded = false;

                StepsToRepeat.Clear();

                for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode child = parentNode.ChildNodes[i];

                    if (child.Name == "Repetitions")
                    {
                        RepetitionCount = byte.Parse(child.FirstChild.Value);
                        repetitionsLoaded = true;
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

                        if (newStep != null && newStep.Deserialize(child))
                        {
                            ParentWorkout.AddNewStep(newStep, this);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                if (repetitionsLoaded && StepsToRepeat.Count > 0)
                {
                    return true;
                }
            }

            return false;
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
                    m_RepetitionCount = value;

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

        private Byte m_RepetitionCount;
        private List<IStep> m_StepsToRepeat = new List<IStep>();
    }
}

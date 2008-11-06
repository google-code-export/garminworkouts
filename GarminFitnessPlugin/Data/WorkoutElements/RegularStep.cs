using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPlugin.View;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class RegularStep : IStep
    {
        public RegularStep(Workout parent)
            : base(StepType.Regular, parent)
        {
            Duration = new LapButtonDuration(this);
            Target = new NullTarget(this);
        }

        public RegularStep(string name, Workout parent)
            : this(parent)
        {
            Name = name;
        }

        public RegularStep(IDuration duration, ITarget target, Workout parent)
            : base(StepType.Regular, parent)
        {
            Duration = duration;
            Target = target;
        }

        public RegularStep(string name, IDuration duration, ITarget target, Workout parent)
            : this(duration, target, parent)
        {
            Name = name;
        }

        public RegularStep(Stream stream, DataVersion version, Workout parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        private void OnDurationChanged(IDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            TriggerDurationChangedEvent(modifiedDuration, changedProperty);
        }

        private void OnTargetChanged(ITarget modifiedTarget, PropertyChangedEventArgs changedProperty)
        {
            TriggerTargetChangedEvent(modifiedTarget, changedProperty);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            // Name
            if (Name != null && Name != String.Empty)
            {
                stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Name)), 0, sizeof(Int32));
                stream.Write(Encoding.UTF8.GetBytes(Name), 0, Encoding.UTF8.GetByteCount(Name));
            }
            else
            {
                stream.Write(BitConverter.GetBytes((Int32)0), 0, sizeof(Int32));
            }

            // Resting
            stream.Write(BitConverter.GetBytes(IsRestingStep), 0, sizeof(bool));

            Duration.Serialize(stream);
            Target.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            Int32 stringLength;

            // Name
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Name = Encoding.UTF8.GetString(stringBuffer);

            // Resting
            stream.Read(boolBuffer, 0, sizeof(bool));
            IsRestingStep = BitConverter.ToBoolean(boolBuffer, 0);

            // Duration
            stream.Read(intBuffer, 0, sizeof(Int32));
            DurationFactory.Create((IDuration.DurationType)BitConverter.ToInt32(intBuffer, 0), stream, version, this);

            // Target
            stream.Read(intBuffer, 0, sizeof(Int32));
            Int32 type = BitConverter.ToInt32(intBuffer, 0);

            // This sucks but I changed the order of the enum between version 0 and 1,
            //  so make sure the version is right
            if (version.VersionNumber == 0)
            {
                if (type == 3)
                {
                    // Null was #3, now #0
                    type = 0;
                }
                else
                {
                    // Everything else is pushed up 1 position
                    ++type;
                }
            }

            TargetFactory.Create((ITarget.TargetType)type, stream, version, this);
        }

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            if (Target.Type == ITarget.TargetType.Power)
            {
                // Power was added to the format as an extension which gives me a headache
                //  So we need to serialize this target as having a NullTarget and then
                //  add a step extension that uses the real target with power info
                ITarget realTarget = Target;

                // Create the fake target
                TargetFactory.Create(ITarget.TargetType.Null, this);
                Serialize(parentNode, document);

                // Restore old target
                Target = realTarget;

                // Create new parent node and add it to the extensions
                parentNode = document.CreateElement("Step");
                ParentWorkout.AddStepExtension(parentNode);
            }

            // Ok now this the real stuff but the target can either be the fake one or the real one
            base.Serialize(parentNode, document);

            XmlNode elementNode;

            if (Name != String.Empty && Name != null)
            {
                elementNode = document.CreateElement("Name");
                elementNode.AppendChild(document.CreateTextNode(Name));
                parentNode.AppendChild(elementNode);
            }

            // Duration
            elementNode = document.CreateElement("Duration");
            Duration.Serialize(elementNode, document);
            parentNode.AppendChild(elementNode);

            // Intensity
            elementNode = document.CreateElement("Intensity");
            elementNode.AppendChild(document.CreateTextNode(Constants.StepIntensityZoneTCXString[IsRestingStep ? 1 : 0]));
            parentNode.AppendChild(elementNode);

            // Target
            elementNode = document.CreateElement("Target");
            Target.Serialize(elementNode, document);
            parentNode.AppendChild(elementNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                bool durationLoaded = false;
                bool targetLoaded = false;
                bool intensityLoaded = false;

                for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode child = parentNode.ChildNodes[i];

                    if (child.Name == "Name")
                    {
                        Name = child.FirstChild.Value;
                    }
                    else if (child.Name == "Duration")
                    {
                        if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
                        {
                            string stepTypeString = child.Attributes[0].Value;

                            for (int j = 0; j < (int)IDuration.DurationType.DurationTypeCount; ++j)
                            {
                                if (stepTypeString == Constants.DurationTypeTCXString[j])
                                {
                                    DurationFactory.Create((IDuration.DurationType)j, child, this);

                                    if (Duration != null)
                                    {
                                        durationLoaded = true;
                                        break;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (child.Name == "Target")
                    {
                        if (child.Attributes.Count == 1 && child.Attributes[0].Name == "xsi:type")
                        {
                            string stepTypeString = child.Attributes[0].Value;

                            for (int j = 0; j < (int)ITarget.TargetType.TargetTypeCount; ++j)
                            {
                                if (stepTypeString == Constants.TargetTypeTCXString[j])
                                {
                                    TargetFactory.Create((ITarget.TargetType)j, child, this);

                                    if (Target != null)
                                    {
                                        targetLoaded = true;
                                        break;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                    else if (child.Name == "Intensity")
                    {
                        IsRestingStep = child.FirstChild.Value == Constants.StepIntensityZoneTCXString[1];
                        intensityLoaded = true;
                    }
                }

                if (durationLoaded && targetLoaded && intensityLoaded)
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

            return new RegularStep(stream, Constants.CurrentVersion, ParentWorkout);
        }

        public void HandleTargetOverride(XmlNode extensionNode)
        {
            Target.HandleTargetOverride(extensionNode);
        }

        public override bool ValidateAfterZoneCategoryChanged(IZoneCategory changedCategory)
        {
            bool valueChanged = false;

            // We have to check the target if it's a ST zone and if so,
            //  make sure the zone is still valid
            switch (Target.Type)
            {
                case ITarget.TargetType.Cadence:
                    {
                        BaseCadenceTarget baseTarget = (BaseCadenceTarget)Target;

                        if (baseTarget.ConcreteTarget.Type == BaseCadenceTarget.IConcreteCadenceTarget.CadenceTargetType.ZoneST)
                        {
                            CadenceZoneSTTarget concreteTarget = (CadenceZoneSTTarget)baseTarget.ConcreteTarget;

                            if (!Utils.NamedZoneStillExists(PluginMain.GetApplication().Logbook.CadenceZones, concreteTarget.Zone))
                            {
                                // Revert zone to a valid default zone
                                concreteTarget.Zone = Options.CadenceZoneCategory.Zones[0];

                                // Mark as dirty
                                concreteTarget.IsDirty = true;

                                valueChanged = true;
                            }
                        }
                        break;
                    }
                case ITarget.TargetType.HeartRate:
                    {
                        BaseHeartRateTarget baseTarget = (BaseHeartRateTarget)Target;

                        if (baseTarget.ConcreteTarget.Type == BaseHeartRateTarget.IConcreteHeartRateTarget.HeartRateTargetType.ZoneST)
                        {
                            HeartRateZoneSTTarget concreteTarget = (HeartRateZoneSTTarget)baseTarget.ConcreteTarget;

                            if(!Utils.NamedZoneStillExists(PluginMain.GetApplication().Logbook.HeartRateZones, concreteTarget.Zone))
                            {
                                // Revert zone to a valid default zone
                                concreteTarget.Zone = ParentWorkout.Category.HeartRateZone.Zones[0];

                                // Mark as dirty
                                concreteTarget.IsDirty = true;

                                valueChanged = true;
                            }
                        }
                        break;
                    }
                case ITarget.TargetType.Speed:
                    {
                        BaseSpeedTarget baseTarget = (BaseSpeedTarget)Target;

                        if (baseTarget.ConcreteTarget.Type == BaseSpeedTarget.IConcreteSpeedTarget.SpeedTargetType.ZoneST)
                        {
                            SpeedZoneSTTarget concreteTarget = (SpeedZoneSTTarget)baseTarget.ConcreteTarget;

                            if(!Utils.NamedZoneStillExists(PluginMain.GetApplication().Logbook.SpeedZones, concreteTarget.Zone))
                            {
                                // Revert zone to a valid default zone
                                concreteTarget.Zone = ParentWorkout.Category.SpeedZone.Zones[0];

                                // Mark as dirty
                                concreteTarget.IsDirty = true;

                                valueChanged = true;
                            }
                        }
                        break;
                    }
                case ITarget.TargetType.Power:
                    {
                        BasePowerTarget baseTarget = (BasePowerTarget)Target;

                        if (baseTarget.ConcreteTarget.Type == BasePowerTarget.IConcretePowerTarget.PowerTargetType.ZoneST)
                        {
                            PowerZoneSTTarget concreteTarget = (PowerZoneSTTarget)baseTarget.ConcreteTarget;

                            if(!Utils.NamedZoneStillExists(PluginMain.GetApplication().Logbook.PowerZones, concreteTarget.Zone))
                            {
                                // Revert zone to a valid default zone
                                concreteTarget.Zone = Options.PowerZoneCategory.Zones[0];

                                // Mark as dirty
                                concreteTarget.IsDirty = true;

                                valueChanged = true;
                            }
                        }
                        break;
                    }
            }

            return valueChanged;
        }

        public IDuration Duration
        {
            get { return m_Duration; }
            set
            {
                if (m_Duration != value)
                {
                    if (m_Duration != null)
                    {
                        m_Duration.DurationChanged -= new IDuration.DurationChangedEventHandler(OnDurationChanged);
                    }

                    m_Duration = value;
                    m_Duration.DurationChanged += new IDuration.DurationChangedEventHandler(OnDurationChanged);

                    TriggerStepChanged( new PropertyChangedEventArgs("Duration"));
                }
            }
        }

        public ITarget Target
        {
            get { return m_Target; }
            set
            {
                if (m_Target != value)
                {
                    if (m_Target != null)
                    {
                        m_Target.TargetChanged -= new ITarget.TargetChangedEventHandler(OnTargetChanged);
                    }

                    m_Target = value;
                    m_Target.TargetChanged += new ITarget.TargetChangedEventHandler(OnTargetChanged);

                    TriggerStepChanged(new PropertyChangedEventArgs("Target"));
                }
            }
        }

        public string Name
        {
            get { return m_Name; }
            set
            {
                if (m_Name != value)
                {
                    Trace.Assert(value.Length <= 15);

                    m_Name = value;

                    TriggerStepChanged( new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return Target.IsDirty; }
            set { Trace.Assert(false); }
        }

        public bool IsRestingStep
        {
            get { return m_IsRestingStep; }
            set
            {
                if (m_IsRestingStep != value)
                {
                    m_IsRestingStep = value;
                    
                    TriggerStepChanged( new PropertyChangedEventArgs("IsRestingStep"));
                }
            }
        }

        private IDuration m_Duration;
        private ITarget m_Target;
        private string m_Name;
        private bool m_IsRestingStep = false;
    }
}

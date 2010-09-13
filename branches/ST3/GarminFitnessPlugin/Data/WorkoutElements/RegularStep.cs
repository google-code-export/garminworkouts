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

            m_Name.Serialize(stream);
            m_IsRestingStep.Serialize(stream);
            Duration.Serialize(stream);
            Target.Serialize(stream);
        }

        public override void SerializetoFIT(Stream stream)
        {
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];

            m_Name.Deserialize(stream, version);
            m_IsRestingStep.Deserialize(stream, version);

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

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            bool addNodeToExtensions = false;

            if (Target.Type == ITarget.TargetType.Power)
            {
                // Power was added to the format as an extension which gives me a headache
                //  So we need to serialize this target as having a NullTarget and then
                //  add a step extension that uses the real target with power info
                ITarget realTarget = Target;

                // Create the fake target
                TargetFactory.Create(ITarget.TargetType.Null, this);
                Serialize(parentNode, nodeName, document);

                // Remove the step extension that was added so there's no duplicate
                ParentWorkout.STExtensions.RemoveAt(ParentWorkout.STExtensions.Count - 1);

                // Restore old target
                Target = realTarget;

                // Create new parent node and add it to the extensions
                nodeName = "Step";
                addNodeToExtensions = true;
            }

            // Ok now this the real stuff but the target can either be the fake one or the real one
            base.Serialize(parentNode, nodeName, document);

            if (Name != String.Empty && Name != null)
            {
                m_Name.Serialize(parentNode.LastChild, "Name", document);
            }

            // Duration
            Duration.Serialize(parentNode.LastChild, "Duration", document);

            // Intensity
            m_IsRestingStep.Serialize(parentNode.LastChild, "Intensity", document);

            // Target
            Target.Serialize(parentNode.LastChild, "Target", document);

            if (addNodeToExtensions)
            {
                XmlNode extensionNode = parentNode.LastChild;

                parentNode.RemoveChild(extensionNode);
                ParentWorkout.AddStepExtension(extensionNode);
            }
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            bool durationLoaded = false;
            bool targetLoaded = false;
            bool intensityLoaded = false;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.Name == "Name")
                {
                    m_Name.Deserialize(child);
                }
                else if (child.Name == "Duration")
                {
                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString)
                    {
                        string stepTypeString = child.Attributes[0].Value;

                        for (int j = 0; j < (int)IDuration.DurationType.DurationTypeCount; ++j)
                        {
                            if (stepTypeString == Constants.DurationTypeTCXString[j])
                            {
                                DurationFactory.Create((IDuration.DurationType)j, child, this);
                                durationLoaded = true;
                                break;
                            }
                        }
                    }
                }
                else if (child.Name == "Target")
                {
                    if (child.Attributes.Count == 1 && child.Attributes[0].Name == Constants.XsiTypeTCXString)
                    {
                        string stepTypeString = child.Attributes[0].Value;

                        for (int j = 0; j < (int)ITarget.TargetType.TargetTypeCount; ++j)
                        {
                            if (stepTypeString == Constants.TargetTypeTCXString[j])
                            {
                                TargetFactory.Create((ITarget.TargetType)j, child, this);
                                targetLoaded = true;
                                break;
                            }
                        }
                    }
                }
                else if (child.Name == "Intensity")
                {
                    m_IsRestingStep.Deserialize(child);
                    intensityLoaded = true;
                }
            }

            if (!durationLoaded || !targetLoaded || !intensityLoaded)
            {
                throw new GarminFitnessXmlDeserializationException("Information missing in the XML node", parentNode);
            }
        }

        public override UInt32 Serialize(GarXFaceNet._Workout workout, UInt32 stepIndex)
        {
            GarXFaceNet._Workout._Step step = workout.GetStep(stepIndex);

            step.SetCustomName(Name);
            step.SetIntensity(this.IsRestingStep ? GarXFaceNet._Workout._Step.IntensityTypes.Rest : GarXFaceNet._Workout._Step.IntensityTypes.Active);

            Duration.Serialize(step);
            Target.Serialize(step);

            return stepIndex + 1;
        }

        public override void Deserialize(GarXFaceNet._Workout workout, UInt32 stepIndex)
        {
            GarXFaceNet._Workout._Step step = workout.GetStep(stepIndex);

            Name = step.GetCustomName();
            IsRestingStep = step.GetIntensity() == GarXFaceNet._Workout._Step.IntensityTypes.Rest;

            Duration.Deserialize(step);

            try
            {
                Target.Deserialize(step);
            }
            catch (NoDeviceSupportException)
            {
                // Unsupported target = power.  Replace with a null target
                new NullTarget(this).Serialize(step);
            }
        }

        public override IStep Clone()
        {
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(sizeof(Int32), SeekOrigin.Begin);

            return new RegularStep(stream, Constants.CurrentVersion, ParentConcreteWorkout);
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
                                concreteTarget.Zone = Options.Instance.CadenceZoneCategory.Zones[0];

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
                                concreteTarget.Zone = ParentConcreteWorkout.Category.HeartRateZone.Zones[0];

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
                                concreteTarget.Zone = ParentConcreteWorkout.Category.SpeedZone.Zones[0];

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
                                concreteTarget.Zone = Options.Instance.PowerZoneCategory.Zones[0];

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
                    m_Name.Value = value;

                    TriggerStepChanged( new PropertyChangedEventArgs("Name"));
                }
            }
        }

        public override bool IsDirty
        {
            get { return Target.IsDirty; }
            set { Debug.Assert(false); }
        }

        public bool IsRestingStep
        {
            get { return m_IsRestingStep; }
            set
            {
                if (m_IsRestingStep != value)
                {
                    m_IsRestingStep.Value = value;
                    
                    TriggerStepChanged( new PropertyChangedEventArgs("IsRestingStep"));
                }
            }
        }

        private IDuration m_Duration;
        private ITarget m_Target;
        private GarminFitnessString m_Name = new GarminFitnessString(String.Empty, Constants.MaxNameLength);
        private GarminFitnessBool m_IsRestingStep = new GarminFitnessBool(false, Constants.StepIntensityZoneTCXString[1], Constants.StepIntensityZoneTCXString[0]);
    }
}

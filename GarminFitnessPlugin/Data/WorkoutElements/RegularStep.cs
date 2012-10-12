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
    public class RegularStep : IStep
    {
        public enum StepIntensity
        {
            Active = 0,
            Rest,
            Warmup,
            Cooldown,
            IntensityCount
        };

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
            GarminFitnessByteRange intensity = new GarminFitnessByteRange((Byte)m_Intensity);
            base.Serialize(stream);

            m_Name.Serialize(stream);
            Duration.Serialize(stream);
            Target.Serialize(stream);
            intensity.Serialize(stream);
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            base.FillFITStepMessage(message);

            FITMessageField stepName = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.StepName);
            FITMessageField intensity = message.GetExistingOrAddField((Byte)FITWorkoutStepFieldIds.Intensity);

            if (!String.IsNullOrEmpty(Name))
            {
                stepName.SetString(Name, (Byte)(Constants.MaxNameLength + 1));
            }

            Duration.FillFITStepMessage(message);
            Target.FillFITStepMessage(message);

            intensity.SetEnum((Byte)Intensity);
        }

        public override void DeserializeFromFIT(FITMessage stepMessage)
        {
            FITMessageField nameField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.StepName);
            FITMessageField intensityField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.Intensity);

            if (nameField != null)
            {
                Name = nameField.GetString();
            }

            if (intensityField != null)
            {
                Intensity = (StepIntensity)intensityField.GetEnum();
            }

            Duration = DurationFactory.Create(stepMessage, this);
            Target = TargetFactory.Create(stepMessage, this);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            GarminFitnessBool isRestingStep = new GarminFitnessBool(false);
            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];

            m_Name.Deserialize(stream, version);
            isRestingStep.Deserialize(stream, version);

            if (isRestingStep)
            {
                m_Intensity = StepIntensity.Rest;
            }
            else
            {
                m_Intensity = StepIntensity.Active;
            }

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

        public void Deserialize_V17(Stream stream, DataVersion version)
        {
            GarminFitnessByteRange intensity = new GarminFitnessByteRange(0);

            // Call base deserialization
            Deserialize(typeof(IStep), stream, version);

            byte[] intBuffer = new byte[sizeof(Int32)];

            m_Name.Deserialize(stream, version);

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

            intensity.Deserialize(stream, Constants.CurrentVersion);
            m_Intensity = (StepIntensity)(Byte)intensity;
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
            GarminFitnessBool isRestingStep = new GarminFitnessBool(Intensity == StepIntensity.Active ||
                                                                    (Intensity == StepIntensity.Warmup && Options.Instance.TCXExportWarmupAs == StepIntensity.Active) ||
                                                                    (Intensity == StepIntensity.Cooldown && Options.Instance.TCXExportCooldownAs == StepIntensity.Active),
                                                                    Constants.StepIntensityZoneTCXString[0],
                                                                    Constants.StepIntensityZoneTCXString[1]);
            isRestingStep.Serialize(parentNode.LastChild, "Intensity", document);

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

            // Reset the name in case it's not in the XML (which means no name)
            m_Name.Value = "";

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes[i];

                if (child.Name == "Name")
                {
                    m_Name.Deserialize(child);
                }
                else if (child.Name == "Duration")
                {
                    durationLoaded = (DurationFactory.Create(child, this) != null);
                }
                else if (child.Name == "Target")
                {
                    targetLoaded = (TargetFactory.Create(child, this) != null);
                }
                else if (child.Name == "Intensity")
                {
                    GarminFitnessBool isActiveStep = new GarminFitnessBool(false,
                                                                            Constants.StepIntensityZoneTCXString[0],
                                                                            Constants.StepIntensityZoneTCXString[1]);
                    isActiveStep.Deserialize(child);

                    if (isActiveStep)
                    {
                        Intensity = StepIntensity.Active;
                    }
                    else
                    {
                        Intensity = StepIntensity.Rest;
                    }

                    intensityLoaded = true;
                }
            }

            if (!durationLoaded || !targetLoaded || !intensityLoaded)
            {
                throw new GarminFitnessXmlDeserializationException("Information missing in the XML node", parentNode);
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

        protected void TriggerDurationChangedEvent(IDuration duration, PropertyChangedEventArgs args)
        {
            Debug.Assert(Type == StepType.Regular);

            if (DurationChanged != null)
            {
                DurationChanged((RegularStep)this, duration, args);
            }
        }

        protected void TriggerTargetChangedEvent(ITarget target, PropertyChangedEventArgs args)
        {
            Debug.Assert(Type == StepType.Regular);

            if (TargetChanged != null)
            {
                TargetChanged((RegularStep)this, target, args);
            }
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
                    Debug.Assert(m_Duration != null);

                    m_Duration.DurationChanged += new IDuration.DurationChangedEventHandler(OnDurationChanged);

                    TriggerStepChanged(new PropertyChangedEventArgs("Duration"));
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

        public StepIntensity Intensity
        {
            get { return m_Intensity; }
            set
            {
                if (m_Intensity != value)
                {
                    m_Intensity = value;

                    TriggerStepChanged(new PropertyChangedEventArgs("Intensity"));
                }
            }
        }

        public override bool ContainsFITOnlyFeatures
        {
            get
            {
                return Target.ContainsFITOnlyFeatures ||
                       Duration.ContainsFITOnlyFeatures;
            }
        }

        public override bool ContainsTCXExtensionFeatures
        {
            get
            {
                return Target.ContainsTCXExtensionFeatures ||
                       Duration.ContainsTCXExtensionFeatures;
            }
        }

        public delegate void StepDurationChangedEventHandler(RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event StepDurationChangedEventHandler DurationChanged;

        public delegate void StepTargetChangedEventHandler(RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty);
        public event StepTargetChangedEventHandler TargetChanged;

        private IDuration m_Duration = null;
        private ITarget m_Target = null;
        private GarminFitnessString m_Name = new GarminFitnessString(String.Empty, Constants.MaxNameLength);
        private StepIntensity m_Intensity = StepIntensity.Active;
    }
}

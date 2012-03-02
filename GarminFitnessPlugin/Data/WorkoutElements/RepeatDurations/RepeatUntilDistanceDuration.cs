using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    public class RepeatUntilDistanceDuration : IRepeatDuration
    {
        public RepeatUntilDistanceDuration(RepeatStep parent)
            : base(RepeatDurationType.RepeatUntilDistance, parent)
        {
            SetDistanceInBaseUnit(1);
        }

        public RepeatUntilDistanceDuration(double distanceToGo, Length.Units distanceUnit, RepeatStep parent)
            : this(parent)
        {
            SetDistanceInUnits(distanceToGo, distanceUnit);
        }

        public RepeatUntilDistanceDuration(Stream stream, DataVersion version, RepeatStep parent)
            : this(parent)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_Distance.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IRepeatDuration), stream, version);

            m_Distance.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            // XML not supported
            throw new NotSupportedException();
        }

        public override void Deserialize(XmlNode parentNode)
        {
            // XML not supported
            throw new NotSupportedException();
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            FITMessageField durationType = new FITMessageField((Byte)FITWorkoutStepFieldIds.DurationType);
            FITMessageField repeatPower = new FITMessageField((Byte)FITWorkoutStepFieldIds.TargetValue);

            durationType.SetEnum((Byte)FITWorkoutStepDurationTypes.RepeatUntilDistance);
            message.AddField(durationType);
            repeatPower.SetUInt32((UInt32)GetDistanceInUnits(Length.Units.Centimeter));
            message.AddField(repeatPower);
        }

        public double GetDistanceInBaseUnit()
        {
            return GetDistanceInUnits(BaseUnit);
        }

        public void SetDistanceInBaseUnit(double distanceToGo)
        {
            SetDistanceInUnits(distanceToGo, BaseUnit);
        }

        private double GetDistanceInUnits(Length.Units distanceUnit)
        {
            return Length.Convert(Distance, Length.Units.Meter, distanceUnit);
        }

        private void SetDistanceInUnits(double distanceToGo, Length.Units distanceUnit)
        {
            distanceToGo = Length.Convert(distanceToGo, distanceUnit, Length.Units.Meter);

            Debug.Assert(distanceToGo >= Constants.MinDistanceMeters && distanceToGo <= Constants.MaxDistanceMeters);

            Distance = distanceToGo;
        }

        public override bool ContainsFITOnlyFeatures
        {
            get { return true; }
        }

        private double Distance
        {
            get { return m_Distance; }
            set
            {
                if (Distance != value)
                {
                    m_Distance.Value = value;

                    TriggerDurationChangedEvent(new PropertyChangedEventArgs("Distance"));
                }
            }
        }

        public Length.Units BaseUnit
        {
            get
            {
                if (Utils.IsStatute(ParentStep.ParentConcreteWorkout.Category.DistanceUnits))
                {
                    return Length.Units.Mile;
                }
                else
                {
                    return Length.Units.Kilometer;
                }
            }
        }

        private GarminFitnessDoubleRange m_Distance = new GarminFitnessDoubleRange(Constants.MinDistanceMeters, Constants.MinDistanceMeters, Constants.MaxDistanceMeters);
    }
}

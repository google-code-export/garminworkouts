using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Measurement;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class DistanceDuration : IDuration
    {
        public DistanceDuration(IStep parent)
            : base(DurationType.Distance, parent)
        {
            SetDistanceInBaseUnit(1);
        }

        public DistanceDuration(double distanceToGo, Length.Units distanceUnit, IStep parent)
            : this(parent)
        {
            SetDistanceInUnits(distanceToGo, distanceUnit);
        }

        public DistanceDuration(Stream stream, DataVersion version, IStep parent)
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
            Deserialize(typeof(IDuration), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];

            stream.Read(doubleBuffer, 0, sizeof(double));
            stream.Read(intBuffer, 0, sizeof(Int32));

            SetDistanceInUnits(BitConverter.ToDouble(doubleBuffer, 0),
                               (Length.Units)BitConverter.ToInt32(intBuffer, 0));
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(IDuration), stream, version);

            m_Distance.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            GarminFitnessUInt16Range distanceInMeters = new GarminFitnessUInt16Range((UInt16)Distance);
            distanceInMeters.Serialize(parentNode, "Meters", document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            if (parentNode.ChildNodes.Count != 1 || parentNode.FirstChild.Name != "Meters")
            {
                throw new GarminFitnessXmlDeserializationException("Missing information in distance duration XML node", parentNode);
            }

            m_Distance.Deserialize(parentNode.FirstChild);
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetDurationType(GarXFaceNet._Workout._Step.DurationTypes.Distance);
            step.SetDurationValue((UInt16)Math.Round(Distance, 0));
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            Distance = step.GetDurationValue();
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

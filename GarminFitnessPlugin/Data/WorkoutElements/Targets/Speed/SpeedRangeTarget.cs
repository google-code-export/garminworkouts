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
    class SpeedRangeTarget : BaseSpeedTarget.IConcreteSpeedTarget
    {
        public SpeedRangeTarget(BaseSpeedTarget baseTarget)
            : base(SpeedTargetType.Range, baseTarget)
        {
            SetRangeInBaseUnitsPerHour(15, 25);
        }

        public SpeedRangeTarget(double minUnitsPerHour, double maxUnitsPerHour, Length.Units speedUnit, Speed.Units speedPace, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            if (speedPace == Speed.Units.Pace)
            {
                SetRangeInMinutesPerUnit(minUnitsPerHour, maxUnitsPerHour, speedUnit);
            }
            else
            {
                SetRangeInUnitsPerHour(minUnitsPerHour, maxUnitsPerHour, speedUnit);
            }
        }

        public SpeedRangeTarget(Stream stream, DataVersion version, BaseSpeedTarget baseTarget)
            : this(baseTarget)
        {
            Deserialize(stream, version);
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            m_MinMetersPerSecond.Serialize(stream);
            m_MaxMetersPerSecond.Serialize(stream);
        }

        public new void Deserialize_V0(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, (Length.Units)BitConverter.ToInt32(intBuffer, 0));
        }

        public void Deserialize_V1(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;
            Length.Units speedUnit;
            Speed.Units speedOrPace;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedUnit = (Length.Units)BitConverter.ToInt32(intBuffer, 0);

            // This is deprecated in version 0.2.58, should be removed from next data version
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedOrPace = (Speed.Units)BitConverter.ToInt32(intBuffer, 0);

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, speedUnit);
        }

        public void Deserialize_V6(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            byte[] doubleBuffer = new byte[sizeof(double)];
            byte[] intBuffer = new byte[sizeof(Int32)];
            double minSpeed;
            double maxSpeed;
            Length.Units speedUnit;

            stream.Read(doubleBuffer, 0, sizeof(double));
            minSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(doubleBuffer, 0, sizeof(double));
            maxSpeed = BitConverter.ToDouble(doubleBuffer, 0) * Constants.SecondsPerHour;
            stream.Read(intBuffer, 0, sizeof(Int32));
            speedUnit = (Length.Units)BitConverter.ToInt32(intBuffer, 0);

            SetRangeInUnitsPerHour(minSpeed, maxSpeed, speedUnit);
        }

        public void Deserialize_V10(Stream stream, DataVersion version)
        {
            // Call base deserialization
            Deserialize(typeof(BaseSpeedTarget.IConcreteSpeedTarget), stream, version);

            m_MinMetersPerSecond.Deserialize(stream, version);
            m_MaxMetersPerSecond.Deserialize(stream, version);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            base.Serialize(parentNode, nodeName, document);

            // This node was added by our parent...
            parentNode = parentNode.LastChild;

            XmlNode valueNode;
            XmlAttribute attribute;

            // Type
            attribute = document.CreateAttribute(Constants.XsiTypeTCXString, Constants.xsins);
            attribute.Value = "CustomSpeedZone_t";
            parentNode.Attributes.Append(attribute);

            // View as
            valueNode = document.CreateElement(Constants.ViewAsTCXString);
            valueNode.AppendChild(document.CreateTextNode(Constants.SpeedOrPaceTCXString[ViewAsPace ? 0 : 1]));
            parentNode.AppendChild(valueNode);

            m_MinMetersPerSecond.Serialize(parentNode, Constants.LowInMeterPerSecTCXString, document);

            m_MaxMetersPerSecond.Serialize(parentNode, Constants.HighInMeterPerSecTCXString, document);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            base.Deserialize(parentNode);

            double minSpeed = 0;
            double maxSpeed = 0;

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode valueNode = parentNode.ChildNodes[i];
                CultureInfo culture = new CultureInfo("en-us");

                // We purposefully ignore the "View as" tag since we derive this from the category

                if (valueNode.Name == Constants.LowInMeterPerSecTCXString &&
                    valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                {
                    if (!double.TryParse(valueNode.FirstChild.Value, NumberStyles.Float, culture.NumberFormat, out minSpeed))
                    {
                    }
                }
                else if (valueNode.Name == Constants.HighInMeterPerSecTCXString &&
                    valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                {
                    if (!double.TryParse(valueNode.FirstChild.Value, NumberStyles.Float, culture.NumberFormat, out maxSpeed))
                    {
                    }
                }
            }

            if (minSpeed > 0 && maxSpeed > 0)
            {
                if (minSpeed < maxSpeed)
                {
                    SetRangeInUnitsPerHour(minSpeed * Constants.SecondsPerHour,
                                           maxSpeed * Constants.SecondsPerHour,
                                           Length.Units.Meter);
                }
                else
                {
                    SetRangeInUnitsPerHour(maxSpeed * Constants.SecondsPerHour,
                                           minSpeed * Constants.SecondsPerHour,
                                           Length.Units.Meter);
                }
            }
        }

        public override void Serialize(GarXFaceNet._Workout._Step step)
        {
            step.SetTargetType(0);
            step.SetTargetValue(0);
            step.SetTargetCustomZoneLow((float)MinMetersPerSecond);
            step.SetTargetCustomZoneHigh((float)MaxMetersPerSecond);
        }

        public override void Deserialize(GarXFaceNet._Workout._Step step)
        {
            MinMetersPerSecond = step.GetTargetCustomZoneLow();
            MaxMetersPerSecond = step.GetTargetCustomZoneHigh();
        }

        public double GetMinSpeedInBaseUnitsPerHour()
        {
            return GetMinSpeedInUnitsPerHour(BaseUnit);
        }

        public double GetMaxSpeedInBaseUnitsPerHour()
        {
            return GetMaxSpeedInUnitsPerHour(BaseUnit);
        }

        public double GetMinSpeedInMinutesPerBaseUnit()
        {
            return GetMinSpeedInMinutesPerUnit(BaseUnit);
        }

        public double GetMaxSpeedInMinutesPerBaseUnit()
        {
            return GetMaxSpeedInMinutesPerUnit(BaseUnit);
        }

        public void SetMinSpeedInBaseUnitsPerHour(double minUnitsPerHour)
        {
            SetMinSpeedInUnitsPerHour(minUnitsPerHour, BaseUnit);
        }

        public void SetMaxSpeedInBaseUnitsPerHour(double maxUnitsPerHour)
        {
            SetMaxSpeedInUnitsPerHour(maxUnitsPerHour, BaseUnit);
        }

        public void SetMinSpeedInMinutesPerBaseUnit(double minMinutesPerUnit)
        {
            SetMinSpeedInMinutesPerUnit(minMinutesPerUnit, BaseUnit);
        }

        public void SetMaxSpeedInMinutesPerBaseUnit(double maxMinutesPerUnit)
        {
            SetMaxSpeedInMinutesPerUnit(maxMinutesPerUnit, BaseUnit);
        }

        public void SetRangeInBaseUnitsPerHour(double minUnitsPerHour, double maxUnitsPerHour)
        {
            SetRangeInUnitsPerHour(minUnitsPerHour, maxUnitsPerHour, BaseUnit);
        }

        public void SetRangeInMinutesPerBaseUnit(double minMinutesPerUnit, double maxMinutesPerUnit)
        {
            SetRangeInMinutesPerUnit(minMinutesPerUnit, maxMinutesPerUnit, BaseUnit);
        }

        private double GetMinSpeedInUnitsPerHour(Length.Units speedUnit)
        {
            return Length.Convert(MinMetersPerSecond, Length.Units.Meter, speedUnit) * Constants.SecondsPerHour;
        }

        private double GetMaxSpeedInUnitsPerHour(Length.Units speedUnit)
        {
            return Length.Convert(MaxMetersPerSecond, Length.Units.Meter, speedUnit) * Constants.SecondsPerHour;
        }

        private double GetMinSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(MinMetersPerSecond, Length.Units.Meter, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        private double GetMaxSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(MaxMetersPerSecond, Length.Units.Meter, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        private void SetMinSpeedInUnitsPerHour(double minUnitsPerHour, Length.Units speedUnit)
        {
            double minMetersPerHour = Length.Convert(minUnitsPerHour, speedUnit, Length.Units.Meter) / Constants.SecondsPerHour;
            MinMetersPerSecond = Utils.Clamp(minMetersPerHour, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
        }

        private void SetMaxSpeedInUnitsPerHour(double maxUnitsPerHour, Length.Units speedUnit)
        {
            double maxMetersPerHour = Length.Convert(maxUnitsPerHour, speedUnit, Length.Units.Meter) / Constants.SecondsPerHour;
            MaxMetersPerSecond = Utils.Clamp(maxMetersPerHour, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
        }

        private void SetMinSpeedInMinutesPerUnit(double minMinutesPerUnit, Length.Units speedUnit)
        {
            SetMinSpeedInUnitsPerHour(Utils.PaceToSpeed(minMinutesPerUnit), speedUnit);
        }

        private void SetMaxSpeedInMinutesPerUnit(double maxMinutesPerUnit, Length.Units speedUnit)
        {
            SetMaxSpeedInUnitsPerHour(Utils.PaceToSpeed(maxMinutesPerUnit), speedUnit);
        }

        private void SetRangeInUnitsPerHour(double minUnitsPerHour, double maxUnitsPerHour, Length.Units speedUnit)
        {
            SetMinSpeedInUnitsPerHour(minUnitsPerHour, speedUnit);
            SetMaxSpeedInUnitsPerHour(maxUnitsPerHour, speedUnit);
        }

        private void SetRangeInMinutesPerUnit(double minMinutesPerUnit, double maxMinutesPerUnit, Length.Units speedUnit)
        {
            // Convert to speed (units/hr)
            SetRangeInUnitsPerHour(Utils.PaceToSpeed(minMinutesPerUnit),
                                   Utils.PaceToSpeed(maxMinutesPerUnit),
                                   speedUnit);
        }

        private double MinMetersPerSecond
        {
            get { return m_MinMetersPerSecond; }
            set
            {
                if (MinMetersPerSecond != value)
                {
                    m_MinMetersPerSecond.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinMetersPerSecond"));
                }
            }
        }

        private double MaxMetersPerSecond
        {
            get { return m_MaxMetersPerSecond; }
            set
            {
                if (MaxMetersPerSecond != value)
                {
                    m_MaxMetersPerSecond.Value = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxMetersPerSecond"));
                }
            }
        }

        public Length.Units BaseUnit
        {
            get
            {
                if (Utils.IsStatute(BaseTarget.ParentStep.ParentConcreteWorkout.Category.DistanceUnits))
                {
                    return Length.Units.Mile;
                }
                else
                {
                    return Length.Units.Kilometer;
                }
            }
        }

        public override bool IsDirty
        {
            get { return false; }
            set { Debug.Assert(false); }
        }

        private GarminFitnessDoubleRange m_MinMetersPerSecond = new GarminFitnessDoubleRange(5, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
        private GarminFitnessDoubleRange m_MaxMetersPerSecond = new GarminFitnessDoubleRange(10, Constants.MinSpeedMetersPerSecond, Constants.MaxSpeedMetersPerSecond);
    }
}

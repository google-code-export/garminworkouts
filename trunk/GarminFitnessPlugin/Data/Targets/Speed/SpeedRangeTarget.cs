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

            stream.Write(BitConverter.GetBytes(MinUnitsPerSecond), 0, sizeof(double));
            stream.Write(BitConverter.GetBytes(MaxUnitsPerSecond), 0, sizeof(double));
            stream.Write(BitConverter.GetBytes((Int32)LastSpeedUnit), 0, sizeof(Int32));
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

        public override void Serialize(XmlNode parentNode, XmlDocument document)
        {
            base.Serialize(parentNode, document);

            CultureInfo culture = new CultureInfo("en-us");
            XmlNode valueNode;
            XmlAttribute attribute;

            // Type
            attribute = document.CreateAttribute("xsi", "type", Constants.xsins);
            attribute.Value = "CustomSpeedZone_t";
            parentNode.Attributes.Append(attribute);

            // View as
            valueNode = document.CreateElement("ViewAs");
            valueNode.AppendChild(document.CreateTextNode(Constants.SpeedOrPaceTCXString[ViewAsPace ? 0 : 1]));
            parentNode.AppendChild(valueNode);

            // Low
            valueNode = document.CreateElement("LowInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MinMetersPerSecond)));
            parentNode.AppendChild(valueNode);

            // High
            valueNode = document.CreateElement("HighInMetersPerSecond");
            valueNode.AppendChild(document.CreateTextNode(String.Format(culture.NumberFormat, "{0:0.00000}", MaxMetersPerSecond)));
            parentNode.AppendChild(valueNode);
        }

        public override bool Deserialize(XmlNode parentNode)
        {
            if (base.Deserialize(parentNode))
            {
                double minSpeed = 0;
                double maxSpeed = 0;

                for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
                {
                    XmlNode valueNode = parentNode.ChildNodes[i];
                    CultureInfo culture = new CultureInfo("en-us");

                    // We purposefully ignore the "View as" tag since we derive this from the category

                    if (valueNode.Name == "LowInMetersPerSecond" &&
                        valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        minSpeed = double.Parse(valueNode.FirstChild.Value, culture.NumberFormat);
                    }
                    else if (valueNode.Name == "HighInMetersPerSecond" &&
                        valueNode.ChildNodes.Count == 1 && valueNode.FirstChild.GetType() == typeof(XmlText))
                    {
                        maxSpeed = double.Parse(valueNode.FirstChild.Value, culture.NumberFormat);
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

                    return true;
                }
            }

            return false;
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
            return Length.Convert(MinUnitsPerSecond, LastSpeedUnit, speedUnit) * Constants.SecondsPerHour;
        }

        private double GetMaxSpeedInUnitsPerHour(Length.Units speedUnit)
        {
            return Length.Convert(MaxUnitsPerSecond, LastSpeedUnit, speedUnit) * Constants.SecondsPerHour;
        }

        private double GetMinSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(MinUnitsPerSecond, LastSpeedUnit, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        private double GetMaxSpeedInMinutesPerUnit(Length.Units speedUnit)
        {
            double unitsPerMinute = Length.Convert(MaxUnitsPerSecond, LastSpeedUnit, speedUnit) * Constants.SecondsPerMinute;
            return 1.0 / unitsPerMinute;
        }

        private void SetMinSpeedInUnitsPerHour(double minUnitsPerHour, Length.Units speedUnit)
        {
            double minInMiles = Length.Convert(minUnitsPerHour, speedUnit, Length.Units.Mile);

            Utils.Clamp(minInMiles, Constants.MinSpeedStatute, Constants.MaxSpeedStatute);

            minUnitsPerHour = Length.Convert(minInMiles, Length.Units.Mile, speedUnit);
            MinUnitsPerSecond = minUnitsPerHour / Constants.SecondsPerHour;

            if (speedUnit != LastSpeedUnit)
            {
                // Convert the old max
                MaxUnitsPerSecond = Length.Convert(MaxUnitsPerSecond, LastSpeedUnit, speedUnit);
            }

            LastSpeedUnit = speedUnit;
        }

        private void SetMaxSpeedInUnitsPerHour(double maxUnitsPerHour, Length.Units speedUnit)
        {
            double maxInMiles = Length.Convert(maxUnitsPerHour, speedUnit, Length.Units.Mile);

            Utils.Clamp(maxInMiles, Constants.MinSpeedStatute, Constants.MaxSpeedStatute);

            maxUnitsPerHour = Length.Convert(maxInMiles, Length.Units.Mile, speedUnit);
            MaxUnitsPerSecond = maxUnitsPerHour / Constants.SecondsPerHour;

            if (speedUnit != LastSpeedUnit)
            {
                // Convert the old max
                MinUnitsPerSecond = Length.Convert(MinUnitsPerSecond, LastSpeedUnit, speedUnit);
            }

            LastSpeedUnit = speedUnit;
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
            double minInMiles = Length.Convert(minUnitsPerHour, speedUnit, Length.Units.Mile);
            double maxInMiles = Length.Convert(maxUnitsPerHour, speedUnit, Length.Units.Mile);

            Utils.Clamp(minInMiles, Constants.MinSpeedStatute, Constants.MaxSpeedStatute);
            Utils.Clamp(maxInMiles, Constants.MinSpeedStatute, Constants.MaxSpeedStatute);

            minUnitsPerHour = Length.Convert(minInMiles, Length.Units.Mile, speedUnit);
            maxUnitsPerHour = Length.Convert(maxInMiles, Length.Units.Mile, speedUnit);

            MinUnitsPerSecond = minUnitsPerHour / Constants.SecondsPerHour;
            MaxUnitsPerSecond = maxUnitsPerHour / Constants.SecondsPerHour;
            LastSpeedUnit = speedUnit;
        }

        private void SetRangeInMinutesPerUnit(double minMinutesPerUnit, double maxMinutesPerUnit, Length.Units speedUnit)
        {
            // Convert to speed (units/hr)
            SetRangeInUnitsPerHour(Utils.PaceToSpeed(minMinutesPerUnit),
                                   Utils.PaceToSpeed(maxMinutesPerUnit),
                                   speedUnit);
        }

        private double MinUnitsPerSecond
        {
            get { return m_MinUnitsPerSecond; }
            set
            {
                if (m_MinUnitsPerSecond != value)
                {
                    m_MinUnitsPerSecond = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MinUnitsPerSecond"));
                }
            }
        }

        private double MaxUnitsPerSecond
        {
            get { return m_MaxUnitsPerSecond; }
            set
            {
                if (m_MaxUnitsPerSecond != value)
                {
                    m_MaxUnitsPerSecond = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("MaxUnitsPerSecond"));
                }
            }
        }

        private Length.Units LastSpeedUnit
        {
            get { return m_LastSpeedUnit; }
            set
            {
                if (m_LastSpeedUnit != value)
                {
                    m_LastSpeedUnit = value;

                    TriggerTargetChangedEvent(this, new PropertyChangedEventArgs("SpeedUnits"));
                }
            }
        }

        private double MinMetersPerSecond
        {
            get { return Length.Convert(MinUnitsPerSecond, LastSpeedUnit, Length.Units.Meter); }
        }

        private double MaxMetersPerSecond
        {
            get { return Length.Convert(MaxUnitsPerSecond, LastSpeedUnit, Length.Units.Meter); }
        }

        public Length.Units BaseUnit
        {
            get
            {
                if (Utils.IsStatute(BaseTarget.ParentStep.ParentWorkout.Category.DistanceUnits))
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
            set { Trace.Assert(false); }
        }

        private double m_MinUnitsPerSecond;
        private double m_MaxUnitsPerSecond;
        private Length.Units m_LastSpeedUnit;
    }
}

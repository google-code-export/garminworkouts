using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    public class FITMessage
    {
        public FITMessage(FITGlobalMessageIds messageType) :
            this(messageType, BitConverter.IsLittleEndian)
        {
        }

        public FITMessage(FITGlobalMessageIds messageType, bool isLittleEndian)
        {
            m_GlobalMessageType = messageType;
            m_LittleEndian = isLittleEndian;
        }

        public void Clear()
        {
            m_Fields.Clear();
        }

        public void AddField(FITMessageField field)
        {
            Debug.Assert(!m_Fields.ContainsKey(field.DefinitionNumber));

            m_Fields.Add(field.DefinitionNumber, field);
        }

        public void Serialize(Stream stream)
        {
            Serialize(stream, true);
        }

        public void Serialize(Stream stream, bool serializeDefiniton)
        {
            // Definition message
            GarminFitnessByteRange recordHeader = new GarminFitnessByteRange(0x40);

            if (serializeDefiniton)
            {
                GarminFitnessByteRange recordReservedData = new GarminFitnessByteRange(0x00);
                GarminFitnessByteRange endianness = new GarminFitnessByteRange(0x00);
                GarminFitnessUInt16Range messageNumber = new GarminFitnessUInt16Range((UInt16)m_GlobalMessageType);
                GarminFitnessByteRange fieldsCount = new GarminFitnessByteRange((Byte)m_Fields.Count);

                recordHeader.Serialize(stream);
                recordReservedData.Serialize(stream);
                endianness.Serialize(stream);
                messageNumber.Serialize(stream);
                fieldsCount.Serialize(stream);

                foreach (FITMessageField field in m_Fields.Values)
                {
                    field.SerializeDefinition(stream);
                }
            }

            // Data message
            recordHeader.Value = 0x00;
            recordHeader.Serialize(stream);

            foreach (FITMessageField field in m_Fields.Values)
            {
                field.SerializeData(stream);
            }
        }

        public void DeserializeDataMessage(Stream stream)
        {
            foreach (FITMessageField field in m_Fields.Values)
            {
                field.DeserializeData(stream, m_LittleEndian);
            }
        }

        public FITMessageField GetField(Byte fieldDefinitionNumber)
        {
            if (m_Fields.ContainsKey(fieldDefinitionNumber))
            {
                FITMessageField field = m_Fields[fieldDefinitionNumber];

                if (field.IsValueValid)
                {
                    return field;
                }
            }

            return null;
        }

        public FITMessageField GetExistingOrAddField(Byte fieldDefinitionNumber)
        {
            FITMessageField field;

            if (m_Fields.ContainsKey(fieldDefinitionNumber))
            {
                field = m_Fields[fieldDefinitionNumber];
            }
            else
            {
                // Create new field
                field = new FITMessageField(fieldDefinitionNumber);
                AddField(field);
            }

            return field;
        }

        public FITGlobalMessageIds GlobalMessageType
        {
            get { return m_GlobalMessageType; }
        }

        private bool m_LittleEndian = true;
        private FITGlobalMessageIds m_GlobalMessageType = 0;
        private Dictionary<Byte, FITMessageField> m_Fields = new Dictionary<Byte, FITMessageField>();
    }
}

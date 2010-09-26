using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class FITMessage
    {
        public FITMessage(FITGlobalMessageIds messageType)
        {
            m_GlobalMessageType = messageType;
        }

        public void AddField(FITMessageField field)
        {
            Debug.Assert(!m_Fields.ContainsKey(field.DefinitionNumber));

            m_Fields.Add(field.DefinitionNumber, field);
        }

        public void Serialize(Stream stream)
        {
            // Definition message
            GarminFitnessByteRange recordHeader = new GarminFitnessByteRange(0x40);
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

            // Data message
            recordHeader.Value = 0x00;
            recordHeader.Serialize(stream);

            foreach (FITMessageField field in m_Fields.Values)
            {
                field.SerializeData(stream);
            }
        }

        private FITGlobalMessageIds m_GlobalMessageType = 0;
        private Dictionary<Byte, FITMessageField> m_Fields = new Dictionary<Byte, FITMessageField>();
    }
}

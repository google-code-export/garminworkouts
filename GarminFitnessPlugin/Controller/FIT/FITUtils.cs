using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GarminFitnessPlugin.Controller
{
    public class FITUtils
    {
        public static UInt16 ComputeStreamCRC(MemoryStream dataStream)
        {
            UInt16 crc = 0;
            Byte[] streamBuffer = dataStream.GetBuffer();

            for (int i = 0; i < dataStream.Length; ++i)
            {
                crc = ComputeByteCRC(crc, streamBuffer[i]);
            }

            return crc;
        }

        private static UInt16 ComputeByteCRC(UInt16 previousCRC, Byte newByte)
        {
            UInt16[] crc_table = new UInt16[]  {
                0x0000, 0xCC01, 0xD801, 0x1400, 0xF001, 0x3C00, 0x2800, 0xE401,
                0xA001, 0x6C00, 0x7800, 0xB401, 0x5000, 0x9C01, 0x8801, 0x4400
            };

            UInt16 tmp;
            UInt16 crc = previousCRC;

            // compute checksum of lower four bits of byte
            tmp = crc_table[crc & 0xF];
            crc = (UInt16)((crc >> 4) & 0x0FFF);
            crc = (UInt16)(crc ^ tmp ^ crc_table[newByte & 0xF]);

            // now compute checksum of upper four bits of byte
            tmp = crc_table[crc & 0xF];
            crc = (UInt16)((crc >> 4) & 0x0FFF);
            crc = (UInt16)(crc ^ tmp ^ crc_table[(newByte >> 4) & 0xF]);

            return crc;
        }
    }
}

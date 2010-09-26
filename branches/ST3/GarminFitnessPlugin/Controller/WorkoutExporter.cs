using System;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.Controller
{
    class WorkoutExporter
    {
        public static void ExportWorkout(IWorkout workout, Stream exportStream)
        {
            List<IWorkout> workouts = new List<IWorkout>();

            workouts.Add(workout);

            ExportWorkouts(workouts, exportStream, false);
        }

        public static void ExportWorkouts(List<IWorkout> workouts, Stream exportStream)
        {
            ExportWorkouts(workouts, exportStream, false);
        }

        public static void ExportWorkoutToFIT(IWorkout workout, Stream exportStream)
        {
            List<IWorkout> workouts = new List<IWorkout>();

            workouts.Add(workout);

            ExportWorkoutsToFIT(workouts, exportStream, false);
        }

        public static void ExportWorkoutsToFIT(List<IWorkout> workouts, Stream exportStream)
        {
            ExportWorkoutsToFIT(workouts, exportStream, false);
        }

        private static void ExportWorkouts(List<IWorkout> workouts, Stream exportStream, bool skipExtensions)
        {
            Debug.Assert(exportStream.CanWrite && exportStream.Length == 0);
            XmlDocument document = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;
            List<IWorkout> concreteWorkouts = new List<IWorkout>();

            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            database = document.CreateNode(XmlNodeType.Element, "TrainingCenterDatabase", null);
            document.AppendChild(database);
            
            // xmlns namespace attribute
            attribute = document.CreateAttribute("xmlns");
            attribute.Value = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
            database.Attributes.Append(attribute);

            // xmlns:xsi namespace attribute
            attribute = document.CreateAttribute("xmlns", "xsi", Constants.xmlns);
            attribute.Value = Constants.xsins;
            database.Attributes.Append(attribute);

            // xsi:schemaLocation namespace attribute
            attribute = document.CreateAttribute("xsi", "schemaLocation", Constants.xsins);
            attribute.Value = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd";
            database.Attributes.Append(attribute);

            XmlNode workoutsNode = document.CreateNode(XmlNodeType.Element, "Workouts", null);
            database.AppendChild(workoutsNode);

            foreach (IWorkout currentWorkout in workouts)
            {
                currentWorkout.LastExportDate = DateTime.Now;
                if (currentWorkout.GetSplitPartsCount() == 1)
                {
                    currentWorkout.Serialize(workoutsNode, "Workout", document);
                }
                else
                {
                    List<WorkoutPart> parts = currentWorkout.SplitInSeperateParts();

                    foreach (WorkoutPart part in parts)
                    {
                        part.Serialize(workoutsNode, "Workout", document);
                    }
                }
            }

            foreach (IWorkout concreteWorkout in concreteWorkouts)
            {
            }

            document.Save(new StreamWriter(exportStream));
        }

        private static void ExportWorkoutsToFIT(List<IWorkout> workouts, Stream exportStream, bool skipExtensions)
        {
            MemoryStream dataStream = new MemoryStream();

            // Reserve size for header
            dataStream.Write(new Byte[12], 0, 12);

            // File id message
            FITMessage fileIdMessage = new FITMessage(FITGlobalMessageIds.FileId);
            FITMessageField fileType = new FITMessageField((Byte)FITFileIdFieldsIds.FileType);
            FITMessageField manufacturerId = new FITMessageField((Byte)FITFileIdFieldsIds.ManufacturerId);
            FITMessageField productId = new FITMessageField((Byte)FITFileIdFieldsIds.ProductId);
            FITMessageField serialNumber = new FITMessageField((Byte)FITFileIdFieldsIds.SerialNumber);
            FITMessageField exportDate = new FITMessageField((Byte)FITFileIdFieldsIds.ExportDate);

            fileType.SetEnum((Byte)FITFileIds.Workout);
            fileIdMessage.AddField(fileType);
            manufacturerId.SetUInt16(1);
            fileIdMessage.AddField(manufacturerId);
            productId.SetUInt16(20119);
            fileIdMessage.AddField(productId);
            serialNumber.SetUInt32z(0);
            fileIdMessage.AddField(serialNumber);
            exportDate.SetUInt32((UInt32)(DateTime.Now - new DateTime(1989, 12, 31)).TotalSeconds);
            fileIdMessage.AddField(exportDate);

            fileIdMessage.Serialize(dataStream);

            // Write workouts
            // Write workouts file id
            foreach (IWorkout currentWorkout in workouts)
            {
                currentWorkout.LastExportDate = DateTime.Now;
                if (currentWorkout.GetSplitPartsCount() > 1)
                {
                    List<WorkoutPart> parts = currentWorkout.SplitInSeperateParts();

                    foreach (WorkoutPart part in parts)
                    {
                        part.SerializetoFIT(dataStream);
                    }
                }
                else
                {
                    currentWorkout.SerializetoFIT(dataStream);
                }
            }

            // Write FIT header at the start of the stream
            GarminFitnessByteRange headerSize = new GarminFitnessByteRange(12);
            GarminFitnessByteRange protocolVersion = new GarminFitnessByteRange((1 << 4) | (0));            // 1.0
            GarminFitnessUInt16Range profileVersion = new GarminFitnessUInt16Range((1 * 100) + (0));         // 1.0
            GarminFitnessInt32Range dataSize = new GarminFitnessInt32Range(0);
            String fitFormatName = ".FIT";

            dataStream.Seek(0, SeekOrigin.Begin);
            dataSize.Value = (int)dataStream.Length - 12;

            headerSize.Serialize(dataStream);
            protocolVersion.Serialize(dataStream);
            profileVersion.Serialize(dataStream);
            dataSize.Serialize(dataStream);
            dataStream.Write(Encoding.UTF8.GetBytes(fitFormatName), 0, 4);

            // Write CRC
            GarminFitnessUInt16Range crc = ComputeStreamCRC(dataStream);
            dataStream.Seek(0, SeekOrigin.End);
            crc.Serialize(dataStream);

            // Write all data to output stream
            exportStream.Write(dataStream.GetBuffer(), 0, (int)dataStream.Length);
        }

        private static GarminFitnessUInt16Range ComputeStreamCRC(MemoryStream dataStream)
        {
            UInt16 crc = 0;
            Byte[] streamBuffer = dataStream.GetBuffer();

            for (int i = 0; i < dataStream.Length; ++i)
            {
                crc = ComputeByteCRC(crc, streamBuffer[i]);
            }

            return new GarminFitnessUInt16Range(crc);
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

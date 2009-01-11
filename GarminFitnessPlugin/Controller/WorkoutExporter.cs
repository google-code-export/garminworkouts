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
        public static void ExportWorkout(Workout workout, Stream exportStream)
        {
            List<Workout> workouts = new List<Workout>();

            workouts.Add(workout);

            ExportWorkout(workouts, exportStream, false);
        }

        public static void ExportWorkout(List<Workout> workouts, Stream exportStream)
        {
            ExportWorkout(workouts, exportStream, false);
        }

        public static void ExportWorkout(List<Workout> workouts, Stream exportStream, bool skipExtensions)
        {
            Debug.Assert(exportStream.CanWrite && exportStream.Length == 0);
            XmlDocument document = new XmlDocument();
            XmlNode database;
            XmlAttribute attribute;

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

            for (int i = 0; i < workouts.Count; ++i)
            {
                ExportWorkoutInternal(workouts[i], document, workoutsNode);
            }

            document.Save(new StreamWriter(exportStream));
        }

        private static void ExportWorkoutInternal(Workout workout, XmlDocument document, XmlNode parentNode)
        {
            workout.LastExportDate = DateTime.Now;
            workout.Serialize(parentNode, "Workout", document);
        }
    }
}

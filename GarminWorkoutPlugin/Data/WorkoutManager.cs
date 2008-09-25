using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using System.Runtime.Serialization.Formatters.Binary;
using GarminWorkoutPlugin.Controller;

namespace GarminWorkoutPlugin.Data
{
    [Serializable()]

    class WorkoutManager : IPluginSerializable
    {
        private WorkoutManager()
        {
            PluginMain.ZoneCategoryChanged += new PluginMain.ZoneCategoryChangedEventHandler(OnZoneCategoryChanged);
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes(HeaderIdString), 0, Encoding.UTF8.GetByteCount(HeaderIdString));
            stream.WriteByte(Constants.CurrentVersion.VersionNumber);
            
            // Write the different options that are logbook related
            // Cadence
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Options.CadenceZoneCategory.ReferenceId)), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(Options.CadenceZoneCategory.ReferenceId), 0, Encoding.UTF8.GetByteCount(Options.CadenceZoneCategory.ReferenceId));

            // Cadence dirty flag
            stream.Write(BitConverter.GetBytes(Options.IsCadenceZoneDirty), 0, sizeof(bool));

            // Power
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Options.PowerZoneCategory.ReferenceId)), 0, sizeof(int));
            stream.Write(Encoding.UTF8.GetBytes(Options.PowerZoneCategory.ReferenceId), 0, Encoding.UTF8.GetByteCount(Options.PowerZoneCategory.ReferenceId));

            // Power dirty flag
            stream.Write(BitConverter.GetBytes(Options.IsPowerZoneDirty), 0, sizeof(bool));

            stream.Write(BitConverter.GetBytes(Workouts.Count), 0, sizeof(int));
            for (int i = 0; i < Workouts.Count; ++i)
            {
                Workouts[i].Serialize(stream);
            }
        }

        public void Deserialize(Stream stream)
        {
            byte[] headerBuffer = new byte[HeaderIdString.Length];
            String headerIdString;
            DataVersion version = null;

            stream.Read(headerBuffer, 0, HeaderIdString.Length);
            headerIdString = Encoding.UTF8.GetString(headerBuffer);

            if (headerIdString != HeaderIdString)
            {
                // Deserialize using version 0
                version = new DataVersion(0);
                stream.Position = 0;
            }
            else
            {
                Byte versionNumber = (Byte)stream.ReadByte();

                if (versionNumber <= Constants.CurrentVersion.VersionNumber)
                {
                    version = new DataVersion(versionNumber);
                }
                else
                {
                    throw new DataTooRecentException(versionNumber);
                }
            }

            Workouts.Clear();
            Deserialize(stream, version);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            int workoutCount;

            Options.CadenceZoneCategory = PluginMain.GetApplication().Logbook.CadenceZones[0];
            Options.PowerZoneCategory = PluginMain.GetApplication().Logbook.PowerZones[0];

            stream.Read(intBuffer, 0, sizeof(Int32));
            workoutCount = BitConverter.ToInt32(intBuffer, 0);
            for (int i = 0; i < workoutCount; ++i)
            {
                Workouts.Add(new Workout(stream, version));
            }
        }

        public void Deserialize_V2(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            int workoutCount;
            int stringLength;

            // Read options that are stored in logbook
            // Cadence zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Options.CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, Encoding.UTF8.GetString(stringBuffer));

            // Power zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Options.PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, Encoding.UTF8.GetString(stringBuffer));

            stream.Read(intBuffer, 0, sizeof(Int32));
            workoutCount = BitConverter.ToInt32(intBuffer, 0);
            for (int i = 0; i < workoutCount; ++i)
            {
                Workouts.Add(new Workout(stream, version));
            }
        }

        public void Deserialize_V3(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            int workoutCount;
            int stringLength;

            // Read options that are stored in logbook
            // Cadence zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Options.CadenceZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.CadenceZones, Encoding.UTF8.GetString(stringBuffer));

            // Cadence dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            Options.IsCadenceZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);

            // Power zone
            stream.Read(intBuffer, 0, sizeof(int));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Options.PowerZoneCategory = Utils.FindZoneCategoryByID(PluginMain.GetApplication().Logbook.PowerZones, Encoding.UTF8.GetString(stringBuffer));

            // Power dirty flag
            stream.Read(boolBuffer, 0, sizeof(bool));
            Options.IsPowerZoneDirty = BitConverter.ToBoolean(boolBuffer, 0);

            stream.Read(intBuffer, 0, sizeof(Int32));
            workoutCount = BitConverter.ToInt32(intBuffer, 0);
            for (int i = 0; i < workoutCount; ++i)
            {
                m_Workouts.Add(new Workout(stream, version));
            }
            m_Workouts.Sort(new WorkoutComparer());
        }

        public static WorkoutManager Instance
        {
            get { return m_Instance; }
        }

        public List<Workout> Workouts
        {
            get { return m_Workouts; }
        }

        public Workout CreateWorkout(IActivityCategory category)
        {
            const string baseName = "NewWorkout ";
            int workoutNumber = 1;
            Workout result;

            while(!IsWorkoutNameAvailable(baseName + workoutNumber.ToString()))
            {
                Trace.Assert(workoutNumber < 10000);
                workoutNumber++;
            }

            result = new Workout(baseName + workoutNumber.ToString(), category);
            m_Workouts.Add(result);
            m_Workouts.Sort(new WorkoutComparer());
            return result;
        }

        public Workout CreateWorkout(string name, IActivityCategory category, List<IStep> steps)
        {
            Workout result;
            string uniqueName;

            uniqueName = GetUniqueName(name);
            result = new Workout(uniqueName, category);
            result.Steps.AddRange(steps);
            m_Workouts.Add(result);
            m_Workouts.Sort(new WorkoutComparer());

            return result;
        }

        public Workout CreateWorkout(XmlNode workoutNode)
        {
            IActivityCategory category = Utils.GetDefaultCategory();

            Workout newWorkout = new Workout("Temp", category);

            if (newWorkout.Deserialize(workoutNode))
            {
                m_Workouts.Add(newWorkout);
                m_Workouts.Sort(new WorkoutComparer());

                return newWorkout;
            }

            return null;
        }

        public Workout GetWorkoutWithName(string name)
        {
            for (int i = 0; i < m_Workouts.Count; ++i)
            {
                if (m_Workouts[i].Name.ToLower().Equals(name.ToLower()))
                {
                    return m_Workouts[i];
                }
            }

            return null;
        }

        private bool IsWorkoutNameAvailable(string name)
        {
            return GetWorkoutWithName(name) == null;
        }

        public string GetUniqueName(string baseName)
        {
            if (IsWorkoutNameAvailable(baseName))
            {
                return baseName;
            }
            else
            {
                int workoutNumber = 1;

                if ((baseName + workoutNumber.ToString()).Length > 15)
                {
                    // We need to truncate the base name (we only remove 1 character at the time
                    //  since the numbers cannot grow faster than 1 char between 2 integers)
                    baseName = baseName.Substring(0, baseName.Length - 1);
                }

                while (!IsWorkoutNameAvailable(baseName + workoutNumber.ToString()))
                {
                    Trace.Assert(workoutNumber < 10000);

                    workoutNumber++;

                    if ((baseName + workoutNumber.ToString()).Length > 15)
                    {
                        // We need to truncate the base name (we only remove 1 character at the time
                        //  since the numbers cannot grow faster than 1 char between 2 integers)
                        baseName = baseName.Substring(0, baseName.Length - 1);
                    }
                }

                return baseName + workoutNumber.ToString();
            }
        }

        public static void CreateNewInstance(MemoryStream stream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                m_Instance = (WorkoutManager)formatter.Deserialize(stream);
            }
            catch(Exception e)
            {
                m_Instance = new WorkoutManager();

                throw e;
            }
        }

        void OnZoneCategoryChanged(object sender, IZoneCategory changedCategory)
        {
            bool valueChanged = false;

            for (int i = 0; i < m_Workouts.Count; ++i)
            {
                if (m_Workouts[i].ValidateAfterZoneCategoryChanged(changedCategory))
                {
                    valueChanged = true;
                }
            }

            if (valueChanged)
            {
                Utils.SaveWorkoutsToLogbook();
            }
        }

        public void MarkAllCadenceSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_Workouts.Count; ++i)
            {
                m_Workouts[i].MarkAllCadenceSTZoneTargetsAsDirty();
            }
        }

        public void MarkAllPowerSTZoneTargetsAsDirty()
        {
            for (int i = 0; i < m_Workouts.Count; ++i)
            {
                m_Workouts[i].MarkAllPowerSTZoneTargetsAsDirty();
            }
        }

        private class WorkoutComparer : IComparer<Workout>
        {
            #region IComparer<Workout> Members

            public int Compare(Workout x, Workout y)
            {
                return x.Name.CompareTo(y.Name);
            }

            #endregion
        }

        private static WorkoutManager m_Instance = new WorkoutManager();

        private List<Workout> m_Workouts = new List<Workout>();
        private const byte HeaderSize = 128;
        private const String HeaderIdString = "Garmin Workouts Plugin made by S->G";
    }
}

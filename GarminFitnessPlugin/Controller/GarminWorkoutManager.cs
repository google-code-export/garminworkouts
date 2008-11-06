using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using System.Runtime.Serialization.Formatters.Binary;
using GarminFitnessPlugin.Data;
using System.ComponentModel;

namespace GarminFitnessPlugin.Controller
{
    class GarminWorkoutManager : IPluginSerializable
    {
        private GarminWorkoutManager()
        {
            PluginMain.ZoneCategoryChanged += new PluginMain.ZoneCategoryChangedEventHandler(OnZoneCategoryChanged);
        }

        private void OnZoneCategoryChanged(object sender, IZoneCategory changedCategory)
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

        private void OnWorkoutChanged(Workout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            Utils.SaveWorkoutsToLogbook();

            if (m_EventTriggerActive && WorkoutChanged != null)
            {
                WorkoutChanged(modifiedWorkout, changedProperty);
            }
        }

        void OnWorkoutStepChanged(Workout modifiedWorkout, IStep modifiedStep, PropertyChangedEventArgs changedProperty)
        {
            Utils.SaveWorkoutsToLogbook();

            if (m_EventTriggerActive && WorkoutStepChanged != null)
            {
                WorkoutStepChanged(modifiedWorkout, modifiedStep, changedProperty);
            }
        }

        void OnStepDurationChanged(Workout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            Utils.SaveWorkoutsToLogbook();

            if (m_EventTriggerActive && WorkoutStepDurationChanged != null)
            {
                WorkoutStepDurationChanged(modifiedWorkout, modifiedStep, modifiedDuration, changedProperty);
            }
        }

        void OnStepTargetChanged(Workout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty)
        {
            Utils.SaveWorkoutsToLogbook();

            if (m_EventTriggerActive && WorkoutStepTargetChanged != null)
            {
                WorkoutStepTargetChanged(modifiedWorkout, modifiedStep, modifiedTarget, changedProperty);
            }
        }

        public override void Serialize(Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes(Constants.DataHeaderIdString), 0, Encoding.UTF8.GetByteCount(Constants.DataHeaderIdString));
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

            // Garmin to ST category map
            Dictionary<IActivityCategory, GarminCategories>.Enumerator iter = Options.STToGarminCategoryMap.GetEnumerator();
            stream.Write(BitConverter.GetBytes(Options.STToGarminCategoryMap.Count), 0, sizeof(int));
            while (iter.MoveNext())
            {
                // ST category Id
                stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(iter.Current.Key.ReferenceId)), 0, sizeof(int));
                stream.Write(Encoding.UTF8.GetBytes(iter.Current.Key.ReferenceId), 0, Encoding.UTF8.GetByteCount(iter.Current.Key.ReferenceId));

                // Mapped Garmin category
                stream.Write(BitConverter.GetBytes((int)iter.Current.Value), 0, sizeof(int));
            }

            stream.Write(BitConverter.GetBytes(Workouts.Count), 0, sizeof(int));
            for (int i = 0; i < Workouts.Count; ++i)
            {
                Workouts[i].Serialize(stream);
            }
        }

        public void Deserialize(Stream stream)
        {
            IsDeserializing = true;

            byte[] headerBuffer = new byte[Constants.DataHeaderIdString.Length];
            String headerIdString;
            DataVersion version = null;

            stream.Read(headerBuffer, 0, Constants.DataHeaderIdString.Length);
            headerIdString = Encoding.UTF8.GetString(headerBuffer);

            if (headerIdString != Constants.DataHeaderIdString)
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

            m_EventTriggerActive = false;
            {
                RemoveAllWorkouts();

                Deserialize(stream, version);

                m_EventTriggerActive = true;
            }
            IsDeserializing = false;

            // Trigger list changed event
            if (WorkoutListChanged != null)
            {
                // Workouts list changed
                WorkoutListChanged();
            }
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
                CreateWorkout(stream, version);
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
                CreateWorkout(stream, version);
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
                CreateWorkout(stream, version);
            }
        }

        public void Deserialize_V7(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] boolBuffer = new byte[sizeof(bool)];
            byte[] stringBuffer;
            int workoutCount;
            int mappingCount;
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

            // Load Garmin to ST category map
            stream.Read(intBuffer, 0, sizeof(int));
            mappingCount = BitConverter.ToInt32(intBuffer, 0);

            for (int i = 0; i < mappingCount; ++i)
            {
                int garminCategory;

                // ST category Id
                stream.Read(intBuffer, 0, sizeof(int));
                stringLength = BitConverter.ToInt32(intBuffer, 0);
                stringBuffer = new byte[stringLength];
                stream.Read(stringBuffer, 0, stringLength);

                // Mapped Garmin category
                stream.Read(intBuffer, 0, sizeof(int));
                garminCategory = BitConverter.ToInt32(intBuffer, 0);

                Options.STToGarminCategoryMap[Utils.FindCategoryByIDSafe(Encoding.UTF8.GetString(stringBuffer))] = (GarminCategories)garminCategory;
            }

            stream.Read(intBuffer, 0, sizeof(Int32));
            workoutCount = BitConverter.ToInt32(intBuffer, 0);
            for (int i = 0; i < workoutCount; ++i)
            {
                CreateWorkout(stream, version);
            }
        }

        public static GarminWorkoutManager Instance
        {
            get { return m_Instance; }
        }

        public List<Workout> Workouts
        {
            get { return m_Workouts; }
        }

        public Workout CreateWorkout(IActivityCategory category)
        {
            return CreateWorkout("NewWorkout 1", category, null);
        }

        public Workout CreateWorkout(Stream stream, DataVersion version, IActivityCategory newCategory)
        {
            Workout result = new Workout(stream, version);

            // We must update the name to avoid duplicates
            result.Name = GarminWorkoutManager.Instance.GetUniqueName(result.Name);

            if (newCategory != null)
            {
                result.Category = newCategory;
            }

            RegisterWorkout(result);

            return result;
        }

        public Workout CreateWorkout(Stream stream, DataVersion version)
        {
            return CreateWorkout(stream, version, null);
        }

        public Workout CreateWorkout(XmlNode workoutNode)
        {
            IActivityCategory category = Utils.GetDefaultCategory();

            Workout result = new Workout("Temp", category);

            if (!result.Deserialize(workoutNode))
            {
                return null;
            }

            // We must update the name to avoid duplicates
            result.Name = GarminWorkoutManager.Instance.GetUniqueName(result.Name);

            RegisterWorkout(result);

            return result;
        }

        public Workout CreateWorkout(string name, IActivityCategory category, List<IStep> steps)
        {
            Workout result;
            string uniqueName;

            uniqueName = GetUniqueName(name);
            result = new Workout(uniqueName, category);

            if (steps != null)
            {
                result.Steps.AddRange(steps);
            }

            RegisterWorkout(result);

            return result;
        }

        public List<Workout> CopyWorkouts(List<Workout> workoutsToMove, IActivityCategory newCategory)
        {
            List<Workout> newList;

            m_EventTriggerActive = false;
            {
                newList = new List<Workout>();

                for (int i = 0; i < workoutsToMove.Count; ++i)
                {
                    newList.Add(workoutsToMove[i].Clone(newCategory));
                }

                // Trigger event
                if (WorkoutListChanged != null)
                {
                    WorkoutListChanged();
                }

                m_EventTriggerActive = true;
            }

            return newList;
        }
 
        public void MoveWorkouts(List<Workout> workoutsToMove, IActivityCategory newCategory)
        {
            m_EventTriggerActive = false;
            {
                for (int i = 0; i < workoutsToMove.Count; ++i)
                {
                    workoutsToMove[i].Category = newCategory;
                }

                // Trigger event
                if (WorkoutListChanged != null)
                {
                    WorkoutListChanged();
                }

                m_EventTriggerActive = true;
            }
        }

        public void RemoveWorkout(Workout workoutToRemove)
        {
            List<Workout> workoutsToRemove = new List<Workout>();

            workoutsToRemove.Add(workoutToRemove);

            RemoveWorkouts(workoutsToRemove);
        }

        public void RemoveWorkouts(List<Workout> workoutsToRemove)
        {
            bool workoutRemoved = false;

            for (int i = 0; i < workoutsToRemove.Count; ++i)
            {
                if (m_Workouts.Contains(workoutsToRemove[i]))
                {
                    UnregisterWorkout(workoutsToRemove[i]);

                    m_Workouts.Remove(workoutsToRemove[i]);

                    workoutRemoved = true;
                }
            }

            // Trigger event
            if (workoutRemoved && m_EventTriggerActive && WorkoutListChanged != null)
            {
                WorkoutListChanged();
            }
        }

        public void RemoveAllWorkouts()
        {
            if (m_Workouts.Count > 0)
            {
                for (int i = 0; i < m_Workouts.Count; ++i)
                {
                    UnregisterWorkout(m_Workouts[i]);
                }

                m_Workouts.Clear();

                // Trigger event
                if (m_EventTriggerActive && WorkoutListChanged != null)
                {
                    WorkoutListChanged();
                }
            }
        }

        public List<Workout> DeserializeWorkouts(Stream stream, IActivityCategory category)
        {
            List<Workout> deserializedWorkouts = new List<Workout>();
            byte[] intBuffer = new byte[sizeof(Int32)];
            Byte workoutCount = (Byte)stream.ReadByte();

            m_EventTriggerActive = false;
            {
                for (int i = 0; i < workoutCount; i++)
                {
                    Workout newWorkout = GarminWorkoutManager.Instance.CreateWorkout(stream, Constants.CurrentVersion, category);

                    deserializedWorkouts.Add(newWorkout);
                }

                // Trigger event
                if (WorkoutListChanged != null)
                {
                    WorkoutListChanged();
                }

                m_EventTriggerActive = true;
            }

            return deserializedWorkouts;
        }

        private void RegisterWorkout(Workout workoutToRegister)
        {
            m_Workouts.Add(workoutToRegister);
            m_Workouts.Sort(new WorkoutComparer());

            // Register on workout events
            workoutToRegister.WorkoutChanged += new Workout.WorkoutChangedEventHandler(OnWorkoutChanged);
            workoutToRegister.StepChanged += new Workout.StepChangedEventHandler(OnWorkoutStepChanged);
            workoutToRegister.StepDurationChanged += new Workout.StepDurationChangedEventHandler(OnStepDurationChanged);
            workoutToRegister.StepTargetChanged += new Workout.StepTargetChangedEventHandler(OnStepTargetChanged);

            if (!IsDeserializing)
            {
                Utils.SaveWorkoutsToLogbook();
            }

            // Trigger event
            if (m_EventTriggerActive && WorkoutListChanged != null)
            {
                WorkoutListChanged();
            }
        }

        private void UnregisterWorkout(Workout workoutToUnregister)
        {
            // Unregister on workout events
            workoutToUnregister.WorkoutChanged -= new Workout.WorkoutChangedEventHandler(OnWorkoutChanged);
            workoutToUnregister.StepChanged -= new Workout.StepChangedEventHandler(OnWorkoutStepChanged);

            if (!IsDeserializing)
            {
                Utils.SaveWorkoutsToLogbook();
            }
        }

        public Workout GetWorkoutWithName(string name)
        {
            for (int i = 0; i < m_Workouts.Count; ++i)
            {
                // Not case-sensitive
                if (m_Workouts[i].Name.ToLower().Equals(name.ToLower()))
                {
                    return m_Workouts[i];
                }
            }

            return null;
        }

        public bool IsWorkoutNameAvailable(string name)
        {
            return GetWorkoutWithName(name) == null;
        }

        public bool IsWorkoutNameValid(string name)
        {
            Workout workoutWithSameName = GetWorkoutWithName(name);

            return name != String.Empty && workoutWithSameName == null;
        }

        public string GetUniqueName(string baseName)
        {
            if (IsWorkoutNameAvailable(baseName))
            {
                return baseName;
            }
            else
            {
                if (!Utils.IsTextInteger(baseName))
                {
                    // Remove all trailing numbers
                    while (baseName.LastIndexOfAny("0123456789".ToCharArray()) == baseName.Length - 1)
                    {
                        baseName = baseName.Substring(0, baseName.Length - 1);
                    }
                }

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

                m_Instance = (GarminWorkoutManager)formatter.Deserialize(stream);
            }
            catch(Exception e)
            {
                m_Instance = new GarminWorkoutManager();

                throw e;
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

        private bool IsDeserializing
        {
            get { return m_IsDeserializing; }
            set { m_IsDeserializing = value; }
        }

        public delegate void WorkoutListChangedEventHandler();
        public event WorkoutListChangedEventHandler WorkoutListChanged;

        public delegate void WorkoutChangedEventHandler(Workout modifiedWorkout, PropertyChangedEventArgs changedProperty);
        public event WorkoutChangedEventHandler WorkoutChanged;

        public delegate void WorkoutStepChangedEventHandler(Workout modifiedWorkout, IStep modifiedStep, PropertyChangedEventArgs changedProperty);
        public event WorkoutStepChangedEventHandler WorkoutStepChanged;

        public delegate void WorkoutStepDurationChangedEventHandler(Workout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event WorkoutStepDurationChangedEventHandler WorkoutStepDurationChanged;

        public delegate void WorkoutStepTargetChangedEventHandler(Workout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedDuration, PropertyChangedEventArgs changedProperty);
        public event WorkoutStepTargetChangedEventHandler WorkoutStepTargetChanged;

        private static GarminWorkoutManager m_Instance = new GarminWorkoutManager();

        private List<Workout> m_Workouts = new List<Workout>();
        private bool m_EventTriggerActive = true;
        private bool m_IsDeserializing = false;

        private const byte HeaderSize = 128;
    }
}

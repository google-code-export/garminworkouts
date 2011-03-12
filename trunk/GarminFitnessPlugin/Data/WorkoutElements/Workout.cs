using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Text;
using System.Xml;
using ZoneFiveSoftware.Common.Data.Fitness;
using GarminFitnessPublic;
using GarminFitnessPlugin.Controller;
using GarminFitnessPlugin.View;

namespace GarminFitnessPlugin.Data
{
    class Workout : IWorkout, IPublicWorkout, IDirty
    {
        public Workout(Guid workoutId, string name, IActivityCategory category) :
            this(name, category)
        {
            m_Id.Value = workoutId;

            CreateStepsList();

            WorkoutChanged += new WorkoutChangedEventHandler(OnWorkoutChanged);
        }

        public Workout(string name, IActivityCategory category)
        {
            m_Name.Value = name;
            Category = category;

            CreateStepsList();
            Steps.AddStepToRoot(new RegularStep(this));

            WorkoutChanged += new WorkoutChangedEventHandler(OnWorkoutChanged);
        }

        public Workout(string name, IActivityCategory category, List<IStep> steps)
        {
            m_Name.Value = name;
            Category = category;

            CreateStepsList();
            Steps.AddStepsToRoot(steps);

            WorkoutChanged += new WorkoutChangedEventHandler(OnWorkoutChanged);
        }

        public Workout(Stream stream, DataVersion version)
        {
            CreateStepsList();

            Deserialize(stream, version);

            WorkoutChanged += new WorkoutChangedEventHandler(OnWorkoutChanged);

            UpdateSplitsCache();
        }

#region IPublicWorkout Members

        Guid IPublicWorkout.Id
        {
            get { return new Guid(Id.ToString()); }
        }

        string IPublicWorkout.Name
        {
            get
            {
                return new string(((string)NameInternal).ToCharArray());
            }
        }

        IActivityCategory IPublicWorkout.Category
        {
            get { return Category; }
        }

        string IPublicWorkout.Notes
        {
            get { return new string(((string)NotesInternal).ToCharArray()); }
        }

        GarminSports IPublicWorkout.Sport
        {
            get { return (GarminSports)Options.Instance.GetGarminCategory(Category); }
        }

#endregion

        void OnStepAdded(IStep addedStep)
        {
            RegisterStep(addedStep);
        }

        void OnStepRemoved(IStep removedStep)
        {
            UnregisterStep(removedStep);
        }

        void OnStepsListChanged(object sender, PropertyChangedEventArgs args)
        {
            TriggerWorkoutChangedEvent(args);
        }

        void OnWorkoutChanged(IWorkout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            if (changedProperty.PropertyName == "PartsCount")
            {
                UpdateSplitsCache();
            }
        }

        public override void Serialize(Stream stream)
        {
            m_Id.Serialize(stream);
            m_Name.Serialize(stream);
            m_Notes.Serialize(stream);

            // Category ID
            stream.Write(BitConverter.GetBytes(Encoding.UTF8.GetByteCount(Category.ReferenceId)), 0, sizeof(Int32));
            stream.Write(Encoding.UTF8.GetBytes(Category.ReferenceId), 0, Encoding.UTF8.GetByteCount(Category.ReferenceId));

            Steps.Serialize(stream);

            // Scheduled dates
            stream.Write(BitConverter.GetBytes(ScheduledDates.Count), 0, sizeof(Int32));
            for (int i = 0; i < ScheduledDates.Count; ++i)
            {
                ScheduledDates[i].Serialize(stream);
            }

            m_LastExportDate.Serialize(stream);

            m_AddToDailyViewOnSchedule.Serialize(stream);
        }

        public void Deserialize_V0(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            byte[] stringBuffer;
            Int32 stringLength;

            m_Name.Deserialize(stream, version);
            m_Notes.Deserialize(stream, version);

            // Category
            stream.Read(intBuffer, 0, sizeof(Int32));
            stringLength = BitConverter.ToInt32(intBuffer, 0);
            stringBuffer = new byte[stringLength];
            stream.Read(stringBuffer, 0, stringLength);
            Category = Utils.FindCategoryByIDSafe(Encoding.UTF8.GetString(stringBuffer));

            // steps
            Steps.Deserialize(stream, version);
        }

        public void Deserialize_V4(Stream stream, DataVersion version)
        {
            byte[] intBuffer = new byte[sizeof(Int32)];
            Int32 scheduledDatesCount;

            Deserialize_V0(stream, version);

            // Scheduled dates
            stream.Read(intBuffer, 0, sizeof(Int32));
            scheduledDatesCount = BitConverter.ToInt32(intBuffer, 0);
            ScheduledDates.Clear();
            for (int i = 0; i < scheduledDatesCount; ++i)
            {
                GarminFitnessDate newDate = new GarminFitnessDate();

                newDate.Deserialize(stream, version);
                ScheduleWorkout(newDate);
            }
        }

        public void Deserialize_V5(Stream stream, DataVersion version)
        {
            Deserialize_V4(stream, version);

            m_LastExportDate.Deserialize(stream, version);
        }

        public void Deserialize_V12(Stream stream, DataVersion version)
        {
            Deserialize_V5(stream, version);

            m_AddToDailyViewOnSchedule.Deserialize(stream, version);
        }

        public void Deserialize_V13(Stream stream, DataVersion version)
        {
            m_Id.Deserialize(stream, version);

            // To fix a bug in vesion 1.1.269 where multiple workouts could have the same GUID.
            //  Since the GUID wasn't used, this only appeared later in 1.1.276, therefore we can
            //  safely override the GUID if data version is exactly 13
            if (version.VersionNumber == 13)
            {
                m_Id.Value = Guid.NewGuid();
            }

            Deserialize_V12(stream, version);
        }

        public override void Deserialize(XmlNode parentNode)
        {
            XmlNode STExtensionsNode = null;
            bool nameRead = false;

            ScheduledDates.Clear();

            for (int i = 0; i < parentNode.ChildNodes.Count; ++i)
            {
                XmlNode child = parentNode.ChildNodes.Item(i);

                if (child.Name == "Name")
                {
                    m_Name.Deserialize(child);
                    nameRead = true;
                }
                else if (child.Name == "Notes")
                {
                    m_Notes.Deserialize(child);
                }
                else if (child.Name == "ScheduledOn")
                {
                    GarminFitnessDate newDate = new GarminFitnessDate();

                    newDate.Deserialize(child);
                    ScheduleWorkout(newDate);
                }
                else if (child.Name == Constants.ExtensionsTCXString)
                {
                    for (int j = 0; j < child.ChildNodes.Count; ++j)
                    {
                        XmlNode currentNode = child.ChildNodes[j];

                        // This condition remains for backsward compatibility with V0 exports
                        if (currentNode.Name == "SportTracksCategory" &&
                            currentNode.FirstChild.ChildNodes.Count == 1 &&
                            currentNode.FirstChild.FirstChild.GetType() == typeof(XmlText))
                        {
                            XmlText categoryNode = (XmlText)child.FirstChild.FirstChild;

                            Category = Utils.FindCategoryByIDSafe(categoryNode.Value);
                        }
                        else if (currentNode.Name == "SportTracksExtensions")
                        {
                            STExtensionsNode = currentNode;
                        }
                    }
                }
            }

            if (!nameRead)
            {
                throw new GarminFitnessXmlDeserializationException("Information missing in the XML node", parentNode);
            }

            m_Name.Value = GarminWorkoutManager.Instance.GetUniqueName(Name);

            Steps.Deserialize(parentNode);

            if (STExtensionsNode != null)
            {
                HandleSTExtension(STExtensionsNode);
            }
        }

        public override void Deserialize(GarXFaceNet._Workout workout)
        {
            Steps.Clear();
            Category = null;

            Name = workout.GetName();

            Steps.Deserialize(workout);
        }

        public override void DeserializeOccurances(GarXFaceNet._WorkoutOccuranceList occuranceList)
        {
            ScheduledDates.Clear();

            for (UInt32 i = 0; i < occuranceList.GetCount(); ++i)
            {
                GarXFaceNet._WorkoutOccurance occurance = occuranceList.GetAtIndex(i);

                if (occurance.GetWorkoutName().Equals(Name))
                {
                    ScheduleWorkout(new DateTime(1989, 12, 31) + new TimeSpan(0, 0, (int)occurance.GetDay()));
                }
            }
        }

        public override void DeserializeFromFIT(FITMessage workoutMessage)
        {
            FITMessage stepMessage;
            FITMessageField numStepsField = workoutMessage.GetField((Byte)FITWorkoutFieldIds.NumSteps);

            if (numStepsField != null)
            {
                UInt16 numSteps = numStepsField.GetUInt16();

                m_Steps.Clear();

                do
                {
                    stepMessage = FITParser.Instance.ReadNextMessage();

                    if (stepMessage != null)
                    {
                        switch (stepMessage.GlobalMessageType)
                        {
                            case FITGlobalMessageIds.WorkoutStep:
                                {
                                    FITMessageField stepTypeField = stepMessage.GetField((Byte)FITWorkoutStepFieldIds.DurationType);

                                    if (stepTypeField != null)
                                    {
                                        FITWorkoutStepDurationTypes durationType = (FITWorkoutStepDurationTypes)stepTypeField.GetEnum();
                                        IStep newStep = null;

                                        switch (durationType)
                                        {
                                            case FITWorkoutStepDurationTypes.Calories:
                                            case FITWorkoutStepDurationTypes.Distance:
                                            case FITWorkoutStepDurationTypes.HeartRateGreaterThan:
                                            case FITWorkoutStepDurationTypes.HeartRateLessThan:
                                            case FITWorkoutStepDurationTypes.Open:
                                            case FITWorkoutStepDurationTypes.Time:
                                            case FITWorkoutStepDurationTypes.PowerGreaterThan:
                                            case FITWorkoutStepDurationTypes.PowerLessThan:
                                                {
                                                    newStep = new RegularStep(this);
                                                    break;
                                                }
                                            case FITWorkoutStepDurationTypes.RepeatCount:
                                            case FITWorkoutStepDurationTypes.RepeatUntilCalories:
                                            case FITWorkoutStepDurationTypes.RepeatUntilDistance:
                                            case FITWorkoutStepDurationTypes.RepeatUntilHeartRateGreaterThan:
                                            case FITWorkoutStepDurationTypes.RepeatUntilHeartRateLessThan:
                                            case FITWorkoutStepDurationTypes.RepeatUntilPowerGreaterThan:
                                            case FITWorkoutStepDurationTypes.RepeatUntilPowerLessThan:
                                            case FITWorkoutStepDurationTypes.RepeatUntilTime:
                                                {
                                                    newStep = new RepeatStep(this);
                                                    break;
                                                }
                                        }

                                        newStep.DeserializeFromFIT(stepMessage);
                                        m_Steps.AddStepToRoot(newStep);
                                    }
                                    else
                                    {
                                        throw new FITParserException("Missing duration type field");
                                    }

                                    break;
                                }
                            default:
                                {
                                    // Nothing to do
                                    break;
                                }
                        }
                    }
                }
                while (stepMessage != null && m_Steps.StepCount < numSteps);

                if (m_Steps.StepCount < numSteps)
                {
                    throw new FITParserException("Unable to deserialize all steps");
                }
            }
            else
            {
                throw new FITParserException("No step count field");
            }
        }

        private void CreateStepsList()
        {
            m_Steps = new WorkoutStepsList(this);
            m_Steps.StepAdded += new WorkoutStepsList.StepAddedEventHandler(OnStepAdded);
            m_Steps.StepRemoved += new WorkoutStepsList.StepRemovedEventHandler(OnStepRemoved);
            m_Steps.ListChanged += new PropertyChangedEventHandler(OnStepsListChanged);
        }

        public Workout Clone()
        {
            Workout result;
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(0, SeekOrigin.Begin);

            result = GarminWorkoutManager.Instance.CreateWorkout(stream, Constants.CurrentVersion);
            result.m_Id.Value = Guid.NewGuid();

            return result;
        }

        public Workout CloneUnregistered()
        {
            Workout result;
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(0, SeekOrigin.Begin);
            result = new Workout(stream, Constants.CurrentVersion);
            result.m_Id.Value = Guid.NewGuid();

            return result;
        }

        public Workout Clone(IActivityCategory newCategory)
        {
            Workout result;
            MemoryStream stream = new MemoryStream();

            Serialize(stream);

            // Put back at start but skip the first 4 bytes which are the step type
            stream.Seek(0, SeekOrigin.Begin);

            result = GarminWorkoutManager.Instance.CreateWorkout(stream,
                                                                 Constants.CurrentVersion,
                                                                 newCategory,
                                                                 Guid.NewGuid());

            stream.Close();

            return result;
        }


        private void HandleSTExtension(XmlNode extensionsNode)
        {
            for (int i = 0; i < extensionsNode.ChildNodes.Count; ++i)
            {
                XmlNode currentExtension = extensionsNode.ChildNodes[i];

                if (currentExtension.Name == "SportTracksCategory")
                {
                    if (currentExtension.ChildNodes.Count == 1 &&
                        currentExtension.FirstChild.GetType() == typeof(XmlText))
                    {
                        XmlText categoryNode = (XmlText)currentExtension.FirstChild;

                        Category = Utils.FindCategoryByIDSafe(categoryNode.Value);
                    }
                }
                else if (currentExtension.Name == "TargetOverride")
                {
                    XmlNode idNode = null;
                    XmlNode categoryNode = null;

                    for (int j = 0; j < currentExtension.ChildNodes.Count; ++j)
                    {
                        XmlNode childNode = currentExtension.ChildNodes[j];

                        if (childNode.Name == "StepId" && childNode.ChildNodes.Count == 1 &&
                            childNode.FirstChild.GetType() == typeof(XmlText) &&
                            Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 1, 20))
                        {
                            idNode = childNode.FirstChild;
                        }
                        else if (childNode.Name == "Category")
                        {
                            categoryNode = childNode;
                        }
                    }

                    if (idNode != null && categoryNode != null)
                    {
                        int stepId = Byte.Parse(idNode.Value);
                        IStep step = Steps.GetStepById(stepId);

                        if (step != null)
                        {
                            Debug.Assert(step.Type == IStep.StepType.Regular);

                            RegularStep concreteStep = (RegularStep)step;

                            concreteStep.HandleTargetOverride(categoryNode);
                        }
                    }
                }
                else if (currentExtension.Name == "StepNotes")
                {
                    XmlNode idNode = null;
                    XmlNode notesNode = null;

                    for (int j = 0; j < currentExtension.ChildNodes.Count; ++j)
                    {
                        XmlNode childNode = currentExtension.ChildNodes[j];

                        if (childNode.Name == "StepId" && childNode.ChildNodes.Count == 1 &&
                            childNode.FirstChild.GetType() == typeof(XmlText) &&
                            Utils.IsTextIntegerInRange(childNode.FirstChild.Value, 1, 20))
                        {
                            idNode = childNode.FirstChild;
                        }
                        else if (childNode.Name == "Notes" && childNode.ChildNodes.Count == 1 &&
                                 childNode.FirstChild.GetType() == typeof(XmlText))
                        {
                            notesNode = childNode;
                        }
                    }

                    if (idNode != null && notesNode != null)
                    {
                        int stepId = Byte.Parse(idNode.Value);
                        IStep step = Steps.GetStepById(stepId);

                        step.Notes = notesNode.FirstChild.Value;
                    }
                }
            }
        }

        public override UInt16 StepCount
        {
            get
            {
                UInt16 stepCount = 0;

                foreach (IStep currentStep in Steps)
                {
                    stepCount += currentStep.StepCount;
                }

                return stepCount;
            }
        }

        public List<WorkoutPart> GetSplitParts()
        {
            return m_SplitParts;
        }

        private void UpdateSplitsCache()
        {
            UInt16 partsCount = GetSplitPartsCount();

            if (partsCount > 1)
            {
                if (partsCount > m_SplitParts.Count)
                {
                    // Add the new parts in cache
                    for (int i = m_SplitParts.Count; i < partsCount; ++i)
                    {
                        m_SplitParts.Add(GarminWorkoutManager.Instance.CreateWorkoutPart(this, i));
                    }
                }
                else
                {
                    // Remove the excess parts from cache
                    for (int i = m_SplitParts.Count; i > partsCount; --i)
                    {
                        m_SplitParts.RemoveAt(i - 1);
                    }
                }
            }
        }

        public override bool CanAcceptNewStep(int newStepCount, IStep destinationStep)
        {
            // Hard limit at PluginMaxStepsPerWorkout steps
            if (StepCount + newStepCount > Constants.PluginMaxStepsPerWorkout)
            {
                return false;
            }

            IStep topMostRepeat = Steps.GetTopMostRepeatForStep(destinationStep);

            if (topMostRepeat == null)
            {
                // Destination is not part of a repeat
                if (Options.Instance.AllowSplitWorkouts)
                {
                    return true;
                }
                else
                {
                    return StepCount + newStepCount <= Constants.MaxStepsPerWorkout;
                }
            }
            else
            {
                // When destination is part of a repeat it's a bit easier, there's a 19
                //  child steps limit.  Since this will occur of the repeat of the base
                //  level, check if we bust the limit
                if (Options.Instance.AllowSplitWorkouts)
                {
                    return topMostRepeat.StepCount + newStepCount <= Constants.MaxStepsPerWorkout;
                }
                else
                {
                    return StepCount + newStepCount <= Constants.MaxStepsPerWorkout;
                }
            }
        }

        public bool ContainsWorkoutLink(Workout workoutLink)
        {
            foreach(IStep currentStep in Steps)
            {
                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;

                    if (linkStep.LinkedWorkout == workoutLink)
                    {
                        return true;
                    }
                    else if (linkStep.LinkedWorkout.ContainsWorkoutLink(workoutLink))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override Workout ConcreteWorkout
        {
            get { return this; }
        }

        public override DateTime LastExportDate
        {
            get { return m_LastExportDate; }
            set
            {
                if (m_LastExportDate != value)
                {
                    m_LastExportDate.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("ExportDate"));
                }
            }
        }

        public override List<GarminFitnessDate> ScheduledDates
        {
            get { return m_ScheduledDates; }
        }

        public override WorkoutStepsList Steps
        {
            get { return m_Steps; }
        }

        public override IActivityCategory Category
        {
            get { return m_Category; }
            set
            {
                if (m_Category != value)
                {
                    Debug.Assert(value != null);
                    m_Category = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("Category"));
                }
            }
        }

        public override GarminFitnessString NameInternal
        {
            get { return m_Name; }
        }

        public override GarminFitnessGuid IdInternal
        {
            get { return m_Id; }
        }

        public override GarminFitnessString NotesInternal
        {
            get { return m_Notes; }
        }

        public override bool AddToDailyViewOnSchedule
        {
            get { return m_AddToDailyViewOnSchedule; }
            set
            {
                if (AddToDailyViewOnSchedule != value)
                {
                    m_AddToDailyViewOnSchedule.Value = value;

                    TriggerWorkoutChangedEvent(new PropertyChangedEventArgs("AddToDailyViewOnSchedule"));
                }
            }
        }

        public bool IsDirty
        {
            get
            {
                for (int i = 0; i < m_Steps.Count; ++i)
                {
                    if (m_Steps[i].IsDirty)
                    {
                        return true;
                    }
                }

                return false;
            }
            set { Debug.Assert(false); }
        }

        public override List<XmlNode> StepsExtensions
        {
            get { return m_StepsExtensions; }
        }

        public override List<XmlNode> STExtensions
        {
            get { return m_STExtensions; }
        }

        private GarminFitnessString m_Name = new GarminFitnessString("", Constants.MaxNameLength);
        private GarminFitnessGuid m_Id = new GarminFitnessGuid(Guid.NewGuid());
        private GarminFitnessDate m_LastExportDate = new GarminFitnessDate();
        private List<GarminFitnessDate> m_ScheduledDates = new List<GarminFitnessDate>();
        private WorkoutStepsList m_Steps = null;
        private List<WorkoutPart> m_SplitParts = new List<WorkoutPart>();
        private IActivityCategory m_Category;
        private GarminFitnessString m_Notes = new GarminFitnessString("", 30000);
        private GarminFitnessBool m_AddToDailyViewOnSchedule = new GarminFitnessBool(false);
        private List<XmlNode> m_STExtensions = new List<XmlNode>();
        private List<XmlNode> m_StepsExtensions = new List<XmlNode>();
    }
}

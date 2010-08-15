using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.Data
{
    class WorkoutLinkStep : IStep
    {
        public WorkoutLinkStep(Workout parent, Workout linkedWorkout)
            : base(StepType.Link, parent)
        {
            m_LinkedWorkout = linkedWorkout;
            m_WorkoutStepsCopy = new WorkoutStepsList(ParentConcreteWorkout);

            UpdateWorkoutStepsCopy();
            m_CurrentTopMostRepeat = ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this);

            LinkedWorkout.Steps.StepAdded += new WorkoutStepsList.StepAddedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.StepRemoved += new WorkoutStepsList.StepRemovedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.ListChanged += new PropertyChangedEventHandler(OnWorkoutStepsListChanged);
            ParentConcreteWorkout.Steps.ListChanged += new PropertyChangedEventHandler(OnWorkoutStepsListChanged);
        }

        public WorkoutLinkStep(Stream stream, DataVersion version, Workout parent)
            : base(StepType.Link, parent)
        {
            m_WorkoutStepsCopy = new WorkoutStepsList(ParentConcreteWorkout);

            Deserialize(stream, version);
            Debug.Assert(LinkedWorkout != null);

            LinkedWorkout.Steps.StepAdded += new WorkoutStepsList.StepAddedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.StepRemoved += new WorkoutStepsList.StepRemovedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.ListChanged += new PropertyChangedEventHandler(OnWorkoutStepsListChanged);
            ParentConcreteWorkout.Steps.ListChanged += new PropertyChangedEventHandler(OnWorkoutStepsListChanged);
        }

        void OnWorkoutStepsChanged(IStep addedRemovedStep)
        {
            if (addedRemovedStep == this)
            {
                m_CurrentTopMostRepeat = ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this);
            }

            UpdateWorkoutStepsCopy();
        }

        void OnWorkoutStepsListChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender == ParentConcreteWorkout.Steps)
            {
                RepeatStep newTopMostRepeat = ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this);

                if (newTopMostRepeat != m_CurrentTopMostRepeat)
                {
                    m_CurrentTopMostRepeat = newTopMostRepeat;
                    UpdateWorkoutStepsCopy();
                }
            }
            else if (sender == LinkedWorkout.Steps)
            {
                UpdateWorkoutStepsCopy();
            }
        }

#region IStep members

        public override uint Serialize(GarXFaceNet._Workout workout, uint stepIndex)
        {
            return LinkedWorkoutSteps.Serialize(workout, stepIndex);
        }

        public override void Deserialize(GarXFaceNet._Workout workout, uint stepIndex)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            GarminFitnessGuid linkedWorkoutId = new GarminFitnessGuid(LinkedWorkout.Id);

            linkedWorkoutId.Serialize(stream);
        }

        public void Deserialize_V14(Stream stream, DataVersion version)
        {
            base.Deserialize(typeof(IStep), stream, version);

            GarminFitnessGuid linkedWorkoutId = new GarminFitnessGuid();

            linkedWorkoutId.Deserialize(stream, version);
            m_LinkedWorkout = GarminWorkoutManager.Instance.GetWorkout(linkedWorkoutId);

            UpdateWorkoutStepsCopy();
            m_CurrentTopMostRepeat = ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this);
        }

        public override void Serialize(XmlNode parentNode, String nodeName, XmlDocument document)
        {
            foreach (IStep step in m_WorkoutStepsCopy)
            {
                step.Serialize(parentNode, nodeName, document);
            }
        }

        public override void Deserialize(XmlNode parentNode)
        {
            Debug.Assert(false);
        }

        public override IStep Clone()
        {
            return new WorkoutLinkStep(ParentConcreteWorkout, m_LinkedWorkout);
        }

        public override UInt16 StepCount
        {
            get
            {
                return LinkedWorkoutSteps.StepCount;
            }
        }

        public override bool ValidateAfterZoneCategoryChanged(ZoneFiveSoftware.Common.Data.Fitness.IZoneCategory changedCategory)
        {
            return false;
        }

        public override bool IsDirty
        {
            get { return m_Dirty; }
            set
            {
                if (IsDirty != value)
                {
                    m_Dirty = value;

                    TriggerStepChanged(new PropertyChangedEventArgs("IsDirty"));
                }
            }
        }

#endregion

        private void UpdateWorkoutStepsCopy()
        {
            m_WorkoutStepsCopy.Clear();
            IsDirty = false;

            foreach (IStep step in LinkedWorkout.Steps)
            {
                IStep stepCopy = step.Clone();

                stepCopy.ParentConcreteWorkout = ParentConcreteWorkout;
                if (!m_WorkoutStepsCopy.AddStepToRoot(stepCopy))
                {
                    IsDirty = true;
                }
            }
                
            TriggerStepChanged(new PropertyChangedEventArgs("LinkSteps"));
        }

        public Workout LinkedWorkout
        {
            get { return m_LinkedWorkout; }
        }

        public WorkoutStepsList LinkedWorkoutSteps
        {
            get { return m_WorkoutStepsCopy; }
        }

        private bool m_Dirty = false;
        private Workout m_LinkedWorkout = null;
        private WorkoutStepsList m_WorkoutStepsCopy = null;
        private RepeatStep m_CurrentTopMostRepeat = null;
    }
}

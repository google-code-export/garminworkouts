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
    public class WorkoutLinkStep : IStep
    {
        public WorkoutLinkStep(Workout parent, Workout linkedWorkout)
            : base(StepType.Link, parent)
        {
            m_LinkedWorkout = linkedWorkout;

            Initialize();
        }

        public WorkoutLinkStep(Stream stream, DataVersion version, Workout parent)
            : base(StepType.Link, parent)
        {
            Deserialize(stream, version);

            // Make sure we found the linked workout, it is possible we are copy-pasting
            //  an unavailable one, in which case this step is temporary and will be disposed
            if (LinkedWorkout != null)
            {
                Initialize();
            }
        }

        void OnWorkoutChanged(IWorkout modifiedWorkout, PropertyChangedEventArgs changedProperty)
        {
            if (changedProperty.PropertyName == "Steps" ||
                changedProperty.PropertyName == "PartsCount")
            {
                UpdateWorkoutStepsCopy();
            }

            TriggerStepChanged(new PropertyChangedEventArgs("ParentWorkout"));
        }

        void OnWorkoutStepChanged(IWorkout modifiedWorkout, IStep modifiedStep, PropertyChangedEventArgs changedProperty)
        {
            UpdateWorkoutStepsCopy();
        }

        void OnWorkoutStepTargetChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, ITarget modifiedTarget, PropertyChangedEventArgs changedProperty)
        {
            UpdateWorkoutStepsCopy();
        }

        void OnWorkoutStepDurationChanged(IWorkout modifiedWorkout, RegularStep modifiedStep, IDuration modifiedDuration, PropertyChangedEventArgs changedProperty)
        {
            UpdateWorkoutStepsCopy();
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

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            GarminFitnessGuid linkedWorkoutId = new GarminFitnessGuid(LinkedWorkout.Id);

            linkedWorkoutId.Serialize(stream);
            m_WorkoutStepsCopy.Serialize(stream);
        }

        public override void SerializetoFIT(Stream stream)
        {
            foreach (IStep step in m_WorkoutStepsCopy)
            {
                step.SerializetoFIT(stream);
            }
        }

        public override void FillFITStepMessage(FITMessage message)
        {
            Debug.Assert(false);
        }

        public override void DeserializeFromFIT(FITMessage stepMessage)
        {
            Debug.Assert(false);
        }

        public void Deserialize_V14(Stream stream, DataVersion version)
        {
            base.Deserialize(typeof(IStep), stream, version);

            GarminFitnessGuid linkedWorkoutId = new GarminFitnessGuid();

            linkedWorkoutId.Deserialize(stream, version);
            m_LinkedWorkout = GarminWorkoutManager.Instance.GetWorkout(linkedWorkoutId);
        }

        public void Deserialize_V15(Stream stream, DataVersion version)
        {
            Deserialize_V14(stream, version);

            // Check if we found the workout link.  If we did deserialize the steps
            //  list in a disposable one.  If we can't find the workout, then we must
            //  not deserialize the list so it can simply add the steps the the workout
            if (m_LinkedWorkout != null)
            {
                WorkoutStepsList tempStepsList = new WorkoutStepsList(ParentConcreteWorkout);

                tempStepsList.Deserialize(stream, version);
            }
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

        public override bool ValidateAfterZoneCategoryChanged(ZoneFiveSoftware.Common.Data.Fitness.IZoneCategory changedCategory)
        {
            return false;
        }

        public override bool ContainsFITOnlyFeatures
        {
            get
            {
                foreach (IStep step in m_WorkoutStepsCopy)
                {
                    if (step.ContainsFITOnlyFeatures)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override bool ContainsTCXExtensionFeatures
        {
            get
            {
                foreach (IStep step in m_WorkoutStepsCopy)
                {
                    if (step.ContainsTCXExtensionFeatures)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public override UInt16 StepCount
        {
            get
            {
                return LinkedWorkoutSteps.StepCount;
            }
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

        private void Initialize()
        {
            m_WorkoutStepsCopy = new WorkoutStepsList(ParentConcreteWorkout);
            
            LinkedWorkout.WorkoutChanged += new IWorkout.WorkoutChangedEventHandler(OnWorkoutChanged);
            LinkedWorkout.StepChanged += new IWorkout.StepChangedEventHandler(OnWorkoutStepChanged);
            LinkedWorkout.StepDurationChanged += new IWorkout.StepDurationChangedEventHandler(OnWorkoutStepDurationChanged);
            LinkedWorkout.StepTargetChanged += new IWorkout.StepTargetChangedEventHandler(OnWorkoutStepTargetChanged);
            LinkedWorkout.Steps.StepAdded += new WorkoutStepsList.StepAddedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.StepRemoved += new WorkoutStepsList.StepRemovedEventHandler(OnWorkoutStepsChanged);
            LinkedWorkout.Steps.ListChanged += new PropertyChangedEventHandler(OnWorkoutStepsListChanged);

            UpdateWorkoutStepsCopy();
            m_CurrentTopMostRepeat = ParentConcreteWorkout.Steps.GetTopMostRepeatForStep(this);
        }

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

        public WorkoutStepsList GetStepsForPart(int partNumber, ref WorkoutStepsList result,
                                                ref UInt16 partNumberCounter, ref int stepCounter)
        {
            for (int i = 0; i < LinkedWorkoutSteps.Count; ++i)
            {
                IStep currentStep = LinkedWorkoutSteps[i];

                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;

                    linkStep.GetStepsForPart(partNumber, ref result,
                                             ref partNumberCounter, ref stepCounter);
                }
                else
                {
                    int previousCounter = stepCounter;

                    stepCounter += currentStep.StepCount;

                    if ((previousCounter != 0 || partNumberCounter != 0) && (currentStep.ForceSplitOnStep || stepCounter > Constants.MaxStepsPerWorkout))
                    {
                        partNumberCounter++;
                        stepCounter = currentStep.StepCount;
                    }

                    if (partNumberCounter == partNumber)
                    {
                        // Add step to result, it's in the right part
                        result.Add(currentStep);
                    }
                    else if (partNumberCounter > partNumber)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        public Workout LinkedWorkout
        {
            get { return m_LinkedWorkout; }
        }

        public WorkoutStepsList LinkedWorkoutSteps
        {
            get { return m_WorkoutStepsCopy; }
        }

        public override Workout ParentConcreteWorkout
        {
            get { return m_ParentWorkout; }
            set
            {
                m_ParentWorkout = value;

                if (m_WorkoutStepsCopy != null)
                {
                    foreach (IStep step in m_WorkoutStepsCopy)
                    {
                        step.ParentConcreteWorkout = value;
                    }
                }
            }
        }

        private bool m_Dirty = false;
        private Workout m_LinkedWorkout = null;
        private Workout m_ParentWorkout;
        private WorkoutStepsList m_WorkoutStepsCopy = null;
        private RepeatStep m_CurrentTopMostRepeat = null;
    }
}

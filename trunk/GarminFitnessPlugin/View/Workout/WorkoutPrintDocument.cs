using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using GarminFitnessPlugin.Data;
using GarminFitnessPlugin.Controller;

namespace GarminFitnessPlugin.View
{
    class WorkoutPrintDocument : PrintDocument
    {
        public WorkoutPrintDocument(List<Workout> workoutsToPrint, bool inkFriendlyMode, bool unrollRepeats)
        {
            m_WorkoutsToPrint = new List<Workout>();
            m_WorkoutsToPrint.AddRange(workoutsToPrint);

            m_InkFriendlyMode = inkFriendlyMode;
            m_UnrollRepeats = unrollRepeats;
        }

        protected override void OnBeginPrint(PrintEventArgs e)
        {
            base.OnBeginPrint(e);

            if (m_InkFriendlyMode)
            {
                m_HeaderTextBrush = new SolidBrush(Color.Black);
                m_HeaderBackgroundBrush = new SolidBrush(Color.White);

                m_BorderPen = new Pen(Color.DarkGray, 3);
                m_BackgroundBrush = new SolidBrush(Color.White);

                m_WorkoutDetailsBrush = new SolidBrush(Color.Black);
            }
            else
            {
                m_HeaderTextBrush = new SolidBrush(Color.White);
                m_HeaderBackgroundBrush = new SolidBrush(Color.Black);

                m_BorderPen = new Pen(Color.Black, 3);
                m_BackgroundBrush = new SolidBrush(Color.LightGray);

                m_WorkoutDetailsBrush = new SolidBrush(Color.Black);
            }

            m_HeaderFont = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold);
            m_HeaderNotesFont = new Font(FontFamily.GenericSansSerif, 10);
            m_WorkoutDetailsFont = new Font(FontFamily.GenericSansSerif, 10);
            m_StepHeaderFont = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        }

        protected override void  OnEndPrint(PrintEventArgs e)
        {
 	        base.OnEndPrint(e);

            m_HeaderFont.Dispose();
            m_HeaderTextBrush.Dispose();
            m_HeaderBackgroundBrush.Dispose();
            m_BorderPen.Dispose();
            m_BackgroundBrush.Dispose();
            m_WorkoutDetailsFont.Dispose();
            m_StepHeaderFont.Dispose();
            m_WorkoutDetailsBrush.Dispose();
        }

        protected override void OnPrintPage(PrintPageEventArgs e)
        {
            try
            {
                base.OnPrintPage(e);

                if (m_CurrentWorkout != null || m_WorkoutsToPrint.Count > 0)
                {
                    RectangleF outputArea = new RectangleF(e.PageSettings.Bounds.Left + e.PageSettings.Margins.Left,
                                                           e.PageSettings.Bounds.Top + e.PageSettings.Margins.Top,
                                                           e.PageSettings.Bounds.Width - e.PageSettings.Margins.Left - e.PageSettings.Margins.Right,
                                                           e.PageSettings.Bounds.Height - e.PageSettings.Margins.Top - e.PageSettings.Margins.Bottom);
                    float headerHeight = 0;

                    if (m_CurrentWorkout == null)
                    {
                        m_CurrentWorkout = m_WorkoutsToPrint[0].CloneUnregistered();
                        m_CurrentWorkoutSteps = UnrollRepeats(m_CurrentWorkout.Steps, m_CurrentWorkout);
                        m_CurrentStep = 0;
                        m_WorkoutsToPrint.RemoveAt(0);

                        headerHeight = PrintWorkoutHeader(m_CurrentWorkout,
                                                          e.Graphics,
                                                          new RectangleF(outputArea.Left, outputArea.Top,
                                                                         outputArea.Width - (1.5f * m_CornerRadius), outputArea.Height));
                    }

                    int nextStepToDraw = PrintWorkoutSteps(m_CurrentWorkout, m_CurrentStep,
                                                           e.Graphics,
                                                           new RectangleF(outputArea.Left, outputArea.Top + headerHeight,
                                                                          outputArea.Width - (1.5f * m_CornerRadius), outputArea.Height - headerHeight));

                    if (nextStepToDraw < m_CurrentWorkoutSteps.StepCount)
                    {
                        m_CurrentStep = nextStepToDraw;
                    }
                    else
                    {
                        m_CurrentWorkout = null;
                        m_CurrentWorkoutSteps = null;
                    }

                    e.HasMorePages = m_CurrentWorkout != null || m_WorkoutsToPrint.Count > 0;
                }
            }
            catch(Exception exception)
            {
                throw exception;
            }
        }

        private float PrintWorkoutHeader(Workout workout, Graphics graphics, RectangleF outputArea)
        {
            RectangleF headerNameArea;
            RectangleF headerArea;
            RectangleF headerNotesArea;
            SizeF measuredSize;
            const float maxNotesHeight = 150;
            float headerCornerRadius = m_CornerRadius;

            headerNameArea = new RectangleF(outputArea.Left + m_CornerRadius, outputArea.Top + 5,
                                            outputArea.Width, m_CornerRadius);
            headerNotesArea = new RectangleF(headerNameArea.Left + m_CornerRadius, headerNameArea.Bottom + 15,
                                             headerNameArea.Width - (m_CornerRadius * 2), headerNameArea.Height);
            measuredSize = graphics.MeasureString(workout.Notes.Trim(), m_HeaderNotesFont, (int)headerNotesArea.Width);
            headerNotesArea.Height = Math.Min(measuredSize.Height, maxNotesHeight);
            headerArea = new RectangleF(outputArea.Left, outputArea.Top,
                                        outputArea.Width, headerNotesArea.Bottom - headerNameArea.Top);

            if (!String.IsNullOrEmpty(workout.Notes.Trim()))
            {
                headerArea.Height += 15;
            }
            else
            {
                headerArea.Height = headerNameArea.Height + 10;
                headerCornerRadius = headerArea.Height * 0.5f;
            }

            FilledRoundedRectangle.Draw(graphics, m_BorderPen, m_HeaderBackgroundBrush,
                                        headerArea, headerCornerRadius);
            graphics.DrawString(workout.Name, m_HeaderFont, m_HeaderTextBrush, headerNameArea);

            if (!String.IsNullOrEmpty(workout.Notes.Trim()))
            {
                graphics.DrawString(workout.Notes.Trim(), m_HeaderNotesFont, m_HeaderTextBrush, headerNotesArea);

                if (measuredSize.Height > maxNotesHeight)
                {
                    RectangleF moreNotesArea = new RectangleF(headerNotesArea.Left, headerNotesArea.Bottom - 20,
                                                              headerNotesArea.Width - m_CornerRadius, 20);

                    graphics.FillRectangle(m_HeaderBackgroundBrush, moreNotesArea);
                    graphics.DrawString("...", m_HeaderNotesFont, m_HeaderTextBrush, moreNotesArea);
                }
            }

            return headerArea.Height;
        }

        private int PrintWorkoutSteps(Workout workout, int startStep,
                                      Graphics graphics, RectangleF outputArea)
        {
            RectangleF contentsArea;
            float dummy;

            contentsArea = new RectangleF(outputArea.Left, outputArea.Top + 15,
                                          outputArea.Width, outputArea.Height - 15);
            FilledRoundedRectangle.Draw(graphics, m_BorderPen, m_BackgroundBrush,
                                        contentsArea, m_CornerRadius);

            return PrintSteps(m_CurrentWorkoutSteps, startStep, graphics,
                              new RectangleF(contentsArea.Left, contentsArea.Top + 10,
                                             contentsArea.Width, contentsArea.Height - 10),
                              out dummy);
        }

        private int PrintSteps(List<IStep> steps, int startStep, Graphics graphics,
                               RectangleF outputArea, out float nextRenderTop)
        {
            const float stepDetailsIndentation = 20;
            float oneRowStepHeaderHeight = graphics.MeasureString("Dummy row height", m_StepHeaderFont).Height;
            float maxStepDetailsHeight = graphics.MeasureString("Five\nDummy\nRows\nIs\nPlenty", m_WorkoutDetailsFont).Height;
            SizeF extraNotesSize = graphics.MeasureString("...", m_WorkoutDetailsFont);
            RectangleF stepArea;
            int stepIndex = 0;

            nextRenderTop = 0;

            stepArea = new RectangleF(outputArea.Left + m_CornerRadius, outputArea.Top,
                                      outputArea.Width - m_CornerRadius, outputArea.Height);

            foreach (IStep currentStep in steps)
            {
                if (currentStep is WorkoutLinkStep)
                {
                    WorkoutLinkStep linkStep = currentStep as WorkoutLinkStep;
                    float nextAreaTop = 0;

                    stepIndex += PrintSteps(linkStep.LinkedWorkoutSteps,
                                            startStep - stepIndex, graphics,
                                            outputArea, out nextAreaTop);

                    stepArea.Offset(0, nextAreaTop - outputArea.Top);
                }
                else if (stepIndex >= startStep)
                {
                    SizeF titleSize = new SizeF(0, 0);
                    SizeF detailsSize = new SizeF(0, 0);
                    RectangleF detailsArea;
                    String titleText = String.Empty;
                    String detailsText = String.Empty;

                    stepArea = new RectangleF(outputArea.Left + m_CornerRadius, stepArea.Top,
                                              outputArea.Width - m_CornerRadius, outputArea.Height - stepArea.Top + outputArea.Top);

                    if (currentStep is RegularStep)
                    {
                        RegularStep regularStep = currentStep as RegularStep;

                        if (!String.IsNullOrEmpty(regularStep.Name))
                        {
                            titleText = StepDescriptionStringFormatter.FormatStepDescription(regularStep);
                            titleSize = graphics.MeasureString(titleText, m_StepHeaderFont, (int)stepArea.Width);

                            detailsText = StepDescriptionStringFormatter.FormatDurationDescription(regularStep.Duration) +
                                       StepDescriptionStringFormatter.FormatTargetDescription(regularStep.Target) +
                                       "\n" +
                                       regularStep.Notes.Trim();
                        }
                        else
                        {
                            titleText = StepDescriptionStringFormatter.FormatStepDescription(regularStep);
                            titleSize = graphics.MeasureString(titleText, m_StepHeaderFont, (int)stepArea.Width);

                            if (titleSize.Height > oneRowStepHeaderHeight * 1.25f)
                            {
                                titleText = StepDescriptionStringFormatter.FormatDurationDescription(regularStep.Duration) +
                                           "\n" +
                                           StepDescriptionStringFormatter.FormatTargetDescription(regularStep.Target) + " (" +
                                           StepDescriptionStringFormatter.GetStepIntensityText(regularStep.Intensity) + ")";
                                titleSize = graphics.MeasureString(titleText, m_StepHeaderFont, (int)stepArea.Width);
                            }

                            detailsText = regularStep.Notes.Trim();
                        }

                        detailsSize = graphics.MeasureString(detailsText, m_HeaderNotesFont, (int)(stepArea.Width - stepDetailsIndentation));
                        detailsArea = new RectangleF(stepArea.Left + stepDetailsIndentation, stepArea.Top + titleSize.Height,
                                                     detailsSize.Width, Math.Min(maxStepDetailsHeight, detailsSize.Height));

                        if (detailsArea.Height == maxStepDetailsHeight)
                        {
                            detailsSize.Height = detailsArea.Height + extraNotesSize.Height;
                        }

                        if ((stepArea.Top + titleSize.Height + detailsSize.Height) < outputArea.Bottom)
                        {
                            graphics.DrawString(titleText, m_StepHeaderFont, m_WorkoutDetailsBrush, stepArea.Location);
                            graphics.DrawString(detailsText, m_WorkoutDetailsFont, m_WorkoutDetailsBrush, detailsArea);

                            if (detailsArea.Height == maxStepDetailsHeight)
                            {
                                RectangleF moreNotesArea = new RectangleF(detailsArea.Left, detailsArea.Bottom,
                                                                          detailsArea.Width, extraNotesSize.Height);

                                graphics.DrawString("...", m_WorkoutDetailsFont, m_WorkoutDetailsBrush, moreNotesArea);

                                detailsArea.Height += extraNotesSize.Height;
                            }
                        }
                        else
                        {
                            break;
                        }

                        stepArea.Offset(0, titleSize.Height + detailsArea.Height + 10);
                    }
                    else if (currentStep is RepeatStep)
                    {
                        RepeatStep repeatStep = currentStep as RepeatStep;

                        titleText = StepDescriptionStringFormatter.FormatRepeatDurationDescription(repeatStep.Duration);
                        titleSize = graphics.MeasureString(titleText, m_StepHeaderFont, (int)stepArea.Width);
                        detailsText = repeatStep.Notes.Trim();
                        detailsSize = graphics.MeasureString(detailsText, m_StepHeaderFont, (int)(stepArea.Width - stepDetailsIndentation));
                        detailsArea = new RectangleF(stepArea.Left + stepDetailsIndentation, stepArea.Top + titleSize.Height,
                                                     detailsSize.Width, Math.Min(maxStepDetailsHeight, detailsSize.Height));

                        if (detailsArea.Height == maxStepDetailsHeight)
                        {
                            detailsSize.Height = detailsArea.Height + extraNotesSize.Height;
                        }

                        // Make sure we can fit at least 1 step underneath
                        if ((stepArea.Top + titleSize.Height + detailsSize.Height + (oneRowStepHeaderHeight * 2) + maxStepDetailsHeight) < outputArea.Bottom)
                        {
                            float nextAreaTop = 0;

                            graphics.DrawString(titleText, m_StepHeaderFont, m_WorkoutDetailsBrush, stepArea);
                            graphics.DrawString(detailsText, m_WorkoutDetailsFont, m_WorkoutDetailsBrush, detailsArea);

                            if (detailsArea.Height == maxStepDetailsHeight)
                            {
                                RectangleF moreNotesArea = new RectangleF(detailsArea.Left, detailsArea.Bottom,
                                                                          detailsArea.Width, extraNotesSize.Height);

                                graphics.DrawString("...", m_WorkoutDetailsFont, m_WorkoutDetailsBrush, moreNotesArea);

                                detailsArea.Height += extraNotesSize.Height;
                            }

                            int stepsRendered = PrintSteps(repeatStep.StepsToRepeat, 0, graphics,
                                                           new RectangleF(stepArea.Left, stepArea.Top + titleSize.Height + detailsArea.Height,
                                                                          stepArea.Width, outputArea.Bottom - stepArea.Top - titleSize.Height - detailsArea.Height),
                                                                          out nextAreaTop);

                            stepArea.Offset(0, nextAreaTop - stepArea.Top);

                            stepIndex += stepsRendered;
                            if (stepsRendered < (repeatStep.StepCount - 1))
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (currentStep is RepeatStep)
                    {
                        RepeatStep repeatStep = currentStep as RepeatStep;
                        float nextAreaTop = 0;

                        stepIndex += PrintSteps(repeatStep.StepsToRepeat, startStep - stepIndex, graphics,
                                                outputArea, out nextAreaTop);

                        stepArea.Offset(0, nextAreaTop - outputArea.Top);
                    }
                }

                ++stepIndex;
            }

            nextRenderTop = stepArea.Top;

            return stepIndex;
        }

        private WorkoutStepsList UnrollRepeats(List<IStep> originalSteps, Workout parent)
        {
            WorkoutStepsList result = new WorkoutStepsList(parent);

            foreach(IStep currentStep in originalSteps)
            {
                bool copyOriginal = true;

                if (m_UnrollRepeats)
                {
                    if (currentStep is RepeatStep)
                    {
                        RepeatStep currentRepeat = currentStep as RepeatStep;
                        WorkoutStepsList unrolledRepeat;

                        if (currentRepeat.Duration is RepeatCountDuration)
                        {
                            RepeatCountDuration duration = currentRepeat.Duration as RepeatCountDuration;

                            copyOriginal = false;
                            unrolledRepeat = UnrollRepeats(currentRepeat.StepsToRepeat, parent);

                            for (int i = 0; i < duration.RepetitionCount; ++i)
                            {
                                foreach (IStep currentUnrolledStep in unrolledRepeat)
                                {
                                    result.Add(currentUnrolledStep.Clone());
                                }
                            }
                        }
                    }
                    else if (currentStep is WorkoutLinkStep)
                    {
                        WorkoutLinkStep currentLink = currentStep as WorkoutLinkStep;
                        WorkoutStepsList unrolledRepeat;

                        copyOriginal = false;
                        unrolledRepeat = UnrollRepeats(currentLink.LinkedWorkoutSteps, parent);

                        foreach (IStep currentUnrolledStep in unrolledRepeat)
                        {
                            result.Add(currentUnrolledStep);
                        }
                    }
                }

                if (copyOriginal)
                {
                    result.Add(currentStep.Clone());
                }
            }

            return result;
        }

        private List<Workout> m_WorkoutsToPrint = null;
        private Workout m_CurrentWorkout = null;
        private WorkoutStepsList m_CurrentWorkoutSteps = null;
        private int m_CurrentStep = 0;
        private bool m_InkFriendlyMode = true;

        // Rendering objects
        private Font m_HeaderFont = null;
        private Font m_HeaderNotesFont = null;
        private SolidBrush m_HeaderTextBrush = null;
        private SolidBrush m_HeaderBackgroundBrush = null;
        private Pen m_BorderPen = null;
        private SolidBrush m_BackgroundBrush = null;
        private Font m_WorkoutDetailsFont = null;
        private Font m_StepHeaderFont = null;
        private SolidBrush m_WorkoutDetailsBrush = null;
        private const float m_CornerRadius = 30;
        private bool m_UnrollRepeats = false;
    }
}

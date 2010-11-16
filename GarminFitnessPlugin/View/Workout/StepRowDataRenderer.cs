using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using System.Drawing;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class StepRowDataRenderer : ExtendedRowDataRenderer
    {
        public StepRowDataRenderer(ExtendedTreeList list)
            : base(list)
        {
            m_ParentList = list;
        }

        protected override RowDecoration GetRowDecoration(object element)
        {
            if (m_IsInDrag)
            {
                IList rows = GetRows();

                if (m_DragResultPosition < rows.Count && rows[m_DragResultPosition] == element)
                {
                    if (m_IsInUpperHalf)
                    {
                        return RowDecoration.TopLineSingle;
                    }
                    else
                    {
                        return RowDecoration.BottomLineSingle;
                    }
                }
            }

            return base.GetRowDecoration(element);
        }

        protected override void DrawRowBackground(Graphics graphics, Rectangle clipRect, Rectangle rectDraw)
        {
            object currentRowElement = m_ParentList.RowHitTest(m_DragOverClientPosition,
                                                               out m_DragResultPosition,
                                                               out m_IsInUpperHalf);
            StepWrapper wrapper = currentRowElement as StepWrapper;

            if (m_IsInDrag)
            {
                if (currentRowElement == null)
                {
                    m_DragResultPosition = GetRows().Count - 1;
                    m_IsInUpperHalf = false;
                }
                else
                {
                    if (wrapper.Element is RepeatStep)
                    {
                        if (!m_IsInUpperHalf)
                        {
                            RepeatStep step = wrapper.Element as RepeatStep;

                            m_DragResultPosition += step.StepCount - 1;
                        }
                    }
                }
            }

            base.DrawRowBackground(graphics, clipRect, rectDraw);

            // Draw workout link rows as disabled
            if (GetRows().Count > 0)
            {
                Brush disabledColorBrush = new SolidBrush(Color.FromArgb(255, 100, 100, 100));
                Rectangle rowRect = new Rectangle(rectDraw.Left, rectDraw.Top - m_ParentList.TopRow * RowHeight(GetRows()[0]), rectDraw.Width, RowHeight(GetRows()[0]));

                foreach (StepWrapper currentRowWrapper in GetRows())
                {
                    if (currentRowWrapper.IsWorkoutLinkChild &&
                        !m_ParentList.SelectedItems.Contains(currentRowWrapper))
                    {
                        graphics.FillRectangle(disabledColorBrush, rowRect);
                    }

                    rowRect.Offset(0, RowHeight(currentRowWrapper));
                }

                disabledColorBrush.Dispose();
            }
        }

        protected override void DrawCell(Graphics graphics, TreeList.DrawItemState rowState, object element, TreeList.Column column, Rectangle cellRect)
        {
            TreeList.DrawItemState rowDrawState = rowState;
            StepWrapper rowWrapper = element as StepWrapper;

            Debug.Assert(rowWrapper != null);

            if (rowWrapper.IsWorkoutLinkChild)
            {
                rowDrawState = new TreeList.DrawItemState(TreeList.DrawItemState.DrawItemStateBits.Disabled);
            }

            base.DrawCell(graphics, rowDrawState, element, column, cellRect);
        }

        public bool IsInDrag
        {
            set { m_IsInDrag = value; }
        }

        public Point DragOverClientPosition
        {
            set { m_DragOverClientPosition = value; }
        }

        private bool m_IsInDrag = false;
        private Point m_DragOverClientPosition;
        private bool m_IsInUpperHalf = false;
        private int m_DragResultPosition;
        private ExtendedTreeList m_ParentList;
    }
}

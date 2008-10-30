using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ZoneFiveSoftware.Common.Visuals;
using System.Drawing;
using GarminFitnessPlugin.Data;

namespace GarminFitnessPlugin.View
{
    class StepRowDataRenderer : TreeList.DefaultRowDataRenderer
    {
        public StepRowDataRenderer(TreeList list)
            : base(list)
        {
        }

        protected override RowDecoration GetRowDecoration(object element)
        {
            if (m_IsInDrag && GetRows()[m_DragResultPosition] == element)
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

            return base.GetRowDecoration(element);
        }

        protected override void DrawRowBackground(Graphics graphics, Rectangle clipRect, Rectangle rectDraw)
        {
            if (m_IsInDrag)
            {
                object currentRowElement = RowHitTest(m_DragOverClientPosition,
                                                      out m_DragResultPosition,
                                                      out m_IsInUpperHalf);

                if (currentRowElement == null)
                {
                    m_DragResultPosition = GetRows().Count - 1;
                    m_IsInUpperHalf = false;
                }
                else
                {
                    if (((StepWrapper)currentRowElement).Element.GetType() == typeof(RepeatStep))
                    {
                        if (!m_IsInUpperHalf)
                        {
                            RepeatStep step = (RepeatStep)((StepWrapper)currentRowElement).Element;

                            m_DragResultPosition += step.GetStepCount() - 1;
                        }
                    }
                }
            }

            base.DrawRowBackground(graphics, clipRect, rectDraw);
        }

        public object RowHitTest(Point position)
        {
            int rowNumber;
            bool isInUpperHalf;

            return RowHitTest(position, out rowNumber, out isInUpperHalf);
        }

        public object RowHitTest(Point position, out int rowNumber, out bool isInUpperHalf)
        {
            ArrayList rows = (ArrayList)GetRows();
            int rowY = position.Y;
            int i = -1;
            object currentRowElement = null;

            do
            {
                i++;
                currentRowElement = rows[i];
                rowY -= RowHeight(currentRowElement);
            }
            while (rowY > 0 && i < rows.Count - 1);

            // This is past the last row
            if (rowY > 0)
            {
                currentRowElement = null;
                rowNumber = -1;
                isInUpperHalf = false;
            }
            else
            {
                rowNumber = i;
                isInUpperHalf = -rowY > RowHeight(currentRowElement) / 2.0f;
            }

            return currentRowElement;
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
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
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
            if (m_IsInDrag)
            {
                object currentRowElement = m_ParentList.RowHitTest(m_DragOverClientPosition,                                                                   
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

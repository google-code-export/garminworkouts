using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ZoneFiveSoftware.Common.Visuals;

namespace GarminFitnessPlugin.View
{
    public class ExtendedTreeList : TreeList
    {
        public ExtendedTreeList()
        {
            base.SelectedItemsChanged += new EventHandler(OnBaseSelectedItemsChanged);
            base.MouseDown += new System.Windows.Forms.MouseEventHandler(OnBaseMouseDown);
            base.MouseUp += new System.Windows.Forms.MouseEventHandler(OnBaseMouseUp);
            base.MouseMove += new System.Windows.Forms.MouseEventHandler(OnBaseMouseMove);
            base.EnabledChanged += new EventHandler(OnBaseEnabledChanged);
            base.DragOver += new DragEventHandler(OnBaseDragOver);
            base.DragDrop += new DragEventHandler(OnBaseDragDrop);
            VScrollBar.ValueChanged += new EventHandler(OnVScrollBarValueChanged);
            
            RowDataRenderer = new ExtendedRowDataRenderer(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
            {
                e.SuppressKeyPress = true;
            }

            base.OnKeyDown(e);
        }

        void OnBaseDragDrop(object sender, DragEventArgs e)
        {
            m_RefocusOnItem = true;
        }

        void OnVScrollBarValueChanged(object sender, EventArgs e)
        {
            if (m_RefocusOnItem)
            {
                // Force focus to selection after drop
                if (Selected.Count > 0)
                {
                    EnsureVisible(Selected[0]);
                }

                m_RefocusOnItem = false;
            }
        }

        void OnBaseSelectedItemsChanged(object sender, EventArgs e)
        {
            if (m_SelectionCancelled)
            {
                if (m_CancelledSelection == null)
                {
                    m_CancelledSelection = Selected;
                    Selected = m_LastSelection;
                }
            }
            else
            {
                if (SelectedItemsChanged != null)
                {
                    SelectedItemsChanged(sender, e);
                }
                m_LastSelection = Selected;
            }
        }

        private void OnBaseMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                TreeList.RowHitState type;
                object futureSelection = RowHitTest(e.Location, out type);

                if (type == TreeList.RowHitState.PlusMinus)
                {
                    if(ExpandedChanged != null)
                    {
                        ExpandedChanged(futureSelection, !IsExpanded(futureSelection));
                    }
                }
                else if (futureSelection != null)
                {
                    m_IsMouseDownInList = true;
                    m_MouseMovedPixels = 0;
                    m_LastMouseDownLocation = e.Location;

                    m_SelectionCancelled = Selected.Count != 1 && Selected.Contains(futureSelection);
                }
                else
                {
                    Selected = new List<TreeList.TreeListNode>();
                }
            }
        }

        private void OnBaseMouseUp(object sender, MouseEventArgs e)
        {
            if (m_SelectionCancelled)
            {
                m_SelectionCancelled = false;
                Selected = m_CancelledSelection;
            }

            m_IsMouseDownInList = false;
            m_CancelledSelection = null;
        }

        private void OnBaseMouseMove(object sender, MouseEventArgs e)
        {
            if (m_IsMouseDownInList && m_MouseMovedPixels < 5)
            {
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.X - e.X);
                m_MouseMovedPixels += Math.Abs(m_LastMouseDownLocation.Y - e.Y);

                if (m_MouseMovedPixels >= 5)
                {
                    m_SelectionCancelled = false;
                    m_IsMouseDownInList = false;
                    m_CancelledSelection = null;

                    // Notify of drag start
                    if (Selected.Count > 0 && DragStart != null)
                    {
                        DragStart(this, new EventArgs());
                    }
                }
            }
        }

        void OnBaseEnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                BackColor = System.Drawing.SystemColors.Window;
            }
            else
            {
                BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            }
        }

        void OnBaseDragOver(object sender, DragEventArgs e)
        {
            Point clientPosition = PointToClient(new Point(e.X, e.Y));

            if (clientPosition.Y < DragAutoScrollSize && VScrollBar.Value > VScrollBar.Minimum)
            {
                VScrollBar.Value--;
            }
            else if(clientPosition.Y > Size.Height - DragAutoScrollSize && VScrollBar.Value < VScrollBar.Maximum)
            {
                VScrollBar.Value++;
            }
        }

        public object RowHitTest(Point position, RowHitState state)
        {
            state = RowHitState.Row;
            return RowHitTest(position);
        }

        public object RowHitTest(Point position)
        {
            int rowNumber;
            bool isInUpperHalf;

            return RowHitTest(position, out rowNumber, out isInUpperHalf);
        }

        public object RowHitTest(Point position, out int rowNumber, out bool isInUpperHalf)
        {
            IList rows = ((ExtendedRowDataRenderer)RowDataRenderer).GetRows();
            int rowY = position.Y;
            int i = VScrollBar.Value - 1;
            object currentRowElement = null;

            do
            {
                i++;
                currentRowElement = rows[i];
                rowY -= RowDataRenderer.RowHeight(currentRowElement);
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
                isInUpperHalf = -rowY > RowDataRenderer.RowHeight(currentRowElement) / 2.0f;
            }

            return currentRowElement;
        }

        public new IRowDataRenderer RowDataRenderer
        {
            get
            {
                Debug.Assert(base.RowDataRenderer is ExtendedRowDataRenderer);

                return base.RowDataRenderer;
            }
            set
            {
                Debug.Assert(value is ExtendedRowDataRenderer);

                base.RowDataRenderer = value;
            }
        }

        public Byte DragAutoScrollSize
        {
            get { return m_DragAutoScrollSize; }
            set { m_DragAutoScrollSize = value; }
        }

        public delegate void ExpandedChangedEventHandler(object sender, bool expanded);

        public new event EventHandler SelectedItemsChanged;
        public event EventHandler DragStart;
        public event ExpandedChangedEventHandler ExpandedChanged;

        private Byte m_DragAutoScrollSize = 20;

        private IList m_LastSelection = null;
        private IList m_CancelledSelection = null;
        private Point m_LastMouseDownLocation;
        private int m_MouseMovedPixels = 0;
        private bool m_RefocusOnItem = false;
        private bool m_IsMouseDownInList = false;
        private bool m_SelectionCancelled = false;
    }
}

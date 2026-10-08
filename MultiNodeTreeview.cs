using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace WinAGI.Editor {
    /// <summary>
    /// A TreeView control that allows for multi-node selection and manipulation.
    /// </summary>
    public class MultiNodeTreeview : TreeView {
        #region Fields
        private bool selecting;
        private bool isInsertion;
        // flag to prevent changes in SelectedNode from triggering OnAfterSelect event logic
        private bool noupdate = false;
        #endregion

        #region Constructors
        public MultiNodeTreeview() {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            // Enable double buffering to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            typeof(TreeView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(this, true, null);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether the control is in a state where a single node selected
        /// is highlighted (indicating 'selected') or not (indicating 'insertion').
        /// </summary>
        public bool IsInsertion {
            get {
                return isInsertion;
            }
            set {
                if (FirstSelectedNode != LastSelectedNode || SelectedNode is null) {
                    return; // only allow this if one node is selected
                }
                if (isInsertion != value) {
                    isInsertion = value;
                    // repaint the selection
                    Rectangle bounds = SelectedNode.Bounds;
                    bounds = InflateNodeBounds(bounds);
                    Invalidate(bounds, false);
                }
            }
        }
        
        public TreeNode FirstSelectedNode {
            get; private set;
        }

        public TreeNode LastSelectedNode {
            get; private set;
        }


        /// <summary>
        /// Gets the currently selected level 2 nodes in the tree view.
        /// </summary>
        public IEnumerable<TreeNode> SelectedNodes {
            get {
                if (SelectedNode is null ||
                    SelectedNode.Level != 2 || 
                    FirstSelectedNode is null ||
                    LastSelectedNode is null ||
                    IsInsertion) {
                    yield break;
                }

                if (FirstSelectedNode.Parent != LastSelectedNode.Parent)
                    yield break;

                TreeNodeCollection nodes = FirstSelectedNode.Parent?.Nodes ?? Nodes;
                Debug.Assert(FirstSelectedNode.Index <= LastSelectedNode.Index);
                int start = FirstSelectedNode.Index;
                int end = LastSelectedNode.Index;

                // preserve End-node rule for level 2 nodes
                end = Math.Min(end, nodes.Count - 2);

                for (int i = start; i <= end; i++) {
                    yield return nodes[i];
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        /// <summary>
        /// Gets the currently selected node in the tree view. This property is overridden
        /// to prevent external modification. DO NOT use the base.SelectedNode property directly;
        /// calling programs must use the SelectRange methods instead.
        /// </summary>
        public new TreeNode SelectedNode {
            get => base.SelectedNode;
            private set => base.SelectedNode = value;
        }
        #endregion

        #region Methods
        private static Rectangle InflateNodeBounds(Rectangle bounds) {
            bounds.X -= 2;
            bounds.Width += 4;
            return bounds;
        }

        private void SetSelectedNode(TreeNode node) {
            if (node != SelectedNode) {
                try {
                    noupdate = true;
                    SelectedNode = node;
                }
                finally {
                    noupdate = false;
                }
            }
        }

        /// <summary>
        /// Selects a range of nodes between the specified start and end nodes, inclusive.
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <exception cref="ArgumentException"></exception>
        public void SelectRange(TreeNode startNode, TreeNode endNode, bool topanchor = true) {
            if (startNode.Parent != endNode.Parent) {
                throw new ArgumentException("Start and end nodes must have the same parent.");
            }
            if (startNode.Index > endNode.Index) {
                throw new ArgumentException("startNode must precede endNode.");
            }
            FirstSelectedNode = startNode;
            LastSelectedNode = endNode;
            SetSelectedNode(topanchor ? startNode : endNode);
            isInsertion = false;
            Invalidate();
        }

        public void SelectRange(TreeNode node, int length) {
            if (node.Level != 2) {
                throw new ArgumentException("Node must be at level 2.");
            }
            TreeNode parent = node.Parent;
            if (parent is null) {
                throw new ArgumentException("Node must have a parent.");
            }
            if (length < 0) {
                throw new ArgumentException("Invalid length");
            }
            int startIndex = node.Index;
            int endIndex = Math.Min(startIndex + length - 1, parent.Nodes.Count - 2);
            FirstSelectedNode = node;
            LastSelectedNode = parent.Nodes[endIndex];
            SetSelectedNode(FirstSelectedNode);
            Invalidate();
        }

        /// <summary>
        /// Selects a single node, clearing any previous selection.
        /// </summary>
        /// <param name="node"></param>
        public void SelectRange(TreeNode node) {
            FirstSelectedNode = node;
            LastSelectedNode = node;
            SetSelectedNode(node);
            Invalidate();
        }

        private void InvalidateSelectionBounds(Rectangle bounds) {
            if (isInsertion) {
                isInsertion = false;
                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
            }

            bounds = InflateNodeBounds(bounds);
            Invalidate(bounds, false);
        }
        #endregion

        #region Event Overrides
        protected override void OnDrawNode(DrawTreeNodeEventArgs e) {
            // Determine if the node is in selection collection
            // (or is a non-collection selection)
            bool isSelected;
            if (SelectedNode is null || FirstSelectedNode is null || LastSelectedNode is null) {
                isSelected = false;
            }
            else {
                switch (e.Node.Level) {
                case 2:
                    // if node is in same group as collection
                    if (e.Node.Parent == SelectedNode.Parent) {
                        // check if it's within selected range bounds
                        isSelected = e.Node.Index >= FirstSelectedNode.Index && e.Node.Index <= LastSelectedNode.Index;
                    }
                    else {
                        isSelected = false;
                    }
                    break;
                default:
                    // always show full selection if non-level 2 node is selected
                    isSelected = e.Node == SelectedNode;
                    break;
                }
            }
            Color backColor = isSelected ? SystemColors.Highlight : BackColor;
            Color foreColor = isSelected ? SystemColors.HighlightText : ForeColor;
            // if single level 2 node, and it's not selected (i.e., it marks
            // an insertion point), highlight it differently
            if (isInsertion && e.Node == SelectedNode && e.Node.Level == 2) {
                backColor = SystemColors.ControlLight;
                foreColor = Color.Blue;
            }

            using Brush backBrush = new SolidBrush(backColor);
            using Brush foreBrush = new SolidBrush(foreColor);
            e.Graphics.FillRectangle(backBrush, e.Bounds);
            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                Font,
                e.Bounds,
                foreColor,
                TextFormatFlags.GlyphOverhangPadding
            );

            // Draw focus rectangle if needed
            if ((e.State & TreeNodeStates.Focused) != 0) {
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreColor, backColor);
            }
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e) {
            // mouse selection is disabled; selection is handled in OnMouseDown
            switch (e.Action) {
            case TreeViewAction.ByMouse:
                e.Cancel = true;
                break;
            case TreeViewAction.ByKeyboard:
                // need to resync selection/start/stop
                FirstSelectedNode = e.Node;
                LastSelectedNode = e.Node;
                break;
            //case TreeViewAction.Unknown:
            //    break;
            //case TreeViewAction.Collapse:
            //    break;
            //case TreeViewAction.Expand:
            //    break;
            }
            base.OnBeforeSelect(e);
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            TreeNode node = GetNodeAt(e.X, e.Y);
            if (node is not null) {
                // check for right-click within the bounds of the selection
                if (e.Button == MouseButtons.Right) {
                    // if not inside the selection, select the node under the mouse
                    // happens if level changes, OR level is NOT same parent OR index outside first-last bounds
                    if (node.Level != SelectedNode.Level || node.Parent != SelectedNode.Parent ||
                        node.Index < FirstSelectedNode.Index || node.Index > LastSelectedNode.Index) {
                        isInsertion = node.Level == 2;
                        SelectRange(node);
                    }
                }
                else {
                    // check for left button, node level 2, clicked node has same parent as selection
                    if (e.Button == MouseButtons.Left && node.Level == 2 && node.Parent == SelectedNode.Parent) {
                        switch (ModifierKeys) {
                        case Keys.None:
                            // begin multi-select (default to inserting only)
                            isInsertion = true;
                            SelectRange(node);
                            selecting = true;
                            break;
                        case Keys.Control:
                            // full-select the node
                            isInsertion = false;
                            SelectRange(node);
                            break;
                        case Keys.Shift:
                            // extend selection if shift-mouse
                            if (node == SelectedNode) {
                                SelectRange(node);
                            }
                            else {
                                if (SelectedNode.Index > node.Index) {
                                    SelectRange(node, SelectedNode, false);
                                }
                                else {
                                    SelectRange(SelectedNode, node, true);
                                }
                            }
                            break;
                        }
                    }
                    else {
                        // select a single node
                        SelectRange(node);
                        isInsertion = node.Level == 2 && ModifierKeys != Keys.Control;
                    }
                    // repaint the treeview
                    Invalidate();
                }
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            // Find the node under the mouse
            TreeNode node = GetNodeAt(e.X, e.Y);
            // if over selection and left button is down
            if (node == SelectedNode && e.Button == MouseButtons.Left) {
                // begin selection
                selecting = true;
            }
            if (selecting && SelectedNode is not null) {
                // only level 2 in same group can affect selection
                if (node is not null && node.Level == 2 && node.Parent == SelectedNode.Parent) {
                    Rectangle bounds;
                    // save old start and end indices for comparison
                    int oldstart = FirstSelectedNode.Index;
                    int oldend = LastSelectedNode.Index;
                    // determine the start and end indices for the selection range
                    int start = Math.Min(SelectedNode.Index, node.Index);
                    int end = Math.Max(SelectedNode.Index, node.Index);
                    if (FirstSelectedNode.Index != start || LastSelectedNode.Index != end) {
                        // update selection
                        FirstSelectedNode = SelectedNode.Parent.Nodes[start];
                        LastSelectedNode = SelectedNode.Parent.Nodes[end];
                        if (oldstart < start) {
                            bounds = SelectedNode.Parent.Nodes[oldstart].Bounds;
                            for (int i = oldstart + 1; i < start; i++) {
                                bounds = Rectangle.Union(bounds, SelectedNode.Parent.Nodes[i].Bounds);
                            }
                            InvalidateSelectionBounds(bounds);
                        }
                        else if (oldstart > start) {
                            bounds = SelectedNode.Parent.Nodes[start].Bounds;
                            for (int i = start + 1; i < oldstart; i++) {
                                bounds = Rectangle.Union(bounds, SelectedNode.Parent.Nodes[i].Bounds);
                            }
                            InvalidateSelectionBounds(bounds);
                        }
                        if (oldend > end) {
                            bounds = SelectedNode.Parent.Nodes[oldend].Bounds;
                            for (int i = oldend - 1; i > end; i--) {
                                bounds = Rectangle.Union(bounds, SelectedNode.Parent.Nodes[i].Bounds);
                            }
                            InvalidateSelectionBounds(bounds);
                        }
                        else if (oldend < end) {
                            bounds = SelectedNode.Parent.Nodes[end].Bounds;
                            for (int i = end - 1; i > oldend; i--) {
                                bounds = Rectangle.Union(bounds, SelectedNode.Parent.Nodes[i].Bounds);
                            }
                            InvalidateSelectionBounds(bounds);
                        }
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            selecting = false;
            if (FirstSelectedNode != LastSelectedNode) {
                // if last node is included in a multi-selection, de-select it
                if (LastSelectedNode == LastSelectedNode.Parent.Nodes[^1]) {
                    Rectangle bounds = LastSelectedNode.Bounds;
                    bounds = InflateNodeBounds(bounds);
                    if (LastSelectedNode == SelectedNode) {
                        SetSelectedNode(LastSelectedNode.PrevNode);
                        bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                    }
                    LastSelectedNode = LastSelectedNode.PrevNode;
                    Invalidate(bounds, false);
                }
            }
            base.OnMouseUp(e);
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            // if no node is selected, just pass it to the base class
            // if modifier keys is not 'Shift' or 'None', pass it to base class
            // if level is not 2, pass it to base class


            if (SelectedNode is null || SelectedNode.Level != 2 ||
                (ModifierKeys != Keys.Shift && ModifierKeys != Keys.None)) {
                base.OnKeyDown(e);
                return;
            }
            if (ModifierKeys == Keys.None) {
                // NOTE: no action if not shifting but need to resync
                // first/last with Selected in the OnBeforeSelect handler
            }
            else {
                // if arrow up or down with shift, expand or contact selection
                switch (e.KeyCode) {
                case Keys.Up:
                    if (SelectedNode == LastSelectedNode) {
                        if (FirstSelectedNode.PrevNode is not null) {
                            // move start node to next node to expand selection
                            FirstSelectedNode = FirstSelectedNode.PrevNode;
                            Rectangle bounds = FirstSelectedNode.Bounds;
                            InvalidateSelectionBounds(bounds);
                        }
                    }
                    else {
                        // move end node to previous node to reduce selection
                        Rectangle bounds = LastSelectedNode.Bounds;
                        LastSelectedNode = LastSelectedNode.PrevNode;
                        InvalidateSelectionBounds(bounds);
                    }
                    // set handled so builtin selection doesn't happen
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (SelectedNode == FirstSelectedNode) {
                        if (LastSelectedNode.NextNode is not null && LastSelectedNode.NextNode != LastSelectedNode.Parent.Nodes[^1]) {
                            // move end node to next node to expand the selection
                            LastSelectedNode = LastSelectedNode.NextNode;
                            Rectangle bounds = LastSelectedNode.Bounds;
                            InvalidateSelectionBounds(bounds);
                        }
                    }
                    else {
                        Debug.Assert(SelectedNode == LastSelectedNode);
                        // move start node to next node to shrink selection
                        Rectangle bounds = FirstSelectedNode.Bounds;
                        FirstSelectedNode = FirstSelectedNode.NextNode;
                        InvalidateSelectionBounds(bounds);
                    }
                    e.Handled = true;
                    break;
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnAfterSelect(TreeViewEventArgs e) {
            // don't fire the event if we're in the middle of a selection operation
            if (noupdate) {
                return;
            }
            base.OnAfterSelect(e);
        }
        #endregion
    }
}

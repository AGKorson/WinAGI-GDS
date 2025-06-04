using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections;
using System;
using static WinAGI.Common.API;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics.Eventing.Reader;
using System.Collections.Generic;

namespace WinAGI.Editor {
    public class MultiNodeTreeview : TreeView {
        protected List<TreeNode> nodecollection = [];
        protected TreeNode endnode, anchornode;
        private bool selecting;
        private bool noselection;
        private bool forceselection = false;
        private TreeNode forcenode;

        public MultiNodeTreeview() {
            DrawMode = TreeViewDrawMode.OwnerDrawText;
            // Enable double buffering to reduce flicker
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            typeof(TreeView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(this, true, null);
        }

        public bool NoSelection {
            get { return noselection; }
            set {
                if (nodecollection.Count != 1) {
                    return; // only allow this if one node is selected
                }
                if (noselection != value) {
                    noselection = value;
                    // repaint the selection
                    Rectangle bounds = SelectedNode.Bounds;
                    bounds.X -= 2;
                    bounds.Width += 4;
                    Invalidate(bounds, false);
                }
            }
        }

        public List<TreeNode> SelectedNodes {
            get { return nodecollection; }
            set {
                // confirm all nodes are in the treeview, level 
                // 2 nodes only, and same parent node
                if (value == null) {
                    nodecollection.Clear();
                }
                else if (value.Count == 0) {
                    nodecollection.Clear();
                }
                else {
                    int parent = -1;
                    foreach (TreeNode n in value) {
                        if (n.TreeView != this) {
                            throw new ArgumentException("All nodes must be part of this TreeView.", nameof(value));
                        }
                        if (n.Level != 2) {
                            throw new ArgumentException("All nodes must be at level 2.", nameof(value));
                        }
                        if (parent == -1) {
                            parent = n.Parent.Index; // get parent index of first node
                        }
                        else if (n.Parent.Index != parent) {
                            throw new ArgumentException("All nodes must have the same parent.", nameof(value));
                        }
                    }
                    nodecollection = value;
                }
                Invalidate();
            }
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e) {
            // Determine if the node is in selection collection
            bool isSelected = nodecollection.Contains(e.Node);
            // Set colors based on selection state
            Color backColor = isSelected ? SystemColors.Highlight : this.BackColor; 
            Color foreColor = isSelected ? SystemColors.HighlightText : this.ForeColor;
            // if single level 2 node, and it's not selected (i.e., it marks
            // an insertion point), highlight it differently
            if (noselection && e.Node == SelectedNode && e.Node.Level == 2) {
                backColor = SystemColors.ControlLight;
                foreColor = Color.Blue;
            }

            using (Brush backBrush = new SolidBrush(backColor))
            using (Brush foreBrush = new SolidBrush(foreColor)) {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                TextRenderer.DrawText(
                    e.Graphics,
                    e.Node.Text,
                    this.Font,
                    e.Bounds,
                    foreColor,
                    TextFormatFlags.GlyphOverhangPadding
                );
            }

            // Draw focus rectangle if needed
            if ((e.State & TreeNodeStates.Focused) != 0) {
                //ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreColor, backColor);
                ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds, foreColor, backColor);
            }
            //Debug.Print($"anchor: {anchornode.Text}, end: {endnode.Text}, coll: {nodecollection[0].Text}..{nodecollection[^1].Text}");
        }

        protected override void OnMouseDown(MouseEventArgs e) {
            TreeNode node = GetNodeAt(e.X, e.Y);

            // check for right-click within the bounds of the selection
            if (e.Button == MouseButtons.Right) {
                if (nodecollection.Count > 0) {
                    if (nodecollection.Contains(node)) {
                        base.OnMouseDown(e);
                        // re-select the anchor node
                        BeginInvoke((Action)(() => {
                            SelectedNode = anchornode;
                        }));
                        return;
                    }
                }
            }
            noselection = false;
            SelectedNode = node;
            nodecollection.Clear();
            nodecollection.Add(node);
            if (e.Button == MouseButtons.Left && node.Level == 2) {
                switch (ModifierKeys) {
                case Keys.None:
                    // begin multi-select if left button is pressed and node is at level 2
                    // and no modifier keys
                    anchornode = node;
                    endnode = node;
                    selecting = true;
                    // default to inserting only
                    noselection = true;
                    break;
                case Keys.Shift:
                    // if currently on an 'End' node, treat this like a regular
                    // mouse click
                    if (anchornode.Index == anchornode.Parent.Nodes.Count - 1) {
                        anchornode = node;
                        endnode = node;
                        selecting = true;
                        // default to inserting only
                        noselection = true;
                        break;
                    }
                    // extend selection if shift-mouse
                    if (anchornode.Index != node.Index) {
                        TreeNode parent = anchornode.Parent;
                        if (anchornode.Index > node.Index) {
                            for (int i = node.Index + 1; i <= anchornode.Index; i++) {
                                if (i != parent.Nodes.Count - 1) {
                                    nodecollection.Add(parent.Nodes[i]);
                                }
                            }
                            endnode = nodecollection[0];
                        }
                        else if (anchornode.Index < node.Index) {
                            // build list from top to bottom
                            nodecollection.Clear();
                            for (int i = anchornode.Index; i <= node.Index; i++) {
                                if (i != parent.Nodes.Count - 1) {
                                    nodecollection.Add(parent.Nodes[i]);
                                }
                            }
                            endnode = nodecollection[^1];
                        }
                        forcenode = anchornode;
                        forceselection = true;
                    }
                    break;
                }
            }
            // repaint the treeview
            Invalidate();
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (selecting && anchornode != null) {
                // Find the node under the mouse
                TreeNode node = GetNodeAt(e.X, e.Y);
                // only level 2 in same group
                if (node != null && node.Level == 2 && node.Parent == anchornode.Parent) {
                    int oldstart =  Math.Min(anchornode.Index, endnode.Index);
                    int oldend = Math.Max(anchornode.Index, endnode.Index);
                    Rectangle bounds = new();
                    endnode = node;
                    // Get all siblings between m_firstNode and m_lastNode (inclusive)
                    TreeNode parent = anchornode.Parent;
                    int start = Math.Min(anchornode.Index, endnode.Index);
                    int end = Math.Max(anchornode.Index, endnode.Index);

                    if (nodecollection[0].Index != start ||
                    nodecollection[^1].Index != end) {
                        nodecollection.Clear();
                        for (int i = start; i <= end; i++) {
                            nodecollection.Add(parent.Nodes[i]);
                        }
                        // repaint nodes that were removed or added
                        if (oldstart < start) {
                            bounds = anchornode.Parent.Nodes[oldstart].Bounds;
                            for (int i = oldstart + 1; i < start; i++) {
                                bounds = Rectangle.Union(bounds, anchornode.Parent.Nodes[i].Bounds);
                            }
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                        else if (oldstart > start) {
                            bounds = anchornode.Parent.Nodes[start].Bounds;
                            for (int i = start + 1; i < oldstart; i++) {
                                bounds = Rectangle.Union(bounds, anchornode.Parent.Nodes[i].Bounds);
                            }
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                        if (oldend > end) {
                            bounds = anchornode.Parent.Nodes[oldend].Bounds;
                            for (int i = oldend - 1; i > end; i--) {
                                bounds = Rectangle.Union(bounds, anchornode.Parent.Nodes[i].Bounds);
                            }
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                        else if (oldend < end) {
                            bounds = anchornode.Parent.Nodes[end].Bounds;
                            for (int i = end - 1; i > oldend; i--) {
                                bounds = Rectangle.Union(bounds, anchornode.Parent.Nodes[i].Bounds);
                            }
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                    }
                }
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) {
            selecting = false;
            // if last node is included, de-select it
            if (nodecollection.Count > 1) {
                if (nodecollection[^1] == anchornode.Parent.Nodes[^1]) {
                    Rectangle bounds = nodecollection[^1].Bounds;
                    bounds.X -= 2;
                    bounds.Width += 4;
                    if (nodecollection[^1] == SelectedNode) {
                        SelectedNode = nodecollection[^2];
                        bounds = Rectangle.Union(bounds, nodecollection[^2].Bounds);
                    }
                    nodecollection.RemoveAt(nodecollection.Count - 1);
                    Invalidate(bounds, false);
                }
            }
            base.OnMouseUp(e);
            if (forceselection) {
                // if forcing selection, set selection to the forced node
                forceselection = false;
                // re-select the anchor node
                BeginInvoke((Action)(() => {
                    SelectedNode = forcenode;
                }));
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            // check for shift or control keys
            bool bShift = ModifierKeys== Keys.Shift;
            bool bNoShift = ModifierKeys == 0;

            // if arrow up or down, with shift, expand or contact selection
            if (SelectedNode != null && SelectedNode.Level == 2 && bShift) {
                if (e.KeyCode == Keys.Up) {
                    if (anchornode != endnode && anchornode.Index < endnode.Index) {
                        // move end node to previous node to reduce selection
                        Rectangle bounds = endnode.Bounds;
                        endnode = endnode.PrevNode;
                        nodecollection.RemoveAt(nodecollection.Count - 1);
                        if (noselection) {
                            noselection = false;
                            bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                        }
                        bounds.X -= 2;
                        bounds.Width += 4;
                        Invalidate(bounds, false);
                    }
                    else {
                        // move end node to previous node to expand selection
                        if (endnode.PrevNode != null) {
                            endnode = endnode.PrevNode;
                            nodecollection.Insert(0, endnode);
                            Rectangle bounds = endnode.Bounds;
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                    }
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Down) {
                    if (anchornode != endnode && anchornode.Index > endnode.Index) {
                        // move end node to next node to reduce selection
                        Rectangle bounds = endnode.Bounds;
                        endnode = endnode.NextNode;
                        nodecollection.RemoveAt(0);
                        if (noselection) {
                            noselection = false;
                            bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                        }
                        bounds.X -= 2;
                        bounds.Width += 4;
                        Invalidate(bounds, false);
                    }
                    else {
                        if (endnode.NextNode != null) {
                            // move end node to next node to expand selection
                            endnode = endnode.NextNode;
                            nodecollection.Add(endnode);
                            Rectangle bounds = endnode.Bounds;
                            if (noselection) {
                                noselection = false;
                                bounds = Rectangle.Union(bounds, SelectedNode.Bounds);
                            }
                            bounds.X -= 2;
                            bounds.Width += 4;
                            Invalidate(bounds, false);
                        }
                    }
                    e.Handled = true;
                }
            }
            else if (SelectedNode != null && SelectedNode.Level == 2 && bNoShift) {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) {
                    // if there is a selection, deselect it first
                    if (nodecollection.Count > 1) {
                        Rectangle bounds = nodecollection[0].Bounds;
                        for (int i = 1; i < nodecollection.Count; i++) {
                            bounds = Rectangle.Union(bounds, nodecollection[i].Bounds);
                        }
                        bounds.X -= 2;
                        bounds.Width += 4;
                        Invalidate(bounds, false);
                        nodecollection.Clear();
                    }
                }
            }
            base.OnKeyDown(e);
        }

        protected override void OnAfterSelect(TreeViewEventArgs e) {
            // update the selected nodes collection
            if (e.Node == null) {
                return;
            }
            Rectangle bounds;

            if (nodecollection.Count > 0) {
                bounds = nodecollection[0].Bounds;
                for (int i = 1; i < nodecollection.Count; i++) {
                    bounds = Rectangle.Union(bounds, nodecollection[i].Bounds);
                }
                bounds.X -= 2;
                bounds.Width += 4;
                Invalidate(bounds, false);
                if (!nodecollection.Contains(e.Node)) {
                    nodecollection.Clear();
                    nodecollection.Add(e.Node);
                }
            }
            else {
                nodecollection.Add(e.Node);
                noselection = true;
            }
            bounds = nodecollection[0].Bounds;
            bounds.X -= 2;
            bounds.Width += 4;
            Invalidate(bounds, false);
            base.OnAfterSelect(e);
        }
    }
}

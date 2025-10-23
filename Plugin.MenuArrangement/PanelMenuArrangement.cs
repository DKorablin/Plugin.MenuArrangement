using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Plugin.MenuArrangement.Logic;
using SAL.Flatbed;
using SAL.Windows;

namespace Plugin.MenuArrangement
{
	/// <summary>Dialog for rearranging menu items at runtime</summary>
	public partial class PanelMenuArrangement : UserControl
	{
		private Plugin Plugin { get => (Plugin)this.Window.Plugin; }
		private IWindow Window { get => (IWindow)base.Parent; }

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern Int32 SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam);

		private const Int32 WM_VSCROLL = 0x0115;
		private const Int32 SB_LINEUP = 0;
		private const Int32 SB_LINEDOWN = 1;

		private System.Collections.Generic.HashSet<String> _hiddenItems;

		/// <summary>Create instance of menu arrangement dialog</summary>
		/// <param name="menu">Menu to arrange</param>
		public PanelMenuArrangement()
			=> this.InitializeComponent();

		protected override void OnCreateControl()
		{
			this.Window.Caption = "Menu Arrangement";
			this.Window.SetDockAreas(DockAreas.DockBottom
				| DockAreas.DockTop
				| DockAreas.DockRight
				| DockAreas.DockLeft
				| DockAreas.Float);

			this._hiddenItems = this.Plugin.Settings.GetHiddenMenuItemsSet();
			this.LoadMenuItems();

			tsbnReset.Enabled = !String.IsNullOrEmpty(this.Plugin.Settings.CustomMenuOrder) ||
				this._hiddenItems.Count > 0;
			tvMenuItems.ExpandAll();

			base.OnCreateControl();
		}

		/// <summary>Get identifier for menu item</summary>
		private static String GetMenuItemIdentifier(IMenuItem menuItem)
		{
			if(menuItem == null)
				return null;
			return String.IsNullOrEmpty(menuItem.Name) ? menuItem.Text : menuItem.Name;
		}

		/// <summary>Load menu items into the tree view</summary>
		private void LoadMenuItems()
		{
			tvMenuItems.Nodes.Clear();

			foreach(IMenuItem item in this.Plugin.HostWindows.MainMenu.Items)
			{
				TreeNode node = this.CreateTreeNode(item);
				tvMenuItems.Nodes.Add(node);
			}

			if(tvMenuItems.Nodes.Count > 0)
			{
				tvMenuItems.SelectedNode = tvMenuItems.Nodes[0];
				tvMenuItems.SelectedNode.EnsureVisible();
			}
		}

		/// <summary>Create tree node from menu item recursively</summary>
		private TreeNode CreateTreeNode(IMenuItem menuItem)
		{
			if(menuItem == null)
				return null;

			String displayText = GetMenuItemDisplayText(menuItem);
			String identifier = GetMenuItemIdentifier(menuItem);

			TreeNode node = new TreeNode(displayText)
			{
				Tag = menuItem,
				Checked = !this._hiddenItems.Contains(identifier ?? String.Empty)
			};

			// Apply visual styling based on menu item type
			ApplyNodeStyling(node, menuItem);

			// Recursively add child menu items
			if(menuItem is IMenu subMenu && subMenu.Items != null)
			{
				foreach(IMenuItem childItem in subMenu.Items)
				{
					TreeNode childNode = this.CreateTreeNode(childItem);
					node.Nodes.Add(childNode);
				}
			}

			return node;
		}

		/// <summary>Apply visual styling to tree node based on menu item type</summary>
		private static void ApplyNodeStyling(TreeNode node, IMenuItem menuItem)
		{
			if(menuItem == null)
				return;

			// Get the underlying object
			Object underlyingObject = null;
			if(menuItem is IHostItem hostItem)
				underlyingObject = hostItem.Object;

			// Apply different colors/styles for different types
			if(underlyingObject is ToolStripSeparator)
			{
				node.ForeColor = System.Drawing.Color.Gray;
			} /*else if(underlyingObject is ToolStripMenuItem)
			{
			}*/ else if(underlyingObject is ToolStripButton ||
					  underlyingObject is ToolStripLabel ||
					  underlyingObject is ToolStripComboBox ||
					  underlyingObject is ToolStripTextBox)
			{
				// Special items - show in different color
				node.ForeColor = System.Drawing.Color.Blue;
			}
		}

		/// <summary>Get display text for menu item based on its type</summary>
		private static String GetMenuItemDisplayText(IMenuItem menuItem)
		{
			if(menuItem == null)
				return "[Unknown]";

			// Get the underlying object
			Object underlyingObject = null;
			if(menuItem is IHostItem hostItem)
				underlyingObject = hostItem.Object;

			// Handle different menu item types
			if(underlyingObject is ToolStripSeparator)
				return "─────────────";
			else if(underlyingObject is ToolStripMenuItem)
			{
				String text = menuItem.Text;
				return String.IsNullOrEmpty(text) ? "[Empty Menu Item]" : text;
			} else if(underlyingObject is ToolStripButton)
				return "[Button: " + menuItem.Text + "]";
			else if(underlyingObject is ToolStripLabel)
				return "[Label: " + menuItem.Text + "]";
			else if(underlyingObject is ToolStripComboBox)
				return "[ComboBox: " + menuItem.Text + "]";
			else if(underlyingObject is ToolStripTextBox)
				return "[TextBox: " + menuItem.Text + "]";
			else
			{
				// Default fallback
				String text = menuItem.Text;
				return String.IsNullOrEmpty(text) ? "[Menu Item]" : text;
			}
		}

		/// <summary>Update button states based on selection</summary>
		private void tvMenuItems_AfterSelect(Object sender, TreeViewEventArgs e)
		{
			if(tvMenuItems.SelectedNode == null)
			{
				tsbnMoveUp.Enabled = false;
				tsbnMoveDown.Enabled = false;
				tsbnIndent.Enabled = false;
				tsbnOutdent.Enabled = false;
				return;
			}

			TreeNode selectedNode = tvMenuItems.SelectedNode;
			TreeNode parentNode = selectedNode.Parent;

			// Move up/down enabled if not at edges
			if(parentNode == null)
			{
				// Top-level node
				tsbnMoveUp.Enabled = selectedNode.Index > 0;
				tsbnMoveDown.Enabled = selectedNode.Index < tvMenuItems.Nodes.Count - 1;
			} else
			{
				// Child node
				tsbnMoveUp.Enabled = selectedNode.Index > 0;
				tsbnMoveDown.Enabled = selectedNode.Index < parentNode.Nodes.Count - 1;
			}

			// Indent: can indent if there's a previous sibling to become child of
			tsbnIndent.Enabled = selectedNode.Index > 0;

			// Outdent: can outdent if there's a parent (not top-level)
			tsbnOutdent.Enabled = parentNode != null;
		}

		/// <summary>Get index of menu item inside collection</summary>
		private static Int32 GetItemIndex(IMenuItemCollection collection, IMenuItem item)
		{
			if(collection == null || item == null)
				return -1;
			for(Int32 i = 0; i < collection.Count; i++)
				if(collection[i] == item)
					return i;
			return -1;
		}

		/// <summary>Move a tree node and underlying menu item to a new parent/index</summary>
		/// <param name="node">Node to move</param>
		/// <param name="newParent">Target parent node (null for top-level)</param>
		/// <param name="newIndex">Target index within parent (ignored if parent collection is null)</param>
		private void MoveNode(TreeNode node, TreeNode newParent, Int32 newIndex)
		{
			if(node == null)
				return;

			if(!(node.Tag is IMenuItem menuItem))
				return;

			// Determine current parent collection
			IMenuItemCollection currentCollection;
			TreeNode oldParentNode = node.Parent;
			if(oldParentNode == null)
				currentCollection = this.Plugin.HostWindows.MainMenu.Items;
			else
			{
				IMenuItem oldParentItem = oldParentNode.Tag as IMenuItem;
				currentCollection = oldParentItem?.Items;
			}

			// Determine target parent collection
			IMenuItemCollection targetCollection;
			if(newParent == null)
				targetCollection = this.Plugin.HostWindows.MainMenu.Items;
			else
			{
				IMenuItem newParentItem = newParent.Tag as IMenuItem;
				targetCollection = newParentItem?.Items;
			}

			// If target collection is null (not a submenu) we cannot move as child
			if(targetCollection == null)
				return;

			// Remove from current tree collection
			if(oldParentNode == null)
				this.tvMenuItems.Nodes.Remove(node);
			else
				oldParentNode.Nodes.Remove(node);

			// Remove from current menu collection
			Int32 oldIndex = GetItemIndex(currentCollection, menuItem);
			if(oldIndex >= 0)
				currentCollection.RemoveAt(oldIndex);

			// Adjust index if moving within same collection and original index was before insert position
			if(currentCollection == targetCollection && oldIndex >= 0 && oldIndex < newIndex)
				newIndex--;

			// Insert into tree
			TreeNodeCollection targetNodes = newParent == null ? this.tvMenuItems.Nodes : newParent.Nodes;
			if(newIndex < 0 || newIndex > targetNodes.Count)
				targetNodes.Add(node);
			else
				targetNodes.Insert(newIndex, node);

			// Insert into menu collection
			if(newIndex < 0 || newIndex > targetCollection.Count)
				targetCollection.Add(menuItem);
			else
				targetCollection.Insert(newIndex, menuItem);

			// Expand parent to show moved node
			newParent?.Expand();

			// Reselect moved node
			this.tvMenuItems.SelectedNode = node;
			CustomMenuOrderStorage.SaveMenuOrder(this.Plugin);
		}

		/// <summary>Move selected item up</summary>
		private void btnMoveUp_Click(Object sender, EventArgs e)
		{
			TreeNode selectedNode = tvMenuItems.SelectedNode;
			if(selectedNode == null || selectedNode.Index == 0)
				return;
			TreeNode parentNode = selectedNode.Parent;
			Int32 newIndex = selectedNode.Index - 1;
			MoveNode(selectedNode, parentNode, newIndex);
		}

		/// <summary>Move selected item down</summary>
		private void tsbnMoveDown_Click(Object sender, EventArgs e)
		{
			TreeNode selectedNode = tvMenuItems.SelectedNode;
			if(selectedNode == null)
				return;
			TreeNode parentNode = selectedNode.Parent;
			Int32 maxIndex = parentNode == null ? tvMenuItems.Nodes.Count - 1 : parentNode.Nodes.Count - 1;
			if(selectedNode.Index >= maxIndex)
				return;
			Int32 newIndex = selectedNode.Index + 1;
			MoveNode(selectedNode, parentNode, newIndex);
		}

		/// <summary>Indent selected item (make it child of previous sibling)</summary>
		private void tsbnIndent_Click(Object sender, EventArgs e)
		{
			TreeNode selectedNode = tvMenuItems.SelectedNode;
			if(selectedNode == null || selectedNode.Index == 0)
				return;
			TreeNode parentNode = selectedNode.Parent;
			TreeNode newParentNode = parentNode == null
				? tvMenuItems.Nodes[selectedNode.Index - 1]
				: parentNode.Nodes[selectedNode.Index - 1];
			// Add as last child
			Int32 newIndex = newParentNode.Nodes.Count; // append
			MoveNode(selectedNode, newParentNode, newIndex);
		}

		/// <summary>Outdent selected item (move it up one level)</summary>
		private void tsbnOutdent_Click(Object sender, EventArgs e)
		{
			TreeNode selectedNode = tvMenuItems.SelectedNode;
			if(selectedNode == null)
				return;
			TreeNode parentNode = selectedNode.Parent;
			if(parentNode == null)
				return; // already top-level
			TreeNode grandParentNode = parentNode.Parent;
			// Insert after parent in grandparent (or root)
			Int32 parentIndex = parentNode.Index;
			Int32 insertIndex = parentIndex + 1;
			MoveNode(selectedNode, grandParentNode, insertIndex);
		}

		/// <summary>Drag and drop support - begin drag</summary>
		private void tvMenuItems_ItemDrag(Object sender, ItemDragEventArgs e)
		{
			if(e.Item is TreeNode node)
			{
				tvMenuItems.SelectedNode = node;
				tvMenuItems.DoDragDrop(node, DragDropEffects.Move);
			}
		}

		/// <summary>Drag and drop support - drag enter</summary>
		private void tvMenuItems_DragEnter(Object sender, DragEventArgs e)
		{
			e.Effect = e.Data.GetDataPresent(typeof(TreeNode))
				? DragDropEffects.Move
				: DragDropEffects.None;
		}

		/// <summary>Drag and drop support - drag over</summary>
		private void tvMenuItems_DragOver(Object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(typeof(TreeNode)))
			{
				e.Effect = DragDropEffects.None;
				return;
			}

			Point pt = tvMenuItems.PointToClient(new Point(e.X, e.Y));
			TreeNode targetNode = tvMenuItems.GetNodeAt(pt);

			if(targetNode != null)
			{
				tvMenuItems.SelectedNode = targetNode;
				e.Effect = DragDropEffects.Move;
			} else
				e.Effect = DragDropEffects.None;

			Int32 scrollRegion = 20; // pixel area near the edge for scrolling
			if(pt.Y < scrollRegion)
			{
				// Scroll up
				SendMessage(tvMenuItems.Handle, WM_VSCROLL, SB_LINEUP, 0);
			} else if(pt.Y > tvMenuItems.Height - scrollRegion)
			{
				// Scroll down
				SendMessage(tvMenuItems.Handle, WM_VSCROLL, SB_LINEDOWN, 0);
			}
		}

		/// <summary>Drag and drop support - drop item</summary>
		private void tvMenuItems_DragDrop(Object sender, DragEventArgs e)
		{
			if(!e.Data.GetDataPresent(typeof(TreeNode)))
				return;

			TreeNode dragNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
			Point pt = tvMenuItems.PointToClient(new Point(e.X, e.Y));
			TreeNode targetNode = tvMenuItems.GetNodeAt(pt);

			if(targetNode == null || dragNode == targetNode)
				return;

			// Don't allow dropping on own descendants
			if(IsDescendant(targetNode, dragNode))
				return;

			// Determine drop position based on mouse Y relative to target node bounds
			Int32 nodeHeight = targetNode.Bounds.Height;
			Int32 relativeY = pt.Y - targetNode.Bounds.Y;

			TreeNode parentNode;
			Int32 insertIndex;

			if(relativeY < nodeHeight / 3)
			{
				// Drop before target
				parentNode = targetNode.Parent;
				insertIndex = targetNode.Index;
			} else if(relativeY > 2 * nodeHeight / 3)
			{
				// Drop after target
				parentNode = targetNode.Parent;
				insertIndex = targetNode.Index + 1;
			} else
			{
				// Drop as child of target
				parentNode = targetNode;
				insertIndex = targetNode.Nodes.Count;
			}

			MoveNode(dragNode, parentNode, insertIndex);
		}

		/// <summary>Handle checkbox state changes</summary>
		private void tvMenuItems_AfterCheck(Object sender, TreeViewEventArgs e)
		{
			if(e.Node == null || e.Node.Tag == null)
				return;

			if(e.Node.Tag is IMenuItem menuItem)
			{
				String identifier = GetMenuItemIdentifier(menuItem);
				if(String.IsNullOrEmpty(identifier))
					return;

				if(e.Node.Checked)
					this._hiddenItems.Remove(identifier);
				else
					this._hiddenItems.Add(identifier);

				if(menuItem.Object is ToolStripItem toolStripItem)
					toolStripItem.Visible = e.Node.Checked;

				this.Plugin.Settings.SetHiddenMenuItemsSet(this._hiddenItems);
			}
		}

		private void tvMenuItems_KeyDown(Object sender, KeyEventArgs e)
		{
			switch(e.KeyCode)
			{
			case Keys.Up:
				if(e.Control && tsbnMoveUp.Enabled)
					this.btnMoveUp_Click(sender, e);
				break;
			case Keys.Down:
				if(e.Control && tsbnMoveDown.Enabled)
					this.tsbnMoveDown_Click(sender, e);
				break;
			case Keys.Right:
				if(e.Control && tsbnIndent.Enabled)
					this.tsbnIndent_Click(sender, e);
				break;
			case Keys.Left:
				if(e.Control && tsbnOutdent.Enabled)
					this.tsbnOutdent_Click(sender, e);
				break;
			}
		}

		/// <summary>Check if node is descendant of another node</summary>
		private static Boolean IsDescendant(TreeNode node, TreeNode potentialAncestor)
		{
			TreeNode current = node;
			while(current != null)
			{
				if(current == potentialAncestor)
					return true;
				current = current.Parent;
			}
			return false;
		}

		private void btnReset_Click(Object sender, EventArgs e)
		{
			this.Plugin.Settings.CustomMenuOrder = null;
			this.Plugin.Settings.SetHiddenMenuItemsSet(null);
			_hiddenItems.Clear();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using POCOGenerator.Objects;

using POCOGeneratorUI.Filtering;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Context Menu

		private void trvServer_MouseUp(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				Point point = new(e.X, e.Y);
				TreeNode node = trvServer.GetNodeAt(point);
				if (node != null)
				{
					trvServer.SelectedNode = node;
					if (node.Tag is Database)
					{
						removeFilterToolStripMenuItem.Visible = false;
						filterSettingsToolStripMenuItem.Visible = false;
						clearCheckBoxesToolStripMenuItem.Visible = true;
						contextMenu.Show(trvServer, point);
					}
					else if (
						node.Tag is IEnumerable<Table> or
						IEnumerable<POCOGenerator.Objects.View> or
						IEnumerable<Procedure> or
						IEnumerable<Function> or
						IEnumerable<TVP>)
					{
						// stop the user from filtering unless the node was expanded
						// this prevents error when exporting
						if (node.Nodes.Count > 0)
						{
							removeFilterToolStripMenuItem.Visible = true;
							filterSettingsToolStripMenuItem.Visible = true;
							clearCheckBoxesToolStripMenuItem.Visible = true;
							removeFilterToolStripMenuItem.Enabled = filters.ContainsKey(node) && filters[node].IsEnabled;
							contextMenu.Show(trvServer, point);
						}
						else if (node.IsExpanded)
						{
							removeFilterToolStripMenuItem.Visible = true;
							filterSettingsToolStripMenuItem.Visible = true;
							clearCheckBoxesToolStripMenuItem.Visible = false;
							removeFilterToolStripMenuItem.Enabled = filters.ContainsKey(node) && filters[node].IsEnabled;
							contextMenu.Show(trvServer, point);
						}
						else
						{
							contextMenu.Hide();
							contextMenuTable.Hide();
						}
					}
					else if (node.Tag is Table table)
					{
						string tableName = table.ToString();
						checkReferencedTablesToolStripMenuItem.Text = "Check Tables Referenced From " + tableName;
						checkReferencingTablesToolStripMenuItem.Text = "Check Tables Referencing To " + tableName;
						checkAccessibleTablesToolStripMenuItem.Text = "Check Recursively Tables Accessible From && To " + tableName;
						contextMenuTable.Show(trvServer, point);
					}
					else
					{
						contextMenu.Hide();
						contextMenuTable.Hide();
					}
				}
				else
				{
					contextMenu.Hide();
					contextMenuTable.Hide();
				}
			}
		}

		#region Filters

		private readonly Dictionary<TreeNode, FilterSettings> filters = [];

		private void removeFilterToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode parent = trvServer.SelectedNode;
			if (parent == null)
			{
				return;
			}
			if (!filters.ContainsKey(parent))
			{
				return;
			}
			FilterSettings filterSettings = filters[parent];
			if (filterSettings.IsEnabled)
			{
				filters.Remove(parent);
				parent.ShowAll();
				parent.Text = parent.Text.Replace(" (filtered)", String.Empty);
				CheckDbObjectsNodeAfterFilter(parent);
			}
		}

		private void filterSettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode parent = trvServer.SelectedNode;
			if (parent == null)
			{
				return;
			}
			FilterSettings filterSettings = filters.ContainsKey(parent) ? filters[parent] : new FilterSettings();
			FilterSettingsForm filterSettingsForm = new(filterSettings, _generator.Support.SupportSchema);
			if (filterSettingsForm.ShowDialog(this) == DialogResult.OK)
			{
				filterSettings.FilterName.FilterType = filterSettingsForm.FilterTypeName;
				filterSettings.FilterName.Filter = filterSettingsForm.FilterName;
				filterSettings.FilterSchema.FilterType = filterSettingsForm.FilterTypeSchema;
				filterSettings.FilterSchema.Filter = filterSettingsForm.FilterSchema;
				if (filterSettings.IsEnabled)
				{
					if (!filters.ContainsKey(parent))
					{
						filters.Add(parent, filterSettings);
					}
					SetFilteredNodesVisibility(parent, filterSettings);
				}
				else
				{
					if (filters.ContainsKey(parent))
					{
						filters.Remove(parent);
					}
					parent.ShowAll();
					parent.Text = parent.Text.Replace(" (filtered)", String.Empty);
					CheckDbObjectsNodeAfterFilter(parent);
				}
			}
		}

		private void SetFilteredNodesVisibility(TreeNode parent, FilterSettings filterSettings)
		{
			List<TreeNode> toShow = [];
			List<TreeNode> toHide = [];
			foreach (TreeNode node in parent.Nodes)
			{
				(IsMatchFilter(node.Tag as POCOGenerator.Objects.IDbObject, filterSettings) ? toShow : toHide).Add(node);
			}
			bool isUncheckedHiddenNode = false;
			if (toHide.HasAny())
			{
				DisableServerTreeAfterCheck();
				foreach (TreeNode node in toHide)
				{
					isUncheckedHiddenNode |= node.Checked;
					node.Checked = false;
					node.Hide();
				}
				EnableServerTreeAfterCheck();
			}
			if (toShow.HasAny())
			{
				foreach (TreeNode node in toShow)
				{
					node.Show();
				}
			}
			if (!parent.Text.Contains(" (filtered)"))
			{
				parent.Text += " (filtered)";
			}
			CheckDbObjectsNodeAfterFilter(parent);
			if (isUncheckedHiddenNode)
			{
				TreeNodeChecked();
			}
		}

		private bool IsMatchFilter(POCOGenerator.Objects.IDbObject dbObject, FilterSettings filterSettings)
		{
			if (filterSettings.FilterName.IsEnabled)
			{
				if (filterSettings.FilterName.FilterType == FilterType.Equals)
				{
					if (String.Compare(dbObject.Name, filterSettings.FilterName.Filter, true) != 0)
					{
						return false;
					}
				}
				else if (filterSettings.FilterName.FilterType == FilterType.Contains)
				{
					if (dbObject.Name.IndexOf(filterSettings.FilterName.Filter, StringComparison.OrdinalIgnoreCase) == -1)
					{
						return false;
					}
				}
				else if (filterSettings.FilterName.FilterType == FilterType.DoesNotContain)
				{
					if (dbObject.Name.IndexOf(filterSettings.FilterName.Filter, StringComparison.OrdinalIgnoreCase) != -1)
					{
						return false;
					}
				}
			}
			if (_generator.Support.SupportSchema)
			{
				if (filterSettings.FilterSchema.IsEnabled)
				{
					if (filterSettings.FilterSchema.FilterType == FilterType.Equals)
					{
						if (String.Compare(dbObject.Schema, filterSettings.FilterSchema.Filter, true) != 0)
						{
							return false;
						}
					}
					else if (filterSettings.FilterSchema.FilterType == FilterType.Contains)
					{
						if (dbObject.Schema.IndexOf(filterSettings.FilterSchema.Filter, StringComparison.OrdinalIgnoreCase) == -1)
						{
							return false;
						}
					}
					else if (filterSettings.FilterSchema.FilterType == FilterType.DoesNotContain)
					{
						if (dbObject.Schema.IndexOf(filterSettings.FilterSchema.Filter, StringComparison.OrdinalIgnoreCase) != -1)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private void CheckDbObjectsNodeAfterFilter(TreeNode parent)
		{
			bool toCheckParent = !parent.Checked &&
					parent.Nodes.Count > 0 &&
					parent.Nodes.Cast<TreeNode>().All(n => n.Checked)
				;
			bool toUncheckParent =
				parent.Checked && (
									  parent.Nodes.Count == 0 ||
									  parent.Nodes.Cast<TreeNode>().Any(n => !n.Checked)
								  );
			if (toCheckParent || toUncheckParent)
			{
				bool isChecked = toCheckParent;
				if (parent.Tag is not null and IEnumerable<Table>)
				{
					isCheckedTables = isChecked;
				}
				else if (parent.Tag is not null and IEnumerable<POCOGenerator.Objects.View>)
				{
					isCheckedViews = isChecked;
				}
				else if (parent.Tag is not null and IEnumerable<Procedure>)
				{
					isCheckedProcedures = isChecked;
				}
				else if (parent.Tag is not null and IEnumerable<Function>)
				{
					isCheckedFunctions = isChecked;
				}
				else if (parent.Tag is not null and IEnumerable<TVP>)
				{
					isCheckedTVPs = isChecked;
				}
				DisableServerTreeAfterCheck();
				parent.Checked = isChecked;
				EnableServerTreeAfterCheck();
			}
		}

		#endregion

		#region Check Boxes

		private void clearCheckBoxesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode node = trvServer.SelectedNode;
			if (node == null)
			{
				return;
			}
			if (node.Tag is Database)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				DatabaseAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
			else if (node.Tag is IEnumerable<Table>)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				TablesAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
			else if (node.Tag is IEnumerable<POCOGenerator.Objects.View>)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				ViewsAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
			else if (node.Tag is IEnumerable<Procedure>)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				ProceduresAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
			else if (node.Tag is IEnumerable<Function>)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				FunctionsAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
			else if (node.Tag is IEnumerable<TVP>)
			{
				DisableServerTreeAfterCheck();
				node.Checked = false;
				TVPsAfterCheck(node, false);
				EnableServerTreeAfterCheck();
				TreeNodeChecked();
			}
		}

		#endregion

		#region Table-Connected Check Boxes

		private void checkReferencedTablesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode node = trvServer.SelectedNode;
			if (node == null)
			{
				return;
			}
			if (node.Tag is not Table table)
			{
				return;
			}
			DisableServerTreeAfterCheck();
			List<Table> primaryTables = table.ForeignKeys.Select(fk => fk.PrimaryTable).Where(pt => pt != null).ToList();
			List<TreeNode> toCheck = [];
			foreach (TreeNode n in node.Parent.Nodes)
			{
				if (primaryTables.Remove((Table)n.Tag))
				{
					if (!n.Checked)
					{
						toCheck.Add(n);
					}
					if (primaryTables.IsNullOrEmpty())
					{
						break;
					}
				}
			}
			if (toCheck.HasAny())
			{
				foreach (TreeNode tableNode in toCheck)
				{
					tableNode.Checked = true;
				}
				TreeNodeChecked();
			}
			EnableServerTreeAfterCheck();
		}

		private void checkReferencingTablesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode node = trvServer.SelectedNode;
			if (node == null)
			{
				return;
			}
			if (node.Tag is not Table table)
			{
				return;
			}
			DisableServerTreeAfterCheck();
			List<TreeNode> toCheck = [];
			foreach (TreeNode n in node.Parent.Nodes)
			{
				if (((Table)n.Tag).ForeignKeys.Any(fk => fk.PrimaryTable == table))
				{
					if (!n.Checked)
					{
						toCheck.Add(n);
					}
				}
			}
			if (toCheck.HasAny())
			{
				foreach (TreeNode tableNode in toCheck)
				{
					tableNode.Checked = true;
				}
				TreeNodeChecked();
			}
			EnableServerTreeAfterCheck();
		}

		private void checkAccessibleTablesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TreeNode node = trvServer.SelectedNode;
			if (node == null)
			{
				return;
			}
			if (node.Tag is not Table table)
			{
				return;
			}
			DisableServerTreeAfterCheck();
			List<Table> accessibleTables = [table];
			int fromIndex = 0;
			int toIndex = 0;
			while (fromIndex <= toIndex)
			{
				while (fromIndex <= toIndex)
				{
					for (int i = fromIndex; i <= toIndex; i++)
					{
						foreach (ForeignKey fk in accessibleTables[i].ForeignKeys)
						{
							if (!accessibleTables.Contains(fk.PrimaryTable))
							{
								accessibleTables.Add(fk.PrimaryTable);
							}
						}
					}
					fromIndex = toIndex + 1;
					toIndex = accessibleTables.Count - 1;
				}
				foreach (TreeNode n in node.Parent.Nodes)
				{
					Table t = (Table)n.Tag;
					if (!accessibleTables.Contains(t))
					{
						if (t.ForeignKeys.Any(fk => accessibleTables.Contains(fk.PrimaryTable)))
						{
							accessibleTables.Add(t);
						}
					}
				}
				toIndex = accessibleTables.Count - 1;
			}
			List<TreeNode> toCheck = [];
			foreach (TreeNode n in node.Parent.Nodes)
			{
				Table t = (Table)n.Tag;
				if (accessibleTables.Remove(t))
				{
					if (!n.Checked)
					{
						toCheck.Add(n);
					}
					if (accessibleTables.IsNullOrEmpty())
					{
						break;
					}
				}
			}
			if (toCheck.HasAny())
			{
				foreach (TreeNode tableNode in toCheck)
				{
					tableNode.Checked = true;
				}
				TreeNodeChecked();
			}
			EnableServerTreeAfterCheck();
		}

		#endregion

		#endregion
	}
}

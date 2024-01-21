using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using POCOGenerator;
using POCOGenerator.Objects;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Generate POCOs

		private bool generatePOCOsFromCheckedTreeNodes;
		private TreeNode currentSelectedNode;
		private readonly List<Table> selectedTables = [];
		private readonly List<POCOGenerator.Objects.View> selectedViews = [];
		private readonly List<Procedure> selectedProcedures = [];
		private readonly List<Function> selectedFunctions = [];
		private readonly List<TVP> selectedTVPs = [];

		private void GetSelectedDbObjects()
		{
			selectedTables.Clear();
			selectedViews.Clear();
			selectedProcedures.Clear();
			selectedFunctions.Clear();
			selectedTVPs.Clear();
			TreeNode serverNode = trvServer.Nodes[0];
			foreach (TreeNode databaseNode in serverNode.Nodes)
			{
				foreach (TreeNode dbObjectsNode in databaseNode.Nodes)
				{
					if (dbObjectsNode.Tag is IEnumerable<Table>)
					{
						GetCheckedDbObjects<Table>(dbObjectsNode, selectedTables);
					}
					else if (dbObjectsNode.Tag is IEnumerable<POCOGenerator.Objects.View>)
					{
						GetCheckedDbObjects<POCOGenerator.Objects.View>(dbObjectsNode, selectedViews);
					}
					else if (dbObjectsNode.Tag is IEnumerable<Procedure>)
					{
						GetCheckedDbObjects<Procedure>(dbObjectsNode, selectedProcedures);
					}
					else if (dbObjectsNode.Tag is IEnumerable<Function>)
					{
						GetCheckedDbObjects<Function>(dbObjectsNode, selectedFunctions);
					}
					else if (dbObjectsNode.Tag is IEnumerable<TVP>)
					{
						GetCheckedDbObjects<TVP>(dbObjectsNode, selectedTVPs);
					}
				}
			}
		}

		private void GetCheckedDbObjects<T>(TreeNode dbObjectsNode, List<T> lst)
			where T : POCOGenerator.Objects.IDbObject
		{
			foreach (TreeNode dbObjectNode in dbObjectsNode.Nodes)
			{
				if (dbObjectNode.Checked)
				{
					lst.Add((T)dbObjectNode.Tag);
				}
			}
		}

		private void trvServer_AfterSelect(object sender, TreeViewEventArgs e) => TreeNodeSelected();

		private void TreeNodeSelected()
		{
			ClearStatus();
			if (generatePOCOsFromCheckedTreeNodes)
			{
				return;
			}
			GeneratorResults results = GeneratePOCOs();
			if (results == GeneratorResults.NoDbObjectsIncluded)
			{
				txtPocoEditor.Clear();
				PrintDatabaseErrors();
			}
		}

		private void PrintDatabaseErrors()
		{
			TreeNode selectedNode = trvServer.SelectedNode;
			if (selectedNode != null && selectedNode.Tag != null)
			{
				if (selectedNode.Tag is Database database)
				{
					if (database.Errors.Any())
					{
						txtPocoEditor.Select(txtPocoEditor.TextLength, 0);
						txtPocoEditor.SelectionColor = _generator.Settings.SyntaxHighlight.Error;
						txtPocoEditor.SelectedText = string.Join(Environment.NewLine + Environment.NewLine, database.Errors);
						txtPocoEditor.SelectionColor = _generator.Settings.SyntaxHighlight.Text;
					}
				}
			}
		}

		private void POCOOptionChanged()
		{
			ClearStatus();
			GeneratorResults results = GeneratePOCOs(forceGeneratingPOCOs: true);
			if (results == GeneratorResults.NoDbObjectsIncluded)
			{
				txtPocoEditor.Clear();
			}
		}

		private void TreeNodeChecked()
		{
			ClearStatus();
			GetSelectedDbObjects();
			GeneratorResults results = GeneratePOCOs();
			if (results == GeneratorResults.NoDbObjectsIncluded)
			{
				txtPocoEditor.Clear();
			}
		}

		private void ExportPOCOs([InstantHandle] Action<IGenerator> BeforeGeneratePOCOs = null,
								 [InstantHandle] Action<IGenerator> AfterGeneratePOCOs = null) =>
			GeneratePOCOs(false, true, BeforeGeneratePOCOs, AfterGeneratePOCOs);

		private GeneratorResults GeneratePOCOs(bool forceGeneratingPOCOs = false, bool isExport = false,
											   [InstantHandle] Action<IGenerator> BeforeGeneratePOCOs = null,
											   [InstantHandle] Action<IGenerator> AfterGeneratePOCOs = null)
		{
			generatePOCOsFromCheckedTreeNodes =
				this.selectedTables.HasAny() ||
				this.selectedViews.HasAny() ||
				this.selectedProcedures.HasAny() ||
				this.selectedFunctions.HasAny() ||
				this.selectedTVPs.HasAny();
			List<Table> selectedTables = null;
			List<POCOGenerator.Objects.View> selectedViews = null;
			List<Procedure> selectedProcedures = null;
			List<Function> selectedFunctions = null;
			List<TVP> selectedTVPs = null;
			List<ComplexTypeTable> selectedComplexTypeTables = null;
			if (generatePOCOsFromCheckedTreeNodes)
			{
				selectedTables = new List<Table>(this.selectedTables);
				selectedViews = new List<POCOGenerator.Objects.View>(this.selectedViews);
				selectedProcedures = new List<Procedure>(this.selectedProcedures);
				selectedFunctions = new List<Function>(this.selectedFunctions);
				selectedTVPs = new List<TVP>(this.selectedTVPs);
			}
			else
			{
				TreeNode selectedNode = GetSelectedNode();
				if (selectedNode != null)
				{
					if (isExport == false)
					{
						if (forceGeneratingPOCOs == false)
						{
							if (currentSelectedNode == selectedNode)
							{
								return GeneratorResults.None;
							}
						}
					}
					if (selectedNode.Tag is Table table)
					{
						selectedTables = [table];
					}
					else if (selectedNode.Tag is POCOGenerator.Objects.View view)
					{
						selectedViews = [view];
					}
					else if (selectedNode.Tag is Procedure procedure)
					{
						selectedProcedures = [procedure];
					}
					else if (selectedNode.Tag is Function function)
					{
						selectedFunctions = [function];
					}
					else if (selectedNode.Tag is TVP tvp)
					{
						selectedTVPs = [tvp];
					}
					else
					{
						if (isExport == false)
						{
							currentSelectedNode = null;
						}
						return GeneratorResults.NoDbObjectsIncluded;
					}
					if (isExport == false)
					{
						currentSelectedNode = selectedNode;
					}
				}
				else
				{
					if (isExport == false)
					{
						currentSelectedNode = null;
					}
					return GeneratorResults.NoDbObjectsIncluded;
				}
			}
			if (chkComplexTypes.Checked && selectedTables.HasAny())
			{
				selectedComplexTypeTables =
					new List<ComplexTypeTable>(selectedTables.SelectMany(t => t.ComplexTypeTables).Distinct().OrderBy(t => t.Name));
			}
			List<Database> selectedDatabases =
				(selectedTables.HasAny() ? selectedTables.Select(t => t.Database) : Enumerable.Empty<Database>())
				.Union(selectedViews.HasAny() ? selectedViews.Select(v => v.Database) : Enumerable.Empty<Database>())
				.Union(selectedProcedures.HasAny() ? selectedProcedures.Select(p => p.Database) : Enumerable.Empty<Database>())
				.Union(selectedFunctions.HasAny() ? selectedFunctions.Select(f => f.Database) : Enumerable.Empty<Database>())
				.Union(selectedTVPs.HasAny() ? selectedTVPs.Select(t => t.Database) : Enumerable.Empty<Database>())
				.Distinct()
				.ToList();

			void databaseGenerating(object sendeer, DatabaseGeneratingEventArgs e) =>
				e.Skip = selectedDatabases.Contains(e.Database) == false;

			void tablesGenerating(object sender, TablesGeneratingEventArgs e)
			{
				e.Skip = selectedTables.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void tableGenerating(object sender, TableGeneratingEventArgs e) => e.Skip = selectedTables.Contains(e.Table) == false;

			void tableGenerated(object sender, TableGeneratedEventArgs e)
			{
				selectedTables.Remove(e.Table);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void complexTypeTablesGenerating(object sender, ComplexTypeTablesGeneratingEventArgs e)
			{
				e.Skip = selectedComplexTypeTables.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void complexTypeTableGenerating(object sender, ComplexTypeTableGeneratingEventArgs e) =>
				e.Skip = selectedComplexTypeTables.Contains(e.ComplexTypeTable) == false;

			void complexTypeTableGenerated(object sender, ComplexTypeTableGeneratedEventArgs e)
			{
				selectedComplexTypeTables.Remove(e.ComplexTypeTable);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void viewsGenerating(object sender, ViewsGeneratingEventArgs e)
			{
				e.Skip = selectedViews.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void viewGenerating(object sender, ViewGeneratingEventArgs e) => e.Skip = selectedViews.Contains(e.View) == false;

			void viewGenerated(object sender, ViewGeneratedEventArgs e)
			{
				selectedViews.Remove(e.View);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void proceduresGenerating(object sender, ProceduresGeneratingEventArgs e)
			{
				e.Skip = selectedProcedures.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void procedureGenerating(object sender, ProcedureGeneratingEventArgs e) =>
				e.Skip = selectedProcedures.Contains(e.Procedure) == false;

			void procedureGenerated(object sender, ProcedureGeneratedEventArgs e)
			{
				selectedProcedures.Remove(e.Procedure);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void functionsGenerating(object sender, FunctionsGeneratingEventArgs e)
			{
				e.Skip = selectedFunctions.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void functionGenerating(object sender, FunctionGeneratingEventArgs e) =>
				e.Skip = selectedFunctions.Contains(e.Function) == false;

			void functionGenerated(object sender, FunctionGeneratedEventArgs e)
			{
				selectedFunctions.Remove(e.Function);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void tvpsGenerating(object sender, TVPsGeneratingEventArgs e)
			{
				e.Skip = selectedTVPs.IsNullOrEmpty();
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void tvpGenerating(object sender, TVPGeneratingEventArgs e) => e.Skip = selectedTVPs.Contains(e.TVP) == false;

			void tvpGenerated(object sender, TVPGeneratedEventArgs e)
			{
				selectedTVPs.Remove(e.TVP);
				e.Stop =
					selectedTables.IsNullOrEmpty() &&
					selectedComplexTypeTables.IsNullOrEmpty() &&
					selectedViews.IsNullOrEmpty() &&
					selectedProcedures.IsNullOrEmpty() &&
					selectedFunctions.IsNullOrEmpty() &&
					selectedTVPs.IsNullOrEmpty();
			}

			void databaseGenerated(object sendeer, DatabaseGeneratedEventArgs e)
			{
				selectedDatabases.Remove(e.Database);
				e.Stop = selectedDatabases.IsNullOrEmpty();
			}

			_generator.DatabaseGenerating += databaseGenerating;
			_generator.TablesGenerating += tablesGenerating;
			_generator.TableGenerating += tableGenerating;
			_generator.TableGenerated += tableGenerated;
			_generator.ComplexTypeTablesGenerating += complexTypeTablesGenerating;
			_generator.ComplexTypeTableGenerating += complexTypeTableGenerating;
			_generator.ComplexTypeTableGenerated += complexTypeTableGenerated;
			_generator.ViewsGenerating += viewsGenerating;
			_generator.ViewGenerating += viewGenerating;
			_generator.ViewGenerated += viewGenerated;
			_generator.ProceduresGenerating += proceduresGenerating;
			_generator.ProcedureGenerating += procedureGenerating;
			_generator.ProcedureGenerated += procedureGenerated;
			_generator.FunctionsGenerating += functionsGenerating;
			_generator.FunctionGenerating += functionGenerating;
			_generator.FunctionGenerated += functionGenerated;
			_generator.TVPsGenerating += tvpsGenerating;
			_generator.TVPGenerating += tvpGenerating;
			_generator.TVPGenerated += tvpGenerated;
			_generator.DatabaseGenerated += databaseGenerated;
			SetGeneratorSettings();
			BeforeGeneratePOCOs?.Invoke(_generator);
			GeneratorResults results = _generator.GeneratePOCOs();
			AfterGeneratePOCOs?.Invoke(_generator);
			_generator.DatabaseGenerating -= databaseGenerating;
			_generator.TablesGenerating -= tablesGenerating;
			_generator.TableGenerating -= tableGenerating;
			_generator.TableGenerated -= tableGenerated;
			_generator.ComplexTypeTablesGenerating -= complexTypeTablesGenerating;
			_generator.ComplexTypeTableGenerating -= complexTypeTableGenerating;
			_generator.ComplexTypeTableGenerated -= complexTypeTableGenerated;
			_generator.ViewsGenerating -= viewsGenerating;
			_generator.ViewGenerating -= viewGenerating;
			_generator.ViewGenerated -= viewGenerated;
			_generator.ProceduresGenerating -= proceduresGenerating;
			_generator.ProcedureGenerating -= procedureGenerating;
			_generator.ProcedureGenerated -= procedureGenerated;
			_generator.FunctionsGenerating -= functionsGenerating;
			_generator.FunctionGenerating -= functionGenerating;
			_generator.FunctionGenerated -= functionGenerated;
			_generator.TVPsGenerating -= tvpsGenerating;
			_generator.TVPGenerating -= tvpGenerating;
			_generator.TVPGenerated -= tvpGenerated;
			_generator.DatabaseGenerated -= databaseGenerated;
			return results;
		}

		private TreeNode GetSelectedNode()
		{
			TreeNode selectedNode = trvServer.SelectedNode;
			if (selectedNode == null || selectedNode.Tag == null)
			{
				return null;
			}
			else if (selectedNode.Tag is Table)
			{
				return selectedNode;
			}
			else if (selectedNode.Tag is POCOGenerator.Objects.View)
			{
				return selectedNode;
			}
			else if (selectedNode.Tag is Procedure)
			{
				return selectedNode;
			}
			else if (selectedNode.Tag is Function)
			{
				return selectedNode;
			}
			else if (selectedNode.Tag is TVP)
			{
				return selectedNode;
			}
			else if (selectedNode.Tag is Server)
			{
				return null;
			}
			else if (selectedNode.Tag is Database)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<Table>)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<POCOGenerator.Objects.View>)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<Procedure>)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<Function>)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<TVP>)
			{
				return null;
			}
			else if (selectedNode.Tag is IEnumerable<TableColumn>)
			{
				return selectedNode.Parent;
			}
			else if (selectedNode.Tag is TableColumn)
			{
				return selectedNode.Parent.Parent;
			}
			else if (selectedNode.Tag is IEnumerable<PrimaryKey>)
			{
				return selectedNode.Parent;
			}
			else if (selectedNode.Tag is PrimaryKey)
			{
				return selectedNode.Parent.Parent;
			}
			else if (selectedNode.Tag is PrimaryKeyColumn)
			{
				return selectedNode.Parent.Parent.Parent;
			}
			else if (selectedNode.Tag is IEnumerable<UniqueKey>)
			{
				return selectedNode.Parent;
			}
			else if (selectedNode.Tag is UniqueKey)
			{
				return selectedNode.Parent.Parent;
			}
			else if (selectedNode.Tag is UniqueKeyColumn)
			{
				return selectedNode.Parent.Parent.Parent;
			}
			else if (selectedNode.Tag is IEnumerable<ForeignKey>)
			{
				return selectedNode.Parent;
			}
			else if (selectedNode.Tag is ForeignKey)
			{
				return selectedNode.Parent.Parent;
			}
			else if (selectedNode.Tag is ForeignKeyColumn)
			{
				return selectedNode.Parent.Parent.Parent;
			}
			else if (selectedNode.Tag is IEnumerable<TableIndex>)
			{
				return selectedNode.Parent;
			}
			else
			{
				return selectedNode.Tag is TableIndex
					? selectedNode.Parent.Parent
					: selectedNode.Tag is TableIndexColumn
									? selectedNode.Parent.Parent.Parent
									: selectedNode.Tag is IEnumerable<ViewColumn>
														   ? selectedNode.Parent
														   : selectedNode.Tag is ViewColumn
															   ? selectedNode.Parent.Parent
															   : selectedNode.Tag is IEnumerable<ViewIndex>
																   ? selectedNode.Parent
																   : selectedNode.Tag is ViewIndex
																	   ? selectedNode.Parent.Parent
																	   : selectedNode.Tag is ViewIndexColumn
																		   ? selectedNode.Parent.Parent.Parent
																		   : selectedNode.Tag is IEnumerable<ProcedureParameter>
																			   ? selectedNode.Parent
																			   : selectedNode.Tag is ProcedureParameter
																				   ? selectedNode.Parent.Parent
																				   : selectedNode.Tag is IEnumerable<ProcedureColumn>
																					   ? selectedNode.Parent
																					   : selectedNode.Tag is ProcedureColumn
																						   ? selectedNode.Parent.Parent
																						   : selectedNode.Tag is IEnumerable<FunctionParameter>
																							   ? selectedNode.Parent
																							   : selectedNode.Tag is FunctionParameter
																								   ? selectedNode.Parent.Parent
																								   : selectedNode.Tag is IEnumerable<FunctionColumn>
																									   ? selectedNode.Parent
																									   : selectedNode.Tag is FunctionColumn
																										   ? selectedNode.Parent.Parent
																										   : selectedNode.Tag is IEnumerable<TVPColumn>
																											   ? selectedNode.Parent
																											   : selectedNode.Tag is TVPColumn
																												   ? selectedNode.Parent.Parent
																												   : null;
			}
		}

		#endregion
	}
}

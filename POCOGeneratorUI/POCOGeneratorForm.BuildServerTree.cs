using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using POCOGenerator;
using POCOGenerator.Objects;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region Build Server Tree

		private enum ImageType
		{
			Server,
			Database,
			Folder,
			Table,
			View,
			Procedure,
			Function,
			TVP,
			Column,
			PrimaryKey,
			ForeignKey,
			UniqueKey,
			Index
		}

		private void BuildServerTree()
		{
			try
			{
				txtPocoEditor.Clear();
				DisableServerTree();
				filters.Clear();
				trvServer.Nodes.Clear();
				SetStatusMessage("Building...");
				Server server = null;

				void serverBuilt(object sender, ServerBuiltEventArgs e)
				{
					server = e.Server;
					e.Stop = true;
				}

				_generator.ServerBuilt += serverBuilt;
				GeneratorResults results = _generator.Generate();
				_generator.ServerBuilt -= serverBuilt;
				if (results != GeneratorResults.None)
				{
					HandleGeneratorError(results);
					return;
				}
				TreeNode serverNode = BuildServerNode(server);
				trvServer.Nodes.Add(serverNode);
				Application.DoEvents();
				foreach (Database database in server.Databases)
				{
					TreeNode databaseNode = BuildDatabaseNode(database);
					serverNode.Nodes.AddSorted(databaseNode);
					EnableServerTree();
					serverNode.Expand();
					DisableServerTree();
					Application.DoEvents();
					BuildDbObjectsNode(databaseNode, "Tables", database.Tables);
					BuildDbObjectsNode(databaseNode, "Views", database.Views);
					BuildDbObjectsNode(databaseNode, "Stored Procedures", database.Procedures);
					BuildDbObjectsNode(databaseNode, "Table-valued Functions", database.Functions);
					BuildDbObjectsNode(databaseNode, "User-Defined Table Types", database.TVPs);
					EnableServerTree();
					databaseNode.Expand();
					DisableServerTree();
					Application.DoEvents();
				}
				trvServer.SelectedNode = serverNode;
				EnableServerTree();
				Application.DoEvents();
				SetFormControls(_generator.Support.SupportSchema, _generator.Support.SupportTVPs,
								_generator.Support.SupportEnumDataType);
				ClearStatus();
				trvServer.Focus();
			}
			catch (Exception ex)
			{
				SetStatusErrorMessage("Error. " +
									  (ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : string.Empty))
									  .Replace(Environment.NewLine, " "));
			}
		}

		private void HandleGeneratorError(GeneratorResults results)
		{
			string errorMessage = "Error";
			if ((results & GeneratorResults.NoDbObjectsIncluded) == GeneratorResults.NoDbObjectsIncluded)
			{
				errorMessage = "No Database Objects Included";
			}
			else if ((results & GeneratorResults.ConnectionStringMissing) == GeneratorResults.ConnectionStringMissing)
			{
				errorMessage = "Connection String Missing";
			}
			else if ((results & GeneratorResults.ConnectionStringMalformed) == GeneratorResults.ConnectionStringMalformed)
			{
				errorMessage = "Connection String Malformed";
			}
			else if ((results & GeneratorResults.ConnectionStringNotMatchAnyRDBMS) ==
					 GeneratorResults.ConnectionStringNotMatchAnyRDBMS)
			{
				errorMessage = "Connection String Not Match Any RDBMS";
			}
			else if ((results & GeneratorResults.ConnectionStringMatchMoreThanOneRDBMS) ==
					 GeneratorResults.ConnectionStringMatchMoreThanOneRDBMS)
			{
				errorMessage = "Connection String Match More Than One RDBMS";
			}
			else if ((results & GeneratorResults.ServerNotResponding) == GeneratorResults.ServerNotResponding)
			{
				errorMessage = "Server Not Responding";
			}
			else if ((results & GeneratorResults.UnexpectedError) == GeneratorResults.UnexpectedError)
			{
				errorMessage = "Unexpected Error";
			}
			else if ((results & GeneratorResults.Error) == GeneratorResults.Error)
			{
				errorMessage = "Error";
			}
			else if ((results & GeneratorResults.Warning) == GeneratorResults.Warning)
			{
				errorMessage = "Warning";
			}
			errorMessage += ".";
			string statusErrorMessage = errorMessage;
			if (_generator != null && string.IsNullOrEmpty(_generator.Settings.Connection.ConnectionString) == false)
			{
				errorMessage += Environment.NewLine + Environment.NewLine + "CONNECTION STRING: " +
								_generator.Settings.Connection.ConnectionString;
			}
			if (_generator != null && _generator.Error != null)
			{
				errorMessage += Environment.NewLine + Environment.NewLine + _generator.Error.GetUnhandledExceptionErrorMessage();
				statusErrorMessage += " ERROR: " + _generator.Error.Message;
			}
			SetStatusErrorMessage(statusErrorMessage);
			bool isWarning = (results & GeneratorResults.Warning) == GeneratorResults.Warning;
			MessageBox.Show(this, errorMessage, isWarning ? "Warning" : "Error", MessageBoxButtons.OK,
							isWarning ? MessageBoxIcon.Warning : MessageBoxIcon.Error);
		}

		private TreeNode BuildServerNode(Server server)
		{
			return new TreeNode(server.ToStringWithVersion())
			{
				Tag = server,
				ImageIndex = (int)ImageType.Server,
				SelectedImageIndex = (int)ImageType.Server
			};
		}

		private TreeNode BuildDatabaseNode(Database database)
		{
			TreeNode databaseNode = new(database.ToString())
			{
				Tag = database,
				ImageIndex = (int)ImageType.Database,
				SelectedImageIndex = (int)ImageType.Database
			};
			if (database.Errors.Any())
			{
				databaseNode.ForeColor = Color.Red;
			}
			return databaseNode;
		}

		private void BuildDbObjectsNode(TreeNode databaseNode, string dbObjectsName,
										IEnumerable<POCOGenerator.Objects.IDbObject> dbObjects)
		{
			if (dbObjects.Any())
			{
				TreeNode node = new(dbObjectsName)
				{
					Tag = dbObjects,
					ImageIndex = (int)ImageType.Folder,
					SelectedImageIndex = (int)ImageType.Folder
				};
				databaseNode.Nodes.Add(node);
				node.ShowPlus();
			}
		}

		private void trvServer_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			if (e.Node.Nodes.Count > 0)
			{
				return;
			}
			else if (e.Node.Tag == null)
			{
				return;
			}
			else if (e.Node.Tag is Server)
			{
				return;
			}
			else if (e.Node.Tag is Database)
			{
				return;
			}
			else if (e.Node.Tag is IEnumerable<Table>)
			{
				BuildDbGroup<Table>(e.Node, ImageType.Table, isCheckedTables);
			}
			else if (e.Node.Tag is IEnumerable<POCOGenerator.Objects.View>)
			{
				BuildDbGroup<POCOGenerator.Objects.View>(e.Node, ImageType.View, isCheckedViews);
			}
			else if (e.Node.Tag is IEnumerable<Procedure>)
			{
				BuildDbGroup<Procedure>(e.Node, ImageType.Procedure, isCheckedProcedures);
			}
			else if (e.Node.Tag is IEnumerable<Function>)
			{
				BuildDbGroup<Function>(e.Node, ImageType.Function, isCheckedFunctions);
			}
			else if (e.Node.Tag is IEnumerable<TVP>)
			{
				BuildDbGroup<TVP>(e.Node, ImageType.TVP, isCheckedTVPs);
			}
			else if (e.Node.Tag is Table)
			{
				Table table = e.Node.Tag as Table;
				BuildColumnsNode(e.Node, table.TableColumns);
				BuildPrimaryKeyNode(e.Node, table.PrimaryKey);
				BuildUniqueKeysNode(e.Node, table.UniqueKeys);
				BuildForeignKeysNode(e.Node, table.ForeignKeys);
				BuildIndexesNode(e.Node, table.Indexes);
			}
			else if (e.Node.Tag is POCOGenerator.Objects.View)
			{
				POCOGenerator.Objects.View view = e.Node.Tag as POCOGenerator.Objects.View;
				BuildColumnsNode(e.Node, view.ViewColumns);
				BuildIndexesNode(e.Node, view.Indexes);
			}
			else if (e.Node.Tag is Procedure)
			{
				Procedure procedure = e.Node.Tag as Procedure;
				BuildParametersNode(e.Node, procedure.ProcedureParameters);
				BuildColumnsNode(e.Node, procedure.ProcedureColumns);
			}
			else if (e.Node.Tag is Function)
			{
				Function function = e.Node.Tag as Function;
				BuildParametersNode(e.Node, function.FunctionParameters);
				BuildColumnsNode(e.Node, function.FunctionColumns);
			}
			else if (e.Node.Tag is TVP)
			{
				TVP tvp = e.Node.Tag as TVP;
				BuildColumnsNode(e.Node, tvp.TVPColumns);
			}
			else if (e.Node.Tag is IEnumerable<TableColumn>)
			{
				BuildTableColumns(e);
			}
			else if (e.Node.Tag is IEnumerable<PrimaryKey>)
			{
				BuildTablePrimaryKey(e.Node);
			}
			else if (e.Node.Tag is PrimaryKey)
			{
				BuildTablePrimaryKeyColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<UniqueKey>)
			{
				BuildTableUniqueKeys(e.Node);
			}
			else if (e.Node.Tag is UniqueKey)
			{
				BuildTableUniqueKeyColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<ForeignKey>)
			{
				BuildTableForeignKeys(e.Node);
			}
			else if (e.Node.Tag is ForeignKey)
			{
				BuildTableForeignKeyColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<TableIndex>)
			{
				BuildTableIndexes(e.Node);
			}
			else if (e.Node.Tag is TableIndex)
			{
				BuildTableIndexColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<ViewColumn>)
			{
				BuildViewColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<ViewIndex>)
			{
				BuildViewIndexes(e.Node);
			}
			else if (e.Node.Tag is ViewIndex)
			{
				BuildViewIndexColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<ProcedureParameter>)
			{
				BuildProcedureParameters(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<ProcedureColumn>)
			{
				BuildProcedureColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<FunctionParameter>)
			{
				BuildFunctionParameters(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<FunctionColumn>)
			{
				BuildFunctionColumns(e.Node);
			}
			else if (e.Node.Tag is IEnumerable<TVPColumn>)
			{
				BuildTVPColumns(e.Node);
			}
		}

		private void BuildDbGroup<T>(TreeNode node, ImageType imageType, bool isChecked)
			where T : POCOGenerator.Objects.IDbObject
		{
			string text = node.Text;
			node.Text += " (expanding...)";
			Application.DoEvents();
			IEnumerable<T> items = node.Tag as IEnumerable<T>;
			foreach (T item in items)
			{
				TreeNode itemNode = new(item.ToString())
				{
					Tag = item,
					ImageIndex = (int)imageType,
					SelectedImageIndex = (int)imageType
				};
				if (string.IsNullOrEmpty(item.Error) == false)
				{
					itemNode.ForeColor = Color.Red;
				}
				node.Nodes.AddSorted(itemNode);
				itemNode.ShowPlus();
				if (isChecked)
				{
					DisableServerTreeAfterCheck();
					itemNode.Checked = true;
					EnableServerTreeAfterCheck();
				}
			}
			node.Text = text;
			Application.DoEvents();
		}

		private void BuildColumnsNode(TreeNode node, IEnumerable<IDbColumn> columns)
		{
			TreeNode columnsNode = new("Columns")
			{
				Tag = columns,
				ImageIndex = (int)ImageType.Folder,
				SelectedImageIndex = (int)ImageType.Folder
			};
			node.Nodes.Add(columnsNode);
			if (columns.Any())
			{
				columnsNode.ShowPlus();
			}
		}

		private void BuildParametersNode(TreeNode node, IEnumerable<IDbParameter> parameters)
		{
			TreeNode parametersNode = new("Parameters")
			{
				Tag = parameters,
				ImageIndex = (int)ImageType.Folder,
				SelectedImageIndex = (int)ImageType.Folder
			};
			node.Nodes.Add(parametersNode);
			if (parameters.Any())
			{
				parametersNode.ShowPlus();
			}
		}

		private void BuildPrimaryKeyNode(TreeNode node, PrimaryKey primaryKey)
		{
			if (primaryKey != null)
			{
				TreeNode primaryKeyNode = new("Primary Key")
				{
					Tag = new PrimaryKey[] { primaryKey },
					ImageIndex = (int)ImageType.Folder,
					SelectedImageIndex = (int)ImageType.Folder
				};
				node.Nodes.Add(primaryKeyNode);
				primaryKeyNode.ShowPlus();
			}
		}

		private void BuildUniqueKeysNode(TreeNode node, IEnumerable<UniqueKey> uniqueKeys)
		{
			if (uniqueKeys.Any())
			{
				TreeNode uniqueKeysNode = new("Unique Keys")
				{
					Tag = uniqueKeys,
					ImageIndex = (int)ImageType.Folder,
					SelectedImageIndex = (int)ImageType.Folder
				};
				node.Nodes.Add(uniqueKeysNode);
				uniqueKeysNode.ShowPlus();
			}
		}

		private void BuildForeignKeysNode(TreeNode node, IEnumerable<ForeignKey> foreignKeys)
		{
			if (foreignKeys.Any())
			{
				TreeNode foreignKeysNode = new("Foreign Keys")
				{
					Tag = foreignKeys,
					ImageIndex = (int)ImageType.Folder,
					SelectedImageIndex = (int)ImageType.Folder
				};
				node.Nodes.Add(foreignKeysNode);
				foreignKeysNode.ShowPlus();
			}
		}

		private void BuildIndexesNode(TreeNode node, IEnumerable<POCOGenerator.Objects.Index> indexes)
		{
			if (indexes.Any())
			{
				TreeNode indexesNode = new("Indexes")
				{
					Tag = indexes,
					ImageIndex = (int)ImageType.Folder,
					SelectedImageIndex = (int)ImageType.Folder
				};
				node.Nodes.Add(indexesNode);
				indexesNode.ShowPlus();
			}
		}

		private void BuildTableColumns(TreeViewCancelEventArgs e)
		{
			IEnumerable<TableColumn> tableColumns = e.Node.Tag as IEnumerable<TableColumn>;
			foreach (TableColumn column in tableColumns)
			{
				e.Node.Nodes.Add(BuildTableColumnNode(column));
			}
		}

		private TreeNode BuildTableColumnNode(TableColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)(column.PrimaryKeyColumn != null
											  ? ImageType.PrimaryKey
											  : (column.ForeignKeyColumns.Any() ? ImageType.ForeignKey : ImageType.Column)),
				SelectedImageIndex = (int)(column.PrimaryKeyColumn != null
													  ? ImageType.PrimaryKey
													  : (column.ForeignKeyColumns.Any() ? ImageType.ForeignKey : ImageType.Column))
			};
		}

		private void BuildTablePrimaryKey(TreeNode node)
		{
			PrimaryKey primaryKey = (node.Tag as IEnumerable<PrimaryKey>).First();
			TreeNode primaryKeyNode = BuildTablePrimaryKeyNode(primaryKey);
			node.Nodes.Add(primaryKeyNode);
			primaryKeyNode.ShowPlus();
		}

		private TreeNode BuildTablePrimaryKeyNode(PrimaryKey primaryKey)
		{
			return new TreeNode(primaryKey.ToString())
			{
				Tag = primaryKey,
				ImageIndex = (int)ImageType.PrimaryKey,
				SelectedImageIndex = (int)ImageType.PrimaryKey
			};
		}

		private void BuildTablePrimaryKeyColumns(TreeNode node)
		{
			PrimaryKey primaryKey = node.Tag as PrimaryKey;
			foreach (PrimaryKeyColumn column in primaryKey.PrimaryKeyColumns)
			{
				node.Nodes.Add(BuildTablePrimaryKeyColumnNode(column));
			}
		}

		private TreeNode BuildTablePrimaryKeyColumnNode(PrimaryKeyColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.PrimaryKey,
				SelectedImageIndex = (int)ImageType.PrimaryKey
			};
		}

		private void BuildTableUniqueKeys(TreeNode node)
		{
			IEnumerable<UniqueKey> uniqueKeys = node.Tag as IEnumerable<UniqueKey>;
			foreach (UniqueKey uniqueKey in uniqueKeys)
			{
				TreeNode uniqueKeyNode = BuildTableUniqueKeyNode(uniqueKey);
				node.Nodes.Add(uniqueKeyNode);
				uniqueKeyNode.ShowPlus();
			}
		}

		private TreeNode BuildTableUniqueKeyNode(UniqueKey uniqueKey)
		{
			return new TreeNode(uniqueKey.ToString())
			{
				Tag = uniqueKey,
				ImageIndex = (int)ImageType.UniqueKey,
				SelectedImageIndex = (int)ImageType.UniqueKey
			};
		}

		private void BuildTableUniqueKeyColumns(TreeNode node)
		{
			UniqueKey uniqueKey = node.Tag as UniqueKey;
			foreach (UniqueKeyColumn column in uniqueKey.UniqueKeyColumns)
			{
				node.Nodes.Add(BuildTableUniqueKeyColumnNode(column));
			}
		}

		private TreeNode BuildTableUniqueKeyColumnNode(UniqueKeyColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)(column.TableColumn.PrimaryKeyColumn != null
											  ? ImageType.PrimaryKey
											  : (column.TableColumn.ForeignKeyColumns.Any()
													 ? ImageType.ForeignKey
													 : ImageType.Column)),
				SelectedImageIndex = (int)(column.TableColumn.PrimaryKeyColumn != null
													  ? ImageType.PrimaryKey
													  : (column.TableColumn.ForeignKeyColumns.Any()
															 ? ImageType.ForeignKey
															 : ImageType.Column))
			};
		}

		private void BuildTableForeignKeys(TreeNode node)
		{
			IEnumerable<ForeignKey> foreignKeys = node.Tag as IEnumerable<ForeignKey>;
			foreach (ForeignKey foreignKey in foreignKeys)
			{
				TreeNode foreignKeyNode = BuildTableForeignKeyNode(foreignKey);
				node.Nodes.Add(foreignKeyNode);
				foreignKeyNode.ShowPlus();
			}
		}

		private TreeNode BuildTableForeignKeyNode(ForeignKey foreignKey)
		{
			return new TreeNode(foreignKey.ToString())
			{
				Tag = foreignKey,
				ImageIndex = (int)ImageType.ForeignKey,
				SelectedImageIndex = (int)ImageType.ForeignKey
			};
		}

		private void BuildTableForeignKeyColumns(TreeNode node)
		{
			ForeignKey foreignKey = node.Tag as ForeignKey;
			foreach (ForeignKeyColumn column in foreignKey.ForeignKeyColumns)
			{
				node.Nodes.Add(BuildTableForeignKeyColumnNode(column, foreignKey));
			}
		}

		private TreeNode BuildTableForeignKeyColumnNode(ForeignKeyColumn column, ForeignKey foreignKey)
		{
			return new TreeNode(string.Format("{0} {1} {2}.{3}", column.ToFullString(), char.ConvertFromUtf32(8594),
											  foreignKey.PrimaryTable, column.PrimaryTableColumn))
			{
				Tag = column,
				ImageIndex = (int)ImageType.ForeignKey,
				SelectedImageIndex = (int)ImageType.ForeignKey
			};
		}

		private void BuildTableIndexes(TreeNode node)
		{
			IEnumerable<TableIndex> tableIndexes = node.Tag as IEnumerable<TableIndex>;
			foreach (TableIndex tableIndex in tableIndexes)
			{
				TreeNode tableIndexNode = BuildTableIndexNode(tableIndex);
				node.Nodes.Add(tableIndexNode);
				tableIndexNode.ShowPlus();
			}
		}

		private TreeNode BuildTableIndexNode(TableIndex tableIndex)
		{
			return new TreeNode(tableIndex.ToString())
			{
				Tag = tableIndex,
				ImageIndex = (int)ImageType.Index,
				SelectedImageIndex = (int)ImageType.Index
			};
		}

		private void BuildTableIndexColumns(TreeNode node)
		{
			TableIndex tableIndex = node.Tag as TableIndex;
			foreach (TableIndexColumn column in tableIndex.IndexColumns)
			{
				node.Nodes.Add(BuildTableIndexColumnNode(column));
			}
		}

		private TreeNode BuildTableIndexColumnNode(TableIndexColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)(column.TableColumn.PrimaryKeyColumn != null
											  ? ImageType.PrimaryKey
											  : (column.TableColumn.ForeignKeyColumns.Any()
													 ? ImageType.ForeignKey
													 : ImageType.Column)),
				SelectedImageIndex = (int)(column.TableColumn.PrimaryKeyColumn != null
													  ? ImageType.PrimaryKey
													  : (column.TableColumn.ForeignKeyColumns.Any()
															 ? ImageType.ForeignKey
															 : ImageType.Column))
			};
		}

		private void BuildViewColumns(TreeNode node)
		{
			IEnumerable<ViewColumn> viewColumns = node.Tag as IEnumerable<ViewColumn>;
			foreach (ViewColumn column in viewColumns)
			{
				node.Nodes.Add(BuildViewColumnNode(column));
			}
		}

		private TreeNode BuildViewColumnNode(ViewColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildViewIndexes(TreeNode node)
		{
			IEnumerable<ViewIndex> viewIndexes = node.Tag as IEnumerable<ViewIndex>;
			foreach (ViewIndex viewIndex in viewIndexes)
			{
				TreeNode viewIndexNode = BuildViewIndexNode(viewIndex);
				node.Nodes.Add(viewIndexNode);
				viewIndexNode.ShowPlus();
			}
		}

		private TreeNode BuildViewIndexNode(ViewIndex viewIndex)
		{
			return new TreeNode(viewIndex.ToString())
			{
				Tag = viewIndex,
				ImageIndex = (int)ImageType.Index,
				SelectedImageIndex = (int)ImageType.Index
			};
		}

		private void BuildViewIndexColumns(TreeNode node)
		{
			ViewIndex viewIndex = node.Tag as ViewIndex;
			foreach (ViewIndexColumn column in viewIndex.IndexColumns)
			{
				node.Nodes.Add(BuildViewIndexColumnNode(column));
			}
		}

		private TreeNode BuildViewIndexColumnNode(ViewIndexColumn column)
		{
			return new TreeNode(column.ToFullString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildProcedureParameters(TreeNode node)
		{
			IEnumerable<ProcedureParameter> procedureParameters = node.Tag as IEnumerable<ProcedureParameter>;
			foreach (ProcedureParameter parameter in procedureParameters)
			{
				node.Nodes.Add(BuildProcedureParameterNode(parameter));
			}
		}

		private TreeNode BuildProcedureParameterNode(ProcedureParameter parameter)
		{
			return new TreeNode(parameter.ToString())
			{
				Tag = parameter,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildProcedureColumns(TreeNode node)
		{
			IEnumerable<ProcedureColumn> procedureColumns = node.Tag as IEnumerable<ProcedureColumn>;
			foreach (ProcedureColumn column in procedureColumns)
			{
				node.Nodes.Add(BuildProcedureColumnNode(column));
			}
		}

		private TreeNode BuildProcedureColumnNode(ProcedureColumn column)
		{
			return new TreeNode(column.ToString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildFunctionParameters(TreeNode node)
		{
			IEnumerable<FunctionParameter> functionParameters = node.Tag as IEnumerable<FunctionParameter>;
			foreach (FunctionParameter parameter in functionParameters)
			{
				node.Nodes.Add(BuildFunctionParameterNode(parameter));
			}
		}

		private TreeNode BuildFunctionParameterNode(FunctionParameter parameter)
		{
			return new TreeNode(parameter.ToString())
			{
				Tag = parameter,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildFunctionColumns(TreeNode node)
		{
			IEnumerable<FunctionColumn> functionColumns = node.Tag as IEnumerable<FunctionColumn>;
			foreach (FunctionColumn column in functionColumns)
			{
				node.Nodes.Add(BuildFunctionColumnNode(column));
			}
		}

		private TreeNode BuildFunctionColumnNode(FunctionColumn column)
		{
			return new TreeNode(column.ToString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		private void BuildTVPColumns(TreeNode node)
		{
			IEnumerable<TVPColumn> tvpColumns = node.Tag as IEnumerable<TVPColumn>;
			foreach (TVPColumn column in tvpColumns)
			{
				node.Nodes.Add(BuildTVPColumnNode(column));
			}
		}

		private TreeNode BuildTVPColumnNode(TVPColumn column)
		{
			return new TreeNode(column.ToString())
			{
				Tag = column,
				ImageIndex = (int)ImageType.Column,
				SelectedImageIndex = (int)ImageType.Column
			};
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using POCOGenerator;
using POCOGenerator.Forms;
using POCOGenerator.Objects;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		private void btnFolder_Click(object sender, EventArgs e)
		{
			if (Directory.Exists(txtFolder.Text))
			{
				folderBrowserDialogExport.SelectedPath = txtFolder.Text;
			}
			if (folderBrowserDialogExport.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
			{
				txtFolder.Text = folderBrowserDialogExport.SelectedPath;
			}
		}

		private void btnExport_Click(object sender, EventArgs e)
		{
			if (_generator == null)
			{
				return;
			}
			ClearStatus();
			if (String.IsNullOrEmpty(txtFolder.Text))
			{
				SetStatusErrorMessage("Folder is empty.");
				return;
			}
			int selectedDbObjectsCount =
				selectedTables.Count +
				selectedViews.Count +
				selectedProcedures.Count +
				selectedFunctions.Count +
				selectedTVPs.Count;
			POCOGenerator.Objects.IDbObject dbObject = null;
			TreeNode selectedNode = GetSelectedNode();
			if (selectedNode != null)
			{
				if (selectedNode.Tag is Table table)
				{
					dbObject = table;
				}
				else if (selectedNode.Tag is POCOGenerator.Objects.View view)
				{
					dbObject = view;
				}
				else if (selectedNode.Tag is Procedure procedure)
				{
					dbObject = procedure;
				}
				else if (selectedNode.Tag is Function function)
				{
					dbObject = function;
				}
				else if (selectedNode.Tag is TVP tvp)
				{
					dbObject = tvp;
				}
			}
			if (selectedDbObjectsCount == 0 && dbObject == null)
			{
				return;
			}
			SetStatusMessage("Exporting...");
			try
			{
				if (rdbSingleFile.Checked)
				{
					string fileName = Export_SingleFile_GetFileName(selectedDbObjectsCount, dbObject);
					Export_SingleFile(fileName);
				}
				else if (rdbMultipleFilesSingleFolder.Checked)
				{
					Export_MultipleFiles_SingleFolder();
				}
				else if (rdbMultipleFilesRelativeFolders.Checked)
				{
					Export_MultipleFiles_RelativeFolders();
				}
			}
			catch (Exception ex)
			{
				_generator.RedirectTo(txtPocoEditor);
				SetStatusErrorMessage("Exporting failed. " + ex.Message);
			}
		}

		private void Export_SingleFile(string fileName)
		{
			string path = Path.GetFullPath(Path.Combine(txtFolder.Text, fileName));
			string folder = Path.GetDirectoryName(path);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			Exception exportError = null;
			try
			{
				using (FileStream stream = File.Open(path, FileMode.Create))
				{
					ExportPOCOs(g => g.RedirectTo(stream),
								g => g.RedirectTo(txtPocoEditor));
				}
			}
			catch (Exception ex)
			{
				_generator.RedirectTo(txtPocoEditor);
				exportError = ex;
			}
			if (exportError != null)
			{
				SetStatusErrorMessage("Exporting failed. " + path);
				string errorMessage =
					"Failed to export to file." +
					Environment.NewLine +
					String.Format("{0}: {1}", fileName, exportError.Message);
				MessageBox.Show(this, errorMessage, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				SetStatusMessage("Exporting done. " + path);
			}
		}

		private string Export_SingleFile_GetFileName(int selectedDbObjectsCount,
													 POCOGenerator.Objects.IDbObject dbObject)
		{
			string dbObjectName = null;
			if (selectedDbObjectsCount == 0 && dbObject != null)
			{
				dbObjectName = dbObject.ToString();
			}
			else if (selectedDbObjectsCount == 1)
			{
				if (selectedTables.HasSingle())
				{
					dbObjectName = selectedTables[0].ToString();
				}
				else if (selectedViews.HasSingle())
				{
					dbObjectName = selectedViews[0].ToString();
				}
				else if (selectedProcedures.HasSingle())
				{
					dbObjectName = selectedProcedures[0].ToString();
				}
				else if (selectedFunctions.HasSingle())
				{
					dbObjectName = selectedFunctions[0].ToString();
				}
				else if (selectedTVPs.HasSingle())
				{
					dbObjectName = selectedTVPs[0].ToString();
				}
			}
			else if (selectedDbObjectsCount > 1)
			{
				List<Database> databases =
					selectedTables.Select(x => x.Database).Union(
																 selectedViews.Select(x => x.Database)).Union(
									   selectedProcedures.Select(x => x.Database)).Union(
									   selectedFunctions.Select(x => x.Database))
								  .Union(
										 selectedTVPs.Select(x => x.Database))
								  .Distinct()
								  .ToList();
				if (databases.HasSingle())
				{
					dbObjectName = databases[0].ToString();
				}
			}
			if (String.IsNullOrEmpty(dbObjectName))
			{
				TreeNode serverNode = trvServer.Nodes[0];
				Server server = serverNode.Tag as Server;
				dbObjectName = server.ToString();
			}
			return String.Join("_", dbObjectName.Split(Path.GetInvalidFileNameChars())) + ".cs";
		}

		private void Export_MultipleFiles_SingleFolder()
		{
			string folder = Path.GetFullPath(txtFolder.Text);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			int filesCount = 0;
			List<Tuple<string, Exception>> exportErrors = [];

			void tablePOCO(object sender1, TablePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.Table.Schema, e1.Table.Database.Name,
																	  e1.POCO, folder, exportErrors))
				{
					filesCount++;
				}
			}

			void complexTypeTablePOCO(object sender1, ComplexTypeTablePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.ComplexTypeTable.Schema,
																	  e1.ComplexTypeTable.Database.Name, e1.POCO, folder,
																	  exportErrors))
				{
					filesCount++;
				}
			}

			void viewPOCO(object sender1, ViewPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.View.Schema, e1.View.Database.Name, e1.POCO,
																	  folder, exportErrors))
				{
					filesCount++;
				}
			}

			void procedurePOCO(object sender1, ProcedurePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.Procedure.Schema, e1.Procedure.Database.Name,
																	  e1.POCO, folder, exportErrors))
				{
					filesCount++;
				}
			}

			void functionPOCO(object sender1, FunctionPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.Function.Schema, e1.Function.Database.Name,
																	  e1.POCO, folder, exportErrors))
				{
					filesCount++;
				}
			}

			void tvpPOCO(object sender1, TVPPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_SingleFolder_WritePOCOToFile(e1.ClassName, e1.TVP.Schema, e1.TVP.Database.Name, e1.POCO,
																	  folder, exportErrors))
				{
					filesCount++;
				}
			}

			ExportPOCOs(
						g => {
							g.Settings.POCO.WrapAroundEachClass = true;
							g.RedirectToOutputEmpty();
							g.TablePOCO += tablePOCO;
							g.ComplexTypeTablePOCO += complexTypeTablePOCO;
							g.ViewPOCO += viewPOCO;
							g.ProcedurePOCO += procedurePOCO;
							g.FunctionPOCO += functionPOCO;
							g.TVPPOCO += tvpPOCO;
						},
						g => {
							g.TablePOCO -= tablePOCO;
							g.ComplexTypeTablePOCO -= complexTypeTablePOCO;
							g.ViewPOCO -= viewPOCO;
							g.ProcedurePOCO -= procedurePOCO;
							g.FunctionPOCO -= functionPOCO;
							g.TVPPOCO -= tvpPOCO;
							g.RedirectTo(txtPocoEditor);
						}
					   );
			Export_MultipleFiles_SetExportMessage(exportErrors, folder, filesCount);
		}

		private bool Export_MultipleFiles_SingleFolder_WritePOCOToFile(string className, string schema, string database,
																	   string poco, string folder,
																	   List<Tuple<string, Exception>> exportErrors)
		{
			try
			{
				string fileName = Export_MultipleFiles_GetFileName(className, schema, database);
				string path = Path.Combine(folder, fileName);
				File.WriteAllText(path, poco);
				return true;
			}
			catch (Exception ex)
			{
				exportErrors.Add(new(className, ex));
				return false;
			}
		}

		private void Export_MultipleFiles_RelativeFolders()
		{
			string folder = Path.GetFullPath(txtFolder.Text);
			if (!Directory.Exists(folder))
			{
				Directory.CreateDirectory(folder);
			}
			string path = folder;
			int filesCount = 0;
			List<Tuple<string, Exception>> exportErrors = [];
			ExportPOCOs(g => {
				g.Settings.POCO.WrapAroundEachClass = true;
				g.RedirectToOutputEmpty();
				g.ServerGenerating += serverGenerating;
				g.DatabaseGenerating += databaseGenerating;
				g.TablesGenerating += tablesGenerating;
				g.ComplexTypeTablesGenerating += complexTypeTablesGenerating;
				g.ViewsGenerating += viewsGenerating;
				g.ProceduresGenerating += proceduresGenerating;
				g.FunctionsGenerating += functionsGenerating;
				g.TVPsGenerating += tvpsGenerating;
				g.TableGenerating += tableGenerating;
				g.ComplexTypeTableGenerating += complexTypeTableGenerating;
				g.ViewGenerating += viewGenerating;
				g.ProcedureGenerating += procedureGenerating;
				g.FunctionGenerating += functionGenerating;
				g.TVPGenerating += tvpGenerating;
				g.TablePOCO += tablePOCO;
				g.ComplexTypeTablePOCO += complexTypeTablePOCO;
				g.ViewPOCO += viewPOCO;
				g.ProcedurePOCO += procedurePOCO;
				g.FunctionPOCO += functionPOCO;
				g.TVPPOCO += tvpPOCO;
				g.TablesGenerated += tablesGenerated;
				g.ComplexTypeTablesGenerated += complexTypeTablesGenerated;
				g.ViewsGenerated += viewsGenerated;
				g.ProceduresGenerated += proceduresGenerated;
				g.FunctionsGenerated += functionsGenerated;
				g.TVPsGenerated += tvpsGenerated;
				g.DatabaseGenerated += databaseGenerated;
				g.ServerGenerated += serverGenerated;
			},
						g => {
							g.ServerGenerating -= serverGenerating;
							g.DatabaseGenerating -= databaseGenerating;
							g.TablesGenerating -= tablesGenerating;
							g.ComplexTypeTablesGenerating -= complexTypeTablesGenerating;
							g.ViewsGenerating -= viewsGenerating;
							g.ProceduresGenerating -= proceduresGenerating;
							g.FunctionsGenerating -= functionsGenerating;
							g.TVPsGenerating -= tvpsGenerating;
							g.TableGenerating -= tableGenerating;
							g.ComplexTypeTableGenerating -= complexTypeTableGenerating;
							g.ViewGenerating -= viewGenerating;
							g.ProcedureGenerating -= procedureGenerating;
							g.FunctionGenerating -= functionGenerating;
							g.TVPGenerating -= tvpGenerating;
							g.TablePOCO -= tablePOCO;
							g.ComplexTypeTablePOCO -= complexTypeTablePOCO;
							g.ViewPOCO -= viewPOCO;
							g.ProcedurePOCO -= procedurePOCO;
							g.FunctionPOCO -= functionPOCO;
							g.TVPPOCO -= tvpPOCO;
							g.TablesGenerated -= tablesGenerated;
							g.ComplexTypeTablesGenerated -= complexTypeTablesGenerated;
							g.ViewsGenerated -= viewsGenerated;
							g.ProceduresGenerated -= proceduresGenerated;
							g.FunctionsGenerated -= functionsGenerated;
							g.TVPsGenerated -= tvpsGenerated;
							g.DatabaseGenerated -= databaseGenerated;
							g.ServerGenerated -= serverGenerated;
							g.RedirectTo(txtPocoEditor);
						}
					   );
			Export_MultipleFiles_SetExportMessage(exportErrors, folder, filesCount);
			return;

			void serverGenerating(object sender1, ServerGeneratingEventArgs e1)
			{
				if (e1.Stop)
				{
					return;
				}
				path = Path.Combine(path,
									String.Join("_", e1.Server.ToString().Split(Path.GetInvalidFileNameChars())));
				if (!String.IsNullOrEmpty(_generator.Settings.POCO.Namespace))
				{
					path = Path.Combine(path,
										String.Join("_", _generator.Settings.POCO.Namespace.Split(Path.GetInvalidFileNameChars())));
				}
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void databaseGenerating(object sender1, DatabaseGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path,
									String.Join("_", e1.Database.ToString().Split(Path.GetInvalidFileNameChars())));
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void tablesGenerating(object sender1, TablesGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "Tables");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void complexTypeTablesGenerating(object sender1, ComplexTypeTablesGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "Tables");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void viewsGenerating(object sender1, ViewsGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "Views");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void proceduresGenerating(object sender1, ProceduresGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "Procedures");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void functionsGenerating(object sender1, FunctionsGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "Functions");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void tvpsGenerating(object sender1, TVPsGeneratingEventArgs e1)
			{
				if (e1.Stop || e1.Skip)
				{
					return;
				}
				path = Path.Combine(path, "TVPs");
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
			}

			void tableGenerating(object sender1, TableGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.Table.Database,
																				 "Tables",
																				 e1.Table.Schema);
			}

			void complexTypeTableGenerating(object sender1, ComplexTypeTableGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.ComplexTypeTable.Database,
																				 "Tables",
																				 e1.ComplexTypeTable.Schema);
			}

			void viewGenerating(object sender1, ViewGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.View.Database,
																				 "Views",
																				 e1.View.Schema);
			}

			void procedureGenerating(object sender1, ProcedureGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.Procedure.Database,
																				 "Procedures",
																				 e1.Procedure.Schema);
			}

			void functionGenerating(object sender1, FunctionGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.Function.Database,
																				 "Functions",
																				 e1.Function.Schema);
			}

			void tvpGenerating(object sender1, TVPGeneratingEventArgs e1)
			{
				e1.Namespace = Export_MultipleFiles_RelativeFolders_GetNamespace(e1.Namespace,
																				 e1.TVP.Database,
																				 "TVPs",
																				 e1.TVP.Schema);
			}

			void tablePOCO(object sender1, TablePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.Table.Schema,
																		 e1.Table.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void complexTypeTablePOCO(object sender1, ComplexTypeTablePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.ComplexTypeTable.Schema,
																		 e1.ComplexTypeTable.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void viewPOCO(object sender1, ViewPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.View.Schema,
																		 e1.View.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void procedurePOCO(object sender1, ProcedurePOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.Procedure.Schema,
																		 e1.Procedure.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void functionPOCO(object sender1, FunctionPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.Function.Schema,
																		 e1.Function.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void tvpPOCO(object sender1, TVPPOCOEventArgs e1)
			{
				if (Export_MultipleFiles_RelativeFolders_WritePOCOToFile(e1.ClassName,
																		 e1.TVP.Schema,
																		 e1.TVP.Database.Name,
																		 e1.POCO,
																		 path,
																		 exportErrors))
				{
					filesCount++;
				}
			}

			void tablesGenerated(object sender1, TablesGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void complexTypeTablesGenerated(object sender1, ComplexTypeTablesGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void viewsGenerated(object sender1, ViewsGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void proceduresGenerated(object sender1, ProceduresGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void functionsGenerated(object sender1, FunctionsGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void tvpsGenerated(object sender1, TVPsGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void databaseGenerated(object sender1, DatabaseGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}

			void serverGenerated(object sender1, ServerGeneratedEventArgs e1)
			{
				path = Path.GetDirectoryName(path);
			}
		}

		private string Export_MultipleFiles_RelativeFolders_GetNamespace(string @namespace, Database database, string dbGroup,
																		 string schema)
		{
			@namespace = String.IsNullOrEmpty(@namespace)
							 ? $"{database}.{dbGroup}"
							 : $"{@namespace}.{database}.{dbGroup}";
			if (!String.IsNullOrEmpty(schema))
			{
				@namespace = $"{@namespace}.{schema}";
			}
			return @namespace;
		}

		private bool Export_MultipleFiles_RelativeFolders_WritePOCOToFile(string className,
																		  string schema,
																		  string database,
																		  string poco,
																		  string path,
																		  List<Tuple<string, Exception>> exportErrors)
		{
			try
			{
				schema = String.Join("_", (schema ?? String.Empty).Split(Path.GetInvalidFileNameChars()));
				path = Path.Combine(path, schema);
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				string fileName = Export_MultipleFiles_GetFileName(className, schema, database);
				path = Path.Combine(path, fileName);
				File.WriteAllText(path, poco);
				return true;
			}
			catch (Exception ex)
			{
				exportErrors.Add(new(className, ex));
				return false;
			}
		}

		private string Export_MultipleFiles_GetFileName(string className, string schema, string database)
		{
			string fileName = null;
			if (rdbFileNameName.Checked)
			{
				fileName = className;
			}
			else if (rdbFileNameSchemaName.Checked)
			{
				fileName = schema + "." + className;
			}
			else if (rdbFileNameDatabaseName.Checked)
			{
				fileName = database + "." + className;
			}
			else if (rdbFileNameDatabaseSchemaName.Checked)
			{
				fileName = database + "." + schema + "." + className;
			}
			fileName = String.Join("_", fileName.Split(Path.GetInvalidFileNameChars())) + ".cs";
			return fileName;
		}

		private void Export_MultipleFiles_SetExportMessage(List<Tuple<string, Exception>> exportErrors, string path, int filesCount)
		{
			if (exportErrors.HasAny())
			{
				SetStatusErrorMessage($"Exporting failed. {(filesCount > 0
																? filesCount.ToString("N0") + " file" + (filesCount > 1 ? "s" : String.Empty) + ". "
																: String.Empty)}{path}"
									 );
				string errorMessage =
					String.Format("Failed to export {0:N0} file{1}.", exportErrors.Count,
								  exportErrors.HasAny() ? "s" : String.Empty) +
					Environment.NewLine +
					String.Join(Environment.NewLine,
								exportErrors.Take(5).Select(x => $"{x.Item1}: {x.Item2.Message}")) +
					(exportErrors.Count > 5 ? Environment.NewLine + "and more." : String.Empty);
				MessageBox.Show(this, errorMessage, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				SetStatusMessage(
								 "Exporting done. " +
								 (filesCount > 0
									  ? filesCount.ToString("N0") + " file" + (filesCount > 1 ? "s" : String.Empty) + ". "
									  : String.Empty) +
								 path
								);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using POCOGenerator;
using POCOGenerator.Objects;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
		: Form
	{
		#region Form

		public POCOGeneratorForm()
		{
			InitializeComponent();
			GetControlsOriginalLocation();
			Text += " " + Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);
		}

		private bool hasUISettings;

		private void POCOGeneratorForm_Load(object sender, EventArgs e) => hasUISettings = LoadUISettings();

		private void POCOGeneratorForm_Shown(object sender, EventArgs e)
		{
			if (hasUISettings == false)
			{
				ShowDisclaimer();
			}
		}

		private void POCOGeneratorForm_FormClosing(object sender, FormClosingEventArgs e) => SerializeUISettings();

		private void btnClose_Click(object sender, EventArgs e) => Close();

		private void GroupBox_Paint(object sender, PaintEventArgs e) =>
			(sender as GroupBox).DrawGroupBox(e.Graphics, BackColor, Color.Black, SystemColors.ActiveBorder, FontStyle.Bold);

		#endregion

		#region Generator

		private IGenerator GetGenerator(RDBMS rdbms, string connectionString)
		{
			bool isSupportTableFunctions = rdbms == RDBMS.SQLServer;
			bool isSupportTVPs = rdbms == RDBMS.SQLServer;

			DbObjectsForm dbObjectsForm = new(
											  isSupportTableFunctions,
											  isSupportTVPs,
											  dbObjectsForm_IsEnableTables,
											  dbObjectsForm_IsEnableViews,
											  dbObjectsForm_IsEnableProcedures,
											  dbObjectsForm_IsEnableFunctions,
											  dbObjectsForm_IsEnableTVPs
											 );

			if (dbObjectsForm.ShowDialog(this) == DialogResult.OK)
			{
				if (dbObjectsForm.IsEnableTables || dbObjectsForm.IsEnableViews || dbObjectsForm.IsEnableProcedures ||
					dbObjectsForm.IsEnableFunctions || dbObjectsForm.IsEnableTVPs)
				{
					IGenerator generator = GeneratorWinFormsFactory.GetGenerator(txtPocoEditor);
					generator.Settings.Connection.ConnectionString = connectionString;
					generator.Settings.Connection.RDBMS = rdbms;

					generator.Settings.DatabaseObjects.IncludeAll = false;
					generator.Settings.DatabaseObjects.Tables.IncludeAll = dbObjectsForm.IsEnableTables;
					generator.Settings.DatabaseObjects.Views.IncludeAll = dbObjectsForm.IsEnableViews;
					generator.Settings.DatabaseObjects.StoredProcedures.IncludeAll = dbObjectsForm.IsEnableProcedures;
					generator.Settings.DatabaseObjects.Functions.IncludeAll = dbObjectsForm.IsEnableFunctions;
					generator.Settings.DatabaseObjects.TVPs.IncludeAll = dbObjectsForm.IsEnableTVPs;

					dbObjectsForm_IsEnableTables = dbObjectsForm.IsEnableTables;
					dbObjectsForm_IsEnableViews = dbObjectsForm.IsEnableViews;
					dbObjectsForm_IsEnableProcedures = dbObjectsForm.IsEnableProcedures;
					dbObjectsForm_IsEnableFunctions = dbObjectsForm.IsEnableFunctions;
					dbObjectsForm_IsEnableTVPs = dbObjectsForm.IsEnableTVPs;

					return generator;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		private void SetGeneratorSettings()
		{
			SetPocoSettings();
			SetClassNameSettings();
			SetNavigationPropertySettings();
			SetEFAnnotationsSettings();
		}

		private void SetEFAnnotationsSettings()
		{
			// EF Annotations
			IEFAnnotations efAnnotations = _generator.Settings.EFAnnotations;
			efAnnotations.Enable = chkEF.Checked;
			efAnnotations.Column = chkEFColumn.Checked;
			efAnnotations.Required = chkEFRequired.Checked;
			efAnnotations.RequiredWithErrorMessage = chkEFRequiredWithErrorMessage.Checked;
			efAnnotations.ConcurrencyCheck = chkEFConcurrencyCheck.Checked;
			efAnnotations.StringLength = chkEFStringLength.Checked;
			efAnnotations.Display = chkEFDisplay.Checked;
			efAnnotations.Description = chkEFDescription.Checked;
			efAnnotations.ComplexType = chkEFComplexType.Checked;
			efAnnotations.Index = chkEFIndex.Checked;
			efAnnotations.ForeignKeyAndInverseProperty = chkEFForeignKeyAndInverseProperty.Checked;
		}

		private void SetNavigationPropertySettings()
		{
			// Navigation Properties
			INavigationProperties navigationProperties = _generator.Settings.NavigationProperties;
			navigationProperties.Enable = chkNavigationProperties.Checked;
			navigationProperties.VirtualNavigationProperties = chkVirtualNavigationProperties.Checked;
			navigationProperties.OverrideNavigationProperties = chkOverrideNavigationProperties.Checked;
			navigationProperties.ManyToManyJoinTable = chkManyToManyJoinTable.Checked;
			navigationProperties.Comments = chkNavigationPropertiesComments.Checked;
			navigationProperties.ListNavigationProperties = rdbListNavigationProperties.Checked;
			navigationProperties.IListNavigationProperties = rdbIListNavigationProperties.Checked;
			navigationProperties.ICollectionNavigationProperties = rdbICollectionNavigationProperties.Checked;
			navigationProperties.IEnumerableNavigationProperties = rdbIEnumerableNavigationProperties.Checked;
		}

		private void SetClassNameSettings()
		{
			// Class Name
			IClassName className = _generator.Settings.ClassName;
			className.Singular = chkSingular.Checked;
			className.IncludeDB = chkIncludeDB.Checked;
			className.DBSeparator = txtDBSeparator.Text;
			className.IncludeSchema = chkIncludeSchema.Checked;
			className.IgnoreDboSchema = chkIgnoreDboSchema.Checked;
			className.SchemaSeparator = txtSchemaSeparator.Text;
			className.WordsSeparator = txtWordsSeparator.Text;
			className.CamelCase = chkCamelCase.Checked;
			className.UpperCase = chkUpperCase.Checked;
			className.LowerCase = chkLowerCase.Checked;
			className.Search = txtSearch.Text;
			className.Replace = txtReplace.Text;
			className.SearchIgnoreCase = chkSearchIgnoreCase.Checked;
			className.FixedClassName = txtFixedClassName.Text;
			className.Prefix = txtPrefix.Text;
			className.Suffix = txtSuffix.Text;
		}

		private void SetPocoSettings()
		{
			// POCO
			IPOCO poco = _generator.Settings.POCO;
			poco.Properties = rdbProperties.Checked;
			poco.Fields = rdbFields.Checked;
			poco.VirtualProperties = chkVirtualProperties.Checked;
			poco.OverrideProperties = chkOverrideProperties.Checked;
			poco.PartialClass = chkPartialClass.Checked;
			poco.StructTypesNullable = chkStructTypesNullable.Checked;
			poco.Comments = chkComments.Checked;
			poco.CommentsWithoutNull = chkCommentsWithoutNull.Checked;
			poco.Using = chkUsing.Checked;
			poco.UsingInsideNamespace = chkUsingInsideNamespace.Checked;
			poco.Namespace = txtNamespace.Text;
			poco.WrapAroundEachClass = false;
			poco.Inherit = txtInherit.Text;
			poco.ColumnDefaults = chkColumnDefaults.Checked;
			poco.NewLineBetweenMembers = chkNewLineBetweenMembers.Checked;
			poco.ComplexTypes = chkComplexTypes.Checked;
			poco.EnumSQLTypeToString = rdbEnumSQLTypeToString.Checked;
			poco.EnumSQLTypeToEnumUShort = rdbEnumSQLTypeToEnumUShort.Checked;
			poco.EnumSQLTypeToEnumInt = rdbEnumSQLTypeToEnumInt.Checked;
			poco.Tab = "    ";
		}

		#endregion

		#region Controls Appearance

		private Dictionary<Control, Point> controlsOriginalLocation;

		private void GetControlsOriginalLocation()
		{
			controlsOriginalLocation = new Control[]
				{
					lblWordsSeparator,
					txtWordsSeparator,
					lblWordsSeparatorDesc,
					chkCamelCase,
					chkUpperCase,
					chkLowerCase,
					lblSearch,
					txtSearch,
					lblReplace,
					txtReplace,
					chkSearchIgnoreCase,
					lblFixedName,
					txtFixedClassName,
					lblPrefix,
					txtPrefix,
					lblSuffix,
					txtSuffix
				}
				.ToDictionary(c => c, c => new Point(c.Location.X, c.Location.Y));
		}

		private void SetFormControls(bool isSupportSchema, bool isSupportTVPs, bool isSupportEnumDataType)
		{
			chkIncludeSchema.Visible = isSupportSchema;

			chkIgnoreDboSchema.Visible = isSupportSchema;

			if (isSupportSchema == false)
			{
				txtSchemaSeparator.Text = string.Empty;
			}

			lblSchemaSeparator.Visible = isSupportSchema;
			txtSchemaSeparator.Visible = isSupportSchema;

			rdbFileNameSchemaName.Visible = isSupportSchema;
			rdbFileNameDatabaseSchemaName.Visible = isSupportSchema;

			if (isSupportSchema)
			{
				foreach (KeyValuePair<Control, Point> item in controlsOriginalLocation)
				{
					item.Key.Location = item.Value;
				}
			}
			else
			{
				foreach (KeyValuePair<Control, Point> item in controlsOriginalLocation)
				{
					item.Key.Location = new Point(item.Value.X, item.Value.Y - 23);
				}
			}

			lblSingularDesc.Text = isSupportTVPs ? "(Tables, Views, TVPs)" : "(Tables, Views)";

			if (isSupportEnumDataType)
			{
				panelEnum.Visible = true;
			}
			else
			{
				SetRadioButton(rdbEnumSQLTypeToString, rdbEnumSQLTypeToString_CheckedChanged, true);
				SetRadioButton(rdbEnumSQLTypeToEnumUShort, rdbEnumSQLTypeToEnumUShort_CheckedChanged, false);
				SetRadioButton(rdbEnumSQLTypeToEnumInt, rdbEnumSQLTypeToEnumInt_CheckedChanged, false);

				panelEnum.Visible = false;
			}
		}

		#endregion

		#region Enable/Disable Server Tree

		private void EnableServerTree()
		{
			trvServer.BeforeCollapse -= trvServer_DisableEvent;
			trvServer.BeforeExpand -= trvServer_DisableEvent;
			trvServer.BeforeExpand += trvServer_BeforeExpand;
			EnableServerTreeAfterCheck();
			trvServer.MouseUp += trvServer_MouseUp;
			trvServer.AfterSelect += trvServer_AfterSelect;
		}

		private void DisableServerTree()
		{
			trvServer.BeforeCollapse += trvServer_DisableEvent;
			trvServer.BeforeExpand -= trvServer_BeforeExpand;
			trvServer.BeforeExpand += trvServer_DisableEvent;
			DisableServerTreeAfterCheck();
			trvServer.MouseUp -= trvServer_MouseUp;
			trvServer.AfterSelect -= trvServer_AfterSelect;
		}

		private void trvServer_DisableEvent(object sender, TreeViewCancelEventArgs e) => e.Cancel = true;

		private bool isServerTreeAfterCheckEnabled = true;

		private void EnableServerTreeAfterCheck()
		{
			if (isServerTreeAfterCheckEnabled == false)
			{
				trvServer.AfterCheck += trvServer_AfterCheck;
				isServerTreeAfterCheckEnabled = true;
			}
		}

		private void DisableServerTreeAfterCheck()
		{
			if (isServerTreeAfterCheckEnabled)
			{
				trvServer.AfterCheck -= trvServer_AfterCheck;
				isServerTreeAfterCheckEnabled = false;
			}
		}

		#endregion

		#region Server Tree Check Boxes

		private void trvServer_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			bool isDrawCheckBox =
				e.Node.Tag is not null and (Database or
					IEnumerable<Table> or
					IEnumerable<POCOGenerator.Objects.View> or
					IEnumerable<Procedure> or
					IEnumerable<Function> or
					IEnumerable<TVP> or
					Table or
					POCOGenerator.Objects.View or
					Procedure or
					Function or
					TVP);

			if (isDrawCheckBox == false)
			{
				e.Node.HideCheckBox();
			}

			e.DrawDefault = true;
		}

		private bool isCheckedDatabase;
		private bool isCheckedTables;
		private bool isCheckedViews;
		private bool isCheckedProcedures;
		private bool isCheckedFunctions;
		private bool isCheckedTVPs;

		private void trvServer_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag is not null and Database)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				foreach (TreeNode node in e.Node.Nodes)
				{
					if (node.IsExpanded == false)
					{
						node.Expand();
						Application.DoEvents();
					}
				}

				DatabaseAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and IEnumerable<Table>)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				TablesAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and IEnumerable<POCOGenerator.Objects.View>)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				ViewsAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and IEnumerable<Procedure>)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				ProceduresAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and IEnumerable<Function>)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				FunctionsAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and IEnumerable<TVP>)
			{
				if (e.Node.IsExpanded == false)
				{
					e.Node.Expand();
					Application.DoEvents();
				}

				TVPsAfterCheck(e.Node, e.Node.Checked);
			}
			else if (e.Node.Tag is not null and Table)
			{
				CheckDbObjectNode(e.Node, e.Node.Checked, ref isCheckedTables);
			}
			else if (e.Node.Tag is not null and POCOGenerator.Objects.View)
			{
				CheckDbObjectNode(e.Node, e.Node.Checked, ref isCheckedViews);
			}
			else if (e.Node.Tag is not null and Procedure)
			{
				CheckDbObjectNode(e.Node, e.Node.Checked, ref isCheckedProcedures);
			}
			else if (e.Node.Tag is not null and Function)
			{
				CheckDbObjectNode(e.Node, e.Node.Checked, ref isCheckedFunctions);
			}
			else if (e.Node.Tag is not null and TVP)
			{
				CheckDbObjectNode(e.Node, e.Node.Checked, ref isCheckedTVPs);
			}

			TreeNodeChecked();
		}

		private void DatabaseAfterCheck(TreeNode databaseNode, bool isChecked)
		{
			isCheckedDatabase = isChecked;
			isCheckedTables = isChecked;
			isCheckedViews = isChecked;
			isCheckedProcedures = isChecked;
			isCheckedFunctions = isChecked;
			isCheckedTVPs = isChecked;

			CheckDatabaseNode(databaseNode, isChecked);
		}

		private void CheckDatabaseNode(TreeNode databaseNode, bool isChecked)
		{
			if (databaseNode.Nodes.Count > 0)
			{
				DisableServerTreeAfterCheck();
				foreach (TreeNode dbObjectsNode in databaseNode.Nodes)
				{
					dbObjectsNode.Checked = isChecked;
					foreach (TreeNode node in dbObjectsNode.Nodes)
					{
						node.Checked = isChecked;
					}
				}
				EnableServerTreeAfterCheck();
			}
		}

		private void ChangeCheckDatabaseNode(TreeNode databaseNode)
		{
			if (isCheckedDatabase)
			{
				if (isCheckedTables == false || isCheckedViews == false || isCheckedProcedures == false ||
					isCheckedFunctions == false || isCheckedTVPs == false)
				{
					isCheckedDatabase = false;
					DisableServerTreeAfterCheck();
					databaseNode.Checked = false;
					EnableServerTreeAfterCheck();
				}
			}
			else
			{
				if (isCheckedTables && isCheckedViews && isCheckedProcedures && isCheckedFunctions && isCheckedTVPs)
				{
					isCheckedDatabase = true;
					DisableServerTreeAfterCheck();
					databaseNode.Checked = true;
					EnableServerTreeAfterCheck();
				}
			}
		}

		private void TablesAfterCheck(TreeNode dbObjectsNode, bool isChecked)
		{
			isCheckedTables = isChecked;
			CheckDbObjectsNode(dbObjectsNode, isChecked);
			ChangeCheckDatabaseNode(dbObjectsNode.Parent);
		}

		private void ViewsAfterCheck(TreeNode dbObjectsNode, bool isChecked)
		{
			isCheckedViews = isChecked;
			CheckDbObjectsNode(dbObjectsNode, isChecked);
			ChangeCheckDatabaseNode(dbObjectsNode.Parent);
		}

		private void ProceduresAfterCheck(TreeNode dbObjectsNode, bool isChecked)
		{
			isCheckedProcedures = isChecked;
			CheckDbObjectsNode(dbObjectsNode, isChecked);
			ChangeCheckDatabaseNode(dbObjectsNode.Parent);
		}

		private void FunctionsAfterCheck(TreeNode dbObjectsNode, bool isChecked)
		{
			isCheckedFunctions = isChecked;
			CheckDbObjectsNode(dbObjectsNode, isChecked);
			ChangeCheckDatabaseNode(dbObjectsNode.Parent);
		}

		private void TVPsAfterCheck(TreeNode dbObjectsNode, bool isChecked)
		{
			isCheckedTVPs = isChecked;
			CheckDbObjectsNode(dbObjectsNode, isChecked);
			ChangeCheckDatabaseNode(dbObjectsNode.Parent);
		}

		private void CheckDbObjectsNode(TreeNode dbObjectsNode, bool isChecked)
		{
			if (dbObjectsNode.Nodes.Count > 0)
			{
				DisableServerTreeAfterCheck();
				foreach (TreeNode node in dbObjectsNode.Nodes)
				{
					node.Checked = isChecked;
				}

				EnableServerTreeAfterCheck();
			}
		}

		private void CheckDbObjectNode(TreeNode node, bool isChecked, ref bool isDbObjectsChecked)
		{
			if (isChecked)
			{
				foreach (TreeNode siblingNode in node.Parent.Nodes)
				{
					if (siblingNode.Checked == false)
					{
						return;
					}
				}

				isDbObjectsChecked = true;

				DisableServerTreeAfterCheck();
				node.Parent.Checked = true;
				EnableServerTreeAfterCheck();

				ChangeCheckDatabaseNode(node.Parent.Parent);
			}
			else
			{
				if (isDbObjectsChecked)
				{
					isDbObjectsChecked = false;

					DisableServerTreeAfterCheck();
					node.Parent.Checked = false;
					EnableServerTreeAfterCheck();

					ChangeCheckDatabaseNode(node.Parent.Parent);
				}
			}
		}

		#endregion

		#region Status Message

		private void SetStatusMessage(string message)
		{
			toolStripStatusLabel.Text = message;
			toolStripStatusLabel.ForeColor = Color.Black;
			Application.DoEvents();
		}

		private void SetStatusErrorMessage(string message)
		{
			toolStripStatusLabel.Text = message;
			toolStripStatusLabel.ForeColor = Color.Red;
			Application.DoEvents();
		}

		private void ClearStatus() => SetStatusMessage(string.Empty);

		#endregion

		#region Copy

		private void btnCopy_Click(object sender, EventArgs e)
		{
			string text = txtPocoEditor.Text;
			if (string.IsNullOrEmpty(text))
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(text);
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string text = txtPocoEditor.SelectedText;
			if (string.IsNullOrEmpty(text))
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(text);
			}

			txtPocoEditor.Focus();
		}

		private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			txtPocoEditor.SelectAll();
			txtPocoEditor.Focus();
		}

		#endregion
	}
}

using System.IO;
using System.Text.Json;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region UI Settings

		private bool dbObjectsForm_IsEnableTables;
		private bool dbObjectsForm_IsEnableViews;
		private bool dbObjectsForm_IsEnableProcedures;
		private bool dbObjectsForm_IsEnableFunctions;
		private bool dbObjectsForm_IsEnableTVPs;

		private UISettings GetUISettings()
		{
			return new UISettings()
			{
				RDBMS = _rdbms,
				ConnectionString = _connectionString,
				SupportSchema = _generator == null || _generator.Support.SupportSchema,
				SupportTVPs = _generator == null || _generator.Support.SupportTVPs,
				SupportEnumDataType = _generator != null && _generator.Support.SupportEnumDataType,
				dbObjectsForm_IsEnableTables = dbObjectsForm_IsEnableTables,
				dbObjectsForm_IsEnableViews = dbObjectsForm_IsEnableViews,
				dbObjectsForm_IsEnableProcedures = dbObjectsForm_IsEnableProcedures,
				dbObjectsForm_IsEnableFunctions = dbObjectsForm_IsEnableFunctions,
				dbObjectsForm_IsEnableTVPs = dbObjectsForm_IsEnableTVPs,

				// POCO
				rdbProperties_Checked = rdbProperties.Checked,
				rdbFields_Checked = rdbFields.Checked,
				chkVirtualProperties_Checked = chkVirtualProperties.Checked,
				chkOverrideProperties_Checked = chkOverrideProperties.Checked,
				chkPartialClass_Checked = chkPartialClass.Checked,
				chkStructTypesNullable_Checked = chkStructTypesNullable.Checked,
				chkComments_Checked = chkComments.Checked,
				chkCommentsWithoutNull_Checked = chkCommentsWithoutNull.Checked,
				chkUsing_Checked = chkUsing.Checked,
				chkUsingInsideNamespace_Checked = chkUsingInsideNamespace.Checked,
				txtNamespace_Text = txtNamespace.Text,
				txtInherit_Text = txtInherit.Text,
				chkColumnDefaults_Checked = chkColumnDefaults.Checked,
				chkNewLineBetweenMembers_Checked = chkNewLineBetweenMembers.Checked,
				chkComplexTypes_Checked = chkComplexTypes.Checked,
				rdbEnumSQLTypeToString_Checked = rdbEnumSQLTypeToString.Checked,
				rdbEnumSQLTypeToEnumUShort_Checked = rdbEnumSQLTypeToEnumUShort.Checked,
				rdbEnumSQLTypeToEnumInt_Checked = rdbEnumSQLTypeToEnumInt.Checked,

				// Class Name
				chkSingular_Checked = chkSingular.Checked,
				chkIncludeDB_Checked = chkIncludeDB.Checked,
				txtDBSeparator_Text = txtDBSeparator.Text,
				chkIncludeSchema_Checked = chkIncludeSchema.Checked,
				chkIgnoreDboSchema_Checked = chkIgnoreDboSchema.Checked,
				txtSchemaSeparator_Text = txtSchemaSeparator.Text,
				txtWordsSeparator_Text = txtWordsSeparator.Text,
				chkCamelCase_Checked = chkCamelCase.Checked,
				chkUpperCase_Checked = chkUpperCase.Checked,
				chkLowerCase_Checked = chkLowerCase.Checked,
				txtSearch_Text = txtSearch.Text,
				txtReplace_Text = txtReplace.Text,
				chkSearchIgnoreCase_Checked = chkSearchIgnoreCase.Checked,
				txtFixedClassName_Text = txtFixedClassName.Text,
				txtPrefix_Text = txtPrefix.Text,
				txtSuffix_Text = txtSuffix.Text,

				// Navigation Properties
				chkNavigationProperties_Checked = chkNavigationProperties.Checked,
				chkVirtualNavigationProperties_Checked = chkVirtualNavigationProperties.Checked,
				chkOverrideNavigationProperties_Checked = chkOverrideNavigationProperties.Checked,
				chkManyToManyJoinTable_Checked = chkManyToManyJoinTable.Checked,
				chkNavigationPropertiesComments_Checked = chkNavigationPropertiesComments.Checked,
				rdbListNavigationProperties_Checked = rdbListNavigationProperties.Checked,
				rdbIListNavigationProperties_Checked = rdbIListNavigationProperties.Checked,
				rdbICollectionNavigationProperties_Checked = rdbICollectionNavigationProperties.Checked,
				rdbIEnumerableNavigationProperties_Checked = rdbIEnumerableNavigationProperties.Checked,

				// EF Annotations
				chkEF_Checked = chkEF.Checked,
				chkEFColumn_Checked = chkEFColumn.Checked,
				chkEFRequired_Checked = chkEFRequired.Checked,
				chkEFRequiredWithErrorMessage_Checked = chkEFRequiredWithErrorMessage.Checked,
				chkEFConcurrencyCheck_Checked = chkEFConcurrencyCheck.Checked,
				chkEFStringLength_Checked = chkEFStringLength.Checked,
				chkEFDisplay_Checked = chkEFDisplay.Checked,
				chkEFDescription_Checked = chkEFDescription.Checked,
				chkEFComplexType_Checked = chkEFComplexType.Checked,
				chkEFIndex_Checked = chkEFIndex.Checked,
				chkEFForeignKeyAndInverseProperty_Checked = chkEFForeignKeyAndInverseProperty.Checked,

				// Export To Files
				txtFolder_Text = txtFolder.Text,
				rdbSingleFile_Checked = rdbSingleFile.Checked,
				rdbMultipleFilesSingleFolder_Checked = rdbMultipleFilesSingleFolder.Checked,
				rdbMultipleFilesRelativeFolders_Checked = rdbMultipleFilesRelativeFolders.Checked,
				rdbFileNameName_Checked = rdbFileNameName.Checked,
				rdbFileNameSchemaName_Checked = rdbFileNameSchemaName.Checked,
				rdbFileNameDatabaseName_Checked = rdbFileNameDatabaseName.Checked,
				rdbFileNameDatabaseSchemaName_Checked = rdbFileNameDatabaseSchemaName.Checked
			};
		}

		private bool LoadUISettings()
		{
			UISettings settings = DeserializeUISettings();
			if (settings == null)
			{
				// fallback to SQL Server
				SetFormControls(true /*SupportSchema*/, true /*SupportTVPs*/, false /*SupportEnumDataType*/);
				return false;
			}
			_rdbms = settings.RDBMS;
			_connectionString = settings.ConnectionString;
			dbObjectsForm_IsEnableTables = settings.dbObjectsForm_IsEnableTables;
			dbObjectsForm_IsEnableViews = settings.dbObjectsForm_IsEnableViews;
			dbObjectsForm_IsEnableProcedures = settings.dbObjectsForm_IsEnableProcedures;
			dbObjectsForm_IsEnableFunctions = settings.dbObjectsForm_IsEnableFunctions;
			dbObjectsForm_IsEnableTVPs = settings.dbObjectsForm_IsEnableTVPs;

			// POCO
			SetRadioButton(rdbProperties, rdbProperties_CheckedChanged, settings.rdbProperties_Checked);
			SetRadioButton(rdbFields, rdbFields_CheckedChanged, settings.rdbFields_Checked);
			SetCheckBox(chkVirtualProperties, chkVirtualProperties_CheckedChanged, settings.chkVirtualProperties_Checked);
			SetCheckBox(chkOverrideProperties, chkOverrideProperties_CheckedChanged, settings.chkOverrideProperties_Checked);
			SetCheckBox(chkPartialClass, chkPartialClass_CheckedChanged, settings.chkPartialClass_Checked);
			SetCheckBox(chkStructTypesNullable, chkStructTypesNullable_CheckedChanged, settings.chkStructTypesNullable_Checked);
			SetCheckBox(chkComments, chkComments_CheckedChanged, settings.chkComments_Checked);
			SetCheckBox(chkCommentsWithoutNull, chkCommentsWithoutNull_CheckedChanged, settings.chkCommentsWithoutNull_Checked);
			SetCheckBox(chkUsing, chkUsing_CheckedChanged, settings.chkUsing_Checked);
			SetCheckBox(chkUsingInsideNamespace, chkUsingInsideNamespace_CheckedChanged, settings.chkUsingInsideNamespace_Checked);
			SetTextBox(txtNamespace, txtNamespace_TextChanged, settings.txtNamespace_Text);
			SetTextBox(txtInherit, txtInherit_TextChanged, settings.txtInherit_Text);
			SetCheckBox(chkColumnDefaults, chkColumnDefaults_CheckedChanged, settings.chkColumnDefaults_Checked);
			SetCheckBox(chkNewLineBetweenMembers, chkNewLineBetweenMembers_CheckedChanged,
						settings.chkNewLineBetweenMembers_Checked);
			SetCheckBox(chkComplexTypes, chkComplexTypes_CheckedChanged, settings.chkComplexTypes_Checked);
			SetRadioButton(rdbEnumSQLTypeToString, rdbEnumSQLTypeToString_CheckedChanged, settings.rdbEnumSQLTypeToString_Checked);
			SetRadioButton(rdbEnumSQLTypeToEnumUShort, rdbEnumSQLTypeToEnumUShort_CheckedChanged,
						   settings.rdbEnumSQLTypeToEnumUShort_Checked);
			SetRadioButton(rdbEnumSQLTypeToEnumInt, rdbEnumSQLTypeToEnumInt_CheckedChanged,
						   settings.rdbEnumSQLTypeToEnumInt_Checked);

			// Class Name
			SetCheckBox(chkSingular, chkSingular_CheckedChanged, settings.chkSingular_Checked);
			SetCheckBox(chkIncludeDB, chkIncludeDB_CheckedChanged, settings.chkIncludeDB_Checked);
			SetTextBox(txtDBSeparator, txtDBSeparator_TextChanged, settings.txtDBSeparator_Text);
			SetCheckBox(chkIncludeSchema, chkIncludeSchema_CheckedChanged, settings.chkIncludeSchema_Checked);
			SetCheckBox(chkIgnoreDboSchema, chkIgnoreDboSchema_CheckedChanged, settings.chkIgnoreDboSchema_Checked);
			SetTextBox(txtSchemaSeparator, txtSchemaSeparator_TextChanged, settings.txtSchemaSeparator_Text);
			SetTextBox(txtWordsSeparator, txtWordsSeparator_TextChanged, settings.txtWordsSeparator_Text);
			SetCheckBox(chkCamelCase, chkCamelCase_CheckedChanged, settings.chkCamelCase_Checked);
			SetCheckBox(chkUpperCase, chkUpperCase_CheckedChanged, settings.chkUpperCase_Checked);
			SetCheckBox(chkLowerCase, chkLowerCase_CheckedChanged, settings.chkLowerCase_Checked);
			SetTextBox(txtSearch, txtSearch_TextChanged, settings.txtSearch_Text);
			SetTextBox(txtReplace, txtReplace_TextChanged, settings.txtReplace_Text);
			SetCheckBox(chkSearchIgnoreCase, chkSearchIgnoreCase_CheckedChanged, settings.chkSearchIgnoreCase_Checked);
			SetTextBox(txtFixedClassName, txtFixedClassName_TextChanged, settings.txtFixedClassName_Text);
			SetTextBox(txtPrefix, txtPrefix_TextChanged, settings.txtPrefix_Text);
			SetTextBox(txtSuffix, txtSuffix_TextChanged, settings.txtSuffix_Text);

			// Navigation Properties
			SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, settings.chkNavigationProperties_Checked);
			SetCheckBox(chkVirtualNavigationProperties, chkVirtualNavigationProperties_CheckedChanged,
						settings.chkVirtualNavigationProperties_Checked);
			SetCheckBox(chkOverrideNavigationProperties, chkOverrideNavigationProperties_CheckedChanged,
						settings.chkOverrideNavigationProperties_Checked);
			SetCheckBox(chkManyToManyJoinTable, chkManyToManyJoinTable_CheckedChanged, settings.chkManyToManyJoinTable_Checked);
			SetCheckBox(chkNavigationPropertiesComments, chkNavigationPropertiesComments_CheckedChanged,
						settings.chkNavigationPropertiesComments_Checked);
			SetRadioButton(rdbListNavigationProperties, rdbListNavigationProperties_CheckedChanged,
						   settings.rdbListNavigationProperties_Checked);
			SetRadioButton(rdbIListNavigationProperties, rdbIListNavigationProperties_CheckedChanged,
						   settings.rdbIListNavigationProperties_Checked);
			SetRadioButton(rdbICollectionNavigationProperties, rdbICollectionNavigationProperties_CheckedChanged,
						   settings.rdbICollectionNavigationProperties_Checked);
			SetRadioButton(rdbIEnumerableNavigationProperties, rdbIEnumerableNavigationProperties_CheckedChanged,
						   settings.rdbIEnumerableNavigationProperties_Checked);

			// EF Annotations
			SetCheckBox(chkEF, chkEF_CheckedChanged, settings.chkEF_Checked);
			SetCheckBox(chkEFColumn, chkEFColumn_CheckedChanged, settings.chkEFColumn_Checked);
			SetCheckBox(chkEFRequired, chkEFRequired_CheckedChanged, settings.chkEFRequired_Checked);
			SetCheckBox(chkEFRequiredWithErrorMessage, chkEFRequiredWithErrorMessage_CheckedChanged,
						settings.chkEFRequiredWithErrorMessage_Checked);
			SetCheckBox(chkEFConcurrencyCheck, chkEFConcurrencyCheck_CheckedChanged, settings.chkEFConcurrencyCheck_Checked);
			SetCheckBox(chkEFStringLength, chkEFStringLength_CheckedChanged, settings.chkEFStringLength_Checked);
			SetCheckBox(chkEFDisplay, chkEFDisplay_CheckedChanged, settings.chkEFDisplay_Checked);
			SetCheckBox(chkEFDescription, chkEFDescription_CheckedChanged, settings.chkEFDescription_Checked);
			SetCheckBox(chkEFComplexType, chkEFComplexType_CheckedChanged, settings.chkEFComplexType_Checked);
			SetCheckBox(chkEFIndex, chkEFIndex_CheckedChanged, settings.chkEFIndex_Checked);
			SetCheckBox(chkEFForeignKeyAndInverseProperty, chkEFForeignKeyAndInverseProperty_CheckedChanged,
						settings.chkEFForeignKeyAndInverseProperty_Checked);

			// Export To Files
			txtFolder.Text = settings.txtFolder_Text;
			rdbSingleFile.Checked = settings.rdbSingleFile_Checked;
			rdbMultipleFilesSingleFolder.Checked = settings.rdbMultipleFilesSingleFolder_Checked;
			rdbMultipleFilesRelativeFolders.Checked = settings.rdbMultipleFilesRelativeFolders_Checked;
			rdbFileNameName.Checked = settings.rdbFileNameName_Checked;
			rdbFileNameSchemaName.Checked = settings.rdbFileNameSchemaName_Checked;
			rdbFileNameDatabaseName.Checked = settings.rdbFileNameDatabaseName_Checked;
			rdbFileNameDatabaseSchemaName.Checked = settings.rdbFileNameDatabaseSchemaName_Checked;
			SetFormControls(settings.SupportSchema, settings.SupportTVPs, settings.SupportEnumDataType);
			return true;
		}

		private const string settingsFileName = "POCOGeneratorUI.settings";

		private void SerializeUISettings()
		{
			try
			{
				UISettings settings = GetUISettings();
				using (FileStream fs = new(settingsFileName, FileMode.Create))
				{
					//new BinaryFormatter().Serialize(fs, settings);
					JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
					{
						WriteIndented = true,
					};
					JsonSerializer.Serialize(fs, settings, options);
				}
			}
			catch
			{
			}
		}

		private UISettings DeserializeUISettings()
		{
			try
			{
				if (!File.Exists(settingsFileName))
				{
					return null;
				}
				using (FileStream fs = new(settingsFileName, FileMode.Open))
				{
					//return (UISettings)new BinaryFormatter().Deserialize(fs);
					return JsonSerializer.Deserialize<UISettings>(fs);
				}
			}
			catch
			{
				return null;
			}
		}

		#endregion
	}
}

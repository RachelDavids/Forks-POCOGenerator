using System;
using System.Windows.Forms;

namespace POCOGeneratorUI
{
	public sealed partial class POCOGeneratorForm
	{
		#region POCO Options

		#region Set Control

		private bool SetCheckBox(CheckBox chk, EventHandler checkedChangedHandler, bool isChecked)
		{
			chk.CheckedChanged -= checkedChangedHandler;
			bool isSettingChanged = chk.Checked != isChecked;
			chk.Checked = isChecked;
			chk.CheckedChanged += checkedChangedHandler;
			return isSettingChanged;
		}

		private bool SetRadioButton(RadioButton rdb, EventHandler checkedChangedHandler, bool isChecked)
		{
			rdb.CheckedChanged -= checkedChangedHandler;
			bool isSettingChanged = rdb.Checked != isChecked;
			rdb.Checked = isChecked;
			rdb.CheckedChanged += checkedChangedHandler;
			return isSettingChanged;
		}

		private bool SetTextBox(TextBox txt, EventHandler textChangedHandler, string text)
		{
			txt.TextChanged -= textChangedHandler;
			bool isSettingChanged = (txt.Text ?? string.Empty) != (text ?? string.Empty);
			txt.Text = text;
			txt.TextChanged += textChangedHandler;
			return isSettingChanged;
		}

		#endregion

		#region POCO

		private void rdbProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (rdbProperties.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbFields_CheckedChanged(object sender, EventArgs e)
		{
			if (rdbFields.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void chkVirtualProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkVirtualProperties.Checked && chkOverrideProperties.Checked)
			{
				SetCheckBox(chkOverrideProperties, chkOverrideProperties_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkOverrideProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkVirtualProperties.Checked && chkOverrideProperties.Checked)
			{
				SetCheckBox(chkVirtualProperties, chkVirtualProperties_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkPartialClass_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkStructTypesNullable_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();

		private void chkComments_CheckedChanged(object sender, EventArgs e)
		{
			if (chkComments.Checked && chkCommentsWithoutNull.Checked)
			{
				SetCheckBox(chkCommentsWithoutNull, chkCommentsWithoutNull_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkCommentsWithoutNull_CheckedChanged(object sender, EventArgs e)
		{
			if (chkComments.Checked && chkCommentsWithoutNull.Checked)
			{
				SetCheckBox(chkComments, chkComments_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkUsing_CheckedChanged(object sender, EventArgs e)
		{
			if (chkUsing.Checked == false && chkUsingInsideNamespace.Checked)
			{
				SetCheckBox(chkUsingInsideNamespace, chkUsingInsideNamespace_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkUsingInsideNamespace_CheckedChanged(object sender, EventArgs e)
		{
			if (chkUsingInsideNamespace.Checked && chkUsing.Checked == false)
			{
				SetCheckBox(chkUsing, chkUsing_CheckedChanged, true);
			}
			POCOOptionChanged();
		}

		private void txtNamespace_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtInherit_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkColumnDefaults_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkNewLineBetweenMembers_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();

		private void chkComplexTypes_CheckedChanged(object sender, EventArgs e)
		{
			if (chkComplexTypes.Checked == false)
			{
				SetCheckBox(chkEFComplexType, chkEFComplexType_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void rdbEnumSQLTypeToString_CheckedChanged(object sender, EventArgs e)
		{
			if (_generator != null && _generator.Support.SupportEnumDataType && rdbEnumSQLTypeToString.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbEnumSQLTypeToEnumUShort_CheckedChanged(object sender, EventArgs e)
		{
			if (_generator != null && _generator.Support.SupportEnumDataType && rdbEnumSQLTypeToEnumUShort.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbEnumSQLTypeToEnumInt_CheckedChanged(object sender, EventArgs e)
		{
			if (_generator != null && _generator.Support.SupportEnumDataType && rdbEnumSQLTypeToEnumInt.Checked)
			{
				POCOOptionChanged();
			}
		}

		#endregion

		#region Class Name

		private void chkSingular_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkIncludeDB_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtDBSeparator_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkIncludeSchema_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkIgnoreDboSchema_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtSchemaSeparator_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtWordsSeparator_TextChanged(object sender, EventArgs e) => POCOOptionChanged();

		private void chkCamelCase_CheckedChanged(object sender, EventArgs e)
		{
			if (chkCamelCase.Checked)
			{
				SetCheckBox(chkUpperCase, chkUpperCase_CheckedChanged, false);
				SetCheckBox(chkLowerCase, chkLowerCase_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkUpperCase_CheckedChanged(object sender, EventArgs e)
		{
			if (chkUpperCase.Checked)
			{
				SetCheckBox(chkCamelCase, chkCamelCase_CheckedChanged, false);
				SetCheckBox(chkLowerCase, chkLowerCase_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkLowerCase_CheckedChanged(object sender, EventArgs e)
		{
			if (chkLowerCase.Checked)
			{
				SetCheckBox(chkCamelCase, chkCamelCase_CheckedChanged, false);
				SetCheckBox(chkUpperCase, chkUpperCase_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void txtSearch_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtReplace_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void chkSearchIgnoreCase_CheckedChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtFixedClassName_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtPrefix_TextChanged(object sender, EventArgs e) => POCOOptionChanged();
		private void txtSuffix_TextChanged(object sender, EventArgs e) => POCOOptionChanged();

		#endregion

		#region Navigation Properties

		private void chkNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkVirtualNavigationProperties, chkVirtualNavigationProperties_CheckedChanged, false);
				SetCheckBox(chkOverrideNavigationProperties, chkOverrideNavigationProperties_CheckedChanged, false);
				SetCheckBox(chkManyToManyJoinTable, chkManyToManyJoinTable_CheckedChanged, false);
				SetCheckBox(chkNavigationPropertiesComments, chkNavigationPropertiesComments_CheckedChanged, false);
				SetCheckBox(chkEFForeignKeyAndInverseProperty, chkEFForeignKeyAndInverseProperty_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkVirtualNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkVirtualNavigationProperties.Checked && chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, true);
			}
			if (chkVirtualNavigationProperties.Checked && chkOverrideNavigationProperties.Checked)
			{
				SetCheckBox(chkOverrideNavigationProperties, chkOverrideNavigationProperties_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkOverrideNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkOverrideNavigationProperties.Checked && chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, true);
			}
			if (chkVirtualNavigationProperties.Checked && chkOverrideNavigationProperties.Checked)
			{
				SetCheckBox(chkVirtualNavigationProperties, chkVirtualNavigationProperties_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void chkManyToManyJoinTable_CheckedChanged(object sender, EventArgs e)
		{
			if (chkManyToManyJoinTable.Checked && chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, true);
			}
			POCOOptionChanged();
		}

		private void chkNavigationPropertiesComments_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationPropertiesComments.Checked && chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, true);
			}
			POCOOptionChanged();
		}

		private void rdbListNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationProperties.Checked && rdbListNavigationProperties.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbIListNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationProperties.Checked && rdbIListNavigationProperties.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbICollectionNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationProperties.Checked && rdbICollectionNavigationProperties.Checked)
			{
				POCOOptionChanged();
			}
		}

		private void rdbIEnumerableNavigationProperties_CheckedChanged(object sender, EventArgs e)
		{
			if (chkNavigationProperties.Checked && rdbIEnumerableNavigationProperties.Checked)
			{
				POCOOptionChanged();
			}
		}

		#endregion

		#region EF Annotations

		private void chkEF_CheckedChanged(object sender, EventArgs e)
		{
			if (chkEF.Checked == false)
			{
				SetCheckBox(chkEFColumn, chkEFColumn_CheckedChanged, false);
				SetCheckBox(chkEFRequired, chkEFRequired_CheckedChanged, false);
				SetCheckBox(chkEFRequiredWithErrorMessage, chkEFRequiredWithErrorMessage_CheckedChanged, false);
				SetCheckBox(chkEFConcurrencyCheck, chkEFConcurrencyCheck_CheckedChanged, false);
				SetCheckBox(chkEFStringLength, chkEFStringLength_CheckedChanged, false);
				SetCheckBox(chkEFDisplay, chkEFDisplay_CheckedChanged, false);
				SetCheckBox(chkEFDescription, chkEFDescription_CheckedChanged, false);
				SetCheckBox(chkEFComplexType, chkEFComplexType_CheckedChanged, false);
				SetCheckBox(chkEFIndex, chkEFIndex_CheckedChanged, false);
				SetCheckBox(chkEFForeignKeyAndInverseProperty, chkEFForeignKeyAndInverseProperty_CheckedChanged, false);
			}
			POCOOptionChanged();
		}

		private void CheckEFCheckBox(bool otherEF)
		{
			if (otherEF && chkEF.Checked == false)
			{
				SetCheckBox(chkEF, chkEF_CheckedChanged, true);
			}
		}

		private void chkEFColumn_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFColumn.Checked);
			POCOOptionChanged();
		}

		private void chkEFRequired_CheckedChanged(object sender, EventArgs e)
		{
			if (chkEFRequired.Checked && chkEFRequiredWithErrorMessage.Checked)
			{
				SetCheckBox(chkEFRequiredWithErrorMessage, chkEFRequiredWithErrorMessage_CheckedChanged, false);
			}
			CheckEFCheckBox(chkEFRequired.Checked);
			POCOOptionChanged();
		}

		private void chkEFRequiredWithErrorMessage_CheckedChanged(object sender, EventArgs e)
		{
			if (chkEFRequired.Checked && chkEFRequiredWithErrorMessage.Checked)
			{
				SetCheckBox(chkEFRequired, chkEFRequired_CheckedChanged, false);
			}
			CheckEFCheckBox(chkEFRequiredWithErrorMessage.Checked);
			POCOOptionChanged();
		}

		private void chkEFConcurrencyCheck_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFConcurrencyCheck.Checked);
			POCOOptionChanged();
		}

		private void chkEFStringLength_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFStringLength.Checked);
			POCOOptionChanged();
		}

		private void chkEFDisplay_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFDisplay.Checked);
			POCOOptionChanged();
		}

		private void chkEFDescription_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFDescription.Checked);
			POCOOptionChanged();
		}

		private void chkEFComplexType_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFComplexType.Checked);
			if (chkEFComplexType.Checked && chkComplexTypes.Checked == false)
			{
				SetCheckBox(chkComplexTypes, chkComplexTypes_CheckedChanged, true);
			}
			POCOOptionChanged();
		}

		private void chkEFIndex_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFIndex.Checked);
			POCOOptionChanged();
		}

		private void chkEFForeignKeyAndInverseProperty_CheckedChanged(object sender, EventArgs e)
		{
			CheckEFCheckBox(chkEFForeignKeyAndInverseProperty.Checked);
			if (chkEFForeignKeyAndInverseProperty.Checked && chkNavigationProperties.Checked == false)
			{
				SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, true);
			}
			POCOOptionChanged();
		}

		#endregion

		#region Reset

		private void btnResetPOCOSettings_Click(object sender, EventArgs e)
		{
			bool isAnySettingChanged = ResetPOCOSettings();
			if (isAnySettingChanged)
			{
				POCOOptionChanged();
			}
		}

		private bool ResetPOCOSettings()
		{
			bool isAnySettingChanged = false;
			isAnySettingChanged |= SetRadioButton(rdbProperties, rdbProperties_CheckedChanged, true);
			isAnySettingChanged |= SetRadioButton(rdbFields, rdbFields_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkVirtualProperties, chkVirtualProperties_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkOverrideProperties, chkOverrideProperties_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkPartialClass, chkPartialClass_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkStructTypesNullable, chkStructTypesNullable_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkComments, chkComments_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkCommentsWithoutNull, chkCommentsWithoutNull_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkUsing, chkUsing_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkUsingInsideNamespace, chkUsingInsideNamespace_CheckedChanged, false);
			isAnySettingChanged |= SetTextBox(txtNamespace, txtNamespace_TextChanged, string.Empty);
			isAnySettingChanged |= SetTextBox(txtInherit, txtInherit_TextChanged, string.Empty);
			isAnySettingChanged |= SetCheckBox(chkColumnDefaults, chkColumnDefaults_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkNewLineBetweenMembers, chkNewLineBetweenMembers_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkComplexTypes, chkComplexTypes_CheckedChanged, false);
			isAnySettingChanged |= SetRadioButton(rdbEnumSQLTypeToString, rdbEnumSQLTypeToString_CheckedChanged, true);
			isAnySettingChanged |= SetRadioButton(rdbEnumSQLTypeToEnumUShort, rdbEnumSQLTypeToEnumUShort_CheckedChanged, false);
			isAnySettingChanged |= SetRadioButton(rdbEnumSQLTypeToEnumInt, rdbEnumSQLTypeToEnumInt_CheckedChanged, false);
			return isAnySettingChanged;
		}

		private void btnResetClassNameSettings_Click(object sender, EventArgs e)
		{
			bool isAnySettingChanged = ResetClassNameSettings();
			if (isAnySettingChanged)
			{
				POCOOptionChanged();
			}
		}

		private bool ResetClassNameSettings()
		{
			bool isAnySettingChanged = false;
			isAnySettingChanged |= SetCheckBox(chkSingular, chkSingular_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkIncludeDB, chkIncludeDB_CheckedChanged, false);
			isAnySettingChanged |= SetTextBox(txtDBSeparator, txtDBSeparator_TextChanged, string.Empty);
			isAnySettingChanged |= SetCheckBox(chkIncludeSchema, chkIncludeSchema_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkIgnoreDboSchema, chkIgnoreDboSchema_CheckedChanged, false);
			isAnySettingChanged |= SetTextBox(txtSchemaSeparator, txtSchemaSeparator_TextChanged, string.Empty);
			isAnySettingChanged |= SetTextBox(txtWordsSeparator, txtWordsSeparator_TextChanged, string.Empty);
			isAnySettingChanged |= SetCheckBox(chkCamelCase, chkCamelCase_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkUpperCase, chkUpperCase_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkLowerCase, chkLowerCase_CheckedChanged, false);
			isAnySettingChanged |= SetTextBox(txtSearch, txtSearch_TextChanged, string.Empty);
			isAnySettingChanged |= SetTextBox(txtReplace, txtReplace_TextChanged, string.Empty);
			isAnySettingChanged |= SetCheckBox(chkSearchIgnoreCase, chkSearchIgnoreCase_CheckedChanged, false);
			isAnySettingChanged |= SetTextBox(txtFixedClassName, txtFixedClassName_TextChanged, string.Empty);
			isAnySettingChanged |= SetTextBox(txtPrefix, txtPrefix_TextChanged, string.Empty);
			isAnySettingChanged |= SetTextBox(txtSuffix, txtSuffix_TextChanged, string.Empty);
			return isAnySettingChanged;
		}

		private void btnResetNavigationPropertiesSettings_Click(object sender, EventArgs e)
		{
			bool isAnySettingChanged = ResetNavigationPropertiesSettings();
			if (isAnySettingChanged)
			{
				POCOOptionChanged();
			}
		}

		private bool ResetNavigationPropertiesSettings()
		{
			bool isAnySettingChanged = false;
			isAnySettingChanged |= SetCheckBox(chkNavigationProperties, chkNavigationProperties_CheckedChanged, false);
			isAnySettingChanged |=
				SetCheckBox(chkVirtualNavigationProperties, chkVirtualNavigationProperties_CheckedChanged, false);
			isAnySettingChanged |=
				SetCheckBox(chkOverrideNavigationProperties, chkOverrideNavigationProperties_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkManyToManyJoinTable, chkManyToManyJoinTable_CheckedChanged, false);
			isAnySettingChanged |=
				SetCheckBox(chkNavigationPropertiesComments, chkNavigationPropertiesComments_CheckedChanged, false);
			isAnySettingChanged |= SetRadioButton(rdbListNavigationProperties, rdbListNavigationProperties_CheckedChanged, true);
			isAnySettingChanged |= SetRadioButton(rdbIListNavigationProperties, rdbIListNavigationProperties_CheckedChanged, false);
			isAnySettingChanged |= SetRadioButton(rdbICollectionNavigationProperties,
												  rdbICollectionNavigationProperties_CheckedChanged, false);
			isAnySettingChanged |= SetRadioButton(rdbIEnumerableNavigationProperties,
												  rdbIEnumerableNavigationProperties_CheckedChanged, false);
			return isAnySettingChanged;
		}

		private void btnResetEFAnnotationsSettings_Click(object sender, EventArgs e)
		{
			bool isAnySettingChanged = ResetEFAnnotationsSettings();
			if (isAnySettingChanged)
			{
				POCOOptionChanged();
			}
		}

		private bool ResetEFAnnotationsSettings()
		{
			bool isAnySettingChanged = false;
			isAnySettingChanged |= SetCheckBox(chkEF, chkEF_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFColumn, chkEFColumn_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFRequired, chkEFRequired_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFRequiredWithErrorMessage, chkEFRequiredWithErrorMessage_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFConcurrencyCheck, chkEFConcurrencyCheck_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFStringLength, chkEFStringLength_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFDisplay, chkEFDisplay_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFDescription, chkEFDescription_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFComplexType, chkEFComplexType_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFIndex, chkEFIndex_CheckedChanged, false);
			isAnySettingChanged |= SetCheckBox(chkEFForeignKeyAndInverseProperty, chkEFForeignKeyAndInverseProperty_CheckedChanged,
											   false);
			return isAnySettingChanged;
		}

		private void btnReset_Click(object sender, EventArgs e)
		{
			bool isAnySettingChanged =
				ResetPOCOSettings() |
				ResetNavigationPropertiesSettings() |
				ResetClassNameSettings() |
				ResetEFAnnotationsSettings();
			if (isAnySettingChanged)
			{
				POCOOptionChanged();
			}
		}

		#endregion

		#endregion
	}
}

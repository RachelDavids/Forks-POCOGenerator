using System;
using System.Windows.Forms;

namespace POCOGeneratorUI.Filtering
{
	public partial class FilterSettingsForm : Form
	{
		public FilterSettingsForm(FilterSettings filterSettings, bool isSupportSchema)
		{
			InitializeComponent();

			SetFilterSettingsForm();
			SetSchemaSupport(isSupportSchema);
			SetFilter(filterSettings);
		}

		private class FilterItem(FilterType filterType)
		{
			public FilterType FilterType { get; private set; } = filterType;

			public override string ToString()
			{
				return FilterType.ToString().Replace('_', ' ');
			}
		}

		private void SetFilterSettingsForm()
		{
			ddlFilterName.Items.Add(new FilterItem(FilterType.Equals));
			ddlFilterName.Items.Add(new FilterItem(FilterType.Contains));
			ddlFilterName.Items.Add(new FilterItem(FilterType.DoesNotContain));

			ddlFilterSchema.Items.Add(new FilterItem(FilterType.Equals));
			ddlFilterSchema.Items.Add(new FilterItem(FilterType.Contains));
			ddlFilterSchema.Items.Add(new FilterItem(FilterType.DoesNotContain));

			ClearFilter();
		}

		private void SetSchemaSupport(bool isSupportSchema)
		{
			lblFilterSchema.Visible = isSupportSchema;
			ddlFilterSchema.Visible = isSupportSchema;
			txtFilterSchema.Visible = isSupportSchema;
			int height = 150;
			if (!isSupportSchema)
			{
				height -= 50;
			}
			Height = height;
		}

		private void SetFilter(FilterSettings filterSettings)
		{
			SetFilterName(filterSettings.FilterName.FilterType, filterSettings.FilterName.Filter);
			SetFilterSchema(filterSettings.FilterSchema.FilterType, filterSettings.FilterSchema.Filter);
		}

		private void ClearFilter()
		{
			SetFilterName(FilterType.Contains, null);
			SetFilterSchema(FilterType.Contains, null);
		}

		private void SetFilterName(FilterType filterType, string value)
		{
			ddlFilterName.SelectedIndex = (int)filterType;
			txtFilterName.Text = value;
		}

		private void SetFilterSchema(FilterType filterType, string value)
		{
			ddlFilterSchema.SelectedIndex = (int)filterType;
			txtFilterSchema.Text = value;
		}

		public FilterType FilterTypeName => ((FilterItem)ddlFilterName.SelectedItem).FilterType;

		public string FilterName => txtFilterName.Text;

		public FilterType FilterTypeSchema => ((FilterItem)ddlFilterSchema.SelectedItem).FilterType;

		public string FilterSchema => txtFilterSchema.Text;

		private void btnClearFilter_Click(object sender, EventArgs e)
		{
			ClearFilter();
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void FilterSettingsForm_Shown(object sender, EventArgs e)
		{
			txtFilterName.Focus();
		}
	}
}

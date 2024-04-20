using System;

namespace POCOGeneratorUI.Filtering
{
	public enum FilterType
	{
		Equals,
		Contains,
		DoesNotContain
	}

	public class FilterSetting
	{
		public FilterType FilterType { get; set; }
		public string Filter { get; set; }

		public FilterSetting()
		{
			FilterType = FilterType.Contains;
			Filter = null;
		}

		public bool IsEnabled => !String.IsNullOrEmpty(Filter);
	}

	public class FilterSettings
	{
		public FilterSetting FilterName { get; set; }
		public FilterSetting FilterSchema { get; set; }

		public FilterSettings()
		{
			FilterName = new FilterSetting();
			FilterSchema = new FilterSetting();
		}

		public bool IsEnabled => FilterName.IsEnabled || FilterSchema.IsEnabled;
	}
}

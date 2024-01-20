using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings determine which views to generate classes out of and which views to exclude from generating classes.</summary>
	public interface IViews
	{
		/// <summary>Resets the views settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to include all views to generated classes.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets or sets a value indicating whether to exclude all views from generated classes.</summary>
		bool ExcludeAll { get; set; }

		/// <summary>List of views to include to generated classes.</summary>
		IList<string> Include { get; }

		/// <summary>List of views to exclude from generated classes.</summary>
		IList<string> Exclude { get; }
	}
}

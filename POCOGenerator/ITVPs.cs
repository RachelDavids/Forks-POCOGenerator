using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings determine which TVPs to generate classes out of and which TVPs to exclude from generating classes.</summary>
	public interface ITVPs
	{
		/// <summary>Resets the TVPs settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to include all TVPs to generated classes.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets or sets a value indicating whether to exclude all TVPs from generated classes.</summary>
		bool ExcludeAll { get; set; }

		/// <summary>List of TVPs to include to generated classes.</summary>
		IList<string> Include { get; }

		/// <summary>List of TVPs to exclude from generated classes.</summary>
		IList<string> Exclude { get; }
	}
}

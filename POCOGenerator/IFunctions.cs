using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings determine which functions to generate classes out of and which functions to exclude from generating classes.</summary>
	public interface IFunctions
	{
		/// <summary>Resets the functions settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to include all functions to generated classes.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets or sets a value indicating whether to exclude all functions from generated classes.</summary>
		bool ExcludeAll { get; set; }

		/// <summary>List of functions to include to generated classes.</summary>
		IList<string> Include { get; }

		/// <summary>List of functions to exclude from generated classes.</summary>
		IList<string> Exclude { get; }
	}
}

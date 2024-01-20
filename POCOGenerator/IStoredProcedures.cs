using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings determine which stored procedures to generate classes out of and which stored procedures to exclude from generating classes.</summary>
	public interface IStoredProcedures
	{
		/// <summary>Resets the stored procedures settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to include all stored procedures to generated classes.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets or sets a value indicating whether to exclude all stored procedures from generated classes.</summary>
		bool ExcludeAll { get; set; }

		/// <summary>List of stored procedures to include to generated classes.</summary>
		IList<string> Include { get; }

		/// <summary>List of stored procedures to exclude from generated classes.</summary>
		IList<string> Exclude { get; }
	}
}

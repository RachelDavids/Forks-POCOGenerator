using System.Collections.Generic;

namespace POCOGenerator
{
	/// <summary>The settings determine which tables to generate classes out of and which tables to exclude from generating classes.</summary>
	public interface ITables
	{
		/// <summary>Resets the tables settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to include all tables to generated classes.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets or sets a value indicating whether to exclude all tables from generated classes.</summary>
		bool ExcludeAll { get; set; }

		/// <summary>List of tables to include to generated classes.</summary>
		IList<string> Include { get; }

		/// <summary>List of tables to exclude from generated classes.</summary>
		IList<string> Exclude { get; }
	}
}

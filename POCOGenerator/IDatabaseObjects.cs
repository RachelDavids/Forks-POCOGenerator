namespace POCOGenerator
{
	/// <summary>The settings determine which database objects to generate classes out of and which database objects to exclude from generating classes.</summary>
	public interface IDatabaseObjects
	{
		/// <summary>Resets the database objects settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to generate classes out of all database objects.</summary>
		bool IncludeAll { get; set; }

		/// <summary>Gets the settings that determine which tables to generate classes out of and which tables to exclude from generating classes.</summary>
		ITables Tables { get; }

		/// <summary>Gets the settings that determine which views to generate classes out of and which views to exclude from generating classes.</summary>
		IViews Views { get; }

		/// <summary>Gets the settings that determine which stored procedures to generate classes out of and which stored procedures to exclude from generating classes.</summary>
		IStoredProcedures StoredProcedures { get; }

		/// <summary>Gets the settings that determine which functions to generate classes out of and which functions to exclude from generating classes.</summary>
		IFunctions Functions { get; }

		/// <summary>Gets the settings that determine which TVPs to generate classes out of and which TVPs to exclude from generating classes.</summary>
		ITVPs TVPs { get; }
	}
}

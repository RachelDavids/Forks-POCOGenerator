namespace POCOGenerator
{
	/// <summary>The settings for the class name transformations.</summary>
	public interface IClassName
	{
		/// <summary>Resets the class name settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether the class name is changed from plural to singular.
		/// <para>This setting is applicable only for tables, views &amp; TVPs.</para></summary>
		bool Singular { get; set; }

		/// <summary>Gets or sets a value indicating whether to add the database name to the class name.</summary>
		bool IncludeDB { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the separator to add after the database name.
		/// <para>This setting is applicable only when <see cref="IncludeDB" /> is set to <c>true</c>.
		/// The value must be not null and not an empty string to take effect.</para></summary>
		string DBSeparator { get; set; }

		/// <summary>Gets or sets a value indicating whether to add the schema to the class name.
		/// <para>This setting is applicable only when the RDBMS supports schema, such as SQLServer.</para></summary>
		bool IncludeSchema { get; set; }

		/// <summary>Gets or sets a value indicating whether to not add the schema to the class name when the schema is "dbo".</summary>
		bool IgnoreDboSchema { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the separator to add after the schema name.
		/// <para>This setting is applicable only when <see cref="IncludeSchema" /> is set to <c>true</c>.
		/// The value must be not null and not an empty string to take effect.</para></summary>
		string SchemaSeparator { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the separator to add between words in the class name. A word is defined as text between underscores or in a camel case.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string WordsSeparator { get; set; }

		/// <summary>Gets or sets a value indicating whether to change the class name to camel case.</summary>
		bool CamelCase { get; set; }

		/// <summary>Gets or sets a value indicating whether to change the class name to upper case.</summary>
		bool UpperCase { get; set; }

		/// <summary>Gets or sets a value indicating whether to change the class name to lower case.</summary>
		bool LowerCase { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the string to search in the class name as part of the search-and-replace.
		/// <para>The searching is case sensitive by default. The searching can be set to insensitive when <see cref="SearchIgnoreCase" /> is set to <c>true</c>.</para></summary>
		string Search { get; set; }

		/// <summary>Gets or sets a value indicating the string to replace with the search string in the class name as part of the search-and-replace.</summary>
		string Replace { get; set; }

		/// <summary>Gets or sets a value indicating whether the searching, as part of the search-and-replace, is case insensitive.</summary>
		bool SearchIgnoreCase { get; set; }

		/// <summary>Gets or sets a value indicating a fixed class name for all generated classes.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string FixedClassName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating a prefix string to be added to the start of the class name all generated classes.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string Prefix { get; set; }

		/// <summary>
		/// Gets or sets a value indicating a suffix string to be added to the end of the class name all generated classes.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string Suffix { get; set; }
	}
}

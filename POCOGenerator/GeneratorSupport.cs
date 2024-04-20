using POCOGenerator.DbObjects;

namespace POCOGenerator
{
	/// <summary>Provides information about the RDBMS and what capabilities it supports.</summary>
	public interface Support
	{
		/// <summary>Gets a value indicating whether the RDBMS supports schema.</summary>
		/// <value>
		///   <c>true</c> if the RDBMS supports schema; otherwise, <c>false</c>.</value>
		/// <seealso cref="DefaultSchema" />
		bool SupportSchema { get; }

		/// <summary>Gets a value indicating whether the RDBMS supports table functions.
		/// <para>Table function returns a table as result. Scalar function returns a single scalar value.</para></summary>
		/// <value>
		///   <c>true</c> if the RDBMS supports table functions; otherwise, <c>false</c>.</value>
		bool SupportTableFunctions { get; }

		/// <summary>Gets a value indicating whether the RDBMS supports user-defined table types.</summary>
		/// <value>
		///   <c>true</c> if the RDBMS supports user-defined table types; otherwise, <c>false</c>.</value>
		bool SupportTVPs { get; }

		/// <summary>Gets a value indicating whether the RDBMS supports enum &amp; set data types.</summary>
		/// <value>
		///   <c>true</c> if the RDBMS supports enum &amp; set data types; otherwise, <c>false</c>.</value>
		bool SupportEnumDataType { get; }

		/// <summary>Gets the default schema.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The default schema.</value>
		/// <seealso cref="SupportSchema" />
		string DefaultSchema { get; }

		/// <summary>Gets the version of the server.</summary>
		/// <value>The version of the server.</value>
		string Version { get; }
	}

	internal sealed class GeneratorSupport(IDbSupport support)
		: Support
	{
		public bool SupportSchema => support.IsSupportSchema;
		public bool SupportTableFunctions => support.IsSupportTableFunctions;
		public bool SupportTVPs => support.IsSupportTVPs;
		public bool SupportEnumDataType => support.IsSupportEnumDataType;
		public string DefaultSchema => support.DefaultSchema;
		public string Version => support.Version.ToString();
	}
}

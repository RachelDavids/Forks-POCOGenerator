namespace POCOGenerator
{
	/// <summary>The settings determine how the POCO will be generated.</summary>
	public interface IPOCO
	{
		/// <summary>Resets the POCO settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets a value indicating whether to generate class members as properties.
		/// <para>The default value is <c>true</c>.</para></summary>
		bool Properties { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate class members as fields.
		/// <para>The default value is <c>false</c>.</para></summary>
		bool Fields { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate class properties with <see langword="virtual" /> modifier.</summary>
		bool VirtualProperties { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate class properties with <see langword="override" /> modifier.</summary>
		bool OverrideProperties { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate classes with <see langword="partial" /> modifier.</summary>
		bool PartialClass { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate struct types as nullable when the SQL column is mapped to a .NET struct type and it is not nullable.</summary>
		bool StructTypesNullable { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate comments for data members with the SQL column's data type and whether the SQL column is nullable or not.</summary>
		bool Comments { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate comments for data members with the SQL column's data type. The column nullability will not be generated.</summary>
		bool CommentsWithoutNull { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate a using directive clause.</summary>
		bool Using { get; set; }

		/// <summary>Gets or sets a value indicating whether the using directive clause will be inside the namespace.</summary>
		bool UsingInsideNamespace { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate classes wrapped with a namespace.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string Namespace { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to wrap a namespace and a using directive clause around each class individually.
		/// <para>This setting is useful when generating to multiple outputs (such as multiple files).</para></summary>
		bool WrapAroundEachClass { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate classes with inheritance clause.
		/// <para>The value must be not null and not an empty string to take effect.</para></summary>
		string Inherit { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate initialize data members with default values.</summary>
		bool ColumnDefaults { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate a new line between class members.</summary>
		bool NewLineBetweenMembers { get; set; }

		/// <summary>Gets or sets a value indicating whether to generate complex types.</summary>
		bool ComplexTypes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate a string data member for a SQL enum &amp; set date types.
		/// <para>This setting is applicable when the RDBMS supports SQL enum &amp; set data types, such as MySQL. The default value is <c>true</c>.</para></summary>
		bool EnumSQLTypeToString { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate an enum of type ushort for a SQL enum date type and an enum of type ulong for a SQL set date type.
		/// <para>This setting is applicable when the RDBMS supports SQL enum &amp; set data types, such as MySQL. The default value is <c>false</c>.</para></summary>
		bool EnumSQLTypeToEnumUShort { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate an enum of type int for a SQL enum &amp; set date types.
		/// <para>This setting is applicable when the RDBMS supports SQL enum &amp; set data types, such as MySQL. The default value is <c>false</c>.</para></summary>
		bool EnumSQLTypeToEnumInt { get; set; }

		/// <summary>Gets or sets a value indicating the tab string. The default value is 4 spaces.</summary>
		string Tab { get; set; }
	}
}

namespace POCOGenerator.POCOIterators
{
	public interface IPOCOIteratorSettings
	{
		bool Properties { get; set; }
		bool Fields { get; set; }
		bool VirtualProperties { get; set; }
		bool OverrideProperties { get; set; }
		bool PartialClass { get; set; }
		bool StructTypesNullable { get; set; }
		bool Comments { get; set; }
		bool CommentsWithoutNull { get; set; }
		bool Using { get; set; }
		bool UsingInsideNamespace { get; set; }
		string Namespace { get; set; }
		bool WrapAroundEachClass { get; set; }
		string Inherit { get; set; }
		bool ColumnDefaults { get; set; }
		bool NewLineBetweenMembers { get; set; }
		bool ComplexTypes { get; set; }
		bool EnumSQLTypeToString { get; set; }
		bool EnumSQLTypeToEnumUShort { get; set; }
		bool EnumSQLTypeToEnumInt { get; set; }
		string Tab { get; set; }
	}
}
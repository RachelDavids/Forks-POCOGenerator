namespace POCOGenerator.POCOIterators
{
	public interface IClassNameIteratorSettings
	{
		bool Singular { get; set; }
		bool IncludeDB { get; set; }
		string DBSeparator { get; set; }
		bool IncludeSchema { get; set; }
		bool IgnoreDboSchema { get; set; }
		string SchemaSeparator { get; set; }
		string WordsSeparator { get; set; }
		bool CamelCase { get; set; }
		bool UpperCase { get; set; }
		bool LowerCase { get; set; }
		string Search { get; set; }
		string Replace { get; set; }
		bool SearchIgnoreCase { get; set; }
		string FixedClassName { get; set; }
		string Prefix { get; set; }
		string Suffix { get; set; }
	}
}
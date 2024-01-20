using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class ClassNameSettings
			: IClassName, IClassNameIteratorSettings
		{
			private readonly object _lockObject;

			internal ClassNameSettings(object lockObject)
			{
				_lockObject = lockObject;
			}

			public void Reset()
			{
				lock (_lockObject)
				{
					Singular = false;
					IncludeDB = false;
					DBSeparator = null;
					IncludeSchema = false;
					IgnoreDboSchema = false;
					SchemaSeparator = null;
					WordsSeparator = null;
					CamelCase = false;
					UpperCase = false;
					LowerCase = false;
					Search = null;
					Replace = null;
					SearchIgnoreCase = false;
					FixedClassName = null;
					Prefix = null;
					Suffix = null;
				}
			}

			private bool singular;

			public bool Singular
			{
				get
				{
					lock (_lockObject)
					{
						return singular;
					}
				}

				set
				{
					lock (_lockObject)
					{
						singular = value;
					}
				}
			}

			private bool includeDB;

			public bool IncludeDB
			{
				get
				{
					lock (_lockObject)
					{
						return includeDB;
					}
				}

				set
				{
					lock (_lockObject)
					{
						includeDB = value;
					}
				}
			}

			private string dbSeparator;

			public string DBSeparator
			{
				get
				{
					lock (_lockObject)
					{
						return dbSeparator;
					}
				}

				set
				{
					lock (_lockObject)
					{
						dbSeparator = value;
					}
				}
			}

			private bool includeSchema;

			public bool IncludeSchema
			{
				get
				{
					lock (_lockObject)
					{
						return includeSchema;
					}
				}

				set
				{
					lock (_lockObject)
					{
						includeSchema = value;
					}
				}
			}

			private bool ignoreDboSchema;

			public bool IgnoreDboSchema
			{
				get
				{
					lock (_lockObject)
					{
						return ignoreDboSchema;
					}
				}

				set
				{
					lock (_lockObject)
					{
						ignoreDboSchema = value;
					}
				}
			}

			private string schemaSeparator;

			public string SchemaSeparator
			{
				get
				{
					lock (_lockObject)
					{
						return schemaSeparator;
					}
				}

				set
				{
					lock (_lockObject)
					{
						schemaSeparator = value;
					}
				}
			}

			private string wordsSeparator;

			public string WordsSeparator
			{
				get
				{
					lock (_lockObject)
					{
						return wordsSeparator;
					}
				}

				set
				{
					lock (_lockObject)
					{
						wordsSeparator = value;
					}
				}
			}

			private bool camelCase;

			public bool CamelCase
			{
				get
				{
					lock (_lockObject)
					{
						return camelCase;
					}
				}

				set
				{
					lock (_lockObject)
					{
						camelCase = value;
					}
				}
			}

			private bool upperCase;

			public bool UpperCase
			{
				get
				{
					lock (_lockObject)
					{
						return upperCase;
					}
				}

				set
				{
					lock (_lockObject)
					{
						upperCase = value;
					}
				}
			}

			private bool lowerCase;

			public bool LowerCase
			{
				get
				{
					lock (_lockObject)
					{
						return lowerCase;
					}
				}

				set
				{
					lock (_lockObject)
					{
						lowerCase = value;
					}
				}
			}

			private string search;

			public string Search
			{
				get
				{
					lock (_lockObject)
					{
						return search;
					}
				}

				set
				{
					lock (_lockObject)
					{
						search = value;
					}
				}
			}

			private string replace;

			public string Replace
			{
				get
				{
					lock (_lockObject)
					{
						return replace;
					}
				}

				set
				{
					lock (_lockObject)
					{
						replace = value;
					}
				}
			}

			private bool searchIgnoreCase;

			public bool SearchIgnoreCase
			{
				get
				{
					lock (_lockObject)
					{
						return searchIgnoreCase;
					}
				}

				set
				{
					lock (_lockObject)
					{
						searchIgnoreCase = value;
					}
				}
			}

			private string fixedClassName;

			public string FixedClassName
			{
				get
				{
					lock (_lockObject)
					{
						return fixedClassName;
					}
				}

				set
				{
					lock (_lockObject)
					{
						fixedClassName = value;
					}
				}
			}

			private string prefix;

			public string Prefix
			{
				get
				{
					lock (_lockObject)
					{
						return prefix;
					}
				}

				set
				{
					lock (_lockObject)
					{
						prefix = value;
					}
				}
			}

			private string suffix;

			public string Suffix
			{
				get
				{
					lock (_lockObject)
					{
						return suffix;
					}
				}

				set
				{
					lock (_lockObject)
					{
						suffix = value;
					}
				}
			}
		}
	}
}

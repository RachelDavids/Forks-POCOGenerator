using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class POCOSettings
			: IPOCO, IPOCOIteratorSettings
		{
			private readonly object lockObject;

			internal POCOSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					Properties = true;
					Fields = false;
					VirtualProperties = false;
					OverrideProperties = false;
					PartialClass = false;
					StructTypesNullable = false;
					Comments = false;
					CommentsWithoutNull = false;
					Using = false;
					UsingInsideNamespace = false;
					Namespace = null;
					WrapAroundEachClass = false;
					Inherit = null;
					ColumnDefaults = false;
					NewLineBetweenMembers = false;
					ComplexTypes = false;
					EnumSQLTypeToString = true;
					EnumSQLTypeToEnumUShort = false;
					EnumSQLTypeToEnumInt = false;
					Tab = "    ";
				}
			}

			private bool properties = true;
			private bool fields;

			public bool Properties
			{
				get
				{
					lock (lockObject)
					{
						return properties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (properties != value)
						{
							if (value)
							{
								properties = true;
								fields = false;
							}
							else
							{
								properties = false;
								fields = true;
							}
						}
					}
				}
			}

			public bool Fields
			{
				get
				{
					lock (lockObject)
					{
						return fields;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (fields != value)
						{
							if (value)
							{
								properties = false;
								fields = true;
							}
							else
							{
								properties = true;
								fields = false;
							}
						}
					}
				}
			}

			private bool virtualProperties;
			private bool overrideProperties;

			public bool VirtualProperties
			{
				get
				{
					lock (lockObject)
					{
						return virtualProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (virtualProperties != value)
						{
							virtualProperties = value;
							if (virtualProperties && virtualProperties == overrideProperties)
							{
								overrideProperties = false;
							}
						}
					}
				}
			}

			public bool OverrideProperties
			{
				get
				{
					lock (lockObject)
					{
						return overrideProperties;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (overrideProperties != value)
						{
							overrideProperties = value;
							if (overrideProperties && virtualProperties == overrideProperties)
							{
								virtualProperties = false;
							}
						}
					}
				}
			}

			private bool partialClass;

			public bool PartialClass
			{
				get
				{
					lock (lockObject)
					{
						return partialClass;
					}
				}

				set
				{
					lock (lockObject)
					{
						partialClass = value;
					}
				}
			}

			private bool structTypesNullable;

			public bool StructTypesNullable
			{
				get
				{
					lock (lockObject)
					{
						return structTypesNullable;
					}
				}

				set
				{
					lock (lockObject)
					{
						structTypesNullable = value;
					}
				}
			}

			private bool comments;
			private bool commentsWithoutNull;

			public bool Comments
			{
				get
				{
					lock (lockObject)
					{
						return comments;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (comments != value)
						{
							comments = value;
							if (comments && comments == commentsWithoutNull)
							{
								commentsWithoutNull = false;
							}
						}
					}
				}
			}

			public bool CommentsWithoutNull
			{
				get
				{
					lock (lockObject)
					{
						return commentsWithoutNull;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (commentsWithoutNull != value)
						{
							commentsWithoutNull = value;
							if (commentsWithoutNull && comments == commentsWithoutNull)
							{
								comments = false;
							}
						}
					}
				}
			}

			private bool @using;

			public bool Using
			{
				get
				{
					lock (lockObject)
					{
						return @using;
					}
				}

				set
				{
					lock (lockObject)
					{
						@using = value;
					}
				}
			}

			private bool usingInsideNamespace;

			public bool UsingInsideNamespace
			{
				get
				{
					lock (lockObject)
					{
						return usingInsideNamespace;
					}
				}

				set
				{
					lock (lockObject)
					{
						usingInsideNamespace = value;
					}
				}
			}

			private string @namespace;

			public string Namespace
			{
				get
				{
					lock (lockObject)
					{
						return @namespace;
					}
				}

				set
				{
					lock (lockObject)
					{
						@namespace = value;
					}
				}
			}

			private bool wrapAroundEachClass;

			public bool WrapAroundEachClass
			{
				get
				{
					lock (lockObject)
					{
						return wrapAroundEachClass;
					}
				}

				set
				{
					lock (lockObject)
					{
						wrapAroundEachClass = value;
					}
				}
			}

			private string inherit;

			public string Inherit
			{
				get
				{
					lock (lockObject)
					{
						return inherit;
					}
				}

				set
				{
					lock (lockObject)
					{
						inherit = value;
					}
				}
			}

			private bool columnDefaults;

			public bool ColumnDefaults
			{
				get
				{
					lock (lockObject)
					{
						return columnDefaults;
					}
				}

				set
				{
					lock (lockObject)
					{
						columnDefaults = value;
					}
				}
			}

			private bool newLineBetweenMembers;

			public bool NewLineBetweenMembers
			{
				get
				{
					lock (lockObject)
					{
						return newLineBetweenMembers;
					}
				}

				set
				{
					lock (lockObject)
					{
						newLineBetweenMembers = value;
					}
				}
			}

			private bool complexTypes;

			public bool ComplexTypes
			{
				get
				{
					lock (lockObject)
					{
						return complexTypes;
					}
				}

				set
				{
					lock (lockObject)
					{
						complexTypes = value;
					}
				}
			}

			private bool enumSQLTypeToString = true;
			private bool enumSQLTypeToEnumUShort;
			private bool enumSQLTypeToEnumInt;

			public bool EnumSQLTypeToString
			{
				get
				{
					lock (lockObject)
					{
						return enumSQLTypeToString;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (enumSQLTypeToString != value)
						{
							if (value)
							{
								enumSQLTypeToString = true;
								enumSQLTypeToEnumUShort = false;
								enumSQLTypeToEnumInt = false;
							}
							else
							{
								enumSQLTypeToString = false;
								enumSQLTypeToEnumUShort = true;
								enumSQLTypeToEnumInt = false;
							}
						}
					}
				}
			}

			public bool EnumSQLTypeToEnumUShort
			{
				get
				{
					lock (lockObject)
					{
						return enumSQLTypeToEnumUShort;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (enumSQLTypeToEnumUShort != value)
						{
							if (value)
							{
								enumSQLTypeToString = false;
								enumSQLTypeToEnumUShort = true;
								enumSQLTypeToEnumInt = false;
							}
							else
							{
								enumSQLTypeToString = true;
								enumSQLTypeToEnumUShort = false;
								enumSQLTypeToEnumInt = false;
							}
						}
					}
				}
			}

			public bool EnumSQLTypeToEnumInt
			{
				get
				{
					lock (lockObject)
					{
						return enumSQLTypeToEnumInt;
					}
				}

				set
				{
					lock (lockObject)
					{
						if (enumSQLTypeToEnumInt != value)
						{
							if (value)
							{
								enumSQLTypeToString = false;
								enumSQLTypeToEnumUShort = false;
								enumSQLTypeToEnumInt = true;
							}
							else
							{
								enumSQLTypeToString = true;
								enumSQLTypeToEnumUShort = false;
								enumSQLTypeToEnumInt = false;
							}
						}
					}
				}
			}

			private string tab = "    ";

			public string Tab
			{
				get
				{
					lock (lockObject)
					{
						return tab;
					}
				}

				set
				{
					lock (lockObject)
					{
						tab = value;
					}
				}
			}
		}

		#region Constructors

		private void InitializePOCO(IPOCO pocoSettings)
		{
			POCO.Properties = pocoSettings.Properties;
			POCO.Fields = pocoSettings.Fields;
			POCO.VirtualProperties = pocoSettings.VirtualProperties;
			POCO.OverrideProperties = pocoSettings.OverrideProperties;
			POCO.PartialClass = pocoSettings.PartialClass;
			POCO.StructTypesNullable = pocoSettings.StructTypesNullable;
			POCO.Comments = pocoSettings.Comments;
			POCO.CommentsWithoutNull = pocoSettings.CommentsWithoutNull;
			POCO.Using = pocoSettings.Using;
			POCO.UsingInsideNamespace = pocoSettings.UsingInsideNamespace;
			POCO.Namespace = pocoSettings.Namespace;
			POCO.WrapAroundEachClass = pocoSettings.WrapAroundEachClass;
			POCO.Inherit = pocoSettings.Inherit;
			POCO.ColumnDefaults = pocoSettings.ColumnDefaults;
			POCO.NewLineBetweenMembers = pocoSettings.NewLineBetweenMembers;
			POCO.ComplexTypes = pocoSettings.ComplexTypes;
			POCO.EnumSQLTypeToString = pocoSettings.EnumSQLTypeToString;
			POCO.EnumSQLTypeToEnumUShort = pocoSettings.EnumSQLTypeToEnumUShort;
			POCO.EnumSQLTypeToEnumInt = pocoSettings.EnumSQLTypeToEnumInt;
			POCO.Tab = pocoSettings.Tab;
		}

		#endregion
	}
}

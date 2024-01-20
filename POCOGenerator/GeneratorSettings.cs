using System;
using System.Drawing;
using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
		: ISettings, IDbIteratorSettings, ICloneable
	{
		#region Constructors

		private readonly object _lockObject;

		internal GeneratorSettings(object lockObject)
		{
			_lockObject = lockObject;

			Connection = new ConnectionSettings(lockObject);
			POCO = new POCOSettings(lockObject);
			ClassName = new ClassNameSettings(lockObject);
			NavigationProperties = new NavigationPropertiesSettings(lockObject);
			EFAnnotations = new EFAnnotationsSettings(lockObject);
			DatabaseObjects = new DatabaseObjectsSettings(lockObject);
			SyntaxHighlight = new SyntaxHighlightSettings(lockObject);
		}

		private GeneratorSettings(ISettings settings)
			: this(new object())
		{
			InitializeConnection(settings.Connection);
			InitializePOCO(settings);
			InitializeClassName(settings);
			InitializeNavigationProperties(settings);
			InitializeEFAnnotations(settings);
			InitializeDatabaseObjects(settings.DatabaseObjects);
			InitializeSyntaxHighlight(settings.SyntaxHighlight);
		}

		private void InitializeSyntaxHighlight(ISyntaxHighlight syntaxHighlight)
		{
			SyntaxHighlight.Text = syntaxHighlight.Text;
			SyntaxHighlight.Keyword = syntaxHighlight.Keyword;
			SyntaxHighlight.UserType = syntaxHighlight.UserType;
			SyntaxHighlight.String = syntaxHighlight.String;
			SyntaxHighlight.Comment = syntaxHighlight.Comment;
			SyntaxHighlight.Error = syntaxHighlight.Error;
			SyntaxHighlight.Background = syntaxHighlight.Background;
		}

		private void InitializeDatabaseObjects(IDatabaseObjects settingsDatabaseObjects)
		{
			#region Database Objects

			DatabaseObjects.IncludeAll = settingsDatabaseObjects.IncludeAll;

			#endregion

			#region Tables

			ITables tables = DatabaseObjects.Tables;
			ITables sourceTables = settingsDatabaseObjects.Tables;
			tables.IncludeAll = sourceTables.IncludeAll;
			tables.ExcludeAll = sourceTables.ExcludeAll;

			foreach (string item in sourceTables.Include)
			{
				tables.Include.Add(item);
			}

			foreach (string item in sourceTables.Exclude)
			{
				tables.Exclude.Add(item);
			}

			#endregion

			#region Views

			DatabaseObjects.Views.IncludeAll = settingsDatabaseObjects.Views.IncludeAll;
			DatabaseObjects.Views.ExcludeAll = settingsDatabaseObjects.Views.ExcludeAll;

			foreach (string item in settingsDatabaseObjects.Views.Include)
			{
				DatabaseObjects.Views.Include.Add(item);
			}

			foreach (string item in settingsDatabaseObjects.Views.Exclude)
			{
				DatabaseObjects.Views.Exclude.Add(item);
			}

			#endregion

			#region Stored Procedures

			DatabaseObjects.StoredProcedures.IncludeAll = settingsDatabaseObjects.StoredProcedures.IncludeAll;
			DatabaseObjects.StoredProcedures.ExcludeAll = settingsDatabaseObjects.StoredProcedures.ExcludeAll;

			foreach (string item in settingsDatabaseObjects.StoredProcedures.Include)
			{
				DatabaseObjects.StoredProcedures.Include.Add(item);
			}

			foreach (string item in settingsDatabaseObjects.StoredProcedures.Exclude)
			{
				DatabaseObjects.StoredProcedures.Exclude.Add(item);
			}

			#endregion

			#region Functions

			DatabaseObjects.Functions.IncludeAll = settingsDatabaseObjects.Functions.IncludeAll;
			DatabaseObjects.Functions.ExcludeAll = settingsDatabaseObjects.Functions.ExcludeAll;

			foreach (string item in settingsDatabaseObjects.Functions.Include)
			{
				DatabaseObjects.Functions.Include.Add(item);
			}

			foreach (string item in settingsDatabaseObjects.Functions.Exclude)
			{
				DatabaseObjects.Functions.Exclude.Add(item);
			}

			#endregion

			#region TVPs

			DatabaseObjects.TVPs.IncludeAll = settingsDatabaseObjects.TVPs.IncludeAll;
			DatabaseObjects.TVPs.ExcludeAll = settingsDatabaseObjects.TVPs.ExcludeAll;

			foreach (string item in settingsDatabaseObjects.TVPs.Include)
			{
				DatabaseObjects.TVPs.Include.Add(item);
			}

			foreach (string item in settingsDatabaseObjects.TVPs.Exclude)
			{
				DatabaseObjects.TVPs.Exclude.Add(item);
			}

			#endregion
		}

		private void InitializeEFAnnotations(ISettings settings)
		{
			#region EF Annotations

			EFAnnotations.Enable = settings.EFAnnotations.Enable;
			EFAnnotations.Column = settings.EFAnnotations.Column;
			EFAnnotations.Required = settings.EFAnnotations.Required;
			EFAnnotations.RequiredWithErrorMessage = settings.EFAnnotations.RequiredWithErrorMessage;
			EFAnnotations.ConcurrencyCheck = settings.EFAnnotations.ConcurrencyCheck;
			EFAnnotations.StringLength = settings.EFAnnotations.StringLength;
			EFAnnotations.Display = settings.EFAnnotations.Display;
			EFAnnotations.Description = settings.EFAnnotations.Description;
			EFAnnotations.ComplexType = settings.EFAnnotations.ComplexType;
			EFAnnotations.Index = settings.EFAnnotations.Index;
			EFAnnotations.ForeignKeyAndInverseProperty = settings.EFAnnotations.ForeignKeyAndInverseProperty;

			#endregion
		}

		private void InitializeNavigationProperties(ISettings settings)
		{
			#region Navigation Properties

			NavigationProperties.Enable = settings.NavigationProperties.Enable;
			NavigationProperties.VirtualNavigationProperties = settings.NavigationProperties.VirtualNavigationProperties;
			NavigationProperties.OverrideNavigationProperties = settings.NavigationProperties.OverrideNavigationProperties;
			NavigationProperties.ManyToManyJoinTable = settings.NavigationProperties.ManyToManyJoinTable;
			NavigationProperties.Comments = settings.NavigationProperties.Comments;
			NavigationProperties.ListNavigationProperties = settings.NavigationProperties.ListNavigationProperties;
			NavigationProperties.IListNavigationProperties = settings.NavigationProperties.IListNavigationProperties;
			NavigationProperties.ICollectionNavigationProperties = settings.NavigationProperties.ICollectionNavigationProperties;
			NavigationProperties.IEnumerableNavigationProperties = settings.NavigationProperties.IEnumerableNavigationProperties;

			#endregion
		}

		private void InitializeClassName(ISettings settings)
		{
			#region Class Name

			ClassName.Singular = settings.ClassName.Singular;
			ClassName.IncludeDB = settings.ClassName.IncludeDB;
			ClassName.DBSeparator = settings.ClassName.DBSeparator;
			ClassName.IncludeSchema = settings.ClassName.IncludeSchema;
			ClassName.IgnoreDboSchema = settings.ClassName.IgnoreDboSchema;
			ClassName.SchemaSeparator = settings.ClassName.SchemaSeparator;
			ClassName.WordsSeparator = settings.ClassName.WordsSeparator;
			ClassName.CamelCase = settings.ClassName.CamelCase;
			ClassName.UpperCase = settings.ClassName.UpperCase;
			ClassName.LowerCase = settings.ClassName.LowerCase;
			ClassName.Search = settings.ClassName.Search;
			ClassName.Replace = settings.ClassName.Replace;
			ClassName.SearchIgnoreCase = settings.ClassName.SearchIgnoreCase;
			ClassName.FixedClassName = settings.ClassName.FixedClassName;
			ClassName.Prefix = settings.ClassName.Prefix;
			ClassName.Suffix = settings.ClassName.Suffix;

			#endregion
		}

		private void InitializePOCO(ISettings settings)
		{
			#region POCO

			POCO.Properties = settings.POCO.Properties;
			POCO.Fields = settings.POCO.Fields;
			POCO.VirtualProperties = settings.POCO.VirtualProperties;
			POCO.OverrideProperties = settings.POCO.OverrideProperties;
			POCO.PartialClass = settings.POCO.PartialClass;
			POCO.StructTypesNullable = settings.POCO.StructTypesNullable;
			POCO.Comments = settings.POCO.Comments;
			POCO.CommentsWithoutNull = settings.POCO.CommentsWithoutNull;
			POCO.Using = settings.POCO.Using;
			POCO.UsingInsideNamespace = settings.POCO.UsingInsideNamespace;
			POCO.Namespace = settings.POCO.Namespace;
			POCO.WrapAroundEachClass = settings.POCO.WrapAroundEachClass;
			POCO.Inherit = settings.POCO.Inherit;
			POCO.ColumnDefaults = settings.POCO.ColumnDefaults;
			POCO.NewLineBetweenMembers = settings.POCO.NewLineBetweenMembers;
			POCO.ComplexTypes = settings.POCO.ComplexTypes;
			POCO.EnumSQLTypeToString = settings.POCO.EnumSQLTypeToString;
			POCO.EnumSQLTypeToEnumUShort = settings.POCO.EnumSQLTypeToEnumUShort;
			POCO.EnumSQLTypeToEnumInt = settings.POCO.EnumSQLTypeToEnumInt;
			POCO.Tab = settings.POCO.Tab;

			#endregion
		}

		private void InitializeConnection(IConnection connection)
		{
			Connection.ConnectionString = connection.ConnectionString;
			Connection.RDBMS = connection.RDBMS;
		}

		#endregion

		#region ICloneable

		public object Clone() => new GeneratorSettings(this);

		#endregion

		#region Reset

		public void Reset()
		{
			lock (_lockObject)
			{
				Connection.Reset();
				POCO.Reset();
				ClassName.Reset();
				NavigationProperties.Reset();
				EFAnnotations.Reset();
				DatabaseObjects.Reset();
				SyntaxHighlight.Reset();
			}
		}

		#endregion

		#region Settings

		public IConnection Connection { get; private set; }
		public IPOCO POCO { get; private set; }
		public IClassName ClassName { get; private set; }
		public INavigationProperties NavigationProperties { get; private set; }
		public IEFAnnotations EFAnnotations { get; private set; }
		public IDatabaseObjects DatabaseObjects { get; private set; }
		public ISyntaxHighlight SyntaxHighlight { get; private set; }

		#endregion

		#region Tables Settings

		private sealed class TablesSettings : DbObjectsSettingsBase, ITables
		{
			internal TablesSettings(object lockObject)
				: base(lockObject)
			{
			}
		}

		#endregion

		#region Views Settings

		private sealed class ViewsSettings : DbObjectsSettingsBase, IViews
		{
			internal ViewsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}

		#endregion

		#region Stored Procedures Settings

		private sealed class StoredProceduresSettings : DbObjectsSettingsBase, IStoredProcedures
		{
			internal StoredProceduresSettings(object lockObject)
				: base(lockObject)
			{
			}
		}

		#endregion

		#region Functions Settings

		private sealed class FunctionsSettings : DbObjectsSettingsBase, IFunctions
		{
			internal FunctionsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}

		#endregion

		#region TVPs Settings

		private sealed class TVPsSettings : DbObjectsSettingsBase, ITVPs
		{
			internal TVPsSettings(object lockObject)
				: base(lockObject)
			{
			}
		}

		#endregion

		#region Syntax Highlight Settings

		private sealed class SyntaxHighlightSettings : ISyntaxHighlight
		{
			private readonly object lockObject;

			internal SyntaxHighlightSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					Text = Color.FromArgb(0, 0, 0);
					Keyword = Color.FromArgb(0, 0, 255);
					UserType = Color.FromArgb(43, 145, 175);
					String = Color.FromArgb(163, 21, 21);
					Comment = Color.FromArgb(0, 128, 0);
					Error = Color.FromArgb(255, 0, 0);
					Background = Color.FromArgb(255, 255, 255);
				}
			}

			private Color text = Color.FromArgb(0, 0, 0);
			public Color Text
			{
				get
				{
					lock (lockObject)
					{
						return text;
					}
				}

				set
				{
					lock (lockObject)
					{
						text = value;
					}
				}
			}

			private Color keyword = Color.FromArgb(0, 0, 255);
			public Color Keyword
			{
				get
				{
					lock (lockObject)
					{
						return keyword;
					}
				}

				set
				{
					lock (lockObject)
					{
						keyword = value;
					}
				}
			}

			private Color userType = Color.FromArgb(43, 145, 175);
			public Color UserType
			{
				get
				{
					lock (lockObject)
					{
						return userType;
					}
				}

				set
				{
					lock (lockObject)
					{
						userType = value;
					}
				}
			}

			private Color @string = Color.FromArgb(163, 21, 21);
			public Color String
			{
				get
				{
					lock (lockObject)
					{
						return @string;
					}
				}

				set
				{
					lock (lockObject)
					{
						@string = value;
					}
				}
			}

			private Color comment = Color.FromArgb(0, 128, 0);
			public Color Comment
			{
				get
				{
					lock (lockObject)
					{
						return comment;
					}
				}

				set
				{
					lock (lockObject)
					{
						comment = value;
					}
				}
			}

			private Color error = Color.FromArgb(255, 0, 0);
			public Color Error
			{
				get
				{
					lock (lockObject)
					{
						return error;
					}
				}

				set
				{
					lock (lockObject)
					{
						error = value;
					}
				}
			}

			private Color background = Color.FromArgb(255, 255, 255);
			public Color Background
			{
				get
				{
					lock (lockObject)
					{
						return background;
					}
				}

				set
				{
					lock (lockObject)
					{
						background = value;
					}
				}
			}
		}

		#endregion

		#region Db Iterator Settings

		public IPOCOIteratorSettings POCOIteratorSettings => (POCOSettings)POCO;

		public IClassNameIteratorSettings ClassNameIteratorSettings => (ClassNameSettings)ClassName;

		public INavigationPropertiesIteratorSettings NavigationPropertiesIteratorSettings => (NavigationPropertiesSettings)NavigationProperties;

		public IEFAnnotationsIteratorSettings EFAnnotationsIteratorSettings => (EFAnnotationsSettings)EFAnnotations;

		#endregion
	}
}

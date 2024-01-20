using System;
using POCOGenerator.POCOIterators;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
		: ISettings, IDbIteratorSettings, ICloneable
	{
		private readonly object _lockObject;

		internal GeneratorSettings(object lockObject)
		{
			_lockObject = lockObject ?? new();

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
			InitializePOCO(settings.POCO);
			InitializeClassName(settings.ClassName);
			InitializeNavigationProperties(settings.NavigationProperties);
			InitializeEFAnnotations(settings.EFAnnotations);
			InitializeDatabaseObjects(settings.DatabaseObjects);
			InitializeSyntaxHighlight(settings.SyntaxHighlight);
		}

		public object Clone() => new GeneratorSettings(this);

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

		public IConnection Connection { get; }
		public IPOCO POCO { get; }
		public IClassName ClassName { get; }
		public INavigationProperties NavigationProperties { get; }
		public IEFAnnotations EFAnnotations { get; }
		public IDatabaseObjects DatabaseObjects { get; }
		public ISyntaxHighlight SyntaxHighlight { get; }

		public IPOCOIteratorSettings POCOIteratorSettings => (POCOSettings)POCO;
		public IClassNameIteratorSettings ClassNameIteratorSettings => (ClassNameSettings)ClassName;
		public INavigationPropertiesIteratorSettings NavigationPropertiesIteratorSettings => (NavigationPropertiesSettings)NavigationProperties;
		public IEFAnnotationsIteratorSettings EFAnnotationsIteratorSettings => (EFAnnotationsSettings)EFAnnotations;
	}
}

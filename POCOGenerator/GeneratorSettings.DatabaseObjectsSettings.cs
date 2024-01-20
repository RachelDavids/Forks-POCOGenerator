namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private sealed class DatabaseObjectsSettings
			: IDatabaseObjects
		{
			private readonly object lockObject;

			internal DatabaseObjectsSettings(object lockObject)
			{
				this.lockObject = lockObject;
				Tables = new TablesSettings(lockObject);
				Views = new ViewsSettings(lockObject);
				StoredProcedures = new StoredProceduresSettings(lockObject);
				Functions = new FunctionsSettings(lockObject);
				TVPs = new TVPsSettings(lockObject);
			}

			public void Reset()
			{
				lock (lockObject)
				{
					IncludeAll = false;
					Tables.Reset();
					Views.Reset();
					StoredProcedures.Reset();
					Functions.Reset();
					TVPs.Reset();
				}
			}

			private bool includeAll;

			public bool IncludeAll
			{
				get
				{
					lock (lockObject)
					{
						return includeAll;
					}
				}

				set
				{
					lock (lockObject)
					{
						includeAll = value;
					}
				}
			}

			public ITables Tables { get; private set; }
			public IViews Views { get; private set; }
			public IStoredProcedures StoredProcedures { get; private set; }
			public IFunctions Functions { get; private set; }
			public ITVPs TVPs { get; private set; }
		}
	}
}

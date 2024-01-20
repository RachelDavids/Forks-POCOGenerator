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

		#region Constructors

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

		#endregion
	}
}

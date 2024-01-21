using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using POCOGenerator.DbFactories;
using POCOGenerator.DbHandlers;
using POCOGenerator.DbObjects;
using POCOGenerator.POCOIterators;
using POCOGenerator.POCOWriters;
using POCOGenerator.Utils;

namespace POCOGenerator
{
	internal class Generator : IGenerator
	{
		internal readonly object lockObject = new();

		internal Generator(Func<IWriter> createWriter)
		{
			this.createWriter = createWriter;
			settings = new GeneratorSettings(lockObject);
		}

		#region Settings

		private readonly GeneratorSettings settings;
		private GeneratorSettings settingsInternal;

		public ISettings Settings
		{
			get
			{
				lock (lockObject)
				{
					return settings;
				}
			}
		}

		#endregion

		private bool isServerBuilt;
		private IDbHandler dbHandler;
		private IDbHelper dbHelper;
		internal Func<IWriter> createWriter;
		private Func<IWriter> createWriterInternal;
		private POCOGenerator.Objects.Server proxyServer;
		private List<IDbObjectTraverse> dbObjectsToGenerate;

		public Support Support { get; private set; }

		public Exception Error { get; private set; }

		public GeneratorResults Generate()
		{
			try
			{
				// clear the generator error
				Error = null;

				// copy the settings and event to internal counterparts
				SetInternalSettingsAndEvents();

				// clear data members
				isServerBuilt = false;
				dbHandler = null;
				dbHelper = null;
				Support = null;
				proxyServer = null;
				dbObjectsToGenerate = null;

				// validate connection string
				string serverName = null;
				string instanceName = null;
				string userId = null;
				string initialDatabase = null;
				GeneratorResults results = ValidateConnectionString(ref dbHandler, ref serverName, ref instanceName, ref userId, ref initialDatabase);
				if (results != GeneratorResults.None)
				{
					return results;
				}

				// create new server
				IServer server = dbHandler.GetServer();
				server.ServerName = serverName;
				server.InstanceName = instanceName;
				server.UserId = userId;

				// determine which object types to generate and which ones to skip, based on the user's settings
				bool isEnableTables = IsEnableDbObjects(settingsInternal.DatabaseObjects.IncludeAll, settingsInternal.DatabaseObjects.Tables.IncludeAll, settingsInternal.DatabaseObjects.Tables.ExcludeAll, settingsInternal.DatabaseObjects.Tables.Include);
				bool isEnableViews = IsEnableDbObjects(settingsInternal.DatabaseObjects.IncludeAll, settingsInternal.DatabaseObjects.Views.IncludeAll, settingsInternal.DatabaseObjects.Views.ExcludeAll, settingsInternal.DatabaseObjects.Views.Include);
				bool isEnableStoredProcedures = IsEnableDbObjects(settingsInternal.DatabaseObjects.IncludeAll, settingsInternal.DatabaseObjects.StoredProcedures.IncludeAll, settingsInternal.DatabaseObjects.StoredProcedures.ExcludeAll, settingsInternal.DatabaseObjects.StoredProcedures.Include);
				bool isEnableFunctions = IsEnableDbObjects(settingsInternal.DatabaseObjects.IncludeAll, settingsInternal.DatabaseObjects.Functions.IncludeAll, settingsInternal.DatabaseObjects.Functions.ExcludeAll, settingsInternal.DatabaseObjects.Functions.Include);
				bool isEnableTVPs = IsEnableDbObjects(settingsInternal.DatabaseObjects.IncludeAll, settingsInternal.DatabaseObjects.TVPs.IncludeAll, settingsInternal.DatabaseObjects.TVPs.ExcludeAll, settingsInternal.DatabaseObjects.TVPs.Include);

				// get db helper
				dbHelper = dbHandler.GetDbHelper(settingsInternal.Connection.ConnectionString);

				// create generator support
				Support = new GeneratorSupport(dbHelper.Support);

				// disable function and tvp rendering, if the db doesn't support it
				isEnableFunctions &= dbHelper.Support.IsSupportTableFunctions;
				isEnableTVPs &= dbHelper.Support.IsSupportTVPs;

				// build server schema
				dbHelper.BuildServerSchema(
					server,
					initialDatabase,
					isEnableTables,
					isEnableViews,
					isEnableStoredProcedures,
					isEnableFunctions,
					isEnableTVPs
				);

				// get all the accessible objects, including the ones that are not listed in the user's settings
				// tables that are accessible through foreign keys or complex types
				List<DatabaseAccessibleObjects> databasesAccessibleObjects = GetDatabasesAccessibleObjects(server);

				// set the db objects to generate
				SetDbObjectsToGenerate(databasesAccessibleObjects);

				// create proxy server
				proxyServer = new POCOGenerator.Objects.Server(server, databasesAccessibleObjects);

				// the server is built
				isServerBuilt = true;

				// fire server built async event
				serverBuiltAsyncInternal.RaiseAsync(this, () => new ServerBuiltAsyncEventArgs(proxyServer));

				// fire server built sync event
				ServerBuiltEventArgs args = serverBuiltInternal.Raise(this, () => new ServerBuiltEventArgs(proxyServer));

				// stop the generator if the event args Stop was set to true
				if (args != null && args.Stop)
				{
					return GeneratorResults.None;
				}

				// generate classes
				return IterateDbObjects();
			}
			catch (Exception ex)
			{
				Error = ex;
				return GeneratorResults.UnexpectedError;
			}
			finally
			{
				ClearInternalSettingsAndEvents();
			}
		}

		public GeneratorResults GeneratePOCOs()
		{
			if (isServerBuilt)
			{
				try
				{
					// clear the generator error
					Error = null;

					// copy the settings and event to internal counterparts
					SetInternalSettingsAndEvents();

					// generate classes
					return IterateDbObjects();
				}
				catch (Exception ex)
				{
					Error = ex;
					return GeneratorResults.UnexpectedError;
				}
				finally
				{
					ClearInternalSettingsAndEvents();
				}
			}
			else
			{
				return Generate();
			}
		}

		private GeneratorResults IterateDbObjects()
		{
			// create writer
			IWriter writer = createWriterInternal();

			// set the writer syntax colors
			if (writer is POCOWriters.ISyntaxHighlight syntaxHighlight)
			{
				syntaxHighlight.Text = settingsInternal.SyntaxHighlight.Text;
				syntaxHighlight.Keyword = settingsInternal.SyntaxHighlight.Keyword;
				syntaxHighlight.UserType = settingsInternal.SyntaxHighlight.UserType;
				syntaxHighlight.String = settingsInternal.SyntaxHighlight.String;
				syntaxHighlight.Comment = settingsInternal.SyntaxHighlight.Comment;
				syntaxHighlight.Error = settingsInternal.SyntaxHighlight.Error;
				syntaxHighlight.Background = settingsInternal.SyntaxHighlight.Background;
			}

			// create iterator
			IDbIterator iterator = dbHandler.GetIterator(writer, dbHelper.Support, settingsInternal);

			// set iterator events
			SetPOCOIteratorEvents(iterator, proxyServer);

			// generate classes
			iterator.Iterate(dbObjectsToGenerate);

			return dbObjectsToGenerate.IsNullOrEmpty() ? GeneratorResults.NoDbObjectsIncluded : GeneratorResults.None;
		}

		private void SetInternalSettingsAndEvents()
		{
			lock (lockObject)
			{
				settingsInternal = (GeneratorSettings)settings.Clone();

				createWriterInternal = createWriter;

				serverBuiltAsyncInternal = serverBuiltAsync;
				serverBuiltInternal = serverBuilt;
				serverGeneratingAsyncInternal = serverGeneratingAsync;
				serverGeneratingInternal = serverGenerating;
				databaseGeneratingAsyncInternal = databaseGeneratingAsync;
				databaseGeneratingInternal = databaseGenerating;
				tablesGeneratingAsyncInternal = tablesGeneratingAsync;
				tablesGeneratingInternal = tablesGenerating;
				tableGeneratingAsyncInternal = tableGeneratingAsync;
				tableGeneratingInternal = tableGenerating;
				tablePOCOAsyncInternal = tablePOCOAsync;
				tablePOCOInternal = tablePOCO;
				tableGeneratedAsyncInternal = tableGeneratedAsync;
				tableGeneratedInternal = tableGenerated;
				tablesGeneratedAsyncInternal = tablesGeneratedAsync;
				tablesGeneratedInternal = tablesGenerated;
				complexTypeTablesGeneratingAsyncInternal = complexTypeTablesGeneratingAsync;
				complexTypeTablesGeneratingInternal = complexTypeTablesGenerating;
				complexTypeTableGeneratingAsyncInternal = complexTypeTableGeneratingAsync;
				complexTypeTableGeneratingInternal = complexTypeTableGenerating;
				complexTypeTablePOCOAsyncInternal = complexTypeTablePOCOAsync;
				complexTypeTablePOCOInternal = complexTypeTablePOCO;
				complexTypeTableGeneratedAsyncInternal = complexTypeTableGeneratedAsync;
				complexTypeTableGeneratedInternal = complexTypeTableGenerated;
				complexTypeTablesGeneratedAsyncInternal = complexTypeTablesGeneratedAsync;
				complexTypeTablesGeneratedInternal = complexTypeTablesGenerated;
				viewsGeneratingAsyncInternal = viewsGeneratingAsync;
				viewsGeneratingInternal = viewsGenerating;
				viewGeneratingAsyncInternal = viewGeneratingAsync;
				viewGeneratingInternal = viewGenerating;
				viewPOCOAsyncInternal = viewPOCOAsync;
				viewPOCOInternal = viewPOCO;
				viewGeneratedAsyncInternal = viewGeneratedAsync;
				viewGeneratedInternal = viewGenerated;
				viewsGeneratedAsyncInternal = viewsGeneratedAsync;
				viewsGeneratedInternal = viewsGenerated;
				proceduresGeneratingAsyncInternal = proceduresGeneratingAsync;
				proceduresGeneratingInternal = proceduresGenerating;
				procedureGeneratingAsyncInternal = procedureGeneratingAsync;
				procedureGeneratingInternal = procedureGenerating;
				procedurePOCOAsyncInternal = procedurePOCOAsync;
				procedurePOCOInternal = procedurePOCO;
				procedureGeneratedAsyncInternal = procedureGeneratedAsync;
				procedureGeneratedInternal = procedureGenerated;
				proceduresGeneratedAsyncInternal = proceduresGeneratedAsync;
				proceduresGeneratedInternal = proceduresGenerated;
				functionsGeneratingAsyncInternal = functionsGeneratingAsync;
				functionsGeneratingInternal = functionsGenerating;
				functionGeneratingAsyncInternal = functionGeneratingAsync;
				functionGeneratingInternal = functionGenerating;
				functionPOCOAsyncInternal = functionPOCOAsync;
				functionPOCOInternal = functionPOCO;
				functionGeneratedAsyncInternal = functionGeneratedAsync;
				functionGeneratedInternal = functionGenerated;
				functionsGeneratedAsyncInternal = functionsGeneratedAsync;
				functionsGeneratedInternal = functionsGenerated;
				tvpsGeneratingAsyncInternal = tvpsGeneratingAsync;
				tvpsGeneratingInternal = tvpsGenerating;
				tvpGeneratingAsyncInternal = tvpGeneratingAsync;
				tvpGeneratingInternal = tvpGenerating;
				tvpPOCOAsyncInternal = tvpPOCOAsync;
				tvpPOCOInternal = tvpPOCO;
				tvpGeneratedAsyncInternal = tvpGeneratedAsync;
				tvpGeneratedInternal = tvpGenerated;
				tvpsGeneratedAsyncInternal = tvpsGeneratedAsync;
				tvpsGeneratedInternal = tvpsGenerated;
				databaseGeneratedAsyncInternal = databaseGeneratedAsync;
				databaseGeneratedInternal = databaseGenerated;
				serverGeneratedAsyncInternal = serverGeneratedAsync;
				serverGeneratedInternal = serverGenerated;
			}
		}

		private void ClearInternalSettingsAndEvents()
		{
			settingsInternal = null;

			createWriterInternal = null;

			serverBuiltAsyncInternal = null;
			serverBuiltInternal = null;
			serverGeneratingAsyncInternal = null;
			serverGeneratingInternal = null;
			databaseGeneratingAsyncInternal = null;
			databaseGeneratingInternal = null;
			tablesGeneratingAsyncInternal = null;
			tablesGeneratingInternal = null;
			tableGeneratingAsyncInternal = null;
			tableGeneratingInternal = null;
			tablePOCOAsyncInternal = null;
			tablePOCOInternal = null;
			tableGeneratedAsyncInternal = null;
			tableGeneratedInternal = null;
			tablesGeneratedAsyncInternal = null;
			tablesGeneratedInternal = null;
			complexTypeTablesGeneratingAsyncInternal = null;
			complexTypeTablesGeneratingInternal = null;
			complexTypeTableGeneratingAsyncInternal = null;
			complexTypeTableGeneratingInternal = null;
			complexTypeTablePOCOAsyncInternal = null;
			complexTypeTablePOCOInternal = null;
			complexTypeTableGeneratedAsyncInternal = null;
			complexTypeTableGeneratedInternal = null;
			complexTypeTablesGeneratedAsyncInternal = null;
			complexTypeTablesGeneratedInternal = null;
			viewsGeneratingAsyncInternal = null;
			viewsGeneratingInternal = null;
			viewGeneratingAsyncInternal = null;
			viewGeneratingInternal = null;
			viewPOCOAsyncInternal = null;
			viewPOCOInternal = null;
			viewGeneratedAsyncInternal = null;
			viewGeneratedInternal = null;
			viewsGeneratedAsyncInternal = null;
			viewsGeneratedInternal = null;
			proceduresGeneratingAsyncInternal = null;
			proceduresGeneratingInternal = null;
			procedureGeneratingAsyncInternal = null;
			procedureGeneratingInternal = null;
			procedurePOCOAsyncInternal = null;
			procedurePOCOInternal = null;
			procedureGeneratedAsyncInternal = null;
			procedureGeneratedInternal = null;
			proceduresGeneratedAsyncInternal = null;
			proceduresGeneratedInternal = null;
			functionsGeneratingAsyncInternal = null;
			functionsGeneratingInternal = null;
			functionGeneratingAsyncInternal = null;
			functionGeneratingInternal = null;
			functionPOCOAsyncInternal = null;
			functionPOCOInternal = null;
			functionGeneratedAsyncInternal = null;
			functionGeneratedInternal = null;
			functionsGeneratedAsyncInternal = null;
			functionsGeneratedInternal = null;
			tvpsGeneratingAsyncInternal = null;
			tvpsGeneratingInternal = null;
			tvpGeneratingAsyncInternal = null;
			tvpGeneratingInternal = null;
			tvpPOCOAsyncInternal = null;
			tvpPOCOInternal = null;
			tvpGeneratedAsyncInternal = null;
			tvpGeneratedInternal = null;
			tvpsGeneratedAsyncInternal = null;
			tvpsGeneratedInternal = null;
			databaseGeneratedAsyncInternal = null;
			databaseGeneratedInternal = null;
			serverGeneratedAsyncInternal = null;
			serverGeneratedInternal = null;
		}

		private IDbHandler GetDbHandler(RDBMS rdbms)
		{
			if (rdbms == RDBMS.SQLServer)
			{
				return DbFactory.Instance.SQLServerHandler;
			}
			else if (rdbms == RDBMS.MySQL)
			{
				return DbFactory.Instance.MySQLHandler;
			}

			return null;
		}

		private GeneratorResults ValidateConnectionString(ref IDbHandler dbHandler, ref string serverName, ref string instanceName, ref string userId, ref string initialDatabase)
		{
			if (string.IsNullOrEmpty(settingsInternal.Connection.ConnectionString))
			{
				return GeneratorResults.ConnectionStringMissing;
			}

			if (settingsInternal.Connection.RDBMS == RDBMS.None)
			{
				RDBMS[] items = Enum.GetValues(typeof(RDBMS))
					.Cast<RDBMS>()
					.Where(rdbms => rdbms != RDBMS.None)
					.Where(rdbms => GetDbHandler(rdbms).GetConnectionStringParser().Validate(settingsInternal.Connection.ConnectionString))
					.ToArray();

				if (items.Length == 0)
				{
					return GeneratorResults.ConnectionStringNotMatchAnyRDBMS;
				}
				else if (items.Length == 1)
				{
					settingsInternal.Connection.RDBMS = items[0];
					dbHandler = GetDbHandler(settingsInternal.Connection.RDBMS);

					if (dbHandler.GetConnectionStringParser().Ping(settingsInternal.Connection.ConnectionString) == false)
					{
						dbHandler = null;
						return GeneratorResults.ServerNotResponding;
					}
				}
				else if (items.Length > 1)
				{
					items = items.Where(rdbms => GetDbHandler(rdbms).GetConnectionStringParser().Ping(settingsInternal.Connection.ConnectionString)).ToArray();
					if (items.Length == 0)
					{
						return GeneratorResults.ConnectionStringNotMatchAnyRDBMS;
					}
					else if (items.Length == 1)
					{
						settingsInternal.Connection.RDBMS = items[0];
						dbHandler = GetDbHandler(settingsInternal.Connection.RDBMS);
					}
					else if (items.Length > 1)
					{
						return GeneratorResults.ConnectionStringMatchMoreThanOneRDBMS;
					}
				}
			}
			else
			{
				dbHandler = GetDbHandler(settingsInternal.Connection.RDBMS);

				if (dbHandler.GetConnectionStringParser().Validate(settingsInternal.Connection.ConnectionString) == false)
				{
					dbHandler = null;
					return GeneratorResults.ConnectionStringMalformed;
				}

				if (dbHandler.GetConnectionStringParser().Ping(settingsInternal.Connection.ConnectionString) == false)
				{
					dbHandler = null;
					return GeneratorResults.ServerNotResponding;
				}
			}

			settingsInternal.Connection.ConnectionString = dbHandler.GetConnectionStringParser().Fix(settingsInternal.Connection.ConnectionString);

			bool integratedSecurity = false;
			dbHandler.GetConnectionStringParser().Parse(settingsInternal.Connection.ConnectionString, ref serverName, ref initialDatabase, ref userId, ref integratedSecurity);

			if (string.IsNullOrEmpty(serverName))
			{
				return GeneratorResults.ConnectionStringMalformed;
			}

			int index = serverName.LastIndexOf('\\');
			if (index != -1)
			{
				instanceName = serverName[(index + 1)..];
				serverName = serverName[..index];
			}

			if (integratedSecurity)
			{
				userId = OperatingSystem.IsWindows()
							 ? WindowsIdentity.GetCurrent().Name
							 : "unknown";
			}

			return GeneratorResults.None;
		}

		private bool IsEnableDbObjects(bool isIncludeAll, bool isIncludeAllDbObjects, bool isExcludeAllDbObjects, IList<string> includeDbObjects)
		{
			return
				(isIncludeAll || isIncludeAllDbObjects || includeDbObjects.HasAny()) &&
				(isExcludeAllDbObjects == false);
		}

		#region Accessible Db Objects

		private List<DatabaseAccessibleObjects> GetDatabasesAccessibleObjects(IServer server)
		{
			List<DatabaseAccessibleObjects> databasesAccessibleObjects = server.Databases.Select(database => new DatabaseAccessibleObjects()
			{
				Database = database,
				Tables = GetDatabaseObjects(
					database.Tables,
					settingsInternal.DatabaseObjects.IncludeAll,
					settingsInternal.DatabaseObjects.Tables.IncludeAll,
					settingsInternal.DatabaseObjects.Tables.Include,
					settingsInternal.DatabaseObjects.Tables.ExcludeAll,
					settingsInternal.DatabaseObjects.Tables.Exclude
				),
				Views = GetDatabaseObjects(
					database.Views,
					settingsInternal.DatabaseObjects.IncludeAll,
					settingsInternal.DatabaseObjects.Views.IncludeAll,
					settingsInternal.DatabaseObjects.Views.Include,
					settingsInternal.DatabaseObjects.Views.ExcludeAll,
					settingsInternal.DatabaseObjects.Views.Exclude
				),
				Procedures = GetDatabaseObjects(
					database.Procedures,
					settingsInternal.DatabaseObjects.IncludeAll,
					settingsInternal.DatabaseObjects.StoredProcedures.IncludeAll,
					settingsInternal.DatabaseObjects.StoredProcedures.Include,
					settingsInternal.DatabaseObjects.StoredProcedures.ExcludeAll,
					settingsInternal.DatabaseObjects.StoredProcedures.Exclude
				),
				Functions = GetDatabaseObjects(
					database.Functions,
					settingsInternal.DatabaseObjects.IncludeAll,
					settingsInternal.DatabaseObjects.Functions.IncludeAll,
					settingsInternal.DatabaseObjects.Functions.Include,
					settingsInternal.DatabaseObjects.Functions.ExcludeAll,
					settingsInternal.DatabaseObjects.Functions.Exclude
				),
				TVPs = GetDatabaseObjects(
					database.TVPs,
					settingsInternal.DatabaseObjects.IncludeAll,
					settingsInternal.DatabaseObjects.TVPs.IncludeAll,
					settingsInternal.DatabaseObjects.TVPs.Include,
					settingsInternal.DatabaseObjects.TVPs.ExcludeAll,
					settingsInternal.DatabaseObjects.TVPs.Exclude
				),
			})
			.Where(d =>
				d.Tables.HasAny() ||
				d.Views.HasAny() ||
				d.Procedures.HasAny() ||
				d.Functions.HasAny() ||
				d.TVPs.HasAny())
			.ToList();

			SetDatabaseAccessibleTables(databasesAccessibleObjects);

			databasesAccessibleObjects.Sort((x, y) => x.Database.ToString().CompareTo(y.Database.ToString()));

			foreach (DatabaseAccessibleObjects item in databasesAccessibleObjects)
			{
				if (item.Tables.HasAny())
				{
					item.Tables.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}

				if (item.AccessibleTables.HasAny())
				{
					item.AccessibleTables.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}

				if (item.Views.HasAny())
				{
					item.Views.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}

				if (item.Procedures.HasAny())
				{
					item.Procedures.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}

				if (item.Functions.HasAny())
				{
					item.Functions.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}

				if (item.TVPs.HasAny())
				{
					item.TVPs.Sort((x, y) => x.ToString().CompareTo(y.ToString()));
				}
			}

			return databasesAccessibleObjects;
		}

		private List<T> GetDatabaseObjects<T>(
			List<T> dbObjects,
			bool isIncludeAll,
			bool isIncludeAllDbObjects,
			IList<string> includeDbObjects,
			bool isExcludeAllDbObjects,
			IList<string> excludeDbObjects) where T : IDbObjectTraverse
		{
			if (dbObjects.HasAny())
			{
				if (isExcludeAllDbObjects == false)
				{
					if (isIncludeAll || isIncludeAllDbObjects)
					{
						if (excludeDbObjects.IsNullOrEmpty())
						{
							return dbObjects.ToList();
						}
						else
						{
							string[] excludePatterns = excludeDbObjects.Select(eo => "^" + Regex.Escape(eo).Replace(@"\*", ".*?").Replace(@"\?", ".") + "$").ToArray();
							return dbObjects.Where(o => excludePatterns.All(pattern => Regex.IsMatch(o.ToString(), pattern) == false && Regex.IsMatch(o.Name, pattern) == false)).ToList();
						}
					}
					else if (includeDbObjects.HasAny())
					{
						string[] includePatterns = includeDbObjects.Select(io => "^" + Regex.Escape(io).Replace(@"\*", ".*?").Replace(@"\?", ".") + "$").ToArray();
						IEnumerable<T> includedObjects = dbObjects.Where(o => includePatterns.Any(pattern => Regex.IsMatch(o.ToString(), pattern) || Regex.IsMatch(o.Name, pattern)));

						if (excludeDbObjects.IsNullOrEmpty())
						{
							return includedObjects.ToList();
						}
						else
						{
							string[] excludePatterns = excludeDbObjects.Select(eo => "^" + Regex.Escape(eo).Replace(@"\*", ".*?").Replace(@"\?", ".") + "$").ToArray();
							return includedObjects.Where(o => excludePatterns.All(pattern => Regex.IsMatch(o.ToString(), pattern) == false && Regex.IsMatch(o.Name, pattern) == false)).ToList();
						}
					}
				}
			}

			return null;
		}

		private void SetDatabaseAccessibleTables(List<DatabaseAccessibleObjects> databasesAccessibleObjects)
		{
			foreach (DatabaseAccessibleObjects item in databasesAccessibleObjects)
			{
				if (item.Tables.HasAny())
				{
					item.AccessibleTables = new List<ITable>(item.Tables);

					int fromIndex = 0;
					int tablesCount = item.AccessibleTables.Count;
					bool isContinue = true;

					while (isContinue)
					{
						for (int i = fromIndex; i < tablesCount; i++)
						{
							ITable table = item.AccessibleTables[i];

							if (table.ForeignKeys.HasAny())
							{
								foreach (IForeignKey foreignKey in table.ForeignKeys)
								{
									if (item.AccessibleTables.Contains(foreignKey.PrimaryTable) == false)
									{
										item.AccessibleTables.Add(foreignKey.PrimaryTable);
									}
								}
							}

							foreach (ITable foreignTable in item.Database.Tables)
							{
								if (foreignTable.ForeignKeys.HasAny())
								{
									if (item.AccessibleTables.Contains(foreignTable) == false)
									{
										if (foreignTable.ForeignKeys.Any(x => x.PrimaryTable == table))
										{
											item.AccessibleTables.Add(foreignTable);
										}
									}
								}
							}

							if (table.ComplexTypeTables.HasAny())
							{
								foreach (IComplexTypeTable complexTypeTable in table.ComplexTypeTables)
								{
									foreach (ITable t in complexTypeTable.Tables)
									{
										if (item.AccessibleTables.Contains(t) == false)
										{
											item.AccessibleTables.Add(t);
										}
									}
								}
							}
						}

						isContinue = tablesCount < item.AccessibleTables.Count;

						fromIndex = tablesCount;
						tablesCount = item.AccessibleTables.Count;
					}

					item.AccessibleTables.RemoveRange(0, item.Tables.Count);

					if (item.AccessibleTables.IsNullOrEmpty())
					{
						item.AccessibleTables = null;
					}
				}
			}
		}

		private void SetDbObjectsToGenerate(List<DatabaseAccessibleObjects> databasesAccessibleObjects)
		{
			if (databasesAccessibleObjects.HasAny())
			{
				dbObjectsToGenerate = [];

				foreach (DatabaseAccessibleObjects item in databasesAccessibleObjects)
				{
					if (item.Tables.HasAny())
					{
						dbObjectsToGenerate.AddRange(item.Tables);
					}

					if (item.Views.HasAny())
					{
						dbObjectsToGenerate.AddRange(item.Views);
					}

					if (item.Procedures.HasAny())
					{
						dbObjectsToGenerate.AddRange(item.Procedures);
					}

					if (item.Functions.HasAny())
					{
						dbObjectsToGenerate.AddRange(item.Functions);
					}

					if (item.TVPs.HasAny())
					{
						dbObjectsToGenerate.AddRange(item.TVPs);
					}
				}
			}
			else
			{
				dbObjectsToGenerate = null;
			}
		}

		#endregion

		#region Events

		// in order of execution
		private event EventHandler<ServerBuiltAsyncEventArgs> serverBuiltAsync;
		private event EventHandler<ServerBuiltEventArgs> serverBuilt;
		private event EventHandler<ServerGeneratingAsyncEventArgs> serverGeneratingAsync;
		private event EventHandler<ServerGeneratingEventArgs> serverGenerating;
		private event EventHandler<DatabaseGeneratingAsyncEventArgs> databaseGeneratingAsync;
		private event EventHandler<DatabaseGeneratingEventArgs> databaseGenerating;
		private event EventHandler<TablesGeneratingAsyncEventArgs> tablesGeneratingAsync;
		private event EventHandler<TablesGeneratingEventArgs> tablesGenerating;
		private event EventHandler<TableGeneratingAsyncEventArgs> tableGeneratingAsync;
		private event EventHandler<TableGeneratingEventArgs> tableGenerating;
		private event EventHandler<TablePOCOAsyncEventArgs> tablePOCOAsync;
		private event EventHandler<TablePOCOEventArgs> tablePOCO;
		private event EventHandler<TableGeneratedAsyncEventArgs> tableGeneratedAsync;
		private event EventHandler<TableGeneratedEventArgs> tableGenerated;
		private event EventHandler<TablesGeneratedAsyncEventArgs> tablesGeneratedAsync;
		private event EventHandler<TablesGeneratedEventArgs> tablesGenerated;
		private event EventHandler<ComplexTypeTablesGeneratingAsyncEventArgs> complexTypeTablesGeneratingAsync;
		private event EventHandler<ComplexTypeTablesGeneratingEventArgs> complexTypeTablesGenerating;
		private event EventHandler<ComplexTypeTableGeneratingAsyncEventArgs> complexTypeTableGeneratingAsync;
		private event EventHandler<ComplexTypeTableGeneratingEventArgs> complexTypeTableGenerating;
		private event EventHandler<ComplexTypeTablePOCOAsyncEventArgs> complexTypeTablePOCOAsync;
		private event EventHandler<ComplexTypeTablePOCOEventArgs> complexTypeTablePOCO;
		private event EventHandler<ComplexTypeTableGeneratedAsyncEventArgs> complexTypeTableGeneratedAsync;
		private event EventHandler<ComplexTypeTableGeneratedEventArgs> complexTypeTableGenerated;
		private event EventHandler<ComplexTypeTablesGeneratedAsyncEventArgs> complexTypeTablesGeneratedAsync;
		private event EventHandler<ComplexTypeTablesGeneratedEventArgs> complexTypeTablesGenerated;
		private event EventHandler<ViewsGeneratingAsyncEventArgs> viewsGeneratingAsync;
		private event EventHandler<ViewsGeneratingEventArgs> viewsGenerating;
		private event EventHandler<ViewGeneratingAsyncEventArgs> viewGeneratingAsync;
		private event EventHandler<ViewGeneratingEventArgs> viewGenerating;
		private event EventHandler<ViewPOCOAsyncEventArgs> viewPOCOAsync;
		private event EventHandler<ViewPOCOEventArgs> viewPOCO;
		private event EventHandler<ViewGeneratedAsyncEventArgs> viewGeneratedAsync;
		private event EventHandler<ViewGeneratedEventArgs> viewGenerated;
		private event EventHandler<ViewsGeneratedAsyncEventArgs> viewsGeneratedAsync;
		private event EventHandler<ViewsGeneratedEventArgs> viewsGenerated;
		private event EventHandler<ProceduresGeneratingAsyncEventArgs> proceduresGeneratingAsync;
		private event EventHandler<ProceduresGeneratingEventArgs> proceduresGenerating;
		private event EventHandler<ProcedureGeneratingAsyncEventArgs> procedureGeneratingAsync;
		private event EventHandler<ProcedureGeneratingEventArgs> procedureGenerating;
		private event EventHandler<ProcedurePOCOAsyncEventArgs> procedurePOCOAsync;
		private event EventHandler<ProcedurePOCOEventArgs> procedurePOCO;
		private event EventHandler<ProcedureGeneratedAsyncEventArgs> procedureGeneratedAsync;
		private event EventHandler<ProcedureGeneratedEventArgs> procedureGenerated;
		private event EventHandler<ProceduresGeneratedAsyncEventArgs> proceduresGeneratedAsync;
		private event EventHandler<ProceduresGeneratedEventArgs> proceduresGenerated;
		private event EventHandler<FunctionsGeneratingAsyncEventArgs> functionsGeneratingAsync;
		private event EventHandler<FunctionsGeneratingEventArgs> functionsGenerating;
		private event EventHandler<FunctionGeneratingAsyncEventArgs> functionGeneratingAsync;
		private event EventHandler<FunctionGeneratingEventArgs> functionGenerating;
		private event EventHandler<FunctionPOCOAsyncEventArgs> functionPOCOAsync;
		private event EventHandler<FunctionPOCOEventArgs> functionPOCO;
		private event EventHandler<FunctionGeneratedAsyncEventArgs> functionGeneratedAsync;
		private event EventHandler<FunctionGeneratedEventArgs> functionGenerated;
		private event EventHandler<FunctionsGeneratedAsyncEventArgs> functionsGeneratedAsync;
		private event EventHandler<FunctionsGeneratedEventArgs> functionsGenerated;
		private event EventHandler<TVPsGeneratingAsyncEventArgs> tvpsGeneratingAsync;
		private event EventHandler<TVPsGeneratingEventArgs> tvpsGenerating;
		private event EventHandler<TVPGeneratingAsyncEventArgs> tvpGeneratingAsync;
		private event EventHandler<TVPGeneratingEventArgs> tvpGenerating;
		private event EventHandler<TVPPOCOAsyncEventArgs> tvpPOCOAsync;
		private event EventHandler<TVPPOCOEventArgs> tvpPOCO;
		private event EventHandler<TVPGeneratedAsyncEventArgs> tvpGeneratedAsync;
		private event EventHandler<TVPGeneratedEventArgs> tvpGenerated;
		private event EventHandler<TVPsGeneratedAsyncEventArgs> tvpsGeneratedAsync;
		private event EventHandler<TVPsGeneratedEventArgs> tvpsGenerated;
		private event EventHandler<DatabaseGeneratedAsyncEventArgs> databaseGeneratedAsync;
		private event EventHandler<DatabaseGeneratedEventArgs> databaseGenerated;
		private event EventHandler<ServerGeneratedAsyncEventArgs> serverGeneratedAsync;
		private event EventHandler<ServerGeneratedEventArgs> serverGenerated;

		// in order of execution
		private event EventHandler<ServerBuiltAsyncEventArgs> serverBuiltAsyncInternal;
		private event EventHandler<ServerBuiltEventArgs> serverBuiltInternal;
		private event EventHandler<ServerGeneratingAsyncEventArgs> serverGeneratingAsyncInternal;
		private event EventHandler<ServerGeneratingEventArgs> serverGeneratingInternal;
		private event EventHandler<DatabaseGeneratingAsyncEventArgs> databaseGeneratingAsyncInternal;
		private event EventHandler<DatabaseGeneratingEventArgs> databaseGeneratingInternal;
		private event EventHandler<TablesGeneratingAsyncEventArgs> tablesGeneratingAsyncInternal;
		private event EventHandler<TablesGeneratingEventArgs> tablesGeneratingInternal;
		private event EventHandler<TableGeneratingAsyncEventArgs> tableGeneratingAsyncInternal;
		private event EventHandler<TableGeneratingEventArgs> tableGeneratingInternal;
		private event EventHandler<TablePOCOAsyncEventArgs> tablePOCOAsyncInternal;
		private event EventHandler<TablePOCOEventArgs> tablePOCOInternal;
		private event EventHandler<TableGeneratedAsyncEventArgs> tableGeneratedAsyncInternal;
		private event EventHandler<TableGeneratedEventArgs> tableGeneratedInternal;
		private event EventHandler<TablesGeneratedAsyncEventArgs> tablesGeneratedAsyncInternal;
		private event EventHandler<TablesGeneratedEventArgs> tablesGeneratedInternal;
		private event EventHandler<ComplexTypeTablesGeneratingAsyncEventArgs> complexTypeTablesGeneratingAsyncInternal;
		private event EventHandler<ComplexTypeTablesGeneratingEventArgs> complexTypeTablesGeneratingInternal;
		private event EventHandler<ComplexTypeTableGeneratingAsyncEventArgs> complexTypeTableGeneratingAsyncInternal;
		private event EventHandler<ComplexTypeTableGeneratingEventArgs> complexTypeTableGeneratingInternal;
		private event EventHandler<ComplexTypeTablePOCOAsyncEventArgs> complexTypeTablePOCOAsyncInternal;
		private event EventHandler<ComplexTypeTablePOCOEventArgs> complexTypeTablePOCOInternal;
		private event EventHandler<ComplexTypeTableGeneratedAsyncEventArgs> complexTypeTableGeneratedAsyncInternal;
		private event EventHandler<ComplexTypeTableGeneratedEventArgs> complexTypeTableGeneratedInternal;
		private event EventHandler<ComplexTypeTablesGeneratedAsyncEventArgs> complexTypeTablesGeneratedAsyncInternal;
		private event EventHandler<ComplexTypeTablesGeneratedEventArgs> complexTypeTablesGeneratedInternal;
		private event EventHandler<ViewsGeneratingAsyncEventArgs> viewsGeneratingAsyncInternal;
		private event EventHandler<ViewsGeneratingEventArgs> viewsGeneratingInternal;
		private event EventHandler<ViewGeneratingAsyncEventArgs> viewGeneratingAsyncInternal;
		private event EventHandler<ViewGeneratingEventArgs> viewGeneratingInternal;
		private event EventHandler<ViewPOCOAsyncEventArgs> viewPOCOAsyncInternal;
		private event EventHandler<ViewPOCOEventArgs> viewPOCOInternal;
		private event EventHandler<ViewGeneratedAsyncEventArgs> viewGeneratedAsyncInternal;
		private event EventHandler<ViewGeneratedEventArgs> viewGeneratedInternal;
		private event EventHandler<ViewsGeneratedAsyncEventArgs> viewsGeneratedAsyncInternal;
		private event EventHandler<ViewsGeneratedEventArgs> viewsGeneratedInternal;
		private event EventHandler<ProceduresGeneratingAsyncEventArgs> proceduresGeneratingAsyncInternal;
		private event EventHandler<ProceduresGeneratingEventArgs> proceduresGeneratingInternal;
		private event EventHandler<ProcedureGeneratingAsyncEventArgs> procedureGeneratingAsyncInternal;
		private event EventHandler<ProcedureGeneratingEventArgs> procedureGeneratingInternal;
		private event EventHandler<ProcedurePOCOAsyncEventArgs> procedurePOCOAsyncInternal;
		private event EventHandler<ProcedurePOCOEventArgs> procedurePOCOInternal;
		private event EventHandler<ProcedureGeneratedAsyncEventArgs> procedureGeneratedAsyncInternal;
		private event EventHandler<ProcedureGeneratedEventArgs> procedureGeneratedInternal;
		private event EventHandler<ProceduresGeneratedAsyncEventArgs> proceduresGeneratedAsyncInternal;
		private event EventHandler<ProceduresGeneratedEventArgs> proceduresGeneratedInternal;
		private event EventHandler<FunctionsGeneratingAsyncEventArgs> functionsGeneratingAsyncInternal;
		private event EventHandler<FunctionsGeneratingEventArgs> functionsGeneratingInternal;
		private event EventHandler<FunctionGeneratingAsyncEventArgs> functionGeneratingAsyncInternal;
		private event EventHandler<FunctionGeneratingEventArgs> functionGeneratingInternal;
		private event EventHandler<FunctionPOCOAsyncEventArgs> functionPOCOAsyncInternal;
		private event EventHandler<FunctionPOCOEventArgs> functionPOCOInternal;
		private event EventHandler<FunctionGeneratedAsyncEventArgs> functionGeneratedAsyncInternal;
		private event EventHandler<FunctionGeneratedEventArgs> functionGeneratedInternal;
		private event EventHandler<FunctionsGeneratedAsyncEventArgs> functionsGeneratedAsyncInternal;
		private event EventHandler<FunctionsGeneratedEventArgs> functionsGeneratedInternal;
		private event EventHandler<TVPsGeneratingAsyncEventArgs> tvpsGeneratingAsyncInternal;
		private event EventHandler<TVPsGeneratingEventArgs> tvpsGeneratingInternal;
		private event EventHandler<TVPGeneratingAsyncEventArgs> tvpGeneratingAsyncInternal;
		private event EventHandler<TVPGeneratingEventArgs> tvpGeneratingInternal;
		private event EventHandler<TVPPOCOAsyncEventArgs> tvpPOCOAsyncInternal;
		private event EventHandler<TVPPOCOEventArgs> tvpPOCOInternal;
		private event EventHandler<TVPGeneratedAsyncEventArgs> tvpGeneratedAsyncInternal;
		private event EventHandler<TVPGeneratedEventArgs> tvpGeneratedInternal;
		private event EventHandler<TVPsGeneratedAsyncEventArgs> tvpsGeneratedAsyncInternal;
		private event EventHandler<TVPsGeneratedEventArgs> tvpsGeneratedInternal;
		private event EventHandler<DatabaseGeneratedAsyncEventArgs> databaseGeneratedAsyncInternal;
		private event EventHandler<DatabaseGeneratedEventArgs> databaseGeneratedInternal;
		private event EventHandler<ServerGeneratedAsyncEventArgs> serverGeneratedAsyncInternal;
		private event EventHandler<ServerGeneratedEventArgs> serverGeneratedInternal;

		// in order of execution
		public event EventHandler<ServerBuiltAsyncEventArgs> ServerBuiltAsync
		{
			add
			{
				lock (lockObject)
				{
					serverBuiltAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverBuiltAsync -= value;
				}
			}
		}
		public event EventHandler<ServerBuiltEventArgs> ServerBuilt
		{
			add
			{
				lock (lockObject)
				{
					serverBuilt += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverBuilt -= value;
				}
			}
		}
		public event EventHandler<ServerGeneratingAsyncEventArgs> ServerGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					serverGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ServerGeneratingEventArgs> ServerGenerating
		{
			add
			{
				lock (lockObject)
				{
					serverGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverGenerating -= value;
				}
			}
		}
		public event EventHandler<DatabaseGeneratingAsyncEventArgs> DatabaseGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					databaseGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					databaseGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<DatabaseGeneratingEventArgs> DatabaseGenerating
		{
			add
			{
				lock (lockObject)
				{
					databaseGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					databaseGenerating -= value;
				}
			}
		}
		public event EventHandler<TablesGeneratingAsyncEventArgs> TablesGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					tablesGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablesGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<TablesGeneratingEventArgs> TablesGenerating
		{
			add
			{
				lock (lockObject)
				{
					tablesGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablesGenerating -= value;
				}
			}
		}
		public event EventHandler<TableGeneratingAsyncEventArgs> TableGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					tableGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tableGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<TableGeneratingEventArgs> TableGenerating
		{
			add
			{
				lock (lockObject)
				{
					tableGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tableGenerating -= value;
				}
			}
		}
		public event EventHandler<TablePOCOAsyncEventArgs> TablePOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					tablePOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablePOCOAsync -= value;
				}
			}
		}
		public event EventHandler<TablePOCOEventArgs> TablePOCO
		{
			add
			{
				lock (lockObject)
				{
					tablePOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablePOCO -= value;
				}
			}
		}
		public event EventHandler<TableGeneratedAsyncEventArgs> TableGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					tableGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tableGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<TableGeneratedEventArgs> TableGenerated
		{
			add
			{
				lock (lockObject)
				{
					tableGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tableGenerated -= value;
				}
			}
		}
		public event EventHandler<TablesGeneratedAsyncEventArgs> TablesGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					tablesGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablesGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<TablesGeneratedEventArgs> TablesGenerated
		{
			add
			{
				lock (lockObject)
				{
					tablesGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tablesGenerated -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablesGeneratingAsyncEventArgs> ComplexTypeTablesGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablesGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablesGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablesGeneratingEventArgs> ComplexTypeTablesGenerating
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablesGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablesGenerating -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTableGeneratingAsyncEventArgs> ComplexTypeTableGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTableGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTableGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTableGeneratingEventArgs> ComplexTypeTableGenerating
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTableGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTableGenerating -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablePOCOAsyncEventArgs> ComplexTypeTablePOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablePOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablePOCOAsync -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablePOCOEventArgs> ComplexTypeTablePOCO
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablePOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablePOCO -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTableGeneratedAsyncEventArgs> ComplexTypeTableGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTableGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTableGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTableGeneratedEventArgs> ComplexTypeTableGenerated
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTableGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTableGenerated -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablesGeneratedAsyncEventArgs> ComplexTypeTablesGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablesGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablesGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ComplexTypeTablesGeneratedEventArgs> ComplexTypeTablesGenerated
		{
			add
			{
				lock (lockObject)
				{
					complexTypeTablesGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					complexTypeTablesGenerated -= value;
				}
			}
		}
		public event EventHandler<ViewsGeneratingAsyncEventArgs> ViewsGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					viewsGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewsGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ViewsGeneratingEventArgs> ViewsGenerating
		{
			add
			{
				lock (lockObject)
				{
					viewsGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewsGenerating -= value;
				}
			}
		}
		public event EventHandler<ViewGeneratingAsyncEventArgs> ViewGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					viewGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ViewGeneratingEventArgs> ViewGenerating
		{
			add
			{
				lock (lockObject)
				{
					viewGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewGenerating -= value;
				}
			}
		}
		public event EventHandler<ViewPOCOAsyncEventArgs> ViewPOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					viewPOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewPOCOAsync -= value;
				}
			}
		}
		public event EventHandler<ViewPOCOEventArgs> ViewPOCO
		{
			add
			{
				lock (lockObject)
				{
					viewPOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewPOCO -= value;
				}
			}
		}
		public event EventHandler<ViewGeneratedAsyncEventArgs> ViewGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					viewGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ViewGeneratedEventArgs> ViewGenerated
		{
			add
			{
				lock (lockObject)
				{
					viewGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewGenerated -= value;
				}
			}
		}
		public event EventHandler<ViewsGeneratedAsyncEventArgs> ViewsGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					viewsGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewsGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ViewsGeneratedEventArgs> ViewsGenerated
		{
			add
			{
				lock (lockObject)
				{
					viewsGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					viewsGenerated -= value;
				}
			}
		}
		public event EventHandler<ProceduresGeneratingAsyncEventArgs> ProceduresGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					proceduresGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					proceduresGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ProceduresGeneratingEventArgs> ProceduresGenerating
		{
			add
			{
				lock (lockObject)
				{
					proceduresGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					proceduresGenerating -= value;
				}
			}
		}
		public event EventHandler<ProcedureGeneratingAsyncEventArgs> ProcedureGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					procedureGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedureGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<ProcedureGeneratingEventArgs> ProcedureGenerating
		{
			add
			{
				lock (lockObject)
				{
					procedureGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedureGenerating -= value;
				}
			}
		}
		public event EventHandler<ProcedurePOCOAsyncEventArgs> ProcedurePOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					procedurePOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedurePOCOAsync -= value;
				}
			}
		}
		public event EventHandler<ProcedurePOCOEventArgs> ProcedurePOCO
		{
			add
			{
				lock (lockObject)
				{
					procedurePOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedurePOCO -= value;
				}
			}
		}
		public event EventHandler<ProcedureGeneratedAsyncEventArgs> ProcedureGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					procedureGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedureGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ProcedureGeneratedEventArgs> ProcedureGenerated
		{
			add
			{
				lock (lockObject)
				{
					procedureGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					procedureGenerated -= value;
				}
			}
		}
		public event EventHandler<ProceduresGeneratedAsyncEventArgs> ProceduresGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					proceduresGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					proceduresGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ProceduresGeneratedEventArgs> ProceduresGenerated
		{
			add
			{
				lock (lockObject)
				{
					proceduresGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					proceduresGenerated -= value;
				}
			}
		}
		public event EventHandler<FunctionsGeneratingAsyncEventArgs> FunctionsGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					functionsGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionsGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<FunctionsGeneratingEventArgs> FunctionsGenerating
		{
			add
			{
				lock (lockObject)
				{
					functionsGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionsGenerating -= value;
				}
			}
		}
		public event EventHandler<FunctionGeneratingAsyncEventArgs> FunctionGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					functionGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<FunctionGeneratingEventArgs> FunctionGenerating
		{
			add
			{
				lock (lockObject)
				{
					functionGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionGenerating -= value;
				}
			}
		}
		public event EventHandler<FunctionPOCOAsyncEventArgs> FunctionPOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					functionPOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionPOCOAsync -= value;
				}
			}
		}
		public event EventHandler<FunctionPOCOEventArgs> FunctionPOCO
		{
			add
			{
				lock (lockObject)
				{
					functionPOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionPOCO -= value;
				}
			}
		}
		public event EventHandler<FunctionGeneratedAsyncEventArgs> FunctionGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					functionGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<FunctionGeneratedEventArgs> FunctionGenerated
		{
			add
			{
				lock (lockObject)
				{
					functionGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionGenerated -= value;
				}
			}
		}
		public event EventHandler<FunctionsGeneratedAsyncEventArgs> FunctionsGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					functionsGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionsGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<FunctionsGeneratedEventArgs> FunctionsGenerated
		{
			add
			{
				lock (lockObject)
				{
					functionsGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					functionsGenerated -= value;
				}
			}
		}
		public event EventHandler<TVPsGeneratingAsyncEventArgs> TVPsGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					tvpsGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpsGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<TVPsGeneratingEventArgs> TVPsGenerating
		{
			add
			{
				lock (lockObject)
				{
					tvpsGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpsGenerating -= value;
				}
			}
		}
		public event EventHandler<TVPGeneratingAsyncEventArgs> TVPGeneratingAsync
		{
			add
			{
				lock (lockObject)
				{
					tvpGeneratingAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpGeneratingAsync -= value;
				}
			}
		}
		public event EventHandler<TVPGeneratingEventArgs> TVPGenerating
		{
			add
			{
				lock (lockObject)
				{
					tvpGenerating += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpGenerating -= value;
				}
			}
		}
		public event EventHandler<TVPPOCOAsyncEventArgs> TVPPOCOAsync
		{
			add
			{
				lock (lockObject)
				{
					tvpPOCOAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpPOCOAsync -= value;
				}
			}
		}
		public event EventHandler<TVPPOCOEventArgs> TVPPOCO
		{
			add
			{
				lock (lockObject)
				{
					tvpPOCO += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpPOCO -= value;
				}
			}
		}
		public event EventHandler<TVPGeneratedAsyncEventArgs> TVPGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					tvpGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<TVPGeneratedEventArgs> TVPGenerated
		{
			add
			{
				lock (lockObject)
				{
					tvpGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpGenerated -= value;
				}
			}
		}
		public event EventHandler<TVPsGeneratedAsyncEventArgs> TVPsGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					tvpsGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpsGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<TVPsGeneratedEventArgs> TVPsGenerated
		{
			add
			{
				lock (lockObject)
				{
					tvpsGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					tvpsGenerated -= value;
				}
			}
		}
		public event EventHandler<DatabaseGeneratedAsyncEventArgs> DatabaseGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					databaseGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					databaseGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<DatabaseGeneratedEventArgs> DatabaseGenerated
		{
			add
			{
				lock (lockObject)
				{
					databaseGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					databaseGenerated -= value;
				}
			}
		}
		public event EventHandler<ServerGeneratedAsyncEventArgs> ServerGeneratedAsync
		{
			add
			{
				lock (lockObject)
				{
					serverGeneratedAsync += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverGeneratedAsync -= value;
				}
			}
		}
		public event EventHandler<ServerGeneratedEventArgs> ServerGenerated
		{
			add
			{
				lock (lockObject)
				{
					serverGenerated += value;
				}
			}
			remove
			{
				lock (lockObject)
				{
					serverGenerated -= value;
				}
			}
		}

		private void SetPOCOIteratorEvents(IDbIterator iterator, POCOGenerator.Objects.Server proxyServer)
		{
			if (serverGeneratingAsyncInternal != null)
			{
				iterator.ServerGeneratingAsync += (sender, e) =>
				{
					serverGeneratingAsyncInternal.RaiseAsync(
						this,
						new ServerGeneratingAsyncEventArgs(
							proxyServer
						)
					);
				};
			}

			if (serverGeneratingInternal != null)
			{
				iterator.ServerGenerating += (sender, e) =>
				{
					ServerGeneratingEventArgs args = new(
						proxyServer
					);
					serverGeneratingInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (databaseGeneratingAsyncInternal != null)
			{
				iterator.DatabaseGeneratingAsync += (sender, e) =>
				{
					databaseGeneratingAsyncInternal.RaiseAsync(
						this,
						new DatabaseGeneratingAsyncEventArgs(
							proxyServer.Databases.First(d => d.InternalEquals(e.Database))
						)
					);
				};
			}

			if (databaseGeneratingInternal != null)
			{
				iterator.DatabaseGenerating += (sender, e) =>
				{
					DatabaseGeneratingEventArgs args = new(
						proxyServer.Databases.First(d => d.InternalEquals(e.Database))
					);
					databaseGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (tablesGeneratingAsyncInternal != null)
			{
				iterator.TablesGeneratingAsync += (sender, e) =>
				{
					tablesGeneratingAsyncInternal.RaiseAsync(
						this,
						new TablesGeneratingAsyncEventArgs()
					);
				};
			}

			if (tablesGeneratingInternal != null)
			{
				iterator.TablesGenerating += (sender, e) =>
				{
					TablesGeneratingEventArgs args = new();
					tablesGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (tableGeneratingAsyncInternal != null)
			{
				iterator.TableGeneratingAsync += (sender, e) =>
				{
					tableGeneratingAsyncInternal.RaiseAsync(
						this,
						new TableGeneratingAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
							e.Namespace
						)
					);
				};
			}

			if (tableGeneratingInternal != null)
			{
				iterator.TableGenerating += (sender, e) =>
				{
					TableGeneratingEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
						e.Namespace
					);
					tableGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (tablePOCOAsyncInternal != null)
			{
				iterator.TablePOCOAsync += (sender, e) =>
				{
					tablePOCOAsyncInternal.RaiseAsync(
						this,
						new TablePOCOAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
							e.POCO
						)
					);
				};
			}

			if (tablePOCOInternal != null)
			{
				iterator.TablePOCO += (sender, e) =>
				{
					TablePOCOEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
						e.POCO
					);
					tablePOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (tableGeneratedAsyncInternal != null)
			{
				iterator.TableGeneratedAsync += (sender, e) =>
				{
					tableGeneratedAsyncInternal.RaiseAsync(
						this,
						new TableGeneratedAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
							e.Namespace
						)
					);
				};
			}

			if (tableGeneratedInternal != null)
			{
				iterator.TableGenerated += (sender, e) =>
				{
					TableGeneratedEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Tables).First(t => t.InternalEquals(e.Table)),
						e.Namespace
					);
					tableGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (tablesGeneratedAsyncInternal != null)
			{
				iterator.TablesGeneratedAsync += (sender, e) =>
				{
					tablesGeneratedAsyncInternal.RaiseAsync(
						this,
						new TablesGeneratedAsyncEventArgs()
					);
				};
			}

			if (tablesGeneratedInternal != null)
			{
				iterator.TablesGenerated += (sender, e) =>
				{
					TablesGeneratedEventArgs args = new();
					tablesGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (complexTypeTablesGeneratingAsyncInternal != null)
			{
				iterator.ComplexTypeTablesGeneratingAsync += (sender, e) =>
				{
					complexTypeTablesGeneratingAsyncInternal.RaiseAsync(
						this,
						new ComplexTypeTablesGeneratingAsyncEventArgs()
					);
				};
			}

			if (complexTypeTablesGeneratingInternal != null)
			{
				iterator.ComplexTypeTablesGenerating += (sender, e) =>
				{
					ComplexTypeTablesGeneratingEventArgs args = new();
					complexTypeTablesGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (complexTypeTableGeneratingAsyncInternal != null)
			{
				iterator.ComplexTypeTableGeneratingAsync += (sender, e) =>
				{
					complexTypeTableGeneratingAsyncInternal.RaiseAsync(
						this,
						new ComplexTypeTableGeneratingAsyncEventArgs(
							proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
							e.Namespace
						)
					);
				};
			}

			if (complexTypeTableGeneratingInternal != null)
			{
				iterator.ComplexTypeTableGenerating += (sender, e) =>
				{
					ComplexTypeTableGeneratingEventArgs args = new(
						proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
						e.Namespace
					);
					complexTypeTableGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (complexTypeTablePOCOAsyncInternal != null)
			{
				iterator.ComplexTypeTablePOCOAsync += (sender, e) =>
				{
					complexTypeTablePOCOAsyncInternal.RaiseAsync(
						this,
						new ComplexTypeTablePOCOAsyncEventArgs(
							proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
							e.POCO
						)
					);
				};
			}

			if (complexTypeTablePOCOInternal != null)
			{
				iterator.ComplexTypeTablePOCO += (sender, e) =>
				{
					ComplexTypeTablePOCOEventArgs args = new(
						proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
						e.POCO
					);
					complexTypeTablePOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (complexTypeTableGeneratedAsyncInternal != null)
			{
				iterator.ComplexTypeTableGeneratedAsync += (sender, e) =>
				{
					complexTypeTableGeneratedAsyncInternal.RaiseAsync(
						this,
						new ComplexTypeTableGeneratedAsyncEventArgs(
							proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
							e.Namespace
						)
					);
				};
			}

			if (complexTypeTableGeneratedInternal != null)
			{
				iterator.ComplexTypeTableGenerated += (sender, e) =>
				{
					ComplexTypeTableGeneratedEventArgs args = new(
						proxyServer.Databases.Where(d => d.ComplexTypeTables.HasAny()).SelectMany(d => d.ComplexTypeTables).First(t => t.InternalEquals(e.ComplexTypeTable)),
						e.Namespace
					);
					complexTypeTableGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (complexTypeTablesGeneratedAsyncInternal != null)
			{
				iterator.ComplexTypeTablesGeneratedAsync += (sender, e) =>
				{
					complexTypeTablesGeneratedAsyncInternal.RaiseAsync(
						this,
						new ComplexTypeTablesGeneratedAsyncEventArgs()
					);
				};
			}

			if (complexTypeTablesGeneratedInternal != null)
			{
				iterator.ComplexTypeTablesGenerated += (sender, e) =>
				{
					ComplexTypeTablesGeneratedEventArgs args = new();
					complexTypeTablesGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (viewsGeneratingAsyncInternal != null)
			{
				iterator.ViewsGeneratingAsync += (sender, e) =>
				{
					viewsGeneratingAsyncInternal.RaiseAsync(
						this,
						new ViewsGeneratingAsyncEventArgs()
					);
				};
			}

			if (viewsGeneratingInternal != null)
			{
				iterator.ViewsGenerating += (sender, e) =>
				{
					ViewsGeneratingEventArgs args = new();
					viewsGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (viewGeneratingAsyncInternal != null)
			{
				iterator.ViewGeneratingAsync += (sender, e) =>
				{
					viewGeneratingAsyncInternal.RaiseAsync(
						this,
						new ViewGeneratingAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
							e.Namespace
						)
					);
				};
			}

			if (viewGeneratingInternal != null)
			{
				iterator.ViewGenerating += (sender, e) =>
				{
					ViewGeneratingEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
						e.Namespace
					);
					viewGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (viewPOCOAsyncInternal != null)
			{
				iterator.ViewPOCOAsync += (sender, e) =>
				{
					viewPOCOAsyncInternal.RaiseAsync(
						this,
						new ViewPOCOAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
							e.POCO
						)
					);
				};
			}

			if (viewPOCOInternal != null)
			{
				iterator.ViewPOCO += (sender, e) =>
				{
					ViewPOCOEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
						e.POCO
					);
					viewPOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (viewGeneratedAsyncInternal != null)
			{
				iterator.ViewGeneratedAsync += (sender, e) =>
				{
					viewGeneratedAsyncInternal.RaiseAsync(
						this,
						new ViewGeneratedAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
							e.Namespace
						)
					);
				};
			}

			if (viewGeneratedInternal != null)
			{
				iterator.ViewGenerated += (sender, e) =>
				{
					ViewGeneratedEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Views).First(v => v.InternalEquals(e.View)),
						e.Namespace
					);
					viewGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (viewsGeneratedAsyncInternal != null)
			{
				iterator.ViewsGeneratedAsync += (sender, e) =>
				{
					viewsGeneratedAsyncInternal.RaiseAsync(
						this,
						new ViewsGeneratedAsyncEventArgs()
					);
				};
			}

			if (viewsGeneratedInternal != null)
			{
				iterator.ViewsGenerated += (sender, e) =>
				{
					ViewsGeneratedEventArgs args = new();
					viewsGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (proceduresGeneratingAsyncInternal != null)
			{
				iterator.ProceduresGeneratingAsync += (sender, e) =>
				{
					proceduresGeneratingAsyncInternal.RaiseAsync(
						this,
						new ProceduresGeneratingAsyncEventArgs()
					);
				};
			}

			if (proceduresGeneratingInternal != null)
			{
				iterator.ProceduresGenerating += (sender, e) =>
				{
					ProceduresGeneratingEventArgs args = new();
					proceduresGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (procedureGeneratingAsyncInternal != null)
			{
				iterator.ProcedureGeneratingAsync += (sender, e) =>
				{
					procedureGeneratingAsyncInternal.RaiseAsync(
						this,
						new ProcedureGeneratingAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
							e.Namespace
						)
					);
				};
			}

			if (procedureGeneratingInternal != null)
			{
				iterator.ProcedureGenerating += (sender, e) =>
				{
					ProcedureGeneratingEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
						e.Namespace
					);
					procedureGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (procedurePOCOAsyncInternal != null)
			{
				iterator.ProcedurePOCOAsync += (sender, e) =>
				{
					procedurePOCOAsyncInternal.RaiseAsync(
						this,
						new ProcedurePOCOAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
							e.POCO
						)
					);
				};
			}

			if (procedurePOCOInternal != null)
			{
				iterator.ProcedurePOCO += (sender, e) =>
				{
					ProcedurePOCOEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
						e.POCO
					);
					procedurePOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (procedureGeneratedAsyncInternal != null)
			{
				iterator.ProcedureGeneratedAsync += (sender, e) =>
				{
					procedureGeneratedAsyncInternal.RaiseAsync(
						this,
						new ProcedureGeneratedAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
							e.Namespace
						)
					);
				};
			}

			if (procedureGeneratedInternal != null)
			{
				iterator.ProcedureGenerated += (sender, e) =>
				{
					ProcedureGeneratedEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Procedures).First(p => p.InternalEquals(e.Procedure)),
						e.Namespace
					);
					procedureGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (proceduresGeneratedAsyncInternal != null)
			{
				iterator.ProceduresGeneratedAsync += (sender, e) =>
				{
					proceduresGeneratedAsyncInternal.RaiseAsync(
						this,
						new ProceduresGeneratedAsyncEventArgs()
					);
				};
			}

			if (proceduresGeneratedInternal != null)
			{
				iterator.ProceduresGenerated += (sender, e) =>
				{
					ProceduresGeneratedEventArgs args = new();
					proceduresGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (functionsGeneratingAsyncInternal != null)
			{
				iterator.FunctionsGeneratingAsync += (sender, e) =>
				{
					functionsGeneratingAsyncInternal.RaiseAsync(
						this,
						new FunctionsGeneratingAsyncEventArgs()
					);
				};
			}

			if (functionsGeneratingInternal != null)
			{
				iterator.FunctionsGenerating += (sender, e) =>
				{
					FunctionsGeneratingEventArgs args = new();
					functionsGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (functionGeneratingAsyncInternal != null)
			{
				iterator.FunctionGeneratingAsync += (sender, e) =>
				{
					functionGeneratingAsyncInternal.RaiseAsync(
						this,
						new FunctionGeneratingAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
							e.Namespace
						)
					);
				};
			}

			if (functionGeneratingInternal != null)
			{
				iterator.FunctionGenerating += (sender, e) =>
				{
					FunctionGeneratingEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
						e.Namespace
					);
					functionGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (functionPOCOAsyncInternal != null)
			{
				iterator.FunctionPOCOAsync += (sender, e) =>
				{
					functionPOCOAsyncInternal.RaiseAsync(
						this,
						new FunctionPOCOAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
							e.POCO
						)
					);
				};
			}

			if (functionPOCOInternal != null)
			{
				iterator.FunctionPOCO += (sender, e) =>
				{
					FunctionPOCOEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
						e.POCO
					);
					functionPOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (functionGeneratedAsyncInternal != null)
			{
				iterator.FunctionGeneratedAsync += (sender, e) =>
				{
					functionGeneratedAsyncInternal.RaiseAsync(
						this,
						new FunctionGeneratedAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
							e.Namespace
						)
					);
				};
			}

			if (functionGeneratedInternal != null)
			{
				iterator.FunctionGenerated += (sender, e) =>
				{
					FunctionGeneratedEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.Functions).First(f => f.InternalEquals(e.Function)),
						e.Namespace
					);
					functionGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (functionsGeneratedAsyncInternal != null)
			{
				iterator.FunctionsGeneratedAsync += (sender, e) =>
				{
					functionsGeneratedAsyncInternal.RaiseAsync(
						this,
						new FunctionsGeneratedAsyncEventArgs()
					);
				};
			}

			if (functionsGeneratedInternal != null)
			{
				iterator.FunctionsGenerated += (sender, e) =>
				{
					FunctionsGeneratedEventArgs args = new();
					functionsGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (tvpsGeneratingAsyncInternal != null)
			{
				iterator.TVPsGeneratingAsync += (sender, e) =>
				{
					tvpsGeneratingAsyncInternal.RaiseAsync(
						this,
						new TVPsGeneratingAsyncEventArgs()
					);
				};
			}

			if (tvpsGeneratingInternal != null)
			{
				iterator.TVPsGenerating += (sender, e) =>
				{
					TVPsGeneratingEventArgs args = new();
					tvpsGeneratingInternal.Raise(this, args);
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (tvpGeneratingAsyncInternal != null)
			{
				iterator.TVPGeneratingAsync += (sender, e) =>
				{
					tvpGeneratingAsyncInternal.RaiseAsync(
						this,
						new TVPGeneratingAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
							e.Namespace
						)
					);
				};
			}

			if (tvpGeneratingInternal != null)
			{
				iterator.TVPGenerating += (sender, e) =>
				{
					TVPGeneratingEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
						e.Namespace
					);
					tvpGeneratingInternal.Raise(this, args);
					e.Namespace = args.Namespace;
					e.Skip = args.Skip;
					e.Stop = args.Stop;
				};
			}

			if (tvpPOCOAsyncInternal != null)
			{
				iterator.TVPPOCOAsync += (sender, e) =>
				{
					tvpPOCOAsyncInternal.RaiseAsync(
						this,
						new TVPPOCOAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
							e.POCO
						)
					);
				};
			}

			if (tvpPOCOInternal != null)
			{
				iterator.TVPPOCO += (sender, e) =>
				{
					TVPPOCOEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
						e.POCO
					);
					tvpPOCOInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (tvpGeneratedAsyncInternal != null)
			{
				iterator.TVPGeneratedAsync += (sender, e) =>
				{
					tvpGeneratedAsyncInternal.RaiseAsync(
						this,
						new TVPGeneratedAsyncEventArgs(
							proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
							e.Namespace
						)
					);
				};
			}

			if (tvpGeneratedInternal != null)
			{
				iterator.TVPGenerated += (sender, e) =>
				{
					TVPGeneratedEventArgs args = new(
						proxyServer.Databases.SelectMany(d => d.TVPs).First(t => t.InternalEquals(e.TVP)),
						e.Namespace
					);
					tvpGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (tvpsGeneratedAsyncInternal != null)
			{
				iterator.TVPsGeneratedAsync += (sender, e) =>
				{
					tvpsGeneratedAsyncInternal.RaiseAsync(
						this,
						new TVPsGeneratedAsyncEventArgs()
					);
				};
			}

			if (tvpsGeneratedInternal != null)
			{
				iterator.TVPsGenerated += (sender, e) =>
				{
					TVPsGeneratedEventArgs args = new();
					tvpsGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (databaseGeneratedAsyncInternal != null)
			{
				iterator.DatabaseGeneratedAsync += (sender, e) =>
				{
					databaseGeneratedAsyncInternal.RaiseAsync(
						this,
						new DatabaseGeneratedAsyncEventArgs(
							proxyServer.Databases.First(d => d.InternalEquals(e.Database))
						)
					);
				};
			}

			if (databaseGeneratedInternal != null)
			{
				iterator.DatabaseGenerated += (sender, e) =>
				{
					DatabaseGeneratedEventArgs args = new(
						proxyServer.Databases.First(d => d.InternalEquals(e.Database))
					);
					databaseGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}

			if (serverGeneratedAsyncInternal != null)
			{
				iterator.ServerGeneratedAsync += (sender, e) =>
				{
					serverGeneratedAsyncInternal.RaiseAsync(
						this,
						new ServerGeneratedAsyncEventArgs(
							proxyServer
						)
					);
				};
			}

			if (serverGeneratedInternal != null)
			{
				iterator.ServerGenerated += (sender, e) =>
				{
					ServerGeneratedEventArgs args = new(
						proxyServer
					);
					serverGeneratedInternal.Raise(this, args);
					e.Stop = args.Stop;
				};
			}
		}

		#endregion

		#region Disclaimer

		public string Disclaimer => POCOGenerator.Disclaimer.Message;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using POCOGenerator.DbObjects;
using POCOGenerator.POCOWriters;
using POCOGenerator.Utils;

namespace POCOGenerator.POCOIterators
{
	public abstract class DbIterator(IWriter writer,
									 IDbSupport support,
									 IDbIteratorSettings settings)
		: IDbIterator
	{
		protected IWriter Writer { get; } = writer;
		protected IDbSupport Support { get; } = support;
		protected IDbIteratorSettings Settings { get; } = settings;

		// in order of execution
		public event EventHandler<ServerGeneratingAsyncEventArgs> ServerGeneratingAsync;
		public event EventHandler<ServerGeneratingEventArgs> ServerGenerating;
		public event EventHandler<DatabaseGeneratingAsyncEventArgs> DatabaseGeneratingAsync;
		public event EventHandler<DatabaseGeneratingEventArgs> DatabaseGenerating;
		public event EventHandler<TablesGeneratingAsyncEventArgs> TablesGeneratingAsync;
		public event EventHandler<TablesGeneratingEventArgs> TablesGenerating;
		public event EventHandler<TableGeneratingAsyncEventArgs> TableGeneratingAsync;
		public event EventHandler<TableGeneratingEventArgs> TableGenerating;
		public event EventHandler<TablePOCOAsyncEventArgs> TablePOCOAsync;
		public event EventHandler<TablePOCOEventArgs> TablePOCO;
		public event EventHandler<TableGeneratedAsyncEventArgs> TableGeneratedAsync;
		public event EventHandler<TableGeneratedEventArgs> TableGenerated;
		public event EventHandler<TablesGeneratedAsyncEventArgs> TablesGeneratedAsync;
		public event EventHandler<TablesGeneratedEventArgs> TablesGenerated;
		public event EventHandler<ComplexTypeTablesGeneratingAsyncEventArgs> ComplexTypeTablesGeneratingAsync;
		public event EventHandler<ComplexTypeTablesGeneratingEventArgs> ComplexTypeTablesGenerating;
		public event EventHandler<ComplexTypeTableGeneratingAsyncEventArgs> ComplexTypeTableGeneratingAsync;
		public event EventHandler<ComplexTypeTableGeneratingEventArgs> ComplexTypeTableGenerating;
		public event EventHandler<ComplexTypeTablePOCOAsyncEventArgs> ComplexTypeTablePOCOAsync;
		public event EventHandler<ComplexTypeTablePOCOEventArgs> ComplexTypeTablePOCO;
		public event EventHandler<ComplexTypeTableGeneratedAsyncEventArgs> ComplexTypeTableGeneratedAsync;
		public event EventHandler<ComplexTypeTableGeneratedEventArgs> ComplexTypeTableGenerated;
		public event EventHandler<ComplexTypeTablesGeneratedAsyncEventArgs> ComplexTypeTablesGeneratedAsync;
		public event EventHandler<ComplexTypeTablesGeneratedEventArgs> ComplexTypeTablesGenerated;
		public event EventHandler<ViewsGeneratingAsyncEventArgs> ViewsGeneratingAsync;
		public event EventHandler<ViewsGeneratingEventArgs> ViewsGenerating;
		public event EventHandler<ViewGeneratingAsyncEventArgs> ViewGeneratingAsync;
		public event EventHandler<ViewGeneratingEventArgs> ViewGenerating;
		public event EventHandler<ViewPOCOAsyncEventArgs> ViewPOCOAsync;
		public event EventHandler<ViewPOCOEventArgs> ViewPOCO;
		public event EventHandler<ViewGeneratedAsyncEventArgs> ViewGeneratedAsync;
		public event EventHandler<ViewGeneratedEventArgs> ViewGenerated;
		public event EventHandler<ViewsGeneratedAsyncEventArgs> ViewsGeneratedAsync;
		public event EventHandler<ViewsGeneratedEventArgs> ViewsGenerated;
		public event EventHandler<ProceduresGeneratingAsyncEventArgs> ProceduresGeneratingAsync;
		public event EventHandler<ProceduresGeneratingEventArgs> ProceduresGenerating;
		public event EventHandler<ProcedureGeneratingAsyncEventArgs> ProcedureGeneratingAsync;
		public event EventHandler<ProcedureGeneratingEventArgs> ProcedureGenerating;
		public event EventHandler<ProcedurePOCOAsyncEventArgs> ProcedurePOCOAsync;
		public event EventHandler<ProcedurePOCOEventArgs> ProcedurePOCO;
		public event EventHandler<ProcedureGeneratedAsyncEventArgs> ProcedureGeneratedAsync;
		public event EventHandler<ProcedureGeneratedEventArgs> ProcedureGenerated;
		public event EventHandler<ProceduresGeneratedAsyncEventArgs> ProceduresGeneratedAsync;
		public event EventHandler<ProceduresGeneratedEventArgs> ProceduresGenerated;
		public event EventHandler<FunctionsGeneratingAsyncEventArgs> FunctionsGeneratingAsync;
		public event EventHandler<FunctionsGeneratingEventArgs> FunctionsGenerating;
		public event EventHandler<FunctionGeneratingAsyncEventArgs> FunctionGeneratingAsync;
		public event EventHandler<FunctionGeneratingEventArgs> FunctionGenerating;
		public event EventHandler<FunctionPOCOAsyncEventArgs> FunctionPOCOAsync;
		public event EventHandler<FunctionPOCOEventArgs> FunctionPOCO;
		public event EventHandler<FunctionGeneratedAsyncEventArgs> FunctionGeneratedAsync;
		public event EventHandler<FunctionGeneratedEventArgs> FunctionGenerated;
		public event EventHandler<FunctionsGeneratedAsyncEventArgs> FunctionsGeneratedAsync;
		public event EventHandler<FunctionsGeneratedEventArgs> FunctionsGenerated;
		public event EventHandler<TVPsGeneratingAsyncEventArgs> TVPsGeneratingAsync;
		public event EventHandler<TVPsGeneratingEventArgs> TVPsGenerating;
		public event EventHandler<TVPGeneratingAsyncEventArgs> TVPGeneratingAsync;
		public event EventHandler<TVPGeneratingEventArgs> TVPGenerating;
		public event EventHandler<TVPPOCOAsyncEventArgs> TVPPOCOAsync;
		public event EventHandler<TVPPOCOEventArgs> TVPPOCO;
		public event EventHandler<TVPGeneratedAsyncEventArgs> TVPGeneratedAsync;
		public event EventHandler<TVPGeneratedEventArgs> TVPGenerated;
		public event EventHandler<TVPsGeneratedAsyncEventArgs> TVPsGeneratedAsync;
		public event EventHandler<TVPsGeneratedEventArgs> TVPsGenerated;
		public event EventHandler<DatabaseGeneratedAsyncEventArgs> DatabaseGeneratedAsync;
		public event EventHandler<DatabaseGeneratedEventArgs> DatabaseGenerated;
		public event EventHandler<ServerGeneratedAsyncEventArgs> ServerGeneratedAsync;
		public event EventHandler<ServerGeneratedEventArgs> ServerGenerated;

		public void Iterate(IEnumerable<IDbObjectTraverse> dbObjects)
		{
			Clear();

			if (dbObjects == null || !dbObjects.Any())
			{
				return;
			}

			bool isExistDbObject = dbObjects.Any(o => o.Error == null);

			string namespaceOffset = ItereatorStart(isExistDbObject, dbObjects);

			bool isFirstDbObject = true;

			if (IterateServers(dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
			{
				return;
			}

			IteratorEnd(isExistDbObject);
		}

		protected virtual bool IterateServers(IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IGrouping<IServer, IDbObjectTraverse>> servers = dbObjects.GroupBy(x => x.Database.Server).OrderBy(x => x.Key.ToString());
			foreach (IGrouping<IServer, IDbObjectTraverse> server in servers)
			{
				ServerGeneratingAsync.RaiseAsync(this, () => new ServerGeneratingAsyncEventArgs(server.Key));

				ServerGeneratingEventArgs argsServerGenerating = ServerGenerating.Raise(this, () => new ServerGeneratingEventArgs(server.Key));
				if (argsServerGenerating != null && argsServerGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (IterateDatabases(server, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
				{
					return true;
				}

				ServerGeneratedAsync.RaiseAsync(this, () => new ServerGeneratedAsyncEventArgs(server.Key));

				ServerGeneratedEventArgs argsServerGenerated = ServerGenerated.Raise(this, () => new ServerGeneratedEventArgs(server.Key));
				if (argsServerGenerated != null && argsServerGenerated.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}
			}

			return false;
		}

		protected virtual bool IterateDatabases(IGrouping<IServer, IDbObjectTraverse> server, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IGrouping<IDatabase, IDbObjectTraverse>> databases = server.GroupBy(x => x.Database).OrderBy(x => x.Key.ToString());
			foreach (IGrouping<IDatabase, IDbObjectTraverse> database in databases)
			{
				DatabaseGeneratingAsync.RaiseAsync(this, () => new DatabaseGeneratingAsyncEventArgs(database.Key));

				DatabaseGeneratingEventArgs argsDatabaseGenerating = DatabaseGenerating.Raise(this, () => new DatabaseGeneratingEventArgs(database.Key));
				if (argsDatabaseGenerating is { Stop: true })
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsDatabaseGenerating == null || !argsDatabaseGenerating.Skip)
				{
					if (IterateTables(database, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}

					if (IterateViews(database, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}

					if (IterateProcedures(database, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}

					if (IterateFunctions(database, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}

					if (IterateTVPs(database, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}

					DatabaseGeneratedAsync.RaiseAsync(this, () => new DatabaseGeneratedAsyncEventArgs(database.Key));

					DatabaseGeneratedEventArgs argsDatabaseGenerated = DatabaseGenerated.Raise(this, () => new DatabaseGeneratedEventArgs(database.Key));
					if (argsDatabaseGenerated != null && argsDatabaseGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateTables(IGrouping<IDatabase, IDbObjectTraverse> database, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IDbObjectTraverse> tables = database.Where(x => x.DbObjectType == DbObjectType.Table).OrderBy(x => x.ToString());
			if (tables.Any())
			{
				TablesGeneratingAsync.RaiseAsync(this, () => new TablesGeneratingAsyncEventArgs());

				TablesGeneratingEventArgs argsTablesGenerating = TablesGenerating.Raise(this, () => new TablesGeneratingEventArgs());
				if (argsTablesGenerating != null && argsTablesGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsTablesGenerating == null || !argsTablesGenerating.Skip)
				{
					List<IComplexTypeTable> complexTypeTables = null;

					foreach (IDbObjectTraverse table in tables)
					{
						// don't write join table
						if (((ITable)table).IsJoinTable && !Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable)
						{
							continue;
						}

						// collect complex type tables
						if (Settings.POCOIteratorSettings.ComplexTypes && ((ITable)table).ComplexTypeTables.HasAny())
						{
							if (complexTypeTables == null)
							{
								complexTypeTables = new List<IComplexTypeTable>(((ITable)table).ComplexTypeTables);
							}
							else
							{
								foreach (IComplexTypeTable ctt in ((ITable)table).ComplexTypeTables)
								{
									if (!complexTypeTables.Contains(ctt))
									{
										complexTypeTables.Add(ctt);
									}
								}
							}
						}

						bool stop = WriteDbObject(dbObjects, table, namespaceOffset, RaiseTableGeneratingEvent, RaiseTableGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					TablesGeneratedAsync.RaiseAsync(this, () => new TablesGeneratedAsyncEventArgs());

					TablesGeneratedEventArgs argsTablesGenerated = TablesGenerated.Raise(this, () => new TablesGeneratedEventArgs());
					if (argsTablesGenerated != null && argsTablesGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}

					if (IterateComplexTypeTables(complexTypeTables, dbObjects, namespaceOffset, ref isFirstDbObject, isExistDbObject))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateComplexTypeTables(List<IComplexTypeTable> complexTypeTables, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			if (complexTypeTables.HasAny())
			{
				ComplexTypeTablesGeneratingAsync.RaiseAsync(this, () => new ComplexTypeTablesGeneratingAsyncEventArgs());

				ComplexTypeTablesGeneratingEventArgs argsComplexTypeTablesGenerating = ComplexTypeTablesGenerating.Raise(this, () => new ComplexTypeTablesGeneratingEventArgs());
				if (argsComplexTypeTablesGenerating != null && argsComplexTypeTablesGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsComplexTypeTablesGenerating == null || !argsComplexTypeTablesGenerating.Skip)
				{
					foreach (IComplexTypeTable complexTypeTable in complexTypeTables)
					{
						bool stop = WriteDbObject(dbObjects, complexTypeTable, namespaceOffset, RaiseComplexTypeTableGeneratingEvent, RaiseComplexTypeTableGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					ComplexTypeTablesGeneratedAsync.RaiseAsync(this, () => new ComplexTypeTablesGeneratedAsyncEventArgs());

					ComplexTypeTablesGeneratedEventArgs argsComplexTypeTablesGenerated = ComplexTypeTablesGenerated.Raise(this, () => new ComplexTypeTablesGeneratedEventArgs());
					if (argsComplexTypeTablesGenerated != null && argsComplexTypeTablesGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateViews(IGrouping<IDatabase, IDbObjectTraverse> database, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IDbObjectTraverse> views = database.Where(x => x.DbObjectType == DbObjectType.View).OrderBy(x => x.ToString());
			if (views.Any())
			{
				ViewsGeneratingAsync.RaiseAsync(this, () => new ViewsGeneratingAsyncEventArgs());

				ViewsGeneratingEventArgs argsViewsGenerating = ViewsGenerating.Raise(this, () => new ViewsGeneratingEventArgs());
				if (argsViewsGenerating != null && argsViewsGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsViewsGenerating == null || !argsViewsGenerating.Skip)
				{
					foreach (IDbObjectTraverse view in views)
					{
						bool stop = WriteDbObject(dbObjects, view, namespaceOffset, RaiseViewGeneratingEvent, RaiseViewGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					ViewsGeneratedAsync.RaiseAsync(this, () => new ViewsGeneratedAsyncEventArgs());

					ViewsGeneratedEventArgs argsViewsGenerated = ViewsGenerated.Raise(this, () => new ViewsGeneratedEventArgs());
					if (argsViewsGenerated != null && argsViewsGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateProcedures(IGrouping<IDatabase, IDbObjectTraverse> database, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IDbObjectTraverse> procedures = database.Where(x => x.DbObjectType == DbObjectType.Procedure).OrderBy(x => x.ToString());
			if (procedures.Any())
			{
				ProceduresGeneratingAsync.RaiseAsync(this, () => new ProceduresGeneratingAsyncEventArgs());

				ProceduresGeneratingEventArgs argsProceduresGenerating = ProceduresGenerating.Raise(this, () => new ProceduresGeneratingEventArgs());
				if (argsProceduresGenerating != null && argsProceduresGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsProceduresGenerating == null || !argsProceduresGenerating.Skip)
				{
					foreach (IDbObjectTraverse procedure in procedures)
					{
						bool stop = WriteDbObject(dbObjects, procedure, namespaceOffset, RaiseProcedureGeneratingEvent, RaiseProcedureGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					ProceduresGeneratedAsync.RaiseAsync(this, () => new ProceduresGeneratedAsyncEventArgs());

					ProceduresGeneratedEventArgs argsProceduresGenerated = ProceduresGenerated.Raise(this, () => new ProceduresGeneratedEventArgs());
					if (argsProceduresGenerated != null && argsProceduresGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateFunctions(IGrouping<IDatabase, IDbObjectTraverse> database, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IDbObjectTraverse> functions = database.Where(x => x.DbObjectType == DbObjectType.Function).OrderBy(x => x.ToString());
			if (functions.Any())
			{
				FunctionsGeneratingAsync.RaiseAsync(this, () => new FunctionsGeneratingAsyncEventArgs());

				FunctionsGeneratingEventArgs argsFunctionsGenerating = FunctionsGenerating.Raise(this, () => new FunctionsGeneratingEventArgs());
				if (argsFunctionsGenerating != null && argsFunctionsGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsFunctionsGenerating == null || !argsFunctionsGenerating.Skip)
				{
					foreach (IDbObjectTraverse function in functions)
					{
						bool stop = WriteDbObject(dbObjects, function, namespaceOffset, RaiseFunctionGeneratingEvent, RaiseFunctionGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					FunctionsGeneratedAsync.RaiseAsync(this, () => new FunctionsGeneratedAsyncEventArgs());

					FunctionsGeneratedEventArgs argsFunctionsGenerated = FunctionsGenerated.Raise(this, () => new FunctionsGeneratedEventArgs());
					if (argsFunctionsGenerated != null && argsFunctionsGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IterateTVPs(IGrouping<IDatabase, IDbObjectTraverse> database, IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset, ref bool isFirstDbObject, bool isExistDbObject)
		{
			IOrderedEnumerable<IDbObjectTraverse> tvps = database.Where(x => x.DbObjectType == DbObjectType.TVP).OrderBy(x => x.ToString());
			if (tvps.Any())
			{
				TVPsGeneratingAsync.RaiseAsync(this, () => new TVPsGeneratingAsyncEventArgs());

				TVPsGeneratingEventArgs argsTVPsGenerating = TVPsGenerating.Raise(this, () => new TVPsGeneratingEventArgs());
				if (argsTVPsGenerating != null && argsTVPsGenerating.Stop)
				{
					IteratorEnd(isExistDbObject);
					return true;
				}

				if (argsTVPsGenerating == null || !argsTVPsGenerating.Skip)
				{
					foreach (IDbObjectTraverse tvp in tvps)
					{
						bool stop = WriteDbObject(dbObjects, tvp, namespaceOffset, RaiseTVPGeneratingEvent, RaiseTVPGeneratedEvent, ref isFirstDbObject);
						if (stop)
						{
							IteratorEnd(isExistDbObject);
							return true;
						}
					}

					TVPsGeneratedAsync.RaiseAsync(this, () => new TVPsGeneratedAsyncEventArgs());

					TVPsGeneratedEventArgs argsTVPsGenerated = TVPsGenerated.Raise(this, () => new TVPsGeneratedEventArgs());
					if (argsTVPsGenerated != null && argsTVPsGenerated.Stop)
					{
						IteratorEnd(isExistDbObject);
						return true;
					}
				}
			}

			return false;
		}

		protected virtual string ItereatorStart(bool isExistDbObject, IEnumerable<IDbObjectTraverse> dbObjects)
		{
			string namespaceOffset = String.Empty;

			if (isExistDbObject)
			{
				if (!Settings.POCOIteratorSettings.WrapAroundEachClass)
				{
					// Using
					if (!Settings.POCOIteratorSettings.UsingInsideNamespace)
					{
						WriteUsing(dbObjects, namespaceOffset);
					}

					// Namespace Start
					namespaceOffset = WriteNamespaceStart(Settings.POCOIteratorSettings.Namespace);

					// Using
					if (Settings.POCOIteratorSettings.UsingInsideNamespace)
					{
						WriteUsing(dbObjects, namespaceOffset);
					}
				}
			}

			return namespaceOffset;
		}

		protected virtual void IteratorEnd(bool isExistDbObject)
		{
			if (isExistDbObject)
			{
				if (!Settings.POCOIteratorSettings.WrapAroundEachClass)
				{
					// Namespace End
					WriteNamespaceEnd(Settings.POCOIteratorSettings.Namespace);
				}
			}
		}

		protected virtual bool WriteDbObject(
			IEnumerable<IDbObjectTraverse> dbObjects,
			IDbObjectTraverse dbObject,
			string namespaceOffset,
			RaiseGeneratingEventHandler RaiseGeneratingEvent,
			RaiseGeneratedEventHandler RaiseGeneratedEvent,
			ref bool isFirstDbObject)
		{
			// Class Name
			string className = GetClassName(
				dbObject.Database.ToString(),
				dbObject is ISchema schema ? schema.Schema : null,
				dbObject.Name,
				dbObject.DbObjectType
			);
			dbObject.ClassName = className;

			string @namespace = Settings.POCOIteratorSettings.Namespace;
			bool skip = false;
			bool stop = RaiseGeneratingEvent(dbObject, ref @namespace, ref skip);
			if (stop)
			{
				return true;
			}

			if (skip)
			{
				return false;
			}

			if (!isFirstDbObject)
			{
				Writer.WriteLine();
			}

			isFirstDbObject = false;

			if (dbObject.Error != null)
			{
				// Write Error
				WriteError(dbObject, namespaceOffset);
			}
			else
			{
				// Write Class
				WriteDbObjectClass(dbObjects, dbObject, @namespace, namespaceOffset, className);
			}

			return RaiseGeneratedEvent(dbObject, @namespace);
		}

		protected virtual void WriteDbObjectClass(IEnumerable<IDbObjectTraverse> dbObjects, IDbObjectTraverse dbObject, string @namespace, string namespaceOffset, string className)
		{
			// Enums
			List<IEnumColumn> enumColumns = GetEnumColumns(dbObject);

			// Navigation Properties
			List<INavigationProperty> navigationProperties = GetNavigationProperties(dbObject);

			if (Settings.POCOIteratorSettings.WrapAroundEachClass)
			{
				// Using
				if (!Settings.POCOIteratorSettings.UsingInsideNamespace)
				{
					WriteUsing(dbObject, namespaceOffset);
				}

				// Namespace Start
				namespaceOffset = WriteNamespaceStart(@namespace);

				// Using
				if (Settings.POCOIteratorSettings.UsingInsideNamespace)
				{
					WriteUsing(dbObject, namespaceOffset);
				}
			}

			// Class Attributes
			WriteClassAttributes(dbObject, namespaceOffset);

			// Class Start
			WriteClassStart(className, dbObject, namespaceOffset);

			// Constructor
			WriteConstructor(className, enumColumns, navigationProperties, dbObject, namespaceOffset);

			// Columns
			if (dbObject.Columns.HasAny())
			{
				IOrderedEnumerable<IColumn> columns = dbObject.Columns.OrderBy(c => c.ColumnOrdinal ?? 0);

				if (Settings.POCOIteratorSettings.ComplexTypes &&
					dbObject.DbObjectType == DbObjectType.Table &&
					((ITable)dbObject).ComplexTypeTables.HasAny())
				{
					ITable table = (ITable)dbObject;

					// for each group of columns, that are mapped to a complex type,
					// print just the first one, from each group, and ignore the rest of them
					IEnumerable<ITableColumn> toIgnore = table.TableColumns
						.Where(c => c.ComplexTypeTableColumn != null)
						.GroupBy(c => (
							c.ComplexTypeTableColumn.ComplexTypeTable,
							PropertyName: c.ColumnName[..c.ColumnName.IndexOf('_')]
						))
						.SelectMany(g => g.Except([g.First()]));

					ITableColumn[] tableColumns = table.TableColumns.Except(toIgnore).ToArray();

					ITableColumn lastColumn = tableColumns.Last();
					foreach (ITableColumn column in tableColumns)
					{
						bool isComplexTypeTableColumn = column.ComplexTypeTableColumn != null;
						WriteColumn(column, isComplexTypeTableColumn, column == lastColumn, dbObject, namespaceOffset);
					}
				}
				else
				{
					IColumn lastColumn = columns.Last();
					foreach (IColumn column in columns)
					{
						WriteColumn(column, false, column == lastColumn, dbObject, namespaceOffset);
					}
				}
			}

			// Enums
			if (!Settings.POCOIteratorSettings.EnumSQLTypeToString && (Settings.POCOIteratorSettings.EnumSQLTypeToEnumUShort || Settings.POCOIteratorSettings.EnumSQLTypeToEnumInt))
			{
				if (enumColumns.HasAny())
				{
					IEnumColumn lastEnumColumn = enumColumns.Last();
					foreach (IEnumColumn enumColumn in enumColumns)
					{
						WriteEnum(enumColumn, enumColumn == lastEnumColumn, dbObject, namespaceOffset);
					}
				}
			}

			// Navigation Properties
			WriteNavigationProperties(navigationProperties, dbObject, namespaceOffset);

			// Class End
			WriteClassEnd(dbObject, namespaceOffset);

			if (Settings.POCOIteratorSettings.WrapAroundEachClass)
			{
				// Namespace End
				WriteNamespaceEnd(@namespace);
			}
		}

		protected delegate bool RaiseGeneratingEventHandler(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip);

		protected virtual bool RaiseTableGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			TableGeneratingAsync.RaiseAsync(this, () => new TableGeneratingAsyncEventArgs((ITable)dbObject, argsNamespace));

			TableGeneratingEventArgs argsTable = TableGenerating.Raise(this, () => new TableGeneratingEventArgs((ITable)dbObject, argsNamespace));

			if (argsTable != null)
			{
				if (argsTable.Stop)
				{
					return true;
				}

				skip = argsTable.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsTable.Namespace;
			}

			if (TablePOCO != null || TablePOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected virtual bool RaiseComplexTypeTableGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			ComplexTypeTableGeneratingAsync.RaiseAsync(this, () => new ComplexTypeTableGeneratingAsyncEventArgs((IComplexTypeTable)dbObject, argsNamespace));

			ComplexTypeTableGeneratingEventArgs argsComplexTypeTable = ComplexTypeTableGenerating.Raise(this, () => new ComplexTypeTableGeneratingEventArgs((IComplexTypeTable)dbObject, argsNamespace));

			if (argsComplexTypeTable != null)
			{
				if (argsComplexTypeTable.Stop)
				{
					return true;
				}

				skip = argsComplexTypeTable.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsComplexTypeTable.Namespace;
			}

			if (ComplexTypeTablePOCO != null || ComplexTypeTablePOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected virtual bool RaiseViewGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			ViewGeneratingAsync.RaiseAsync(this, () => new ViewGeneratingAsyncEventArgs((IView)dbObject, argsNamespace));

			ViewGeneratingEventArgs argsView = ViewGenerating.Raise(this, () => new ViewGeneratingEventArgs((IView)dbObject, argsNamespace));

			if (argsView != null)
			{
				if (argsView.Stop)
				{
					return true;
				}

				skip = argsView.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsView.Namespace;
			}

			if (ViewPOCO != null || ViewPOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected virtual bool RaiseProcedureGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			ProcedureGeneratingAsync.RaiseAsync(this, () => new ProcedureGeneratingAsyncEventArgs((IProcedure)dbObject, argsNamespace));

			ProcedureGeneratingEventArgs argsProcedure = ProcedureGenerating.Raise(this, () => new ProcedureGeneratingEventArgs((IProcedure)dbObject, argsNamespace));

			if (argsProcedure != null)
			{
				if (argsProcedure.Stop)
				{
					return true;
				}

				skip = argsProcedure.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsProcedure.Namespace;
			}

			if (ProcedurePOCO != null || ProcedurePOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected virtual bool RaiseFunctionGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			FunctionGeneratingAsync.RaiseAsync(this, () => new FunctionGeneratingAsyncEventArgs((IFunction)dbObject, argsNamespace));

			FunctionGeneratingEventArgs argsFunction = FunctionGenerating.Raise(this, () => new FunctionGeneratingEventArgs((IFunction)dbObject, argsNamespace));

			if (argsFunction != null)
			{
				if (argsFunction.Stop)
				{
					return true;
				}

				skip = argsFunction.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsFunction.Namespace;
			}

			if (FunctionPOCO != null || FunctionPOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected virtual bool RaiseTVPGeneratingEvent(IDbObjectTraverse dbObject, ref string @namespace, ref bool skip)
		{
			string argsNamespace = @namespace;

			TVPGeneratingAsync.RaiseAsync(this, () => new TVPGeneratingAsyncEventArgs((ITVP)dbObject, argsNamespace));

			TVPGeneratingEventArgs argsTVP = TVPGenerating.Raise(this, () => new TVPGeneratingEventArgs((ITVP)dbObject, argsNamespace));

			if (argsTVP != null)
			{
				if (argsTVP.Stop)
				{
					return true;
				}

				skip = argsTVP.Skip;
				if (skip)
				{
					return false;
				}

				@namespace = argsTVP.Namespace;
			}

			if (TVPPOCO != null || TVPPOCOAsync != null)
			{
				Writer.StartSnapshot();
			}

			return false;
		}

		protected delegate bool RaiseGeneratedEventHandler(IDbObjectTraverse dbObject, string @namespace);

		protected virtual bool RaiseTableGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (TablePOCO != null || TablePOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				TablePOCOAsync.RaiseAsync(this, () => new TablePOCOAsyncEventArgs((ITable)dbObject, poco));

				TablePOCOEventArgs argsPOCO = TablePOCO.Raise(this, () => new TablePOCOEventArgs((ITable)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			TableGeneratedAsync.RaiseAsync(this, () => new TableGeneratedAsyncEventArgs((ITable)dbObject, @namespace));

			TableGeneratedEventArgs argsTable = TableGenerated.Raise(this, () => new TableGeneratedEventArgs((ITable)dbObject, @namespace));
			return argsTable != null && argsTable.Stop;
		}

		protected virtual bool RaiseComplexTypeTableGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (ComplexTypeTablePOCO != null || ComplexTypeTablePOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				ComplexTypeTablePOCOAsync.RaiseAsync(this, () => new ComplexTypeTablePOCOAsyncEventArgs((IComplexTypeTable)dbObject, poco));

				ComplexTypeTablePOCOEventArgs argsPOCO = ComplexTypeTablePOCO.Raise(this, () => new ComplexTypeTablePOCOEventArgs((IComplexTypeTable)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			ComplexTypeTableGeneratedAsync.RaiseAsync(this, () => new ComplexTypeTableGeneratedAsyncEventArgs((IComplexTypeTable)dbObject, @namespace));

			ComplexTypeTableGeneratedEventArgs argsComplexTypeTable = ComplexTypeTableGenerated.Raise(this, () => new ComplexTypeTableGeneratedEventArgs((IComplexTypeTable)dbObject, @namespace));
			return argsComplexTypeTable != null && argsComplexTypeTable.Stop;
		}

		protected virtual bool RaiseViewGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (ViewPOCO != null || ViewPOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				ViewPOCOAsync.RaiseAsync(this, () => new ViewPOCOAsyncEventArgs((IView)dbObject, poco));

				ViewPOCOEventArgs argsPOCO = ViewPOCO.Raise(this, () => new ViewPOCOEventArgs((IView)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			ViewGeneratedAsync.RaiseAsync(this, () => new ViewGeneratedAsyncEventArgs((IView)dbObject, @namespace));

			ViewGeneratedEventArgs argsView = ViewGenerated.Raise(this, () => new ViewGeneratedEventArgs((IView)dbObject, @namespace));
			return argsView != null && argsView.Stop;
		}

		protected virtual bool RaiseProcedureGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (ProcedurePOCO != null || ProcedurePOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				ProcedurePOCOAsync.RaiseAsync(this, () => new ProcedurePOCOAsyncEventArgs((IProcedure)dbObject, poco));

				ProcedurePOCOEventArgs argsPOCO = ProcedurePOCO.Raise(this, () => new ProcedurePOCOEventArgs((IProcedure)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			ProcedureGeneratedAsync.RaiseAsync(this, () => new ProcedureGeneratedAsyncEventArgs((IProcedure)dbObject, @namespace));

			ProcedureGeneratedEventArgs argsProcedure = ProcedureGenerated.Raise(this, () => new ProcedureGeneratedEventArgs((IProcedure)dbObject, @namespace));
			return argsProcedure != null && argsProcedure.Stop;
		}

		protected virtual bool RaiseFunctionGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (FunctionPOCO != null || FunctionPOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				FunctionPOCOAsync.RaiseAsync(this, () => new FunctionPOCOAsyncEventArgs((IFunction)dbObject, poco));

				FunctionPOCOEventArgs argsPOCO = FunctionPOCO.Raise(this, () => new FunctionPOCOEventArgs((IFunction)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			FunctionGeneratedAsync.RaiseAsync(this, () => new FunctionGeneratedAsyncEventArgs((IFunction)dbObject, @namespace));

			FunctionGeneratedEventArgs argsFunction = FunctionGenerated.Raise(this, () => new FunctionGeneratedEventArgs((IFunction)dbObject, @namespace));
			return argsFunction != null && argsFunction.Stop;
		}

		protected virtual bool RaiseTVPGeneratedEvent(IDbObjectTraverse dbObject, string @namespace)
		{
			if (TVPPOCO != null || TVPPOCOAsync != null)
			{
				string poco = Writer.EndSnapshot().ToString().Trim();

				TVPPOCOAsync.RaiseAsync(this, () => new TVPPOCOAsyncEventArgs((ITVP)dbObject, poco));

				TVPPOCOEventArgs argsPOCO = TVPPOCO.Raise(this, () => new TVPPOCOEventArgs((ITVP)dbObject, poco));
				if (argsPOCO != null && argsPOCO.Stop)
				{
					return true;
				}
			}

			TVPGeneratedAsync.RaiseAsync(this, () => new TVPGeneratedAsyncEventArgs((ITVP)dbObject, @namespace));

			TVPGeneratedEventArgs argsTVP = TVPGenerated.Raise(this, () => new TVPGeneratedEventArgs((ITVP)dbObject, @namespace));
			return argsTVP != null && argsTVP.Stop;
		}

		protected virtual void Clear() => Writer.Clear();

		protected virtual string DefaultSchema => Support.DefaultSchema;

		protected virtual void WriteUsing(IDbObjectTraverse dbObject, string namespaceOffset) => WriteUsing(new IDbObjectTraverse[] { dbObject }, namespaceOffset);

		protected virtual void WriteUsing(IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset)
		{
			if (Settings.POCOIteratorSettings.Using)
			{
				WriteUsingClause(dbObjects, namespaceOffset);
				WriteEFUsingClause(dbObjects, namespaceOffset);
				Writer.WriteLine();
			}
		}

		protected virtual void WriteUsingClause(IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.WriteKeyword("using");
			Writer.WriteLine(" System;");

			if (Settings.NavigationPropertiesIteratorSettings.Enable)
			{
				Writer.Write(namespaceOffset);
				Writer.WriteKeyword("using");
				Writer.WriteLine(" System.Collections.Generic;");
			}

			if (HasSpecialSQLTypes(dbObjects))
			{
				WriteSpecialSQLTypesUsingClause(namespaceOffset);
			}
		}

		protected virtual bool HasSpecialSQLTypes(IEnumerable<IDbObjectTraverse> dbObjects)
		{
			if (dbObjects.HasAny())
			{
				foreach (IDbObjectTraverse dbObject in dbObjects)
				{
					if (dbObject.Columns.HasAny())
					{
						foreach (IColumn column in dbObject.Columns)
						{
							string dataTypeName = (column.DataTypeName ?? String.Empty).ToLower();
							if (IsSQLTypeRDBMSSpecificType(dataTypeName))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		protected abstract void WriteSpecialSQLTypesUsingClause(string namespaceOffset);

		protected virtual void WriteEFUsingClause(IEnumerable<IDbObjectTraverse> dbObjects, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Enable)
			{
				if (dbObjects.HasAny())
				{
					if (dbObjects.Any(o => o.DbObjectType == DbObjectType.Table))
					{
						if (Settings.EFAnnotationsIteratorSettings.Description)
						{
							Writer.Write(namespaceOffset);
							Writer.WriteKeyword("using");
							Writer.WriteLine(" System.ComponentModel;");
						}

						Writer.Write(namespaceOffset);
						Writer.WriteKeyword("using");
						Writer.WriteLine(" System.ComponentModel.DataAnnotations;");

						Writer.Write(namespaceOffset);
						Writer.WriteKeyword("using");
						Writer.WriteLine(" System.ComponentModel.DataAnnotations.Schema;");
					}
				}
			}
		}

		protected virtual string WriteNamespaceStart(string @namespace)
		{
			string namespaceOffset = String.Empty;

			if (!String.IsNullOrEmpty(@namespace))
			{
				WriteNamespaceStartClause(@namespace);
				namespaceOffset = Settings.POCOIteratorSettings.Tab;
			}

			return namespaceOffset;
		}

		protected virtual void WriteNamespaceStartClause(string @namespace)
		{
			Writer.WriteKeyword("namespace");
			Writer.Write(" ");
			Writer.WriteLine(@namespace);
			Writer.WriteLine("{");
		}

		protected virtual void WriteError(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.WriteLineError("/*");

			Writer.Write(namespaceOffset);
			Writer.WriteLineError(String.Format("{0}.{1}", dbObject.Database.ToString(), dbObject.ToString()));

			Exception currentError = dbObject.Error;
			while (currentError != null)
			{
				Writer.Write(namespaceOffset);
				Writer.WriteLineError(currentError.Message);
				currentError = currentError.InnerException;
			}

			Writer.Write(namespaceOffset);
			Writer.WriteLineError("*/");
		}

		protected virtual void WriteClassAttributes(IDbObjectTraverse dbObject, string namespaceOffset) => WriteEFClassAttributes(dbObject, namespaceOffset);

		protected virtual void WriteEFClassAttributes(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Enable)
			{
				if (dbObject.DbObjectType == DbObjectType.Table)
				{
					WriteEFTable(dbObject, namespaceOffset);
				}
				else if (dbObject.DbObjectType == DbObjectType.ComplexTypeTable)
				{
					if (Settings.EFAnnotationsIteratorSettings.ComplexType)
					{
						WriteEFComplexType(dbObject, namespaceOffset);
					}
				}

				if (Settings.EFAnnotationsIteratorSettings.Description && dbObject is IDescription descObject)
				{
					if (!String.IsNullOrEmpty(descObject.Description))
					{
						WriteEFDescription(descObject.Description, false, namespaceOffset);
					}
				}
			}
		}

		protected virtual void WriteEFTable(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write("[");
			Writer.WriteUserType("Table");
			Writer.Write("(");
			Writer.WriteString("\"");

			if (dbObject is ISchema schema)
			{
				if (!String.IsNullOrEmpty(schema.Schema))
				{
					if (schema.Schema != DefaultSchema)
					{
						Writer.WriteString(schema.Schema);
						Writer.WriteString(".");
					}
				}
			}

			Writer.WriteString(dbObject.Name);
			Writer.WriteString("\"");
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFComplexType(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write("[");
			Writer.WriteUserType("ComplexType");
			Writer.WriteLine("]");
		}

		protected virtual string GetClassName(string database, string schema, string name, DbObjectType dbObjectType)
		{
			string className = null;

			// prefix
			if (!String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.Prefix))
			{
				className += Settings.ClassNameIteratorSettings.Prefix;
			}

			if (String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.FixedClassName))
			{
				if (Settings.ClassNameIteratorSettings.IncludeDB)
				{
					// database
					if (Settings.ClassNameIteratorSettings.CamelCase || !String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.WordsSeparator))
					{
						className += NameHelper.TransformName(database, Settings.ClassNameIteratorSettings.WordsSeparator, Settings.ClassNameIteratorSettings.CamelCase, Settings.ClassNameIteratorSettings.UpperCase, Settings.ClassNameIteratorSettings.LowerCase);
					}
					else if (Settings.ClassNameIteratorSettings.UpperCase)
					{
						className += database.ToUpper();
					}
					else if (Settings.ClassNameIteratorSettings.LowerCase)
					{
						className += database.ToLower();
					}
					else
					{
						className += database;
					}

					// db separator
					if (!String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.DBSeparator))
					{
						className += Settings.ClassNameIteratorSettings.DBSeparator;
					}
				}

				if (Settings.ClassNameIteratorSettings.IncludeSchema)
				{
					if (!String.IsNullOrEmpty(schema))
					{
						if (!Settings.ClassNameIteratorSettings.IgnoreDboSchema || schema != DefaultSchema)
						{
							// schema
							if (Settings.ClassNameIteratorSettings.CamelCase || !String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.WordsSeparator))
							{
								className += NameHelper.TransformName(schema, Settings.ClassNameIteratorSettings.WordsSeparator, Settings.ClassNameIteratorSettings.CamelCase, Settings.ClassNameIteratorSettings.UpperCase, Settings.ClassNameIteratorSettings.LowerCase);
							}
							else if (Settings.ClassNameIteratorSettings.UpperCase)
							{
								className += schema.ToUpper();
							}
							else if (Settings.ClassNameIteratorSettings.LowerCase)
							{
								className += schema.ToLower();
							}
							else
							{
								className += schema;
							}

							// schema separator
							if (!String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.SchemaSeparator))
							{
								className += Settings.ClassNameIteratorSettings.SchemaSeparator;
							}
						}
					}
				}

				// name
				if (Settings.ClassNameIteratorSettings.Singular)
				{
					if (dbObjectType is DbObjectType.Table or DbObjectType.View or DbObjectType.TVP)
					{
						name = NameHelper.GetSingularName(name);
					}
				}

				if (Settings.ClassNameIteratorSettings.CamelCase || !String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.WordsSeparator))
				{
					className += NameHelper.TransformName(name, Settings.ClassNameIteratorSettings.WordsSeparator, Settings.ClassNameIteratorSettings.CamelCase, Settings.ClassNameIteratorSettings.UpperCase, Settings.ClassNameIteratorSettings.LowerCase);
				}
				else if (Settings.ClassNameIteratorSettings.UpperCase)
				{
					className += name.ToUpper();
				}
				else if (Settings.ClassNameIteratorSettings.LowerCase)
				{
					className += name.ToLower();
				}
				else
				{
					className += name;
				}

				if (!String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.Search))
				{
					className = Settings.ClassNameIteratorSettings.SearchIgnoreCase
						? Regex.Replace(className, Settings.ClassNameIteratorSettings.Search, Settings.ClassNameIteratorSettings.Replace ?? String.Empty, RegexOptions.IgnoreCase)
						: className.Replace(Settings.ClassNameIteratorSettings.Search, Settings.ClassNameIteratorSettings.Replace ?? String.Empty);
				}
			}
			else
			{
				// fixed name
				className += Settings.ClassNameIteratorSettings.FixedClassName;
			}

			// suffix
			if (!String.IsNullOrEmpty(Settings.ClassNameIteratorSettings.Suffix))
			{
				className += Settings.ClassNameIteratorSettings.Suffix;
			}

			return className;
		}

		protected virtual void WriteClassStart(string className, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.WriteKeyword("public");
			Writer.Write(" ");
			if (Settings.POCOIteratorSettings.PartialClass)
			{
				Writer.WriteKeyword("partial");
				Writer.Write(" ");
			}
			Writer.WriteKeyword("class");
			Writer.Write(" ");
			Writer.WriteUserType(className);
			if (!String.IsNullOrEmpty(Settings.POCOIteratorSettings.Inherit))
			{
				Writer.Write(" : ");
				string[] inherit = Settings.POCOIteratorSettings.Inherit.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				Writer.WriteUserType(inherit[0]);
				for (int i = 1; i < inherit.Length; i++)
				{
					Writer.Write(", ");
					Writer.WriteUserType(inherit[i]);
				}
			}
			Writer.WriteLine();

			Writer.Write(namespaceOffset);
			Writer.WriteLine("{");
		}

		protected virtual void WriteConstructor(string className, List<IEnumColumn> enumColumns, List<INavigationProperty> navigationProperties, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			List<ITableColumn> tableColumnsWithColumnDefault = GetTableColumnsWithColumnDefaultConstructor(dbObject);
			bool isConstructorHasColumnDefaults = tableColumnsWithColumnDefault.HasAny();

			List<ITableColumn> tableColumnsWithEnum = GetTableColumnsWithEnumConstructor(dbObject, enumColumns);
			bool isConstructorHasEnumColumns = tableColumnsWithEnum.HasAny();

			if (isConstructorHasColumnDefaults && isConstructorHasEnumColumns)
			{
				tableColumnsWithColumnDefault.RemoveAll(tableColumnsWithEnum.Contains);
				isConstructorHasColumnDefaults = tableColumnsWithColumnDefault.HasAny();
			}

			bool isConstructorHasNavigationProperties = IsConstructorHasNavigationProperties(dbObject, navigationProperties);

			if (isConstructorHasColumnDefaults || isConstructorHasEnumColumns || isConstructorHasNavigationProperties)
			{
				WriteConstructorStart(className, dbObject, namespaceOffset);

				if (isConstructorHasColumnDefaults && !isConstructorHasEnumColumns)
				{
					foreach (ITableColumn column in tableColumnsWithColumnDefault.OrderBy(c => c.ColumnOrdinal ?? 0))
					{
						WriteColumnDefaultConstructorInitialization(column, namespaceOffset);
					}
				}
				else if (isConstructorHasEnumColumns && !isConstructorHasColumnDefaults)
				{
					foreach (ITableColumn column in tableColumnsWithEnum.OrderBy(c => c.ColumnOrdinal ?? 0))
					{
						WriteEnumConstructorInitialization(column, namespaceOffset);
					}
				}
				else if (isConstructorHasColumnDefaults && isConstructorHasEnumColumns)
				{
					IEnumerable<(ITableColumn column, bool isColumnDefault)> lst1 = tableColumnsWithColumnDefault.Select(column => (column, IsColumnDefault: true));
					IEnumerable<(ITableColumn column, bool isColumnDefault)> lst2 = tableColumnsWithEnum.Select(column => (column, IsColumnDefault: false));
					foreach ((ITableColumn column, bool isColumnDefault) in lst1.Union(lst2).OrderBy(i => i.column.ColumnOrdinal ?? 0))
					{
						if (isColumnDefault)
						{
							WriteColumnDefaultConstructorInitialization(column, namespaceOffset);
						}
						else
						{
							WriteEnumConstructorInitialization(column, namespaceOffset);
						}
					}
				}

				if ((isConstructorHasColumnDefaults || isConstructorHasEnumColumns) && isConstructorHasNavigationProperties)
				{
					Writer.WriteLine();
				}

				if (isConstructorHasNavigationProperties)
				{
					foreach (INavigationProperty np in navigationProperties.Where(p => p.IsCollection))
					{
						WriteNavigationPropertyConstructorInitialization(np, namespaceOffset);
					}
				}

				WriteConstructorEnd(dbObject, namespaceOffset);
				Writer.WriteLine();
			}
		}

		protected virtual void WriteConstructorStart(string className, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("public");
			Writer.Write(" ");
			Writer.Write(className);
			Writer.WriteLine("()");

			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteLine("{");
		}

		protected virtual void WriteConstructorEnd(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteLine("}");
		}

		protected virtual List<ITableColumn> GetTableColumnsWithColumnDefaultConstructor(IDbObjectTraverse dbObject) => GetColumnDefaults_NotNull(dbObject);

		protected virtual List<ITableColumn> GetColumnDefaults_NotNull(IDbObjectTraverse dbObject)
		{
			if (Settings.POCOIteratorSettings.ColumnDefaults && dbObject.DbObjectType == DbObjectType.Table)
			{
				ITable table = (ITable)dbObject;
				if (table.TableColumns.HasAny())
				{
					return table.TableColumns.Where(c => c.ColumnDefault != null).ToList();
				}
			}

			return null;
		}

		protected virtual List<ITableColumn> GetColumnDefaults_NotNullOrEmpty(IDbObjectTraverse dbObject)
		{
			if (Settings.POCOIteratorSettings.ColumnDefaults && dbObject.DbObjectType == DbObjectType.Table)
			{
				ITable table = (ITable)dbObject;
				if (table.TableColumns.HasAny())
				{
					return table.TableColumns.Where(c => !String.IsNullOrEmpty(c.ColumnDefault)).ToList();
				}
			}

			return null;
		}

		protected virtual void WriteColumnDefaultConstructorInitialization(ITableColumn column, string namespaceOffset)
		{
			string dataTypeName = (column.DataTypeName ?? String.Empty).ToLower();
			string cleanColumnDefault = CleanColumnDefault(column.ColumnDefault);

			if (IsSQLTypeMappedToBool(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				if (IsSQLTypeMappedToBoolTrue(dataTypeName, column.IsUnsigned, column.NumericPrecision, cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationBool(true, column, namespaceOffset);
				}
				else if (IsSQLTypeMappedToBoolFalse(dataTypeName, column.IsUnsigned, column.NumericPrecision, cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationBool(false, column, namespaceOffset);
				}
				else
				{
					WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
				}
			}
			else if (IsSQLTypeMappedToByte(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationByte(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToSByte(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationSByte(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToShort(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationShort(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToUShort(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationUShort(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToInt(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationInt(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToUInt(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationUInt(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToLong(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationLong(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToULong(dataTypeName, column.IsUnsigned, column.NumericPrecision))
			{
				WriteColumnDefaultConstructorInitializationULong(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToFloat(dataTypeName, column.IsUnsigned, column.NumericPrecision, column.NumericScale))
			{
				WriteColumnDefaultConstructorInitializationFloat(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToDouble(dataTypeName, column.IsUnsigned, column.NumericPrecision, column.NumericScale))
			{
				WriteColumnDefaultConstructorInitializationDouble(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToDecimal(dataTypeName, column.IsUnsigned, column.NumericPrecision, column.NumericScale))
			{
				WriteColumnDefaultConstructorInitializationDecimal(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToDateTime(dataTypeName, column.DateTimePrecision))
			{
				if (IsColumnDefaultNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTime_Now(column, namespaceOffset);
				}
				else if (IsColumnDefaultUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTime_UtcNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTime_OffsetNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTime_OffsetUtcNow(column, namespaceOffset);
				}
				else
				{
					WriteColumnDefaultConstructorInitializationDateTime(column, namespaceOffset, cleanColumnDefault);
				}
			}
			else if (IsSQLTypeMappedToTimeSpan(dataTypeName, column.DateTimePrecision))
			{
				if (IsColumnDefaultNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationTimeSpan_Now(column, namespaceOffset);
				}
				else if (IsColumnDefaultUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationTimeSpan_UtcNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationTimeSpan_OffsetNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationTimeSpan_OffsetUtcNow(column, namespaceOffset);
				}
				else
				{
					WriteColumnDefaultConstructorInitializationTimeSpan(column, namespaceOffset, cleanColumnDefault);
				}
			}
			else if (IsSQLTypeMappedToDateTimeOffset(dataTypeName, column.DateTimePrecision))
			{
				if (IsColumnDefaultNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTimeOffset_Now(column, namespaceOffset);
				}
				else if (IsColumnDefaultUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTimeOffset_UtcNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTimeOffset_OffsetNow(column, namespaceOffset);
				}
				else if (IsColumnDefaultOffsetUtcNow(cleanColumnDefault))
				{
					WriteColumnDefaultConstructorInitializationDateTimeOffset_OffsetUtcNow(column, namespaceOffset);
				}
				else
				{
					WriteColumnDefaultConstructorInitializationDateTimeOffset(column, namespaceOffset, cleanColumnDefault);
				}
			}
			else if (IsSQLTypeMappedToString(dataTypeName, column.StringPrecision))
			{
				WriteColumnDefaultConstructorInitializationString(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToByteArray(dataTypeName))
			{
				WriteColumnDefaultConstructorInitializationByteArray(cleanColumnDefault, column, namespaceOffset);
			}
			else if (IsSQLTypeMappedToGuid(dataTypeName) && IsSQLTypeMappedToGuidNewGuid(dataTypeName, cleanColumnDefault))
			{
				WriteColumnDefaultConstructorInitializationNewGuid(column, namespaceOffset);
			}
			else if (IsSQLTypeRDBMSSpecificType(dataTypeName))
			{
				WriteColumnDefaultConstructorInitializationRDBMSSpecificType(cleanColumnDefault, column, namespaceOffset);
			}
			else
			{
				WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
			}
		}

		protected virtual bool IsSQLTypeMappedToBool(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToBoolTrue(string dataTypeName, bool isUnsigned, int? numericPrecision, string cleanColumnDefault) => false;
		protected virtual bool IsSQLTypeMappedToBoolFalse(string dataTypeName, bool isUnsigned, int? numericPrecision, string cleanColumnDefault) => false;
		protected virtual bool IsSQLTypeMappedToByte(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToSByte(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToShort(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToUShort(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToInt(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToUInt(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToLong(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToULong(string dataTypeName, bool isUnsigned, int? numericPrecision) => false;
		protected virtual bool IsSQLTypeMappedToFloat(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale) => false;
		protected virtual bool IsSQLTypeMappedToDouble(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale) => false;
		protected virtual bool IsSQLTypeMappedToDecimal(string dataTypeName, bool isUnsigned, int? numericPrecision, int? numericScale) => false;
		protected virtual bool IsSQLTypeMappedToDateTime(string dataTypeName, int? dateTimePrecision) => false;
		protected virtual bool IsSQLTypeMappedToTimeSpan(string dataTypeName, int? dateTimePrecision) => false;
		protected virtual bool IsSQLTypeMappedToDateTimeOffset(string dataTypeName, int? dateTimePrecision) => false;
		protected virtual bool IsColumnDefaultNow(string cleanColumnDefault) => false;
		protected virtual bool IsColumnDefaultUtcNow(string cleanColumnDefault) => false;
		protected virtual bool IsColumnDefaultOffsetNow(string cleanColumnDefault) => false;
		protected virtual bool IsColumnDefaultOffsetUtcNow(string cleanColumnDefault) => false;
		protected virtual bool IsSQLTypeMappedToString(string dataTypeName, int? stringPrecision) => false;
		protected virtual bool IsSQLTypeMappedToByteArray(string dataTypeName) => false;
		protected virtual bool IsSQLTypeMappedToGuid(string dataTypeName) => false;
		protected virtual bool IsSQLTypeMappedToGuidNewGuid(string dataTypeName, string cleanColumnDefault) => false;
		protected virtual bool IsSQLTypeRDBMSSpecificType(string dataTypeName) => false;

		protected virtual string CleanColumnDefault(string columnDefault)
		{
			if (columnDefault.StartsWith("('") && columnDefault.EndsWith("')"))
			{
				columnDefault = columnDefault[2..^2];
			}
			else if (columnDefault.StartsWith("(N'") && columnDefault.EndsWith("')"))
			{
				columnDefault = columnDefault[3..^2];
			}
			else if (columnDefault.StartsWith("((") && columnDefault.EndsWith("))"))
			{
				columnDefault = columnDefault[2..^2];
			}

			return columnDefault;
		}

		protected virtual void WriteColumnDefaultConstructorInitializationStart(ITableColumn column, string namespaceOffset, bool isComment = false)
		{
			string cleanColumnName = NameHelper.CleanName(column.ColumnName);
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			if (isComment)
			{
				Writer.WriteComment("/* this.");
				Writer.WriteComment(cleanColumnName);
				Writer.WriteComment(" = ");
			}
			else
			{
				Writer.WriteKeyword("this");
				Writer.Write(".");
				Writer.Write(cleanColumnName);
				Writer.Write(" = ");
			}
		}

		protected virtual void WriteColumnDefaultConstructorInitializationEnd(bool isComment = false)
		{
			if (isComment)
			{
				Writer.WriteLineComment("; */");
			}
			else
			{
				Writer.WriteLine(";");
			}
		}

		protected virtual void WriteColumnDefaultConstructorInitializationBool(bool value, ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			Writer.WriteKeyword(value.ToString().ToLower());
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationByte(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationSByte(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationShort(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationUShort(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationInt(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationUInt(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset, "u");

		protected virtual void WriteColumnDefaultConstructorInitializationLong(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset, "l");

		protected virtual void WriteColumnDefaultConstructorInitializationULong(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset, "ul");

		protected virtual void WriteColumnDefaultConstructorInitializationFloat(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset, "f");

		protected virtual void WriteColumnDefaultConstructorInitializationDouble(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationDecimal(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationNumber(columnDefault, column, namespaceOffset, "m");

		protected virtual void WriteColumnDefaultConstructorInitializationNumber(string columnDefault, ITableColumn column, string namespaceOffset, string suffix = null)
		{
			columnDefault = CleanNumberDefault(columnDefault);
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			Writer.Write(columnDefault);
			if (!String.IsNullOrEmpty(suffix))
			{
				Writer.Write(suffix);
			}

			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual string CleanNumberDefault(string columnDefault) => columnDefault.Replace("(", String.Empty).Replace(")", String.Empty);

		protected virtual void WriteColumnDefaultConstructorInitializationDateTime_Now(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTime_Now();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTime_UtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTime_UtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTime_OffsetNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTime_OffsetNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTime_OffsetUtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTime_OffsetUtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTime(ITableColumn column, string namespaceOffset, string cleanColumnDefault)
		{
			if (DateTime.TryParse(cleanColumnDefault, out DateTime dateTime))
			{
				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
				WriteDateTime(dateTime);
				WriteColumnDefaultConstructorInitializationEnd();
			}
			else
			{
				WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
			}
		}

		protected virtual void WriteColumnDefaultConstructorInitializationTimeSpan_Now(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteTimeSpan_Now();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationTimeSpan_UtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteTimeSpan_UtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationTimeSpan_OffsetNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteTimeSpan_OffsetNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationTimeSpan_OffsetUtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteTimeSpan_OffsetUtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected static readonly Regex regexTimeSpan = new(@"^(?<hh>-?\d+)\:(?<mm>\d{2})\:(?<ss>\d{2})(?:\.(?<ms>\d+))?$", RegexOptions.Compiled);

		protected virtual void WriteColumnDefaultConstructorInitializationTimeSpan(ITableColumn column, string namespaceOffset, string cleanColumnDefault)
		{
			Match match = regexTimeSpan.Match(cleanColumnDefault);
			if (match.Success)
			{
				int hours = Int32.Parse(match.Groups["hh"].Value);
				int minutes = Int32.Parse(match.Groups["mm"].Value);
				int seconds = Int32.Parse(match.Groups["ss"].Value);
				int milliseconds = 0;
				if (match.Groups["ms"].Success)
				{
					milliseconds = Int32.Parse(match.Groups["ms"].Value);
				}

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
				WriteTimeSpan(hours, minutes, seconds, milliseconds);
				WriteColumnDefaultConstructorInitializationEnd();
			}
			else
			{
				if (DateTime.TryParse(cleanColumnDefault, out DateTime dateTime))
				{
					WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
					WriteTimeSpan(dateTime);
					WriteColumnDefaultConstructorInitializationEnd();
				}
				else
				{
					WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
				}
			}
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTimeOffset_Now(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTimeOffset_Now();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTimeOffset_UtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTimeOffset_UtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTimeOffset_OffsetNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTimeOffset_OffsetNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTimeOffset_OffsetUtcNow(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			WriteDateTimeOffset_OffsetUtcNow();
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationDateTimeOffset(ITableColumn column, string namespaceOffset, string cleanColumnDefault)
		{
			if (DateTimeOffset.TryParse(cleanColumnDefault, out DateTimeOffset dateTimeOffset))
			{
				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
				WriteDateTimeOffset(dateTimeOffset);
				WriteColumnDefaultConstructorInitializationEnd();
			}
			else
			{
				if (DateTime.TryParse(cleanColumnDefault, out DateTime dateTime))
				{
					WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
					WriteDateTimeOffset(dateTime);
					WriteColumnDefaultConstructorInitializationEnd();
				}
				else
				{
					WriteColumnDefaultConstructorInitializationComment(column.ColumnDefault, column, namespaceOffset);
				}
			}
		}

		protected virtual void WriteDateTime_Now()
		{
			Writer.WriteUserType("DateTime");
			Writer.Write(".Now");
		}

		protected virtual void WriteDateTime_UtcNow()
		{
			Writer.WriteUserType("DateTime");
			Writer.Write(".UtcNow");
		}

		protected virtual void WriteDateTime_OffsetNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".Now.DateTime");
		}

		protected virtual void WriteDateTime_OffsetUtcNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".UtcNow.UtcDateTime");
		}

		protected virtual void WriteDateTime(DateTime dateTime)
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("DateTime");
			Writer.Write("(");
			Writer.Write(dateTime.Year.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTime.Month.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTime.Day.ToString(CultureInfo.InvariantCulture));
			if (dateTime.Hour != 0 || dateTime.Minute != 0 || dateTime.Second != 0 || dateTime.Millisecond != 0)
			{
				Writer.Write(", ");
				Writer.Write(dateTime.Hour.ToString(CultureInfo.InvariantCulture));
				Writer.Write(", ");
				Writer.Write(dateTime.Minute.ToString(CultureInfo.InvariantCulture));
				Writer.Write(", ");
				Writer.Write(dateTime.Second.ToString(CultureInfo.InvariantCulture));
				if (dateTime.Millisecond != 0)
				{
					Writer.Write(", ");
					Writer.Write(dateTime.Millisecond.ToString(CultureInfo.InvariantCulture));
				}
			}
			Writer.Write(")");
		}

		protected virtual void WriteTimeSpan_Now()
		{
			Writer.WriteUserType("DateTime");
			Writer.Write(".Now.TimeOfDay");
		}

		protected virtual void WriteTimeSpan_UtcNow()
		{
			Writer.WriteUserType("DateTime");
			Writer.Write(".UtcNow.TimeOfDay");
		}

		protected virtual void WriteTimeSpan_OffsetNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".Now.DateTime.TimeOfDay");
		}

		protected virtual void WriteTimeSpan_OffsetUtcNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".UtcNow.UtcDateTime.TimeOfDay");
		}

		protected virtual void WriteTimeSpan(TimeSpan timeSpan) => WriteTimeSpan(timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);

		protected virtual void WriteTimeSpan(int hours, int minutes, int seconds)
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("TimeSpan");
			Writer.Write("(");
			Writer.Write(hours.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(minutes.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(seconds.ToString(CultureInfo.InvariantCulture));
			Writer.Write(")");
		}

		protected virtual void WriteTimeSpan(int hours, int minutes, int seconds, int milliseconds)
		{
			if (milliseconds == 0)
			{
				WriteTimeSpan(hours, minutes, seconds);
			}
			else
			{
				Writer.WriteKeyword("new ");
				Writer.WriteUserType("TimeSpan");
				Writer.Write("(0, ");
				Writer.Write(hours.ToString(CultureInfo.InvariantCulture));
				Writer.Write(", ");
				Writer.Write(minutes.ToString(CultureInfo.InvariantCulture));
				Writer.Write(", ");
				Writer.Write(seconds.ToString(CultureInfo.InvariantCulture));
				Writer.Write(", ");
				Writer.Write(milliseconds.ToString(CultureInfo.InvariantCulture));
				Writer.Write(")");
			}
		}

		protected virtual void WriteTimeSpan(DateTime dateTime)
		{
			WriteDateTime(dateTime);
			Writer.Write(".TimeOfDay");
		}

		protected virtual void WriteDateTimeOffset_Now()
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write("(");
			Writer.WriteUserType("DateTime");
			Writer.Write(".Now)");
		}

		protected virtual void WriteDateTimeOffset_UtcNow()
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write("(");
			Writer.WriteUserType("DateTime");
			Writer.Write(".UtcNow)");
		}

		protected virtual void WriteDateTimeOffset_OffsetNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".Now");
		}

		protected virtual void WriteDateTimeOffset_OffsetUtcNow()
		{
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write(".UtcNow");
		}

		protected virtual void WriteDateTimeOffset(DateTimeOffset dateTimeOffset)
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write("(");
			Writer.Write(dateTimeOffset.Year.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Month.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Day.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Hour.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Minute.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Second.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			Writer.Write(dateTimeOffset.Millisecond.ToString(CultureInfo.InvariantCulture));
			Writer.Write(", ");
			WriteTimeSpan(dateTimeOffset.Offset);
			Writer.Write(")");
		}

		protected virtual void WriteDateTimeOffset(DateTime dateTime)
		{
			Writer.WriteKeyword("new ");
			Writer.WriteUserType("DateTimeOffset");
			Writer.Write("(");
			WriteDateTime(dateTime);
			Writer.Write(")");
		}

		protected virtual void WriteColumnDefaultConstructorInitializationString(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			columnDefault = CleanStringDefault(columnDefault);
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			Writer.WriteString("\"");
			Writer.WriteString(columnDefault);
			Writer.WriteString("\"");
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual string CleanStringDefault(string columnDefault) => columnDefault.Replace("\\", "\\\\").Replace("\"", "\\\"");

		protected abstract void WriteColumnDefaultConstructorInitializationByteArray(string columnDefault, ITableColumn column, string namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationByteArray_Hex(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			columnDefault = CleanBinaryDefault(columnDefault);
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			Writer.WriteUserType("BitConverter");
			Writer.Write(".GetBytes(");
			Writer.WriteUserType("Convert");
			Writer.Write(".ToInt64(");
			Writer.WriteString("\"");
			if (!columnDefault.StartsWith("0x"))
			{
				Writer.WriteString("0x");
			}

			Writer.WriteString(columnDefault);
			Writer.WriteString("\"");
			Writer.Write(", 16)");
			Writer.Write(")");
			WriteColumnDefaultConstructorInitializationEnd();

			string cleanColumnName = NameHelper.CleanName(column.ColumnName);
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("if");
			Writer.Write(" (");
			Writer.WriteUserType("BitConverter");
			Writer.WriteLine(".IsLittleEndian)");
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteUserType("Array");
			Writer.Write(".Reverse(");
			Writer.WriteKeyword("this");
			Writer.Write(".");
			Writer.Write(cleanColumnName);
			Writer.WriteLine(");");
		}

		protected virtual string CleanBinaryDefault(string columnDefault) => columnDefault.Replace("(", String.Empty).Replace(")", String.Empty);

		protected virtual void WriteColumnDefaultConstructorInitializationByteArray_String(string columnDefault, ITableColumn column, string namespaceOffset) => WriteColumnDefaultConstructorInitializationByteArray_Hex("0x" + BitConverter.ToString(Encoding.UTF8.GetBytes(columnDefault)).Replace("-", String.Empty), column, namespaceOffset);

		protected virtual void WriteColumnDefaultConstructorInitializationNewGuid(ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
			Writer.WriteUserType("Guid");
			Writer.Write(".NewGuid()");
			WriteColumnDefaultConstructorInitializationEnd();
		}

		protected virtual void WriteColumnDefaultConstructorInitializationRDBMSSpecificType(string columnDefault, ITableColumn column, string namespaceOffset)
		{
		}

		protected virtual void WriteColumnDefaultConstructorInitializationComment(string columnDefault, ITableColumn column, string namespaceOffset)
		{
			WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset, true);
			Writer.WriteComment(columnDefault);
			WriteColumnDefaultConstructorInitializationEnd(true);
		}

		protected virtual List<ITableColumn> GetTableColumnsWithEnumConstructor(IDbObjectTraverse dbObject, List<IEnumColumn> enumColumns) => null;

		protected virtual void WriteEnumConstructorInitialization(ITableColumn column, string namespaceOffset)
		{
			if (column is not IEnumColumn enumColumn)
			{
				return;
			}

			string cleanColumnName = NameHelper.CleanName(enumColumn.Column.ColumnName);

			if (enumColumn.IsEnumDataType)
			{
				WriteEnumDataTypeConstructorInitialization(column, namespaceOffset, enumColumn, cleanColumnName);
			}
			else if (enumColumn.IsSetDataType)
			{
				WriteSetDataTypeConstructorInitialization(column, namespaceOffset, enumColumn, cleanColumnName);
			}
		}

		protected virtual void WriteEnumDataTypeConstructorInitialization(ITableColumn column, string namespaceOffset, IEnumColumn enumColumn, string cleanColumnName)
		{
			string literal = GetEnumDataTypeLiteralConstructorInitialization(enumColumn);

			if (!String.IsNullOrEmpty(literal))
			{
				string cleanLiteral = NameHelper.CleanEnumLiteral(literal);

				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
				WriteEnumName(cleanColumnName);
				Writer.Write(".");
				Writer.Write(cleanLiteral);
				WriteColumnDefaultConstructorInitializationEnd();
			}
		}

		protected virtual void WriteSetDataTypeConstructorInitialization(ITableColumn column, string namespaceOffset, IEnumColumn enumColumn, string cleanColumnName)
		{
			List<string> literals = GetSetDataTypeLiteralsConstructorInitialization(enumColumn);

			if (literals.HasAny())
			{
				WriteColumnDefaultConstructorInitializationStart(column, namespaceOffset);
				for (int i = 0; i < literals.Count; i++)
				{
					string literal = literals[i];
					string cleanLiteral = NameHelper.CleanEnumLiteral(literal);

					if (i > 0)
					{
						Writer.Write(" | ");
					}

					WriteEnumName(cleanColumnName);
					Writer.Write(".");
					Writer.Write(cleanLiteral);
				}
				WriteColumnDefaultConstructorInitializationEnd();
			}
		}

		protected virtual string GetEnumDataTypeLiteralConstructorInitialization(IEnumColumn enumColumn) => null;

		protected virtual List<string> GetSetDataTypeLiteralsConstructorInitialization(IEnumColumn enumColumn) => null;

		protected virtual bool IsConstructorHasNavigationProperties(IDbObjectTraverse dbObject, List<INavigationProperty> navigationProperties)
		{
			return
				IsNavigableObject(dbObject) &&
				navigationProperties.HasAny(p => p.IsCollection);
		}

		protected virtual void WriteNavigationPropertyConstructorInitialization(INavigationProperty navigationProperty, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("this");
			Writer.Write(".");
			Writer.Write(navigationProperty.ToString());
			Writer.Write(" = ");
			Writer.WriteKeyword("new");
			Writer.Write(" ");
			Writer.WriteUserType(Settings.NavigationPropertiesIteratorSettings.ICollectionNavigationProperties ? "HashSet" : "List");
			Writer.Write("<");
			Writer.WriteUserType(navigationProperty.ClassName);
			Writer.WriteLine(">();");
		}

		protected virtual void WriteColumnAttributes(IColumn column, string cleanColumnName, IDbObjectTraverse dbObject, string namespaceOffset) => WriteEFColumnAttributes(column, cleanColumnName, dbObject, namespaceOffset);

		protected virtual void WriteEFColumnAttributes(IColumn column, string cleanColumnName, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Enable)
			{
				if (dbObject.DbObjectType == DbObjectType.Table)
				{
					ITableColumn tableColumn = (ITableColumn)column;

					// Primary Key
					WriteEFPrimaryKeyAttribute(tableColumn, dbObject, namespaceOffset);

					// Index
					WriteEFIndexAttribute(tableColumn, namespaceOffset);

					// Column
					WriteEFColumnAttribute(tableColumn, dbObject, cleanColumnName, namespaceOffset);

					// MaxLength
					WriteEFMaxLengthAttribute(tableColumn, namespaceOffset);

					// StringLength
					WriteEFStringLengthAttribute(tableColumn, namespaceOffset);

					// Timestamp
					WriteEFTimestampAttribute(tableColumn, namespaceOffset);

					// ConcurrencyCheck
					WriteEFConcurrencyCheckAttribute(tableColumn, namespaceOffset);

					// DatabaseGenerated Identity
					WriteEFDatabaseGeneratedIdentityAttribute(tableColumn, namespaceOffset);

					// DatabaseGenerated Computed
					WriteEFDatabaseGeneratedComputedAttribute(tableColumn, namespaceOffset);

					// Required
					WriteEFRequiredAttribute(tableColumn, namespaceOffset);

					// Display
					WriteEFDisplayAttribute(tableColumn, namespaceOffset);
				}
				else if (dbObject.DbObjectType == DbObjectType.ComplexTypeTable)
				{
					IComplexTypeTableColumn complexTypeTableColumn = (IComplexTypeTableColumn)column;

					// MaxLength
					WriteEFMaxLengthAttribute(complexTypeTableColumn, namespaceOffset);

					// StringLength
					WriteEFStringLengthAttribute(complexTypeTableColumn, namespaceOffset);

					// Timestamp
					WriteEFTimestampAttribute(complexTypeTableColumn, namespaceOffset);

					// ConcurrencyCheck
					WriteEFConcurrencyCheckAttribute(complexTypeTableColumn, namespaceOffset);

					// DatabaseGenerated Identity
					WriteEFDatabaseGeneratedIdentityAttribute(complexTypeTableColumn, namespaceOffset);

					// DatabaseGenerated Computed
					WriteEFDatabaseGeneratedComputedAttribute(complexTypeTableColumn, namespaceOffset);

					// Required
					WriteEFRequiredAttribute(complexTypeTableColumn, namespaceOffset);

					// Display
					WriteEFDisplayAttribute(complexTypeTableColumn, namespaceOffset);
				}
				else if (dbObject.DbObjectType == DbObjectType.View)
				{
					ITableColumn tableColumn = (ITableColumn)column;

					// Index
					WriteEFIndexAttribute(tableColumn, namespaceOffset);
				}

				// Description
				WriteEFDescriptionAttribute(column, namespaceOffset);
			}
		}

		protected virtual void WriteEFPrimaryKeyAttribute(ITableColumn tableColumn, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			bool isPrimaryKey = tableColumn.PrimaryKeyColumn != null;

			if (isPrimaryKey)
			{
				bool isCompositePrimaryKey = IsCompositePrimaryKey(dbObject);

				if (isCompositePrimaryKey)
				{
					WriteEFCompositePrimaryKey(tableColumn.ColumnName, tableColumn.DataTypeName, tableColumn.PrimaryKeyColumn.Ordinal, namespaceOffset);
				}
				else
				{
					WriteEFPrimaryKey(namespaceOffset);
				}
			}
		}

		protected virtual void WriteEFIndexAttribute(ITableColumn tableColumn, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Index && tableColumn.IndexColumns.HasAny())
			{
				foreach (IIndexColumn indexColumn in tableColumn.IndexColumns.OrderBy(ic => ic.Index.Name))
				{
					bool isCompositeIndex = indexColumn.Index.IndexColumns.Count > 1;
					if (isCompositeIndex)
					{
						WriteEFCompositeIndex(indexColumn.Index.Name, indexColumn.Index.Is_Unique, indexColumn.Index.Is_Clustered, indexColumn.Is_Descending, indexColumn.Ordinal, namespaceOffset);
					}
					else
					{
						WriteEFIndex(indexColumn.Index.Name, indexColumn.Index.Is_Unique, indexColumn.Index.Is_Clustered, indexColumn.Is_Descending, namespaceOffset);
					}
				}
			}
		}

		protected virtual void WriteEFColumnAttribute(ITableColumn tableColumn, IDbObjectTraverse dbObject, string cleanColumnName, string namespaceOffset)
		{
			bool isPrimaryKey = tableColumn.PrimaryKeyColumn != null;
			bool isCompositePrimaryKey = IsCompositePrimaryKey(dbObject);

			if ((Settings.EFAnnotationsIteratorSettings.Column && (!isPrimaryKey || !isCompositePrimaryKey)) ||
				(tableColumn.ColumnName != cleanColumnName))
			{
				WriteEFColumn(tableColumn.ColumnName, tableColumn.DataTypeName, namespaceOffset);
			}
		}

		protected virtual void WriteEFMaxLengthAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (IsEFAttributeMaxLength(tableColumn.DataTypeName))
			{
				WriteEFMaxLength(tableColumn.StringPrecision, namespaceOffset);
			}
		}

		protected virtual void WriteEFStringLengthAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.StringLength)
			{
				if (IsEFAttributeStringLength(tableColumn.DataTypeName))
				{
					if (tableColumn.StringPrecision > 0)
					{
						WriteEFStringLength(tableColumn.StringPrecision.Value, namespaceOffset);
					}
				}
			}
		}

		protected virtual void WriteEFTimestampAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (IsEFAttributeTimestamp(tableColumn.DataTypeName))
			{
				WriteEFTimestamp(namespaceOffset);
			}
		}

		protected virtual void WriteEFConcurrencyCheckAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.ConcurrencyCheck)
			{
				if (IsEFAttributeConcurrencyCheck(tableColumn.DataTypeName))
				{
					WriteEFConcurrencyCheck(namespaceOffset);
				}
			}
		}

		protected virtual void WriteEFDatabaseGeneratedIdentityAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (tableColumn.IsIdentity)
			{
				WriteEFDatabaseGeneratedIdentity(namespaceOffset);
			}
		}

		protected virtual void WriteEFDatabaseGeneratedComputedAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (tableColumn.IsComputed)
			{
				WriteEFDatabaseGeneratedComputed(namespaceOffset);
			}
		}

		protected virtual void WriteEFRequiredAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Required || Settings.EFAnnotationsIteratorSettings.RequiredWithErrorMessage)
			{
				if (!tableColumn.IsNullable)
				{
					string display = null;
					if (Settings.EFAnnotationsIteratorSettings.RequiredWithErrorMessage)
					{
						display = GetEFDisplay(tableColumn.ColumnName);
					}

					WriteEFRequired(display, namespaceOffset);
				}
			}
		}

		protected virtual void WriteEFDisplayAttribute(IColumn tableColumn, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Display)
			{
				string display = GetEFDisplay(tableColumn.ColumnName);
				WriteEFDisplay(display, namespaceOffset);
			}
		}

		protected virtual void WriteEFDescriptionAttribute(IColumn column, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Description && column is IDescription descObject)
			{
				if (!String.IsNullOrEmpty(descObject.Description))
				{
					WriteEFDescription(descObject.Description, true, namespaceOffset);
				}
			}
		}

		protected abstract bool IsEFAttributeMaxLength(string dataTypeName);
		protected abstract bool IsEFAttributeStringLength(string dataTypeName);
		protected abstract bool IsEFAttributeTimestamp(string dataTypeName);
		protected abstract bool IsEFAttributeConcurrencyCheck(string dataTypeName);

		protected virtual bool IsCompositePrimaryKey(IDbObjectTraverse dbObject)
		{
			return dbObject.Columns.HasAny()
&& dbObject.Columns.Count(c => c is ITableColumn tableColumn && tableColumn.PrimaryKeyColumn != null) > 1;
		}

		protected virtual void WriteEFPrimaryKey(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Key");
			Writer.WriteLine("]");
		}

		protected virtual void WriteEFCompositePrimaryKey(string columnName, string dataTypeName, byte ordinal, string namespaceOffset)
		{
			WriteEFPrimaryKey(namespaceOffset);

			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Column");
			Writer.Write("(");

			if (Settings.EFAnnotationsIteratorSettings.Column)
			{
				Writer.Write("Name = ");
				Writer.WriteString("\"");
				Writer.WriteString(columnName);
				Writer.WriteString("\"");
				Writer.Write(", TypeName = ");
				Writer.WriteString("\"");
				Writer.WriteString(dataTypeName);
				Writer.WriteString("\"");
				Writer.Write(", ");
			}

			Writer.Write("Order = ");
			Writer.Write(ordinal.ToString());
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFIndex(string indexName, bool isUnique, bool isClustered, bool isDescending, string namespaceOffset)
		{
			WriteEFIndexSortOrderError(indexName, isDescending, namespaceOffset);
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Index");
			Writer.Write("(");
			Writer.WriteString("\"");
			Writer.WriteString(indexName);
			Writer.WriteString("\"");
			if (isUnique)
			{
				Writer.Write(", IsUnique = ");
				Writer.WriteKeyword("true");
			}
			if (isClustered)
			{
				Writer.Write(", IsClustered = ");
				Writer.WriteKeyword("true");
			}
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFCompositeIndex(string indexName, bool isUnique, bool isClustered, bool isDescending, byte ordinal, string namespaceOffset)
		{
			WriteEFIndexSortOrderError(indexName, isDescending, namespaceOffset);
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Index");
			Writer.Write("(");
			Writer.WriteString("\"");
			Writer.WriteString(indexName);
			Writer.WriteString("\"");
			Writer.Write(", ");
			Writer.Write(ordinal.ToString());
			if (isUnique)
			{
				Writer.Write(", IsUnique = ");
				Writer.WriteKeyword("true");
			}
			if (isClustered)
			{
				Writer.Write(", IsClustered = ");
				Writer.WriteKeyword("true");
			}
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFIndexSortOrderError(string indexName, bool isDescending, string namespaceOffset)
		{
			if (isDescending)
			{
				Writer.Write(namespaceOffset);
				Writer.Write(Settings.POCOIteratorSettings.Tab);
				Writer.WriteError("/* ");
				Writer.WriteError(indexName);
				Writer.WriteLineError(". Sort order is Descending. Index doesn't support sort order. */");
			}
		}

		protected virtual void WriteEFColumn(string columnName, string dataTypeName, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Column");
			Writer.Write("(Name = ");
			Writer.WriteString("\"");
			Writer.WriteString(columnName);
			Writer.WriteString("\"");
			Writer.Write(", TypeName = ");
			Writer.WriteString("\"");
			Writer.WriteString(dataTypeName);
			Writer.WriteString("\"");
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFMaxLength(int? stringPrecision, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("MaxLength");
			if (stringPrecision >= 0)
			{
				Writer.Write("(");
				Writer.Write(stringPrecision.ToString());
				Writer.Write(")");
			}
			Writer.WriteLine("]");
		}

		protected virtual void WriteEFStringLength(int stringPrecision, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("StringLength");
			Writer.Write("(");
			Writer.Write(stringPrecision.ToString());
			Writer.Write(")");
			Writer.WriteLine("]");
		}

		protected virtual void WriteEFTimestamp(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Timestamp");
			Writer.WriteLine("]");
		}

		protected virtual void WriteEFConcurrencyCheck(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("ConcurrencyCheck");
			Writer.WriteLine("]");
		}

		protected virtual void WriteEFDatabaseGeneratedIdentity(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("DatabaseGenerated");
			Writer.Write("(");
			Writer.WriteUserType("DatabaseGeneratedOption");
			Writer.WriteLine(".Identity)]");
		}

		protected virtual void WriteEFDatabaseGeneratedComputed(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("DatabaseGenerated");
			Writer.Write("(");
			Writer.WriteUserType("DatabaseGeneratedOption");
			Writer.WriteLine(".Computed)]");
		}

		protected static readonly Regex regexDisplay1 = new("[^0-9a-zA-Z]", RegexOptions.Compiled);
		protected static readonly Regex regexDisplay2 = new("([^A-Z]|^)(([A-Z\\s]*)($|[A-Z]))", RegexOptions.Compiled);
		protected static readonly Regex regexDisplay3 = new("\\s{2,}", RegexOptions.Compiled);

		protected virtual string GetEFDisplay(string columnName)
		{
			string display = columnName;
			display = regexDisplay1.Replace(display, " ");
			display = regexDisplay2.Replace(display, "$1 $3 $4");
			display = display.Trim();
			display = regexDisplay3.Replace(display, " ");
			return display;
		}

		protected virtual void WriteEFRequired(string display, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Required");
			if (Settings.EFAnnotationsIteratorSettings.RequiredWithErrorMessage)
			{
				WriteEFRequiredErrorMessage(display);
			}

			Writer.WriteLine("]");
		}

		protected virtual void WriteEFRequiredErrorMessage(string display)
		{
			Writer.Write("(ErrorMessage = ");
			Writer.WriteString("\"");
			Writer.WriteString(display);
			Writer.WriteString(" is required");
			Writer.WriteString("\"");
			Writer.Write(")");
		}

		protected virtual void WriteEFDisplay(string display, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Display");
			Writer.Write("(Name = ");
			Writer.WriteString("\"");
			Writer.WriteString(display);
			Writer.WriteString("\"");
			Writer.WriteLine(")]");
		}

		protected virtual void WriteEFDescription(string description, bool writeTab, string namespaceOffset)
		{
			if (!String.IsNullOrEmpty(description))
			{
				Writer.Write(namespaceOffset);
				if (writeTab)
				{
					Writer.Write(Settings.POCOIteratorSettings.Tab);
				}

				Writer.Write("[");
				Writer.WriteUserType("Description");
				Writer.Write("(");
				Writer.WriteString("\"");
				Writer.WriteString(NameHelper.Escape(description));
				Writer.WriteString("\"");
				Writer.WriteLine(")]");
			}
		}

		protected virtual void WriteColumn(IColumn column, bool isComplexTypeTableColumn, bool isLastColumn, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (isComplexTypeTableColumn)
			{
				WriteComplexTypeTableColumn(column, isLastColumn, dbObject, namespaceOffset);
			}
			else
			{
				WriteDbColumn(column, isLastColumn, dbObject, namespaceOffset);
			}
		}

		protected virtual void WriteDbColumn(IColumn column, bool isLastColumn, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			string cleanColumnName = NameHelper.CleanName(column.ColumnName);

			WriteColumnAttributes(column, cleanColumnName, dbObject, namespaceOffset);

			WriteColumnStart(namespaceOffset);

			WriteColumnDataType(column);

			WriteColumnName(cleanColumnName);

			WriteColumnEnd();

			WriteColumnComments(column);

			Writer.WriteLine();

			if (Settings.POCOIteratorSettings.NewLineBetweenMembers && !isLastColumn)
			{
				Writer.WriteLine();
			}
		}

		protected virtual void WriteComplexTypeTableColumn(IColumn column, bool isLastColumn, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			ITableColumn tableColumn = (ITableColumn)column;

			IComplexTypeTable complexTypeTable = tableColumn.ComplexTypeTableColumn.ComplexTypeTable;
			string complexTypeName = GetClassName(
				dbObject.Database.ToString(),
				complexTypeTable is ISchema schema ? schema.Schema : null,
				complexTypeTable.Name,
				complexTypeTable.DbObjectType
			);

			string propertyName = tableColumn.ColumnName[..tableColumn.ColumnName.IndexOf('_')];
			string cleanColumnName = NameHelper.CleanName(propertyName);

			WriteColumnStart(namespaceOffset);

			WriteComplexTypeName(complexTypeName);

			WriteColumnName(cleanColumnName);

			WriteColumnEnd();

			Writer.WriteLine();

			if (Settings.POCOIteratorSettings.NewLineBetweenMembers && !isLastColumn)
			{
				Writer.WriteLine();
			}
		}

		protected virtual void WriteColumnStart(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("public");
			Writer.Write(" ");

			if (Settings.POCOIteratorSettings.Properties && Settings.POCOIteratorSettings.VirtualProperties)
			{
				Writer.WriteKeyword("virtual");
				Writer.Write(" ");
			}
			else if (Settings.POCOIteratorSettings.Properties && Settings.POCOIteratorSettings.OverrideProperties)
			{
				Writer.WriteKeyword("override");
				Writer.Write(" ");
			}
		}

		protected virtual void WriteColumnDataType(IColumn column) => WriteDbColumnDataType(column);

		protected abstract void WriteDbColumnDataType(IColumn column);

		protected virtual void WriteComplexTypeName(string complexTypeName) => Writer.WriteUserType(complexTypeName);

		protected virtual void WriteColumnName(string columnName)
		{
			Writer.Write(" ");
			Writer.Write(columnName);
		}

		protected virtual void WriteColumnEnd()
		{
			if (Settings.POCOIteratorSettings.Properties)
			{
				Writer.Write(" { ");
				Writer.WriteKeyword("get");
				Writer.Write("; ");
				Writer.WriteKeyword("set");
				Writer.Write("; }");
			}
			else if (Settings.POCOIteratorSettings.Fields)
			{
				Writer.Write(";");
			}
		}

		protected virtual void WriteColumnComments(IColumn column)
		{
			if (Settings.POCOIteratorSettings.Comments || Settings.POCOIteratorSettings.CommentsWithoutNull)
			{
				Writer.Write(" ");
				Writer.WriteComment("//");
				Writer.WriteComment(" ");
				Writer.WriteComment(column.DataTypeDisplay);
				Writer.WriteComment(column.Precision ?? String.Empty);

				if (!Settings.POCOIteratorSettings.CommentsWithoutNull)
				{
					Writer.WriteComment(",");
					Writer.WriteComment(" ");
					Writer.WriteComment(column.IsNullable ? "null" : "not null");
				}
			}
		}

		protected virtual void WriteColumnBool(bool isNullable)
		{
			Writer.WriteKeyword("bool");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnByte(bool isNullable)
		{
			Writer.WriteKeyword("byte");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnByteArray()
		{
			Writer.WriteKeyword("byte");
			Writer.Write("[]");
		}

		protected virtual void WriteColumnDateTime(bool isNullable)
		{
			Writer.WriteUserType("DateTime");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnDateTimeOffset(bool isNullable)
		{
			Writer.WriteUserType("DateTimeOffset");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnDecimal(bool isNullable)
		{
			Writer.WriteKeyword("decimal");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnDouble(bool isNullable)
		{
			Writer.WriteKeyword("double");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnFloat(bool isNullable)
		{
			Writer.WriteKeyword("float");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnGuid(bool isNullable)
		{
			Writer.WriteUserType("Guid");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnInt(bool isNullable)
		{
			Writer.WriteKeyword("int");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnLong(bool isNullable)
		{
			Writer.WriteKeyword("long");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnObject() => Writer.WriteKeyword("object");

		protected virtual void WriteColumnSByte(bool isNullable)
		{
			Writer.WriteKeyword("sbyte");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnShort(bool isNullable)
		{
			Writer.WriteKeyword("short");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnString() => Writer.WriteKeyword("string");

		protected virtual void WriteColumnTimeSpan(bool isNullable)
		{
			Writer.WriteUserType("TimeSpan");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnUInt(bool isNullable)
		{
			Writer.WriteKeyword("uint");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnULong(bool isNullable)
		{
			Writer.WriteKeyword("ulong");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnUShort(bool isNullable)
		{
			Writer.WriteKeyword("ushort");
			if (isNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual void WriteColumnEnum(IEnumColumn enumColumn)
		{
			string cleanColumnName = NameHelper.CleanName(enumColumn.Column.ColumnName);
			WriteEnumName(cleanColumnName);
			if (enumColumn.Column.IsNullable || Settings.POCOIteratorSettings.StructTypesNullable)
			{
				Writer.Write("?");
			}
		}

		protected virtual List<IEnumColumn> GetEnumColumns(IDbObjectTraverse dbObject)
		{
			if (Support.IsSupportEnumDataType)
			{
				if (!Settings.POCOIteratorSettings.EnumSQLTypeToString && (Settings.POCOIteratorSettings.EnumSQLTypeToEnumUShort || Settings.POCOIteratorSettings.EnumSQLTypeToEnumInt))
				{
					if (dbObject.Columns != null && dbObject.Columns.Any(c => c is IEnumColumn))
					{
						return dbObject.Columns
							.Where(c => c is IEnumColumn)
							.Cast<IEnumColumn>()
							.Where(c => c.IsEnumDataType || c.IsSetDataType)
							.OrderBy(c => c.Column.ColumnOrdinal ?? 0)
							.ToList();
					}
				}
			}

			return null;
		}

		protected virtual void WriteEnum(IEnumColumn enumColumn, bool isLastColumn, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.WriteLine();

			string cleanColumnName = NameHelper.CleanName(enumColumn.Column.ColumnName);

			List<string> literals = enumColumn.EnumLiterals.Select(NameHelper.CleanEnumLiteral).ToList();

			if (enumColumn.IsEnumDataType)
			{
				WriteEnumDeclaration(cleanColumnName, literals, namespaceOffset);
			}
			else if (enumColumn.IsSetDataType)
			{
				WriteSetDeclaration(cleanColumnName, literals, namespaceOffset);
			}
		}

		protected virtual void WriteEnumDeclaration(string columnName, List<string> literals, string namespaceOffset)
		{
			WriteEnumStart(columnName, false, namespaceOffset);

			for (int i = 0; i < literals.Count; i++)
			{
				string literal = literals[i];
				string literalValue = (i + 1).ToString();
				bool isLastLiteral = i < literals.Count - 1;
				WriteEnumLiteral(literal, literalValue, isLastLiteral, namespaceOffset);
			}

			WriteEnumEnd(namespaceOffset);
		}

		protected virtual void WriteSetDeclaration(string columnName, List<string> literals, string namespaceOffset)
		{
			WriteEnumFlags(namespaceOffset);
			WriteEnumStart(columnName, true, namespaceOffset);

			for (int i = 0; i < literals.Count; i++)
			{
				string literal = literals[i];

				string literalValue = "1";
				if (Settings.POCOIteratorSettings.EnumSQLTypeToEnumUShort)
				{
					literalValue += "ul";
				}

				if (i > 0)
				{
					literalValue += " << ";
					literalValue += i.ToString();
				}

				bool isLastLiteral = i < literals.Count - 1;

				WriteEnumLiteral(literal, literalValue, isLastLiteral, namespaceOffset);
			}

			WriteEnumEnd(namespaceOffset);
		}

		protected virtual void WriteEnumFlags(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("Flags");
			Writer.Write("]");
			Writer.WriteLine();
		}

		protected virtual void WriteEnumStart(string columnName, bool isSetDataType, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("public");
			Writer.Write(" ");
			Writer.WriteKeyword("enum");
			Writer.Write(" ");
			WriteEnumName(columnName);
			Writer.Write(" : ");
			if (Settings.POCOIteratorSettings.EnumSQLTypeToEnumUShort)
			{
				if (isSetDataType)
				{
					Writer.WriteKeyword("ulong");
				}
				else
				{
					Writer.WriteKeyword("ushort");
				}
			}
			else if (Settings.POCOIteratorSettings.EnumSQLTypeToEnumInt)
			{
				Writer.WriteKeyword("int");
			}
			Writer.WriteLine();

			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("{");
			Writer.WriteLine();
		}

		protected virtual void WriteEnumName(string columnName)
		{
			WriteEnumNamePrefix();
			Writer.WriteUserType(columnName);
			WriteEnumNameSuffix();
		}

		protected virtual void WriteEnumNamePrefix()
		{
		}

		protected virtual void WriteEnumNameSuffix() => Writer.WriteUserType("_Values");

		protected virtual void WriteEnumLiteral(string literal, string literalValue, bool isLastLiteral, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write(literal);
			Writer.Write(" = ");
			Writer.Write(literalValue);
			if (isLastLiteral)
			{
				Writer.Write(",");
			}

			Writer.WriteLine();
		}

		protected virtual void WriteEnumEnd(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("}");
			Writer.WriteLine();
		}

		protected virtual bool IsNavigableObject(IDbObjectTraverse dbObject)
		{
			return
				Settings.NavigationPropertiesIteratorSettings.Enable &&
				dbObject.DbObjectType == DbObjectType.Table;
		}

		protected virtual List<INavigationProperty> GetNavigationProperties(IDbObjectTraverse dbObject)
		{
			List<INavigationProperty> navigationProperties = null;

			if (IsNavigableObject(dbObject))
			{
				ITable table = (ITable)dbObject;

				if (table.ForeignKeys.HasAny())
				{
					navigationProperties = [];

					foreach (IForeignKey fk in table.ForeignKeys)
					{
						if ((Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
							fk.NavigationPropertyFromForeignToPrimary.IsVisibleWhenManyToManyJoinTableIsOn) ||
							(!Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
							fk.NavigationPropertyFromForeignToPrimary.IsVisibleWhenManyToManyJoinTableIsOff))
						{
							string className = GetClassName(
								dbObject.Database.ToString(),
								fk.PrimaryTable is ISchema schema ? schema.Schema : null,
								fk.PrimaryTable.Name,
								dbObject.DbObjectType
							);

							fk.NavigationPropertyFromForeignToPrimary.ClassName = className;
							navigationProperties.Add(fk.NavigationPropertyFromForeignToPrimary);
						}
					}
				}

				if (table.PrimaryForeignKeys.HasAny())
				{
					navigationProperties ??= [];

					foreach (IForeignKey fk in table.PrimaryForeignKeys)
					{
						if ((Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
							fk.NavigationPropertyFromPrimaryToForeign.IsVisibleWhenManyToManyJoinTableIsOn) ||
							(!Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
							fk.NavigationPropertyFromPrimaryToForeign.IsVisibleWhenManyToManyJoinTableIsOff))
						{
							string className = GetClassName(
								dbObject.Database.ToString(),
								fk.ForeignTable is ISchema schema ? schema.Schema : null,
								fk.ForeignTable.Name,
								dbObject.DbObjectType
							);

							fk.NavigationPropertyFromPrimaryToForeign.ClassName = className;
							navigationProperties.Add(fk.NavigationPropertyFromPrimaryToForeign);
						}

						if (fk.VirtualNavigationProperties.HasAny())
						{
							foreach (INavigationProperty vnp in fk.VirtualNavigationProperties)
							{
								if ((Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
									vnp.IsVisibleWhenManyToManyJoinTableIsOn) ||
									(!Settings.NavigationPropertiesIteratorSettings.ManyToManyJoinTable &&
									vnp.IsVisibleWhenManyToManyJoinTableIsOff))
								{
									IForeignKey vfk = vnp.ForeignKey;

									string className = GetClassName(
										dbObject.Database.ToString(),
										vfk.ForeignTable is ISchema schema ? schema.Schema : null,
										vfk.ForeignTable.Name,
										dbObject.DbObjectType
									);

									vnp.ClassName = className;
									navigationProperties.Add(vnp);
								}
							}
						}
					}
				}

				// rename duplicates
				RenameDuplicateNavigationProperties(navigationProperties);
			}

			SetNavigationPropertiesMultipleRelationships(navigationProperties);

			return navigationProperties;
		}

		protected static readonly Regex regexEndNumber = new("(\\d+)$", RegexOptions.Compiled);

		protected virtual void RenameDuplicateNavigationProperties(List<INavigationProperty> navigationProperties)
		{
			if (navigationProperties.HasAny())
			{
				// groups of navigation properties with the same name
				IEnumerable<IGrouping<string, INavigationProperty>> npGroups1 = navigationProperties.GroupBy(p => p.ToString()).Where(g => g.Count() > 1);

				// if the original column name ended with a number, then assign that number to the property name
				foreach (IGrouping<string, INavigationProperty> npGroup in npGroups1)
				{
					foreach (INavigationProperty np in npGroup)
					{
						string columnName =
							np.IsFromForeignToPrimary ?
							np.ForeignKey.ForeignKeyColumns[0].PrimaryTableColumn.ColumnName :
							np.ForeignKey.ForeignKeyColumns[0].ForeignTableColumn.ColumnName
						;

						Match match = regexEndNumber.Match(columnName);
						if (match.Success)
						{
							np.RenamedPropertyName = np.ToString() + match.Value;
						}
					}
				}

				// if there are still duplicate property names, then rename them with a running number suffix
				IEnumerable<IGrouping<string, INavigationProperty>> npGroups2 = navigationProperties.GroupBy(p => p.ToString()).Where(g => g.Count() > 1);
				foreach (IGrouping<string, INavigationProperty> npGroup in npGroups2)
				{
					int suffix = 1;
					foreach (INavigationProperty np in npGroup.Skip(1))
					{
						np.RenamedPropertyName = np.ToString() + suffix++;
					}
				}
			}
		}

		protected virtual void WriteNavigationProperties(List<INavigationProperty> navigationProperties, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (IsNavigableObject(dbObject))
			{
				if (navigationProperties.HasAny())
				{
					if (!Settings.POCOIteratorSettings.NewLineBetweenMembers)
					{
						Writer.WriteLine();
					}

					foreach (INavigationProperty np in navigationProperties)
					{
						WriteNavigationProperty(np, dbObject, namespaceOffset);
					}
				}
			}
		}

		protected virtual void WriteNavigationProperty(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (Settings.POCOIteratorSettings.NewLineBetweenMembers)
			{
				Writer.WriteLine();
			}

			WriteNavigationPropertyComments(navigationProperty, dbObject, namespaceOffset);

			WriteNavigationPropertyAttributes(navigationProperty, dbObject, namespaceOffset);

			if (navigationProperty.IsCollection)
			{
				WriteNavigationPropertyCollection(navigationProperty, dbObject, namespaceOffset);
			}
			else
			{
				WriteNavigationPropertySingular(navigationProperty, dbObject, namespaceOffset);
			}
		}

		protected virtual void WriteNavigationPropertyComments(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (Settings.NavigationPropertiesIteratorSettings.Comments)
			{
				if (!navigationProperty.IsVirtualNavigationProperty)
				{
					foreach (IForeignKeyColumn fkc in navigationProperty.ForeignKey.ForeignKeyColumns)
					{
						Writer.Write(namespaceOffset);
						Writer.Write(Settings.POCOIteratorSettings.Tab);
						Writer.WriteComment("// ");
						if (navigationProperty.ForeignKey.ForeignTable is ISchema schemaFT)
						{
							Writer.WriteComment(schemaFT.Schema);
							Writer.WriteComment(".");
						}
						Writer.WriteComment(navigationProperty.ForeignKey.ForeignTable.Name);
						Writer.WriteComment(".");
						Writer.WriteComment(fkc.ForeignTableColumn.ColumnName);
						Writer.WriteComment(" -> ");
						if (navigationProperty.ForeignKey.PrimaryTable is ISchema schemaPT)
						{
							Writer.WriteComment(schemaPT.Schema);
							Writer.WriteComment(".");
						}
						Writer.WriteComment(navigationProperty.ForeignKey.PrimaryTable.Name);
						Writer.WriteComment(".");
						Writer.WriteComment(fkc.PrimaryTableColumn.ColumnName);
						Writer.WriteComment(" (");
						Writer.WriteComment(navigationProperty.ForeignKey.Name);
						Writer.WriteComment(")");
						Writer.WriteLine();
					}
				}
			}
		}

		protected virtual void WriteNavigationPropertyAttributes(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset) => WriteEFNavigationPropertyAttributes(navigationProperty, dbObject, namespaceOffset);

		protected virtual void WriteNavigationPropertySingular(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			WriteNavigationPropertyStart(namespaceOffset);
			Writer.WriteUserType(navigationProperty.ClassName);
			Writer.Write(" ");
			Writer.Write(navigationProperty.ToString());
			WriteNavigationPropertyEnd();
			Writer.WriteLine();
		}

		protected virtual void WriteNavigationPropertyCollection(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			WriteNavigationPropertyStart(namespaceOffset);
			if (Settings.NavigationPropertiesIteratorSettings.ListNavigationProperties)
			{
				Writer.WriteUserType("List");
			}
			else if (Settings.NavigationPropertiesIteratorSettings.IListNavigationProperties)
			{
				Writer.WriteUserType("IList");
			}
			else if (Settings.NavigationPropertiesIteratorSettings.ICollectionNavigationProperties)
			{
				Writer.WriteUserType("ICollection");
			}
			else if (Settings.NavigationPropertiesIteratorSettings.IEnumerableNavigationProperties)
			{
				Writer.WriteUserType("IEnumerable");
			}

			Writer.Write("<");
			Writer.WriteUserType(navigationProperty.ClassName);
			Writer.Write("> ");
			Writer.Write(navigationProperty.ToString());
			WriteNavigationPropertyEnd();
			Writer.WriteLine();
		}

		protected virtual void WriteNavigationPropertyStart(string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.WriteKeyword("public");
			Writer.Write(" ");

			if (Settings.POCOIteratorSettings.Properties && Settings.NavigationPropertiesIteratorSettings.VirtualNavigationProperties)
			{
				Writer.WriteKeyword("virtual");
				Writer.Write(" ");
			}
			else if (Settings.POCOIteratorSettings.Properties && Settings.NavigationPropertiesIteratorSettings.OverrideNavigationProperties)
			{
				Writer.WriteKeyword("override");
				Writer.Write(" ");
			}
		}

		protected virtual void WriteNavigationPropertyEnd()
		{
			if (Settings.POCOIteratorSettings.Properties)
			{
				Writer.Write(" { ");
				Writer.WriteKeyword("get");
				Writer.Write("; ");
				Writer.WriteKeyword("set");
				Writer.Write("; }");
			}
			else if (Settings.POCOIteratorSettings.Fields)
			{
				Writer.Write(";");
			}
		}

		protected virtual void SetNavigationPropertiesMultipleRelationships(List<INavigationProperty> navigationProperties)
		{
			if (Settings.EFAnnotationsIteratorSettings.Enable)
			{
				if (navigationProperties.HasAny())
				{
					IEnumerable<INavigationProperty> multipleRels = navigationProperties
						.GroupBy(np => (np.ForeignKey.ForeignTable, np.ForeignKey.PrimaryTable))
						.Where(g => g.Count() > 1)
						.SelectMany(g => g);

					foreach (INavigationProperty np in multipleRels)
					{
						np.HasMultipleRelationships = true;
					}
				}
			}
		}

		protected virtual void WriteEFNavigationPropertyAttributes(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			if (Settings.EFAnnotationsIteratorSettings.Enable)
			{
				if (Settings.EFAnnotationsIteratorSettings.ForeignKeyAndInverseProperty)
				{
					if (IsNavigableObject(dbObject))
					{
						if (navigationProperty.IsFromForeignToPrimary)
						{
							WriteNavigationPropertyForeignKeyAttribute(navigationProperty, dbObject, namespaceOffset);
						}

						if (!navigationProperty.IsFromForeignToPrimary && navigationProperty.HasMultipleRelationships)
						{
							WriteNavigationPropertyInversePropertyAttribute(navigationProperty, dbObject, namespaceOffset);
						}
					}
				}
			}
		}

		protected virtual void WriteNavigationPropertyForeignKeyAttribute(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("ForeignKey");
			Writer.Write("(");
			Writer.WriteString("\"");
			if (navigationProperty.HasMultipleRelationships)
			{
				Writer.WriteString(String.Join(", ", navigationProperty.ForeignKey.ForeignKeyColumns.Select(c => c.ForeignTableColumn.ColumnName)));
			}
			else
			{
				Writer.WriteString(String.Join(", ", navigationProperty.ForeignKey.ForeignKeyColumns.Select(c => c.PrimaryTableColumn.ColumnName)));
			}

			Writer.WriteString("\"");
			Writer.WriteLine(")]");
		}

		protected virtual void WriteNavigationPropertyInversePropertyAttribute(INavigationProperty navigationProperty, IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.Write(Settings.POCOIteratorSettings.Tab);
			Writer.Write("[");
			Writer.WriteUserType("InverseProperty");
			Writer.Write("(");
			Writer.WriteString("\"");
			Writer.WriteString(navigationProperty.InverseProperty.ToString());
			Writer.WriteString("\"");
			Writer.WriteLine(")]");
		}

		protected virtual void WriteClassEnd(IDbObjectTraverse dbObject, string namespaceOffset) => WriteDbClassEnd(dbObject, namespaceOffset);

		protected virtual void WriteDbClassEnd(IDbObjectTraverse dbObject, string namespaceOffset)
		{
			Writer.Write(namespaceOffset);
			Writer.WriteLine("}");
		}

		protected virtual void WriteNamespaceEnd(string @namespace)
		{
			if (!String.IsNullOrEmpty(@namespace))
			{
				Writer.WriteLine("}");
			}
		}
	}
}

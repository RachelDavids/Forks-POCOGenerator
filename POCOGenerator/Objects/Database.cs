using System;
using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a RDBMS database.</summary>
	public sealed class Database
	{
		private readonly DbObjects.IDatabase database;
		private readonly DatabaseAccessibleObjects databaseAccessibleObjects;

		internal Database(DbObjects.IDatabase database, Server server, DatabaseAccessibleObjects databaseAccessibleObjects)
		{
			this.database = database;
			Server = server;
			this.databaseAccessibleObjects = databaseAccessibleObjects;
		}

		internal bool InternalEquals(DbObjects.IDatabase database)
		{
			return this.database == database;
		}

		/// <summary>Gets the server that this database belongs to.</summary>
		/// <value>The server that this database belongs to.</value>
		public Server Server { get; private set; }

		private CachedEnumerable<DbObjects.ITable, Table> tables;
		/// <summary>Gets the collection of tables that belong to this database.</summary>
		/// <value>Collection of tables.</value>
		public IEnumerable<Table> Tables {
			get {
				if (database.Tables.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.Tables.IsNullOrEmpty())
				{
					yield break;
				}

				if (tables == null)
				{
					List<DbObjects.ITable> source = database.Tables.Intersect(databaseAccessibleObjects.Tables).ToList();
					tables = new(source, t => new(t, this));
				}

				foreach (Table table in tables)
				{
					yield return table;
				}
			}
		}

		// AccessibleTables property contains non-included tables that can be recursively accessed
		// from included tables through foreign keys.
		// In a well-connected database (lots of foreign keys), AccessibleTables is memory consuming.
		// It might include all tables in the database.
		// This is a trade-off between maintaining tables accessibility over memory consumption.
		private CachedEnumerable<DbObjects.ITable, Table> accessibleTables;
		internal IEnumerable<Table> AccessibleTables {
			get {
				if (database.Tables.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.AccessibleTables.IsNullOrEmpty())
				{
					yield break;
				}

				if (accessibleTables == null)
				{
					List<DbObjects.ITable> source = database.Tables.Intersect(databaseAccessibleObjects.AccessibleTables).ToList();
					accessibleTables = new(source, t => new(t, this));
				}

				foreach (Table table in accessibleTables)
				{
					yield return table;
				}
			}
		}

		private CachedEnumerable<DbObjects.IComplexTypeTable, ComplexTypeTable> complexTypeTables;
		internal IEnumerable<ComplexTypeTable> ComplexTypeTables {
			get {
				if (database.Tables.IsNullOrEmpty())
				{
					yield break;
				}

				if (complexTypeTables == null)
				{
					IEnumerable<DbObjects.ITable> tables = database.Tables.Intersect(databaseAccessibleObjects.Tables);
					List<DbObjects.IComplexTypeTable> source = tables.Where(t => t.ComplexTypeTables.HasAny()).SelectMany(t => t.ComplexTypeTables).Distinct().OrderBy(t => t.Name).ToList();
					complexTypeTables = new(source, t => new(t, this));
				}

				foreach (ComplexTypeTable complexTypeTable in complexTypeTables)
				{
					yield return complexTypeTable;
				}
			}
		}

		private CachedEnumerable<DbObjects.IView, View> views;
		/// <summary>Gets the collection of views that belong to this database.</summary>
		/// <value>Collection of views.</value>
		public IEnumerable<View> Views {
			get {
				if (database.Views.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.Views.IsNullOrEmpty())
				{
					yield break;
				}

				if (views == null)
				{
					List<DbObjects.IView> source = database.Views.Intersect(databaseAccessibleObjects.Views).ToList();
					views = new(source, v => new(v, this));
				}

				foreach (View view in views)
				{
					yield return view;
				}
			}
		}

		private CachedEnumerable<DbObjects.IProcedure, Procedure> procedures;
		/// <summary>Gets the collection of stored procedures that belong to this database.</summary>
		/// <value>Collection of stored procedures.</value>
		public IEnumerable<Procedure> Procedures {
			get {
				if (database.Procedures.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.Procedures.IsNullOrEmpty())
				{
					yield break;
				}

				if (procedures == null)
				{
					List<DbObjects.IProcedure> source = database.Procedures.Intersect(databaseAccessibleObjects.Procedures).ToList();
					procedures = new(source, p => new(p, this));
				}

				foreach (Procedure procedure in procedures)
				{
					yield return procedure;
				}
			}
		}

		private CachedEnumerable<DbObjects.IFunction, Function> functions;
		/// <summary>Gets the collection of table-valued functions that belong to this database.</summary>
		/// <value>Collection of table-valued functions.</value>
		public IEnumerable<Function> Functions {
			get {
				if (database.Functions.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.Functions.IsNullOrEmpty())
				{
					yield break;
				}

				if (functions == null)
				{
					List<DbObjects.IFunction> source = database.Functions.Intersect(databaseAccessibleObjects.Functions).ToList();
					functions = new(source, f => new(f, this));
				}

				foreach (Function function in functions)
				{
					yield return function;
				}
			}
		}

		private CachedEnumerable<DbObjects.ITVP, TVP> tvps;
		/// <summary>Gets the collection of user-defined table types that belong to this database.</summary>
		/// <value>Collection of user-defined table types.</value>
		public IEnumerable<TVP> TVPs {
			get {
				if (database.TVPs.IsNullOrEmpty())
				{
					yield break;
				}

				if (databaseAccessibleObjects == null || databaseAccessibleObjects.TVPs.IsNullOrEmpty())
				{
					yield break;
				}

				if (tvps == null)
				{
					List<DbObjects.ITVP> source = database.TVPs.Intersect(databaseAccessibleObjects.TVPs).ToList();
					tvps = new(source, t => new(t, this));
				}

				foreach (TVP tvp in tvps)
				{
					yield return tvp;
				}
			}
		}

		private CachedEnumerable<Exception, string> errors;
		/// <summary>Gets the collection of error messages that occurred during the generating process of this database.</summary>
		/// <value>Collection of error messages.</value>
		public IEnumerable<string> Errors {
			get {
				if (database.Errors.IsNullOrEmpty())
				{
					yield break;
				}

				errors ??= new(database.Errors, ex => {
					string errorMessage = ex.Message;
#if DEBUG
					if (!String.IsNullOrEmpty(ex.StackTrace))
					{
						errorMessage += Environment.NewLine + ex.StackTrace;
					}

					while (ex.InnerException != null)
					{
						errorMessage += Environment.NewLine + ex.InnerException.Message;
						if (!String.IsNullOrEmpty(ex.InnerException.StackTrace))
						{
							errorMessage += Environment.NewLine + ex.InnerException.StackTrace;
						}

						ex = ex.InnerException;
					}
#endif

					return errorMessage;
				});

				foreach (string error in errors)
				{
					yield return error;
				}
			}
		}

		/// <summary>Gets the name of the database.</summary>
		/// <value>The name of the database.</value>
		public string Name => database.Name;

		/// <summary>Gets the description of the database.</summary>
		/// <value>The description of the database.</value>
		public string Description => database.Description;

		/// <summary>Returns a string that represents this database.</summary>
		/// <returns>A string that represents this database.</returns>
		public override string ToString()
		{
			return database.ToString();
		}
	}
}

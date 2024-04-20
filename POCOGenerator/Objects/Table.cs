using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database table.</summary>
	public sealed class Table : IDbObject
	{
		private readonly DbObjects.ITable table;

		internal Table(DbObjects.ITable table, Database database)
		{
			this.table = table;
			Database = database;
		}

		internal bool InternalEquals(DbObjects.ITable table)
		{
			return this.table == table;
		}

		/// <summary>Gets a value indicating whether this table is a many-to-many join table.</summary>
		/// <value>
		///   <c>true</c> if this table is a many-to-many join table; otherwise, <c>false</c>.</value>
		public bool IsJoinTable => table.IsJoinTable;

		internal string ClassName => table.ClassName;

		/// <summary>Gets the error message that occurred during the generating process of this table.</summary>
		/// <value>The error message that occurred during the generating process of this table.</value>
		public string Error => table.Error?.Message;

		/// <summary>Gets the database that this table belongs to.</summary>
		/// <value>The database that this table belongs to.</value>
		public Database Database { get; private set; }

		/// <summary>Gets the collection of database columns that belong to this table.</summary>
		/// <value>Collection of database columns.</value>
		public IEnumerable<IDbColumn> Columns => TableColumns;

		private CachedEnumerable<DbObjects.ITableColumn, TableColumn> tableColumns;
		/// <summary>Gets the columns of the table.</summary>
		/// <value>The columns of the table.</value>
		public IEnumerable<TableColumn> TableColumns {
			get {
				if (table.TableColumns.IsNullOrEmpty())
				{
					yield break;
				}

				tableColumns ??= new(table.TableColumns, c => new(c, this));

				foreach (TableColumn tableColumn in tableColumns)
				{
					yield return tableColumn;
				}
			}
		}

		private PrimaryKey primaryKey;
		/// <summary>Gets the primary key of the table.
		/// <para>Returns <see langword="null" /> if the table has no primary key.</para></summary>
		/// <value>The primary key of the table.</value>
		public PrimaryKey PrimaryKey {
			get {
				if (table.PrimaryKey == null)
				{
					return null;
				}

				primaryKey ??= new(table.PrimaryKey, this);

				return primaryKey;
			}
		}

		private CachedEnumerable<DbObjects.IUniqueKey, UniqueKey> uniqueKeys;
		/// <summary>Gets the unique keys of the table.</summary>
		/// <value>The unique keys of the table.</value>
		public IEnumerable<UniqueKey> UniqueKeys {
			get {
				if (table.UniqueKeys.IsNullOrEmpty())
				{
					yield break;
				}

				uniqueKeys ??= new(table.UniqueKeys, uk => new(uk, this));

				foreach (UniqueKey uniqueKey in uniqueKeys)
				{
					yield return uniqueKey;
				}
			}
		}

		private CachedEnumerable<DbObjects.IForeignKey, ForeignKey> foreignKeys;
		/// <summary>Gets the foreign keys of the table.</summary>
		/// <value>The foreign keys of the table.</value>
		public IEnumerable<ForeignKey> ForeignKeys {
			get {
				if (table.ForeignKeys.IsNullOrEmpty())
				{
					yield break;
				}

				foreignKeys ??= new(table.ForeignKeys, fk => new(fk, this));

				foreach (ForeignKey foreignKey in foreignKeys)
				{
					yield return foreignKey;
				}
			}
		}

		private CachedEnumerable<DbObjects.INavigationProperty, NavigationProperty> navigationPropertiesSingular;
		private CachedEnumerable<DbObjects.INavigationProperty, NavigationProperty> navigationPropertiesCollection;
		private CachedEnumerable<DbObjects.INavigationProperty, NavigationProperty> virtualNavigationProperties;
		/// <summary>Gets the navigation properties of the table.</summary>
		/// <value>The navigation properties of the table.</value>
		public IEnumerable<NavigationProperty> NavigationProperties {
			get {
				if (table.ForeignKeys.IsNullOrEmpty() && table.PrimaryForeignKeys.IsNullOrEmpty())
				{
					yield break;
				}

				if (table.ForeignKeys.HasAny())
				{
					navigationPropertiesSingular ??= new(
																table.ForeignKeys.Select(fk => fk.NavigationPropertyFromForeignToPrimary),
																np => new(np, this)
															   );

					foreach (NavigationProperty navigationProperty in navigationPropertiesSingular)
					{
						yield return navigationProperty;
					}
				}

				if (table.PrimaryForeignKeys.HasAny())
				{
					navigationPropertiesCollection ??= new(
																  table.PrimaryForeignKeys.Select(fk => fk.NavigationPropertyFromPrimaryToForeign),
																  np => new(np, this)
																 );

					foreach (NavigationProperty navigationProperty in navigationPropertiesCollection)
					{
						yield return navigationProperty;
					}

					virtualNavigationProperties ??= new(
															   table.PrimaryForeignKeys.Where(fk => fk.VirtualNavigationProperties.HasAny()).SelectMany(fk => fk.VirtualNavigationProperties),
															   np => new(np, this)
															  );

					foreach (NavigationProperty virtualNavigationProperty in virtualNavigationProperties)
					{
						yield return virtualNavigationProperty;
					}
				}
			}
		}

		private CachedEnumerable<DbObjects.IIndex, TableIndex> indexes;
		/// <summary>Gets the indexes of the table.</summary>
		/// <value>The indexes of the table.</value>
		public IEnumerable<TableIndex> Indexes {
			get {
				if (table.Indexes.IsNullOrEmpty())
				{
					yield break;
				}

				indexes ??= new(table.Indexes, i => new(i, this));

				foreach (TableIndex index in indexes)
				{
					yield return index;
				}
			}
		}

		private CachedEnumerable<DbObjects.IComplexTypeTable, ComplexTypeTable> complexTypeTables;
		/// <summary>Gets the complex types of the table.</summary>
		/// <value>The complex types of the table.</value>
		public IEnumerable<ComplexTypeTable> ComplexTypeTables {
			get {
				if (table.ComplexTypeTables.IsNullOrEmpty())
				{
					yield break;
				}

				complexTypeTables ??= new(
												 table.ComplexTypeTables,
												 ctt1 => Database.ComplexTypeTables.First(ctt2 => ctt2.InternalEquals(ctt1))
												);

				foreach (ComplexTypeTable complexTypeTable in complexTypeTables)
				{
					yield return complexTypeTable;
				}
			}
		}

		/// <summary>Gets the name of the table.</summary>
		/// <value>The name of the table.</value>
		public string Name => table.Name;

		/// <summary>Gets the schema of the table.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The schema of the table.</value>
		/// <seealso cref="Support.SupportSchema" />
		public string Schema => table is DbObjects.ISchema schema ? schema.Schema : null;

		/// <summary>Gets the description of the table.</summary>
		/// <value>The description of the table.</value>
		public string Description => table.Description;

		/// <summary>Returns a string that represents this table.</summary>
		/// <returns>A string that represents this table.</returns>
		public override string ToString()
		{
			return table.ToString();
		}
	}
}

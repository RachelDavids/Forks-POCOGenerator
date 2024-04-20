using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database table column.</summary>
	public sealed class TableColumn : IDbColumn
	{
		private readonly DbObjects.ITableColumn tableColumn;

		internal TableColumn(DbObjects.ITableColumn tableColumn, Table table)
		{
			this.tableColumn = tableColumn;
			Table = table;
		}

		internal bool InternalEquals(DbObjects.ITableColumn tableColumn)
		{
			return this.tableColumn == tableColumn;
		}

		/// <summary>Gets the table that this table column belongs to.</summary>
		/// <value>The table that this table column belongs to.</value>
		public Table Table { get; private set; }

		/// <inheritdoc />
		public IDbObject DbObject => Table;

		private PrimaryKeyColumn primaryKeyColumn;
		/// <summary>Gets the primary key column associated with this table column.
		/// <para>Returns <see langword="null" /> if this table column is not a part of a primary key.</para></summary>
		/// <value>The primary key column associated with this table column.</value>
		public PrimaryKeyColumn PrimaryKeyColumn {
			get {
				if (tableColumn.PrimaryKeyColumn == null)
				{
					return null;
				}

				primaryKeyColumn ??= Table.PrimaryKey.PrimaryKeyColumns.First(pkc => pkc.InternalEquals(tableColumn.PrimaryKeyColumn));

				return primaryKeyColumn;
			}
		}

		private CachedEnumerable<DbObjects.IUniqueKeyColumn, UniqueKeyColumn> uniqueKeyColumns;
		/// <summary>Gets the columns of unique keys associated with this table column.</summary>
		/// <value>The columns of unique keys associated with this table column.</value>
		public IEnumerable<UniqueKeyColumn> UniqueKeyColumns {
			get {
				if (tableColumn.UniqueKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				uniqueKeyColumns ??= new(
												tableColumn.UniqueKeyColumns,
												c => Table.UniqueKeys
														 .SelectMany(uk => uk.UniqueKeyColumns)
														 .First(ukc => ukc.InternalEquals(c))
											   );

				foreach (UniqueKeyColumn uniqueKeyColumn in uniqueKeyColumns)
				{
					yield return uniqueKeyColumn;
				}
			}
		}

		private CachedEnumerable<DbObjects.IForeignKeyColumn, ForeignKeyColumn> foreignKeyColumns;
		/// <summary>
		/// Gets the columns of foreign keys associated with this table column.
		/// <para>This table column is the foreign table column.
		/// This table column is referencing to another table column.</para></summary>
		/// <value>The columns of foreign keys associated with this table column.</value>
		public IEnumerable<ForeignKeyColumn> ForeignKeyColumns {
			get {
				if (tableColumn.ForeignKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				foreignKeyColumns ??= new(
												 tableColumn.ForeignKeyColumns,
												 c => Table.ForeignKeys
														  .SelectMany(fk => fk.ForeignKeyColumns)
														  .First(fkc => fkc.InternalEquals(c))
												);

				foreach (ForeignKeyColumn foreignKeyColumn in foreignKeyColumns)
				{
					yield return foreignKeyColumn;
				}
			}
		}

		private CachedEnumerable<DbObjects.IForeignKeyColumn, ForeignKeyColumn> primaryForeignKeyColumns;
		/// <summary>
		/// Gets the columns of primary foreign keys associated with this table column.
		/// <para>Primary foreign key is a foreign key that is referencing from another table to the table of this column.</para>
		/// <para>This table column is the primary table column.
		/// This table column is referenced from another table column.</para></summary>
		/// <value>The columns of primary foreign keys associated with this table column.</value>
		public IEnumerable<ForeignKeyColumn> PrimaryForeignKeyColumns {
			get {
				if (tableColumn.PrimaryForeignKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				primaryForeignKeyColumns ??= new(
														tableColumn.PrimaryForeignKeyColumns,
														c => Table.Database.Tables
																 .Union(Table.Database.AccessibleTables)
																 .Where(t => t != Table)
																 .SelectMany(t => t.ForeignKeys)
																 .SelectMany(fk => fk.ForeignKeyColumns)
																 .First(fkc => fkc.InternalEquals(c))
													   );

				foreach (ForeignKeyColumn primaryForeignKeyColumn in primaryForeignKeyColumns)
				{
					yield return primaryForeignKeyColumn;
				}
			}
		}

		private CachedEnumerable<DbObjects.IIndexColumn, TableIndexColumn> indexColumns;
		/// <summary>Gets the columns of table indexes associated with this table column.</summary>
		/// <value>The columns of table indexes associated with this table column.</value>
		public IEnumerable<TableIndexColumn> IndexColumns {
			get {
				if (tableColumn.IndexColumns.IsNullOrEmpty())
				{
					yield break;
				}

				indexColumns ??= new(
											tableColumn.IndexColumns,
											c => Table.Indexes
													 .SelectMany(i => i.IndexColumns)
													 .First(ic => ic.InternalEquals(c))
										   );

				foreach (TableIndexColumn indexColumn in indexColumns)
				{
					yield return indexColumn;
				}
			}
		}

		/// <summary>Gets the complex type column associated with this table column.
		/// <para>Returns <see langword="null" /> if this table column is not a part of a complex type.</para></summary>
		/// <value>The complex type column associated with this table column.</value>
		public ComplexTypeTableColumn ComplexTypeTableColumn => tableColumn.ComplexTypeTableColumn == null
					? null
					: Table.ComplexTypeTables
					.SelectMany(ctt => ctt.ComplexTypeTableColumns)
					.First(cttc => cttc.InternalEquals(tableColumn.ComplexTypeTableColumn));

		/// <inheritdoc />
		public string ColumnName => tableColumn.ColumnName;
		/// <inheritdoc />
		public int? ColumnOrdinal => tableColumn.ColumnOrdinal;
		/// <inheritdoc />
		public string DataTypeName => tableColumn.DataTypeName;
		/// <inheritdoc />
		public string DataTypeDisplay => tableColumn.DataTypeDisplay;
		/// <inheritdoc />
		public string Precision => tableColumn.Precision;
		/// <inheritdoc />
		public int? StringPrecision => tableColumn.StringPrecision;
		/// <inheritdoc />
		public int? NumericPrecision => tableColumn.NumericPrecision;
		/// <inheritdoc />
		public int? NumericScale => tableColumn.NumericScale;
		/// <inheritdoc />
		public int? DateTimePrecision => tableColumn.DateTimePrecision;
		/// <inheritdoc />
		public bool IsUnsigned => tableColumn.IsUnsigned;
		/// <inheritdoc />
		public bool IsNullable => tableColumn.IsNullable;
		/// <inheritdoc />
		public bool IsIdentity => tableColumn.IsIdentity;
		/// <inheritdoc />
		public bool IsComputed => tableColumn.IsComputed;

		/// <summary>Gets the default value of the column.</summary>
		/// <value>The default value of the column.</value>
		public string ColumnDefault => tableColumn.ColumnDefault;

		/// <summary>Gets the description of the column.</summary>
		/// <value>The description of the column.</value>
		public string Description => tableColumn.Description;

		/// <summary>Returns a robust string that represents this column.
		/// <para>Includes information whether the column is a primary key, has foreign keys and whether is a computed column.</para></summary>
		/// <returns>A robust string that represents this column.</returns>
		public string ToFullString()
		{
			return tableColumn.ToFullString();
		}

		/// <inheritdoc cref="IDbColumn.ToString" />
		public override string ToString()
		{
			return tableColumn.ToString();
		}
	}
}

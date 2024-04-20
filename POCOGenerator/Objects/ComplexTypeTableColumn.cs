using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents entity framework complex type column.</summary>
	public sealed class ComplexTypeTableColumn : IDbColumn
	{
		private readonly DbObjects.IComplexTypeTableColumn complexTypeTableColumn;

		internal ComplexTypeTableColumn(DbObjects.IComplexTypeTableColumn complexTypeTableColumn, ComplexTypeTable complexTypeTable)
		{
			this.complexTypeTableColumn = complexTypeTableColumn;
			ComplexTypeTable = complexTypeTable;
		}

		internal bool InternalEquals(DbObjects.IComplexTypeTableColumn complexTypeTableColumn)
		{
			return this.complexTypeTableColumn == complexTypeTableColumn;
		}

		/// <summary>Gets the complex type that this complex type column belongs to.</summary>
		/// <value>The complex type that this complex type column belongs to.</value>
		public ComplexTypeTable ComplexTypeTable { get; private set; }

		/// <inheritdoc />
		public IDbObject DbObject => ComplexTypeTable;

		private CachedEnumerable<DbObjects.ITableColumn, TableColumn> tableColumns;
		/// <summary>Gets the table columns associated with this complex type column.</summary>
		/// <value>The table columns associated with this complex type column.</value>
		public IEnumerable<TableColumn> TableColumns {
			get {
				if (complexTypeTableColumn.TableColumns.IsNullOrEmpty())
				{
					yield break;
				}

				tableColumns ??= new(complexTypeTableColumn.TableColumns,
									 c1 => ComplexTypeTable.Tables
														   .First(t => t.InternalEquals(c1.Table))
														   .TableColumns
														   .First(c2 => c2.InternalEquals(c1)));

				foreach (TableColumn tableColumn in tableColumns)
				{
					yield return tableColumn;
				}
			}
		}

		/// <inheritdoc />
		public string ColumnName => complexTypeTableColumn.ColumnName;
		/// <inheritdoc />
		public int? ColumnOrdinal => complexTypeTableColumn.ColumnOrdinal;
		/// <inheritdoc />
		public string DataTypeName => complexTypeTableColumn.DataTypeName;
		/// <inheritdoc />
		public string DataTypeDisplay => complexTypeTableColumn.DataTypeDisplay;
		/// <inheritdoc />
		public string Precision => complexTypeTableColumn.Precision;
		/// <inheritdoc />
		public int? StringPrecision => complexTypeTableColumn.StringPrecision;
		/// <inheritdoc />
		public int? NumericPrecision => complexTypeTableColumn.NumericPrecision;
		/// <inheritdoc />
		public int? NumericScale => complexTypeTableColumn.NumericScale;
		/// <inheritdoc />
		public int? DateTimePrecision => complexTypeTableColumn.DateTimePrecision;
		/// <inheritdoc />
		public bool IsUnsigned => complexTypeTableColumn.IsUnsigned;
		/// <inheritdoc />
		public bool IsNullable => complexTypeTableColumn.IsNullable;
		/// <inheritdoc />
		public bool IsIdentity => complexTypeTableColumn.IsIdentity;
		/// <inheritdoc />
		public bool IsComputed => complexTypeTableColumn.IsComputed;

		/// <summary>Gets the default value of the column.</summary>
		/// <value>The default value of the column.</value>
		public string ColumnDefault => complexTypeTableColumn.ColumnDefault;

		/// <summary>Returns a robust string that represents this column.</summary>
		/// <returns>A robust string that represents this column.</returns>
		public string ToFullString()
		{
			return complexTypeTableColumn.ToFullString();
		}

		/// <inheritdoc cref="IDbColumn.ToString" />
		public override string ToString()
		{
			return complexTypeTableColumn.ToString();
		}
	}
}

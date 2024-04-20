using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database view column.</summary>
	public sealed class ViewColumn : IDbColumn
	{
		private readonly DbObjects.ITableColumn tableColumn;

		internal ViewColumn(DbObjects.ITableColumn tableColumn, View view)
		{
			this.tableColumn = tableColumn;
			View = view;
		}

		internal bool InternalEquals(DbObjects.ITableColumn tableColumn)
		{
			return this.tableColumn == tableColumn;
		}

		/// <summary>Gets the view that this view column belongs to.</summary>
		/// <value>The view that this view column belongs to.</value>
		public View View { get; private set; }

		/// <inheritdoc />
		public IDbObject DbObject => View;

		private CachedEnumerable<DbObjects.IIndexColumn, ViewIndexColumn> indexColumns;
		/// <summary>Gets the columns of view indexes associated with this view column.</summary>
		/// <value>The columns of view indexes associated with this view column.</value>
		public IEnumerable<ViewIndexColumn> IndexColumns {
			get {
				if (tableColumn.IndexColumns.IsNullOrEmpty())
				{
					yield break;
				}

				indexColumns ??= new(
											tableColumn.IndexColumns,
											c => View.Indexes.SelectMany(i => i.IndexColumns).First(ic => ic.InternalEquals(c))
										   );

				foreach (ViewIndexColumn indexColumn in indexColumns)
				{
					yield return indexColumn;
				}
			}
		}

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

		/// <summary>Returns a robust string that represents this column.</summary>
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

using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database index of a database table.</summary>
	public sealed class TableIndex : Index
	{
		internal TableIndex(DbObjects.IIndex index, Table table)
			: base(index)
		{
			Table = table;
		}

		/// <summary>Gets the table that this table index belongs to.</summary>
		/// <value>The table that this table index belongs to.</value>
		public Table Table { get; private set; }

		private CachedEnumerable<DbObjects.IIndexColumn, TableIndexColumn> indexColumns;
		/// <summary>Gets the columns of the table index.</summary>
		/// <value>The columns of the table index.</value>
		public IEnumerable<TableIndexColumn> IndexColumns {
			get {
				if (index.IndexColumns.IsNullOrEmpty())
				{
					yield break;
				}

				indexColumns ??= new(index.IndexColumns, ic => new(ic, this));

				foreach (TableIndexColumn indexColumn in indexColumns)
				{
					yield return indexColumn;
				}
			}
		}
	}
}

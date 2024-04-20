using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database primary key of a database table.</summary>
	public sealed class PrimaryKey
	{
		private readonly DbObjects.IPrimaryKey primaryKey;

		internal PrimaryKey(DbObjects.IPrimaryKey primaryKey, Table table)
		{
			this.primaryKey = primaryKey;
			Table = table;
		}

		internal bool InternalEquals(DbObjects.IPrimaryKey primaryKey)
		{
			return this.primaryKey == primaryKey;
		}

		/// <summary>Gets the table that this primary key belongs to.</summary>
		/// <value>The table that this primary key belongs to.</value>
		public Table Table { get; private set; }

		/// <summary>Gets the name of the primary key.</summary>
		/// <value>The name of the primary key.</value>
		public string Name => primaryKey.Name;

		private CachedEnumerable<DbObjects.IPrimaryKeyColumn, PrimaryKeyColumn> primaryKeyColumns;
		/// <summary>Gets the columns of the primary key.</summary>
		/// <value>The columns of the primary key.</value>
		public IEnumerable<PrimaryKeyColumn> PrimaryKeyColumns {
			get {
				if (primaryKey.PrimaryKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				primaryKeyColumns ??= new(primaryKey.PrimaryKeyColumns, pkc => new(pkc, this));

				foreach (PrimaryKeyColumn primaryKeyColumn in primaryKeyColumns)
				{
					yield return primaryKeyColumn;
				}
			}
		}

		/// <summary>Returns a string that represents this primary key.</summary>
		/// <returns>A string that represents this primary key.</returns>
		public override string ToString()
		{
			return primaryKey.ToString();
		}
	}
}

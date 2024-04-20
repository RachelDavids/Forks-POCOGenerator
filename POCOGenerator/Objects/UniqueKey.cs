using System.Collections.Generic;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database unique key of a database table.</summary>
	public sealed class UniqueKey
	{
		private readonly DbObjects.IUniqueKey uniqueKey;

		internal UniqueKey(DbObjects.IUniqueKey uniqueKey, Table table)
		{
			this.uniqueKey = uniqueKey;
			Table = table;
		}

		internal bool InternalEquals(DbObjects.IUniqueKey uniqueKey)
		{
			return this.uniqueKey == uniqueKey;
		}

		/// <summary>Gets the table that this unique key belongs to.</summary>
		/// <value>The table that this unique key belongs to.</value>
		public Table Table { get; private set; }

		/// <summary>Gets the name of the unique key.</summary>
		/// <value>The name of the unique key.</value>
		public string Name => uniqueKey.Name;

		private CachedEnumerable<DbObjects.IUniqueKeyColumn, UniqueKeyColumn> uniqueKeyColumns;
		/// <summary>Gets the columns of the unique key.</summary>
		/// <value>The columns of the unique key.</value>
		public IEnumerable<UniqueKeyColumn> UniqueKeyColumns {
			get {
				if (uniqueKey.UniqueKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				uniqueKeyColumns ??= new(uniqueKey.UniqueKeyColumns, ukc => new(ukc, this));

				foreach (UniqueKeyColumn uniqueKeyColumn in uniqueKeyColumns)
				{
					yield return uniqueKeyColumn;
				}
			}
		}

		/// <summary>Returns a string that represents this unique key.</summary>
		/// <returns>A string that represents this unique key.</returns>
		public override string ToString()
		{
			return uniqueKey.ToString();
		}
	}
}

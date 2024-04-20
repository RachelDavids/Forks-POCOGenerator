using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a database foreign key between database tables.</summary>
	public sealed class ForeignKey
	{
		private readonly DbObjects.IForeignKey foreignKey;

		internal ForeignKey(DbObjects.IForeignKey foreignKey, Table foreignTable)
		{
			this.foreignKey = foreignKey;
			ForeignTable = foreignTable;
		}

		internal bool InternalEquals(DbObjects.IForeignKey foreignKey)
		{
			return this.foreignKey == foreignKey;
		}

		/// <summary>Gets the table that this foreign key belongs to.</summary>
		/// <value>The table that this foreign key belongs to.</value>
		public Table ForeignTable { get; private set; }

		/// <summary>Gets the name of the foreign key.</summary>
		/// <value>The name of the foreign key.</value>
		public string Name => foreignKey.Name;

		/// <summary>
		/// Gets a value indicating whether this foreign key is part of a one-to-one relationship between tables.
		/// <para>One-to-one relationship is when the primary key of one table is also a foreign key to the primary key of another table.</para></summary>
		/// <value>
		///   <c>true</c> if this foreign key is part of a one-to-one relationship between tables; otherwise, <c>false</c>.</value>
		public bool IsOneToOne => foreignKey.Is_One_To_One;

		/// <summary>
		/// Gets a value indicating whether this foreign key is part of a one-to-many relationship between tables.
		/// <para>A single foreign key, without any special constraints, is a database implementation of a one-to-many relationship between two tables.</para></summary>
		/// <value>
		///   <c>true</c> if this foreign key is part of a one-to-many relationship between tables; otherwise, <c>false</c>.</value>
		public bool IsOneToMany => foreignKey.Is_One_To_Many;

		/// <summary>
		/// Gets a value indicating whether this foreign key is part of a many-to-many relationship between tables.
		/// <para>Many-to-many relationship is when two or more entities have multiple references to all the other entities in the relationship.</para></summary>
		/// <value>
		///   <c>true</c> if this foreign key is part of a many-to-many relationship between tables; otherwise, <c>false</c>.</value>
		public bool IsManyToMany => foreignKey.Is_Many_To_Many;

		/// <summary>
		/// Gets a value indicating whether this foreign key is part of a complete many-to-many relationship between tables.
		/// <para>Complete many-to-many relationship is when all the columns, in the join table, are part of a primary key of other tables.
		/// If there is at least one more column, in the join table, that is not part of the primary key, then it is not a complete many-to-many relationship.</para></summary>
		/// <value>
		///   <c>true</c> if this foreign key is part of a complete many-to-many relationship between tables; otherwise, <c>false</c>.</value>
		public bool IsManyToManyComplete => foreignKey.Is_Many_To_Many_Complete;

		/// <summary>Gets a value indicating whether this foreign key has an option of cascade delete enabled.</summary>
		/// <value>
		///   <c>true</c> if this foreign key has an option of cascade delete enabled; otherwise, <c>false</c>.</value>
		public bool IsCascadeDelete => foreignKey.Is_Cascade_Delete;

		/// <summary>Gets a value indicating whether this foreign key has an option of cascade update enabled.</summary>
		/// <value>
		///   <c>true</c> if this foreign key has an option of cascade update enabled; otherwise, <c>false</c>.</value>
		public bool IsCascadeUpdate => foreignKey.Is_Cascade_Update;

		private Table primaryTable;
		/// <summary>Gets the table that this foreign key referencing to.</summary>
		/// <value>The table that this foreign key referencing to.</value>
		public Table PrimaryTable {
			get {
				if (foreignKey.PrimaryTable == null)
				{
					return null;
				}

				primaryTable ??= ForeignTable.Database
											 .Tables
											 .Union(ForeignTable.Database.AccessibleTables)
											 .First(t => t.InternalEquals(foreignKey.PrimaryTable));

				return primaryTable;
			}
		}

		private CachedEnumerable<DbObjects.IForeignKeyColumn, ForeignKeyColumn> foreignKeyColumns;
		/// <summary>Gets the columns of the foreign key.</summary>
		/// <value>The columns of the foreign key.</value>
		public IEnumerable<ForeignKeyColumn> ForeignKeyColumns {
			get {
				if (foreignKey.ForeignKeyColumns.IsNullOrEmpty())
				{
					yield break;
				}

				foreignKeyColumns ??= new(foreignKey.ForeignKeyColumns, fkc => new(fkc, this));

				foreach (ForeignKeyColumn foreignKeyColumn in foreignKeyColumns)
				{
					yield return foreignKeyColumn;
				}
			}
		}

		/// <summary>Returns a string that represents this foreign key.</summary>
		/// <returns>A string that represents this foreign key.</returns>
		public override string ToString()
		{
			return foreignKey.ToString();
		}
	}
}

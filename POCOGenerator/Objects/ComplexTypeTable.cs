using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents entity framework complex type.</summary>
	public sealed class ComplexTypeTable : IDbObject
	{
		private readonly DbObjects.IComplexTypeTable complexTypeTable;

		internal ComplexTypeTable(DbObjects.IComplexTypeTable complexTypeTable, Database database)
		{
			this.complexTypeTable = complexTypeTable;
			Database = database;
		}

		internal bool InternalEquals(DbObjects.IComplexTypeTable complexTypeTable)
		{
			return this.complexTypeTable == complexTypeTable;
		}

		internal string ClassName => complexTypeTable.ClassName;

		/// <summary>Gets the error message that occurred during the generating process of this complex type.</summary>
		/// <value>The error message that occurred during the generating process of this complex type.</value>
		public string Error => complexTypeTable.Error?.Message;

		/// <summary>Gets the database that this complex type belongs to.</summary>
		/// <value>The database that this complex type belongs to.</value>
		public Database Database { get; private set; }

		/// <summary>Gets the collection of database columns that belong to this complex type.</summary>
		/// <value>Collection of database columns.</value>
		public IEnumerable<IDbColumn> Columns => ComplexTypeTableColumns;

		private CachedEnumerable<DbObjects.IComplexTypeTableColumn, ComplexTypeTableColumn> complexTypeTableColumns;
		/// <summary>Gets the columns of the complex type.</summary>
		/// <value>The columns of the complex type.</value>
		public IEnumerable<ComplexTypeTableColumn> ComplexTypeTableColumns {
			get {
				if (complexTypeTable.ComplexTypeTableColumns.IsNullOrEmpty())
				{
					yield break;
				}

				complexTypeTableColumns ??= new(complexTypeTable.ComplexTypeTableColumns, c => new(c, this));

				foreach (ComplexTypeTableColumn complexTypeTableColumn in complexTypeTableColumns)
				{
					yield return complexTypeTableColumn;
				}
			}
		}

		private CachedEnumerable<DbObjects.ITable, Table> tables;
		/// <summary>Gets the tables of the complex type.</summary>
		/// <value>The tables of the complex type.</value>
		public IEnumerable<Table> Tables {
			get {
				if (complexTypeTable.Tables.IsNullOrEmpty())
				{
					yield break;
				}

				tables ??= new(
							   complexTypeTable.Tables,
							   t1 => Database.Tables.First(t2 => t2.InternalEquals(t1))
							  );

				foreach (Table table in tables)
				{
					yield return table;
				}
			}
		}

		/// <summary>Gets the name of the complex type.</summary>
		/// <value>The name of the complex type.</value>
		public string Name => complexTypeTable.Name;

		/// <summary>Gets the schema of the complex type.
		/// <para>Returns <see langword="null" /> if the RDBMS doesn't support schema.</para></summary>
		/// <value>The schema of the complex type.</value>
		/// <seealso cref="Support.SupportSchema" />
		public string Schema => complexTypeTable is DbObjects.ISchema schema ? schema.Schema : null;

		/// <summary>Gets the description of the complex type.</summary>
		/// <value>Returns <see langword="null" />.</value>
		public string Description => null;

		/// <summary>Returns a string that represents this complex type.</summary>
		/// <returns>A string that represents this complex type.</returns>
		public override string ToString()
		{
			return complexTypeTable.ToString();
		}
	}
}

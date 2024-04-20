using System;
using System.Collections.Generic;
using System.Linq;

using POCOGenerator.DbObjects;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class Table : ITable, ISchema
	{
		public string table_schema { get; set; }
		public string table_name { get; set; }

		public IDatabase Database { get; set; }
		public List<ITableColumn> TableColumns { get; set; }
		public IPrimaryKey PrimaryKey { get; set; }
		public List<IUniqueKey> UniqueKeys { get; set; }
		public List<IForeignKey> ForeignKeys { get; set; }
		public List<IForeignKey> PrimaryForeignKeys { get; set; }
		public List<IIndex> Indexes { get; set; }
		public List<IComplexTypeTable> ComplexTypeTables { get; set; }
		public bool IsJoinTable { get; set; }
		public Exception Error { get; set; }
		public string ClassName { get; set; }

		public string Description { get; set; }

		public override string ToString()
		{
			return Schema + "." + Name;
		}

		public string Schema => table_schema;
		public string Name => table_name;
		public IEnumerable<IColumn> Columns => TableColumns?.Cast<IColumn>();
		public virtual DbObjectType DbObjectType => DbObjectType.Table;
	}
}

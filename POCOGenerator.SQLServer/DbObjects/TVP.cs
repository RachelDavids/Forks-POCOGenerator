using System;
using System.Collections.Generic;
using System.Data;

using POCOGenerator.DbObjects;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class TVP : ITVP, ISchema
	{
		public string tvp_schema { get; set; }
		public string tvp_name { get; set; }
		public int type_table_object_id { get; set; }

		public IDatabase Database { get; set; }
		public List<ITVPColumn> TVPColumns { get; set; }
		public string Description { get; set; }
		public Exception Error { get; set; }
		public string ClassName { get; set; }

		public int TVPId => type_table_object_id;
		public DataTable TVPDataTable { get; set; }

		public override string ToString()
		{
			return $"{Schema}.{Name}";
		}

		public string Schema => tvp_schema;
		public string Name => tvp_name;
		public IEnumerable<IColumn> Columns => TVPColumns;
		public DbObjectType DbObjectType => DbObjectType.TVP;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

using POCOGenerator.DbObjects;

namespace POCOGenerator.SQLServer.DbObjects
{
	internal class Procedure
		: IProcedure, ISchema
	{
		public string routine_schema { get; set; }
		public string routine_name { get; set; }

		public IDatabase Database { get; set; }
		public List<IProcedureParameter> ProcedureParameters { get; set; }
		public List<IProcedureColumn> ProcedureColumns { get; set; }
		public string Description { get; set; }
		public Exception Error { get; set; }
		public string ClassName { get; set; }

		public override string ToString()
		{
			return Schema + "." + Name;
		}

		public string Schema => routine_schema;
		public string Name => routine_name;
		public IEnumerable<IColumn> Columns => ProcedureColumns?.Cast<IColumn>();
		public virtual DbObjectType DbObjectType => DbObjectType.Procedure;
	}
}

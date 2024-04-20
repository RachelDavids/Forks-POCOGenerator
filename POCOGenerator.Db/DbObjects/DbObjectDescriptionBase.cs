using System;

using POCOGenerator.DbObjects;

namespace POCOGenerator.Db.DbObjects
{
	public abstract class DbObjectDescriptionBase : IDbObjectDescription
	{
		public virtual string Type { get; set; }
		public virtual string Name { get; set; }
		public virtual string Minor_Name { get; set; }
		public virtual string Description { get; set; }

		public virtual DbObjectType ObjectType {
			get {
				if (String.IsNullOrEmpty(Type))
				{
					return DbObjectType.None;
				}
				// prevent collapse
				return Type.ToLower() switch {
					"database" => DbObjectType.Database,
					"schema" => DbObjectType.Schema,
					"table" => DbObjectType.Table,
					"view" => DbObjectType.View,
					"tablecolumn" => DbObjectType.Column,
					"viewcolumn" => DbObjectType.Column,
					"index" => DbObjectType.Index,
					"procedure" => DbObjectType.Procedure,
					"function" => DbObjectType.Function,
					"procedureparameter" => DbObjectType.ProcedureParameter,
					"functionparameter" => DbObjectType.ProcedureParameter,
					"tvp" => DbObjectType.TVP,
					"tvpcolumn" => DbObjectType.TVPColumn,
					_ => DbObjectType.None,
				};
			}
		}
	}
}

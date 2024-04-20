using System.Collections.Generic;

using POCOGenerator.DbObjects;

namespace POCOGenerator.Db.DbObjects
{
	public abstract class DbSupportBase
		: IDbSupport
	{
		protected readonly Dictionary<string, bool> support = new()
															  {
																  { "IsSupportSchema", false },
																  { "IsSupportTableFunctions", false },
																  { "IsSupportTVPs", false },
																  { "IsSupportEnumDataType", false }
															  };

		public virtual bool this[string key] {
			get => support[key];
			set => support[key] = value;
		}

		public virtual bool IsSupportSchema { get => support["IsSupportSchema"]; set => support["IsSupportSchema"] = value; }
		public virtual bool IsSupportTableFunctions { get => support["IsSupportTableFunctions"]; set => support["IsSupportTableFunctions"] = value; }
		public virtual bool IsSupportTVPs { get => support["IsSupportTVPs"]; set => support["IsSupportTVPs"] = value; }
		public virtual bool IsSupportEnumDataType { get => support["IsSupportEnumDataType"]; set => support["IsSupportEnumDataType"] = value; }

		public virtual string DefaultSchema { get; set; }
		public virtual Version Version { get; set; }
	}
}

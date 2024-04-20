using POCOGenerator.Db.DbObjects;

namespace POCOGenerator.SQLServer
{
	public sealed class SQLServerSupport
		: DbSupportBase
	{
		internal SQLServerSupport()
		{
			IsSupportSchema = true;
			IsSupportTableFunctions = true;
			IsSupportTVPs = true;
			IsSupportEnumDataType = false;
			DefaultSchema = "dbo";
		}
	}
}

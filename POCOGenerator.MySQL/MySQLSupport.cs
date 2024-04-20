using POCOGenerator.Db.DbObjects;

namespace POCOGenerator.MySQL
{
	public sealed class MySQLSupport
		: DbSupportBase
	{
		internal MySQLSupport()
		{
			IsSupportSchema = false;
			IsSupportTableFunctions = false;
			IsSupportTVPs = false;
			IsSupportEnumDataType = true;
		}
	}
}

using POCOGenerator.DbHandlers;

namespace POCOGenerator.DbFactories
{
	public sealed class DbFactory
	{
		private DbFactory() { }

		public static DbFactory Instance => SingletonCreator.s_instance;

		private class SingletonCreator
		{
			internal static readonly DbFactory s_instance = new();
		}

		public IDbHandler SQLServerHandler => SQLServer.SQLServerHandler.Instance;

		public IDbHandler MySQLHandler => MySQL.MySQLHandler.Instance;
	}
}

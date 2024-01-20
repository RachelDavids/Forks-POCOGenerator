using POCOGenerator.DbHandlers;
using POCOGenerator.DbObjects;
using POCOGenerator.POCOIterators;
using POCOGenerator.POCOWriters;

namespace POCOGenerator.SQLServer
{
	public sealed class SQLServerHandler : IDbHandler
	{
		private static SQLServerHandler _instance;
		public static SQLServerHandler Instance => _instance ??= new();
		private SQLServerHandler() { }

		public IDbHelper GetDbHelper(string connectionString) => new SQLServerHelper(connectionString);

		public IConnectionStringParser GetConnectionStringParser() => SQLServerConnectionStringParser.Instance;

		public IServer GetServer() => new SQLServer.DbObjects.SQLServer();

		public IDbIterator GetIterator(IWriter writer, IDbSupport support, IDbIteratorSettings settings) => new SQLServerIterator(writer, support, settings);
	}
}

using POCOGenerator.DbHandlers;
using POCOGenerator.DbObjects;
using POCOGenerator.POCOIterators;
using POCOGenerator.POCOWriters;

namespace POCOGenerator.MySQL
{
	public sealed class MySQLHandler : IDbHandler
	{
		private MySQLHandler() { }

		public static MySQLHandler Instance => SingletonCreator.s_instance;

		private sealed class SingletonCreator
		{
			internal static readonly MySQLHandler s_instance = new();
		}

		public IDbHelper GetDbHelper(string connectionString)
		{
			return new MySQLHelper(connectionString);
		}

		public IConnectionStringParser GetConnectionStringParser()
		{
			return MySQLConnectionStringParser.Instance;
		}

		public IServer GetServer()
		{
			return new MySQL.DbObjects.MySQL();
		}

		public IDbIterator GetIterator(IWriter writer, IDbSupport support, IDbIteratorSettings settings)
		{
			return new MySQLIterator(writer, support, settings);
		}
	}
}

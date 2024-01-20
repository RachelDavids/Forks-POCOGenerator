using System.Data.SqlClient;
using POCOGenerator.Db;

namespace POCOGenerator.SQLServer
{
	internal sealed class SQLServerConnectionStringParser
		: ConnectionStringParser
	{
		private static SQLServerConnectionStringParser _instance;
		public static SQLServerConnectionStringParser Instance => _instance ??= new();
		private SQLServerConnectionStringParser() { }

		public override void Parse(string connectionString,
								   ref string serverName,
								   ref string initialDatabase,
								   ref string userId,
								   ref bool integratedSecurity)
		{
			SqlConnectionStringBuilder conn = new(connectionString);
			serverName = conn.DataSource;
			initialDatabase = conn.InitialCatalog;
			userId = conn.UserID;
			integratedSecurity = conn.IntegratedSecurity;
		}

		public override bool Ping(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				return false;
			}

			connectionString = SetConnectionTimeoutTo120Seconds(connectionString);

			using (SqlConnection connection = new(connectionString))
			{
				try
				{
					connection.Open();
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
	}
}

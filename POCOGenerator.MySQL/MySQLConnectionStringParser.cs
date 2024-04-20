using System;

using MySql.Data.MySqlClient;

using POCOGenerator.Db;

namespace POCOGenerator.MySQL
{
	internal sealed class MySQLConnectionStringParser
		: ConnectionStringParser
	{
		private MySQLConnectionStringParser() { }

		public static MySQLConnectionStringParser Instance => SingletonCreator.s_instance;

		private class SingletonCreator
		{
			static SingletonCreator() { }
			internal static readonly MySQLConnectionStringParser s_instance = new();
		}

		public override void Parse(string connectionString, ref string serverName, ref string initialDatabase, ref string userId, ref bool integratedSecurity)
		{
			MySqlConnectionStringBuilder conn = new(connectionString);
			serverName = conn.Server;
			initialDatabase = conn.Database;
			userId = conn.UserID;
			integratedSecurity = conn.IntegratedSecurity;
		}

		public override bool Ping(string connectionString)
		{
			if (String.IsNullOrEmpty(connectionString))
			{
				return false;
			}

			connectionString = SetConnectionTimeoutTo120Seconds(connectionString);

			using (MySqlConnection connection = new(connectionString))
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

		public override string Fix(string connectionString)
		{
			if (connectionString.IndexOf("Allow User Variables", StringComparison.OrdinalIgnoreCase) == -1)
			{
				connectionString = connectionString.TrimEnd(';', ' ') + ";Allow User Variables=true";
			}
			else
			{
				string allowUserVariables = "Allow User Variables=false";
				int index = connectionString.IndexOf(allowUserVariables, StringComparison.OrdinalIgnoreCase);
				if (index != -1)
				{
					connectionString = connectionString.Remove(index, allowUserVariables.Length).Insert(index, "Allow User Variables=true");
				}
			}

			return connectionString;
		}
	}
}

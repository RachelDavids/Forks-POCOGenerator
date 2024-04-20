using System.Collections.Generic;
using System.Linq;

using POCOGenerator.Utils;

namespace POCOGenerator.Objects
{
	/// <summary>Represents a RDBMS server.</summary>
	public sealed class Server
	{
		private readonly DbObjects.IServer server;
		private readonly List<DatabaseAccessibleObjects> databasesAccessibleObjects;

		internal Server(DbObjects.IServer server, List<DatabaseAccessibleObjects> databasesAccessibleObjects)
		{
			this.server = server;
			this.databasesAccessibleObjects = databasesAccessibleObjects;
		}

		internal bool InternalEquals(DbObjects.IServer server)
		{
			return this.server == server;
		}

		/// <summary>Gets the name of the server.</summary>
		/// <value>The name of the server.</value>
		public string ServerName => server.ServerName;

		/// <summary>Gets the name of the instance of the server.</summary>
		/// <value>The name of the instance of the server.</value>
		public string InstanceName => server.InstanceName;

		/// <summary>Gets the user identifier connected to the server.</summary>
		/// <value>The user identifier connected to the server.</value>
		public string UserId => server.UserId;

		private Version version;
		/// <summary>Gets the version of the server.</summary>
		/// <value>The version of the server.</value>
		public Version Version {
			get {
				if (server.Version == null)
				{
					return null;
				}

				version ??= new(server.Version);

				return version;
			}
		}

		private CachedEnumerable<DbObjects.IDatabase, Database> databases;
		/// <summary>Gets the collection of databases that belong to this server.</summary>
		/// <value>Collection of databases.</value>
		public IEnumerable<Database> Databases {
			get {
				if (server.Databases.IsNullOrEmpty())
				{
					yield break;
				}

				databases ??= new(
										 server.Databases.Where(d => databasesAccessibleObjects.Any(x => x.Database == d)).ToList(),
										 d => new(d, this, databasesAccessibleObjects.First(x => x.Database == d))
										);

				foreach (Database database in databases)
				{
					yield return database;
				}
			}
		}

		/// <summary>Returns the fully qualified server name with the version of the server.</summary>
		/// <returns>The fully qualified server name with the version of the server.</returns>
		public string ToStringWithVersion()
		{
			return server.ToStringWithVersion();
		}

		/// <summary>Returns the fully qualified server name.</summary>
		/// <returns>The fully qualified server name.</returns>
		public override string ToString()
		{
			return server.ToString();
		}
	}
}

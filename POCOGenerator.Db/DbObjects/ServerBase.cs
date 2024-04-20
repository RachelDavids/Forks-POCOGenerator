using System.Collections.Generic;

using POCOGenerator.DbObjects;

namespace POCOGenerator.Db.DbObjects
{
	public abstract class ServerBase : IServer
	{
		public virtual string RDBMSName { get; set; }

		public virtual string ServerName { get; set; }
		public virtual string InstanceName { get; set; }
		public virtual Version Version { get; set; }
		public virtual string UserId { get; set; }

		public virtual List<IDatabase> Databases { get; set; }

		public override string ToString()
		{
			return ServerName + (System.String.IsNullOrEmpty(InstanceName) ? System.String.Empty : "\\" + InstanceName);
		}

		public virtual string ToStringWithVersion()
		{
			string serverName = ToString();
			if (Version != null)
			{
				serverName += $" ({RDBMSName} {Version.ToString(3)}{(System.String.IsNullOrEmpty(UserId) ? System.String.Empty : " - " + UserId)})";
			}
			return serverName;
		}
	}
}

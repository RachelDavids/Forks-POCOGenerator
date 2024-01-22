using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Versioning;
using Microsoft.Data.Sql;
using Microsoft.Win32;
using Rachel.Core.Win32;
using RegistryHive = Rachel.Core.Win32.RegistryHive;

namespace Rachel.Data.Sql
{
	/// <summary>
	/// Class InstanceCollector. This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// See <see href="https://stackoverflow.com/a/13993652/4694175">HERE</see> for further details
	/// </remarks>
	[SupportedOSPlatform("windows")]
	public sealed class InstanceLocator
	{
		private List<string> sqlInstances = [];

		private void collectInstances()
		{
			while (true)
			{
				SqlDataSourceEnumerator instance = SqlDataSourceEnumerator.Instance;
				DataTable dataTable = instance.GetDataSources();
				foreach (DataRow row in dataTable.Rows)
				{
					string instanceName =
						string.Format(@"{0}\{1}", row["ServerName"].ToString(), row["InstanceName"].ToString());

					//Do not add the local instance, we will add it in the next section. Otherwise, duplicated!
					if (!sqlInstances.Contains(instanceName) && !instanceName.Contains(Environment.MachineName))
					{
						sqlInstances.Add(instanceName);
					}
				}
				/*
					 * For some reason, GetDataSources() does not get local instances. So using code from here to get them
					 * http://stackoverflow.com/questions/6824188/sqldatasourceenumerator-instance-getdatasources-does-not-locate-local-sql-serv
					 */
				List<string> lclInstances = GetLocalSqlServerInstanceNames();
				foreach (string lclInstance in lclInstances)
				{
					string instanceName = $@"{Environment.MachineName}\{lclInstance}";
					if (!sqlInstances.Contains(instanceName))
					{
						sqlInstances.Add(instanceName);
					}
				}
				sqlInstances.Sort();
			}
		}

		//Got code from: http://stackoverflow.com/questions/6824188/sqldatasourceenumerator-instance-getdatasources-does-not-locate-local-sql-serv
		/// <summary>
		///  get local sql server instance names from registry, search both WOW64 and WOW3264 hives
		/// </summary>
		/// <returns>a list of local sql server instance names</returns>
		public static List<string> GetLocalSqlServerInstanceNames()
		{
			// DESKTOP-1O03T0G\RDDBDEV01
			//RegistryValueDataReader registryValueDataReader = new RegistryValueDataReader();
			string[] instances64Bit = RegistryValueDataReader.ReadRegistryValueData(RegistryHive.Wow64,
																					Registry.LocalMachine,
																					@"SOFTWARE\Microsoft\Microsoft SQL Server",
																					"InstalledInstances");
			return [.. instances64Bit];
		}

		public static List<string> GetLocalServerQualifiedInstanceNames()
		{
			List<string> names = GetLocalSqlServerInstanceNames();
			return names.Select(s => $@"{Environment.MachineName}\{s}").ToList();
		}
	}
}

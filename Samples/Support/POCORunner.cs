using System;
using System.Collections.Generic;
using System.Text;
using POCOGenerator;
using Rachel.Data.Sql;

namespace Samples.Support
{
	public class POCORunner
	{
		private readonly Func<IGenerator> _generatorFactory;
		private readonly string _targetDbName;

		public POCORunner(Func<IGenerator> factory, string targetDbName)
		{
			_generatorFactory = factory;
			_targetDbName = targetDbName;
		}

		public IGenerator Initialize(Action<ISettings> settingsUpdate = null)
		{
			IGenerator generator = _generatorFactory();
			ISettings settings = generator.Settings;
			IConnection connection = settings.Connection;
			connection.ConnectionString = FindLocalDb(_targetDbName);
			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";
			className.IgnoreDboSchema = true;
			settingsUpdate?.Invoke(settings);
			return generator;
		}

		private static void PrintError(GeneratorResults results, Exception error)
		{
			bool isError = (results & GeneratorResults.Error) == GeneratorResults.Error;
			bool isWarning = (results & GeneratorResults.Warning) == GeneratorResults.Warning;

			if (results != GeneratorResults.None)
			{
				Console.WriteLine();
			}
			if (isError)
			{
				Console.WriteLine("Error Result: {0}", results);
			}
			else if (isWarning)
			{
				Console.WriteLine("Warning Result: {0}", results);
			}
			if (error != null)
			{
				Console.WriteLine("Error: {0}", error.Message);
				Console.WriteLine("Error Stack Trace:");
				Console.WriteLine(error.StackTrace);
			}
		}
		private static string FindLocalDb(string name)
		{
			StringBuilder sb = new();
			List<string> srv = InstanceLocator.GetLocalServerQualifiedInstanceNames();
			string serverName = srv.Count > 0 ? srv[0] : "DESKTOP-1O03T0G\\RDDBDEV01";
			sb.Append("Data Source=");
			sb.Append(serverName);
			sb.Append(";Initial Catalog=");
			sb.Append(name);
			sb.Append(";Integrated Security=True");
			return sb.ToString();
		}

		public void Run(IGenerator generator)
		{
			GeneratorResults results = generator.Generate();
			PrintError(results, generator.Error);
		}
	}
}

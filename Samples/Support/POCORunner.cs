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
		public IGenerator Generator { get; private set; }
		public ISettings GeneratorSettings => Generator.Settings;

		public POCORunner(Func<IGenerator> factory, string targetDbName)
		{
			_generatorFactory = factory;
			_targetDbName = targetDbName;
		}

		public IGenerator Initialize() => Initialize(ApplyDefaults);
		public IGenerator Initialize(Action<ISettings> settingsUpdate)
		{
			ArgumentNullException.ThrowIfNull(settingsUpdate);
			IGenerator generator = _generatorFactory();
			ISettings settings = generator.Settings;
			IConnection connection = settings.Connection;
			connection.ConnectionString = FindLocalDb(_targetDbName);
			settingsUpdate.Invoke(settings);
			Generator = generator;
			return generator;
		}

		public static void ApplyDefaults(ISettings settings)
		{
			ArgumentNullException.ThrowIfNull(settings);
			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";
			className.IgnoreDboSchema = true;
		}

		public static void PrintError(GeneratorResults results, Exception error)
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
		public static string FindLocalDb(string name)
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

		/// <summary>Connects to the RDBMS server and generates POCO classes from
		/// selected database objects.</summary>
		public void Run()
		{
			GeneratorResults results = Generator.Generate();
			PrintError(results, Generator.Error);
		}

		/// <summary>Generates POCO classes from previously built class objects
		/// without connecting to the RDBMS server again.
		/// <para>Falls back to <see cref="IGenerator.Generate"/> if no class
		/// objects were built previously.</para></summary>
		public void RunPOCOs()
		{
			GeneratorResults results = Generator.GeneratePOCOs();
			PrintError(results, Generator.Error);
		}
	}
}

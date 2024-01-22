using System;
using System.Collections.Generic;
using System.Text;
using POCOGenerator;
using Rachel.Data.Sql;
using Samples.Support;

namespace ConsoleColorDemo
{
	internal static class Program
	{
		private static void Main()
		{
			Console.ForegroundColor = ConsoleColor.Red;

			POCORunner runner = new(GeneratorFactory.GetConsoleColorGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize(s => s.EFAnnotations.Enable = true);
			runner.Run(generator);
			// don't need to prompt since VS does it automatically
			// and causes issues if you wish to redirect
		}

		private static IGenerator Initialize(Func<IGenerator> generatorFactory)
		{
			IGenerator generator = generatorFactory();
			ISettings settings = generator.Settings;
			IConnection connection = settings.Connection;
			//try
			//{
			//	connection.ConnectionString = File.ReadAllText("ConnectionString.txt");
			//}
			//catch { }
			if (string.IsNullOrEmpty(connection.ConnectionString))
			{
				connection.ConnectionString = FindLocalDb("AdventureWorks");
			}

			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";
			className.IgnoreDboSchema = true;
			settings.EFAnnotations.Enable = true;
			return generator;
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
		private static void PrintError(GeneratorResults results, Exception Error)
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

			if (Error != null)
			{
				Console.WriteLine("Error: {0}", Error.Message);
				Console.WriteLine("Error Stack Trace:");
				Console.WriteLine(Error.StackTrace);
			}
		}
	}
}

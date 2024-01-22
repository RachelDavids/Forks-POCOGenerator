using System;
using System.Collections.Generic;
using System.Text;
using POCOGenerator;
using Rachel.Data.Sql;

namespace ComplexTypesDemo
{
	internal class Program
	{
		private static void Main()
		{
			// execute script ComplexTypesDB.sql to create ComplexTypesDB database

			IGenerator generator = Initialize(GeneratorFactory.GetConsoleGenerator);
			GeneratorResults results = generator.Generate();
			PrintError(results, generator.Error);

			Console.WriteLine();
			Console.WriteLine("Press any key to re-generate with complex types");
			Console.ReadKey(true);

			generator.Settings.Reset();

			generator.Settings.POCO.ComplexTypes = true;

			results = generator.GeneratePOCOs();
			PrintError(results, generator.Error);

			Console.WriteLine();
			Console.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
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
				connection.ConnectionString = FindLocalDb("ComplexTypesDB");
			}
			settings.DatabaseObjects.Tables.IncludeAll = true;
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
	}
}

using System;
using System.IO;
using System.Text;
using POCOGenerator;

namespace ConsoleDemo
{
	internal class Program
	{
		private static void Main()
		{
			IGenerator generator = Initialize();
			GeneratorResults results = generator.Generate();

			PrintError(results, generator.Error);

			Console.WriteLine();
			Console.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
		}

		private static IGenerator Initialize()
		{
			IGenerator generator = GeneratorFactory.GetConsoleGenerator();
			ISettings settings = generator.Settings;
			IConnection connection = settings.Connection;
			try
			{
				connection.ConnectionString = File.ReadAllText("ConnectionString.txt");
			}
			catch { }
			if (string.IsNullOrEmpty(connection.ConnectionString))
			{
				connection.ConnectionString = FindLocalDb("AdventureWorks");
			}

			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			settings.ClassName.IncludeSchema = true;
			settings.ClassName.SchemaSeparator = "_";
			settings.ClassName.IgnoreDboSchema = true;
			return generator;
		}

		private static string FindLocalDb(string name)
		{
			StringBuilder sb = new();
			sb.Append("Data Source=DESKTOP-1O03T0G\\RDDBDEV01;Initial Catalog=");
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

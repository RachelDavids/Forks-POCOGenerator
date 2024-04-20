using System;
using System.IO;
using System.Text;

using POCOGenerator;

namespace StringBuilderDemo
{
	internal static class Program
	{
		private static void Main()
		{
			StringBuilder stringBuilder = new();

			IGenerator generator = GeneratorFactory.GetGenerator(stringBuilder);
			try { generator.Settings.Connection.ConnectionString = File.ReadAllText("ConnectionString.txt"); } catch { }
			if (String.IsNullOrEmpty(generator.Settings.Connection.ConnectionString))
			{
				generator.Settings.Connection.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=AdventureWorks2014;Integrated Security=True";
			}

			generator.Settings.DatabaseObjects.Tables.IncludeAll = true;
			generator.Settings.POCO.CommentsWithoutNull = true;
			generator.Settings.ClassName.IncludeSchema = true;
			generator.Settings.ClassName.SchemaSeparator = "_";
			generator.Settings.ClassName.IgnoreDboSchema = true;

			GeneratorResults results = generator.Generate();

			if (results == GeneratorResults.None)
			{
				Console.WriteLine(stringBuilder.ToString());
			}

			PrintError(results, generator.Error);

			Console.WriteLine();
			Console.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
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

using System;
using System.Drawing;
using System.IO;
using POCOGenerator;

namespace ConsoleColorDarkThemeDemo
{
	internal static class Program
	{
		private static void Main()
		{
			Console.ForegroundColor = ConsoleColor.Red;

			IGenerator generator = GeneratorFactory.GetConsoleColorGenerator();
			ISettings settings = generator.Settings;
			try
			{
				settings.Connection.ConnectionString = File.ReadAllText("ConnectionString.txt");
			}
			catch { }
			if (string.IsNullOrEmpty(settings.Connection.ConnectionString))
			{
				settings.Connection.ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=AdventureWorks2014;Integrated Security=True";
			}

			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			settings.ClassName.IncludeSchema = true;
			settings.ClassName.SchemaSeparator = "_";
			settings.ClassName.IgnoreDboSchema = true;
			settings.EFAnnotations.Enable = true;

			settings.SyntaxHighlight.Text = Color.FromArgb(255, 255, 255);
			settings.SyntaxHighlight.Keyword = Color.FromArgb(86, 156, 214);
			settings.SyntaxHighlight.UserType = Color.FromArgb(78, 201, 176);
			settings.SyntaxHighlight.String = Color.FromArgb(214, 157, 133);
			settings.SyntaxHighlight.Comment = Color.FromArgb(96, 139, 78);
			settings.SyntaxHighlight.Error = Color.FromArgb(255, 0, 0);
			settings.SyntaxHighlight.Background = Color.FromArgb(0, 0, 0);

			GeneratorResults results = generator.Generate();

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

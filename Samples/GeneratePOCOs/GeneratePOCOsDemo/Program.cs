using System;
using POCOGenerator;
using Samples.Support;

namespace GeneratePOCOsDemo
{
	internal static class Program
	{
		private static void Main()
		{
			POCORunner runner = new(GeneratorFactory.GetConsoleColorGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize(ApplyFirstRunSettings);

			// first run
			runner.Run();

			Console.WriteLine();
			Console.WriteLine("Press any key to re-generate with navigation properties");
			Console.WriteLine("GeneratePOCOs() doesn't query the database a second time");
			Console.ReadKey(true);

			ApplySecondRunSettings(generator.Settings);

			// second run
			runner.RunPOCOs();

			Console.WriteLine();
			Console.WriteLine("Press any key to continue . . .");
			Console.ReadKey(true);
		}

		private static void ApplySecondRunSettings(ISettings settings)
		{
			// settings reset also clears the list of included database objects ("Sales.Store")
			// but not the list of objects that were previously constructed
			settings.Reset();

			// settings for the second run
			INavigationProperties properties = settings.NavigationProperties;
			properties.Enable = true;
			properties.VirtualNavigationProperties = true;
			properties.IEnumerableNavigationProperties = true;

			// this line has no effect on GeneratePOCOs() (but would for Generate())
			// because GeneratePOCOs() skips calling the database
			settings.DatabaseObjects.Tables.IncludeAll = true;
		}

		private static void ApplyFirstRunSettings(ISettings settings)
		{
			// settings for the first run
			settings.DatabaseObjects.Tables.Include.Add("Sales.Store");
			settings.POCO.CommentsWithoutNull = true;
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";
			className.IgnoreDboSchema = true;
		}
	}
}

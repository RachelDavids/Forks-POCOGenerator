using System;
using POCOGenerator;
using Samples.Support;

namespace ComplexTypesDemo
{
	internal class Program
	{
		private static void Main()
		{
			// execute script ComplexTypesDB.sql to create ComplexTypesDB database
			POCORunner runner = new(GeneratorFactory.GetConsoleGenerator, "ComplexTypesDB");
			_ = runner.Initialize(s => s.DatabaseObjects.Tables.IncludeAll = true);
			runner.Run();
			bool outputRedirected = Console.IsOutputRedirected;
			if (!outputRedirected)
			{
				Console.WriteLine();
				Console.WriteLine("Press any key to re-generate with complex types");
				Console.ReadKey(true);
			}

			ISettings settings = runner.GeneratorSettings;
			settings.Reset();
			settings.POCO.ComplexTypes = true;

			runner.RunPOCOs();

			if (!outputRedirected)
			{
				Console.WriteLine();
				Console.WriteLine("Press any key to continue . . .");
				Console.ReadKey(true);
			}
		}
	}
}

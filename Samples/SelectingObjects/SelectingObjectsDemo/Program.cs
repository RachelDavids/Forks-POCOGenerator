using System;
using System.Collections.Generic;
using POCOGenerator;
using POCOGenerator.Objects;
using Samples.Support;

namespace SelectingObjectsDemo
{
	internal static class Program
	{
		private static void Main()
		{
			POCORunner runner = new(GeneratorFactory.GetGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize(ApplySettings);
			generator.ServerBuilt += OnServerBuilt;
			runner.Run();
		}

		private static void OnServerBuilt(object o, ServerBuiltEventArgs e)
		{
			foreach (Database database in e.Server.Databases)
			{
				Console.WriteLine("Tables:");
				Console.WriteLine("-------");
				foreach (Table table in database.Tables)
				{
					Console.WriteLine(table);
				}

				Console.WriteLine();

				Console.WriteLine("Views:");
				Console.WriteLine("------");
				foreach (View view in database.Views)
				{
					Console.WriteLine(view);
				}

				Console.WriteLine();
			}

			// do not generate classes
			e.Stop = true;
		}

		private static void ApplySettings(ISettings settings)
		{
			// database object is selected when:
			// 1. explicitly included: Settings.IncludeAll, Settings.Tables.IncludeAll, Settings.Tables.Include.Add(), ...
			// 2. not explicitly excluded. it doesn't appear in any excluding setting: Settings.Tables.ExcludeAll, Settings.Tables.Exclude.Add(), ...

			// select all the tables under HumanResources & Purchasing schemas
			// and select table Production.Product
			IDatabaseObjects objects = settings.DatabaseObjects;
			IList<string> include = objects.Tables.Include;
			include.Add("HumanResources.*");
			include.Add("Purchasing.*");
			include.Add("Production.Product");

			// select all views except views under Production & Sales schemas
			// and except view Person.vAdditionalContactInfo
			IViews views = objects.Views;
			views.IncludeAll = true;
			IList<string> exclude = views.Exclude;
			exclude.Add("Production.*");
			exclude.Add("Sales.*");
			exclude.Add("Person.vAdditionalContactInfo");
		}
	}
}

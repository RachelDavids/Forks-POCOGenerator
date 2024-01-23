using System;
using POCOGenerator;
using Samples.Support;

namespace SkipAndStopDemo
{
	internal static class Program
	{
		private static void Main()
		{
			POCORunner runner = new(() => GeneratorFactory.GetGenerator(Console.Out), "AdventureWorks");
			IGenerator generator = runner.Initialize(ApplySettings);
			runner.Run();
			// skip any table that is not under Sales schema
			generator.TableGenerating += (_, e) => e.Skip = e.Table.Schema != "Sales";

			// stop the generator
			// views, procedures, functions and TVPs will not be generated
			generator.TablesGenerated += (_, e) => e.Stop = true;

			runner.Run();
		}

		private static void ApplySettings(ISettings settings)
		{
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";

			// select everything
			settings.DatabaseObjects.IncludeAll = true;
		}
	}
}

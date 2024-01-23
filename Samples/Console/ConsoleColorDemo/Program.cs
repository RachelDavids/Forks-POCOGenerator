using System;
using POCOGenerator;
using Samples.Support;

namespace ConsoleColorDemo
{
	internal static class Program
	{
		private static void Main()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			POCORunner runner = new(GeneratorFactory.GetConsoleColorGenerator, "AdventureWorks");
			Action<ISettings> update = POCORunner.ApplyDefaults;
			update += s => s.EFAnnotations.Enable = true;
			runner.Initialize(update);
			runner.Run();
			// don't need to prompt since VS does it automatically
			// and causes issues if you wish to redirect
		}
	}
}

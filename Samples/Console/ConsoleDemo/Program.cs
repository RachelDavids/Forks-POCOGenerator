using POCOGenerator;
using Samples.Support;

namespace ConsoleDemo
{
	internal static class Program
	{
		private static void Main()
		{
			POCORunner runner = new(GeneratorFactory.GetConsoleGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize();
			runner.Run(generator);
			// don't need to prompt since VS does it automatically
			// and causes issues if you wish to redirect
		}
	}
}

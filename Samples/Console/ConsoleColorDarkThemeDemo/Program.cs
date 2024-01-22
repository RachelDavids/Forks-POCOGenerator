using System;
using System.Drawing;
using POCOGenerator;
using Samples.Support;

namespace ConsoleColorDarkThemeDemo
{
	internal static class Program
	{
		private static void Main()
		{
			Console.ForegroundColor = ConsoleColor.Red;
			POCORunner runner = new(GeneratorFactory.GetConsoleColorGenerator, "AdventureWorks");
			IGenerator generator = runner.Initialize(ApplySettings);
			runner.Run(generator);
		}

		private static void ApplySettings(ISettings s)
		{
			s.EFAnnotations.Enable = true;
			ISyntaxHighlight highlight = s.SyntaxHighlight;
			highlight.Text = Color.FromArgb(255, 255, 255);
			highlight.Keyword = Color.FromArgb(86, 156, 214);
			highlight.UserType = Color.FromArgb(78, 201, 176);
			highlight.String = Color.FromArgb(214, 157, 133);
			highlight.Comment = Color.FromArgb(96, 139, 78);
			highlight.Error = Color.FromArgb(255, 0, 0);
			highlight.Background = Color.FromArgb(0, 0, 0);
		}
	}
}

using System;
using System.Windows.Forms;

namespace RichTextBoxDemo
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			ApplicationConfiguration.Initialize();
			using (DemoForm mainForm = new())
			{
				Application.Run(mainForm);
			}
		}
	}
}

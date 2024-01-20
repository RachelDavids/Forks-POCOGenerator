using System;
using System.Reflection;
using System.Windows.Forms;

namespace POCOGeneratorUI
{
	internal static class Program
	{
		[STAThread]
		private static void Main()
		{
			try
			{
				// UI exceptions
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

				// Non-UI exceptions
				AppDomain.CurrentDomain.UnhandledException += (_, e) => UnhandledException((Exception)e.ExceptionObject);

				ApplicationConfiguration.Initialize();
				using (POCOGeneratorForm mainForm = new())
				{
					Application.Run(mainForm);
				}
			}
			catch (Exception ex)
			{
				UnhandledException(ex);
			}
		}

		private static void UnhandledException(Exception ex)
		{
			try
			{
				AssemblyName name = Assembly.GetExecutingAssembly().GetName();
				MessageBox.Show(ex.GetUnhandledExceptionErrorMessage(),
								$"Unhandled Error - {name.Name} {name.Version!.ToString(3)}",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
			}
			catch
			{
			}
			Application.Exit();
		}
	}
}

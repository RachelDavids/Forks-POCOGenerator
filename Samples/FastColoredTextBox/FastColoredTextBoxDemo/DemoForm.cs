using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using POCOGenerator;
using Samples.Support;
using ISyntaxHighlight = POCOGenerator.ISyntaxHighlight;

namespace FastColoredTextBoxDemo
{
	public sealed partial class DemoForm
		: Form
	{
		public DemoForm()
		{
			InitializeComponent();
		}

		/// <inheritdoc />
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Text = Text;
			txtConnectionString.Text = POCORunner.FindLocalDb("AdventureWorks");
		}

		private readonly StringBuilder generatedCode = new();
		private void OnGenerateClick(object sender, EventArgs e)
		{
			Clear();

			IGenerator generator = CreateGenerator();
			Stopwatch sw = Stopwatch.StartNew();
			GeneratorResults results = generator.Generate();
			TimeSpan elapsed = sw.Elapsed;
			Debug.WriteLine(elapsed);
			sw.Restart();
			txtPocoEditor.Text = generatedCode.ToString();
			elapsed = sw.Elapsed;
			Debug.WriteLine(elapsed);
			AlertError(results);
		}

		private IGenerator CreateGenerator()
		{
			// Approx 30 seconds to AppendText directly to Control
			// Approx 2 seconds to do so via StringBuilder with C# Syntax set on control.
			// where almost all of this is the Write to the StringBuilder
			//IGenerator generator = Factory.GetGenerator(txtPocoEditor);
			IGenerator generator = GeneratorFactory.GetGenerator(generatedCode);
			ISettings settings = generator.Settings;
			settings.Connection.ConnectionString = txtConnectionString.Text;
			settings.DatabaseObjects.Tables.IncludeAll = true;
			settings.POCO.CommentsWithoutNull = true;
			IClassName className = settings.ClassName;
			className.IncludeSchema = true;
			className.SchemaSeparator = "_";
			className.IgnoreDboSchema = true;
			settings.EFAnnotations.Enable = true;
			ISyntaxHighlight highlight = settings.SyntaxHighlight;
			if (rdbLightTheme.Checked)
			{
				highlight.Reset();
			}
			else if (rdbDarkTheme.Checked)
			{
				highlight.Text = Color.FromArgb(255, 255, 255);
				highlight.Keyword = Color.FromArgb(86, 156, 214);
				highlight.UserType = Color.FromArgb(78, 201, 176);
				highlight.String = Color.FromArgb(214, 157, 133);
				highlight.Comment = Color.FromArgb(96, 139, 78);
				highlight.Error = Color.FromArgb(255, 0, 0);
				highlight.Background = Color.FromArgb(0, 0, 0);
			}
			return generator;
		}

		private void AlertError(GeneratorResults results)
		{
			bool isError = (results & GeneratorResults.Error) == GeneratorResults.Error;
			bool isWarning = (results & GeneratorResults.Warning) == GeneratorResults.Warning;

			if (results == GeneratorResults.None || isWarning)
			{
				if (isWarning)
				{
					MessageBox.Show(this, $"Warning Result: {results}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
			else if (isError)
			{
				MessageBox.Show(this, $"Error Result: {results}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void OnCopyClick(object sender, EventArgs e)
		{
			string text = txtPocoEditor.Text;
			if (string.IsNullOrEmpty(text))
			{
				Clipboard.Clear();
			}
			else
			{
				Clipboard.SetText(text);
			}
		}

		private void OnClearClick(object sender, EventArgs e) => Clear();

		private void Clear()
		{
			if (rdbLightTheme.Checked)
			{
				txtPocoEditor.BackColor = Color.FromArgb(255, 255, 255);
			}
			else if (rdbDarkTheme.Checked)
			{
				txtPocoEditor.BackColor = Color.FromArgb(0, 0, 0);
			}

			txtPocoEditor.Clear();
		}

		private void OnCloseClick(object sender, EventArgs e) => Close();
	}
}

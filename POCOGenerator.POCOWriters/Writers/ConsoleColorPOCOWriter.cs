using System.Drawing;
using Console = TrueColorConsole.VTConsole;

namespace POCOGenerator.POCOWriters.Writers
{
	internal class ConsoleColorPOCOWriter
		: POCOWriter, IWriter, ISyntaxHighlight
	{
		internal ConsoleColorPOCOWriter()
		{
		}

		public Color Text { get; set; }
		public Color Keyword { get; set; }
		public Color UserType { get; set; }
		public Color String { get; set; }
		public Color Comment { get; set; }
		public Color Error { get; set; }
		public Color Background { get; set; }

		public void Clear()
		{
			Console.SetColorBackground(Background);
			Console.EraseInDisplay(TrueColorConsole.VTEraseMode.Entirely);
			SnapshotClear();
		}

		public void Write(string text)
		{
			Console.Write(text, Text);
			SnapshotWrite(text);
		}

		public void WriteKeyword(string text)
		{
			Console.Write(text, Keyword);
			SnapshotWrite(text);
		}

		public void WriteUserType(string text)
		{
			Console.Write(text, UserType);
			SnapshotWrite(text);
		}

		public void WriteString(string text)
		{
			Console.Write(text, String);
			SnapshotWrite(text);
		}

		public void WriteComment(string text)
		{
			Console.Write(text, Comment);
			SnapshotWrite(text);
		}

		public void WriteError(string text)
		{
			Console.Write(text, Error);
			SnapshotWrite(text);
		}

		public void WriteLine()
		{
			Console.WriteLine();
			SnapshotWriteLine();
		}

		public void WriteLine(string text)
		{
			Console.WriteLine(text, Text);
			SnapshotWriteLine(text);
		}

		public void WriteLineKeyword(string text)
		{
			Console.WriteLine(text, Keyword);
			SnapshotWriteLine(text);
		}

		public void WriteLineUserType(string text)
		{
			Console.WriteLine(text, UserType);
			SnapshotWriteLine(text);
		}

		public void WriteLineString(string text)
		{
			Console.WriteLine(text, String);
			SnapshotWriteLine(text);
		}

		public void WriteLineComment(string text)
		{
			Console.WriteLine(text, Comment);
			SnapshotWriteLine(text);
		}

		public void WriteLineError(string text)
		{
			Console.WriteLine(text, Error);
			SnapshotWriteLine(text);
		}
	}
}

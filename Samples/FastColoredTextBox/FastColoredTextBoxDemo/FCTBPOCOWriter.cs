using System;
using System.Drawing;
using PavelTorgashov.Forms;
using POCOGenerator.POCOWriters;

namespace FastColoredTextBoxDemo
{
	internal sealed class FCTBPOCOWriter
		: POCOWriter, IWriter, ISyntaxHighlight
	{
		private readonly FastColoredTextBox _richTextBox;

		internal FCTBPOCOWriter(FastColoredTextBox richTextBox)
		{
			_richTextBox = richTextBox;
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
			_richTextBox.BackColor = Background;
			_richTextBox.Clear();
			SnapshotClear();
		}

		private void AppendText(string text, Color _)
		{
			if (string.IsNullOrEmpty(text) == false)
			{
				_richTextBox.AppendText(text);
			}
		}

		public void Write(string text)
		{
			AppendText(text, Text);
			SnapshotWrite(text);
		}

		public void WriteKeyword(string text)
		{
			AppendText(text, Keyword);
			SnapshotWrite(text);
		}

		public void WriteUserType(string text)
		{
			AppendText(text, UserType);
			SnapshotWrite(text);
		}

		public void WriteString(string text)
		{
			AppendText(text, String);
			SnapshotWrite(text);
		}

		public void WriteComment(string text)
		{
			AppendText(text, Comment);
			SnapshotWrite(text);
		}

		public void WriteError(string text)
		{
			AppendText(text, Error);
			SnapshotWrite(text);
		}

		public void WriteLine()
		{
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine();
		}

		public void WriteLine(string text)
		{
			AppendText(text, Text);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}

		public void WriteLineKeyword(string text)
		{
			AppendText(text, Keyword);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}

		public void WriteLineUserType(string text)
		{
			AppendText(text, UserType);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}

		public void WriteLineString(string text)
		{
			AppendText(text, String);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}

		public void WriteLineComment(string text)
		{
			AppendText(text, Comment);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}

		public void WriteLineError(string text)
		{
			AppendText(text, Error);
			_richTextBox.AppendText(Environment.NewLine);
			SnapshotWriteLine(text);
		}
	}
}

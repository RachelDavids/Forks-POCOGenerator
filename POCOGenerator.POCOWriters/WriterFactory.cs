using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using POCOGenerator.POCOWriters.Writers;

namespace POCOGenerator.POCOWriters
{
	public static class WriterFactory
	{
		public static Func<IWriter> GetCreateOutputEmptyWriterHandler() => () => new OutputEmptyPOCOWriter();

		public static Func<IWriter> GetCreateWriterHandler(StringBuilder stringBuilder) => () => new StringBuilderPOCOWriter(stringBuilder);

		public static Func<IWriter> GetCreateWriterHandler(TextWriter textWriter) => () => new TextWriterPOCOWriter(textWriter);

		public static Func<IWriter> GetCreateWriterHandler(Stream stream) => () => new StreamPOCOWriter(stream);

		public static Func<IWriter> GetCreateConsoleWriterHandler() => () => new ConsolePOCOWriter();

		public static Func<IWriter> GetCreateConsoleColorWriterHandler() => () => new ConsoleColorPOCOWriter();

		public static Func<IWriter> GetCreateWriterHandler(RichTextBox richTextBox) => () => new RichTextBoxPOCOWriter(richTextBox);
	}
}

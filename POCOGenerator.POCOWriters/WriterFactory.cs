using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using POCOGenerator.POCOWriters.Writers;

namespace POCOGenerator.POCOWriters
{
	public static class WriterFactory
	{
		//public static Func<IWriter> GetCreateOutputEmptyWriterHandler() => () => new OutputEmptyPOCOWriter();
		//public static Func<IWriter> GetCreateWriterHandler(StringBuilder sb) => () => new StringBuilderPOCOWriter(sb);
		//public static Func<IWriter> GetCreateWriterHandler(TextWriter textWriter) => () => new TextWriterPOCOWriter(textWriter);
		//public static Func<IWriter> GetCreateWriterHandler(Stream stream) => () => new StreamPOCOWriter(stream);

		public static Func<IWriter> GetCreateConsoleWriterHandler() => () => new ConsolePOCOWriter();
		public static Func<IWriter> GetCreateConsoleColorWriterHandler() => () => new ConsoleColorPOCOWriter();
		public static Func<IWriter> GetCreateWriterHandler(RichTextBox richTextBox) => () => new RichTextBoxPOCOWriter(richTextBox);

		public static Func<IWriter> GetCreateWriter<T>(T obj) => () => Factory.Create(obj);
		public static Func<IWriter> GetCreateWriter() => () => new OutputEmptyPOCOWriter();

		static WriterFactory()
		{
			Factory.Register((TextWriter tw) => new TextWriterPOCOWriter(tw));
			Factory.Register((Stream s) => new StreamPOCOWriter(s));
			Factory.Register((StringBuilder sb) => new StringBuilderPOCOWriter(sb));
		}

		public static void Register<T>(Func<T, IWriter> func) => Factory.Register(func);
		private static class Factory
		{
			// ReSharper disable once MemberHidesStaticFromOuterClass
			public static void Register<T>(Func<T, IWriter> func) => Store<T>.Create = func;

			public static IWriter Create<T>(T value) => Store<T>.Create(value);
			private static class Store<T>
			{
				public static Func<T, IWriter> Create { get; set; }
			}
		}
	}
}

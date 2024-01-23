using System;
using System.Windows.Forms;
using PavelTorgashov.Forms;
using POCOGenerator;
using POCOGenerator.POCOWriters;

namespace FastColoredTextBoxDemo
{
	internal static class Factory
	{
		/// <summary>Gets a generator that writes to an instance of <see cref="RichTextBox" />.</summary>
		/// <param name="textBox">The instance of <see cref="RichTextBox" /> that the generator writes to.</param>
		/// <returns>The generator that writes to an instance of <see cref="RichTextBox" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="textBox" /> is <see langword="null" />.</exception>
		public static IGenerator GetGenerator(FastColoredTextBox textBox)
		{
			if (textBox == null)
			{
				throw new ArgumentNullException(nameof(textBox));
			}

			EnsureRegistration();

			return GeneratorFactory.GetGenerator(WriterFactory.GetCreateWriter(textBox));
		}

		private static bool _isRegistered = false;

		private static void EnsureRegistration()
		{
			if (!_isRegistered)
			{
				WriterFactory.Register((FastColoredTextBox c) => new FCTBPOCOWriter(c));
				_isRegistered = true;
			}
		}
	}
}

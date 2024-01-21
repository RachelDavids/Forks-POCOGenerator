using System;
using System.Windows.Forms;
using POCOGenerator.POCOWriters;
using POCOGenerator.POCOWriters.Writers;

namespace POCOGenerator.Forms
{
	/// <summary>Creates instances of the POCO Generator, suitable for WinForms, and provides redirection to other WinForms output sources.</summary>
	public static class GeneratorWinFormsFactory
	{
		#region Get Generator

		/// <summary>Gets a generator that writes to an instance of <see cref="RichTextBox" />.</summary>
		/// <param name="richTextBox">The instance of <see cref="RichTextBox" /> that the generator writes to.</param>
		/// <returns>The generator that writes to an instance of <see cref="RichTextBox" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="richTextBox" /> is <see langword="null" />.</exception>
		public static IGenerator GetGenerator(RichTextBox richTextBox)
		{
			if (richTextBox == null)
			{
				throw new ArgumentNullException(nameof(richTextBox));
			}

			EnsureRegistration();

			return GeneratorFactory.GetGenerator(WriterFactory.GetCreateWriter(richTextBox));
		}

		private static bool _isRegistered = false;

		private static void EnsureRegistration()
		{
			if (!_isRegistered)
			{
				WriterFactory.Register((RichTextBox c) => new RichTextBoxPOCOWriter(c));
				_isRegistered = true;
			}
		}

		#endregion

		#region Redirect To

		/// <summary>Redirects the generator underline output source to an instance of <see cref="RichTextBox" />.</summary>
		/// <param name="generator">The generator to redirect its underline output source.</param>
		/// <param name="richTextBox">The instance of <see cref="RichTextBox" /> that the generator is redirected to.</param>
		/// <exception cref="ArgumentNullException">
		///   <paramref name="generator" /> is <see langword="null" /> or
		///   <paramref name="richTextBox" /> is <see langword="null" />.
		/// </exception>
		public static void RedirectTo(this IGenerator generator, RichTextBox richTextBox)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}
			if (richTextBox == null)
			{
				throw new ArgumentNullException(nameof(richTextBox));
			}
			EnsureRegistration();
			GeneratorFactory.ChangeWriter(generator, WriterFactory.GetCreateWriter(richTextBox));
		}

		#endregion
	}
}

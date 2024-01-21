using System;
using System.IO;
using System.Text;
using POCOGenerator.POCOWriters;

namespace POCOGenerator
{
	/// <summary>Creates instances of the POCO Generator and provides redirection to other output sources.</summary>
	public static class GeneratorFactory
	{
		#region Get Generator

		/// <summary>Gets a generator using the supplied writer factory method</summary>
		/// <returns>A generator that uses the supplied WriterFactory.</returns>
		public static IGenerator GetGenerator(Func<IWriter> createWriter) => new Generator(createWriter);

		/// <summary>Gets an output-empty generator. The generator doesn't write to any underlying output source.</summary>
		/// <returns>The output-empty generator.</returns>
		public static IGenerator GetGenerator() => GetGenerator(WriterFactory.GetCreateWriter());

		/// <summary>Gets a generator that writes to an instance of <see cref="StringBuilder" />.</summary>
		/// <param name="stringBuilder">The instance of <see cref="StringBuilder" /> that the generator writes to.</param>
		/// <returns>The generator that writes to an instance of <see cref="StringBuilder" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="stringBuilder" /> is <see langword="null" />.</exception>
		public static IGenerator GetGenerator(StringBuilder stringBuilder)
		{
			return stringBuilder == null
				? throw new ArgumentNullException(nameof(stringBuilder))
				: GetGenerator(WriterFactory.GetCreateWriter(stringBuilder));
		}

		/// <summary>Gets a generator that writes to an instance of <see cref="TextWriter" />.</summary>
		/// <param name="textWriter">The instance of <see cref="TextWriter" /> that the generator writes to.</param>
		/// <returns>The generator that writes to an instance of <see cref="TextWriter" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="textWriter" /> is <see langword="null" />.</exception>
		public static IGenerator GetGenerator(TextWriter textWriter)
		{
			return textWriter == null
				? throw new ArgumentNullException(nameof(textWriter))
				: GetGenerator(WriterFactory.GetCreateWriter(textWriter));
		}

		/// <summary>Gets a generator that writes to an instance of <see cref="Stream" />.</summary>
		/// <param name="stream">The instance of <see cref="Stream" /> that the generator writes to.</param>
		/// <returns>The generator that writes to an instance of <see cref="Stream" />.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="stream" /> is <see langword="null" />.</exception>
		public static IGenerator GetGenerator(Stream stream) => stream == null
																	? throw new ArgumentNullException(nameof(stream))
																	: GetGenerator(WriterFactory.GetCreateWriter(stream));

		/// <summary>Gets a generator that writes to the <see cref="Console" />.</summary>
		/// <returns>The generator that writes to the <see cref="Console" />.</returns>
		public static IGenerator GetConsoleGenerator() => GetGenerator(WriterFactory.GetCreateConsoleWriterHandler());

		/// <summary>Gets a generator that writes to the <see cref="Console" /> with syntax highlight colors.</summary>
		/// <returns>The generator that writes to the <see cref="Console" /> with syntax highlight colors.</returns>
		public static IGenerator GetConsoleColorGenerator() => GetGenerator(WriterFactory.GetCreateConsoleColorWriterHandler());

		#endregion

		#region Redirect To

		/// <summary>Clears the generator underlying output source.</summary>
		/// <param name="generator">The generator to clear its underlying output source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator" /> is <see langword="null" />.</exception>
		public static void RedirectToOutputEmpty(this IGenerator generator)
		{
			if (generator is null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = WriterFactory.GetCreateWriter();
			}
		}

		/// <summary>Redirects the generator underlying output source to an instance of <see cref="StringBuilder" />.</summary>
		/// <param name="generator">The generator to redirect its underlying output source.</param>
		/// <param name="stringBuilder">The instance of <see cref="StringBuilder" /> that the generator is redirected to.</param>
		/// <exception cref="ArgumentNullException">
		///   <paramref name="generator" /> is <see langword="null" /> or
		///   <paramref name="stringBuilder" /> is <see langword="null" />.
		/// </exception>
		public static void RedirectTo(this IGenerator generator, StringBuilder stringBuilder)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			if (stringBuilder == null)
			{
				throw new ArgumentNullException(nameof(stringBuilder));
			}

			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = WriterFactory.GetCreateWriter(stringBuilder);
			}
		}

		/// <summary>Redirects the generator underlying output source to an instance of <see cref="TextWriter" />.</summary>
		/// <param name="generator">The generator to redirect its underlying output source.</param>
		/// <param name="textWriter">The instance of <see cref="TextWriter" /> that the generator is redirected to.</param>
		/// <exception cref="ArgumentNullException">
		///   <paramref name="generator" /> is <see langword="null" /> or
		///   <paramref name="textWriter" /> is <see langword="null" />.
		/// </exception>
		public static void RedirectTo(this IGenerator generator, TextWriter textWriter)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			if (textWriter == null)
			{
				throw new ArgumentNullException(nameof(textWriter));
			}

			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = WriterFactory.GetCreateWriter(textWriter);
			}
		}

		/// <summary>Redirects the generator underlying output source to an instance of <see cref="Stream" />.</summary>
		/// <param name="generator">The generator to redirect its underlying output source.</param>
		/// <param name="stream">The instance of <see cref="Stream" /> that the generator is redirected to.</param>
		/// <exception cref="ArgumentNullException">
		///   <paramref name="generator" /> is <see langword="null" /> or
		///   <paramref name="stream" /> is <see langword="null" />.
		/// </exception>
		public static void RedirectTo(this IGenerator generator, Stream stream)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}
			_ = ChangeWriter(generator, WriterFactory.GetCreateWriter(stream));
		}

		public static IGenerator ChangeWriter(this IGenerator generator, Func<IWriter> createWriter)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}
			if (createWriter == null)
			{
				throw new ArgumentNullException(nameof(createWriter));
			}
			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = createWriter;
			}
			return g;
		}

		/// <summary>Redirects the generator underlying output source to the <see cref="Console" />.</summary>
		/// <param name="generator">The generator to redirect its underlying output source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator" /> is <see langword="null" />.</exception>
		public static void RedirectToConsole(this IGenerator generator)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = WriterFactory.GetCreateConsoleWriterHandler();
			}
		}

		/// <summary>Redirects the generator underlying output source to the <see cref="Console" /> with syntax highlight colors.</summary>
		/// <param name="generator">The generator to redirect its underlying output source.</param>
		/// <exception cref="ArgumentNullException"><paramref name="generator" /> is <see langword="null" />.</exception>
		public static void RedirectToConsoleColor(this IGenerator generator)
		{
			if (generator == null)
			{
				throw new ArgumentNullException(nameof(generator));
			}

			Generator g = (Generator)generator;
			lock (g.lockObject)
			{
				g.createWriter = WriterFactory.GetCreateConsoleColorWriterHandler();
			}
		}

		#endregion
	}
}

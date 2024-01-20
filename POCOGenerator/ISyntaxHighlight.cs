using System.Drawing;

namespace POCOGenerator
{
	/// <summary>The settings determine the colors for syntax elements.</summary>
	public interface ISyntaxHighlight
	{
		/// <summary>Resets the syntax highlight settings to their default values.</summary>
		void Reset();

		/// <summary>Gets or sets the color for a text (foreground)
		/// that is not keyword, user type, string, comment or an error.
		/// <para>The default color is #000000.</para></summary>
		Color Text { get; set; }

		/// <summary>Gets or sets the color for a C# keyword.
		/// <para>The default color is #0000FF.</para></summary>
		Color Keyword { get; set; }

		/// <summary>Gets or sets the color for a user type.
		/// <para>The default color is #2B91AF.</para></summary>
		Color UserType { get; set; }

		/// <summary>Gets or sets the color for a string.
		/// <para>The default color is #A31515.</para></summary>
		Color String { get; set; }

		/// <summary>Gets or sets the color for a comment.
		/// <para>The default color is #008000.</para></summary>
		Color Comment { get; set; }

		/// <summary>Gets or sets the color for an error.
		/// <para>The default color is #FF0000.</para></summary>
		Color Error { get; set; }

		/// <summary>Gets or sets the background color.
		/// <para>The default color is #FFFFFF.</para></summary>
		Color Background { get; set; }
	}
}

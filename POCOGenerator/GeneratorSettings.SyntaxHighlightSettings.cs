using System.Drawing;

namespace POCOGenerator
{
	internal sealed partial class GeneratorSettings
	{
		private void InitializeSyntaxHighlight(ISyntaxHighlight syntaxHighlight)
		{
			SyntaxHighlight.Text = syntaxHighlight.Text;
			SyntaxHighlight.Keyword = syntaxHighlight.Keyword;
			SyntaxHighlight.UserType = syntaxHighlight.UserType;
			SyntaxHighlight.String = syntaxHighlight.String;
			SyntaxHighlight.Comment = syntaxHighlight.Comment;
			SyntaxHighlight.Error = syntaxHighlight.Error;
			SyntaxHighlight.Background = syntaxHighlight.Background;
		}

		private sealed class SyntaxHighlightSettings
			: ISyntaxHighlight
		{
			private readonly object lockObject;

			internal SyntaxHighlightSettings(object lockObject)
			{
				this.lockObject = lockObject;
			}

			public void Reset()
			{
				lock (lockObject)
				{
					Text = Color.FromArgb(0, 0, 0);
					Keyword = Color.FromArgb(0, 0, 255);
					UserType = Color.FromArgb(43, 145, 175);
					String = Color.FromArgb(163, 21, 21);
					Comment = Color.FromArgb(0, 128, 0);
					Error = Color.FromArgb(255, 0, 0);
					Background = Color.FromArgb(255, 255, 255);
				}
			}

			private Color text = Color.FromArgb(0, 0, 0);

			public Color Text
			{
				get
				{
					lock (lockObject)
					{
						return text;
					}
				}

				set
				{
					lock (lockObject)
					{
						text = value;
					}
				}
			}

			private Color keyword = Color.FromArgb(0, 0, 255);

			public Color Keyword
			{
				get
				{
					lock (lockObject)
					{
						return keyword;
					}
				}

				set
				{
					lock (lockObject)
					{
						keyword = value;
					}
				}
			}

			private Color userType = Color.FromArgb(43, 145, 175);

			public Color UserType
			{
				get
				{
					lock (lockObject)
					{
						return userType;
					}
				}

				set
				{
					lock (lockObject)
					{
						userType = value;
					}
				}
			}

			private Color @string = Color.FromArgb(163, 21, 21);

			public Color String
			{
				get
				{
					lock (lockObject)
					{
						return @string;
					}
				}

				set
				{
					lock (lockObject)
					{
						@string = value;
					}
				}
			}

			private Color comment = Color.FromArgb(0, 128, 0);

			public Color Comment
			{
				get
				{
					lock (lockObject)
					{
						return comment;
					}
				}

				set
				{
					lock (lockObject)
					{
						comment = value;
					}
				}
			}

			private Color error = Color.FromArgb(255, 0, 0);

			public Color Error
			{
				get
				{
					lock (lockObject)
					{
						return error;
					}
				}

				set
				{
					lock (lockObject)
					{
						error = value;
					}
				}
			}

			private Color background = Color.FromArgb(255, 255, 255);

			public Color Background
			{
				get
				{
					lock (lockObject)
					{
						return background;
					}
				}

				set
				{
					lock (lockObject)
					{
						background = value;
					}
				}
			}
		}
	}
}

using System.Collections.Generic;
using System.Drawing;

namespace POCOGeneratorUI.TypesMapping
{
	internal sealed class TypeMapping
	{
		public IEnumerable<TypeMappingPart> FromType { get; private set; }
		public IEnumerable<TypeMappingPart> ToType { get; private set; }

		public TypeMapping(IEnumerable<TypeMappingPart> fromType, IEnumerable<TypeMappingPart> toType)
		{
			FromType = fromType;
			ToType = toType;
		}

		public TypeMapping(string fromTypeText, Color fromTypeSyntaxColor, string toTypeText, Color toTypeSyntaxColor)
		{
			FromType = [new(fromTypeText, fromTypeSyntaxColor)];
			ToType = [new(toTypeText, toTypeSyntaxColor)];
		}

		public TypeMapping(string fromTypeText, Color fromTypeSyntaxColor, params TypeMappingPart[] toType)
		{
			FromType = [new(fromTypeText, fromTypeSyntaxColor)];
			ToType = toType;
		}
	}

	internal sealed class TypeMappingPart(string text, Color syntaxColor)
	{
		public string Text { get; private set; } = text;
		public Color SyntaxColor { get; private set; } = syntaxColor;
	}
}

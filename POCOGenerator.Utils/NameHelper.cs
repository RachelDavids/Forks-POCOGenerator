using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Pluralize.NET;

namespace POCOGenerator.Utils
{
	public static class NameHelper
	{
		private static readonly Regex regexCamelCase = new("(?<word>[A-Z]{2,}|[A-Z][^A-Z]*?|^[^A-Z]*?)(?=[A-Z]|$)", RegexOptions.Compiled);
		private static readonly Regex regexDigits = new("\\d+", RegexOptions.Compiled);

		public static string GetSingularName(string name) => WordQuantifier(name, ToSingular);

		public static string GetPluralName(string name) => WordQuantifier(name, ToPlural);

		private static string WordQuantifier(string name, Func<string, string> quantifier)
		{
			if (String.IsNullOrEmpty(name))
			{
				return name;
			}

			string word = name;
			int index = 0;

			if (word.Length > 0 && word[^1] != '_')
			{
				int index1 = word.LastIndexOf('_');
				if (index1 != -1)
				{
					index = index1 + 1;
					word = word[index..];
				}
			}

			if (regexDigits.IsMatch(word))
			{
				return name + "s";
			}

			Match match = regexCamelCase.Matches(word).Cast<Match>().LastOrDefault();
			if (match != null)
			{
				word = match.Groups["word"].Value;
				index += match.Groups["word"].Index;
			}

			string quantifiedWord = quantifier(word);

			return quantifiedWord == word ? name : name.Length == word.Length ? quantifiedWord : name[..index] + quantifiedWord;
		}

		private static string ToSingular(string word)
		{
			return
				(String.CompareOrdinal(word, word.ToUpper()) == 0) ?
				new Pluralizer().Singularize(word.ToLower()).ToUpper() :
				new Pluralizer().Singularize(word)
			;
		}

		private static string ToPlural(string word)
		{
			return
				(String.CompareOrdinal(word, word.ToUpper()) == 0) ?
				new Pluralizer().Pluralize(word.ToLower()).ToUpper() :
				new Pluralizer().Pluralize(word)
			;
		}

		public static string TransformName(string name,
										   string wordsSeparator = null,
										   bool isCamelCase = true,
										   bool isUpperCase = false,
										   bool isLowerCase = false)
		{
			List<string> words = GetWords(name);

			name = null;
			int index = 0;
			foreach (string word in words)
			{
				if (isCamelCase)
				{
					name += word[..1].ToUpper() + word[1..].ToLower();
				}
				else if (isUpperCase)
				{
					name += word.ToUpper();
				}
				else if (isLowerCase)
				{
					name += word.ToLower();
				}
				else
				{
					name += word;
				}

				index++;
				if (index < words.Count && !String.IsNullOrEmpty(wordsSeparator))
				{
					name += wordsSeparator;
				}
			}

			return name;
		}

		public static List<string> GetWords(string name)
		{
			List<string> camelCaseWords = [];

			string[] words = name.Split('_');
			foreach (string word in words)
			{
				foreach (Match match in regexCamelCase.Matches(word))
				{
					camelCaseWords.Add(match.Groups["word"].Value);
				}
			}

			return camelCaseWords;
		}

		public static string CleanName(string name)
		{
			name = name.Replace(" ", "_").Replace('-', '_').Trim();
			if (name.Length > 0 && '0' <= name[0] && name[0] <= '9')
			{
				name = "_" + name;
			}

			return name;
		}

		public static string CleanEnumLiteral(string name)
		{
			name = name.Replace(' ', '_').Replace('-', '_').Replace(',', '_').Trim();
			if (name.Length > 0 && '0' <= name[0] && name[0] <= '9')
			{
				name = "_" + name;
			}

			return name;
		}

		private static readonly List<string> s_namePrefixes = [
			"first",
			"second",
			"third",
			"fourth",
			"fifth",
			"sixth",
			"seventh",
			"eighth",
			"ninth",
			"tenth",
			"eleventh",
			"twelfth",
			"primary",
			"secondary",
			"tertiary",
			"quaternary",
			"quinary",
			"senary",
			"septenary",
			"octonary",
			"novenary",
			"decenary",
			"undenary",
			"duodenary",
			"current",
			"previous",
			"initial",
			"starting",
			"last",
			"ending"
		];

		public static string AddNamePrefix(string name, string columnName)
		{
			string columnNameLower = columnName.ToLower();
			string prefix = s_namePrefixes.OrderByDescending(p => p.Length).FirstOrDefault(columnNameLower.StartsWith);
			if (!String.IsNullOrEmpty(prefix))
			{
				name = columnName[..prefix.Length] + name;
			}

			return name;
		}

		private class ConjugatedVerb(string verb, string pastParticipleVerb)
		{
			public string Verb { get; set; } = verb;
			public string PastParticipleVerb { get; set; } = pastParticipleVerb;
			public List<string> VerbVariations { get; set; } = [];
		}

		private static readonly List<ConjugatedVerb> s_conjugatedVerbs =
		[
			new("insert", "inserted"),
			new("update", "updated"),
			new("delete", "deleted"),
			new("create", "created"),
			new("write", "written"),
			new("ship", "shipped"),
			new("send", "sent"),
		];

		private static readonly List<string> s_verbVariations =
		[
			"{0}",
			"{0}by",
			"{0}_by",
			"{0}id",
			"{0}_id",
			"user{0}",
			"user_{0}",
			"{0}user",
			"{0}_user",
			"person{0}",
			"person_{0}",
			"{0}person",
			"{0}_person"
		];

		private static List<string> s_variations;

		static NameHelper()
		{
			BuildNameVerbsAndVariations();
		}

		private static void BuildNameVerbsAndVariations()
		{
			foreach (ConjugatedVerb conjugations in s_conjugatedVerbs)
			{
				foreach (string variation in s_verbVariations)
				{
					conjugations.VerbVariations.Add(String.Format(variation, conjugations.Verb));
					conjugations.VerbVariations.Add(String.Format(variation, conjugations.PastParticipleVerb));
				}

				// order length descending
				conjugations.VerbVariations.Sort((x, y) => x.Length == y.Length ? 0 : (x.Length < y.Length ? 1 : -1));
			}

			s_variations = s_conjugatedVerbs.SelectMany(p => p.VerbVariations).ToList();

			// order length descending
			s_variations.Sort((x, y) => x.Length == y.Length ? 0 : (x.Length < y.Length ? 1 : -1));
		}

		public static bool IsNameVerb(string name)
		{
			bool hasTime = name.IndexOf("time", StringComparison.OrdinalIgnoreCase) != -1;
			if (hasTime)
			{
				return false;
			}

			bool hasDate = name.IndexOf("date", StringComparison.OrdinalIgnoreCase) != -1;
			bool hasShip = name.IndexOf("ship", StringComparison.OrdinalIgnoreCase) != -1;
			if (!hasDate && !hasShip)
			{
				return s_variations.Any(variation => name.IndexOf(variation, StringComparison.OrdinalIgnoreCase) != -1);
			}

			if (hasDate)
			{
				bool hasUpdate = name.IndexOf("update", StringComparison.OrdinalIgnoreCase) != -1;
				if (!hasUpdate)
				{
					return false;
				}

				int index = name.IndexOf("date", StringComparison.OrdinalIgnoreCase);
				do
				{
					hasUpdate =
						(index - 2) >= 0 &&
						name.IndexOf("update", index - 2, StringComparison.OrdinalIgnoreCase) == (index - 2);

					if (!hasUpdate)
					{
						return false;
					}

					index = name.IndexOf("date", index + 4, StringComparison.OrdinalIgnoreCase);
				}
				while (index != -1);
			}

			if (hasShip)
			{
				bool hasShipment = name.IndexOf("shipment", StringComparison.OrdinalIgnoreCase) != -1;
				if (hasShipment)
				{
					return false;
				}
			}

			return true;
		}

		public static string ConjugateNameVerbToPastParticiple(string name)
		{
			ConjugatedVerb conjugations = s_conjugatedVerbs.FirstOrDefault(cv => cv.VerbVariations.Any(variation => name.IndexOf(variation, StringComparison.OrdinalIgnoreCase) != -1));

			if (conjugations == null)
			{
				return name;
			}

			// verb past participle
			if (name.IndexOf(conjugations.PastParticipleVerb, StringComparison.OrdinalIgnoreCase) != -1)
			{
				return name;
			}

			// verb
			int index = name.IndexOf(conjugations.Verb, StringComparison.OrdinalIgnoreCase);
			return index != -1
				? conjugations.PastParticipleVerb[..1].ToUpper() + conjugations.PastParticipleVerb[1..].ToLower() + "By"
				: name;
		}

		public static string Escape(string text) => text.Replace("\\", "\\\\").Replace("\"", "\\\"");
	}
}

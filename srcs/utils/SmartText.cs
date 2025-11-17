using System;
using System.Text;

namespace ChariotSanzzo.Utils {
	static public class SmartText {
		private static readonly string	SmartMarkers = "@QBTC";

		private static string	GetSmartLink(ReadOnlySpan<char> smartSlice) {
			if (smartSlice.Length == 0 || smartSlice[0] != '[')
				return "-SmartLinkError-";
			var titleEnd = smartSlice.IndexOf(']');
			if (titleEnd == -1 || titleEnd + 1 >= smartSlice.Length)
				return "-SmartLinkError-";
			var title = smartSlice[1..titleEnd].Trim();
			var href = $"/{smartSlice[(titleEnd + 1)..]}";

			return $"[{title}]({LinkData.GetAlbinaSiteFullAdress()}{href})";
		}
		private static string	GetSmartToggle(ReadOnlySpan<char> smartSlice) {
		if (smartSlice.Length == 0 || smartSlice[0] != '[')
				return "-SmartLinkError-";
			var titleEnd = smartSlice.IndexOf(']');
			if (titleEnd == -1 || titleEnd + 1 >= smartSlice.Length)
				return "-SmartLinkError-";
			var title = smartSlice[1..titleEnd].Trim();
			var content = smartSlice[(titleEnd + 1)..].Trim();
			return $"-> {title} ->\n{content}";
		}
		private static string	GetSmartQuote(ReadOnlySpan<char> quote) {
			return $"\n {quote}";
		}
		private static string	GetSmartBullet(ReadOnlySpan<char> item) {
			return $"\n {item}";
		}
		private static string	GetSmartColor(ReadOnlySpan<char> smartSlice) {
			if (smartSlice.Length == 0 || smartSlice[0] != '[')
				return "-SmartLinkError-";
			var colorEnd = smartSlice.IndexOf(']');
			if (colorEnd == -1 || colorEnd + 1 >= smartSlice.Length)
				return "-SmartLinkError-";
			var color = smartSlice[1..colorEnd].Trim();
			var content = smartSlice[(colorEnd + 1)..].Trim();
			return $"{content}";
		}
		
		private static bool		IsSmartOpeningTrigger(string characters, int index) {
			if (
				index + 2 < characters.Length
					&& SmartMarkers.Contains(characters[index + 1])
					&& characters[index + 2] == '/'
			)
				return true;
			return false;
		}
		private static string	ParseSmartBlock(ReadOnlySpan<char> span, char marker) => marker switch {
			'@' => GetSmartLink(span),
			'Q' => GetSmartQuote(span),
			'B' => GetSmartBullet(span),
			'T' => GetSmartToggle(span),
			'C' => GetSmartColor(span),
			_ => "-UnknownSmartType-"
		};
		public static string	Deserialize(string? content) {
			if (content == null)
				return "";

			StringBuilder parts = new();
			int index = 0;
			int lastIndex = 0;
			

			while (index < content.Length) {
				if (content[index] == '[' && IsSmartOpeningTrigger(content, index)) {
					int start = index;
					int depth = 1;
					index += 3;

					while (index < content.Length && depth > 0) {
						if (content[index] == '[')
							++depth;
						else if (content[index] == ']')
							--depth;
						++index;
					}
					if (depth == 0) {
						if (lastIndex < start)
							parts.Append(content.AsSpan(lastIndex, start - lastIndex));
						ReadOnlySpan<char> contentSpan = content;
						parts.Append(ParseSmartBlock(contentSpan[(start + 3)..(index - 1)], content[start + 1]));
						lastIndex = index;
						continue;
					}
				}
				++index;
			}

			if (lastIndex < content.Length) {
				parts.Append(content[lastIndex..]);
			}
			return parts.ToString().Trim();
		}
	}
}
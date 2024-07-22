namespace STPlib;

public static class STPStr {
	public static int StrIndexChr(this string? str, string? chr) {
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(chr))
			return (-1);
		for (int i = 0; i < str.Length; i++)
			for (int y = 0; y < chr.Length; y++)
				if (str[i] == chr[y])
					return (i);
		return (-1);
	}
	public static bool StrLimitChr(this string? str, string? chr) {
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(chr))
			return (false);
		for (int i = 0; i < str.Length; i++)
			for (int y = 0; y < chr.Length; y++) {
				if (str[i] == chr[y])
					break ;
				else if (y == chr.Length - 1)
					return (false);
			}
		return (true);
	}
	public static bool StrLimitChrSetOrDigit(this string? str, string? chr) {
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(chr))
			return (false);
		for (int i = 0; i < str.Length; i++) {
			if ((str[i] >= '0' && str[i] <= '9'))
				continue ;
			for (int y = 0; y < chr.Length; y++) {
				if (str[i] == chr[y])
					break ;
				else if (y == chr.Length - 1)
					return (false);
			}
		}
		return (true);
	}
	public static int StrCountChr(this string? str, string? chr) {
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(chr))
			return (-1);
		int	count = 0;
		for (int i = 0; i < str.Length; i++)
			for (int y = 0; y < chr.Length; y++)
				if (str[i] == chr[y])
					count++;
		return (count);
	}
	public static int[] StrCountChrFirstIndex(this string? str, string? chr) {
		int[]	ret = new int[2] {0, 0};
		if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(chr))
			return (ret);
		for (int i = 0; i < str.Length; i++)
			for (int y = 0; y < chr.Length; y++)
				if (str[i] == chr[y])
					if (++ret[0] == 1)
						ret[1] = i;
		return (ret);
	}
	public static string RemoveChr(this string? str, char chr) {
		if (string.IsNullOrEmpty(str))
			return ("");
		for (int i = 0; i < str.Length; i++)
			if (str[i] == chr)
				str = str.Replace($"{chr}", string.Empty);
		return (str);
	}
}

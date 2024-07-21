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
}

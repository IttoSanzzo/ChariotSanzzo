namespace STPlib;

public static class STPPut {
	public static void PutStr(this string? str) {
		if (string.IsNullOrEmpty(str))
			return ;
		Console.Write(str);
	}
	public static void PutCStr(this char[]? str) {
		if (str == null)
			return ;
		for (int i = 0; i < str.Length;i++)
			Console.Write(str[i]);
	}
}


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
	/*
	public static int FindSubStringIndex(this string? str, string target) {
		if (str == null)
			return (-1);
		int	retIndex = -1;
		int	j;
		for (int i = 0; i < str.Length; i++) {
			j = 0;
			while (str[i + j] == target[j])
				j++;
			if ()
				return (i);
		}
		return (-1);
	}
	*/
	public static string GetBetween(this string? strSource, string strStart, string strEnd) {
		if (strSource == null)
			return ("");
		if (strSource.Contains(strStart) && strSource.Contains(strEnd)) {
			int	Start;
			int	End;
			Start = strSource.IndexOf(strStart, 0) + strStart.Length;
			End = strSource.IndexOf(strEnd, Start);
			return strSource.Substring(Start, End - Start);
		}
		return ("");
	}
}

public static class ToX {
	public static int StoI(this string? str) {
		int	ret = 0;
		int	sign = +1;
		int	i = 0;

		if (str == null)
			return (ret);
		char[]	nbr = str.ToCharArray();
		while ((nbr[i] >= '\t' && nbr[i] <= '\r') || nbr[i] == ' ')
                i++;
		if ((nbr[i] == '+' || nbr[i] == '-') && nbr[i++] == '-')
				sign = -1;
		while (i < nbr.Length && (nbr[i] >= '0' && nbr[i] <= '9'))
			ret = ret * 10 + (nbr[i++] - '0');
		return (ret * sign);
	}
}

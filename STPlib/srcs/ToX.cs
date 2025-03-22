namespace STPlib;

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

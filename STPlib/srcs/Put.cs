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

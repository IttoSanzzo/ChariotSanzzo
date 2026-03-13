namespace ChariotSanzzo.Utils {
	public static class DiscordUtils {
		public static ulong GetUlongIdFromDiscordEmoteUnsafe(string emote) => ulong.Parse(emote[(emote.LastIndexOf(":") + 1)..^1]);
		public static string GetIdFromDiscordEmote(string emote) => emote[(emote.LastIndexOf(":") + 1)..^1];
		public static string GetNameFromDiscordEmote(string emote) => emote[(emote.IndexOf(":") + 1)..emote.LastIndexOf(":")];
	}
}

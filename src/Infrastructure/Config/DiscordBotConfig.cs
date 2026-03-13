namespace ChariotSanzzo.Infrastructure.Config {
	public static class DiscordBotConfig {
		public static string Name { get; internal set; } = null!;
		public static string Version { get; internal set; } = null!;
		public static string Prefix { get; internal set; } = null!;
		public static string BotToken { get; internal set; } = null!;
		public static ulong BotUserId { get; internal set; }
		public static ulong DebugGuildId { get; internal set; }
	}
}

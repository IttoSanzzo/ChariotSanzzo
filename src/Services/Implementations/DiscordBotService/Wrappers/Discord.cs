using DSharpPlus;

namespace ChariotSanzzo.Services.Wrappers {
	public static class Discord {
		public static DiscordClient Client { get; private set; } = null!;
		public static void Initialize(DiscordClient client) {
			Client = client;
		}
	}
}

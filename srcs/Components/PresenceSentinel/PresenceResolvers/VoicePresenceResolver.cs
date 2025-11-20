using DSharpPlus;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class VoicePresenceResolver(PresenceRegistry registry, DiscordClient client) {
		private readonly PresenceRegistry		Registry = registry;
		private readonly DiscordClient			Client = client;
		private static readonly ulong				NotFound = ulong.MinValue;

		#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		public async Task<UserPresenceState?> ForceResolveUserVoiceStateAsync(ulong userId) {
			Console.WriteLine("Running Resolver");
			var state = Registry.GetOrCreate(userId);
			if (state.Get<ulong?>("voice.guildId") != null)
				return state;
			foreach (var guild in Client.Guilds) {
				if (guild.Value.Members.Keys.Contains(userId) && guild.Value.Members[userId].VoiceState.Channel != null) {
					state.Set("voice.guildId", guild.Value.Id);
					state.Set("voice.channelId", guild.Value.Members[userId].VoiceState.Channel.Id);
					return state;
				}
			}
			state.Set("voice.guildId", NotFound);
			state.Set("voice.channelId", NotFound);
			return state;
		}
		#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	}
}
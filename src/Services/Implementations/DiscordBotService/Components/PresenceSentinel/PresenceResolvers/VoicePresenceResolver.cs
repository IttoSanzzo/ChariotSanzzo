using DSharpPlus;

namespace ChariotSanzzo.Services.Components {
	public class VoicePresenceResolver(PresenceRegistry registry, DiscordClient client) {
		private readonly PresenceRegistry Registry = registry;
		private readonly DiscordClient Client = client;
		private static readonly string NotFound = "0";

		public async Task<UserPresenceState?> ForceResolveUserVoiceStateAsync(ulong userId) {
			var state = await Registry.GetOrCreate(userId);
			if (state.Get<string?>("voice.guildId") != null)
				return state;
			foreach (var (guildId, guild) in Client.Guilds) {
				if (!guild.Members.TryGetValue(userId, out var member) || member.VoiceState?.ChannelId == null)
					continue;
				if (guild.Members.Keys.Contains(userId) && guild.Members[userId].VoiceState.ChannelId != null) {
					state.Set("voice.lastUpdate", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
					state.Set("voice.guildId", guildId.ToString());
					state.Set("voice.guildName", guild.Name);
					state.Set("voice.channelId", guild.Members[userId].VoiceState.ChannelId.ToString()!);
					state.Set("voice.channelName", (await guild.Members[userId].VoiceState.GetChannelAsync())!.Name);
					return state;
				}
			}
			state.Set("voice.guildId", NotFound);
			state.Set("voice.guildName", "");
			state.Set("voice.channelId", NotFound);
			state.Set("voice.channelName", "");
			return state;
		}
	}
}

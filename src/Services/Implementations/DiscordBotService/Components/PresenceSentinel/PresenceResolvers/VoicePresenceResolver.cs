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
			state.Set("voice.lastUpdate", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
			foreach (var (guildId, guild) in Client.Guilds) {
				if (!guild.Members.TryGetValue(userId, out var member) || member.VoiceState?.ChannelId == null)
					continue;
				state.Set("voice.guildId", guildId.ToString());
				state.Set("voice.guildName", guild.Name);
				state.Set("voice.channelId", member.VoiceState.ChannelId.ToString()!);
				state.Set("voice.channelName", (await member.VoiceState.GetChannelAsync())!.Name);
				return state;
			}
			state.Set("voice.guildId", NotFound);
			state.Set("voice.guildName", "");
			state.Set("voice.channelId", NotFound);
			state.Set("voice.channelName", "");
			return state;
		}
	}
}

using DSharpPlus;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class VoicePresenceResolver(PresenceRegistry registry, DiscordClient client) {
		private readonly PresenceRegistry		Registry = registry;
		private readonly DiscordClient			Client = client;
		private static readonly string			NotFound = "0";

		public UserPresenceState? ForceResolveUserVoiceStateAsync(ulong userId) {
			var state = Registry.GetOrCreate(userId);
			if (state.Get<string?>("voice.guildId") != null)
				return state;
			foreach (var (guildId, guild) in Client.Guilds) {
				if (!guild.Members.TryGetValue(userId, out var member) || member.VoiceState?.Channel == null)
					continue;
				if (guild.Members.Keys.Contains(userId) && guild.Members[userId].VoiceState.Channel != null) {
					state.Set("voice.lastUpdate", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
					state.Set("voice.guildId", guildId.ToString());
					state.Set("voice.guildName", "");
					state.Set("voice.channelId", guild.Members[userId].VoiceState.Channel.Id.ToString());
					state.Set("voice.guildName", "");
					return state;
				}
			}
			state.Set("voice.guildId", NotFound);
			state.Set("voice.channelId", NotFound);
			return state;
		}
	}
}
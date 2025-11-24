using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class VoicePresenceTracker(DiscordClient client) : IPresenceTracker {
		private PresenceRegistry	Registry	= null!;
		private readonly DiscordClient Client = client;
		
		public Task InitializeAsync(PresenceRegistry registry) {
			Registry = registry;
			Client.VoiceStateUpdated += OnVoiceStateUpdated;
			return Task.CompletedTask;
		}

		private async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args) {
			var state = Registry.GetOrCreate(args.User.Id);

			if (args.After.Channel != null) {
				state.Set("voice.guildId", args.After.Guild.Id.ToString());
				state.Set("voice.channelId", args.After.Channel.Id.ToString());
			} else {
				state.Set("voice.guildId", "0");
				state.Set("voice.channelId", "0");
			}
			await Program.PresenceSentinel.PostPresenceUpdate(args.User.Id);
			return;
		}
	}
}
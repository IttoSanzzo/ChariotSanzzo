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

		#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		private async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args) {
			var state = Registry.GetOrCreate(args.User.Id);

			if (args.After.Channel != null) {
				state.Set("voice.guildId", args.After.Guild.Id);
				state.Set("voice.channelId", args.After.Channel.Id);
			} else {
				state.Set("voice.guildId", ulong.MinValue);
				state.Set("voice.channelId", ulong.MinValue);
			}
			return;
		}
		#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	}
}
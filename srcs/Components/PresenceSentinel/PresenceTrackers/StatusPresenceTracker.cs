using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class StatusPresenceTracker(DiscordClient client) : IPresenceTracker {
		private PresenceRegistry	Registry	= null!;
		private readonly DiscordClient Client = client;
		
		public Task InitializeAsync(PresenceRegistry registry) {
			Registry = registry;
			Client.PresenceUpdated += OnPresenceUpdated;
			return Task.CompletedTask;
		}

		private async Task OnPresenceUpdated(DiscordClient sender, PresenceUpdateEventArgs args) {
			var state = await Registry.GetOrCreate(args.User.Id);
			state.Set("status", args.PresenceAfter.Status);
			return;
		}
	}
}
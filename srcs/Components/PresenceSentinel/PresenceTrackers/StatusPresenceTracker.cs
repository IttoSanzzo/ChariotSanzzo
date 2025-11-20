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

		#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		private async Task OnPresenceUpdated(DiscordClient sender, PresenceUpdateEventArgs args) {
			var state = Registry.GetOrCreate(args.User.Id);
			state.Set("status", args.PresenceAfter.Status);
			return;
		}
		#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
	}
}
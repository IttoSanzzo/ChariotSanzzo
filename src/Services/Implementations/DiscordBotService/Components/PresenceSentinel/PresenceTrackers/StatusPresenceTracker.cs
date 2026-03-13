using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Services.Components {
	public class StatusPresenceTracker : IPresenceTracker, IEventHandler<PresenceUpdatedEventArgs> {
		private static PresenceRegistry Registry => PresenceSentinel.Registry;

		public async Task HandleEventAsync(DiscordClient sender, PresenceUpdatedEventArgs args) {
			var state = await Registry.GetOrCreate(args.User.Id);
			state.Set("status", args.PresenceAfter.Status);
			return;
		}
	}
}

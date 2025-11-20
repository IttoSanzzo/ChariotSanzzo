using ChariotSanzzo.Components.PresenceSentinel;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace ChariotSanzzo.Events {
	public static class	EventsInit{
	// 0. ClientReady
		private static Task ClientReady(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args) {
			return (Task.CompletedTask);
		}

	// 1. DiscordClient Events
		public static async Task EventsInitRun(this DiscordClient client) {
			client.Ready += ClientReady;
			client.MessageCreated += Events.STPDiceRoller.DiceRoller;
			client.ComponentInteractionCreated += Events.CharitoMusicEvents.MusicInterectionButton;
			Program.PresenceSentinel.AddTracker(new VoicePresenceTracker(client));
			Program.PresenceSentinel.AddTracker(new StatusPresenceTracker(client));
			await Program.PresenceSentinel.InitializeAsync(client);
		}

	// 2. CommdsNextExtension Events Events
		public static void EventsInitRun(this CommandsNextExtension command) {
			command.CommandErrored += STPCommandErrored.CmdErrTask;
		}
	}
}

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Lavalink;

namespace Gjallarhorn.Events {
	public static class	EventsInit{
		public static void EventsInitRun(this DiscordClient client) {
			client.Ready += Client_Ready;
		}
		public static void EventsInitRun(this CommandsNextExtension command) {
			// command.CommandErrored += STPCommandErrored.CmdErrTask;
		}
		private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args) {
			return (Task.CompletedTask);
		}
	}
}

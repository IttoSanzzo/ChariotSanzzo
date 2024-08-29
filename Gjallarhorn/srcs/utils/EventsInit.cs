using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace Gjallarhorn.Events {
	public static class	EventsInit{
		public static void EventsInitRun(this DiscordClient client) {
			client.Ready += Client_Ready;
			// client.MessageCreated += Events.ChariotConn.GetChariotCommunication;
			// client.ComponentInteractionCreated += Events.Music.MusicInterectionButton;
		}
		public static void EventsInitRun(this CommandsNextExtension command) {
			// command.CommandErrored += STPCommandErrored.CmdErrTask;
		}
		private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args) {
			return (Task.CompletedTask);
		}
	}
}

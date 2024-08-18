using DSharpPlus.CommandsNext;
using Gjallarhorn.Commands.Prefix;
using Gjallarhorn.Commands.Slash;
using DSharpPlus.SlashCommands;

namespace Gjallarhorn.Commands {
	public static class CommandsInit {
		public static void CommandsInitRun(this CommandsNextExtension commands) {
			// PrefixCommands
			commands.RegisterCommands<Prefix.TestCommands>();
		}
		public static void CommandsInitRun(this SlashCommandsExtension commands) {
			// SlashCommands
			commands.RegisterCommands<Slash.TestCommands>();
		}
	}
}

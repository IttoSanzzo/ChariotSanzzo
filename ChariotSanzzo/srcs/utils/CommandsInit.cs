using DSharpPlus.CommandsNext;
using ChariotSanzzo.Commands.Prefix;
using ChariotSanzzo.Commands.Slash;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands {
	public static class CommandsInit {
		public static void CommandsInitRun(this CommandsNextExtension commands) {
			// PrefixCommands
			commands.RegisterCommands<Prefix.TestCommands>();
			commands.RegisterCommands<Prefix.HelpCommands>();
			commands.RegisterCommands<Prefix.LinkCommands>();
			commands.RegisterCommands<Prefix.DiceCommands>();
		}
		public static void CommandsInitRun(this SlashCommandsExtension commands) {
			// SlashCommands
			commands.RegisterCommands<Slash.TestCommands>();
			commands.RegisterCommands<Slash.DatabaseCommands>();
			commands.RegisterCommands<Slash.DiceCommands>();
			commands.RegisterCommands<Slash.MusicCommands>();
		}
	}
}

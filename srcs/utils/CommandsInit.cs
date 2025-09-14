using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands {
	public static class CommandsInit {
	// 0. PrefixCommands
		public static void CommandsInitRun(this CommandsNextExtension commands) {
			commands.RegisterCommands<Prefix.TestCommands>();
			commands.RegisterCommands<Prefix.HelpCommands>();
			commands.RegisterCommands<Prefix.LinkCommands>();
			commands.RegisterCommands<Prefix.DiceCommands>();
		}
	// 1. SlashCommands
		public static void CommandsInitRun(this SlashCommandsExtension commands) {
			commands.RegisterCommands<Slash.TestCommands>();
			commands.RegisterCommands<Slash.DatabaseCommands>();
			commands.RegisterCommands<Slash.DiceCommands>();
			commands.RegisterCommands<Slash.MusicCommands>();
			commands.RegisterCommands<Slash.PlaylistCommands>();
			commands.RegisterCommands<Slash.SFXCommands>();
			commands.RegisterCommands<Slash.GuildSettingsCommands>();
			commands.RegisterCommands<Slash.ChannelCommands>();
			commands.RegisterCommands<Slash.AlbinaInfoCommands>();
		}
	}
}

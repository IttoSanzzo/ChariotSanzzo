using DSharpPlus.CommandsNext;

namespace ChariotSanzzo.Commands {
	public static class CommandsInit  {
		public static void CommandsInitRun(this CommandsNextExtension commands) {
			commands.RegisterCommands<TestCommands>();
			commands.RegisterCommands<HelpCommands>();
			commands.RegisterCommands<LinkCommands>();
			commands.RegisterCommands<DiceCommands>();
		}
	}
}

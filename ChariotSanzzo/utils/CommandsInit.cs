using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace ChariotSanzzo.commands {
	public static class CommandsInit  {
		public static void CommandsInitRun(this CommandsNextExtension commands) {
			commands.RegisterCommands<TestCommand>();
			commands.RegisterCommands<HelpCommands>();
			commands.RegisterCommands<AlbinaNotionLink>();
		}
	}
}

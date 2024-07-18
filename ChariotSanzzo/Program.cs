using ChariotSanzzo.commands;
using ChariotSanzzo.config;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace ChariotSanzzo {
	internal class Program {
		private static DiscordClient? Client {get; set;}
		private static CommandsNextExtension? Commands {get; set;}
		static async Task Main(string[] args) {
			var config = new ConfigReader();
			Console.WriteLine($"Ohayou... {config._name} is waking up!");
			var discordConfig = new DiscordConfiguration() {
				Intents = DiscordIntents.All,
				Token = config._token,
				TokenType = TokenType.Bot,
				AutoReconnect = true
			};
			Client = new DiscordClient(discordConfig);
			Client.Ready += Client_Ready;
			var commandsConfig = new CommandsNextConfiguration() {
				StringPrefixes = new string[] {config.GetPrefix()},
				EnableMentionPrefix = true,
				EnableDms = true,
				EnableDefaultHelp = false
			};
			Commands = Client.UseCommandsNext(commandsConfig);
			Commands.CommandsInitRun();
			await Client.ConnectAsync();
			await Task.Delay(-1);
		}
		private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args) {
			return (Task.CompletedTask);
		}
	}
}

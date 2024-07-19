using ChariotSanzzo.Commands;
using ChariotSanzzo.Events;
using ChariotSanzzo.config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo {
	internal class Program {
		public static DiscordClient? Client {get; set;}
		public static CommandsNextExtension? Commands {get; set;}
		static async Task Main(string[] args) {
			// 1. Importing Json configs and starting
			var config = new ConfigReader();
			Console.WriteLine($"Ohayou... {config._name} is waking up!");

			// 2. Setting Discord Client
			var discordConfig = new DiscordConfiguration() {
				Intents = DiscordIntents.All,
				Token = config._token,
				TokenType = TokenType.Bot,
				AutoReconnect = true
			};
			Client = new DiscordClient(discordConfig);
			Client.UseInteractivity(new InteractivityConfiguration() {
				Timeout = TimeSpan.FromMinutes(2)
			});
			Client.EventsInitRun();

			// 3. Setting Commands
			var commandsConfig = new CommandsNextConfiguration() {
				StringPrefixes = new string[] {config.GetPrefix()},
				EnableMentionPrefix = true,
				EnableDms = true,
				EnableDefaultHelp = false
			};
			Commands = Client.UseCommandsNext(commandsConfig);
			var	slashCommandsConfig = Client.UseSlashCommands();
			slashCommandsConfig.CommandsInitRun();
			Commands.CommandsInitRun();
			Commands.EventsInitRun();

			// 4. Finishing, Connecting and Looping
			await Client.ConnectAsync();
			await Task.Delay(-1);
		}
	}
}

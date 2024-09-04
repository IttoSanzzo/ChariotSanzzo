using ChariotSanzzo.Commands;
using ChariotSanzzo.Events;
using ChariotSanzzo.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ChariotSanzzo.Components.SpotifyApi;
using ChariotSanzzo.Database;
using ChariotSanzzo.Commands.Slash;
using ChariotSanzzo.Components.SoundcloudApi;

namespace ChariotSanzzo {
	internal class Program {
	// 0. Program Variables
		public static DiscordClient?			Client			{get; set;}
		public static CommandsNextExtension?	Commands		{get; set;}
		public static SpotifyConn				SpotifyConn		{get; set;} = new SpotifyConn();
		public static SoundcloudConn			SoundcloudConn	{get; set;} = new SoundcloudConn();
	// 0.1. Config Program Variables
		public static bool 							_LocalLavalink	{get; set;} = false;

	// 1. Main
		static async Task Main(string[] args) {
		// -1. Unreasonable
			var DBConfigHolder = new DBConfig();
			if (args.Length < 3 || args[1] != "SafeStart") {
				Program.ColorWriteLine(ConsoleColor.Red, "Not initalized by Core, aborting...");
				return ;
			}
			if (args[2] == "true")
				Program._LocalLavalink = true;
		// 0. TESTING GROUNDS

		// 1. Importing Json configs and starting
			var config = new ConfigReader();
			Program.ColorWriteLine(ConsoleColor.Blue, $"Ohayou... {config._name} is waking up!");

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

		// 4. Lavalink Setup
			// SpotifyConn.RunInit();
			Client.LavalinkRunInit();

		// 5. Finishing, Connecting and Looping
			await Client.ConnectAsync();
			Client.LavalinkConnectAsync();
			ChariotSanzzoSocket.OpenChariotControlSocket();
			Program.ColorWriteLine(ConsoleColor.Blue, $"{config._name} is up!");
			await Task.Delay(-1);
		}
		public static void	ColorWriteLine(ConsoleColor color, string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"ChariotSanzzo: ");
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public static void	WriteLine(string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"ChariotSanzzo: ");
			Console.ResetColor();
			Console.WriteLine(text);
		}
		public static void	Write(string text) {
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.Write($"ChariotSanzzo: ");
			Console.ResetColor();
			Console.Write(text);
		}
		public static void	WriteException(Exception ex) {
			Program.ColorWriteLine(ConsoleColor.Yellow ,ex.ToString());
		}
	}
}

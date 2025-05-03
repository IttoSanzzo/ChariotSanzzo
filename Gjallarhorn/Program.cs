using Gjallarhorn.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Gjallarhorn.Config;
using Gjallarhorn.Events;
using Gjallarhorn.HttpServer;

namespace Gjallarhorn {
	internal class Program {
	// 0. Program Variables
		public static HttpClient				HttpCli			{get; set;} = new HttpClient();
		public static DiscordClient?			Client			{get; set;} = null;
		public static CommandsNextExtension?	Commands		{get; set;}
		public static Thread?					SocketThread	{get; set;} = null;
	// 0.1. Config Program Variables
		public static bool 						_LocalLavalink	{get; set;} = false;

	// 1. Main
		static async Task Main(string[] args) {
			if (args.Length < 3 || args[1] != "SafeStart") {
				Program.ColorWriteLine(ConsoleColor.Red, "Not initalized by Core, aborting...");
				return ;
			}
			await Program.DotEnvLoadAsync();
			if (args[2] == "true")
				Program._LocalLavalink = true;
		// 0. TESTING GROUNDS

		// 1. Importing Json configs and starting
			var config = new ConfigReader();
			Program.ColorWriteLine(ConsoleColor.DarkYellow, $"Ohayou... {config._name} is waking up!");

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
			Client.LavalinkRunInit();

		// 5. Finishing, Connecting and Looping
			await Client.ConnectAsync();
			Client.LavalinkConnectAsync();
			GjallarhornHttpServer.OpenGjallarhornHttpServer();
			Program.ColorWriteLine(ConsoleColor.DarkYellow, $"{config._name} is up!");
			await Task.Delay(-1);
		}
		public static void			ColorWriteLine(ConsoleColor color, string? text) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write($"Gjallarhorn: ");
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ResetColor();
		}
		public static void			WriteLine(string? text) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write($"Gjallarhorn: ");
			Console.ResetColor();
			Console.WriteLine(text);
		}
		public static void			Write(string? text) {
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.Write($"Gjallarhorn: ");
			Console.ResetColor();
			Console.Write(text);
		}
		public static void			WriteException(Exception ex) {
			Program.ColorWriteLine(ConsoleColor.Yellow ,ex.ToString());
		}
		private static async Task	DotEnvLoadAsync() {
			try {
				if (File.Exists(".env") == false)
					return ;
				string[] lines = await File.ReadAllLinesAsync(".env");
				foreach (string line in lines) {
					if (string.IsNullOrWhiteSpace(line) == false && line.Contains('=')) {
						string[] keyValue = line.Split('=');
						if (keyValue.Length == 2)
							Environment.SetEnvironmentVariable(keyValue[0], keyValue[1]);
					}
				}
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
		}
	}
}

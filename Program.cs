using ChariotSanzzo.Commands;
using ChariotSanzzo.Events;
using ChariotSanzzo.Config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using ChariotSanzzo.Components.SpotifyApi;
using ChariotSanzzo.Components.SoundcloudApi;
using ChariotSanzzo.HttpServer;
using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Utils;

namespace ChariotSanzzo {
	internal class Program {
	// M. Program Variables
		public static HttpClient							HttpCli					{get; set;} = new HttpClient();
		public static DiscordClient?					Client					{get; set;}
		public static CommandsNextExtension?	Commands				{get; set;}
		public static SpotifyConn							SpotifyConn			{get; set;} = new SpotifyConn();
		public static SoundcloudConn					SoundcloudConn	{get; set;} = new SoundcloudConn();
		public static AlbinaConn							AlbinaConn			{get; set;} = new AlbinaConn();
	// M.1. Config Program Variables
		public static bool				 						LocalLavalink	{get; set;} = true;

	// 0. Main
		static async Task Main(string[] args) {
			await Program.DotEnvLoadAsync();
			var SafeStartToken = Environment.GetEnvironmentVariable("SAFE_START_TOKEN") ?? throw new Exception("No safe start token received");
			if (SafeStartToken != "SafeStart") {
				Program.ColorWriteLine(ConsoleColor.Red, "Not initalized by Core, aborting...");
				return ;
			}
		// 0. TESTING GROUNDS

		// 1. Importing Json configs and starting
			var config = new ConfigReader();
			await LinkData.SetAll();
			Program.ColorWriteLine(ConsoleColor.Blue, $"Ohayou... {config.Name} is waking up!");

		// 2. Setting Discord Client
			var discordConfig = new DiscordConfiguration() {
				Intents = DiscordIntents.All,
				Token = config.Token,
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
			ChariotSanzzoHttpServer.OpenChariotHttpServer();
			Program.ColorWriteLine(ConsoleColor.Blue, $"{config.Name} is up!");
			await Task.Delay(500);
			Program.ColorWriteLine(ConsoleColor.Magenta, $"| Active Guilds |");
			var guilds = Client.Guilds.ToArray();
			for (int i = 0; i < guilds.Length; i++)
				Program.WriteLine($"{guilds[i].Value}");
			Program.Write("\n");
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

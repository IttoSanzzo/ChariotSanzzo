using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Extensions;

namespace ChariotSanzzo.Services {
	public class DiscordBotService(DiscordClient client) : BackgroundService {
		private readonly DiscordClient Client = client;
		public static bool IsReady { get; set; } = false;

		public static void AddDiscordBotServiceToBuilder(WebApplicationBuilder builder) {
			builder.Services.AddDiscordClient(DiscordBotConfig.BotToken, DiscordIntents.All);
			AddEventHandlers(builder);
			AddCommands(builder);
			builder.Services.AddHostedService<DiscordBotService>();
		}
		protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
			Discord.Initialize(Client);
			PresenceSentinel.Initialize(Client);
			await Client.ConnectAsync();
			// await RemoveStaleCommands();
			Program.WriteLine($"{DiscordBotConfig.Name} connected to Discord.");
			IsReady = await WaitForReady();
			Program.WriteLine($"{DiscordBotConfig.Name} is Ready.");
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}
		private static void AddEventHandlers(WebApplicationBuilder builder) {
			builder.Services.ConfigureEventHandlers(events => {
				events.AddEventHandlers([.. typeof(Program).Assembly
					.GetTypes()
					.Where(t =>
						!t.IsAbstract &&
						!t.IsInterface &&
						t.GetInterfaces().Any(i =>
							i.IsGenericType &&
							i.GetGenericTypeDefinition() == typeof(IEventHandler<>)
						)
					)
				]);
			});
		}
		private static void AddCommands(WebApplicationBuilder builder) {
			builder.Services.AddCommandsExtension((serviceProvider, extension) => {
				LoadCommands(extension);
				extension.AddProcessor(new TextCommandProcessor(new() {
					PrefixResolver = new DefaultPrefixResolver(true, DiscordBotConfig.Prefix).ResolvePrefixAsync
				}));
				extension.CommandErrored += CommandErrorEventHandler.HandleEventAsync;
			}, new CommandsConfiguration() {
				RegisterDefaultCommandProcessors = true,
				UseDefaultCommandErrorHandler = false,
				// DebugGuildId = DiscordBotConfig.DebugGuildId
			});
		}
		private static void LoadCommands(CommandsExtension extension) {
			var commandTypes = typeof(Program).Assembly
				.GetTypes()
				.Where(t =>
						!t.IsAbstract &&
						!t.IsInterface &&
						t.GetMethods().Any(m => m.GetCustomAttributes(typeof(CommandAttribute), true).Length != 0)
				);

			foreach (var type in commandTypes)
				extension.AddCommands(type);
		}
		private async Task RemoveStaleCommands() {
			var staleCommands = await Client.GetGlobalApplicationCommandsAsync();
			bool shouldPrintRemovalLog = true;
			foreach (var command in staleCommands) {
				if (shouldPrintRemovalLog) {
					Program.WriteLine(@$"

						Removing Command
						{command.Name}
						{command.ApplicationId}
						{command.CreationTimestamp}
						{command.Description}
						{command.Type}
						{command.Version}

					");
				}
				await Client.DeleteGlobalApplicationCommandAsync(command.Id);
			}
		}
		private async Task<bool> WaitForReady() {
			const int sleepMs = 50;
			while (true) {
				try {
					if (Client.Guilds.Count == 0) continue;
					if (!Client.Guilds.Any((guild) => guild.Value.Name.Length == 0)) break;
					await Task.Delay(sleepMs);
				} catch {
					await Task.Delay(sleepMs);
				}
			}
			return true;
		}
	}
}

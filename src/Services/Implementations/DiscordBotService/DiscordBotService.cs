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
	}
}

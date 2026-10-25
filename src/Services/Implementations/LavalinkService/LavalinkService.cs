using ChariotSanzzo.Components.Gjallar;
using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Extensions;
using Lavalink4NET.Rest;

namespace ChariotSanzzo.Services {
	public class LavalinkService(IAudioService lavalinkNode) : BackgroundService {
		private readonly IAudioService LavalinkNode = lavalinkNode;

		public static void AddLavalinkServiceToBuilder(WebApplicationBuilder builder) {
			builder.Services.AddLavalink();
			builder.Services.ConfigureLavalink(options => {
				options.BaseAddress = new($"http://{LavalinkConfig.Hostname}:{LavalinkConfig.Port}");
				options.Passphrase = LavalinkConfig.Password;
				options.Label = DiscordBotConfig.Name;
				options.ReadyTimeout = TimeSpan.FromSeconds(9);
				options.BufferSize = 1024 * 1024 * 3; // 3MiB
				options.ResumptionOptions = new LavalinkSessionResumptionOptions(TimeSpan.FromMinutes(30));
				options.TrackCacheOptions = new LavalinkTrackCacheOptions {
					FailureCacheDuration = TimeSpan.FromMinutes(15),
					SuccessCacheDuration = TimeSpan.FromMinutes(60),
				};
				options.Label = DiscordBotConfig.Name;
			});
			builder.Services.AddHostedService<LavalinkService>();
		}
		protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
			Lavalink.Initialize(LavalinkNode);
			Program.WriteLine($"{DiscordBotConfig.Name} connected to Lavalink.");

			Lavalink.LavalinkNode.ConnectionClosed += HandleConnectionClosed;
			Lavalink.LavalinkNode.ConnectionReady += HandleConnectionReady;
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}
		private async Task HandleConnectionReady(object sender, ConnectionReadyEventArgs args) {
			Program.WriteLine("Lavalink connection ready.");
			await StopAllPlayersAsync();
		}
		private async Task HandleConnectionClosed(object sender, ConnectionClosedEventArgs args) {
			Program.WriteLine("Lavalink connection was closed and didn't reconnect. Resetting players...");
			await StopAllPlayersAsync();
		}
		private static async Task StopAllPlayersAsync() {
			var players = Lavalink.PlayerManager.Players.Select((player) => player);
			foreach (var player in players) {
				var gtx = new GjallarContext();
				await gtx.GjallarContextUnsecureAsync(new GjallarGenericCommand() {
					Command = "Stop",
					GuildId = player.GuildId.ToString(),
					TargetBot = "ChariotSanzzo",
					UserId = DiscordBotConfig.BotUserId.ToString()
				});
				await gtx.TryCallingAsync();
			}
		}
	}
}

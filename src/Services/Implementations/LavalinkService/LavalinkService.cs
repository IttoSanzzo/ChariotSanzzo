using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using Lavalink4NET;
using Lavalink4NET.Extensions;

namespace ChariotSanzzo.Services {
	public class LavalinkService(IAudioService lavalinkNode) : BackgroundService {
		private readonly IAudioService LavalinkNode = lavalinkNode;

		public static void AddLavalinkServiceToBuilder(WebApplicationBuilder builder) {
			builder.Services.AddLavalink();
			builder.Services.ConfigureLavalink(options => {
				options.BaseAddress = new($"http://{LavalinkConfig.Hostname}:{LavalinkConfig.Port}");
				options.Passphrase = LavalinkConfig.Password;
				options.Label = DiscordBotConfig.Name;
			});
			builder.Services.AddHostedService<LavalinkService>();
		}
		protected override async Task ExecuteAsync(CancellationToken cancellationToken) {
			Lavalink.Initialize(LavalinkNode);
			Program.WriteLine($"{DiscordBotConfig.Name} connected to Lavalink.");
			await Task.Delay(Timeout.Infinite, cancellationToken);
		}
	}
}

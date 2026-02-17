using ChariotSanzzo.Components.DiceRoller;
using ChariotSanzzo.Components.MusicComponent;
using Microsoft.AspNetCore.Mvc;

namespace ChariotSanzzo.Components.HttpServer {
	public class PlayerGenericCommand {
		public string Command { get; set; } = "";
		public string TargetBot { get; set; } = "";
		public string UserId { get; set; } = "";
		public string GuildId { get; set; } = "";
		public string? TrackUrl { get; set; } = null;
		public string? ChannelId { get; set; } = null;
		public long? Position { get; set; } = null;
		public bool Priority { get; set; } = true;
	}
	public class GjallarhornPostBody {
		public string? TrackUrl { get; set; } = null;
		public string Command { get; set; } = "";
		public string UserId { get; set; } = "";
		public string Color { get; set; } = "";
		public string? Message { get; set; } = "";
		public string? ChannelId { get; set; } = null;
	}
	public static class ChariotSanzzoHttpServer {
		private static readonly HttpClient _httpClient = new();
		private static readonly string ServerPortString = Environment.GetEnvironmentVariable("HTTP_SERVER_PORT") ?? throw new InvalidOperationException("HTTP_SERVER_PORT not set");
		private static readonly int ServerPort = int.Parse(ServerPortString);
		private static readonly string GjallarhornHost = Environment.GetEnvironmentVariable("GJALLARHORN_HOST") ?? throw new InvalidOperationException("GJALLARHORN_HOST not set");
		private static readonly string GjallarhornPort = Environment.GetEnvironmentVariable("GJALLARHORN_PORT") ?? throw new InvalidOperationException("GJALLARHORN_PORT not set");

		static ChariotSanzzoHttpServer() { }

		public static Thread? OpenChariotHttpServer() {
			Program.WriteLine("Opening http server...");
			var thread2 = new Thread(new ThreadStart(HttpServerThread));
			thread2.Start();
			return (thread2);
		}
		private static async void HttpServerThread() {
			var builder = WebApplication.CreateBuilder();
			builder.WebHost.ConfigureKestrel(options => {
				options.ListenAnyIP(ServerPort);
			});
			builder.Services.AddHttpClient();
			builder.Services.AddCors(options => {
				options.AddDefaultPolicy(policy => {
					policy
							.AllowAnyOrigin()
							.AllowAnyMethod()
							.AllowAnyHeader();
				});
			});
			var app = builder.Build();
			app.UseCors();

			app.MapRoutes();
			Program.WriteLine("Http Server Ready...");
			await app.RunAsync();
		}
		private static void MapRoutes(this WebApplication app) {
			app.MapPost("/{guildId}/player", (Delegate)PostGenericCommandAsync);
			app.MapGet("/presence/{userId}/voice", (Delegate)UserVoicePresenceRouteHandler);
			app.MapGet("/presence/{userId}/stalk", (Delegate)UserStalkPresenceRouteHandler);
			app.MapPost("/peer-in/dice-roller", (Delegate)DiceRollerRouteHandler);
			app.MapPost("/peer-in/mute-me", (Delegate)MuteMeRouteHandler);
		}
		static public async Task<IResult> PostGenericCommandAsync(string guildId, HttpContext context) {
			var payload = await context.Request.ReadFromJsonAsync<PlayerGenericCommand>();
			if (payload is null)
				return Results.BadRequest("Payload was null.");
			try {
				payload.GuildId = guildId;
				payload.TargetBot = "ChariotSanzzo";
				ChariotSanzzoHttpServer.FunctionsSwitch(payload);
				return Results.Ok();
			} catch (Exception ex) {
				return Results.BadRequest($"Exception: {ex.Message}");
			}
		}
		public static async Task GjallarhornGenericCommandPost(GjallarhornPostBody body) {
			try {
				var response = await _httpClient.PostAsJsonAsync($"http://{GjallarhornHost}:{GjallarhornPort}/player/full", body);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
		private static async void FunctionsSwitch(PlayerGenericCommand genericCommand) {
			var gCtx = new GjallarhornContext();
			await gCtx.GjallarhornContextAsync(genericCommand);
			gCtx.Data.WithResponse = false;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		static public async Task<IResult> UserVoicePresenceRouteHandler(HttpContext context, ulong userId) {
			var state = await Program.PresenceSentinel.Registry.GetOrCreate(userId);
			if (state.Get<string?>("voice.channelId") == null)
				await Program.PresenceSentinel.ForceResolveVoiceAsync(userId);
			return Results.Ok(new {
				UserId = state.UserId,
				GuildId = state.Get<string>("voice.guildId"),
				GuildName = state.Get<string>("voice.guildName"),
				ChannelId = state.Get<string>("voice.channelId"),
				ChannelName = state.Get<string>("voice.channelName"),
				UpdatedAt = state.UpdatedAt
			});
		}
		static public async Task<IResult> UserStalkPresenceRouteHandler(HttpContext context, ulong userId) {
			return Results.Text(await Program.PresenceSentinel.GetForceStalkeUserJsonStringAsync(userId), "text/plain");
		}
		static public async Task<IResult> DiceRollerRouteHandler([FromBody] DiceExpressionDto dto, HttpRequest request) {
			// request.Headers.TryGetValue("STP-AlbinaUserId", out var albinaUserId);
			request.Headers.TryGetValue("STP-DiscordUserId", out var discordUserIdRaw);

			var diceExpression = dto.ToDiceExpression();
			if (diceExpression.IsValid == false)
				return Results.BadRequest("Invalid dice expression.");
			var results = diceExpression.Roll();
			if (!string.IsNullOrEmpty(discordUserIdRaw) && ulong.TryParse(discordUserIdRaw, out var discordUserId)) {
				var (success, embed) = await results.ToDiscordEmbedAsync(discordUserId);
				if (success) {
					await Program.Client!.SendMessageAsync(
						await Program.Client!.GetChannelAsync(756752942085963847),
						embed
					);
				}
			}
			return Results.Ok(new { diceResults = results });
		}
		static public async Task<IResult> MuteMeRouteHandler(HttpRequest request) {
			try {
				if (!request.Headers.TryGetValue("STP-DiscordUserId", out var discordUserIdRaw)
					|| !ulong.TryParse(discordUserIdRaw, out var discordUserId)
				) return Results.BadRequest(new { message = "Missing discordUserId" });
				var userPresenceState = await Program.PresenceSentinel.ForceResolveVoiceAsync(discordUserId);
				if (userPresenceState == null)
					return Results.Ok(new { state = "disconnected" });
				if (!ulong.TryParse(userPresenceState.Get<string>("voice.channelId"), out var channelId))
					return Results.Ok(new { state = "disconnected" });
				var voiceChannel = await Program.Client!.GetChannelAsync(channelId);
				if (voiceChannel == null)
					return Results.Ok(new { state = "disconnected" });
				var user = voiceChannel.Users.FirstOrDefault((user) => user.Id == discordUserId);
				if (user == null)
					return Results.Ok(new { state = "disconnected" });
				if (user.VoiceState.IsServerMuted) {
					await user.SetMuteAsync(false);
					return Results.Ok(new { state = "unmuted" });
				}
				await user.SetMuteAsync(true);
				return Results.Ok(new { state = "muted" });
			} catch {
				return Results.BadRequest(new { message = "exception" });
			}
		}
	}
}

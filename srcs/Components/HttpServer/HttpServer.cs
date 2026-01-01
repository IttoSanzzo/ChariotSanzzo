using ChariotSanzzo.Components.MusicComponent;

namespace ChariotSanzzo.Components.HttpServer {
	public class PlayerGenericCommand {
		public string		Command		{get;set;} = "";
		public string		TargetBot	{get; set;} = "";
		public string		UserId		{get;set;} = "";
		public string		GuildId		{get; set;} = "";
		public string?	TrackUrl	{get;set;} = null;
		public string?	ChannelId	{get;set;} = null;
		public long?		Position	{get;set;} = null;
		public bool			Priority	{get;set;} = true;
	}
	public class GjallarhornPostBody {
		public string?	TrackUrl				{get;set;} = null;
		public string		Command					{get;set;} = "";
		public string		UserId					{get;set;} = "";
		public string		Color						{get;set;} = "";
		public string?	Message					{get;set;} = "";
		public string?	ChannelId				{get;set;} = null;
	}
	public static class ChariotSanzzoHttpServer {
		private static readonly HttpClient _httpClient	= new();
		private static readonly string ServerPortString	= Environment.GetEnvironmentVariable("HTTP_SERVER_PORT") ?? throw new InvalidOperationException("HTTP_SERVER_PORT not set");
		private static readonly int		 ServerPort				= int.Parse(ServerPortString);
		private static readonly string GjallarhornHost	= Environment.GetEnvironmentVariable("GJALLARHORN_HOST") ?? throw new InvalidOperationException("GJALLARHORN_HOST not set");
		private static readonly string GjallarhornPort	= Environment.GetEnvironmentVariable("GJALLARHORN_PORT") ?? throw new InvalidOperationException("GJALLARHORN_PORT not set");
		
		static ChariotSanzzoHttpServer() {}

		public static Thread?		OpenChariotHttpServer() {
			Program.WriteLine("Opening http server...");
			var thread2 = new Thread(new ThreadStart(HttpServerThread));
			thread2.Start();
			return (thread2);
		}
		private static async void	HttpServerThread() {
			var app = await Task.Run(() => {
				var builder = WebApplication.CreateBuilder();
				builder.WebHost.ConfigureKestrel(options => {
					options.ListenAnyIP(ServerPort);
				});
				builder.Services.AddHttpClient();
				return builder.Build();
			});

			app.MapRoutes();
			Program.WriteLine("Http Server Ready...");
			app.Run();
		}
		private static void MapRoutes(this WebApplication app) {
			app.MapPost("/{guildId}/player", (Delegate)PostGenericCommandAsync);
			app.MapGet("/presence/{userId}/voice", (Delegate)UserVoicePresenceRouteHandler);
			app.MapGet("/presence/{userId}/stalk", (Delegate)UserStalkPresenceRouteHandler);
		}
		static public async Task<IResult>	PostGenericCommandAsync(string guildId, HttpContext context) {
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
		public static async Task					GjallarhornGenericCommandPost(GjallarhornPostBody body) {
			try {
				var response = await _httpClient.PostAsJsonAsync($"http://{GjallarhornHost}:{GjallarhornPort}/player/full", body);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
		private static async void					FunctionsSwitch(PlayerGenericCommand genericCommand) {
			var gCtx = new GjallarhornContext();
			await gCtx.GjallarhornContextAsync(genericCommand);
			gCtx.Data.WithResponse = false;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		static public async Task<IResult>	UserVoicePresenceRouteHandler(HttpContext context, ulong userId) {
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
		static public async Task<IResult>	UserStalkPresenceRouteHandler(HttpContext context, ulong userId) {
			return Results.Text(await Program.PresenceSentinel.GetForceStalkeUserJsonStringAsync(userId), "text/plain");
		}
	}
}
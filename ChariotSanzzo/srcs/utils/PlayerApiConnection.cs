using ChariotSanzzo.Components.MusicComponent;

namespace ChariotSanzzo.HttpServer {
	public class PlayerGenericCommand {
		public string?	TrackUrl	{get;set;} = null;
		public string		Command		{get;set;} = "";
		public string		UserId		{get;set;} = "";
		public string?	ChannelId	{get;set;} = null;
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

			app.MapPost("/player", (Delegate)PostGenericCommandAsync);

			Program.WriteLine("Http Server Ready...");
			app.Run();
		}
		static public async Task<IResult> PostGenericCommandAsync(HttpContext context) {
			var payload = await context.Request.ReadFromJsonAsync<PlayerGenericCommand>();
			if (payload is null)
				return Results.BadRequest("Payload was null.");
			try {
				ChariotSanzzoHttpServer.FunctionsSwitch(payload);
				return Results.Ok();
			} catch (Exception ex) {
				return Results.BadRequest($"Exception: {ex.Message}");
			}
		}
		public static async Task	GjallarhornGenericCommandPost(GjallarhornPostBody body) {
			try {
				var response = await _httpClient.PostAsJsonAsync($"http://{GjallarhornHost}:{GjallarhornPort}/player/full", body);
			} catch (Exception ex) {
				Console.WriteLine(ex);
			}
		}
		private static async void	FunctionsSwitch(PlayerGenericCommand genericCommand) {
			var gCtx = new GjallarhornContext(genericCommand);
			gCtx.Data.WithResponse = false;
			gCtx.Data.Priority = true;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
	}
}
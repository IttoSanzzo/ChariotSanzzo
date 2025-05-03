using Gjallarhorn.Components;

namespace Gjallarhorn.HttpServer {
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
	public static class GjallarhornHttpServer {
		private static readonly string ServerPortString	= Environment.GetEnvironmentVariable("HTTP_SERVER_PORT") ?? throw new InvalidOperationException("HTTP_SERVER_PORT not set");
		private static readonly int		 ServerPort				= int.Parse(ServerPortString);
		
		static GjallarhornHttpServer() {}

		public static Thread? OpenGjallarhornHttpServer() {
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
			app.MapPost("/player/full", (Delegate)PostFullCommandAsync);

			Program.WriteLine("Http Server Ready...");
			app.Run();
		}
		static public async Task<IResult> PostGenericCommandAsync(HttpContext context) {
			var payload = await context.Request.ReadFromJsonAsync<PlayerGenericCommand>();
			if (payload is null)
				return Results.BadRequest("Payload was null.");
			try {
				GjallarhornHttpServer.GenericFunctionSwitch(payload);
				return Results.Ok();
			} catch (Exception ex) {
				return Results.BadRequest($"Exception: {ex.Message}");
			}
		}
		static public async Task<IResult> PostFullCommandAsync(HttpContext context) {
			var payload = await context.Request.ReadFromJsonAsync<GjallarhornPostBody>();
			if (payload is null)
				return Results.BadRequest("Payload was null.");
			try {
				GjallarhornHttpServer.FullFunctionSwitch(payload);
				return Results.Ok();
			} catch (Exception ex) {
				return Results.BadRequest($"Exception: {ex.Message}");
			}
		}

		private static async void	GenericFunctionSwitch(PlayerGenericCommand payload) {
			try {
				var gCtx = new GjallarhornContext(payload);
				await FunctionsSwitch(gCtx);
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		}
		private static async void	FullFunctionSwitch(GjallarhornPostBody payload) {
			try {
				var gCtx = new GjallarhornContext(payload);
				await FunctionsSwitch(gCtx);
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		}
		private static async Task	FunctionsSwitch(GjallarhornContext gCtx) {
			switch (gCtx._command) {
				case ("Message"):
					await GjallarhorCalls.SendEmbedMessageAsync(gCtx);
				break;
				case ("Play"):
					await GjallarhorCalls.PlayAsync(gCtx);
				break;
				case ("Loop"):
					await GjallarhorCalls.LoopAsync(gCtx);
				break;
				case ("Pause"):
					await GjallarhorCalls.PauseAsync(gCtx);
				break;
				case ("Stop"):
					await GjallarhorCalls.StopAsync(gCtx);
				break;
				case ("ControlPanel"):
					await GjallarhorCalls.ControlPanelAsync(gCtx);
				break;
				default:
					Program.ColorWriteLine(ConsoleColor.Red, $"FunctionsSwitch: Command received was not valid. ({gCtx._command})");
				break;
			}
		}
	}
}
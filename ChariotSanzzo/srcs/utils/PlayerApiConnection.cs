using ChariotSanzzo.Components.MusicComponent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace ChariotSanzzo.HttpServer {
	public class PlayerGenericCommand {
		public string?	TrackUrl	{get;set;} = null;
		public string		Command		{get;set;} = "";
		public string		UserId		{get;set;} = "";
		public string?	ChannelId	{get;set;} = null;
	}
	public static class ChariotSanzzoHttpServer {
	// M. Members Variables

	// C. Constructors
		static ChariotSanzzoHttpServer() {}
		
	// 0. Start
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
					options.ListenAnyIP(11768);
				});
				builder.Services.AddHttpClient();
				return builder.Build();
			});

			app.MapPost("/player/{BotName}", PostGenericCommandAsync);

			Program.WriteLine("Http Server Ready...");
			app.Run();
		}
		static public async Task<IResult> PostGenericCommandAsync(string botName, HttpContext context) {
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
		public static async Task	GjallarhornGenericCommandPost(string? content) {}



	// 	private static async void	ControlPanelClientThread(HttpListenerContext context) {
	// 		Program.WriteLine("Socket accepted from ControlPanel!");
	// 		var webSocketContext = await context.AcceptWebSocketAsync(null);
	// 		string response = "";
	// 		var	webSocket = webSocketContext.WebSocket;
	// 		var	buffer = new byte[2048];
	// 		try {
	// 			var	result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
	// 			Program.WriteLine("Socket received connectionn from ControlPanel!");
	// 			while (!webSocket.CloseStatus.HasValue) {
	// 				response = Encoding.UTF8.GetString(buffer, 0, result.Count);
	// 				await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"<|ACK|>")), WebSocketMessageType.Text, true, CancellationToken.None);
	// 				Program.WriteLine("Socket received connectionn from ControlPanel!");
	// 				if (string.IsNullOrEmpty(response) == false)
	// 					ChariotSanzzoSocket.FunctionsSwitch(response);
	// 				result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
	// 			}
	// 			await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
	// 		} catch(Exception ex) {
	// 			Program.WriteException(ex);
	// 		}
	// 	}

	// // 2. Core Functions
	// 	public static async Task	GjallarhornPost(string? content) {
	// 		if (string.IsNullOrEmpty(content) == true) {
	// 			Program.ColorWriteLine(ConsoleColor.Red, "Tried post empty or null content to Gjallarhorn via Socket!");
	// 			return ;
	// 		}
	// 		using Socket client = new(_gjallarhornEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
	// 		while (true) {
	// 			Program.WriteLine("Posting to Gjallarhorn via Socket!");
	// 			await client.ConnectAsync(_gjallarhornEndpoint);
	// 			await client.SendAsync(Encoding.UTF8.GetBytes(content), SocketFlags.None);
	// 	// Receive <|ACK|>
  //   			var buffer = new byte[512];
  //   			var received = await client.ReceiveAsync(buffer, SocketFlags.None);
  //   			var response = Encoding.UTF8.GetString(buffer, 0, received);
  //   			if (response == "<|ACK|>") {
	// 				Program.WriteLine($"Socket received: \"{response}\"");
  //   	    		break;
  //   			}
	// 		}
	// 		client.Shutdown(SocketShutdown.Both);
	// 	}
		private static async void	FunctionsSwitch(PlayerGenericCommand genericCommand) {
			var gCtx = new GjallarhornContext(genericCommand);
			gCtx.Data.WithResponse = false;
			gCtx.Data.Priority = true;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
	}
}
using ChariotSanzzo;
using ChariotSanzzo.Components;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace ChariotSanzzo {
	public static class ChariotSanzzoSocket {
	// 0. Members Variables
		private static ulong				_GjallarhornId	{get; set;} = 1273070668451418122;
		private static string				_hostName		= Dns.GetHostName();
		private static IPHostEntry			_localhost		= Dns.GetHostEntryAsync(_hostName).Result;
		private static IPAddress			_localIpAddress	= _localhost.AddressList[0];
		private const int					_gjallarhornPort		= 11366;
		private const int					_controlPanelPort		= 11368;
		private static readonly IPEndPoint	_gjallarhornEndpoint	= new(_localIpAddress, _gjallarhornPort);
		private static readonly IPEndPoint	_controlPanelEndpoint	= new(_localIpAddress, _controlPanelPort);

	// 1. Constructors
		static ChariotSanzzoSocket() {
		}
		public static Thread? OpenChariotControlSocket() {
			Program.WriteLine("Opening Sockets");
			var thread2 = new Thread(new ThreadStart(ControlPanelSocketHandler));
			thread2.Start();
			return (thread2);
		}
		private static async void	ControlPanelSocketHandler() {
			var listener = new HttpListener();
			listener.Prefixes.Add($"http://localhost:{_controlPanelPort}/");
			listener.Start();
			Program.WriteLine("ControlPanel Socket Ready...");
			while (true) {
				var context = await listener.GetContextAsync();
				ChariotSanzzoSocket.ControlPanelClientThread(context);
			}
		}
		private static async void	ControlPanelClientThread(HttpListenerContext context) {
			Program.WriteLine("Socket accepted from ControlPanel!");
			var webSocketContext = await context.AcceptWebSocketAsync(null);
			string response = "";
			var	webSocket = webSocketContext.WebSocket;
			var	buffer = new byte[2048];
			try {
				var	result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				Program.WriteLine("Socket received connectionn from ControlPanel!");
				while (!webSocket.CloseStatus.HasValue) {
					response = Encoding.UTF8.GetString(buffer, 0, result.Count);
					await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes($"<|ACK|>")), WebSocketMessageType.Text, true, CancellationToken.None);
					Program.WriteLine("Socket received connectionn from ControlPanel!");
					if (string.IsNullOrEmpty(response) == false)
						ChariotSanzzoSocket.FunctionsSwitch(response);
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				}
				await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		}

	// 2. Core Functions
		public static async Task GjallarhornPost(string? content) {
			if (string.IsNullOrEmpty(content) == true) {
				Program.ColorWriteLine(ConsoleColor.Red, "Tried post empty or null content to Gjallarhorn via Socket!");
				return ;
			}
			using Socket client = new(_gjallarhornEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			while (true) {
				Program.WriteLine("Posting to Gjallarhorn via Socket!");
				await client.ConnectAsync(_gjallarhornEndpoint);
				await client.SendAsync(Encoding.UTF8.GetBytes(content), SocketFlags.None);
		// Receive <|ACK|>
    			var buffer = new byte[512];
    			var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    			var response = Encoding.UTF8.GetString(buffer, 0, received);
    			if (response == "<|ACK|>") {
					Program.WriteLine($"Socket received: \"{response}\"");
    	    		break;
    			}
			}
			client.Shutdown(SocketShutdown.Both);
		}
	
		private static async void	FunctionsSwitch(string src) {
			try {
				var gCtx = new GjallarhornContext(src);
				switch (gCtx._command) {
					case ("Message"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await ChariotMusicCalls.SendEmbedMessageAsync(gCtx);
					break;
					case ("Play"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await ChariotMusicCalls.PlayAsync(gCtx);
					break;
					case ("Loop"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await ChariotMusicCalls.LoopAsync(gCtx);
					break;
					case ("Previous"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						// await ChariotMusicCalls.PreviousAsync(gCtx);
					break;
					case ("Pause"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await ChariotMusicCalls.PauseAsync(gCtx);
					break;
					case ("Next"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						// await ChariotMusicCalls.NextAsync(gCtx);
					break;
					case ("Shuffle"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						// await ChariotMusicCalls.ShuffleAsync(gCtx);
					break;
					case ("Stop"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await ChariotMusicCalls.StopAsync(gCtx);
					break;
					default:
						Program.ColorWriteLine(ConsoleColor.Red, $"FunctionsSwitch: Command received was not valid. ({gCtx._command})");
					break;
				}
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		}
	}
}
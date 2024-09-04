using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using Gjallarhorn.Components;

namespace Gjallarhorn.Utils {
	public static class GjallarhornSocket {
	// 0. Members Variables
		private static readonly string		_hostName				= Dns.GetHostName();
		private static readonly IPHostEntry	_localhost				= Dns.GetHostEntryAsync(_hostName).Result;
		private static readonly IPAddress	_localIpAddress			= _localhost.AddressList[0];
		private const int					_chariotSanzzoPort		= 11766;
		private const int					_controlPanelPort		= 11767;
		private static readonly IPEndPoint	_chariotSanzzoEndpoint	= new(_localIpAddress, _chariotSanzzoPort);
		private static readonly IPEndPoint	_controlPanelEndpoint	= new(_localIpAddress, _controlPanelPort);

	// 1. Running
		public static (Thread?, Thread?) OpenGjallarhornSocket() {
			Program.WriteLine("Opening Sockets");
			var thread1 = new Thread(new ThreadStart(ChariotSanzzoSocketHandler));
			thread1.Start();
			var thread2 = new Thread(new ThreadStart(ControlPanelSocketHandler));
			thread2.Start();
			return (thread1, thread2);
		}

	// 2. Core Functions
		private static async void	ControlPanelSocketHandler() {
			var listener = new HttpListener();
			listener.Prefixes.Add($"http://+:{_controlPanelPort}/");
			listener.Start();
			Program.WriteLine("ControlPanel Socket Ready...");
			while (true) {
				var context = await listener.GetContextAsync();
				GjallarhornSocket.ControlPanelClientThread(context);
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
						GjallarhornSocket.FunctionsSwitch(response);
					result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				}
				await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);
			} catch(Exception ex) {
				Program.WriteException(ex);
			}
		}
		private static async void	ChariotSanzzoSocketHandler() {
		// 0. Setting Up
		// 1. Building Socket
			using Socket listener = new(_chariotSanzzoEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			listener.Bind(_chariotSanzzoEndpoint);
			listener.Listen(100);
			Program.WriteLine("ChariotSanzzo Socket Ready...");
			while (true) {
				Socket handler = await listener.AcceptAsync();
				Program.WriteLine("Socket received connectionn from CharioSanzzo!");
			// 2. Running
				string response = "";
				while (true) {
					byte[]	buffer = new byte[2048];
					int		received = await handler.ReceiveAsync(buffer, SocketFlags.None);
					response = Encoding.UTF8.GetString(buffer, 0, received);
					if (response.Contains("<|EOM|>") == true) {
						response = response.Replace("<|EOM|>", "");
						Program.ColorWriteLine(ConsoleColor.Magenta, $"Socket: Message received!");
						await handler.SendAsync(Encoding.UTF8.GetBytes("<|ACK|>"));
					}
					break;
				}
				GjallarhornSocket.FunctionsSwitch(response);
			}
		}
		private static async void	FunctionsSwitch(string src) {
			try {
				var gCtx = new GjallarhornContext(src);
				switch (gCtx._command) {
					case ("Message"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await GjallarhorCalls.SendEmbedMessageAsync(gCtx);
					break;
					case ("Play"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await GjallarhorCalls.PlayAsync(gCtx);
					break;
					case ("Loop"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await GjallarhorCalls.LoopAsync(gCtx);
					break;
					case ("Pause"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await GjallarhorCalls.PauseAsync(gCtx);
					break;
					case ("Stop"):
						Program.WriteLine($"Command received: {gCtx._command}.");
						await GjallarhorCalls.StopAsync(gCtx);
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
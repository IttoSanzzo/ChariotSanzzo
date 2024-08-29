using ChariotSanzzo;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gjallarhorn.Utils {
	public static class GjallarhornSocket {
	// 0. Members Variables
		private static ulong		_GjallarhornId	{get; set;} = 1273070668451418122;
		private static string		_hostName		= Dns.GetHostName();
		private static IPHostEntry	_localhost		= Dns.GetHostEntryAsync(_hostName).Result;
		private static IPAddress	_localIpAddress	= _localhost.AddressList[0];
		private static IPEndPoint	_ipEndPoint		= new(_localIpAddress, 11366);

	// 1. Constructors
		static GjallarhornSocket() {
		}

	// 2. Core Functions
		public static async Task Post(string? content) {
			if (string.IsNullOrEmpty(content) == true) {
				Program.ColorWriteLine(ConsoleColor.Red, "Tried post empty or null content to Gjallarhorn via Socket!");
				return ;
			}
			using Socket client = new(_ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			while (true) {
				Program.WriteLine("Posting to Gjallarhorn via Socket!");
				await client.ConnectAsync(_ipEndPoint);
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
	}
}
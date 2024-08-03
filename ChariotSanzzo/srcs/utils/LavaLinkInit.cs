using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;

namespace ChariotSanzzo {
	public static class LavalinkInit {

		// 0. Member Variables
		public static ConnectionEndpoint?		_endPoint {get; set;} = null;
		public static LavalinkConfiguration?	_config {get; set;} = null;
		public static LavalinkExtension?		_lavalink {get; set;} = null;

		// 1. Main Functions (https://lavalink.darrennathanael.com/SSL/lavalink-with-ssl/#hosted-by-ajiedev)
		public static void LavalinkRunInit(this DiscordClient commands) {
			LavalinkInit._endPoint = new ConnectionEndpoint {
				Hostname = "v3.lavalink.rocks",
				Port = 443,
				Secured = true
			};
			LavalinkInit._config = new LavalinkConfiguration {
				Password = "horizxon.tech",
				RestEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint,
				SocketEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint
			};
			_lavalink = Program.Client.UseLavalink();
			/*
			LavalinkInit._endPoint = new ConnectionEndpoint {
				Hostname = "v3.lavalink.rocks",
				Port = 443,
				Secured = true
			};
			LavalinkInit._config = new LavalinkConfiguration {
				Password = "horizxon.tech",
				RestEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint,
				SocketEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint
			};
			_lavalink = Program.Client.UseLavalink();
			*/
		}
		public static void LavalinkConnectAsync(this DiscordClient commands) {
			if (LavalinkInit._config != null && LavalinkInit._lavalink != null)
				LavalinkInit._lavalink.ConnectAsync(LavalinkInit._config);
		}
	}
}

using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;

namespace ChariotSanzzo {
	public static class LavalinkInit {

		// 0. Member Variables
		public static ConnectionEndpoint?		_endPoint		{get; set;} = null;
		public static LavalinkConfiguration?	_config			{get; set;} = null;
		public static LavalinkExtension?		_lavalink		{get; set;} = null;
		private static int						_serverSwitch	{get; set;} = 1;

		// 1. Main Functions (https://lavalink.darrennathanael.com/SSL/lavalink-with-ssl/#hosted-by-ajiedev)
		public static void LavalinkRunInit(this DiscordClient commands) {
			string	hostname = "";
			int		port = 443;
			bool	secured = true;
			string	password = "";

			switch (LavalinkInit._serverSwitch) {
				case (0): // type "pm2 start 0" at the linux terminal
					hostname = "localhost";
					password = "";
					secured = false;
					port = 2333;
				break;
				case (1):
					hostname = "v3.lavalink.rocks";
					password = "horizxon.tech";
				break;
				case (2):
					hostname = "lavalink1.skybloxsystems.com";
					password = "s4DarqP$&y";
				break;
				case (3):
					hostname = "lava-v3.ajieblogs.eu.org";
					password = "https://dsc.gg/ajidevserver";
				break;
			}
			LavalinkInit._endPoint = new ConnectionEndpoint {
				Hostname = hostname,
				Port = port,
				Secured = secured
			};
			LavalinkInit._config = new LavalinkConfiguration {
				Password = password,
				RestEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint,
				SocketEndpoint = (ConnectionEndpoint)LavalinkInit._endPoint
			};
			LavalinkInit._lavalink = Program.Client.UseLavalink();
		}
		public static void LavalinkConnectAsync(this DiscordClient commands) {
			if (LavalinkInit._config != null && LavalinkInit._lavalink != null)
				LavalinkInit._lavalink.ConnectAsync(LavalinkInit._config);
		}
	}
}

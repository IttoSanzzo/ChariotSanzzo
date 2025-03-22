using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Configuration;

namespace ChariotSanzzo {
	public static class LavalinkInit {
	// M. Member Variables
		public static ConnectionEndpoint?		EndPoint		{get; set;} = null;
		public static LavalinkConfiguration?	Config			{get; set;} = null;
		public static LavalinkExtension?		Lavalink		{get; set;} = null;
		private static int						ServerSwitch	{get; set;} = 1;

	// 0. Main Functions (https://lavalink.darrennathanael.com/SSL/lavalink-with-ssl/#hosted-by-ajiedev)
		public static void LavalinkRunInit(this DiscordClient commands) {
			if (Program.LocalLavalink == true)
				LavalinkInit.ServerSwitch = 0;
			string	hostname = "";
			int		port = 443;
			bool	secured = true;
			string	password = "";

			switch (LavalinkInit.ServerSwitch) {
				case (0): // type "pm2 start 0" at the linux terminal
					var	builder = new ConfigurationBuilder()
						.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
						.AddJsonFile("ChariotLavalink.json", optional: true, reloadOnChange: true)
						.AddUserSecrets<Program>();
						IConfiguration config = builder.Build();
						string?	cHostname = config.GetValue<string>("ChariotLavalink:hostname");
						string?	cPassword = config.GetValue<string>("ChariotLavalink:password");
						bool?	cSecured = config.GetValue<bool>("ChariotLavalink:secured");
						int?	cPort = config.GetValue<int>("ChariotLavalink:port");
					if (cHostname == null ||
						cPassword == null ||
						cSecured == null ||
						cPort == null) {
						Program.WriteLine("Error -> ChariotLavalink.json importing failure!");
						return ;
					}
					hostname = cHostname;
					password = cPassword;
					secured = (bool)cSecured;
					port = (int)cPort;
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
			LavalinkInit.EndPoint = new ConnectionEndpoint {
				Hostname = hostname,
				Port = port,
				Secured = secured
			};
			LavalinkInit.Config = new LavalinkConfiguration {
				Password = password,
				RestEndpoint = (ConnectionEndpoint)LavalinkInit.EndPoint,
				SocketEndpoint = (ConnectionEndpoint)LavalinkInit.EndPoint
			};
			LavalinkInit.Lavalink = Program.Client.UseLavalink();
		}
		public static void LavalinkConnectAsync(this DiscordClient commands) {
			if (LavalinkInit.Config != null && LavalinkInit.Lavalink != null)
				LavalinkInit.Lavalink.ConnectAsync(LavalinkInit.Config);
		}
	}
}

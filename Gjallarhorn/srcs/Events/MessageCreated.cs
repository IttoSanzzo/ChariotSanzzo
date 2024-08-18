using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;

namespace Gjallarhorn.Events {
	public static class ChariotConn {
		// -1. Struct
			public struct t_tools {
				public ulong					serverId	{get; set;}
				public LavalinkExtension		llInstace	{get; set;}
				public LavalinkNodeConnection	node		{get; set;}
				public LavalinkGuildConnection	conn		{get; set;}
			}

		// 0. Members Variables
		static ulong	_GjallarhornId	{get; set;} = 1273070668451418122;

		// 1. Core Event
		public static async Task GetChariotCommunication(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Channel.Id != 1273347333429399633 || ctx.Message.Content.Contains("GjallarhornCall") == false)
				return ;
			// Console.WriteLine($"Chariotto Message: {ctx.Message.Content}");
			string[] args = ctx.Message.Content.Split('\n');
			for (int i = 0; i < args.Length; i++)
				Console.WriteLine(args[i]);
			Console.WriteLine("Chariotto End");
			if (args.Length < 2) {
				Console.WriteLine("Content received had only one part");
				return ;
			}
			switch (args[1]) {
				case ("Message"):
					Console.WriteLine("\nMESSAGE RECEIVED!");
					await ChariotConn.SendEmbedMessageAsync(ulong.Parse(args[2]), (DiscordColor)(Convert.ToInt32(args[3].Substring(1), 16)), args[4]);
				break;
				case ("Play"):
					Console.WriteLine("\nPLAY COMMAND RECEIVED!");
					await ChariotConn.PlayAsync(ulong.Parse(args[2]), ulong.Parse(args[3]), args[4]);
				break;
				case ("Exit"):
					Console.WriteLine("\nEXIT COMMAND RECEIVED!");
					await ChariotConn.StopAsync(ulong.Parse(args[2]), ulong.Parse(args[3]));
				break;
			}

		}
	// Temporary
		private static async Task	SendEmbedMessageAsync(ulong channelId, DiscordColor color, string message) {
			if (Program.Client == null)
				return ;
			DiscordChannel channel = await Program.Client.GetChannelAsync(channelId);
		// 0. Embed Construction
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(color);
			embed.WithDescription(message);
			await ChariotConn.DelMssTimerAsync(15, await channel.SendMessageAsync(embed.Build()));
		}
		private static async Task	PlayAsync(ulong channelId, ulong voiceChannelId, string link) {
			if (Program.Client == null)
				return ;
			var embed = new DiscordEmbedBuilder();
			DiscordChannel channel = await Program.Client.GetChannelAsync(channelId);
			DiscordChannel voiceChannel = await Program.Client.GetChannelAsync(voiceChannelId);
			t_tools tools = await ChariotConn.GetLavalinkTools(voiceChannel, 0);
		// 0. Find Track
			LavalinkLoadResult searchQuery = await tools.node.Rest.GetTracksAsync(link, LavalinkSearchType.Plain);
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ChariotConn.DelMssTimerAsync(20, await channel.SendMessageAsync(embed.Build()));
				return ;
			}
			LavalinkTrack track = searchQuery.Tracks.First();
			await tools.conn.PlayAsync(track);
		// 1. Embed Construction
			embed.WithColor(DiscordColor.DarkBlue);
			embed.WithDescription($"SFX: [{track.Title}]({track.Uri.AbsoluteUri}) Played!");
			await ChariotConn.DelMssTimerAsync(20, await channel.SendMessageAsync(embed.Build()));
		}
		private static async Task	StopAsync(ulong channelId, ulong voiceChannelId) {
				if (Program.Client == null)
					return ;
				DiscordChannel channel = await Program.Client.GetChannelAsync(channelId);
				DiscordChannel voiceChannel = await Program.Client.GetChannelAsync(voiceChannelId);
				t_tools tools = await ChariotConn.GetLavalinkTools(voiceChannel, 1);
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.Black);
				embed.WithDescription("SFX stopped.");
				await tools.conn.StopAsync();
				await tools.conn.DisconnectAsync();
				await ChariotConn.DelMssTimerAsync(20, await channel.SendMessageAsync(embed.Build()));
		}
		private static async Task<t_tools>	GetLavalinkTools(DiscordChannel channel, int type) {
			t_tools	tools = new t_tools();
			tools.llInstace = Program.Client.GetLavalink();
			tools.node = tools.llInstace.ConnectedNodes.Values.First();
			if (type != 1 && ChariotConn.CheckGjallarhornInChannel(channel.Users.ToArray()) == false)
				await tools.node.ConnectAsync(channel);
			tools.conn = tools.node.GetGuildConnection(channel.Guild);
			tools.serverId = channel.Guild.Id;
			return (tools);
		}
		private static bool	CheckGjallarhornInChannel(DiscordMember[] members) {
			for (int i = 0; i < members.Length; i++)
				if (members[i].Id == ChariotConn._GjallarhornId)
					return (true);
			return (false);
		}
	// Miscs
		private static async Task	DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}

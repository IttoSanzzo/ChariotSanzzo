using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace Gjallarhorn.Components {
	public static class GjallarhorCalls {
	// -1. Struct
		public struct t_tools {
			public ulong					serverId	{get; set;}
			public LavalinkExtension		llInstace	{get; set;}
			public LavalinkNodeConnection	node		{get; set;}
			public LavalinkGuildConnection	conn		{get; set;}
		}
	// 0. Member Variables
		private static ulong		_GjallarhornId	{get; set;} = 1273070668451418122;

	// 1. Constructor
	// 2. Core Functions
		public static async Task	SendEmbedMessageAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null)
				return ;
		// 0. Embed Construction
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx._username}", ctx._userIcon);
			embed.WithColor(ctx._color);
			embed.WithDescription(ctx._message);
			await GjallarhorCalls.DelMssTimerAsync(15, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	PlayAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null
				|| ctx._voiceChannel == null)
				return ;
				var obj = await GjallarhorCalls.GetLavalinkTools(ctx._voiceChannel, 0);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		//  Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
		// 0. Find Track
			LavalinkLoadResult searchQuery = await tools.node.Rest.GetTracksAsync(ctx._trackLink, LavalinkSearchType.Plain);
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
				return ;
			}
			LavalinkTrack track = searchQuery.Tracks.First();
			await tools.conn.PlayAsync(track);
		// 1. Embed Construction
			embed.WithColor(ctx._color);
			embed.WithDescription($"SFX: [{track.Title}]({track.Uri.AbsoluteUri}) Played!");
			await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	StopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await GjallarhorCalls.GetLavalinkTools(ctx._voiceChannel, 1);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(ctx._color);
			embed.WithDescription("SFX stopped.");
			await tools.conn.StopAsync();
			await tools.conn.DisconnectAsync();
			await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
	// E. Miscs
		private static async Task<(bool, t_tools)>	GetLavalinkTools(DiscordChannel channel, int type) {
			t_tools	tools = new t_tools();
			tools.llInstace = Program.Client.GetLavalink();
			tools.node = tools.llInstace.ConnectedNodes.Values.First();
			if (type != 1 && GjallarhorCalls.CheckInChannel(channel.Users.ToArray()) == false)
				await tools.node.ConnectAsync(channel);
			tools.conn = tools.node.GetGuildConnection(channel.Guild);
			tools.serverId = channel.Guild.Id;
			if (tools.conn == null) {
				Program.ColorWriteLine(ConsoleColor.Red, "LavalinkConnetion NULL!");
				return (false, tools);
			}
			return (true, tools);
		}
		private static bool					CheckInChannel(DiscordMember[] members) {
			for (int i = 0; i < members.Length; i++)
				if (members[i].Id == GjallarhorCalls._GjallarhornId)
					return (true);
			return (false);
		}
		private static async Task			DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}
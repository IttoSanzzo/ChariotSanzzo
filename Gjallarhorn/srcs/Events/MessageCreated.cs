using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using Gjallarhorn.Components;

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
			if (ctx.Channel.Id != 1273347333429399633 || ctx.Message.Embeds.Any() == false)
				return ;
			GjallarhornContext gCtx = new(ctx.Message.Embeds[0]);
			Program.WriteLine($"Chariotto Log Received: {gCtx._command}");
			switch (gCtx._command) {
				case ("Message"):
					await ChariotConn.SendEmbedMessageAsync(gCtx);
				break;
				case ("Play"):
					await ChariotConn.PlayAsync(gCtx);
				break;
				case ("Exit"):
					await ChariotConn.StopAsync(gCtx);
				break;
			}
		}

	// Temporary
		private static async Task	SendEmbedMessageAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null)
				return ;
		// 0. Embed Construction
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(ctx._color);
			embed.WithDescription(ctx._message);
			await ChariotConn.DelMssTimerAsync(15, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		private static async Task	PlayAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null
				|| ctx._voiceChannel == null)
				return ;
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			t_tools tools = await ChariotConn.GetLavalinkTools(ctx._voiceChannel, 0);
		// 0. Find Track
			LavalinkLoadResult searchQuery = await tools.node.Rest.GetTracksAsync(ctx._trackLink, LavalinkSearchType.Plain);
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ChariotConn.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
				return ;
			}
			LavalinkTrack track = searchQuery.Tracks.First();
			await tools.conn.PlayAsync(track);
		// 1. Embed Construction
			embed.WithColor(DiscordColor.DarkBlue);
			embed.WithDescription($"SFX: [{track.Title}]({track.Uri.AbsoluteUri}) Played!");
			await ChariotConn.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		private static async Task	StopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._chatChannel == null
				|| ctx._voiceChannel == null)
				return ;
			t_tools tools = await ChariotConn.GetLavalinkTools(ctx._voiceChannel, 1);
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(ctx._color);
			embed.WithDescription("SFX stopped.");
			await tools.conn.StopAsync();
			await tools.conn.DisconnectAsync();
			await ChariotConn.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
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

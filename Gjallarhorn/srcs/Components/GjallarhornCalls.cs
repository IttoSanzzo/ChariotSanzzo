using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using STPlib;

namespace Gjallarhorn.Components {
	public static class GjallarhorCalls {
	// -1. Struct
		public struct t_tools {
			public ulong					serverId	{get; set;}
			public LavalinkExtension		llInstace	{get; set;}
			public LavalinkNodeConnection	node		{get; set;}
			public LavalinkGuildConnection	conn		{get; set;}
		}
		public class  GuildPlayerState {
			// 0. Member Variables
				public ulong					_guildId	{get; private set;}
				public bool						_pauseState	{get; set;} = false;
				public bool						_loopState	{get; set;} = false;
				private LavalinkGuildConnection	_conn	{get; set;}

			// 1. Constructor
				public GuildPlayerState(ulong guildId, LavalinkGuildConnection conn) {
					this._guildId = guildId;
					this._conn = conn;
					this._conn.PlaybackFinished += GjallarhorCalls.LoopEventAsync;
				}
		}
	// 0. Member Variables
		private static ulong				_GjallarhornId	{get; set;} = 1273070668451418122;
		public static GuildPlayerState[]	_playerStateArr {get; private set;} = new GuildPlayerState[0];

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
			LavalinkLoadResult searchQuery;
			if (ctx._trackLink.Contains("https://") == true || ctx._trackLink.Contains("http://") == true)
				searchQuery = await tools.node.Rest.GetTracksAsync(ctx._trackLink, LavalinkSearchType.Plain);
			else
				searchQuery = await tools.node.Rest.GetTracksAsync(ctx._trackLink, LavalinkSearchType.Youtube);
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				if (ctx._chatChannel != null)
					await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
				return ;
			}
			LavalinkTrack track;
			/*
			if (ctx._trackLink.Contains("youtube.com/watch?") && ctx._trackLink.Contains("&index="))
			{
				Program.WriteLine("Here we goo");
				Program.WriteLine($"{ctx._trackLink.Substring(ctx._trackLink.IndexOf("&index=") + 7).StoI() - 1}");
				track = searchQuery.Tracks.ElementAt(ctx._trackLink.Substring(ctx._trackLink.IndexOf("&index=") + 7).StoI() - 1);
			}
			else
			*/
			track = searchQuery.Tracks.First();
			await tools.conn.PlayAsync(track);
			var playerState = GjallarhorCalls.GetGuildPlayerState(ctx._guild.Id, tools.conn);
			playerState._pauseState = false;
			playerState._loopState = false;
		// 1. Embed Construction
			embed.WithColor(ctx._color);
			embed.WithDescription($"SFX: [{track.Title}]({track.Uri.AbsoluteUri}) Played!");
			if (ctx._chatChannel != null)
				await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	StopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
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
			GjallarhorCalls.DropGuildPlayerEntry(ctx._guild.Id);
			if (ctx._chatChannel != null)
				await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	PauseAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await GjallarhorCalls.GetLavalinkTools(ctx._voiceChannel, 1);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
			if (tools.conn.CurrentState.CurrentTrack == null)
				return ;
		// Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(ctx._color);
			GuildPlayerState playerState = GjallarhorCalls.GetGuildPlayerState(ctx._guild.Id, tools.conn);
			switch (playerState._pauseState) {
				case (true):
				await tools.conn.ResumeAsync();
				embed.WithDescription("SFX Resumed.");
				playerState._pauseState = false;
				break;
				case (false):
				await tools.conn.PauseAsync();
				embed.WithDescription("SFX Paused.");
				playerState._pauseState = true;
				break;
			}
			if (ctx._chatChannel != null)
				await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	LoopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await GjallarhorCalls.GetLavalinkTools(ctx._voiceChannel, 1);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
			if (tools.conn.CurrentState.CurrentTrack == null)
				return ;
		// Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(ctx._color);
			GuildPlayerState playerState = GjallarhorCalls.GetGuildPlayerState(ctx._guild.Id, tools.conn);
			switch (playerState._loopState) {
				case (true):
				embed.WithDescription("SFX Unlooped.");
				playerState._loopState = false;
				break;
				case (false):
				embed.WithDescription("SFX Looped.");
				playerState._loopState = true;
				break;
			}
			if (ctx._chatChannel != null)
				await GjallarhorCalls.DelMssTimerAsync(20, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}
		public static async Task	ControlPanelAsync(GjallarhornContext ctx) {
			if (ctx._chatChannel == null)
				return ;
			try {
            	string publicIp = await Program.HttpCli.GetStringAsync("https://api.ipify.org");
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.DarkBlue);
				embed.WithTitle("SFX ControlPanel Link");
				embed.WithDescription($"[Here is your link for this Channel's ControlPanel](http://{publicIp}:11760/ControlPanel.html?targetBot=Gjallarhorn&userId={ctx._userId}&channelId={ctx._chatChannel.Id})");
				embed.WithFooter($"For: {ctx._username}", ctx._userIcon);
				var temp = await ctx._chatChannel.SendMessageAsync(embed);
				await Task.Delay(1000 * 15);
				await temp.DeleteAsync();
            } catch (Exception ex) {
                Program.WriteException(ex);
            }
		}
	
	// E. Miscs
		private static async Task<(bool, t_tools)>	GetLavalinkTools(DiscordChannel channel, int type) {
			t_tools	tools = new t_tools();
			tools.llInstace = Program.Client.GetLavalink();
			tools.node = tools.llInstace.ConnectedNodes.Values.First();
			if (type != 1)
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
		private static GuildPlayerState		GetGuildPlayerState(ulong guildId, LavalinkGuildConnection conn) {
			for (int i = 0; i < GjallarhorCalls._playerStateArr.Length; i++)
				if (GjallarhorCalls._playerStateArr[i]._guildId == guildId)
					return(GjallarhorCalls._playerStateArr[i]);
			return (GjallarhorCalls.CreateGuildPlayerEntry(guildId, conn));
		}
		private static void					SetGuildPauseState(ulong guildId, bool value, LavalinkGuildConnection conn) {
			GuildPlayerState guildPlayer = GjallarhorCalls.GetGuildPlayerState(guildId, conn);
			guildPlayer._pauseState = value;
		}
		private static void					SetGuildLoopState(ulong guildId, bool value, LavalinkGuildConnection conn) {
			GuildPlayerState guildPlayer = GjallarhorCalls.GetGuildPlayerState(guildId, conn);
			guildPlayer._loopState = value;
		}
		private static GuildPlayerState		CreateGuildPlayerEntry(ulong guildId, LavalinkGuildConnection conn) {
			var temp = GjallarhorCalls._playerStateArr;
			GjallarhorCalls._playerStateArr = new GuildPlayerState[GjallarhorCalls._playerStateArr.Length + 1];
			for (int i = 0; i < temp.Length; i++)
				GjallarhorCalls._playerStateArr[i] = temp[i];
			GjallarhorCalls._playerStateArr[^1] = new GuildPlayerState(guildId, conn);
			return (GjallarhorCalls._playerStateArr[^1]);
		}
		private static void					DropGuildPlayerEntry(ulong guildId) {
			short pastLess = 0;
			var temp = GjallarhorCalls._playerStateArr;
			GjallarhorCalls._playerStateArr = new GuildPlayerState[GjallarhorCalls._playerStateArr.Length -1];
			for (int i = 0; i < temp.Length; i++) {
				if (temp[i]._guildId == guildId) {
					pastLess = 1;
					continue;
				}
				GjallarhorCalls._playerStateArr[i - pastLess] = temp[i];
			}
		}	
		private static async Task			LoopEventAsync(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) {
			if (ctx.Reason != TrackEndReason.Finished)
				return;
			var playerState = GjallarhorCalls.GetGuildPlayerState(ctx.Player.Guild.Id, conn);
			if (playerState._loopState == true)
				await conn.PlayAsync(ctx.Track);
		}
		private static async Task			DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
	}
}
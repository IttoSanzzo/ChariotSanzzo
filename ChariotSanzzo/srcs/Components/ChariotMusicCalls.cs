using ChariotSanzzo.Components.MusicQueue;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;

namespace ChariotSanzzo.Components {
	public static class ChariotMusicCalls {
	// -1. Struct
		public struct t_tools {
			public ulong					ServerId	{get; set;}
			public TrackQueue				Queue		{get; set;}
			public GjallarhornContext		Ctx			{get; set;}
			public LavalinkExtension		LlInstace	{get; set;}
			public LavalinkNodeConnection	Node		{get; set;}
			public LavalinkGuildConnection	Conn		{get; set;}
		}
		public class  GuildPlayerState {
			// 0. Member Variables
				public ulong					_guildId			{get; private set;}
				public bool						_pauseState			{get; set;} = false;
				public bool						_loopState			{get; set;} = false;
				private LavalinkGuildConnection	_conn				{get; set;}

			// 1. Constructor
				public GuildPlayerState(ulong guildId, LavalinkGuildConnection conn) {
					this._guildId = guildId;
					this._conn = conn;
					this._conn.PlaybackFinished += ChariotMusicCalls.LoopEventAsync;
				}
		}
	// 0. Member Variables
		public static ulong					_ChariotSanzzoId	{get; set;} = 1070103829934260344;
		public static QueueCollection		QColle				{get; set;} = new QueueCollection();

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
			await ChariotMusicCalls.DelMssTimerAsync(15, await ctx._chatChannel.SendMessageAsync(embed.Build()));
		}	
		public static async Task	PlayAsync(GjallarhornContext ctx) {
			if (ctx._member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 0);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Purple};
			embed.WithFooter($"by: {ctx._username}", ctx._userIcon);

		// 1. Find Track
			LavalinkLoadResult	searchQuery;
			if (tools.Ctx._Data.Query.Contains("https://") == true || tools.Ctx._Data.Query.Contains("http://") == true)
				searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx._Data.Query, LavalinkSearchType.Plain);
			else
				switch ((int)ctx._Data.Plataform) {
					case (0): // Youtube
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx._Data.Query, LavalinkSearchType.Youtube);
					break;
					case (1): // Soundcloud
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx._Data.Query, LavalinkSearchType.SoundCloud);
					break;
					default: // Plain
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx._Data.Query, LavalinkSearchType.Plain);
					break;
				}
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return ;
			}

		// 2. Search Tracks
			LavalinkTrack[]	musicTracks;
			if (ctx._Data.Query.Contains("youtube.com/playlist?") == true
				|| ctx._Data.Query.Contains("spotify.com/playlist") == true
				|| (ctx._Data.Query.Contains("soundcloud.com/") == true && ctx._Data.Query.Contains("/sets") == true)) {
				musicTracks = searchQuery.Tracks.ToArray();
				for (int i = 0; i < musicTracks.Length; i++)
					tools.Queue.AddTrackToQueue(new ChariotTrack(musicTracks[i], ctx._member));
			} else {
				musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.First()};
				tools.Queue.AddTrackToQueue(new ChariotTrack(musicTracks[0], ctx._member));
			}
		// 3. Playing It!
			if (musicTracks.Length > 1) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription($"A Playlist was added! {musicTracks.Length} new tracks!");
			}
			if (tools.Conn.CurrentState.CurrentTrack == null) {
				await tools.Queue._conn.PlayAsync(await tools.Queue.UseNextTrackAsync());
				if (musicTracks.Length <= 1 && ctx._Ictx != null)
					await ctx._Ictx.DeleteResponseAsync();
				return ;
			}
			else if (musicTracks.Length == 1) {
				embed.WithDescription($"_**Added to Queue:**_ [{musicTracks[0].Title}]({musicTracks[0].Uri})\n" +
										$"**Author:** {musicTracks[0].Author}\n" +
										$"**Length:** {musicTracks[0].Length}" +
										$"\t\t**Index:** ` {tools.Queue._tracks.Length} `" );
				embed.WithThumbnail(await ChariotTrack.GetArtworkAsync(musicTracks[0].Uri));
			}
			await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
		}
		public static async Task	StopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 1);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx._username, ctx._userIcon);
			embed.WithColor(DiscordColor.Black);
			embed.WithTitle("_**Music Stopped!**_");
			embed.WithDescription($"_**Stopped Track:**_ [{tools.Conn.CurrentState.CurrentTrack.Title}]({tools.Conn.CurrentState.CurrentTrack.Uri})");
			await tools.Conn.StopAsync();
			await tools.Conn.DisconnectAsync();
			ChariotMusicCalls.QColle.DropQueue(tools.ServerId);
			if (ctx._chatChannel != null)
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
		}
		public static async Task	PauseAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 2);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
			if (tools.Conn.CurrentState.CurrentTrack == null)
				return ;
		// 0. Preparing Embed
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx._username}", ctx._userIcon);
			embed.WithColor(DiscordColor.DarkGray);
			embed.WithDescription($"_**Current Track:**_ [{tools.Conn.CurrentState.CurrentTrack.Title}]({tools.Conn.CurrentState.CurrentTrack.Uri})");

		// 1. Starting
			if (tools.Queue._pauseState == true && ctx._Data.PauseType == 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Already Paused.");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return ;
			}
			if (ctx._Data.PauseType == 2)
				ctx._Data.PauseType = tools.Queue.SwitchPause();
			var	resumeButton = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicPlayPauseButton", "Resume Track", false, new DiscordComponentEmoji(1269696547046555688));
			Program.WriteLine("Checkpoint 3");
			switch (ctx._Data.PauseType) {
				case (1): // Pause
					await tools.Queue._conn.PauseAsync();
					embed.WithTitle("_**Music Paused!**_");
					tools.Queue.SetPauseState(true);
					if (ctx._Ictx != null)
						tools.Queue.SetPauseMessage(await ctx._Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed).AddComponents(resumeButton)));
					else if (ctx._chatChannel != null) {
						var tempMss = new DiscordMessageBuilder();
						tempMss.AddEmbed(embed.Build());
						tempMss.AddComponents(resumeButton);
						tools.Queue.SetPauseMessage(await ctx._chatChannel.SendMessageAsync(tempMss));
					}
				break;
				case (0): // Resume
					await tools.Queue._conn.ResumeAsync();
					embed.WithTitle("_**Music Resumed!**_");
					tools.Queue.SetPauseState(false);
					if (tools.Queue._pauseMss != null)
						await tools.Queue._pauseMss.DeleteAsync();
					tools.Queue.SetPauseMessage(null);
					await ChariotMusicCalls.GTXEmbedTimerAsync(10, embed, ctx);
				break;
			}
		}
		public static async Task	LoopAsync(GjallarhornContext ctx) {
			if (ctx._guild == null
				|| ctx._voiceChannel == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 4);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
			if (tools.Conn.CurrentState.CurrentTrack == null)
				return ;
		// 0. Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx._username}", ctx._userIcon);
			embed.WithColor(DiscordColor.Azure);
		// 1. Core
			if (ctx._Data.LoopType == 3) {
				ctx._Data.LoopType = tools.Queue._loop + 1;
				if (ctx._Data.LoopType > 2)
					ctx._Data.LoopType = 0;
			}
			switch (ctx._Data.LoopType) {
				case (0):
					embed.WithDescription("Loop set to _**none**_!");
				break;
				case (1):
					embed.WithDescription("Loop set to _**track**_!");
				break;
				case (2):
					embed.WithDescription("Loop set to _**queue**_!");
				break;
			}
			tools.Queue.SetLoop(ctx._Data.LoopType);
			await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
		}

	// E. Miscs
		private static async Task<(bool, t_tools)>	GetLavalinkTools(GjallarhornContext ctx, int type) {
		// 0. Starting
			t_tools	tools = new t_tools();
			if (ctx._guild == null
				|| ctx._member == null)
				return (false, tools);
			tools.Ctx = ctx;
			tools.LlInstace = Program.Client.GetLavalink();
			tools.ServerId = ctx._guild.Id;
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Red};
		// 1. Primary Checks
			if (ctx._member.VoiceState == null || ctx._member.VoiceState.Channel == null) {	// Error: Enter a Voice Channel
				embed.WithDescription("Please, enter a Voice Channel!");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return (false, tools);
			}
			else if (!tools.LlInstace.ConnectedNodes.Any()) { 								// Error: Node connection not stablished
				embed.WithDescription("The connection is not stablished!");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return (false, tools);
			}
			else if (ctx._member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice) {	// Error: Enter a valid Voice Channel
				embed.WithDescription("Please, enter a valid Voice Channel!");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return (false, tools);
			}
			tools.Node = tools.LlInstace.ConnectedNodes.Values.First();
			if (type == 0)																	// Connect to VC if not in Stop Command
				await tools.Node.ConnectAsync(ctx._member.VoiceState.Channel);
			tools.Conn = tools.Node.GetGuildConnection(ctx._guild);
			if (tools.Conn == null) {														// Error: Conn Null
				embed.WithDescription("Chariot is not in a channel to perform such action!");
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return (false, tools);
			}
			if (type != 1)															// Get Queue if not in Stop Command
				tools.Queue = ChariotMusicCalls.QColle.GetQueue(tools.ServerId, tools.Conn, ctx._chatChannel);
			return (await ChariotMusicCalls.ExtraChecks(tools, ctx, type, embed), tools);
		}
		private static async Task<bool>				ExtraChecks(t_tools tools, GjallarhornContext ctx, int type, DiscordEmbedBuilder embed) {
			if (tools.Conn.CurrentState.CurrentTrack == null && type != 0) {
				switch (type) {
					case (1): // Stop
							embed.WithDescription("There's no music playing to be stopped!");
					break;
					case (2): // Pause
							embed.WithDescription("There's no music playing to be paused!");
					break;
					case (3): // Resume
							embed.WithDescription("There's no music playing to be resumed!");
					break;
					case (4): // Resume
							embed.WithDescription("There's no music playing to be looped!");
					break;
				}
				await ChariotMusicCalls.GTXEmbedTimerAsync(20, embed, ctx);
				return (false);
			}
			return (true);
		}
		private static async Task					LoopEventAsync(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) { // TODO: LoopEventAsync
			if (ctx.Reason != TrackEndReason.Finished)
				return;
			await Task.Delay(1);
			// var playerState = ChariotMusicCalls.GetGuildPlayerState(ctx.Player.Guild.Id, conn);
			// if (playerState._loopState == true)
				// await conn.PlayAsync(ctx.Track);
		}
		private static async Task					DelMssTimerAsync(int seconds, DiscordMessage message) /* Deletes the given discord message past the given seconds */ {
			await Task.Delay(1000 * seconds);
			await message.DeleteAsync();
			return ;
		}
		private static async Task					GTXEmbedTimerAsync(int seconds, DiscordEmbed embed, GjallarhornContext ctx) {
			if (ctx._Ictx != null) {
					await ctx._Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
					await Task.Delay(1000 * seconds);
					await ctx._Ictx.DeleteResponseAsync();
				} else if (ctx._chatChannel != null)
					await ChariotMusicCalls.DelMssTimerAsync(seconds, await ctx._chatChannel.SendMessageAsync(embed));
		}
	}
}
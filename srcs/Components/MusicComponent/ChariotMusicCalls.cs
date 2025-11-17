using ChariotSanzzo.Utils;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using STPlib;

namespace ChariotSanzzo.Components.MusicComponent {
	public static class ChariotMusicCalls {
	// -1. Extras
		public struct t_tools {
			public ulong					ServerId	{get; set;}
			public TrackQueue				Queue		{get; set;}
			public GjallarhornContext		Ctx			{get; set;}
			public LavalinkExtension		LlInstace	{get; set;}
			public LavalinkNodeConnection	Node		{get; set;}
			public LavalinkGuildConnection	Conn		{get; set;}
		}

	// M. Member Variables
		public static QueueCollection		QColle				{get; set;} = new QueueCollection();

	// 0. Main Call
		public static async Task<string>	TryCallAsync(GjallarhornContext ctx) {
			try {
				switch (ctx.Command) {
					case ("Message"):
						await ChariotMusicCalls.SendEmbedMessageAsync(ctx);
					break;
					case ("Play"):
						await ChariotMusicCalls.PlayAsync(ctx);
					break;
					case ("Loop"):
						await ChariotMusicCalls.LoopAsync(ctx);
					break;
					case ("Previous"):
						await ChariotMusicCalls.PreviousAsync(ctx);
					break;
					case ("Pause"):
						await ChariotMusicCalls.PauseAsync(ctx);
					break;
					case ("Next"):
						await ChariotMusicCalls.NextAsync(ctx);
					break;
					case ("Shuffle"):
						await ChariotMusicCalls.ShuffleAsync(ctx);
					break;
					case ("Replay"):
						await ChariotMusicCalls.ReplayAsync(ctx);
					break;
					case ("Reset"):
						await ChariotMusicCalls.ResetAsync(ctx);
					break;
					case ("Stop"):
						await ChariotMusicCalls.StopAsync(ctx);
					break;
					case ("ControlPanel"):
						await ChariotMusicCalls.ControlPanelAsync(ctx);
					break;
					default:
						Program.ColorWriteLine(ConsoleColor.Red, $"FunctionsSwitch: ChariotMusicCall Received was not valid. ({ctx.Command})");
					return ($"FunctionsSwitch: ChariotMusicCall Received was not valid. ({ctx.Command})");
				}
				return ($"ChariotMusicCall Received: {ctx.Command}.");
			} catch(Exception ex) {
				Program.WriteException(ex);
				return ("Caugh Exception...");
			}
		}
	
	// 1. Core Functions
		private static async Task	SendEmbedMessageAsync(GjallarhornContext ctx) {
			if (ctx.Guild == null
				|| ctx.ChatChannel == null)
				return ;
			// 0. Embed Construction
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(ctx.Color);
			embed.WithDescription(ctx.Message);
			await ctx.GTXEmbedTimerAsync(20, embed);
		}	
		private static async Task	PlayAsync(GjallarhornContext ctx) {
			if (ctx.Member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 0);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Purple};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Find Track
			LavalinkLoadResult	searchQuery;
			if (tools.Ctx.Data.Query.Contains("https://") == true || tools.Ctx.Data.Query.Contains("http://") == true)
				searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Plain);
			else
				switch ((int)ctx.Data.Plataform) {
					case (0): // Youtube
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Youtube);
					break;
					case (1): // Soundcloud
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.SoundCloud);
					break;
					default: // Plain
						searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Plain);
					break;
				}
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return ;
			}

		// 2. Search Tracks
			LavalinkTrack[]	musicTracks;
			if (ctx.Data.Query.Contains("youtube.com/playlist?") == true
				|| ctx.Data.Query.Contains("spotify.com/playlist") == true
				|| (ctx.Data.Query.Contains("spotify.com/") == true && ctx.Data.Query.Contains("/album/") == true)
				|| (ctx.Data.Query.Contains("soundcloud.com/") == true && ctx.Data.Query.Contains("/sets") == true)) {
				musicTracks = searchQuery.Tracks.ToArray();
				for (int i = 0; i < musicTracks.Length; i++)
					tools.Queue.AddTrackToQueue(new ChariotTrack(musicTracks[i], ctx.Member));
			} else {
				if (ctx.Data.Query.Contains("youtube.com/watch?") && ctx.Data.Query.Contains("&index="))
					musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.ElementAt(ctx.Data.Query.Substring(ctx.Data.Query.IndexOf("&index=") + 7).StoI() - 1)};
				else
					musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.First()};
				tools.Queue.AddTrackToQueue(new ChariotTrack(musicTracks[0], ctx.Member));
			}
		// 3. Playing It!
			if (musicTracks.Length > 1) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription($"A Playlist was added! {musicTracks.Length} new tracks!");
				if (tools.Conn.CurrentState.CurrentTrack != null)
					await tools.Queue.NowPlayingAsync();
			}
			if (ctx.Data.Priority == true/* && musicTracks.Length == 1*/) { // Priority Mode
				await tools.Queue.Conn.PlayAsync(await tools.Queue.UseTrackAsync(new ChariotTrack(musicTracks.First(), ctx.Member)));
				if (musicTracks.Length <= 1 && ctx.Ictx != null)
					await ctx.Ictx.DeleteResponseAsync();
				return ;
			}
			else if (tools.Conn.CurrentState.CurrentTrack == null) { // Play if there is nothing playing
				await tools.Queue.Conn.PlayAsync(await tools.Queue.UseNextTrackAsync());
				if (musicTracks.Length <= 1 && ctx.Ictx != null) {
					await ctx.Ictx.DeleteResponseAsync();
					return ;
				}
			}
			else if (musicTracks.Length == 1) {
				embed.WithDescription($"_**Added to Queue:**_ [{musicTracks[0].Title}]({musicTracks[0].Uri})\n" +
										$"**Author:** {musicTracks[0].Author}\n" +
										$"**Length:** {musicTracks[0].Length}" +
										$"\t\t**Index:** ` {tools.Queue.Tracks.Length} `" );
				embed.WithThumbnail(await ChariotTrack.GetArtworkAsync(musicTracks[0].Uri));
				if (tools.Conn.CurrentState.CurrentTrack != null)
					await tools.Queue.NowPlayingAsync();
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task	StopAsync(GjallarhornContext ctx) {
			if (ctx.Guild == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 1);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx.Username, ctx.UserIcon);
			embed.WithColor(DiscordColor.Black);
			embed.WithTitle("_**Music Stopped!**_");
			if (tools.Queue != null && tools.Queue.ActivePlayerMss != null)
				await tools.Queue.ActivePlayerMss.DeleteAsync();
			if (tools.Conn.CurrentState.CurrentTrack != null) {
				embed.WithDescription($"_**Stopped Track:**_ [{tools.Conn.CurrentState.CurrentTrack.Title}]({tools.Conn.CurrentState.CurrentTrack.Uri})");
				await tools.Conn.StopAsync();
			}
			await tools.Conn.DisconnectAsync();
			ChariotMusicCalls.QColle.DropQueue(tools.ServerId);
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task	PauseAsync(GjallarhornContext ctx) {
			if (ctx.Guild == null ||
				ctx.VoiceChannel == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 2);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
			if (tools.Conn.CurrentState.CurrentTrack == null)
				return ;
		// 0. Preparing Embed
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(DiscordColor.DarkGray);
			embed.WithDescription($"_**Current Track:**_ [{tools.Conn.CurrentState.CurrentTrack.Title}]({tools.Conn.CurrentState.CurrentTrack.Uri})");

		// 1. Starting
			if (tools.Queue.PauseState == true && ctx.Data.PauseType == 1) {
				if (ctx.Data.MiscValue == 1) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("Already Paused.");
					await ctx.GTXEmbedTimerAsync(20, embed);
				}
				return ;
			}
			if (ctx.Data.PauseType == 2)
				ctx.Data.PauseType = tools.Queue.SwitchPause();
			var	resumeButton = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicPlayPauseButton", "Resume Track", false, new DiscordComponentEmoji(1269696547046555688));
			switch (ctx.Data.PauseType) {
				case (1): // Pause
					await tools.Queue.Conn.PauseAsync();
					tools.Queue.SetPauseState(true);
					embed.WithTitle("_**Music Paused!**_");
					tools.Queue.SetPauseMessage(await ctx.GTXEmbedSendAsync(new DiscordMessageBuilder().WithEmbed(embed.Build()).AddComponents(resumeButton)));
				break;
				case (0): // Resume
					await tools.Queue.Conn.ResumeAsync();
					tools.Queue.SetPauseState(false);
					embed.WithTitle("_**Music Resumed!**_");
					if (tools.Queue.PauseMss != null)
						await tools.Queue.PauseMss.DeleteAsync();
					tools.Queue.SetPauseMessage(null);
					await ctx.GTXEmbedTimerAsync(10, embed);
				break;
			}
			if (tools.Queue.ActivePlayerMss != null)
				await tools.Queue.ActivePlayerMss.ModifyAsync((await tools.Queue.GenNowPlayingAsync()));
		}
		private static async Task	LoopAsync(GjallarhornContext ctx) {
			if (ctx.Guild == null)
				return ;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 3);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(DiscordColor.Azure);
		// 1. Core
			if (ctx.Data.LoopType == 3) {
				ctx.Data.LoopType = tools.Queue.Loop + 1;
				if (ctx.Data.LoopType > 2)
					ctx.Data.LoopType = 0;
			}
			if (tools.Conn.CurrentState.CurrentTrack == null && ctx.Data.LoopType != 0) {
				if (ctx.Data.LoopType == 1)
					ctx.Command = "Replay";
				else
					ctx.Command = "Next";
				await ctx.TryCallingAsync();
			}
			switch (ctx.Data.LoopType) {
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
			tools.Queue.SetLoop(ctx.Data.LoopType);
			await ctx.GTXEmbedTimerAsync(20, embed);
			if (tools.Queue.ActivePlayerMss != null)
				await tools.Queue.ActivePlayerMss.ModifyAsync((await tools.Queue.GenNowPlayingAsync()));
		}
		private static async Task	NextAsync(GjallarhornContext ctx) {
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 4);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			if (ctx.Data.MiscValue == 0)
				embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			if (ctx.Data.SkipCount > 1000 || ctx.Data.SkipCount < 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Invalid skip count value.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return ;
			}
			for (int i = 0; i < ctx.Data.SkipCount - 1; i++)
				tools.Queue.GoNextIndex();
			var	toPlayNow = await tools.Queue.UseNextTrackAsync();
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
			}
			else {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track Skipped.");
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task	PreviousAsync(GjallarhornContext ctx) {
			if (ctx.Member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 4);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			var	toPlayNow = await tools.Queue.UsePreviousTrackAsync();
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
			}
			else {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track set back.");
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task	ShuffleAsync(GjallarhornContext ctx) {
			if (ctx.Member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 5);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			if (await tools.Queue.ShuffleTracks()) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription("Shuffed Succesfully!");
			}
			else {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed Shuffling!");
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task	ReplayAsync(GjallarhornContext ctx) {
			if (ctx.Member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 0);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			if (ctx.Member != null)
				embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			var	toPlayNow = await tools.Queue.UseCurrentTrackAsync();
			if (toPlayNow != null) {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track replayed.");
			}
			else
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
			await ctx.GTXEmbedTimerAsync(10, embed);
		}
		private static async Task	ResetAsync(GjallarhornContext ctx) {
			if (ctx.Member == null)
				return;
			var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 6);
			if (obj.Item1 == false)
				return ;
			t_tools tools = obj.Item2;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			await tools.Conn.StopAsync();
			if (ctx.Guild != null)
				tools.Queue.GetQueueCollection().DropQueue(ctx.Guild.Id);
			embed.WithDescription("Guild Queue was reset.");
			await ctx.GTXEmbedTimerAsync(20, embed);
			if (tools.Queue.ActivePlayerMss != null)
				await tools.Queue.ActivePlayerMss.DeleteAsync();
		}
		private static async Task	ControlPanelAsync(GjallarhornContext ctx) {
			if (ctx.Ictx == null
				|| ctx.Member == null
				|| ctx.ChatChannel == null)
				return ;
			try {
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.DarkBlue);
				embed.WithTitle("Music ControlPanel Link");
				embed.WithDescription($"[Here is your link for this Channel's ControlPanel]({LinkData.GetGjallarhornControlFullAdress()}/ChariotSanzzo/control-panel?&userId={ctx.Member.Id}&channelId={ctx.ChatChannel.Id})");
				embed.WithFooter($"For: {ctx.Username}", ctx.UserIcon);
				await ctx.Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
				await Task.Delay(1000 * 15);
            } catch (Exception ex) {
                Program.WriteException(ex);
            }
			await ctx.Ictx.DeleteResponseAsync();
		}

	// E. Miscs
		private static async Task<(bool, t_tools)>	GetLavalinkTools(GjallarhornContext ctx, int type) {
		// 0. Starting
			t_tools	tools = new t_tools();
			if (ctx.Guild == null
				|| ctx.Member == null)
				return (false, tools);
			tools.Ctx = ctx;
			tools.LlInstace = Program.Client.GetLavalink();
			tools.ServerId = ctx.Guild.Id;
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Red};
		// 1. Primary Checks
			if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null) {	// Error: Enter a Voice Channel
				embed.WithDescription("Please, enter a Voice Channel!");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return (false, tools);
			}
			else if (!tools.LlInstace.ConnectedNodes.Any()) { 								// Error: Node connection not stablished
				embed.WithDescription("The connection is not stablished!");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return (false, tools);
			}
			else if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice) {	// Error: Enter a valid Voice Channel
				embed.WithDescription("Please, enter a valid Voice Channel!");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return (false, tools);
			}
			tools.Node = tools.LlInstace.ConnectedNodes.Values.First();
			if (type == 0)																	// Connect to VC if not in Stop Command
				await tools.Node.ConnectAsync(ctx.Member.VoiceState.Channel);
			tools.Conn = tools.Node.GetGuildConnection(ctx.Guild);
			if (tools.Conn == null) {														// Error: Conn Null
				embed.WithDescription("Chariot is not in a channel to perform such action!");
				await ctx.GTXEmbedTimerAsync(20, embed);
				return (false, tools);
			}
			if (type != 1)																	// Get Queue if not in Stop Command
				tools.Queue = ChariotMusicCalls.QColle.GetQueue(tools.ServerId, ctx.Member, tools.Conn, ctx.ChatChannel);
			else if (ChariotMusicCalls.QColle.QueueExist(tools.ServerId))
				tools.Queue = ChariotMusicCalls.QColle.GetQueue(tools.ServerId, ctx.Member, tools.Conn, ctx.ChatChannel);
			return (await ChariotMusicCalls.ExtraChecks(tools, ctx, type, embed), tools);
		}
		private static async Task<bool>				ExtraChecks(t_tools tools, GjallarhornContext ctx, int type, DiscordEmbedBuilder embed) {
			if (tools.Conn.CurrentState.CurrentTrack == null && ctx.Data.VipCall == false && type > 1) {
				switch (type) {
					case (1): // Stop
							embed.WithDescription("There's no music playing to be stopped!");
					break;
					case (2): // Pause
							embed.WithDescription("There's no music playing to be paused!");
					break;
					case (3): // Loop
							embed.WithDescription("There's no music playing to be looped!");
					break;
					case (4): // Change
							embed.WithDescription("There's no music playing to change tracks!");
					break;
					case (5): // Shuffle
							embed.WithDescription("There's no music playing to shuffle!");
					break;
					case (6): // Reset
							embed.WithDescription("There's no music playing to clear!");
					break;
				}
				await ctx.GTXEmbedTimerAsync(20, embed);
				return (false);
			}
			return (true);
		}
	}
}
using ChariotSanzzo.Utils;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using STPlib;

namespace ChariotSanzzo.Components.MusicComponent {
	public static class ChariotMusicCalls {
	// -1. Extras
		private static readonly string ChariotApiFullAddress	= Environment.GetEnvironmentVariable("CHARIOT_API_FULL_ADDRESS") ?? throw new InvalidOperationException("CHARIOT_API_FULL_ADDRESS not set");
		public struct Tools {
			public ulong										ServerId	{get; set;}
			public TrackQueue								Queue			{get; set;}
			public GjallarhornContext				Ctx				{get; set;}
			public LavalinkExtension				LlInstace	{get; set;}
			public LavalinkNodeConnection		Node			{get; set;}
			public LavalinkGuildConnection	Conn			{get; set;}
		}

	// M. Member Variables
		public static QueueCollection		QColle			{get; set;} = new QueueCollection();

	// 0. Main Call
		public static async Task<string>	TryCallAsync(GjallarhornContext ctx) {
			try {
				string successMessage = $"ChariotMusicCall Received: {ctx.Command}.";
				switch (ctx.Command) {
					case ("Message"):
						await ChariotMusicCalls.SendEmbedMessageAsync(ctx);
					return (successMessage);
					case ("ControlPanel"):
						await ChariotMusicCalls.ControlPanelAsync(ctx);
					return (successMessage);
				}

				var obj = await ChariotMusicCalls.GetLavalinkTools(ctx, 0);
				if (obj.Item1 == false)
					return (successMessage);
				Tools tools = obj.Item2;

				if (tools.Queue.Chat == null && ctx.ChatChannel != null)
					await tools.Queue.SetChatChannel(ctx.ChatChannel);

				ctx.Result.WasSuccess = ctx.Command switch {
					("Play") => await ChariotMusicCalls.PlayAsync(ctx, tools),
					("Loop") => await ChariotMusicCalls.LoopAsync(ctx, tools),
					("Previous") => await ChariotMusicCalls.PreviousAsync(ctx, tools),
					("Pause") => await ChariotMusicCalls.PauseAsync(ctx, tools),
					("Next") => await ChariotMusicCalls.NextAsync(ctx, tools),
					("Shuffle") => await ChariotMusicCalls.ShuffleAsync(ctx, tools),
					("Replay") => await ChariotMusicCalls.ReplayAsync(ctx, tools),
					("Seek") => await ChariotMusicCalls.SeekAsync(ctx, tools),
					("Reset") => await ChariotMusicCalls.ResetAsync(ctx, tools),
					("Stop") => await ChariotMusicCalls.StopAsync(ctx, tools),
					_ => throw new Exception("Invalid Command")
				};
				UpdateFinishedState(ctx, tools);
				await SendStationSocketUpdate(tools);
				await SendQueueSocketUpdate(tools);
				tools.Queue.LastGjallarhornContext = ctx;
				return (successMessage);
			} catch(Exception ex) {
				Program.WriteException(ex);
				if (ex.Message == "Invalid Command")
					return ($"FunctionsSwitch: ChariotMusicCall Received was not valid. ({ctx.Command})");
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
		private static async Task<bool>	PlayAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null) {
				return false;
			}
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Purple};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Find Track
			LavalinkLoadResult	searchQuery;
			if (tools.Ctx.Data.Query.Contains("https://") == true || tools.Ctx.Data.Query.Contains("http://") == true)
				searchQuery = await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Plain);
			else
				searchQuery = (int)ctx.Data.Plataform switch{
					// Youtube
					(0) => await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Youtube),
					// Soundcloud
					(1) => await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.SoundCloud),
					// Plain
					_ => await tools.Node.Rest.GetTracksAsync(tools.Ctx.Data.Query, LavalinkSearchType.Plain),
				};
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.ErrorMessage = "Failed to find proper music using the given query.";
				return false;
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
					musicTracks = [searchQuery.Tracks.ElementAt(ctx.Data.Query.Substring(ctx.Data.Query.IndexOf("&index=") + 7).StoI() - 1)];
				else
					musicTracks = [searchQuery.Tracks.First()];
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
				return true;
			}
			else if (tools.Conn.CurrentState.CurrentTrack == null) { // Play if there is nothing playing
				await tools.Queue.Conn.PlayAsync(await tools.Queue.UseNextTrackAsync());
				if (musicTracks.Length <= 1 && ctx.Ictx != null) {
					await ctx.Ictx.DeleteResponseAsync();
					return true;
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
			return true;
		}
		private static async Task<bool>	StopAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Guild == null)
				return false;
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
			return true;
		}
		private static async Task<bool>	PauseAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Guild == null
				|| tools.Conn.CurrentState.CurrentTrack == null) {
				if (ctx.Ictx != null) {
					var noTrackEmbed = new DiscordEmbedBuilder();
						noTrackEmbed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
						noTrackEmbed.WithColor(DiscordColor.DarkGray);
						noTrackEmbed.WithDescription($"No track playing to be paused.");
					await ctx.GTXEmbedTimerAsync(20, noTrackEmbed);
				}
				return false;
			}
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
				return true;
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
			return true;
		}
		private static async Task<bool>	LoopAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Guild == null)
				return false;
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
			return true;
		}
		private static async Task<bool>	NextAsync(GjallarhornContext ctx, Tools tools) {
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			if (ctx.Data.MiscValue == 0)
				embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			if (ctx.Data.SkipCount > 1000 || ctx.Data.SkipCount < 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Invalid skip count value.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.ErrorMessage = "Invalid skip count value.";
				return false;
			}
			for (int i = 0; i < ctx.Data.SkipCount - 1; i++)
				tools.Queue.GoNextIndex();

			var	toPlayNow = await tools.Queue.UseNextTrackAsync();
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
				ctx.Result.WasSuccess = false;
				ctx.Result.ErrorMessage = "Coundn't Skip (Probably no tracks left).";
			}
			else {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track Skipped.");
				ctx.Result.WasSuccess = true;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool>	PreviousAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null)
				return false;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			var	toPlayNow = await tools.Queue.UsePreviousTrackAsync();
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
				ctx.Result.WasSuccess = false;
				ctx.Result.ErrorMessage = "Coundn't Skip (Probably no tracks left).";
			}
			else {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track set back.");
				ctx.Result.WasSuccess = true;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool>	ShuffleAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null)
				return false;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

		// 1. Start
			if (await tools.Queue.ShuffleTracks()) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription("Shuffed Succesfully!");
				ctx.Result.WasSuccess = true;
			}
			else {
				embed.WithColor(DiscordColor.Red);
				ctx.Result.WasSuccess = false;
				embed.WithDescription("Failed Shuffling!");
				ctx.Result.ErrorMessage = "Shuffed Succesfully!";
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool>	ReplayAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null)
				return false;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			if (ctx.Member != null)
				embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			var	toPlayNow = await tools.Queue.UseCurrentTrackAsync();
			if (toPlayNow != null) {
				await tools.Queue.Conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track replayed.");
				ctx.Result.WasSuccess = true;
			}
			else {
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
				ctx.Result.WasSuccess = false;
				ctx.Result.ErrorMessage = "Coundn't replay (Probably no tracks left).";
			}
			await ctx.GTXEmbedTimerAsync(10, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool>	SeekAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null
					|| tools.Queue.Tracks.Length == 0
					|| ctx.Data.Position > tools.Queue.Tracks[tools.Queue.CurrentIndex].Length.TotalSeconds
					|| ctx.Data.Position < 0)
				return false;
		// 0. Start
			var embed = new DiscordEmbedBuilder() {Color = DiscordColor.Aquamarine};
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			var timespan = TimeSpan.FromSeconds(ctx.Data.Position);
			if (tools.Queue.IsFinished) {
			// if (true) {
				var	toPlayNow = await tools.Queue.UseCurrentTrackAsync();
				if (toPlayNow != null) {
					await tools.Queue.Conn.PlayAsync(toPlayNow);
					while (tools.Conn.CurrentState.PlaybackPosition.TotalSeconds > 1 || tools.Conn.CurrentState.PlaybackPosition.Milliseconds < 1) {
						await Task.Delay(50);
					}
					await tools.Conn.SeekAsync(timespan);
				}
				else {
					embed.WithDescription("Coundn't replay for seek (Probably no tracks left).");
					ctx.Result.WasSuccess = false;
					ctx.Result.ErrorMessage = "Coundn't replay (Probably no tracks left).";
					return false;
				}
			}
			else {
				if (tools.Queue.PauseState || tools.Conn.CurrentState.PlaybackPosition.TotalSeconds < tools.Conn.CurrentState.CurrentTrack.Length.TotalSeconds - 10) {
					await tools.Conn.SeekAsync(timespan);
				} else {
					var	toPlayNow = await tools.Queue.UseCurrentTrackAsync();
					if (toPlayNow != null) {
						await tools.Queue.Conn.PlayAsync(toPlayNow);
						while (tools.Conn.CurrentState.PlaybackPosition.TotalSeconds > 1 || tools.Conn.CurrentState.PlaybackPosition.Milliseconds < 1) {
							await Task.Delay(50);
						}
						await tools.Conn.SeekAsync(timespan);
					}
				}
			}
			embed.WithDescription($"Track seeked to {timespan.ToString(@"hh\:mm\:ss")}.");
			await ctx.GTXEmbedTimerAsync(10, embed);
			return true;
		}
		private static async Task<bool>	ResetAsync(GjallarhornContext ctx, Tools tools) {
			if (ctx.Member == null)
				return false;
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
			return true;
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
		private static async Task<(bool, Tools)>	GetLavalinkTools(GjallarhornContext ctx, int type) {
		// 0. Starting
			Tools	tools = new Tools();
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
		private static async Task<bool>	ExtraChecks(Tools tools, GjallarhornContext ctx, int type, DiscordEmbedBuilder embed) {
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
		private static void							UpdateFinishedState(GjallarhornContext ctx, Tools tools) {
			tools.Queue.IsFinished = ctx.Command switch {
				("Stop" or "Reset") => true,
				("Pause") => tools.Queue.IsFinished,
				("Next" or "Replay" or "Seek") when ctx.Result != null && ctx.Result.WasSuccess == false && ctx.Data.IsFromEvent => tools.Queue.IsFinished = true,
				_ when ctx.Result != null && ctx.Result.WasSuccess => false,
				_  => tools.Queue.IsFinished
			};
		}
		private static (ChariotTrack?, int)	GetCurrentTrackSafe(this TrackQueue queue, GjallarhornContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset")
				return (null, -1);
			return queue.Tracks.Length > 0
				? (queue.Tracks[queue.CurrentIndex], queue.CurrentIndex)
				: (null, -1);
		}
		private static (ChariotTrack?, int)	GetPreviousTrackSafe(this TrackQueue queue, GjallarhornContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset")
				return (null, -1);
			var index = queue.CurrentIndex - 1;
			if (index < 0) {
				if (queue.Loop == 2)
					index = queue.Length - 1;
				else {
					index = 0;
					return (null, 1);
				}
			}
			return (queue.Tracks[index], index);
		}
		private static (ChariotTrack?, int)	GetNextTrackSafe(this TrackQueue queue, GjallarhornContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset")
				return (null, -1);
			var index = queue.CurrentIndex;
			if (index >= queue.Length - 1) {
				if (queue.Loop == 2)
					index = -1;
				else
					return (null, -1);
			}
			index += 1;
			if (index < 0)
				index = 0;
			return (queue.Tracks[index], index);
		}
		private static double								GetTrackCurrentPosition(this ChariotTrack? track, Tools tools, GjallarhornCallResult result) {
			if (track == null)
				return 0;
			return tools.Ctx.Command switch {
				("Seek") => tools.Ctx.Data.Position,
				("Next") when result.WasSuccess == false => Math.Ceiling(tools.Conn.CurrentState?.PlaybackPosition.TotalSeconds ?? track.Length.TotalSeconds),
				("Pause") when result.WasSuccess && tools.Queue.PauseState == false && tools.Queue.LastGjallarhornContext != null && tools.Queue.LastGjallarhornContext.Command == "Seek" && tools.Queue.LastGjallarhornContext.Result.WasSuccess => tools.Queue.LastGjallarhornContext.Data.Position,
				("Play"
					or "Previous"
					or "Next"
					or "Shuffle"
					or "Replay"
					or "Reset"
					or "Stop"
				) => 0,
				_ => Math.Ceiling(tools.Conn.CurrentState?.PlaybackPosition.TotalSeconds ?? track.Length.TotalSeconds),
			};
		}

	// SocketUpdate
		public class TrackInfo(string	title, string	link, string	artwork, double durationInSeconds, int index, string	originalUser, string	originalUserAvatarUrl) {
			public string	Title											{get;set;} = title;
			public string	Link											{get;set;} = link;
			public string	Artwork										{get;set;} = artwork;
			public double	DurationInSeconds					{get;set;} = durationInSeconds;
			public int		Index											{get;set;} = index;
			public string	OriginalUser							{get;set;} = originalUser;
			public string	OriginalUserAvatarUrl			{get;set;} = originalUserAvatarUrl;
		}
		public class CurrentTrackInfo(string	title, string	link, string	artwork, double durationInSeconds, int index, string	originalUser, string	originalUserAvatarUrl, double currentPosition, long lastUpdate) : TrackInfo(title, link, artwork, durationInSeconds, index, originalUser, originalUserAvatarUrl){
			public double	CurrentPosition						{get;set;} = currentPosition;
			public long		LastUpdate								{get;set;} = lastUpdate;
		}
		public class PlayerLiveUpdateDto(string	guildId, string? voiceChannelId, string? chatChannelId, bool isPaused, int loopState, bool isFinished, int currentIndex, GjallarhornCallResult lastCommandResult, CurrentTrackInfo? currentTrackInfo, TrackInfo? previousTrackInfo, TrackInfo? nextTrackInfo) {
			public long										UnixTimestamp			{get; set;} = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			public string									GuildId						{get; set;} = guildId;
			public string?								VoiceChannelId		{get; set;} = voiceChannelId;
			public string?								ChatChannelId			{get; set;} = chatChannelId;
			public bool										IsPaused					{get; set;} = isPaused;
			public int										LoopState					{get; set;} = loopState;
			public bool										IsFinished				{get; set;} = isFinished;
			public int										CurrentIndex			{get; set;} = currentIndex;
			public GjallarhornCallResult	LastCommandResult	{get; set;} = lastCommandResult;
			public CurrentTrackInfo?			CurrentTrack			{get; set;} = currentTrackInfo;
			public TrackInfo?							PreviousTrack			{get; set;} = previousTrackInfo;
			public TrackInfo?							NextTrack					{get; set;} = nextTrackInfo;
		}
		public static async Task<TrackInfo>	ToTrackInfo(this ChariotTrack track, int index) {
			return new(
				track.Title,
				track.Uri.AbsoluteUri,
				await track.GetArtworkAsync(),
				Math.Floor(track.Length.TotalSeconds),
				index,
				track.User.Username,
				track.User.AvatarUrl
			);
		}
		public static async Task<TrackInfo?> ToNullableTrackInfo(this ChariotTrack? track, int index)
			=> track is null ? null : await track.ToTrackInfo(index);
		public static async Task<CurrentTrackInfo?>	ToNullableCurrentTrackInfo(this ChariotTrack? track, int index, double currentPosition, long lastUpdate) {
			if (track == null)
				return null;
			return new(
				track.Title,
				track.Uri.AbsoluteUri,
				await track.GetArtworkAsync(),
				Math.Floor(track.Length.TotalSeconds),
				index,
				track.User.Username,
				track.User.AvatarUrl,
				currentPosition,
				lastUpdate
			);
		}
		
		private static async Task	SendStationSocketUpdate(Tools tools) {
			var (currentTrack, currentTrackIndex) = tools.Queue.GetCurrentTrackSafe(tools.Ctx);
			var (previousTrack, previousTrackIndex) = tools.Queue.GetPreviousTrackSafe(tools.Ctx);
			var (nextTrack, nextTrackIndex) = tools.Queue.GetNextTrackSafe(tools.Ctx);
			var currentPosition = currentTrack.GetTrackCurrentPosition(tools, tools.Ctx.Result);

			await Program.HttpClient.PostAsJsonAsync<PlayerLiveUpdateDto>($"{ChariotApiFullAddress}/live/ChariotSanzzo/{tools.ServerId}/player-update-socket", new (
				tools.ServerId.ToString(),
				tools.Conn.Channel?.Id.ToString(),
				tools.Queue.Chat?.Id.ToString(),
				tools.Queue.PauseState,
				tools.Queue.Loop,
				tools.Queue.IsFinished,
				tools.Queue.CurrentIndex,
				tools.Ctx.Result,
				await currentTrack.ToNullableCurrentTrackInfo(
						currentTrackIndex,
						currentPosition,
						tools.Conn.CurrentState != null
							? tools.Conn.CurrentState.LastUpdate.ToUnixTimeSeconds()
							: DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
				await previousTrack.ToNullableTrackInfo(previousTrackIndex),
				await nextTrack.ToNullableTrackInfo(nextTrackIndex)
			));
		}
		
		public class QueueUpdateDto(string guildId, string guildName, string? voiceChannelId, bool isPaused, int loopState, bool isFinished, int currentIndex, TrackInfo[] tracks) {
			public long					UnixTimestamp		{get; set;} = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			public string				GuildId					{get; set;} = guildId;
			public string				GuildName				{get; set;} = guildName;
			public string?			VoiceChannelId	{get; set;} = voiceChannelId;
			public bool					IsPaused				{get; set;} = isPaused;
			public int					LoopState				{get; set;} = loopState;
			public bool					IsFinished			{get; set;} = isFinished;
			public int					CurrentIndex		{get;set;} = currentIndex;
			public TrackInfo[]	Tracks					{get;set;} = tracks;
		}
		private static async Task	SendQueueSocketUpdate(Tools tools) {
			if (!(tools.Ctx.Command == "Play"
				|| tools.Ctx.Command == "Previous"
				|| tools.Ctx.Command == "Next"
				|| tools.Ctx.Command == "Shuffle"
				|| tools.Ctx.Command == "Pause"
				|| tools.Ctx.Command == "Loop"
				|| tools.Ctx.Command == "Reset"
				|| tools.Ctx.Command == "Stop")
			)
				return;
			var tracks = tools.Ctx.Command switch {
				("Stop" or "Reset") when tools.Ctx.Result.WasSuccess  => [],
				_ => await Task.WhenAll(
					tools.Queue.Tracks.Select((track, index) => track.ToTrackInfo(index))
				)
			};
			var response = await Program.HttpClient.PostAsJsonAsync<QueueUpdateDto>($"{ChariotApiFullAddress}/live/ChariotSanzzo/{tools.ServerId}/queue-update-socket", new (
				tools.ServerId.ToString(),
				tools.Conn.Guild.Name,
				tools.Conn.Channel?.Id.ToString(),
				tools.Queue.PauseState,
				tools.Queue.Loop,
				tools.Queue.IsFinished,
				tools.Queue.CurrentIndex,
				tracks ?? []
			));
		}
	}
}
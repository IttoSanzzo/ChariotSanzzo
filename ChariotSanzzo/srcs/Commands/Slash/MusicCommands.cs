using ChariotSanzzo.Components.MusicQueue;
using ChariotSanzzo.Database;
using ChariotSanzzo.Events;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using STPlib;

namespace ChariotSanzzo.Commands.Slash {
	// -1. Struct
	public struct t_tools {
		public ulong						serverId {get; set;}
		public TrackQueue				queue {get; set;}
		public LavalinkExtension		llInstace {get; set;}
		public LavalinkNodeConnection	node {get; set;}
		public LavalinkGuildConnection	conn {get; set;}
	}

	[SlashCommandGroup("Music", "General Music Slash Commands.")]
	public class MusicCommands : ApplicationCommandModule {
	// -1. Member Variables
		public static QueueCollection			QColle		{get; set;} = new QueueCollection();
		private static LavalinkExtension		_llInstance	{get; set;} = Program.Client.GetLavalink();
		private static LavalinkNodeConnection	_node		{get; set;} = _llInstance.ConnectedNodes.Values.First();

	// 0. Constructor
	static MusicCommands() {
		MusicCommands._node.GuildConnectionCreated += Music.NewConn;
		// MusicCommands._node.GuildConnectionRemoved += Music.Disconnected;
		if (Program.Client != null)
			Program.Client.VoiceStateUpdated += Music.Disconnected;
	}

	// 1. Main
		[SlashCommand("play", "Enters the voice channel and starts to play a song!")]
		public async Task Play(InteractionContext ctx, [Option("SearchQuery", "Name or link of the desired music.")] string query, [Choice("Youtube", 0)][Choice("Soundcloud", 1)][Choice("Plain", 2)][Option("PLataform", "Which plataform should be used as search engine. (Defaults to Youtube)")] long plataform = 0) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1 Embed Initialization
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(DiscordColor.Purple);

			// 2. Core
			LavalinkLoadResult	searchQuery;
			if (query.Contains("https://") == true || query.Contains("http://") == true)
				searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.Plain);
			else
				switch ((int)plataform) {
					case (0): // Youtube
						searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.Youtube);
					break;
					case (1): // Soundcloud
						searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.SoundCloud);
					break;
					default: // Plain
						searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.Plain);
					break;
				}
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("Failed to find proper music using the given query.");
					await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
					await Task.Delay(1000 * 20);
					await ctx.DeleteResponseAsync();
					return ;
				}

			// 3. Playing Track
			LavalinkTrack[]	musicTracks;
		// Search Playlists
			if (query.Contains("youtube.com/playlist?") == true
				|| query.Contains("spotify.com/playlist") == true
				|| (query.Contains("soundcloud.com/") == true && query.Contains("/sets") == true)) {
				musicTracks = searchQuery.Tracks.ToArray();
				for (int i = 0; i < musicTracks.Length; i++)
					tools.queue.AddTrackToQueue(new ChariotTrack(musicTracks[i], ctx.User));
			}
		// Search Single Tracks
			else {
				/*
				Program.WriteLine($"LONERS: {query}");
				Uri? uri = null;
				if (query.Contains("https://") == true || query.Contains("http://") == true)
					uri = new Uri(query);
				if (uri != null && uri.AbsoluteUri.Contains("youtube.com/watch?v=") && uri.Query.Contains("&index=") == true) {
					Program.WriteLine($"LONERS 1: {uri.AbsoluteUri}");
					// int index = uri.Query.Substring(uri.Query.IndexOf("&index=") + 7).StoI();
					// musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.ElementAt(index - 1)};
				}
				else
				*/
				musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.First()};
				tools.queue.AddTrackToQueue(new ChariotTrack(musicTracks[0], ctx.User));
			}

			// 4. Core
			if (musicTracks.Length > 1) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription($"A Playlist was added! {musicTracks.Length} new tracks!");
			}
			if (tools.conn.CurrentState.CurrentTrack == null) {
				await tools.queue._conn.PlayAsync(await tools.queue.UseNextTrackAsync());
				if (!(musicTracks.Length > 1))
					await ctx.DeleteResponseAsync();
			}
			else if (musicTracks.Length == 1) {
				embed.WithDescription($"_**Added to Queue:**_ [{musicTracks[0].Title}]({musicTracks[0].Uri})\n" +
										$"**Author:** {musicTracks[0].Author}\n" +
										$"**Length:** {musicTracks[0].Length}" +
										$"\t\t**Index:** ` {tools.queue._tracks.Length} `" );
				embed.WithThumbnail(await ChariotTrack.GetArtworkAsync(musicTracks[0].Uri));
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 60);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("stop", "Stops the music and exits from the Voice Channel.")]
		public async Task Stop(InteractionContext ctx) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 1);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Embed Creation
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Black,
				Title = "_**Music Stopped!**_",
				Description = $"_**Stopped Track:**_ [{tools.conn.CurrentState.CurrentTrack.Title}]({tools.conn.CurrentState.CurrentTrack.Uri})"
			};

			// 2. Core
			await tools.conn.StopAsync();
			await tools.conn.DisconnectAsync();
			MusicCommands.QColle.DropQueue(tools.serverId);

			// 3. Respond
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 60);
			await ctx.DeleteResponseAsync();
		}

	// 2. Miscs
		[SlashCommand("pause", "Switches the track's pause state.")]
		public async Task Pause(InteractionContext ctx, [Choice("Switch", 2)][Choice("Pause", 1)][Choice("Resume", 0)][Option("Action", "What should happen. (Defaults to Switch)")] long type = 2) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 2);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Embed Preparation
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.DarkGray,
				Description = $"_**Current Track:**_ [{tools.conn.CurrentState.CurrentTrack.Title}]({tools.conn.CurrentState.CurrentTrack.Uri})"
			};
			// 2. Checks
			if (tools.queue._pauseState == true && type == 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Already Paused.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
				await Task.Delay(1000 * 10);
				await ctx.DeleteResponseAsync();
				return ;
			}
			if (type == 2)
				type = tools.queue.SwitchPause();
			// 3. Core
			var	resumeButton = new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "MusicPlayPauseButton", "Resume Track", false, new DiscordComponentEmoji(1269696547046555688));
			switch ((int)type) {
				case (1): // Pause
					await tools.queue._conn.PauseAsync();
					embed.WithTitle("_**Music Paused!**_");
					tools.queue.SetPauseState(true);
					tools.queue.SetPauseMessage(await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed).AddComponents(resumeButton)));
				break;
				case (0): // Resume
					await tools.queue._conn.ResumeAsync();
					embed.WithTitle("_**Music Resumed!**_");
					tools.queue.SetPauseState(false);
					if (tools.queue._pauseMss != null)
						await tools.queue._pauseMss.DeleteAsync();
					tools.queue.SetPauseMessage(null);
					await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
					await Task.Delay(1000 * 10);
					await ctx.DeleteResponseAsync();
				break;
			}
		}
		[SlashCommand("volume", "Tweakes the volume of the music.")]
		public async Task Volume(InteractionContext ctx, [Option("Value", "Changes the playback volume to the especified. (Default = 100)")] double volume = 100) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 3);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Embed Set
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple
			};

			// 1. Core
			if (volume < 0 || volume > 100)
				embed.WithDescription($"_**Volume:**_ {volume} is a invalid value, please use one between 0 and 100.");
			else {
				await tools.queue._conn.SetVolumeAsync((int)volume);
				embed.WithDescription($"_**Volume:**_ Set to {volume}.");
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 20);
			await ctx.DeleteResponseAsync();
		}
	
	// 3. Queue
		[SlashCommand("queue", "Shows the local queue.")]
		public async Task GetQueue(InteractionContext ctx) {
			await ctx.DeferAsync();
			await ctx.DeleteResponseAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			DiscordEmbed[] embeds = tools.queue.GetQueueEmbed();
			DiscordMessage[] excMss = new DiscordMessage[embeds.Length];
			for (int i = 0; i < embeds.Length; i++)
				excMss[i] = await ctx.Channel.SendMessageAsync(embeds[i]);
			await Task.Delay(1000 * 60 * 2);
			for (int i = 0; i < excMss.Length; i++)
				await excMss[i].DeleteAsync();
		}
		[SlashCommand("loop", "Changes the loop setting! (Defaults to Loop Track)")]
		public async Task Loop(InteractionContext ctx, [Choice("none", 0)][Choice("track", 1)][Choice("queue", 2)][Option("Type", "What should be looped.")] long type = 1) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Core
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Azure
			};
			switch ((int)type) {
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
			tools.queue.SetLoop((int)type);
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("skip", "Skips the currently playing track!")]
		public async Task Skip(InteractionContext ctx, [Option("count", "How many tracks should bem skipped. (Defatults to 1)")] long count = 1) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};
			if (count > 1000 || count < 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Invalid skip count value.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
				return ;
			}

		// 1. Core
			for (int i = 0; i < count - 1; i++)
				tools.queue.GoNextIndex();
			var	toPlayNow = await tools.queue.UseNextTrackAsync();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track Skipped.");
			}
			else
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("previous", "Goes back to the previous track!")]
		public async Task Previous(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};

		// 1. Core
			var	toPlayNow = await tools.queue.UsePreviousTrackAsync();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track set back.");
			}
			else
				embed.WithDescription("Coundn't go back (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("replay", "Replays the current track!")]
		public async Task Replay(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};

		// 1. Core
			var	toPlayNow = await tools.queue.UseCurrentTrackAsync();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track replayed.");
			}
			else
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("index", "Plays the track at the given index position!")]
		public async Task Index(InteractionContext ctx, [Option("index", "The track's position in the queue.")] long index) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;
			index -= 1;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};
			if (index < 0 || index > tools.queue._tracks.Length - 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Selected track does no exist!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
				return ;
			}
		// 1. Core
			var	toPlayNow = await tools.queue.UseIndexTrackAsync((int)index);
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				await ctx.DeleteResponseAsync();
				return ;
				// embed.WithDescription("Track replayed.");
			}
			else
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("shuffle", "Shuffles the queue.")]
		public async Task Shuffle(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Core
			var	embed = new DiscordEmbedBuilder();
			if (await tools.queue.ShuffleTracks()) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription("Shuffed Succesfully!");
			}
			else {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed Shuffling!");
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("remove", "Removes a track from the queue.")]
		public async Task Remove(InteractionContext ctx, [Option("index", "Index from the music to be removed.")] long index) {
				await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await MusicCommands.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Core
			var embed = new DiscordEmbedBuilder();
			if (index < 1 || index > tools.queue._tracks.Length) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There is no track with such index.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 10);
				await ctx.DeleteResponseAsync();
				return ;
			}
			embed.WithColor(DiscordColor.Black);
			embed.WithDescription($"[{tools.queue._tracks[index - 1]._title}]({tools.queue._tracks[index - 1]._uri}) was removed from queue.");
			tools.queue.RemoveTrackFromQueue((int)(index - 1));
			if (tools.queue._currentIndex == index - 1) {
				var	toPlayNow = await tools.queue.UseIndexTrackAsync(tools.queue._currentIndex);
				if (toPlayNow != null)
					await tools.queue._conn.PlayAsync(toPlayNow);
				else
					await tools.queue._conn.StopAsync();
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}

	// 5. Checks
		public static async Task<(bool, t_tools)>	PreChecksPass(InteractionContext ctx, short type) {
			t_tools	tools = new t_tools();
			tools.llInstace = ctx.Client.GetLavalink();
			tools.serverId = ctx.Guild.Id;
			Program.WriteLine($"[ServerId..: {tools.serverId}]");
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(DiscordColor.Red);

			// 1. Primary Checks
			if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null) {
				embed.WithDescription("Please enter a Voice Channel!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 20);
				await ctx.DeleteResponseAsync();
				return (false, tools);
			}
			else if (!tools.llInstace.ConnectedNodes.Any()) {
				embed.WithDescription("The connection is not stablished!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 20);
				await ctx.DeleteResponseAsync();
				return (false, tools);
			}
			else if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice) {
				embed.WithDescription("Please enter a valid Voice Channel!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 20);
				await ctx.DeleteResponseAsync();
				return (false, tools);
			}
			tools.node = tools.llInstace.ConnectedNodes.Values.First();
			if (type == 0)
				await tools.node.ConnectAsync(ctx.Member.VoiceState.Channel);
			tools.conn = tools.node.GetGuildConnection(ctx.Member.VoiceState.Guild);
			if (tools.conn == null) {
				embed.WithDescription("Chariot is not in a channel to perform such action!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 20);
				await ctx.DeleteResponseAsync();
				return (false, tools);
			}
			if (type != 1)
				tools.queue = MusicCommands.QColle.GetQueue(tools.serverId, tools.conn, ctx.Channel);

			// 2. Checks per type
			switch (type) {
				case (1): // Stop
					if (tools.conn.CurrentState.CurrentTrack == null) {
						embed.WithDescription("There's no music playing to be stopped!");
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
						await Task.Delay(1000 * 20);
						await ctx.DeleteResponseAsync();
						return (false, tools);
					}
				break;
				case (2): // Pause
					if (tools.conn.CurrentState.CurrentTrack == null) {
						embed.WithDescription("There's no music playing to be paused!");
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
						await Task.Delay(1000 * 20);
						await ctx.DeleteResponseAsync();
						return (false, tools);
					}
				break;
				case (3): // Resume
					if (tools.conn.CurrentState.CurrentTrack == null) {
						embed.WithDescription("There's no music playing to be resumed!");
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
						await Task.Delay(1000 * 20);
						await ctx.DeleteResponseAsync();
						return (false, tools);
					}
				break;
			}
			return (true, tools);
		}
	}
}

using ChariotSanzzo.Components.MusicQueue;
using ChariotSanzzo.Events;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	// -1. Struct
	public struct t_tools {
		public long						serverId {get; set;}
		public TrackQueue				queue {get; set;}
		public LavalinkExtension		llInstace {get; set;}
		public LavalinkNodeConnection	node {get; set;}
		public LavalinkGuildConnection	conn {get; set;}
	}

	[SlashCommandGroup("Music", "General Music Slash Commands.")]
	public class MusicCommands : ApplicationCommandModule {
	// 0. Member Variables
		public static QueueCollection	QColle {get; set;} = new QueueCollection();

	// 1. Main
		[SlashCommand("play", "Enters the voice channel and starts to play a song!")]
		public async Task Play(InteractionContext ctx, [Option("SearchQuery", "Name or link of the desired music.")] string query) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Core
			LavalinkLoadResult	searchQuery;
			if (query.Contains("https://") == true)
				searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.Plain);
			else
				searchQuery = await tools.node.Rest.GetTracksAsync(query, LavalinkSearchType.Youtube);
			if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Failed to find proper music using the given query."));
				return ;
			}

			// 2. Embed Initialization
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Title = $"At {ctx.Member.VoiceState.Channel.Name}"
			};

			// 3. Playing Track
			LavalinkTrack[]	musicTracks;
			if (query.Contains("youtube.com/playlist?") == true) {
				musicTracks = searchQuery.Tracks.ToArray();
				for (int i = 0; i < musicTracks.Length; i++)
					tools.queue.AddTrackToQueue(musicTracks[i]);
			}
			else {
				musicTracks = new LavalinkTrack[1] {searchQuery.Tracks.First()};
				tools.queue.AddTrackToQueue(musicTracks[0]);
			}
			if (musicTracks.Length > 1) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription("A Playlist was added!");
			}

			// 4. Cores
			if (tools.conn.CurrentState.CurrentTrack == null) {
				var	toPlayNow = tools.queue.UseNextTrack();
				await tools.queue._conn.PlayAsync(toPlayNow);
			}
			else if (musicTracks.Length == 1) {
				embed.WithDescription($"_**Added to queue:**_ {musicTracks[0].Title}\n" +
										$"_**Length:**_ {musicTracks[0].Length}\n" +
										$"_**Author:**_ {musicTracks[0].Author}\n" +
										$"_**URL:**_ {musicTracks[0].Uri}");
				if (musicTracks[0].Uri.ToString().Contains("youtube.com") == true)
					embed.WithImageUrl($"https://img.youtube.com/vi/{musicTracks[0].Uri.ToString().Substring(32)}/maxresdefault.jpg");
			}
			// 3. Embed Return
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("stop", "Stops the music and exits from the Voice Channel.")]
		public async Task Stop(InteractionContext ctx) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 1);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Embed Creation
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Title = "_**Music Stopped!**_",
				Description = $"_**Stopped Track:**_ {tools.conn.CurrentState.CurrentTrack.Title}"
			};

			// 2. Core
			await tools.conn.StopAsync();
			await tools.conn.DisconnectAsync();
			MusicCommands.QColle.DropQueue(tools.serverId);

			// 3. Respond
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}

	// 2. Miscs
		[SlashCommand("pause", "Pauses the music.")]
		public async Task Pause(InteractionContext ctx) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 2);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Core
			await tools.queue._conn.PauseAsync();

			// 2. Embed Return
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Title = "_**Music Paused!**_",
				Description = $"_**Current Track:**_ {tools.conn.CurrentState.CurrentTrack.Title}"
			};
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("resume", "Resumes the paused music.")]
		public async Task Resume(InteractionContext ctx) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 3);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

			// 1. Core
			await tools.queue._conn.ResumeAsync();

			// 2. Embed Return
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Title = "_**Music Resumed!**_",
				Description = $"_**Current Track:**_ {tools.conn.CurrentState.CurrentTrack.Title}"
			};
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("volume", "Tweakes the volume of the music.")]
		public async Task Volume(InteractionContext ctx, [Option("Value", "Changes the playback volume to the especified. (Default = 100)")] double volume = 100) {
			await ctx.DeferAsync();
			// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 3);
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
		}
	
	// 3. Queue
	[SlashCommand("loop", "Changes the loop setting! (Defaults to Loop Track)")]
		public async Task Loop(InteractionContext ctx, [Choice("none", 0)][Choice("track", 1)][Choice("queue", 2)][Option("Type", "What should be looped.")] long type = 1) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 0);
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
		}
		[SlashCommand("skip", "Skips the currently playing track!")]
		public async Task Skip(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};

		// 1. Core
			var	toPlayNow = tools.queue.UseNextTrack();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track Skipped.");
			}
			else
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("previous", "Goes back to the previous track!")]
		public async Task Previous(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};

		// 1. Core
			var	toPlayNow = tools.queue.UsePreviousTrack();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track set back.");
			}
			else
				embed.WithDescription("Coundn't go back (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("replay", "Replays the current track!")]
		public async Task Replay(InteractionContext ctx) {
			await ctx.DeferAsync();
		// 0. Initialization
			var testObj = await this.PreChecksPass(ctx, 0);
			if (testObj.Item1 == false)
				return ;
			t_tools	tools = testObj.Item2;

		// 1. Prepare Embed
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Aquamarine
			};

		// 1. Core
			var	toPlayNow = tools.queue.UseCurrentTrack();
			if (toPlayNow != null) {
				await tools.queue._conn.PlayAsync(toPlayNow);
				embed.WithDescription("Track replayed.");
			}
			else
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}

	// 4. Checks
		public async Task<(bool, t_tools)>	PreChecksPass(InteractionContext ctx, short type) {
			t_tools	tools = new t_tools();
			tools.llInstace = ctx.Client.GetLavalink();
			tools.serverId = (long)ctx.Guild.Id;
			Console.WriteLine($"[ServerId..: {tools.serverId}]");

			// 1. Primary Checks
			if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please enter a Voice Channel!"));
				return (false, tools);
			}
			else if (!tools.llInstace.ConnectedNodes.Any()) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The connection is not stablished!"));
				return (false, tools);
			}
			else if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please enter a valid Voice Channel!"));
				return (false, tools);
			}
			tools.node = tools.llInstace.ConnectedNodes.Values.First();
			if (type == 0)
				await tools.node.ConnectAsync(ctx.Member.VoiceState.Channel);
			tools.conn = tools.node.GetGuildConnection(ctx.Member.VoiceState.Guild);
			if (tools.conn == null) {
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Chariot is not in a channel to perform such action!"));
				return (false, tools);
			}
			if (type != 1)
				tools.queue = MusicCommands.QColle.GetQueue(tools.serverId, tools.conn, ctx.Channel);

			// 2. Checks per type
			switch (type) {
				case (1): // Stop
					if (tools.conn.CurrentState.CurrentTrack == null) {
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There's no music playing to be stopped!"));
						return (false, tools);
					}
				break;
				case (2): // Pause
					if (tools.conn.CurrentState.CurrentTrack == null) {
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There's no music playing to be paused!"));
						return (false, tools);
					}
				break;
				case (3): // Resume
					if (tools.conn.CurrentState.CurrentTrack == null) {
						await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There's no music playing to be resumed!"));
						return (false, tools);
					}
				break;
			}
			return (true, tools);
		}
	}
}

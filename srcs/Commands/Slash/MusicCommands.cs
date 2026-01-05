using ChariotSanzzo.Components.MusicComponent;
using ChariotSanzzo.Events;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
// 0. Struct
	public struct t_tools {
		public ulong					serverId {get; set;}
		public TrackQueue				queue {get; set;}
		public LavalinkExtension		llInstace {get; set;}
		public LavalinkNodeConnection	node {get; set;}
		public LavalinkGuildConnection	conn {get; set;}
	}

	[SlashCommandGroup("Music", "General Music Slash Commands.")]
	public class MusicCommands : ApplicationCommandModule {
	// M. Member Variables
		private static LavalinkExtension		LlInstance	{get; set;} = Program.Client.GetLavalink();
		private static LavalinkNodeConnection	Node		{get; set;} = LlInstance.ConnectedNodes.Values.First();

	// C. Constructor
	static MusicCommands() {
		MusicCommands.Node.GuildConnectionCreated += CharitoMusicEvents.NewConn;
	}

	// 0. Main
		[SlashCommand("play", "Enters the voice channel and starts to play a song!")]
		public async Task Play(InteractionContext ctx, [Option("SearchQuery", "Name or link of the desired music.")] string query, [Choice("True", 1)][Choice("False", 0)][Option("Priority", "If the track should be played immediately. (Defaults to false)")] long priority = 0, [Choice("Youtube", 0)][Choice("Soundcloud", 1)][Choice("Plain", 2)][Option("Plataform", "Which plataform should be used as search engine. (Defaults to Youtube)")] long plataform = 0) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Play", null, null, query);
			gCtx.Data.Plataform = (int)plataform;
			if (priority == 1)
				gCtx.Data.Priority = true;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("stop", "Stops the music and exits from the Voice Channel.")]
		public async Task Stop(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Stop");
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("pause", "Switches the track's pause state.")]
		public async Task Pause(InteractionContext ctx, [Choice("Switch", 2)][Choice("Pause", 1)][Choice("Resume", 0)][Option("Action", "What should happen. (Defaults to Switch)")] long type = 2) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Pause");
			gCtx.Data.PauseType = (int)type;
			gCtx.Data.MiscValue = 1;
			await ChariotMusicCalls.TryCallAsync(gCtx);
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
				await tools.queue.Conn.SetVolumeAsync((int)volume);
				embed.WithDescription($"_**Volume:**_ Set to {volume}.");
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
			await Task.Delay(1000 * 20);
			await ctx.DeleteResponseAsync();
		}
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
			var	gCtx = new GjallarhornContext(ctx, "Loop");
			gCtx.Data.LoopType = (int)type;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("skip", "Skips the currently playing track!")]
		public async Task Skip(InteractionContext ctx, [Option("count", "How many tracks should bem skipped. (Defaults to 1)")] long count = 1) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Next");
			gCtx.Data.SkipCount = (int)count;
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("previous", "Goes back to the previous track!")]
		public async Task Previous(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Previous");
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("replay", "Replays the current track!")]
		public async Task Replay(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Replay");
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
		[SlashCommand("seek", "Seeks the music to the given second.")]
		public async Task Seek(InteractionContext ctx, [Option("second", "Second to seek to (Defaults to 0)")] long second = 0) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Seek");
			gCtx.Data.Position = second;
			await ChariotMusicCalls.TryCallAsync(gCtx);
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
			if (index < 0 || index > tools.queue.Tracks.Length - 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Selected track does no exist!");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
				return ;
			}
		// 1. Core
			var	toPlayNow = await tools.queue.UseIndexTrackAsync((int)index);
			if (toPlayNow != null) {
				await tools.queue.Conn.PlayAsync(toPlayNow);
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
			var	gCtx = new GjallarhornContext(ctx, "Shuffle");
			await ChariotMusicCalls.TryCallAsync(gCtx);
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
			if (index < 1 || index > tools.queue.Tracks.Length) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There is no track with such index.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 10);
				await ctx.DeleteResponseAsync();
				return ;
			}
			embed.WithColor(DiscordColor.Black);
			embed.WithDescription($"[{tools.queue.Tracks[index - 1].Title}]({tools.queue.Tracks[index - 1].Uri}) was removed from queue.");
			tools.queue.RemoveTrackFromQueue((int)(index - 1));
			if (tools.queue.CurrentIndex == index - 1) {
				var	toPlayNow = await tools.queue.UseIndexTrackAsync(tools.queue.CurrentIndex);
				if (toPlayNow != null)
					await tools.queue.Conn.PlayAsync(toPlayNow);
				else
					await tools.queue.Conn.StopAsync();
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[SlashCommand("reset", "Resets the guild queue.")]
		public async Task Reset(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "Reset");
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}

	// 1. ControlPanel
		[SlashCommand("ControlPanel", "Returns your Music ControlPanel link for this Channel.")]
		public async Task ControlPanel(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	gCtx = new GjallarhornContext(ctx, "ControlPanel");
			await ChariotMusicCalls.TryCallAsync(gCtx);
		}
	// 3. Checks
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
				tools.queue = ChariotMusicCalls.QColle.GetQueue(tools.serverId, ctx.Member, tools.conn, ctx.Channel);

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

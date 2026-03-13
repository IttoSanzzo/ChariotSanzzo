using System.ComponentModel;
using System.Text.RegularExpressions;
using ChariotSanzzo.Components.Gjallar;
using ChariotSanzzo.Components.Gjallar.Types;
using ChariotSanzzo.Services.Commands.ChoiceProviders;
using ChariotSanzzo.Utils;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands {
	[Command("Music"), Description("General Music Slash Commands.")]
	public partial class MusicCommands {
		[GeneratedRegex(@"^(?=.{1,10}$)\d+(?::\d+){0,2}$")]
		private static partial Regex SeekTimestampRegex();

		static MusicCommands() {
			// MusicCommands.Node.GuildConnectionCreated += CharitoMusicEvents.NewConn;
		}

		[Command("play"), Description("Enters the voice channel and starts to play a song!")]
		[RequireGuild]
		public async Task Play(CommandContext ctx, [Description("Name or link of the desired music.")] string searchQuery, [SlashChoiceProvider<TrueOrFalseChoiceProvider>][Description("If the track should be played immediately. (Defaults to false)")] int immediate = 0, [SlashChoiceProvider<AvailableSearchPlataformsProvider>][Description("Which plataform should be used as search engine. (Defaults to Youtube)")] int plataform = 1) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Play", null, null, searchQuery);
			gCtx.Data.Plataform = TrackSearchModeUtils.IntToTrackSearchMode(plataform);
			if (immediate == 1)
				gCtx.Data.Priority = true;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("stop"), Description("Stops the music and exits from the Voice Channel.")]
		[RequireGuild]
		public async Task Stop(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Stop");
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("pause"), Description("Switches the track's pause state.")]
		[RequireGuild]
		public async Task Pause(CommandContext ctx, [SlashChoiceProvider<PauseStateChoiceProvider>][Description("What should happen. (Defaults to Switch)")] int action = 2) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Pause");
			gCtx.Data.PauseType = (GjallarPauseState)action;
			gCtx.Data.MiscValue = 1;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("volume"), Description("Tweakes the volume of the music.")]
		[RequireGuild]
		public async Task Volume(CommandContext ctx, [Description("Changes the playback volume to the especified. (Default = 100)")] double value = 100) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Volume") {
				Data = { MiscDoubleValue = value }
			};
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("queue"), Description("Shows the local queue.")]
		[RequireGuild]
		public async Task GetQueue(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			await ctx.DeleteResponseAsync();
			try {
				var gCtx = new GjallarContext(ctx, "Queue");
				GjallarCallTools tools = new();
				await tools.InitializeAsync(gCtx);

				DiscordEmbed[] embeds = tools.Player.GetQueueEmbed();
				DiscordMessage[] excMss = new DiscordMessage[embeds.Length];
				for (int i = 0; i < embeds.Length; i++)
					excMss[i] = await ctx.Channel.SendMessageAsync(embeds[i]);
				await Task.Delay(1000 * 60 * 2);
				for (int i = 0; i < excMss.Length; i++)
					await excMss[i].DeleteAsync();
			} catch { }
		}
		[Command("loop"), Description("Changes the loop setting! (Defaults to Loop Track)")]
		[RequireGuild]
		public async Task Loop(CommandContext ctx, [SlashChoiceProvider<LoopStateChoiceProvider>][Description("What should be looped.")] int type = 1) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Loop");
			gCtx.Data.LoopType = (GjallarLoopState)type;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("skip"), Description("Skips the currently playing track!")]
		[RequireGuild]
		public async Task Skip(CommandContext ctx, [Description("How many tracks should bem skipped. (Defaults to 1)")] long count = 1) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Next");
			gCtx.Data.SkipCount = (int)count;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("previous"), Description("Goes back to the previous track!")]
		[RequireGuild]
		public async Task Previous(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Previous");
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("replay"), Description("Replays the current track!")]
		[RequireGuild]
		public async Task Replay(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Replay");
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("seek"), Description("Seeks the music to the given second.")]
		[RequireGuild]
		public async Task Seek(CommandContext ctx, [Description("Timestamp or second to seek to (Defaults to 0)")][MinMaxLength(1, 10)] string timestamp = "0") {
			await ctx.DeferResponseAsync();
			if (SeekTimestampRegex().IsMatch(timestamp) == false) {
				var embed = new DiscordEmbedBuilder() {
					Color = DiscordColor.Red,
					Description = "Invalid timestamp."
				};
				await ctx.EditResponseAsync(embed);
				await Task.Delay(1000);
				await ctx.DeleteResponseAsync();
				return;
			}
			var totalSeconds = 0;
			foreach (var timeSegmentValue in timestamp.Split(':').Select(int.Parse))
				totalSeconds = (totalSeconds * 60) + timeSegmentValue;

			var gCtx = new GjallarContext(ctx, "Seek");
			gCtx.Data.Position = totalSeconds;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("index"), Description("Plays the track at the given index position!")]
		[RequireGuild]
		public async Task Index(CommandContext ctx, [Description("The track's position in the queue.")] int position) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Index") {
				Data = { MiscValue = position }
			};
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("shuffle"), Description("Shuffles the queue.")]
		[RequireGuild]
		public async Task Shuffle(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Shuffle");
			await GjallarCaller.TryCallingAsync(gCtx);
		}
		[Command("remove"), Description("Removes a track from the queue.")]
		[RequireGuild]
		public async Task Remove(CommandContext ctx, [Description("Index from the music to be removed.")] int position) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Volume");
			GjallarCallTools tools = new();
			await tools.InitializeAsync(gCtx);

			var embed = new DiscordEmbedBuilder();
			if (position < 1 || position > tools.Player.TotalTracks) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("There is no track with such index.");
				await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
				await Task.Delay(1000 * 10);
				await ctx.DeleteResponseAsync();
				return;
			}
			embed.WithColor(DiscordColor.Black);
			embed.WithDescription($"[{tools.Player.Tracks[position - 1].Title}]({tools.Player.Tracks[position - 1].Uri}) was removed from queue.");
			tools.Player.RemoveTrackFromQueue(position);
			if (tools.Player.CurrentPosition == position) {
				var toPlayNow = await tools.Player.UseTrackByPositionAsync(tools.Player.CurrentPosition);
				if (toPlayNow != null)
					await tools.Player.PlayAsync(toPlayNow.Track);
				else
					await tools.Player.StopAsync();
			}
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
			await Task.Delay(1000 * 10);
			await ctx.DeleteResponseAsync();
		}
		[Command("reset"), Description("Resets the guild queue.")]
		[RequireGuild]
		public async Task Reset(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "Reset");
			await GjallarCaller.TryCallingAsync(gCtx);
		}

		// 1. ControlPanel
		[Command("ControlPanel"), Description("Returns your Music ControlPanel link for this Channel.")]
		public async Task ControlPanel(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var gCtx = new GjallarContext(ctx, "ControlPanel");
			await GjallarCaller.TryCallingAsync(gCtx);
		}
	}
}

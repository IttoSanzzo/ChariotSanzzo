using ChariotSanzzo.Components.Gjallar.Types;
using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using ChariotSanzzo.Utils;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;

namespace ChariotSanzzo.Components.Gjallar {
	public static class GjallarCaller {
		public static async Task<string> TryCallingAsync(GjallarContext ctx) {
			try {
				string successMessage = $"GjallarCall Received: {ctx.Command}.";
				switch (ctx.Command) {
					case "Message":
						await SendEmbedMessageAsync(ctx);
						return successMessage;
					case "ControlPanel":
						await ControlPanelAsync(ctx);
						return successMessage;
				}

				GjallarCallTools tools = new();
				await tools.InitializeAsync(ctx);
				if (tools.IsValid == false)
					return successMessage;

				if (tools.Player.Chat is null && ctx.ChatChannel is not null)
					await tools.Player.SetChatChannel(ctx.ChatChannel);

				ctx.Result.WasSuccess = ctx.Command switch {
					"Play" => await PlayAsync(ctx, tools),
					"Stop" => await StopAsync(ctx, tools),
					"Pause" => await PauseAsync(ctx, tools),
					"Loop" => await LoopAsync(ctx, tools),
					"Next" => await NextAsync(ctx, tools),
					"Previous" => await PreviousAsync(ctx, tools),
					"Index" => await IndexAsync(ctx, tools),
					"Volume" => await VolumeAsync(ctx, tools),
					"Shuffle" => await ShuffleAsync(ctx, tools),
					"Replay" => await ReplayAsync(ctx, tools),
					"Seek" => await SeekAsync(ctx, tools),
					"Reset" => await ResetAsync(ctx, tools),
					_ => throw new Exception("Invalid Command")
				};
				UpdateFinishedState(ctx, tools);
				await GjallarSocketUpdater.SendStationSocketUpdate(tools);
				await GjallarSocketUpdater.SendQueueSocketUpdate(tools);
				tools.Player.LastGjallarContext = ctx;
				return successMessage;
			} catch (Exception ex) {
				Program.WriteException(ex);
				if (ex.Message == "Invalid Command")
					return $"FunctionsSwitch: GjallarCall Received was not valid. ({ctx.Command})";
				return "Caugh Exception...";
			}
		}

		private static async Task<bool> PlayAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Purple };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			GjallarTrack[] retrievedTracks = [];
			if (IsPlaylistQuery(ctx.Data.Query)) {
				var trackLoadResult = await Lavalink.Tracks.LoadTracksAsync(tools.Ctx.Data.Query, TrackSearchMode.None);
				if (trackLoadResult.IsSuccess)
					retrievedTracks = [.. trackLoadResult.Tracks.Select(track => new GjallarTrack(track) { Member = ctx.Member })];
			} else {
				LavalinkTrack? retrievedTrack = (tools.Ctx.Data.Query.Contains("https://") == true || tools.Ctx.Data.Query.Contains("http://") == true)
					? retrievedTrack = await Lavalink.LavalinkNode.Tracks.LoadTrackAsync(tools.Ctx.Data.Query, TrackSearchMode.None)
					: retrievedTrack = await Lavalink.LavalinkNode.Tracks.LoadTrackAsync(tools.Ctx.Data.Query, ctx.Data.Plataform);
				if (retrievedTrack != null) retrievedTracks = [new(retrievedTrack) { Member = ctx.Member }];
			}
			if (retrievedTracks.Length == 0) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Failed to find proper music using the given query.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.ErrorMessage = "Failed to find proper music using the given query.";
				return false;
			}

			if (retrievedTracks.Length == 1 && tools.Player.TotalTracks > 0 && ctx.Data.Priority == false) {
				var trackPosition = tools.Player.GetTrackPosition(retrievedTracks[0]);
				if (trackPosition != 0) {
					if (trackPosition == tools.Player.CurrentPosition) {
						embed.WithDescription($"_**Already playing...:**_ [{retrievedTracks[0].Title}]({retrievedTracks[0].Uri})\n");
						embed.WithThumbnail(await GjallarTrack.GetArtworkAsync(retrievedTracks[0].Uri!));
						await ctx.GTXEmbedTimerAsync(20, embed);
						return true;
					} else {
						tools.Player.MoveTrackToEndOfQueue(retrievedTracks[0]);
						embed.WithDescription(
							$"_**Moved to the end of Queue:**_ [{retrievedTracks[0].Title}]({retrievedTracks[0].Uri})\n" +
							$"**Author:** {retrievedTracks[0].Author}\n" +
							$"**Length:** {retrievedTracks[0].Duration}" +
							$"\t\t**Index:** ` {tools.Player.TotalTracks} `");
						embed.WithThumbnail(await GjallarTrack.GetArtworkAsync(retrievedTracks[0].Uri!));
						if (tools.Player.CurrentTrack != null)
							await tools.Player.NowPlayingAsync();
						await ctx.GTXEmbedTimerAsync(20, embed);
						return true;
					}
				}
			}

			foreach (var track in retrievedTracks)
				tools.Player.AddTrackToQueue(track);

			if (retrievedTracks.Length > 1) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription($"A Playlist was added! {retrievedTracks.Length} new tracks!");
				if (tools.Player.CurrentTrack != null)
					await tools.Player.NowPlayingAsync();
			}
			if (ctx.Data.Priority == true) {
				if (tools.Player.MoveTrackToEndOfQueue(retrievedTracks[0])) {
					await tools.Player.PlayAsync((await tools.Player.UseTrackAsync(retrievedTracks[0]))!.Track);
					if (retrievedTracks.Length <= 1 && ctx.Ictx != null)
						await ctx.Ictx.DeleteResponseAsync();
					return true;
				} else {
					return false;
				}
			} else if (tools.Player.CurrentTrack == null) {
				await tools.Player.PlayAsync((await tools.Player.UseNextTrackAsync())!.Track);
				if (retrievedTracks.Length == 1 && ctx.Ictx != null) {
					await ctx.Ictx.DeleteResponseAsync();
					return true;
				}
			} else if (retrievedTracks.Length == 1) {
				embed.WithDescription($"_**Added to Queue:**_ [{retrievedTracks[0].Title}]({retrievedTracks[0].Uri})\n" +
					$"**Author:** {retrievedTracks[0].Author}\n" +
					$"**Length:** {retrievedTracks[0].Duration}" +
					$"\t\t**Index:** ` {tools.Player.TotalTracks} `");
				embed.WithThumbnail(await GjallarTrack.GetArtworkAsync(retrievedTracks[0].Uri!));
				if (tools.Player.CurrentTrack != null)
					await tools.Player.NowPlayingAsync();
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return true;
		}
		private static async Task<bool> StopAsync(GjallarContext ctx, GjallarCallTools tools) {
			if (tools.Player is null) return false;
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter(ctx.Username, ctx.UserIcon);
			embed.WithColor(DiscordColor.Black);
			embed.WithTitle("_**Music Stopped!**_");
			if (tools.Player.ActivePlayerMss is not null)
				await tools.Player.DeleteActivePlayerMessageAsync();
			try {
				if (tools.Player.CurrentTrack != null) {
					embed.WithDescription($"_**Stopped Track:**_ [{tools.Player.CurrentTrack.Title}]({tools.Player.CurrentTrack.Uri})");
					await tools.Player.StopAsync();
				}
				await tools.Player.DisconnectAsync();
			} catch { }
			await ctx.GTXEmbedTimerAsync(20, embed);
			return true;
		}
		private static async Task<bool> PauseAsync(GjallarContext ctx, GjallarCallTools tools) {
			if (tools.Player.CurrentTrack == null) {
				if (ctx.Ictx != null) {
					var noTrackEmbed = new DiscordEmbedBuilder();
					noTrackEmbed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
					noTrackEmbed.WithColor(DiscordColor.DarkGray);
					noTrackEmbed.WithDescription($"No track playing to be paused.");
					await ctx.GTXEmbedTimerAsync(20, noTrackEmbed);
				}
				return false;
			}
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(DiscordColor.DarkGray);
			embed.WithDescription($"_**Current Track:**_ [{tools.Player.CurrentTrack.Title}]({tools.Player.CurrentTrack.Uri})");

			if (tools.Player.PauseState == true && ctx.Data.PauseType == GjallarPauseState.Pause) {
				if (ctx.Data.MiscValue == 1) {
					embed.WithColor(DiscordColor.Red);
					embed.WithDescription("Already Paused.");
					await ctx.GTXEmbedTimerAsync(20, embed);
				}
				return true;
			}
			if (ctx.Data.PauseType == GjallarPauseState.Switch)
				ctx.Data.PauseType = tools.Player.SwitchPause();
			var resumeButton = new DiscordButtonComponent(DiscordButtonStyle.Success, "MusicPlayPauseButton", "Resume Track", false, new DiscordComponentEmoji(DiscordUtils.GetUlongIdFromDiscordEmoteUnsafe(EmojisConfig.CircularPlayIcon)));
			switch (ctx.Data.PauseType) {
				case GjallarPauseState.Pause:
					await tools.Player.PauseAsync();
					tools.Player.SetPauseState(true);
					embed.WithTitle("_**Music Paused!**_");
					tools.Player.SetPauseMessage(await ctx.GTXEmbedSendAsync(new DiscordMessageBuilder().AddEmbed(embed.Build()).AddActionRowComponent(resumeButton)));
					break;
				case GjallarPauseState.Resume:
					await tools.Player.ResumeAsync();
					tools.Player.SetPauseState(false);
					embed.WithTitle("_**Music Resumed!**_");
					if (tools.Player.PauseMss is not null)
						await tools.Player.PauseMss.DeleteAsync();
					tools.Player.SetPauseMessage(null);
					await ctx.GTXEmbedTimerAsync(10, embed);
					break;
			}
			if (tools.Player.ActivePlayerMss is not null)
				await tools.Player.ActivePlayerMss.ModifyAsync(await tools.Player.GenNowPlayingAsync() ?? new());
			return true;
		}
		private static async Task<bool> LoopAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(DiscordColor.Azure);

			GjallarLoopState newLoopState = tools.Player.LoopState + 1;
			if (newLoopState > GjallarLoopState.LoopQueue) newLoopState = GjallarLoopState.None;
			tools.Player.SetLoop(newLoopState);

			if (tools.Player.CurrentTrack == null && tools.Player.LoopState != GjallarLoopState.None) {
				ctx.Command = tools.Player.LoopState == GjallarLoopState.LoopTrack ? "Replay" : "Next";
				await ctx.TryCallingAsync();
				ctx.Command = "Loop";
			}
			switch (ctx.Data.LoopType) {
				case GjallarLoopState.None:
					embed.WithDescription("Loop set to _**none**_!");
					break;
				case GjallarLoopState.LoopTrack:
					embed.WithDescription("Loop set to _**track**_!");
					break;
				case GjallarLoopState.LoopQueue:
					embed.WithDescription("Loop set to _**queue**_!");
					break;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			if (tools.Player.ActivePlayerMss is not null)
				await tools.Player.ActivePlayerMss.ModifyAsync(await tools.Player.GenNowPlayingAsync() ?? new());
			return true;
		}
		private static async Task<bool> NextAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			if (ctx.Data.MiscValue == 0)
				embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			if (ctx.Data.SkipCount > 1000 || ctx.Data.SkipCount < 1) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Invalid skip count value.");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.ErrorMessage = "Invalid skip count value.";
				return false;
			}
			if (tools.Player.LoopState != GjallarLoopState.LoopTrack)
				for (int i = 1; i < ctx.Data.SkipCount; i++)
					tools.Player.GoNextPosition();

			var toPlayNow = await tools.Player.UseNextTrackAsync(ctx.Data.IsFromEvent);
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				ctx.Result.WasSuccess = false;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
				ctx.Result.ErrorMessage = "Coundn't Skip (Probably no tracks left).";
			} else {
				await tools.Player.PlayAsync(toPlayNow.Track);
				embed.WithDescription("Track Skipped.");
				ctx.Result.WasSuccess = true;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> PreviousAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			var toPlayNow = await tools.Player.UsePreviousTrackAsync(ctx.Data.IsFromEvent);
			if (toPlayNow == null) {
				ctx.Data.WithResponse = true;
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Coundn't Skip (Probably no tracks left).");
				ctx.Result.WasSuccess = false;
				ctx.Result.ErrorMessage = "Coundn't Skip (Probably no tracks left).";
			} else {
				await tools.Player.PlayAsync(toPlayNow.Track);
				embed.WithDescription("Track set back.");
				ctx.Result.WasSuccess = true;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> VolumeAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Purple };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			if (tools.Ctx.Data.MiscDoubleValue < 0 || tools.Ctx.Data.MiscDoubleValue > 150) {
				embed.WithDescription($"_**Volume:**_ {tools.Ctx.Data.MiscDoubleValue} is a invalid value, please use one between 0 and 150.");
				ctx.Result.WasSuccess = false;
			} else {
				await tools.Player.SetVolumeAsync((float)(tools.Ctx.Data.MiscDoubleValue / 100.0));
				embed.WithDescription($"_**Volume:**_ Set to {tools.Ctx.Data.MiscDoubleValue}.");
				ctx.Result.WasSuccess = true;
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> IndexAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			if (ctx.Data.MiscValue < 1 || ctx.Data.MiscValue > tools.Player.TotalTracks) {
				embed.WithColor(DiscordColor.Red);
				embed.WithDescription("Selected track does no exist!");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.WasSuccess = false;
				return ctx.Result.WasSuccess;
			}
			var toPlayNow = await tools.Player.UseTrackByPositionAsync(ctx.Data.MiscValue);
			if (toPlayNow != null) {
				await tools.Player.PlayAsync(toPlayNow.Track);
				ctx.Result.WasSuccess = true;
			} else {
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
				await ctx.GTXEmbedTimerAsync(20, embed);
				ctx.Result.WasSuccess = false;
			}
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> ShuffleAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			if (await tools.Player.ShuffleTracks()) {
				embed.WithColor(DiscordColor.Aquamarine);
				embed.WithDescription("Shuffed Succesfully!");
				ctx.Result.WasSuccess = true;
			} else {
				embed.WithColor(DiscordColor.Red);
				ctx.Result.WasSuccess = false;
				embed.WithDescription("Failed Shuffling!");
				ctx.Result.ErrorMessage = "Shuffed Succesfully!";
			}
			await ctx.GTXEmbedTimerAsync(20, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> ReplayAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			var toPlayNow = await tools.Player.UseCurrentTrackAsync();
			if (toPlayNow != null) {
				await tools.Player.PlayAsync(toPlayNow.Track);
				embed.WithDescription("Track replayed.");
				ctx.Result.WasSuccess = true;
			} else {
				embed.WithDescription("Coundn't replay (Probably no tracks left).");
				ctx.Result.WasSuccess = false;
				ctx.Result.ErrorMessage = "Coundn't replay (Probably no tracks left).";
			}
			await ctx.GTXEmbedTimerAsync(10, embed);
			return ctx.Result.WasSuccess;
		}
		private static async Task<bool> SeekAsync(GjallarContext ctx, GjallarCallTools tools) {
			if (tools.Player.TotalTracks == 0
					|| ctx.Data.Position > tools.Player.CurrentGjallarTrack?.LengthInSeconds.TotalSeconds
					|| ctx.Data.Position < 0)
				return false;

			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			var timespan = TimeSpan.FromSeconds(ctx.Data.Position);
			if (tools.Player.IsFinished) {
				var toPlayNow = await tools.Player.UseCurrentTrackAsync();
				if (toPlayNow != null) {
					await tools.Player.PlayAsync(toPlayNow.Track);
					while (tools.Player.PlaybackPosition?.TotalSeconds > 1 || tools.Player.PlaybackPosition?.Milliseconds < 1)
						await Task.Delay(50);
					await tools.Player.SeekAsync(timespan);
				} else {
					embed.WithDescription("Coundn't replay for seek (Probably no tracks left).");
					ctx.Result.WasSuccess = false;
					ctx.Result.ErrorMessage = "Coundn't replay (Probably no tracks left).";
					return false;
				}
			} else {
				if (tools.Player.PauseState || tools.Player.PlaybackPosition?.TotalSeconds < tools.Player.CurrentTrack?.Duration.TotalSeconds - 10) {
					await tools.Player.SeekAsync(timespan);
				} else {
					var toPlayNow = await tools.Player.UseCurrentTrackAsync();
					if (toPlayNow != null) {
						await tools.Player.PlayAsync(toPlayNow.Track);
						while (tools.Player.PlaybackPosition?.TotalSeconds > 1 || tools.Player.PlaybackPosition?.Milliseconds < 1) {
							await Task.Delay(50);
						}
						await tools.Player.SeekAsync(timespan);
					}
				}
			}
			embed.WithDescription($"Track seeked to {timespan.ToString(@"hh\:mm\:ss")}.");
			await ctx.GTXEmbedTimerAsync(10, embed);
			return true;
		}
		private static async Task<bool> ResetAsync(GjallarContext ctx, GjallarCallTools tools) {
			var embed = new DiscordEmbedBuilder() { Color = DiscordColor.Aquamarine };
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);

			await tools.Player.StopAsync();
			tools.Player.ClearTracks();
			embed.WithDescription("Guild Queue was reset.");
			await ctx.GTXEmbedTimerAsync(20, embed);
			if (tools.Player.ActivePlayerMss is not null)
				await tools.Player.DeleteActivePlayerMessageAsync();
			return true;
		}
		private static async Task SendEmbedMessageAsync(GjallarContext ctx) {
			if (ctx.ChatChannel is null)
				return;
			var embed = new DiscordEmbedBuilder();
			embed.WithFooter($"By: {ctx.Username}", ctx.UserIcon);
			embed.WithColor(ctx.Color);
			embed.WithDescription(ctx.Message ?? "");
			await ctx.GTXEmbedTimerAsync(20, embed);
		}
		private static async Task ControlPanelAsync(GjallarContext ctx) {
			if (ctx.Ictx == null || ctx.ChatChannel is null) return;
			try {
				var embed = new DiscordEmbedBuilder();
				embed.WithColor(DiscordColor.DarkBlue);
				embed.WithTitle("Music ControlPanel Link");
				embed.WithDescription($"[Here is your link for this Channel's ControlPanel]({LinkData.GetGjallarhornControlFullAddress()}/{DiscordBotConfig.Name}/control-panel?&userId={ctx.Member.Id})");
				embed.WithFooter($"For: {ctx.Username}", ctx.UserIcon);
				await ctx.Ictx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
				await Task.Delay(1000 * 15);
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
			await ctx.Ictx.DeleteResponseAsync();
		}

		private static void UpdateFinishedState(GjallarContext ctx, GjallarCallTools tools) {
			tools.Player.IsFinished = ctx.Command switch {
				("Stop" or "Reset") => true,
				("Pause") => tools.Player.IsFinished,
				("Next" or "Replay" or "Seek") when ctx.Result != null && ctx.Result.WasSuccess == false && ctx.Data.IsFromEvent => tools.Player.IsFinished = true,
				_ when ctx.Result != null && ctx.Result.WasSuccess => false,
				_ => tools.Player.IsFinished
			};
		}
		private static bool IsPlaylistQuery(string query) {
			return query.Contains("youtube.com/playlist?") == true
				|| query.Contains("spotify.com/playlist") == true
				|| (query.Contains("spotify.com/") == true && query.Contains("/album/") == true)
				|| (query.Contains("soundcloud.com/") == true && query.Contains("/sets") == true);
		}
	}
}

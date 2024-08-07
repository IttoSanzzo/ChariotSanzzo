using System.Diagnostics;
using System.Drawing;
using ChariotSanzzo.Commands.Slash;
using ChariotSanzzo.Components.MusicQueue;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Events {
	public static class Music {
		// 0. Eventual Events
		public static async Task PlayNext(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) {
			if (ctx.Reason != TrackEndReason.Finished)
				return ;
			var	queue = MusicCommands.QColle.GetQueue((long)ctx.Player.Guild.Id, conn, null);
			var	toPlayNow = await queue.UseNextTrackAsync();
			if (toPlayNow == null) {
				if (queue._chat != null) {
					var	embed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Purple,
						Description = "There are no tracks left to play next."
					};
					await queue._chat.SendMessageAsync(embed: embed);
				}
				return ;
			}
			await queue._conn.PlayAsync(toPlayNow);
			return ;
		}
		
		// 1. Button Events
		public static async Task MusicInterectionButton(DiscordClient sender, ComponentInteractionCreateEventArgs ctx) {
			TrackQueue?		queue = null;
			var				embed = new DiscordEmbedBuilder();
			LavalinkTrack?	toPlayNow;
			DiscordMessage	trashMss;
			switch (ctx.Interaction.Data.CustomId) {
				case ("MusicPlayPauseButton"): // PlayPauseButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
					// 0. Initialization
					queue = MusicCommands.QColle.GetQueueUnsafe((long)ctx.Guild.Id);
					if (queue == null)
						return ;
					switch (queue._pauseState) {
						case (false):
							await queue._conn.PauseAsync();
							queue.SetPauseState(true);
							await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync(queue, queue._tracks[queue._currentIndex]));
						break;
						case (true):
							await queue._conn.ResumeAsync();
							queue.SetPauseState(false);
							await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync(queue, queue._tracks[queue._currentIndex]));
							if (queue._pauseMss != null)
								await queue._pauseMss.DeleteAsync();
							queue.SetPauseMessage(null);
						break;
					}
				break;
				case ("MusicNextTrackButton"): // NextTrackButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = MusicCommands.QColle.GetQueueUnsafe((long)ctx.Guild.Id);
					if (queue == null)
						return ;

				// 1. Core
					embed.WithColor(DiscordColor.Aquamarine);
					toPlayNow = await queue.UseNextTrackAsync();
					if (toPlayNow != null) {
						await queue._conn.PlayAsync(toPlayNow);
						embed.WithDescription("Track Skipped.");
					}
					else
						embed.WithDescription("Coundn't Skip (Probably no tracks left).");
					trashMss = await ctx.Channel.SendMessageAsync(embed: embed);
					await Task.Delay(1000 * 10);
					await trashMss.DeleteAsync();
				break;
				case ("MusicPreviousTrackButton"): // PreviousTrackButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = MusicCommands.QColle.GetQueueUnsafe((long)ctx.Guild.Id);
					if (queue == null)
						return ;

				// 1. Core
					embed.WithColor(DiscordColor.Aquamarine);
					toPlayNow = await queue.UsePreviousTrackAsync();
					if (toPlayNow != null)
						embed.WithDescription("Track set back.");
					else
						embed.WithDescription("Coundn't go back (Probably no tracks left).");
					trashMss = await ctx.Channel.SendMessageAsync(embed: embed);
					if (toPlayNow != null)
						await queue._conn.PlayAsync(toPlayNow);
					await Task.Delay(1000 * 10);
					await trashMss.DeleteAsync();
				break;
				case ("MusicLoopButton"): // MusicLoopButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = MusicCommands.QColle.GetQueueUnsafe((long)ctx.Guild.Id);
					if (queue == null)
						return ;
					queue.SetLoop(queue._loop + 1);
					await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync(queue, queue._tracks[queue._currentIndex]));
				break;
				case ("MusicShuffleButton"):
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = MusicCommands.QColle.GetQueueUnsafe((long)ctx.Guild.Id);
					if (queue == null)
						return ;
					await queue.ShuffleTracks();
				break;
				default:
					return ;
			}
		}

		/*
		public static async Task NowPlaying(LavalinkGuildConnection conn, TrackStartEventArgs ctx) {
			var	queue = MusicCommands.QColle.GetQueue((long)ctx.Player.Guild.Id, conn, null);
			if (queue._loop == 1)
				return ;
			var	embed = new DiscordEmbedBuilder() {
				Color = DiscordColor.Purple,
				Description = $"_**Now Playing:**_ {ctx.Track.Title}\n" +
										$"_**Length:**_ {ctx.Track.Length}\n" +
										$"_**Author:**_ {ctx.Track.Author}\n" +
										$"_**URL:**_ {ctx.Track.Uri}"
			};
			if (ctx.Track.Uri.ToString().Contains("youtube.com") == true)
				embed.WithImageUrl($"https://img.youtube.com/vi/{ctx.Track.Uri.ToString().Substring(32)}/maxresdefault.jpg");
			if (queue._chat != null)
				await queue._chat.SendMessageAsync(embed: embed);
		}
		*/
	}
}
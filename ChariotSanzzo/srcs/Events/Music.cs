using ChariotSanzzo.Components.MusicComponent;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace ChariotSanzzo.Events {
	public static class Music {
	// 0. Eventual Events
		public static async Task PlayNext(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) {
			if (ctx.Reason != TrackEndReason.Finished)
				return ;
			var	queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Player.Guild.Id);
			if (queue == null)
				return ;
			var	toPlayNow = await queue.UseNextTrackAsync();
			if (toPlayNow == null) {
				if (queue.Chat != null) {
					var	embed = new DiscordEmbedBuilder() {
						Color = DiscordColor.Purple,
						Description = "There are no tracks left to play next."
					};
					await queue.Chat.SendMessageAsync(embed: embed);
				}
				return ;
			}
			await queue.Conn.PlayAsync(toPlayNow);
			return ;
		}
		public static Task Disconnected(DiscordClient sender, VoiceStateUpdateEventArgs ctx) {
			if (ctx.User.Id == 1070103829934260344 && ctx.After.Member.VoiceState == null)
				ChariotMusicCalls.QColle.DropQueue(ctx.Before.Channel.Guild.Id);
			return (Task.CompletedTask);
		}
		public static Task NewConn(LavalinkGuildConnection conn, GuildConnectionCreatedEventArgs ctx) {
			Program.WriteLine("[CONNECTED!]");
			conn.StopAsync();
			ChariotMusicCalls.QColle.DropQueue(conn.Guild.Id);
			return (Task.CompletedTask);
		}

	// 1. Button Events
		public static async Task MusicInterectionButton(DiscordClient sender, ComponentInteractionCreateEventArgs ctx) {
			TrackQueue?		queue = null;
			var				embed = new DiscordEmbedBuilder();
			LavalinkTrack?	toPlayNow;
			DiscordMessage	trashMss;
			switch (ctx.Interaction.Data.CustomId) {
				case ("MusicPlayPauseButton"):		// PlayPauseButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
					// 0. Initialization
					queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
					if (queue == null)
						return ;
					switch (queue.PauseState) {
						case (false):
							await queue.Conn.PauseAsync();
							queue.SetPauseState(true);
							await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync());
						break;
						case (true):
							await queue.Conn.ResumeAsync();
							queue.SetPauseState(false);
							await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync());
							if (queue.PauseMss != null)
								await queue.PauseMss.DeleteAsync();
							queue.SetPauseMessage(null);
						break;
					}
				break;
				case ("MusicNextTrackButton"):		// NextTrackButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
					if (queue == null)
						return ;

				// 1. Core
					embed.WithColor(DiscordColor.Aquamarine);
					toPlayNow = await queue.UseNextTrackAsync();
					if (toPlayNow != null) {
						await queue.Conn.PlayAsync(toPlayNow);
						embed.WithDescription("Track Skipped.");
					}
					else
						embed.WithDescription("Coundn't Skip (Probably no tracks left).");
					trashMss = await ctx.Channel.SendMessageAsync(embed: embed);
					await Task.Delay(1000 * 10);
					await trashMss.DeleteAsync();
				break;
				case ("MusicPreviousTrackButton"):	// PreviousTrackButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
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
						await queue.Conn.PlayAsync(toPlayNow);
					await Task.Delay(1000 * 10);
					await trashMss.DeleteAsync();
				break;
				case ("MusicLoopButton"):			// MusicLoopButton
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
					if (queue == null)
						return ;
					queue.SetLoop(queue.Loop + 1);
					await ctx.Message.ModifyAsync(await queue.GenNowPlayingAsync());
				break;
				case ("MusicShuffleButton"):
					await ctx.Interaction.DeferAsync();
					await ctx.Interaction.DeleteOriginalResponseAsync();
				// 0. Initialization
					queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Guild.Id);
					if (queue == null)
						return ;
					await queue.ShuffleTracks();
				break;
				default:
					return ;
			}
		}
	}
}
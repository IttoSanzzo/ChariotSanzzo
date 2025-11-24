using ChariotSanzzo.Components.MusicComponent;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace ChariotSanzzo.Events {
	public static class CharitoMusicEvents {
		// 0. Eventual Events
		#pragma warning disable CS1998
		public static async Task	TrackStartedEvent(LavalinkGuildConnection conn, TrackStartEventArgs ctx) {
			// var	queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Player.Guild.Id);
			// if (queue == null)
			// 	return ;
			// queue.IsFinished = false;
		}
		#pragma warning restore CS1998
		public static async Task	TrackEndedEvent(LavalinkGuildConnection conn, TrackFinishEventArgs ctx) {
			if (ctx.Reason != TrackEndReason.Finished)
				return ;
			var	queue = ChariotMusicCalls.QColle.GetQueueUnsafe(ctx.Player.Guild.Id);
			if (queue == null)
				return ;
			var gCtx = (queue.Chat != null)
				? new GjallarhornContext(queue.Loop == 1 ? "Replay" : "Next",  queue.Chat, null, queue.Owner)
				: new GjallarhornContext(queue.Loop == 1 ? "Replay" : "Next",  ctx.Player.Guild, null, queue.Owner);
			gCtx.Data.VipCall = true;
			gCtx.Data.WithResponse = false;
			gCtx.Data.MiscValue = 1;
			gCtx.Data.IsFromEvent = true;
			await gCtx.TryCallingAsync();
		}
		public static Task			Disconnected(DiscordClient sender, VoiceStateUpdateEventArgs ctx) {
			if (ctx.User.Id == 1070103829934260344 && ctx.After.Member.VoiceState == null)
				ChariotMusicCalls.QColle.DropQueue(ctx.Before.Channel.Guild.Id);
			Program.ColorWriteLine(ConsoleColor.DarkGreen, $"[DISCONNECTED!] {ctx.Before.Channel.Guild.Id}");
			return Task.CompletedTask;
		}
		public static Task			NewConn(LavalinkGuildConnection conn, GuildConnectionCreatedEventArgs ctx) {
			Program.ColorWriteLine(ConsoleColor.Green, $"[CONNECTED!] {conn.Guild.Id}");
			conn.StopAsync();
			ChariotMusicCalls.QColle.DropQueue(conn.Guild.Id);
			return Task.CompletedTask;
		}

	// 1. Button Events
		public static async Task MusicInterectionButton(DiscordClient sender, ComponentInteractionCreateEventArgs ctx) {
			await ctx.Interaction.DeferAsync();
			await ctx.Interaction.DeleteOriginalResponseAsync();
		// 0. Variable Declarations
			var	embed = new DiscordEmbedBuilder();
			var	gCtx = new GjallarhornContext(ctx);
			gCtx.Data.WithResponse = false;
			gCtx.Data.VipCall = true;
			switch (ctx.Interaction.Data.CustomId) {
				case ("MusicPlayPauseButton"):		// PlayPauseButton
					gCtx.Command = "Pause";
					await gCtx.TryCallingAsync();
				break;
				case ("MusicNextTrackButton"):		// NextTrackButton
					gCtx.Command = "Next";
					await gCtx.TryCallingAsync();
				break;
				case ("MusicPreviousTrackButton"):	// PreviousTrackButton
					gCtx.Command = "Previous";
					await gCtx.TryCallingAsync();
				break;
				case ("MusicLoopButton"):			// MusicLoopButton
					gCtx.Command = "Loop";
					await gCtx.TryCallingAsync();
				break;
				case ("MusicShuffleButton"):		// Shuffle
					gCtx.Command = "Shuffle";
					gCtx.Data.WithResponse = true;
					await gCtx.TryCallingAsync();
				break;
				case ("MusicReplayTrackButton"):	// ReplayTrackButton
					gCtx.Command = "Replay";
					await gCtx.TryCallingAsync();
				break;
				default:
					return ;
			}
		}
	}
}
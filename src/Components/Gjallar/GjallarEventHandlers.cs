using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Services.Wrappers;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;

namespace ChariotSanzzo.Components.Gjallar {
	public static class GjallarEventHandlers {
		public static async ValueTask HandleTrackEndedEvent(GjallarPlayer player, TrackEndReason endReason, CancellationToken cancellationToken = default) {
			if (endReason != TrackEndReason.Finished || !Lavalink.PlayerManager.Players.Any(player => player.GuildId == player.GuildId))
				return;
			var gCtx = (player.Chat is not null)
				? new GjallarContext(player.LoopState == Types.GjallarLoopState.LoopTrack ? "Replay" : "Next", player.Chat, player.Owner)
				: new GjallarContext(player.LoopState == Types.GjallarLoopState.LoopTrack ? "Replay" : "Next", player.Guild, player.Owner);
			gCtx.Data.VipCall = true;
			gCtx.Data.WithResponse = false;
			gCtx.Data.MiscValue = 1;
			gCtx.Data.IsFromEvent = true;
			await gCtx.TryCallingAsync();
		}
		public static async ValueTask HandleTrackStuckEvent(GjallarPlayer player, TimeSpan threshold, CancellationToken cancellationToken = default) {
			Program.WriteLine($"!!!!!!!!!!!!!!!!!!! GjallarError! Stuck ${threshold}");
			await ValueTask.CompletedTask;
		}
		public static async ValueTask HandleTrackExceptionEvent(GjallarPlayer player, TrackException exception, CancellationToken cancellationToken = default) {
			Program.WriteLine($"!!!!!!!!!!!!!!!!!!! GjallarError! Exception ${exception.Message}");
			await ValueTask.CompletedTask;
		}
	}
	public class PlayerBotDisconnected : IEventHandler<VoiceStateUpdatedEventArgs> {
		public async Task HandleEventAsync(DiscordClient client, VoiceStateUpdatedEventArgs ctx) {
			if (
				ctx.Before != null
				&& ctx.Before.UserId == DiscordBotConfig.BotUserId
				&& ctx.After.ChannelId == null
				&& Lavalink.PlayerManager.Players.Any(player => player.GuildId == ctx.Before.GuildId)
			) {
				Program.ColorWriteLine(ConsoleColor.DarkGreen, $"[DISCONNECTED!] {ctx.Before.GuildId}");
				var guild = (await ctx.GetGuildAsync())!;
				DiscordMember bot = await guild.GetMemberAsync(ctx.Before.UserId);
				GjallarContext gtx = new("Disconnect", guild, bot) {
					VoiceChannel = await ctx.Before.GetChannelAsync()
				};
				await gtx.TryCallingAsync();
			}
		}
	}
	public class GjallarInteractionButtonHandler : IEventHandler<ComponentInteractionCreatedEventArgs> {
		public async Task HandleEventAsync(DiscordClient client, ComponentInteractionCreatedEventArgs ctx) {
			await ctx.Interaction.DeferAsync();
			await ctx.Interaction.DeleteOriginalResponseAsync();
			var embed = new DiscordEmbedBuilder();
			var gCtx = new GjallarContext(ctx);
			gCtx.Data.WithResponse = false;
			gCtx.Data.VipCall = true;
			gCtx.Command = ctx.Interaction.Data.CustomId switch {
				"MusicPlayPauseButton" => "Pause",
				"MusicNextTrackButton" => "Next",
				"MusicPreviousTrackButton" => "Previous",
				"MusicLoopButton" => "Loop",
				"MusicShuffleButton" => "Shuffle",
				"MusicReplayTrackButton" => "Replay",
				_ => ""
			};
			if (gCtx.Command != "") await gCtx.TryCallingAsync();
		}
	}
}

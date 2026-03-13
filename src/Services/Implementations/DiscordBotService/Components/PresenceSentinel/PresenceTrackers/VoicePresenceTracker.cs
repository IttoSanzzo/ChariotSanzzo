using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Services.Components {
	public class VoicePresenceTracker : IPresenceTracker, IEventHandler<VoiceStateUpdatedEventArgs> {
		private static PresenceRegistry Registry => PresenceSentinel.Registry;
		private class UpdateDebounceState {
			public long TimestampMilliseconds { get; set; } = 0;
			public CancellationTokenSource CancellationToken { get; set; } = null!;
		}
		private static Dictionary<ulong, UpdateDebounceState> VoiceUpdateDebounceState { get; } = [];

		public async Task HandleEventAsync(DiscordClient sender, VoiceStateUpdatedEventArgs ctx) {
			if (ctx.After.ChannelId != null)
				await Task.Delay(15);
			var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			if (VoiceUpdateDebounceState.TryGetValue(ctx.UserId, out var lastUpdateDebounce)) {
				if (lastUpdateDebounce.TimestampMilliseconds > currentTimestamp) {
					return;
				} else {
					lastUpdateDebounce.CancellationToken.Cancel();
					VoiceUpdateDebounceState.Remove(ctx.UserId);
				}
			}

			var state = await Registry.GetOrCreate(ctx.UserId);
			state.Set("voice.lastUpdate", currentTimestamp.ToString());
			if (ctx.After.ChannelId != null) {
				state.Set("voice.guildId", ctx.After.GuildId.ToString()!);
				state.Set("voice.guildName", (await ctx.After.GetGuildAsync())!.Name);
				state.Set("voice.channelId", ctx.After.ChannelId.ToString()!);
				state.Set("voice.channelName", (await ctx.After.GetChannelAsync())!.Name);
			} else {
				state.Set("voice.guildId", "0");
				state.Set("voice.guildName", "");
				state.Set("voice.channelId", "0");
				state.Set("voice.channelName", "");
			}


			VoiceUpdateDebounceState.Add(ctx.UserId, new() {
				TimestampMilliseconds = currentTimestamp,
				CancellationToken = SetTimeout(async () => {
					await PresenceSentinel.PostPresenceUpdate(ctx.UserId);
					VoiceUpdateDebounceState.Remove(ctx.UserId);
				}, 100)
			});
		}
		private static CancellationTokenSource SetTimeout(Func<Task> action, int delayMs) {
			var cts = new CancellationTokenSource();
			_ = Task.Run(async () => {
				try {
					await Task.Delay(delayMs, cts.Token);
					if (!cts.Token.IsCancellationRequested)
						await action();
				} catch (TaskCanceledException) { }
			});
			return cts;
		}
	}
}

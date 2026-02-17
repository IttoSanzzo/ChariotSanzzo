using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Components.PresenceSentinel {
	public class VoicePresenceTracker(DiscordClient client) : IPresenceTracker {
		private class UpdateDebounceState {
			public long TimestampMilliseconds { get; set; } = 0;
			public CancellationTokenSource CancellationToken { get; set; } = null!;
		}
		private PresenceRegistry Registry = null!;
		private readonly DiscordClient Client = client;
		private static Dictionary<ulong, UpdateDebounceState> VoiceUpdateDebounceState { get; } = [];

		public Task InitializeAsync(PresenceRegistry registry) {
			Registry = registry;
			Client.VoiceStateUpdated += OnVoiceStateUpdated;
			return Task.CompletedTask;
		}

		private async Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdateEventArgs args) {
			if (args.After.Channel != null)
				await Task.Delay(15);
			var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			if (VoiceUpdateDebounceState.TryGetValue(args.User.Id, out var lastUpdateDebounce)) {
				if (lastUpdateDebounce.TimestampMilliseconds > currentTimestamp) {
					return;
				} else {
					lastUpdateDebounce.CancellationToken.Cancel();
					VoiceUpdateDebounceState.Remove(args.User.Id);
				}
			}

			var state = await Registry.GetOrCreate(args.User.Id);
			state.Set("voice.lastUpdate", currentTimestamp.ToString());
			if (args.After.Channel != null) {
				state.Set("voice.guildId", args.After.Guild.Id.ToString());
				state.Set("voice.guildName", args.After.Guild.Name);
				state.Set("voice.channelId", args.After.Channel.Id.ToString());
				state.Set("voice.channelName", args.After.Channel.Name);
			} else {
				state.Set("voice.guildId", "0");
				state.Set("voice.guildName", "");
				state.Set("voice.channelId", "0");
				state.Set("voice.channelName", "");
			}


			VoiceUpdateDebounceState.Add(args.User.Id, new() {
				TimestampMilliseconds = currentTimestamp,
				CancellationToken = SetTimeout(async () => {
					await Program.PresenceSentinel.PostPresenceUpdate(args.User.Id);
					VoiceUpdateDebounceState.Remove(args.User.Id);
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

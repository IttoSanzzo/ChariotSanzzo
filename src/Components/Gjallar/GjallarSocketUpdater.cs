using ChariotSanzzo.Components.Gjallar.Types;
using ChariotSanzzo.Infrastructure.Config;
using ChariotSanzzo.Utils;

namespace ChariotSanzzo.Components.Gjallar {
	public static class GjallarSocketUpdater {
		private class TrackInfo(string title, string link, string artwork, double durationInSeconds, int index, string originalUser, string originalUserAvatarUrl) {
			public string Title { get; set; } = title;
			public string Link { get; set; } = link;
			public string Artwork { get; set; } = artwork;
			public double DurationInSeconds { get; set; } = durationInSeconds;
			public int Index { get; set; } = index;
			public string OriginalUser { get; set; } = originalUser;
			public string OriginalUserAvatarUrl { get; set; } = originalUserAvatarUrl;
		}
		private class CurrentTrackInfo(string title, string link, string artwork, double durationInSeconds, int index, string originalUser, string originalUserAvatarUrl, double currentPosition, long lastUpdate) : TrackInfo(title, link, artwork, durationInSeconds, index, originalUser, originalUserAvatarUrl) {
			public double CurrentPosition { get; set; } = currentPosition;
			public long LastUpdate { get; set; } = lastUpdate;
		}
		private class PlayerLiveUpdateDto(string guildId, string? voiceChannelId, string? chatChannelId, bool isPaused, int loopState, int volume, bool isFinished, int currentIndex, GjallarCallResult lastCommandResult, CurrentTrackInfo? currentTrackInfo, TrackInfo? previousTrackInfo, TrackInfo? nextTrackInfo) {
			public long UnixTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			public string GuildId { get; set; } = guildId;
			public string? VoiceChannelId { get; set; } = voiceChannelId;
			public string? ChatChannelId { get; set; } = chatChannelId;
			public bool IsPaused { get; set; } = isPaused;
			public int LoopState { get; set; } = loopState;
			public int Volume { get; set; } = volume;
			public bool IsFinished { get; set; } = isFinished;
			public int CurrentIndex { get; set; } = currentIndex;
			public GjallarCallResult LastCommandResult { get; set; } = lastCommandResult;
			public CurrentTrackInfo? CurrentTrack { get; set; } = currentTrackInfo;
			public TrackInfo? PreviousTrack { get; set; } = previousTrackInfo;
			public TrackInfo? NextTrack { get; set; } = nextTrackInfo;
		}
		private class QueueUpdateDto(string guildId, string guildName, string? voiceChannelId, bool isPaused, int loopState, bool isFinished, int currentIndex, TrackInfo[] tracks) {
			public long UnixTimestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			public string GuildId { get; set; } = guildId;
			public string GuildName { get; set; } = guildName;
			public string? VoiceChannelId { get; set; } = voiceChannelId;
			public bool IsPaused { get; set; } = isPaused;
			public int LoopState { get; set; } = loopState;
			public bool IsFinished { get; set; } = isFinished;
			public int CurrentIndex { get; set; } = currentIndex;
			public TrackInfo[] Tracks { get; set; } = tracks;
		}

		public static async Task SendStationSocketUpdate(GjallarCallTools tools) {
			var (currentTrack, currentTrackIndex) = tools.Player.GetCurrentTrackSafe(tools.Ctx);
			var (previousTrack, previousTrackIndex) = tools.Player.GetPreviousTrackSafe(tools.Ctx);
			var (nextTrack, nextTrackIndex) = tools.Player.GetNextTrackSafe(tools.Ctx);
			var currentPosition = currentTrack.GetTrackCurrentPosition(tools, tools.Ctx.Result);

			await Program.HttpClient.PostAsJsonAsync<PlayerLiveUpdateDto>(LinkData.GetChariotApiFullAddress($"/live/{DiscordBotConfig.Name}/{tools.GuildId}/player-update-socket"), new(
				tools.GuildId.ToString(),
				tools.Player.VoiceChannelId.ToString(),
				tools.Player.Chat?.Id.ToString(),
				tools.Player.PauseState,
				(int)tools.Player.LoopState,
				float.ConvertToInteger<int>(tools.Player.Volume * 100),
				tools.Player.IsFinished,
				tools.Player.CurrentPosition,
				tools.Ctx.Result,
				await currentTrack.ToNullableCurrentTrackInfo(
						currentTrackIndex,
						currentPosition,
						tools.Player.Position is not null
							? tools.Player.Position?.SyncedAt.ToUnixTimeSeconds()
								?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()
							: DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
				await previousTrack.ToNullableTrackInfo(previousTrackIndex),
				await nextTrack.ToNullableTrackInfo(nextTrackIndex)
			));
		}
		public static async Task SendQueueSocketUpdate(GjallarCallTools tools) {
			if (!(tools.Ctx.Command == "Play"
				|| tools.Ctx.Command == "Index"
				|| tools.Ctx.Command == "Previous"
				|| tools.Ctx.Command == "Next"
				|| tools.Ctx.Command == "Shuffle"
				|| tools.Ctx.Command == "Pause"
				|| tools.Ctx.Command == "Loop"
				|| tools.Ctx.Command == "Reset"
				|| tools.Ctx.Command == "Stop")
			)
				return;
			var tracks = tools.Ctx.Command switch {
				("Stop" or "Reset") when tools.Ctx.Result.WasSuccess => [],
				_ => await Task.WhenAll(
					tools.Player.Tracks.Select((track, index) => track.ToTrackInfo(index))
				)
			};
			var response = await Program.HttpClient.PostAsJsonAsync<QueueUpdateDto>(LinkData.GetChariotApiFullAddress($"/live/{DiscordBotConfig.Name}/{tools.GuildId}/queue-update-socket"), new(
				tools.GuildId.ToString(),
				tools.Ctx.Guild.Name,
				tools.Player.VoiceChannelId.ToString(),
				tools.Player.PauseState,
				(int)tools.Player.LoopState,
				tools.Player.IsFinished,
				tools.Player.CurrentPosition,
				tracks ?? []
			));
		}

		private static (GjallarTrack? track, int position) GetCurrentTrackSafe(this GjallarPlayer player, GjallarContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset") return (null, 0);
			return player.TotalTracks > 0
				? (player.CurrentGjallarTrack, player.CurrentPosition)
				: (null, 0);
		}
		private static (GjallarTrack? track, int position) GetPreviousTrackSafe(this GjallarPlayer player, GjallarContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset") return (null, 0);
			var position = player.CurrentPosition - 1;
			if (position < 1) {
				if (player.LoopState == GjallarLoopState.LoopQueue)
					return (player.Tracks[player.TotalTracks - 1], player.TotalTracks);
				else
					return (null, 0);
			}
			return (player.Tracks[position - 1], position);
		}
		private static (GjallarTrack? track, int position) GetNextTrackSafe(this GjallarPlayer player, GjallarContext ctx) {
			if (ctx.Command == "Stop" || ctx.Command == "Reset") return (null, 0);
			var position = player.CurrentPosition + 1;
			if (position > player.TotalTracks) {
				if (player.LoopState == GjallarLoopState.LoopQueue)
					return (player.Tracks[0], 1);
				else
					return (null, 0);
			}
			return (player.Tracks[position - 1], position);
		}
		private static double GetTrackCurrentPosition(this GjallarTrack? track, GjallarCallTools tools, GjallarCallResult result) {
			if (track == null)
				return 0;
			return tools.Ctx.Command switch {
				"Seek" => tools.Ctx.Data.Position,
				"Next" when result.WasSuccess == false => Math.Ceiling(tools.Player.PlaybackPosition?.TotalSeconds ?? track.Duration.TotalSeconds),
				"Pause" when result.WasSuccess && tools.Player.PauseState == false && tools.Player.LastGjallarContext != null && tools.Player.LastGjallarContext.Command == "Seek" && tools.Player.LastGjallarContext.Result.WasSuccess => tools.Player.LastGjallarContext.Data.Position,
				"Play" or "Previous" or "Next" or "Shuffle" or "Replay" or "Reset" or "Stop" => 0,
				_ => Math.Ceiling(tools.Player.PlaybackPosition?.TotalSeconds ?? track.Duration.TotalSeconds),
			};
		}

		private static async Task<TrackInfo> ToTrackInfo(this GjallarTrack track, int index) {
			return new(
				track.Title,
				track.Uri!.AbsoluteUri,
				await track.GetArtworkAsync(),
				Math.Floor(track.Duration.TotalSeconds),
				index,
				track.Member!.Username,
				track.Member!.AvatarUrl
			);
		}
		private static async Task<TrackInfo?> ToNullableTrackInfo(this GjallarTrack? track, int index)
			=> track is null ? null : await track.ToTrackInfo(index);
		private static async Task<CurrentTrackInfo?> ToNullableCurrentTrackInfo(this GjallarTrack? track, int index, double currentPosition, long lastUpdate) {
			if (track == null)
				return null;
			return new(
				track.Title,
				track.Uri!.AbsoluteUri,
				await track.GetArtworkAsync(),
				Math.Floor(track.Duration.TotalSeconds),
				index,
				track.Member!.Username,
				track.Member!.AvatarUrl,
				currentPosition,
				lastUpdate
			);
		}
	}
}

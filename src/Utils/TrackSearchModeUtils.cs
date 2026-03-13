using Lavalink4NET.Rest.Entities.Tracks;

namespace ChariotSanzzo.Utils {
	public static class TrackSearchModeUtils {
		private static readonly Dictionary<TrackSearchMode, int> ModeToInt = new() {
			[TrackSearchMode.None] = 0,
			[TrackSearchMode.YouTube] = 1,
			[TrackSearchMode.YouTubeMusic] = 2,
			[TrackSearchMode.SoundCloud] = 3,
			[TrackSearchMode.Spotify] = 4,
			[TrackSearchMode.AppleMusic] = 5,
			[TrackSearchMode.Bandcamp] = 6,
			[TrackSearchMode.Deezer] = 7,
			[TrackSearchMode.YandexMusic] = 8,
		};
		private static readonly Dictionary<int, TrackSearchMode> IntToMode =
			ModeToInt.ToDictionary(x => x.Value, x => x.Key);

		public static int ToInt(this TrackSearchMode mode) => ModeToInt[mode];
		public static TrackSearchMode IntToTrackSearchMode(int mode) => IntToMode[mode];
	}
}

using ChariotSanzzo.Utils;
using Lavalink4NET.Rest.Entities.Tracks;

namespace ChariotSanzzo.Services.Commands.ChoiceProviders {
	public class AvailableSearchPlataformsProvider : IIntChoiceProvider {
		public override (string, int)[] Options { get; } = [
			("None", TrackSearchMode.None.ToInt()),
			("YouTube", TrackSearchMode.YouTube.ToInt()),
			("YouTubeMusic", TrackSearchMode.YouTubeMusic.ToInt()),
			("SoundCloud", TrackSearchMode.SoundCloud.ToInt()),
			("Spotify", TrackSearchMode.Spotify.ToInt()),
			("AppleMusic", TrackSearchMode.AppleMusic.ToInt()),
			("Bandcamp", TrackSearchMode.Bandcamp.ToInt()),
			("Deezer", TrackSearchMode.Deezer.ToInt()),
			("YandexMusic", TrackSearchMode.YandexMusic.ToInt()),
		];
	}
}

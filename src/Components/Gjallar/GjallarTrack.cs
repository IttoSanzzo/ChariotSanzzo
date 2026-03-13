using DSharpPlus.Entities;
using Lavalink4NET.Tracks;

namespace ChariotSanzzo.Components.Gjallar {
	public record GjallarTrack {
		private static HttpClient HttpClient { get; set; } = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
		public LavalinkTrack Track { get; private set; }
		public DiscordMember? Member { get; set; }
		public string Host { get; set; }
		public TimeSpan LengthInSeconds { get; set; }
		public DiscordColor Color { get; set; }
		public string Favicon { get; set; }
		private string? Artwork { get; set; } = null;
		public Uri? Uri => Track.Uri;
		public TimeSpan Duration => Track.Duration;
		public string Author => Track.Author;
		public string Title => Track.Title;

		public GjallarTrack(LavalinkTrack track) {
			this.Track = track;
			this.Host = this.Uri!.Host;
			this.LengthInSeconds = TimeSpan.FromSeconds(this.Duration.TotalSeconds);
			(Color, Favicon) = Host switch {
				"youtube.com" or "www.youtube.com" => (DiscordColor.Red, "<:YoutubeIcon:1269684532777320448> "),
				"soundcloud.com" => (DiscordColor.Orange, "<:SoundCloudIcon:1269685534737825822> "),
				"open.spotify.com" => (DiscordColor.DarkGreen, "<:SpotifyIcon:1269685522528211004> "),
				_ => (DiscordColor.Purple, "")
			};
		}

		public async Task<DiscordEmbed> GetEmbedAsync(int? position = null) {
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(this.Color);
			string description = "";
			description += this.Favicon;
			embed.WithImageUrl(await this.GetArtworkAsync());
			if (Member is not null)
				embed.WithThumbnail(this.Member.AvatarUrl);
			description += $"_**Now Playing:**_ [{this.Title}]({this.Uri})\n" +
				$"**Author:** {this.Author}\n" +
				$"**Length:** {this.LengthInSeconds}";
			if (position != null)
				description += $"\t\t**Index:** ` {position} `";
			description += "\n";
			if (this.Member?.VoiceState != null) {
				string voiceChannelName = (await Member.VoiceState.GetChannelAsync())!.Name;
				description += $"**At:** {voiceChannelName}\n";
			}
			embed.WithDescription(description);
			return embed.Build();
		}
		public async Task<string> GetArtworkAsync() {
			if (this.Artwork != null)
				return this.Artwork;
			this.Artwork = await GetArtworkAsync(this.Uri!);
			return this.Artwork;
		}
		public static async Task<string> GetArtworkAsync(Uri uri) {
			string? artwork = null;
			try {
				switch (uri.Host) {
					case "youtube.com" or "www.youtube.com":
						var requestQuery = new HttpRequestMessage(HttpMethod.Head, new Uri($"https://img.youtube.com/vi/{uri.Query[3..]}/maxresdefault.jpg"));
						try {
							HttpResponseMessage response = await HttpClient.SendAsync(requestQuery);
							response.EnsureSuccessStatusCode();
							artwork = $"https://img.youtube.com/vi/{uri.Query[3..]}/maxresdefault.jpg";
						} catch {
							artwork = $"https://img.youtube.com/vi/{uri.Query[3..]}/default.jpg";
						}
						break;
					case "soundcloud.com":
						artwork = await SoundcloudApi.GetArtWorkAsync(uri);
						break;
					case "open.spotify.com":
						artwork = await SpotifyApi.GetArtWorkAsync(uri);
						break;
				}
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
			if (artwork == null)
				return "https://i.redd.it/dtljzwihuh861.jpg";
			return artwork;
		}
	}
}

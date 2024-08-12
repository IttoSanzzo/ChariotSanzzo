using System.Collections;
using System.Net;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicQueue {
	public class ChariotTrack {
	// 0. Member Variables
		private static HttpClient	_httpClient	{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		public LavalinkTrack		_llTrack	{get; set;}
		public DiscordUser			_user		{get; set;}
		public Uri					_uri		{get; set;}
		public string				_title		{get; set;}
		public string				_host		{get; set;}
		public string				_author		{get; set;}
		public TimeSpan				_length		{get; set;}
		public DiscordColor			_color		{get; set;}
		public string				_favicon	{get; set;}
		private string?				_artwork	{get; set;} = null;

	// 1. Constructor
		public ChariotTrack(LavalinkTrack llTrack, DiscordUser user) {
			this._llTrack = llTrack;
			this._user = user;
			this._title = this._llTrack.Title;
			this._uri = this._llTrack.Uri;
			this._host = this._uri.Host;
			this._author = this._llTrack.Author;
			this._length = this._llTrack.Length - TimeSpan.FromMilliseconds(this._llTrack.Length.Milliseconds) - TimeSpan.FromMicroseconds(this._llTrack.Length.Microseconds);
			switch (this._host) { // Chooses color and favicon based on the plataform
				case ("youtube.com"):
				case ("www.youtube.com"):
					this._color = DiscordColor.Red;
					this._favicon = "<:YoutubeIcon:1269684532777320448> ";
				break;
				case ("soundcloud.com"):
					this._color = DiscordColor.Orange;
					this._favicon = "<:SoundCloudIcon:1269685534737825822> ";
				break;
				case ("open.spotify.com"):
					this._color = DiscordColor.DarkGreen;
					this._favicon = "<:SpotifyIcon:1269685522528211004> ";
				break;
				default:
					this._color = DiscordColor.Purple;
					this._favicon = "";
				break;
			}
		}

	// 2. Embed
		public async Task<DiscordEmbed>	GetEmbed(int? index = null) {
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(this._color);
			string description = "";
			description += this._favicon;
			embed.WithImageUrl(await this.GetArtworkAsync());
			embed.WithThumbnail(this._user.AvatarUrl);
			description += $"_**Now Playing:**_ [{this._title}]({this._uri})\n" +
								$"**Author:** {this._author}\n" +
								$"**Length:** {this._length}";
			if (index != null)
				description += $"\t\t**Index:** ` {index + 1} `";
			description += "\n";
			embed.WithDescription(description);
			return (embed.Build());
		}
	// 3. Miscs
		public async Task<string>			GetArtworkAsync() {
			if (this._artwork != null)
				return (this._artwork);
			this._artwork = await ChariotTrack.GetArtworkAsync(this._uri);
			return (this._artwork);
		}
		public static async Task<string>	GetArtworkAsync(Uri uri) {
			string?	artwork = null;
			switch (uri.Host) {
				case ("youtube.com"):
				case ("www.youtube.com"):
					var requestQuery = new HttpRequestMessage(HttpMethod.Head, new Uri($"https://img.youtube.com/vi/{uri.Query.Substring(3)}/maxresdefault.jpg"));
					try {
						HttpResponseMessage response = await ChariotTrack._httpClient.SendAsync(requestQuery);
					    response.EnsureSuccessStatusCode();
						artwork = $"https://img.youtube.com/vi/{uri.Query.Substring(3)}/maxresdefault.jpg";
					}
					catch {
						artwork = $"https://img.youtube.com/vi/{uri.Query.Substring(3)}/default.jpg";
					}
				break;
				case ("soundcloud.com"):
				break;
				case ("open.spotify.com"):
					artwork = await Program.SpotifyConn.GetArtWorkAsync(uri);
				break;
			}
			if (artwork == null)
				return ("https://i.redd.it/dtljzwihuh861.jpg");
			return (artwork);
		}
	}
}

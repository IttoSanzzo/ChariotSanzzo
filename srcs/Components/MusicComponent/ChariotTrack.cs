using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

namespace ChariotSanzzo.Components.MusicComponent {
	public class ChariotTrack {
	// M. Member Variables
		private static HttpClient	HttpClient	{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		public LavalinkTrack			LlTrack			{get; set;}
		public DiscordUser				User				{get; set;}
		public Uri								Uri					{get; set;}
		public string							Title				{get; set;}
		public string							Host				{get; set;}
		public string							Author			{get; set;}
		public TimeSpan						Length			{get; set;}
		public DiscordColor				Color				{get; set;}
		public string							Favicon			{get; set;}
		private string?						Artwork			{get; set;} = null;

	// C. Constructor
		public ChariotTrack(LavalinkTrack llTrack, DiscordUser user) {
			this.LlTrack = llTrack;
			this.User = user;
			this.Title = this.LlTrack.Title;
			this.Uri = this.LlTrack.Uri;
			this.Host = this.Uri.Host;
			this.Author = this.LlTrack.Author;
			this.Length = this.LlTrack.Length - TimeSpan.FromMilliseconds(this.LlTrack.Length.Milliseconds) - TimeSpan.FromMicroseconds(this.LlTrack.Length.Microseconds);
			switch (this.Host) { // Chooses color and favicon based on the plataform
				case ("youtube.com"):
				case ("www.youtube.com"):
					this.Color = DiscordColor.Red;
					this.Favicon = "<:YoutubeIcon:1269684532777320448> ";
				break;
				case ("soundcloud.com"):
					this.Color = DiscordColor.Orange;
					this.Favicon = "<:SoundCloudIcon:1269685534737825822> ";
				break;
				case ("open.spotify.com"):
					this.Color = DiscordColor.DarkGreen;
					this.Favicon = "<:SpotifyIcon:1269685522528211004> ";
				break;
				default:
					this.Color = DiscordColor.Purple;
					this.Favicon = "";
				break;
			}
		}

	// 0. Embed
		public async Task<DiscordEmbed>	GetEmbed(int? index = null) {
			var embed = new DiscordEmbedBuilder();
			embed.WithColor(this.Color);
			string description = "";
			description += this.Favicon;
			embed.WithImageUrl(await this.GetArtworkAsync());
			embed.WithThumbnail(this.User.AvatarUrl);
			description += $"_**Now Playing:**_ [{this.Title}]({this.Uri})\n" +
								$"**Author:** {this.Author}\n" +
								$"**Length:** {this.Length}";
			if (index != null)
				description += $"\t\t**Index:** ` {index + 1} `";
			description += "\n";
			if (((DiscordMember)(this.User)).VoiceState != null)
				description += ("**At:** " + ((DiscordMember)(this.User)).VoiceState.Channel.Name) + "\n";
			embed.WithDescription(description);
			return (embed.Build());
		}
	// E. Miscs
		public async Task<string>					GetArtworkAsync() {
			if (this.Artwork != null)
				return (this.Artwork);
			this.Artwork = await ChariotTrack.GetArtworkAsync(this.Uri);
			return (this.Artwork);
		}
		public static async Task<string>	GetArtworkAsync(Uri uri) {
			string?	artwork = null;
			try {
				switch (uri.Host) {
					case ("youtube.com"):
					case ("www.youtube.com"):
						var requestQuery = new HttpRequestMessage(HttpMethod.Head, new Uri($"https://img.youtube.com/vi/{uri.Query.Substring(3)}/maxresdefault.jpg"));
						try {
							HttpResponseMessage response = await ChariotTrack.HttpClient.SendAsync(requestQuery);
						    response.EnsureSuccessStatusCode();
							artwork = $"https://img.youtube.com/vi/{uri.Query.Substring(3)}/maxresdefault.jpg";
						}
						catch {
							artwork = $"https://img.youtube.com/vi/{uri.Query.Substring(3)}/default.jpg";
						}
					break;
					case ("soundcloud.com"):
						artwork = await Program.SoundcloudConn.GetArtWorkAsync(uri);
					break;
					case ("open.spotify.com"):
						artwork = await Program.SpotifyConn.GetArtWorkAsync(uri);
					break;
				}
			} catch (Exception ex) {
				Program.WriteException(ex);
			}
			if (artwork == null)
				return ("https://i.redd.it/dtljzwihuh861.jpg");
			return (artwork);
		}
	}
}

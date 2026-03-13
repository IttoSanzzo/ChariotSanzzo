using System.Text;
using ChariotSanzzo.Infrastructure.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChariotSanzzo.Components.Gjallar {
	public static class SpotifyApi {
		// M. Member Variables
		private static HttpClient HttpClient { get; set; } = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
		public static string ClientId { get; private set; } = SpotifyConfig.ClientId;
		public static string ClientSecret { get; private set; } = SpotifyConfig.ClientSecret;
		public static string? AccessToken { get; private set; } = null;
		public static DateTime TimeSpanToken { get; private set; } = DateTime.Now;

		// 1. Mine
		public static async Task<string?> GetArtWorkAsync(Uri trackUri) {
			Program.WriteLine($"TrackSpotifyID: {trackUri.Segments[^1]}");
			string? jsonFetch = await FetchWebApiAsync("v1/tracks", (trackUri.Segments[^1]));
			if (jsonFetch == null)
				return null;
			var album = (JObject.Parse(jsonFetch)["album"]);
			if (album == null)
				return null;
			var images = (JObject.Parse(album.ToString())["images"]);
			if (images == null)
				return null;
			var artWorkLink = JsonConvert.DeserializeObject<List<SpotifyImage>>(images.ToString());
			if (artWorkLink == null)
				return null;
			return artWorkLink.First().Url;
		}

		// 2. Core
		private static async Task<string?> GetAccessTokenAsync() {
			// 0. Form HttpRequestMessage
			if (AccessToken == null || DateTime.Now > TimeSpanToken) {
				var requestQuery = new HttpRequestMessage(HttpMethod.Post, new Uri("https://accounts.spotify.com/api/token")) {
					Content = new StringContent($"grant_type=client_credentials&client_id={ClientId}&client_secret={ClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded")
				};

				// 1. Getting Response
				try {
					HttpResponseMessage response = await HttpClient.SendAsync(requestQuery);
					response.EnsureSuccessStatusCode();
					string jsonRet;
					using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
						jsonRet = await reader.ReadToEndAsync();
					AccessToken = ((string?)JObject.Parse(jsonRet)["access_token"]);
					TimeSpanToken = DateTime.Now.AddMinutes(58);
				} catch (HttpRequestException Ex) {
					Program.WriteLine("HttpError: " + Ex.Message);
					AccessToken = null;
				}
			}
			return AccessToken;
		}
		private static async Task<string?> FetchWebApiAsync(string endpoint, string? id) {
			// 0. Common Setup
			if (await GetAccessTokenAsync() == null)
				return null;
			string? fetchRet = null;

			// 1. Form HttpRequestMessage
			var requestQuery = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.spotify.com/{endpoint}" + ((id != null) ? ($"/{id}") : (null))));
			requestQuery.Headers.Add("Authorization", $"Bearer {AccessToken}");

			// 1. Getting Response
			try {
				HttpResponseMessage response = await HttpClient.SendAsync(requestQuery);
				response.EnsureSuccessStatusCode();
				using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
					fetchRet = await reader.ReadToEndAsync();
			} catch (HttpRequestException Ex) {
				Program.WriteLine("HttpError: " + Ex.Message);
				return null;
			}
			return fetchRet;
		}
	}
}

using System.Buffers.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChariotSanzzo.Components.SpotifyApi {
	public class SpotifyConn {
	// 0. Member Variables
		private static HttpClient	_httpClient		{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		public string				_ClientID		{get; private set;} = "";
		public string				_ClientSecret	{get; private set;} = "";
		public string?				_AccessToken	{get; private set;} = null;
		public DateTime				_TimeSpanToken	{get; private set;}

	// 1. Constructors
		public SpotifyConn() {
			this._TimeSpanToken = DateTime.Now;
		}
		public SpotifyConn(string clientID, string clientSecret) {
			this._ClientID = clientID;
			this._ClientSecret = clientSecret;
			this._TimeSpanToken = DateTime.Now;
		}

	// 1. RunInit
		public void	RunInit() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("spotifyAPIconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			string? tempID = config.GetValue<string>("AppData:ClientId");
			string? tempSecret = config.GetValue<string>("AppData:ClientSecret");
			if (tempID == null || tempSecret == null) {
				Console.WriteLine("Error: SpotifyConn: ClientID or ClientSecret null!");
				return ;
			}
			this._ClientID = tempID;
			this._ClientSecret = tempSecret;
		}

	// 2. Mine
		public async Task<string?>	GetArtWorkAsync(Uri trackUri) {
			Console.WriteLine($"TrackSpotifyID: {trackUri.Segments[^1]}");
			string? jsonFetch = await this.FetchWebApiAsync("v1/tracks", (trackUri.Segments[^1]));
			if (jsonFetch == null)
				return (null);
			var	album = (JObject.Parse(jsonFetch)["album"]);
			if (album == null)
				return (null);
			var images = (JObject.Parse(album.ToString())["images"]);
			if (images == null)
				return (null);
			var artWorkLink = JsonConvert.DeserializeObject<List<SpotifyApi.Image>>(images.ToString());
			if (artWorkLink == null)
				return (null);
			return (artWorkLink.First()._url);
		}

	// 3. Core
		private async Task<string?>	GetAccessTokenAsync() {
			// 0. Form HttpRequestMessage
			if (this._AccessToken == null || DateTime.Now > this._TimeSpanToken) {
				var requestQuery = new HttpRequestMessage(HttpMethod.Post, new Uri("https://accounts.spotify.com/api/token"));
				requestQuery.Content = new StringContent($"grant_type=client_credentials&client_id={this._ClientID}&client_secret={this._ClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded");

			// 1. Getting Response
				try {
					HttpResponseMessage response = await SpotifyConn._httpClient.SendAsync(requestQuery);
					response.EnsureSuccessStatusCode();
					string jsonRet;
					using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
					    jsonRet = await reader.ReadToEndAsync();
					this._AccessToken = ((string?)JObject.Parse(jsonRet)["access_token"]);
					this._TimeSpanToken = DateTime.Now.AddMinutes(58);
				}
				catch (HttpRequestException Ex) {
					Console.WriteLine("HttpError: " + Ex.Message);
					this._AccessToken = null;
				}
			}
			return (this._AccessToken);
		}
		private async Task<string?>	FetchWebApiAsync(string endpoint, string? id) {
		// 0. Common Setup
			if (await this.GetAccessTokenAsync() == null)
				return (null);
			string?	fetchRet = null;

		// 1. Form HttpRequestMessage
			var requestQuery = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.spotify.com/{endpoint}" + ((id != null) ? ($"/{id}") : (null))));
			requestQuery.Headers.Add("Authorization", $"Bearer {this._AccessToken}");

		// 1. Getting Response
			try {
				HttpResponseMessage response = await SpotifyConn._httpClient.SendAsync(requestQuery);
				response.EnsureSuccessStatusCode();
				using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
					fetchRet = await reader.ReadToEndAsync();
			}
			catch(HttpRequestException Ex) {
				Console.WriteLine("HttpError: " + Ex.Message);
				return (null);
			}
			return (fetchRet);
		}
	
	// 4. Utils
		private static string?	GetTrackId(string trackUrl) {
			// https://open.spotify.com/intl-pt/track/3UpHW2joNoKO2HfEv8Mchp
			int	i;
			for (i = trackUrl.Length - 1; i > 0; i--)
				if (trackUrl[i] == '/')
					break;
			Console.WriteLine($"TrackSpotifyID: {trackUrl.Substring(i + 1)}");
			return (trackUrl.Substring(i + 1));
		}
	}
}

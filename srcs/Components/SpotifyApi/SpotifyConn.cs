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
	// M. Member Variables
		private static HttpClient	HttpClient		{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		public string				ClientID		{get; private set;} = "";
		public string				ClientSecret	{get; private set;} = "";
		public string?				AccessToken		{get; private set;} = null;
		public DateTime				TimeSpanToken	{get; private set;}

	// C. Constructors
		public SpotifyConn() {
			this.RunInit();
			this.TimeSpanToken = DateTime.Now;
		}
		public SpotifyConn(string clientID, string clientSecret) {
			this.ClientID = clientID;
			this.ClientSecret = clientSecret;
			this.TimeSpanToken = DateTime.Now;
		}

	// 0. RunInit
		public void	RunInit() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("spotifyAPIconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			string? tempID = Environment.GetEnvironmentVariable("SPOTIFYAPIDATA_CLIENTID") ?? throw new InvalidOperationException("SPOTIFYAPIDATA_CLIENTID not set");
			string? tempSecret = Environment.GetEnvironmentVariable("SPOTIFYAPIDATA_CLIENTSECRET") ?? throw new InvalidOperationException("SPOTIFYAPIDATA_CLIENTSECRET not set");
			if (tempID == null || tempSecret == null) {
				Program.WriteLine("Error: SpotifyConn: ClientID or ClientSecret null!");
				return ;
			}
			this.ClientID = tempID;
			this.ClientSecret = tempSecret;
		}

	// 1. Mine
		public async Task<string?>	GetArtWorkAsync(Uri trackUri) {
			Program.WriteLine($"TrackSpotifyID: {trackUri.Segments[^1]}");
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
			return (artWorkLink.First().Url);
		}

	// 2. Core
		private async Task<string?>	GetAccessTokenAsync() {
			// 0. Form HttpRequestMessage
			if (this.AccessToken == null || DateTime.Now > this.TimeSpanToken) {
				var requestQuery = new HttpRequestMessage(HttpMethod.Post, new Uri("https://accounts.spotify.com/api/token"));
				requestQuery.Content = new StringContent($"grant_type=client_credentials&client_id={this.ClientID}&client_secret={this.ClientSecret}", Encoding.UTF8, "application/x-www-form-urlencoded");

			// 1. Getting Response
				try {
					HttpResponseMessage response = await SpotifyConn.HttpClient.SendAsync(requestQuery);
					response.EnsureSuccessStatusCode();
					string jsonRet;
					using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
					    jsonRet = await reader.ReadToEndAsync();
					this.AccessToken = ((string?)JObject.Parse(jsonRet)["access_token"]);
					this.TimeSpanToken = DateTime.Now.AddMinutes(58);
				}
				catch (HttpRequestException Ex) {
					Program.WriteLine("HttpError: " + Ex.Message);
					this.AccessToken = null;
				}
			}
			return (this.AccessToken);
		}
		private async Task<string?>	FetchWebApiAsync(string endpoint, string? id) {
		// 0. Common Setup
			if (await this.GetAccessTokenAsync() == null)
				return (null);
			string?	fetchRet = null;

		// 1. Form HttpRequestMessage
			var requestQuery = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://api.spotify.com/{endpoint}" + ((id != null) ? ($"/{id}") : (null))));
			requestQuery.Headers.Add("Authorization", $"Bearer {this.AccessToken}");

		// 1. Getting Response
			try {
				HttpResponseMessage response = await SpotifyConn.HttpClient.SendAsync(requestQuery);
				response.EnsureSuccessStatusCode();
				using (var reader = new StreamReader(await response.Content.ReadAsStreamAsync()))
					fetchRet = await reader.ReadToEndAsync();
			}
			catch(HttpRequestException Ex) {
				Program.WriteLine("HttpError: " + Ex.Message);
				return (null);
			}
			return (fetchRet);
		}
	}
}

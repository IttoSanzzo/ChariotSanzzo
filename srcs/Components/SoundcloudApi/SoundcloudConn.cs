using System.Buffers.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChariotSanzzo.Components.SoundcloudApi {
	public class SoundcloudConn {
	// M. Member Variables
		private static HttpClient	HttpClient	{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		public string				OAuthToken	{get; private set;} = "";
	// C. Constructors
		public SoundcloudConn() {
			this.RunInit();
		}
	
		public void	RunInit() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("SoundcloudAPIconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			string? oAuthToken = Environment.GetEnvironmentVariable("SOUNDCLOUDAPIDATA_OAUTHTOKEN") ?? throw new InvalidOperationException("SOUNDCLOUDAPIDATA_OAUTHTOKEN not set");
			if (oAuthToken == null) {
				Program.WriteLine("Error: SoundcloudConn: OAuthToken null!");
				return ;
			}
			this.OAuthToken = oAuthToken;
		}
	
	// G. Gets
		public async Task<string?>	GetArtWorkAsync(Uri trackUri) {
			string? jsonFetch = await this.FetchWebApiAsync(trackUri.AbsolutePath);
			if (jsonFetch == null)
				return (null);
			var sndcdnIndex = jsonFetch.IndexOf("https://i1.sndcdn.com/artworks-");
			if (sndcdnIndex == -1)
				return (null);
			string? subStr = jsonFetch.Substring(sndcdnIndex);
			if (subStr == null)
				return (null);
			int len = subStr.IndexOf(".jpg") + 4;
			return (subStr.Remove(len));
		}

	// 0. Core
		private async Task<string?>	FetchWebApiAsync(string endpoint) {
		// 0. Form HttpRequestMessage
			var requestQuery = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://soundcloud.com/{endpoint}"));
			requestQuery.Headers.Add("Accept", $"application/json; charset=utf-8");
			requestQuery.Headers.Add("Authorization", $"OAuth {this.OAuthToken}");
			string?	fetchRet = null;

		// 1. Getting Response
			try {
				HttpResponseMessage response = await SoundcloudConn.HttpClient.SendAsync(requestQuery);
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

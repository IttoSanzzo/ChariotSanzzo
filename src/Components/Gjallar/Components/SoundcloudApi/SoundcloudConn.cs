using ChariotSanzzo.Infrastructure.Config;

namespace ChariotSanzzo.Components.Gjallar {
	public static class SoundcloudApi {
		// M. Member Variables
		private static HttpClient HttpClient { get; set; } = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
		public static string OAuthToken { get; private set; } = SoundcloudConfig.OAuthToken;
		// C. Constructors

		// G. Gets
		public static async Task<string?> GetArtWorkAsync(Uri trackUri) {
			string? jsonFetch = await FetchWebApiAsync(trackUri.AbsolutePath);
			if (jsonFetch == null)
				return null;
			var sndcdnIndex = jsonFetch.IndexOf("https://i1.sndcdn.com/artworks-");
			if (sndcdnIndex == -1)
				return null;
			string? subStr = jsonFetch.Substring(sndcdnIndex);
			if (subStr == null)
				return null;
			int len = subStr.IndexOf(".jpg") + 4;
			return subStr.Remove(len);
		}

		// 0. Core
		private static async Task<string?> FetchWebApiAsync(string endpoint) {
			// 0. Form HttpRequestMessage
			var requestQuery = new HttpRequestMessage(HttpMethod.Get, new Uri($"https://soundcloud.com/{endpoint}"));
			requestQuery.Headers.Add("Accept", $"application/json; charset=utf-8");
			requestQuery.Headers.Add("Authorization", $"OAuth {OAuthToken}");
			string? fetchRet = null;

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

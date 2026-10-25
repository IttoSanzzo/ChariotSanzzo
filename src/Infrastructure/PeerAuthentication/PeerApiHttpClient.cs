using System.Text;
using System.Text.Json;

namespace PeerApiAuth {
	public static class PeerApiHttpClient {
		private static readonly HttpClient DefaultClient = new();

		public static async Task<HttpResponseMessage> SendAsync(
			HttpClient client,
			HttpMethod method,
			string url,
			object? body = null,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) {
			string bodyString = string.Empty;
			HttpContent? content = null;

			if (body is not null) {
				bodyString = body is string s ? s : JsonSerializer.Serialize(body);
				content = new StringContent(bodyString, Encoding.UTF8, "application/json");
			}

			var timestamp = PeerApiAuth.GenerateTimestamp();
			var bodyHash = PeerApiAuth.ComputeBodyHash(bodyString);
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString();

			var signature = PeerApiAuth.GenerateSignature(
				method.Method,
				path,
				timestamp,
				bodyHash
			);

			var request = new HttpRequestMessage(method, url) {
				Content = content
			};
			request.Headers.Add("X-Peer-Timestamp", timestamp);
			request.Headers.Add("X-Peer-Signature", signature);
			if (headers != null)
				foreach (var header in headers)
					request.Headers.Add(header.Item1, header.Item2);
			return await client.SendAsync(request, cancellationToken);
		}
		public static Task<HttpResponseMessage> SendAsync(
			HttpMethod method,
			string url,
			object? body = null,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) => SendAsync(DefaultClient, method, url, body, headers, cancellationToken);

		public static Task<HttpResponseMessage> GetAsync(
			string url,
			object? body,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) => SendAsync(HttpMethod.Get, url, body, headers, cancellationToken);
		public static Task<HttpResponseMessage> PostAsync(
			string url,
			object? body,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) => SendAsync(HttpMethod.Post, url, body, headers, cancellationToken);
		public static Task<HttpResponseMessage> PutAsync(
			string url,
			object? body,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) => SendAsync(HttpMethod.Put, url, body, headers, cancellationToken);
		public static Task<HttpResponseMessage> DeleteAsync(
			string url,
			object? body,
			(string, string)[]? headers = null,
			CancellationToken cancellationToken = default
		) => SendAsync(HttpMethod.Delete, url, body, headers, cancellationToken);
	}
}

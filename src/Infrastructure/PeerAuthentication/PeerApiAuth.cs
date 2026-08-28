using System.Security.Cryptography;
using System.Text;
using ChariotSanzzo.Infrastructure.Config;

namespace PeerApiAuth {
	public class PeerApiAuth {
		public const int DefaultAllowedTimeDrift = 30;

		public static string GenerateSignature(string method, string path, string timestamp, string bodyHash) {
			var payload = BuildPayload(method, path, timestamp, bodyHash);
			return ComputeHmac(payload);
		}
		public static bool ValidateSignature(string receivedSinature, string method, string path, string timestamp, string bodyHash) {
			if (!IsTimestampValid(timestamp))
				return false;
			var expected = GenerateSignature(method, path, timestamp, bodyHash);
			return SecureEquals(receivedSinature, expected);
		}

		public static string GenerateTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
		public static string ComputeBodyHash(string body) {
			if (String.IsNullOrEmpty(body))
				return string.Empty;
			var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(body));
			return Convert.ToHexString(bytes).ToLowerInvariant();
		}

		private static string BuildPayload(string method, string path, string timestamp, string bodyHash) => string.Join("|", method.ToUpperInvariant(), path, timestamp, bodyHash);
		private static string ComputeHmac(string payload) {
			using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(PeerApiConfig.Token));
			var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes((payload)));
			return Convert.ToHexString(hash).ToLowerInvariant();
		}
		private static bool IsTimestampValid(string timestamp) {
			if (!long.TryParse(timestamp, out var ts))
				return false;
			var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			var diff = Math.Abs(now - ts);
			return diff <= DefaultAllowedTimeDrift;
		}
		private static bool SecureEquals(string a, string b) {
			if (a.Length != b.Length)
				return false;
			var result = 0;
			for (int i = 0; i < a.Length; i++)
				result |= a[i] ^ b[i];
			return result == 0;
		}
	}
}

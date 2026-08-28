using System.Text;

namespace PeerApiAuth {
	public static class PeerApiAuthMiddleware {
		public static IApplicationBuilder UsePeerApiAuth(
			this IApplicationBuilder builder
		) {
			return builder.Use(async (context, next) => {
				var request = context.Request;
				if (!request.Headers.TryGetValue("X-Peer-Timestamp", out var timestamp)
					|| !request.Headers.TryGetValue("X-Peer-Signature", out var signature)
				) {
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Missing peer authentication headers.");
					return;
				}
				string body = string.Empty;
				if (request.ContentLength > 0 && request.Body.CanRead) {
					request.EnableBuffering();
					using var reader = new StreamReader(
						request.Body,
						Encoding.UTF8,
						leaveOpen: true
					);
					body = await reader.ReadToEndAsync();
					request.Body.Position = 0;
				}
				var bodyHash = PeerApiAuth.ComputeBodyHash(body);
				var isValid = PeerApiAuth.ValidateSignature(
					signature!,
					request.Method,
					request.Path.ToString(),
					timestamp!,
					bodyHash
				);
				if (!isValid) {
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					await context.Response.WriteAsync("Invalid peer signature.");
					return;
				}
				await next();
			});
		}
	}
}

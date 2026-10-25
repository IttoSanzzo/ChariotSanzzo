using System.Text;

namespace PeerApiAuth {
	public class PeerApiAuthFilter : IEndpointFilter {
		public async ValueTask<object?> InvokeAsync(
			EndpointFilterInvocationContext context,
			EndpointFilterDelegate next
		) {
			var http = context.HttpContext;
			var request = http.Request;


			if (!request.Headers.TryGetValue("X-Peer-Timestamp", out var timestamp)
					|| !request.Headers.TryGetValue("X-Peer-Signature", out var signature)) {
				return Results.Text(
					"Missing peer headers.",
					statusCode: StatusCodes.Status401Unauthorized
				);
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
				return Results.Text(
					"Invalid peer signature.",
					statusCode: StatusCodes.Status401Unauthorized
				);
			}

			return await next(context);
		}
	}
}

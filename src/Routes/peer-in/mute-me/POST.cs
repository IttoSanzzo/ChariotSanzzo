using ChariotSanzzo.Services;
using DSharpPlus;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("PeerIn MuteMe");
		private static async Task<IResult> Handler(HttpRequest request, DiscordClient client) {
			try {
				if (!request.Headers.TryGetValue("STP-DiscordUserId", out var discordUserIdRaw)
					|| !ulong.TryParse(discordUserIdRaw, out var discordUserId)
				) return Results.BadRequest(new { message = "Missing discordUserId" });
				var userPresenceState = await PresenceSentinel.ForceResolveVoiceAsync(discordUserId);
				if (userPresenceState == null)
					return Results.Ok(new { state = "disconnected" });
				if (!ulong.TryParse(userPresenceState.Get<string>("voice.channelId"), out var channelId))
					return Results.Ok(new { state = "disconnected" });
				var voiceChannel = await client.GetChannelAsync(channelId);
				if (voiceChannel == null!)
					return Results.Ok(new { state = "disconnected" });
				var user = voiceChannel.Users.FirstOrDefault((user) => user.Id == discordUserId);
				if (user! == null!)
					return Results.Ok(new { state = "disconnected" });
				if (user.VoiceState.IsServerMuted) {
					await user.SetMuteAsync(false);
					return Results.Ok(new { state = "unmuted" });
				}
				await user.SetMuteAsync(true);
				return Results.Ok(new { state = "muted" });
			} catch {
				return Results.BadRequest(new { message = "exception" });
			}
		}
	}
}

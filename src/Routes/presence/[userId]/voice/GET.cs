using ChariotSanzzo.Services;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Get PresenceSentinel User Voice");
		private static async Task<IResult> Handler(HttpContext context, ulong userId) {
			var state = await PresenceSentinel.Registry.GetOrCreate(userId);
			if (state.Get<string?>("voice.channelId") == null)
				await PresenceSentinel.ForceResolveVoiceAsync(userId);
			return Results.Ok(new {
				UserId = state.UserId,
				GuildId = state.Get<string>("voice.guildId"),
				GuildName = state.Get<string>("voice.guildName"),
				ChannelId = state.Get<string>("voice.channelId"),
				ChannelName = state.Get<string>("voice.channelName"),
				UpdatedAt = state.UpdatedAt
			});
		}
	}
}

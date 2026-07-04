using ChariotSanzzo.Components.Gjallar;
using ChariotSanzzo.Infrastructure.Config;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Get Guild Player Current State");
		private static async Task<IResult> Handler(string guildId, HttpContext context) {
			try {
				GjallarContext gtx = new();
				await gtx.GjallarContextUnsecureAsync(new() {
					GuildId = guildId,
					UserId = DiscordBotConfig.BotUserId.ToString(),
					Command = "StationSocketUpdateString"
				});
				return Results.Text((await gtx.TryCallingAsync()).Data as string, "application/json");
			} catch {
				return Results.Ok("");
			}
		}
	}
}

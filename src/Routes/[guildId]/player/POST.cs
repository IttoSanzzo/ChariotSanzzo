using ChariotSanzzo.Components.Gjallar;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Post Guild Player");
		private static async Task<IResult> Handler(string guildId, HttpContext context) {
			var payload = await context.Request.ReadFromJsonAsync<GjallarGenericCommand>();
			if (payload is null)
				return Results.BadRequest("Payload was null.");
			try {
				payload.GuildId = guildId;
				payload.TargetBot = "ChariotSanzzo";
				FunctionsSwitch(payload);
				return Results.Ok();
			} catch (Exception ex) {
				return Results.BadRequest($"Exception: {ex.Message}");
			}
		}
		private static async void FunctionsSwitch(GjallarGenericCommand genericCommand) {
			var gCtx = new GjallarContext();
			await gCtx.GjallarContextUnsecureAsync(genericCommand);
			gCtx.Data.WithResponse = false;
			await GjallarCaller.TryCallingAsync(gCtx);
		}
	}
}

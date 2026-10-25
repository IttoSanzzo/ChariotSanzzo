using ChariotSanzzo.Services;

namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Get PresenceSentinel User Stalk");
		private static async Task<IResult> Handler(HttpContext context, ulong userId) {
			return Results.Text(await PresenceSentinel.GetForceStalkeUserJsonStringAsync(userId), "text/plain");
		}
	}
}

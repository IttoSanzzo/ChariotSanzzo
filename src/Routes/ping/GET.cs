namespace ChariotSanzzo.Routes {
	file class Route() : WithFilePath(), IRoute {
		public Delegate Handle => Handler;
		public RouteHandlerBuilder Configure(RouteHandlerBuilder builder)
			=> builder.WithName("Ping");
		private static IResult Handler(HttpContext context) {
			return Results.Ok();
		}
	}
}

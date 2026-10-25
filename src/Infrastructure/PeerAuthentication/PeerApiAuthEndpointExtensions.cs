namespace PeerApiAuth {
	public static class PeerApiAuthEndpointExtensions {

		public static RouteHandlerBuilder RequirePeerApiAuth(
			this RouteHandlerBuilder builder
		) {
			builder.AddEndpointFilter<PeerApiAuthFilter>();
			return builder;
		}

		public static RouteGroupBuilder RequirePeerApiAuth(
			this RouteGroupBuilder builder
		) {
			builder.AddEndpointFilter<PeerApiAuthFilter>();
			return builder;
		}
	}
}

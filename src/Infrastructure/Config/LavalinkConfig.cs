namespace ChariotSanzzo.Infrastructure.Config {
	public static class LavalinkConfig {
		public static string Secured { get; internal set; } = null!;
		public static string Hostname { get; internal set; } = null!;
		public static string Password { get; internal set; } = null!;
		public static int Port { get; internal set; }
	}
}

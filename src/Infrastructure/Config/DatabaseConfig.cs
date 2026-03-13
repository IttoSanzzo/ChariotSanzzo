namespace ChariotSanzzo.Infrastructure.Config {
	public static class DatabaseConfig {
		public static string Hostname { get; internal set; } = null!;
		public static string Username { get; internal set; } = null!;
		public static string Password { get; internal set; } = null!;
		public static string Database { get; internal set; } = null!;
		public static string Port { get; internal set; } = null!;
	}
}

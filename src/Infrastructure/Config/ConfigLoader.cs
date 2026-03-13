using System.Reflection;

namespace ChariotSanzzo.Infrastructure.Config {
	public static class ConfigLoader {
		public static IConfiguration ConfigJson { get; internal set; }

		static ConfigLoader() {
			ConfigJson = new ConfigurationBuilder()
				.SetBasePath($"{Directory.GetCurrentDirectory()}/configs/")
				.AddJsonFile("config.json", optional: false)
				.AddEnvironmentVariables()
				.Build();
		}

		public static void Load() {
			PopulateConfig(typeof(DiscordBotConfig), "discord");
			PopulateConfig(typeof(LavalinkConfig), "lavalink");
			PopulateConfig(typeof(DatabaseConfig), "database");
			PopulateConfig(typeof(WebServConfig), "webServ");
			PopulateConfig(typeof(SpotifyConfig), "integrations:spotify");
			PopulateConfig(typeof(SoundcloudConfig), "integrations:soundcloud");
			PopulateConfig(typeof(ChariotApiConfig), "integrations:chariotApi");
			PopulateConfig(typeof(AddressesConfig), "addresses");
		}
		private static void PopulateConfig(Type type, string node) {
			var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
			foreach (var prop in props) {
				if (!prop.CanWrite)
					continue;
				var memberName = LowerFirst(prop.Name);
				var method = typeof(ConfigLoader)
					.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
					.First(m => m.Name == nameof(GetValue) && m.IsGenericMethodDefinition);
				var generic = method.MakeGenericMethod(prop.PropertyType);
				var value = generic.Invoke(null, [$"{node}:{memberName}"]);
				prop.SetValue(null, value);
			}
		}
		private static string LowerFirst(string s) => char.ToLowerInvariant(s[0]) + s[1..];
		private static T GetValue<T>(string node) {
			var value = ConfigJson.GetValue<T>(node);
			if (EqualityComparer<T>.Default.Equals(value, default!))
				throw new Exception($"Failed to load config.json (Missing '{node}')");
			return value ?? throw new Exception($"Failed to load config.json (Missing '{node}')");
		}
	}
}

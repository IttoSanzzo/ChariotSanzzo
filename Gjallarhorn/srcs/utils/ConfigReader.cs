using Microsoft.Extensions.Configuration;

namespace Gjallarhorn.Config {
	internal class ConfigReader {
		public string?	_name {get; set;}
		public string?	_prefix {get; set;}
		public string?	_token {get; set;}
		public ConfigReader() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("appconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			this._name = config.GetValue<string>("BotData:Name");
			this._prefix = config.GetValue<string>("BotData:Prefix");
			this._token = config.GetValue<string>("BotData:BotToken");
		}
		public string GetPrefix() {
			if (string.IsNullOrWhiteSpace(this._prefix))
				return ("");
			return (this._prefix);
		}
	}
}

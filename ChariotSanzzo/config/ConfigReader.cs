using Microsoft.Extensions.Configuration;

namespace ChariotSanzzo.config {
	internal class ConfigReader {
		public string?	_name {get; set;}
		public string?	_prefix {get; set;}
		public string?	_token {get; set;}
		public ConfigReader() {
			/*
			using (StreamReader sr = new StreamReader("config.json")) {
				string json = await sr.ReadToEndAsync();
			}
			*/
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/config/")
			.AddJsonFile("appconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			this._name = config.GetValue<string>("BotData:Name");
			this._prefix = config.GetValue<string>("BotData:Prefix");
			this._token = config.GetValue<string>("BotData:BotToken");
		}
	}
	/*
	internal sealed class ConfigStruct {
		public string	_token {get; set;}
		public string	_prefix {get; set;}
	}
	*/
}
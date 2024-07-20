using Microsoft.Extensions.Configuration;

namespace ChariotSanzzo.Config {
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
	internal class DBConfig {
		public string	_conn {get; private set;} = "";
		public string?	_hostname {get; private set;}
		public string?	_username {get; private set;}
		public string?	_password {get; private set;}
		public string?	_database {get; private set;}
		public string?	_port {get; private set;}
		public DBConfig() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("dbconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			this._hostname = config.GetValue<string>("DBdata:Hostname");
			this._username = config.GetValue<string>("DBdata:Username");
			this._password = config.GetValue<string>("DBdata:Password");
			this._database = config.GetValue<string>("DBdata:Database");
			this._port = config.GetValue<string>("DBdata:Port");
			if (this._hostname == null ||
				this._username == null ||
				this._password == null ||
				this._database == null)
				return ;
			this._conn = $"Host={this._hostname};Username={this._username};Password={this._password};Database={this._database}";
		}
	}
}

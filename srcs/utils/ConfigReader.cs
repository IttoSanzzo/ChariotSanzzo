using Microsoft.Extensions.Configuration;

namespace ChariotSanzzo.Config {
	internal class ConfigReader {
	// M. Member Variables
		public string?	Name {get; set;}
		public string?	Prefix {get; set;}
		public string?	Token {get; set;}

	// C. Constructor
		public ConfigReader() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("appconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			this.Name = config.GetValue<string>("BotData:Name");
			this.Prefix = config.GetValue<string>("BotData:Prefix");
			this.Token = config.GetValue<string>("BotData:BotToken");
		}
		
	// 0. Utils
		public string GetPrefix() {
			if (string.IsNullOrWhiteSpace(this.Prefix))
				return ("");
			return (this.Prefix);
		}
	}
	internal class DBConfig {
	// M. Member Variables
		public static string	Conn		{get; private set;} = "";
		public string?			Hostname	{get; private set;}
		public string?			Username	{get; private set;}
		public string?			Password	{get; private set;}
		public string?			Database	{get; private set;}
		public string?			Port		{get; private set;}

	// C. Constructor
		public DBConfig() {
			var	builder = new ConfigurationBuilder()
			.SetBasePath($"{Directory.GetCurrentDirectory()}/Config/")
			.AddJsonFile("dbconfig.json", optional: true, reloadOnChange: true)
			.AddUserSecrets<Program>();
			IConfiguration config = builder.Build();
			this.Hostname = config.GetValue<string>("DBdata:Hostname");
			this.Username = config.GetValue<string>("DBdata:Username");
			this.Password = config.GetValue<string>("DBdata:Password");
			this.Database = config.GetValue<string>("DBdata:Database");
			this.Port = config.GetValue<string>("DBdata:Port");
			if (this.Hostname == null ||
				this.Username == null ||
				this.Password == null ||
				this.Database == null)
				return ;
			DBConfig.Conn = $"Host={this.Hostname};Username={this.Username};Password={this.Password};Database={this.Database}";
		}
	}
}

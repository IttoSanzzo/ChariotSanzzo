namespace ChariotSanzzo.Utils {
	public static class LinkData {
		private static string	CurrentPublicIp								{get; set;} = null!;
		private static string	AlbinaApiFullAddress					{get; set;} = Environment.GetEnvironmentVariable("ALBINA_API_ADDRESS") ?? throw new InvalidOperationException("ALBINA_API_ADDRESS not set");
		private static string	AlbinaApiPort									{get; set;} = Environment.GetEnvironmentVariable("ALBINA_API_PORT") ?? throw new InvalidOperationException("ALBINA_API_PORT not set");
		private static string	AlbinaSiteFullAddress					{get; set;} = Environment.GetEnvironmentVariable("ALBINA_SITE_ADDRESS") ?? throw new InvalidOperationException("ALBINA_SITE_ADDRESS not set");
		private static string	AlbinaSitePort								{get; set;} = Environment.GetEnvironmentVariable("ALBINA_SITE_PORT") ?? throw new InvalidOperationException("ALBINA_SITE_PORT not set");
		private static string	GjallarhornControlFullAddress	{get; set;} = Environment.GetEnvironmentVariable("GJALLARHORNCONTROL_ADDRESS") ?? throw new InvalidOperationException("ALBINA_SITE_ADDRESS not set");
		private static string	GjallarhornControlPort				{get; set;} = Environment.GetEnvironmentVariable("GJALLARHORNCONTROL_PORT") ?? throw new InvalidOperationException("ALBINA_SITE_PORT not set");

		public static async Task	SetAll() {
			try {
				using var httpClient = new HttpClient();
				LinkData.CurrentPublicIp = await httpClient.GetStringAsync("https://api.ipify.org");
			} catch {
				Console.WriteLine("!!! Error while setting CurrentPublicIp!");
			}
		}
		public static string	GetCurrentPublicIp() {
			return CurrentPublicIp;
		}
		public static string	GetAlbinaApiPort() {
			return AlbinaApiPort;
		}
		public static string	GetAlbinaApiFullAdress(string? endpoint = null) {
			if (endpoint != null)
				return AlbinaApiFullAddress + endpoint;
			return AlbinaApiFullAddress;
		}
		public static string	GetAlbinaSitePort() {
			return AlbinaSitePort;
		}
		public static string	GetAlbinaSiteFullAdress(string? endpoint = null) {
			if (endpoint != null)
				return AlbinaSiteFullAddress + endpoint;
			return AlbinaSiteFullAddress;
		}
		public static string	GetGjallarhornControlPort() {
			return GjallarhornControlPort;
		}
		public static string	GetGjallarhornControlFullAdress(string? endpoint = null) {
			if (endpoint != null)
				return GjallarhornControlFullAddress + endpoint;
			return GjallarhornControlFullAddress;
		}
	}
}
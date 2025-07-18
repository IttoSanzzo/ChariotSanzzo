namespace ChariotSanzzo.Utils {
	public static class LinkData {
		private static string	Host										{get; set;} = null!;
		private static string	AlbinaApiPort						{get; set;} = Environment.GetEnvironmentVariable("ALBINA_API_PORT") ?? throw new InvalidOperationException("ALBINA_API_PORT not set");
		private static string	AlbinaOnlinePort				{get; set;} = Environment.GetEnvironmentVariable("ALBINA_ONLINE_PORT") ?? throw new InvalidOperationException("ALBINA_ONLINE_PORT not set");
		private static string	AlbinaApiBaseAddress		{get; set;} = null!;
		private static string	ApiFullAddress					{get; set;} = null!;
		private static string	AlbinaOnlineBaseAddress	{get; set;} = null!;
		private static string	AlbinaOnlineFullAddress	{get; set;} = null!;

		public static async Task	SetAll() {
			try {
				using var httpClient = new HttpClient();
				LinkData.Host = await httpClient.GetStringAsync("https://api.ipify.org");
				LinkData.AlbinaApiBaseAddress = Host + ":" + AlbinaApiPort;
				LinkData.ApiFullAddress = "http://" + AlbinaApiBaseAddress;
				LinkData.AlbinaOnlineBaseAddress = Host + ":" + AlbinaOnlinePort;
				LinkData.AlbinaOnlineFullAddress = "http://" + AlbinaOnlineBaseAddress;
			} catch {
				Console.WriteLine("!!! Error while setting Base Addresses (using localhost)!");
				LinkData.AlbinaApiBaseAddress = "localhost:" + AlbinaApiPort;
				LinkData.ApiFullAddress = "http://localhost:" + AlbinaApiPort;
				LinkData.AlbinaOnlineBaseAddress = "localhost:" + AlbinaOnlinePort;
				LinkData.AlbinaOnlineFullAddress = "http://localhost:" + AlbinaOnlinePort;
			}
		}
		public static string	GetHost() {
			return Host;
		}
		public static string	GetApiPort() {
			return AlbinaApiPort;
		}
		public static string	GetApiBaseAdress() {
			return AlbinaApiBaseAddress;
		}
		public static string	GetApiFullAdress() {
			return ApiFullAddress;
		}
		public static string	GetAlbinaOnlinePort() {
			return AlbinaOnlinePort;
		}
		public static string	GetAlbinaOnlineBaseAdress() {
			return AlbinaOnlineBaseAddress;
		}
		public static string	GetAlbinaOnlineFullAdress() {
			return AlbinaOnlineFullAddress;
		}
	}
}
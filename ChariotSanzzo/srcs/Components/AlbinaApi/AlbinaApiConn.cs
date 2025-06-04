using System.Text.Json;
using System.Text.Json.Serialization;
using ChariotSanzzo.Components.AlbinaApi.DTOs;

namespace ChariotSanzzo.Components.AlbinaApi {
	public class AlbinaConn {
		private static HttpClient							HttpClient							{get; set;} = new HttpClient(new SocketsHttpHandler {PooledConnectionLifetime = TimeSpan.FromMinutes(1)});
		private static string									HostAddress							{get; set;} = Environment.GetEnvironmentVariable("ALBINA_API") ?? throw new InvalidOperationException("ALBINA_API not set");
		private static JsonSerializerOptions	JsonSerializerOptions		{get; set;} = new JsonSerializerOptions() {
			PropertyNameCaseInsensitive = true,
			Converters = {
				new JsonStringEnumConverter()
			}
		};

		public string										GetHostAddress() {
			return AlbinaConn.HostAddress;
		}

		public async Task<string?>			GetAsync(string endpoint) {
			try {
				return await AlbinaConn.HttpClient.GetStringAsync($"{AlbinaConn.HostAddress}/{endpoint}");
			} catch(HttpRequestException ex) {
				Program.WriteLine("HttpError: " + ex.Message);
				return null;
			}
		}
		public async Task<MasteryDto>		GetMasteryAsync(string slug) {
			try {
				var json = await this.GetAsync($"masteries/{slug}");
				return JsonSerializer.Deserialize<MasteryDto>(json ?? "", AlbinaConn.JsonSerializerOptions) ?? new MasteryDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new MasteryDto();
			}
		}
		public async Task<MasteryDto[]>	GetAllMasteriesAsync() {
			try {
				var json = await this.GetAsync($"masteries");
				return JsonSerializer.Deserialize<MasteryDto[]>(json ?? "", AlbinaConn.JsonSerializerOptions) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<ItemDto>			GetItemAsync(string slug) {
			try {
				var json = await this.GetAsync($"items/{slug}");
				return JsonSerializer.Deserialize<ItemDto>(json ?? "", AlbinaConn.JsonSerializerOptions) ?? new ItemDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new ItemDto();
			}
		}
		public async Task<ItemDto[]>		GetAllItemsAsync() {
			try {
				var json = await this.GetAsync($"items");
				return JsonSerializer.Deserialize<ItemDto[]>(json ?? "", AlbinaConn.JsonSerializerOptions) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
	}
}
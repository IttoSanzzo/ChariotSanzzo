using System.Text.Json;
using System.Text.Json.Serialization;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using ChariotSanzzo.Utils;

namespace ChariotSanzzo.Components.AlbinaApi {
	public static class AlbinaConn {
		private static HttpClient HttpClient { get; set; } = new HttpClient(new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(1) });
		private static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions() {
			PropertyNameCaseInsensitive = true,
			Converters = {
				new JsonStringEnumConverter()
			}
		};

		public static async Task<string?> GetAsync(string endpoint) {
			try {
				return await AlbinaConn.HttpClient.GetStringAsync(LinkData.GetAlbinaApiFullAddress(endpoint));
			} catch (HttpRequestException ex) {
				Program.WriteLine("HttpError: " + ex.Message);
				return null;
			}
		}

		public static async Task<MasteryDto> GetMasteryAsync(string slug) {
			try {
				var json = await GetAsync($"/masteries/{slug}");
				return AlbinaConn.Deserialize<MasteryDto>(json) ?? new MasteryDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new MasteryDto();
			}
		}
		public static async Task<MasteryDto[]> GetAllMasteriesAsync() {
			try {
				var json = await GetAsync($"/masteries");
				return AlbinaConn.Deserialize<MasteryDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}
		public static async Task<ItemDto> GetItemAsync(string slug) {
			try {
				var json = await GetAsync($"/items/{slug}");
				return AlbinaConn.Deserialize<ItemDto>(json) ?? new ItemDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new ItemDto();
			}
		}
		public static async Task<ItemDto[]> GetAllItemsAsync() {
			try {
				var json = await GetAsync($"/items");
				return AlbinaConn.Deserialize<ItemDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}
		public static async Task<SkillDto> GetSkillAsync(string slug) {
			try {
				var json = await GetAsync($"/skills/{slug}");
				return AlbinaConn.Deserialize<SkillDto>(json) ?? new SkillDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new SkillDto();
			}
		}
		public static async Task<SkillDto[]> GetAllSkillsAsync() {
			try {
				var json = await GetAsync($"/skills");
				return AlbinaConn.Deserialize<SkillDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}
		public static async Task<SpellDto> GetSpellAsync(string slug) {
			try {
				var json = await GetAsync($"/spells/{slug}");
				return AlbinaConn.Deserialize<SpellDto>(json) ?? new SpellDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new SpellDto();
			}
		}
		public static async Task<SpellDto[]> GetAllSpellsAsync() {
			try {
				var json = await GetAsync($"/spells");
				return AlbinaConn.Deserialize<SpellDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}
		public static async Task<TraitDto> GetTraitAsync(string slug) {
			try {
				var json = await GetAsync($"/traits/{slug}");
				return AlbinaConn.Deserialize<TraitDto>(json) ?? new TraitDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new TraitDto();
			}
		}
		public static async Task<TraitDto[]> GetAllTraitsAsync() {
			try {
				var json = await GetAsync($"/traits");
				return AlbinaConn.Deserialize<TraitDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}
		public static async Task<RaceDto> GetRaceAsync(string slug) {
			try {
				var json = await GetAsync($"/races/{slug}");
				return AlbinaConn.Deserialize<RaceDto>(json) ?? new RaceDto();
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return new RaceDto();
			}
		}
		public static async Task<RaceDto[]> GetAllRacesAsync() {
			try {
				var json = await GetAsync($"/races");
				return AlbinaConn.Deserialize<RaceDto[]>(json) ?? [];
			} catch (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
				return [];
			}
		}

		private static T? Deserialize<T>(string? json) {
			return JsonSerializer.Deserialize<T>(json ?? "", AlbinaConn.JsonSerializerOptions);
		}
	}
}

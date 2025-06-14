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
				return AlbinaConn.Deserialize<MasteryDto>(json) ?? new MasteryDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new MasteryDto();
			}
		}
		public async Task<MasteryDto[]>	GetAllMasteriesAsync() {
			try {
				var json = await this.GetAsync($"masteries");
				return AlbinaConn.Deserialize<MasteryDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<ItemDto>			GetItemAsync(string slug) {
			try {
				var json = await this.GetAsync($"items/{slug}");
				return AlbinaConn.Deserialize<ItemDto>(json) ?? new ItemDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new ItemDto();
			}
		}
		public async Task<ItemDto[]>		GetAllItemsAsync() {
			try {
				var json = await this.GetAsync($"items");
				return AlbinaConn.Deserialize<ItemDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<SkillDto>			GetSkillAsync(string slug) {
			try {
				var json = await this.GetAsync($"skills/{slug}");
				return AlbinaConn.Deserialize<SkillDto>(json) ?? new SkillDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new SkillDto();
			}
		}
		public async Task<SkillDto[]>		GetAllSkillsAsync() {
			try {
				var json = await this.GetAsync($"skills");
				return AlbinaConn.Deserialize<SkillDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<SpellDto>			GetSpellAsync(string slug) {
			try {
				var json = await this.GetAsync($"spells/{slug}");
				return AlbinaConn.Deserialize<SpellDto>(json) ?? new SpellDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new SpellDto();
			}
		}
		public async Task<SpellDto[]>		GetAllSpellsAsync() {
			try {
				var json = await this.GetAsync($"spells");
				return AlbinaConn.Deserialize<SpellDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<TraitDto>			GetTraitAsync(string slug) {
			try {
				var json = await this.GetAsync($"traits/{slug}");
				return AlbinaConn.Deserialize<TraitDto>(json) ?? new TraitDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new TraitDto();
			}
		}
		public async Task<TraitDto[]>		GetAllTraitsAsync() {
			try {
				var json = await this.GetAsync($"traits");
				return AlbinaConn.Deserialize<TraitDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
		public async Task<RaceDto>			GetRaceAsync(string slug) {
			try {
				var json = await this.GetAsync($"races/{slug}");
				return AlbinaConn.Deserialize<RaceDto>(json) ?? new RaceDto();
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return new RaceDto();
			}
		}
		public async Task<RaceDto[]>		GetAllRacesAsync() {
			try {
				var json = await this.GetAsync($"races");
				return AlbinaConn.Deserialize<RaceDto[]>(json) ?? [];
			} catch  (JsonException ex) {
				Program.WriteLine("JsonError: " + ex.Message);
    		return [];
			}
		}
	
		private static T?								Deserialize<T>(string? json) {
			return JsonSerializer.Deserialize<T>(json ?? "", AlbinaConn.JsonSerializerOptions);
		}
	}
}
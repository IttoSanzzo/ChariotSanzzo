using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash.AutoCompleteProviders {
	public class RaceAutoCompleteProvider : IAutocompleteProvider {
		private static DateTime			LastFetchTime = DateTime.MinValue;
		private static RaceDto[]		CachedRaces = [];

		public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedRaces = await Program.AlbinaConn.GetAllRacesAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.OptionValue?.ToString() ?? "";
			return CachedRaces
				.Where(race => race.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(race => new DiscordAutoCompleteChoice(race.Name, race.Slug));
		}
	}
}
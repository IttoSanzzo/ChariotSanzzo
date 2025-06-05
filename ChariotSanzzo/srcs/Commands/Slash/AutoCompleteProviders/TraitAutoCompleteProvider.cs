using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash.AutoCompleteProviders {
	public class TraitAutoCompleteProvider : IAutocompleteProvider {
		private static DateTime			LastFetchTime = DateTime.MinValue;
		private static TraitDto[]		CachedTraits = [];

		public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedTraits = await Program.AlbinaConn.GetAllTraitsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.OptionValue?.ToString() ?? "";
			return CachedTraits
				.Where(trait => trait.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(trait => new DiscordAutoCompleteChoice(trait.Name, trait.Slug));
		}
	}
}
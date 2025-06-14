using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash.AutoCompleteProviders {
	public class SpellAutoCompleteProvider : IAutocompleteProvider {
		private static DateTime			LastFetchTime = DateTime.MinValue;
		private static SpellDto[]		CachedSpells = [];

		public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedSpells = await Program.AlbinaConn.GetAllSpellsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.OptionValue?.ToString() ?? "";
			return CachedSpells
				.Where(spell => spell.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(spell => new DiscordAutoCompleteChoice(spell.Name, spell.Slug));
		}
	}
}
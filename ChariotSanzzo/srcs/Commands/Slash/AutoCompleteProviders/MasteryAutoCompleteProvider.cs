using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash.AutoCompleteProviders {
	public class MasteryAutoCompleteProvider : IAutocompleteProvider {
		private static DateTime			LastFetchTime = DateTime.MinValue;
		private static MasteryDto[]	CachedMasteries = [];

		public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedMasteries = await Program.AlbinaConn.GetAllMasteriesAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.OptionValue?.ToString() ?? "";
			return CachedMasteries
				.Where(mastery => mastery.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(mastery => new DiscordAutoCompleteChoice(mastery.Name, mastery.Slug));
		}
	}
}
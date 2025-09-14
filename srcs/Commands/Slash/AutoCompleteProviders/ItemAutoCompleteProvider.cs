using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash.AutoCompleteProviders {
	public class ItemAutoCompleteProvider : IAutocompleteProvider {
		private static DateTime			LastFetchTime = DateTime.MinValue;
		private static ItemDto[]		CachedItems = [];

		public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedItems = await Program.AlbinaConn.GetAllItemsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.OptionValue?.ToString() ?? "";
			return CachedItems
				.Where(item => item.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(item => new DiscordAutoCompleteChoice(item.Name, item.Slug));
		}
	}
}
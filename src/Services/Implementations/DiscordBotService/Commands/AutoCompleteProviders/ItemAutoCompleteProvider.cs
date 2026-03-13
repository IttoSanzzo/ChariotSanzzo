using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.AutoCompleteProviders {
	public class ItemAutoCompleteProvider : IAutoCompleteProvider {
		private static DateTime LastFetchTime = DateTime.MinValue;
		private static ItemDto[] CachedItems = [];

		public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedItems = await AlbinaConn.GetAllItemsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.UserInput?.ToString() ?? "";
			return CachedItems
				.Where(item => item.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(item => new DiscordAutoCompleteChoice(item.Name, item.Slug));
		}
	}
}

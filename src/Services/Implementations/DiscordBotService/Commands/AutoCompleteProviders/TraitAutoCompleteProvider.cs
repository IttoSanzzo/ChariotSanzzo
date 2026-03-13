using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.AutoCompleteProviders {
	public class TraitAutoCompleteProvider : IAutoCompleteProvider {
		private static DateTime LastFetchTime = DateTime.MinValue;
		private static TraitDto[] CachedTraits = [];

		public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedTraits = await AlbinaConn.GetAllTraitsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.UserInput?.ToString() ?? "";
			return CachedTraits
				.Where(trait => trait.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(trait => new DiscordAutoCompleteChoice(trait.Name, trait.Slug));
		}
	}
}

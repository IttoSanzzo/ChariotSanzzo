using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.AutoCompleteProviders {
	public class SpellAutoCompleteProvider : IAutoCompleteProvider {
		private static DateTime LastFetchTime = DateTime.MinValue;
		private static SpellDto[] CachedSpells = [];

		public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedSpells = await AlbinaConn.GetAllSpellsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.UserInput?.ToString() ?? "";
			return CachedSpells
				.Where(spell => spell.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(spell => new DiscordAutoCompleteChoice(spell.Name, spell.Slug));
		}
	}
}

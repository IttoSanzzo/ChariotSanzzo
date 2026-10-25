using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.AutoCompleteProviders {
	public class MasteryAutoCompleteProvider : IAutoCompleteProvider {
		private static DateTime LastFetchTime = DateTime.MinValue;
		private static MasteryDto[] CachedMasteries = [];

		public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedMasteries = await AlbinaConn.GetAllMasteriesAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.UserInput?.ToString() ?? "";
			return CachedMasteries
				.Where(mastery => mastery.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(mastery => new DiscordAutoCompleteChoice(mastery.Name, mastery.Slug));
		}
	}
}

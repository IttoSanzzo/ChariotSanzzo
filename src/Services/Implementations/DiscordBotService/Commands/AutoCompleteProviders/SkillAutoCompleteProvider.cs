using ChariotSanzzo.Components.AlbinaApi;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace ChariotSanzzo.Services.Commands.AutoCompleteProviders {
	public class SkillAutoCompleteProvider : IAutoCompleteProvider {
		private static DateTime LastFetchTime = DateTime.MinValue;
		private static SkillDto[] CachedSkills = [];

		public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext ctx) {
			if ((DateTime.UtcNow - LastFetchTime).TotalMinutes > 10) {
				try {
					CachedSkills = await AlbinaConn.GetAllSkillsAsync();
					LastFetchTime = DateTime.UtcNow;
				} catch (Exception ex) {
					Program.WriteLine("Autocomplete provider error: " + ex.Message);
				}
			}
			var input = ctx.UserInput?.ToString() ?? "";
			return CachedSkills
				.Where(skill => skill.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
				.Take(25)
				.Select(skill => new DiscordAutoCompleteChoice(skill.Name, skill.Slug));
		}
	}
}

using System.Text;
using ChariotSanzzo.Commands.Slash.AutoCompleteProviders;
using ChariotSanzzo.Components.AlbinaApi.Common;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("AlbinaInfo", "Commands for fetching Albina related info.")]
	public class AlbinaInfoCommands : ApplicationCommandModule {
		private static string	AlbinaOnlineAddress	{get; set;} = Environment.GetEnvironmentVariable("ALBINA_ONLINE") ?? throw new InvalidOperationException("ALBINA_ONLINE not set");

		[SlashCommand("Mastery", "Fetchs the info from a given mastery.")]
		public static async Task	FetchMastery(InteractionContext ctx, [Option("Mastery", "The mastery to fetch")] [Autocomplete(typeof(MasteryAutoCompleteProvider))] string masterySlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();

			MasteryDto mastery = await Program.AlbinaConn.GetMasteryAsync(masterySlug);
			if (mastery.Id == Guid.Empty) {
				embed.WithDescription("Mastery not found.");
			} else {
				// embed.WithTitle($"Maestria - [{mastery.Name}]({AlbinaInfoCommands.AlbinaOnlineAddress}/maestrias/{mastery.Slug})");
				// embed.WithTitle($"Maestria - [{mastery.Name}]({AlbinaInfoCommands.AlbinaOnlineAddress}/maestrias/{mastery.Slug})");
				embed.WithThumbnail(mastery.IconUrl);
				embed.WithFooter($"{mastery.Type} - {mastery.SubType}");
				var descriptionBuilder = new StringBuilder($"**Maestria - [{mastery.Name}]({AlbinaInfoCommands.AlbinaOnlineAddress}/maestrias/{mastery.Slug})**\n\n");

				if (mastery.Info.Summary.Length > 0) {
					descriptionBuilder.AppendLine("**Resumo:**");
					foreach(var line in mastery.Info.Summary)
						descriptionBuilder.AppendLine($"- {line}");
					descriptionBuilder.AppendLine();
				}
				if (mastery.Info.Description.Length > 0) {
					descriptionBuilder.AppendLine("**Descrição:**");
					foreach(var line in mastery.Info.Description)
						descriptionBuilder.AppendLine($"- {line}");
					descriptionBuilder.AppendLine();
				}
				if (mastery.Effects.Length > 0) {
					descriptionBuilder.AppendLine("**Efeitos:**");
					foreach(var effect in mastery.Effects) {
						descriptionBuilder.AppendLine($"🎯 **[{effect.Role}] - {effect.Name}**");
						foreach (var content in effect.Contents) {
							if (content.Type == EffectContentType.Quote)
								descriptionBuilder.AppendLine($"*{content.Value}*");
							else
								descriptionBuilder.AppendLine(content.Value);
						}
					}
					descriptionBuilder.AppendLine();
				}
				embed.WithDescription(descriptionBuilder.ToString().Trim());
			}
			await ctx.RespondWithEmbedAsync(60, embed);
		}


		[SlashCommand("Item", "Fetchs the info from a given item.")]
		public static async Task	FetchItem(InteractionContext ctx, [Option("Item", "The item to fetch")] [Autocomplete(typeof(ItemAutoCompleteProvider))] string itemSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();

			ItemDto item = await Program.AlbinaConn.GetItemAsync(itemSlug);
			if (item.Id == Guid.Empty) {
				embed.WithDescription("Mastery not found.");
			} else {
				// embed.WithTitle($"Maestria - [{item.Name}]({AlbinaInfoCommands.AlbinaOnlineAddress}/items/{item.Slug})");
				embed.WithThumbnail(item.IconUrl);
				embed.WithFooter($"{item.Type} - {item.SubType}");
				var descriptionBuilder = new StringBuilder($"**Maestria - [{item.Name}]({AlbinaInfoCommands.AlbinaOnlineAddress}/items/{item.Slug})**\n\n");

				if (item.Info.Summary.Length > 0) {
					descriptionBuilder.AppendLine("**Resumo:**");
					foreach(var line in item.Info.Summary)
						descriptionBuilder.AppendLine($"- {line}");
					descriptionBuilder.AppendLine();
				}
				if (item.Info.Description.Length > 0) {
					descriptionBuilder.AppendLine("**Descrição:**");
					foreach(var line in item.Info.Description)
						descriptionBuilder.AppendLine($"- {line}");
					descriptionBuilder.AppendLine();
				}
				if (item.Effects.Length > 0) {
					descriptionBuilder.AppendLine("**Efeitos:**");
					foreach(var effect in item.Effects) {
						descriptionBuilder.AppendLine($"🎯 **[{effect.Role}] - {effect.Name}**");
						foreach (var content in effect.Contents) {
							if (content.Type == EffectContentType.Quote)
								descriptionBuilder.AppendLine($"*{content.Value}*");
							else
								descriptionBuilder.AppendLine(content.Value);
						}
					}
					descriptionBuilder.AppendLine();
				}
				embed.WithDescription(descriptionBuilder.ToString().Trim());
			}
			await ctx.RespondWithEmbedAsync(60, embed);
		}
	}
}
using System.Text;
using ChariotSanzzo.Commands.Slash.AutoCompleteProviders;
using ChariotSanzzo.Components.AlbinaApi.Common;
using ChariotSanzzo.Components.AlbinaApi.DTOs;
using ChariotSanzzo.Utils;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("AlbinaInfo", "Commands for fetching Albina related info.")]
	public class AlbinaInfoCommands : ApplicationCommandModule {
		[SlashCommand("Mastery", "Fetchs the info from a given mastery.")]
		public static async Task	FetchMastery(InteractionContext ctx, [Option("Mastery", "The mastery to fetch")] [Autocomplete(typeof(MasteryAutoCompleteProvider))] string masterySlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			MasteryDto mastery = await Program.AlbinaConn.GetMasteryAsync(masterySlug);

			if (mastery.Id == Guid.Empty) {
				embed.WithDescription("Mastery not found.");
			} else {
				embed.WithThumbnail(mastery.IconUrl);
				embed.WithFooter($"{mastery.Type} - {mastery.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(mastery.Name, mastery.Slug, "maestrias");
				descriptionBuilder.AppendGenericInfoDescription(mastery.Info);
				descriptionBuilder.AppendEffectsDescription(mastery.Effects);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(60, embed);
		}
		[SlashCommand("Item", "Fetchs the info from a given item.")]
		public static async Task	FetchItem(InteractionContext ctx, [Option("Item", "The item to fetch")] [Autocomplete(typeof(ItemAutoCompleteProvider))] string itemSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			ItemDto item = await Program.AlbinaConn.GetItemAsync(itemSlug);

			if (item.Id == Guid.Empty) {
				embed.WithDescription("Item not found.");
			} else {
				embed.WithThumbnail(item.IconUrl);
				embed.WithFooter($"{item.Type} - {item.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(item.Name, item.Slug, "items");
				descriptionBuilder.AppendGenericInfoDescription(item.Info);
				descriptionBuilder.AppendItemPropertiesDescription(item.Properties);
				descriptionBuilder.AppendEffectsDescription(item.Effects);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(60, embed);
		}
		[SlashCommand("Skill", "Fetchs the info from a given skill.")]
		public static async Task	FetchSkill(InteractionContext ctx, [Option("Skill", "The skill to fetch")] [Autocomplete(typeof(SkillAutoCompleteProvider))] string skillSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			SkillDto skill = await Program.AlbinaConn.GetSkillAsync(skillSlug);

			if (skill.Id == Guid.Empty) {
				embed.WithDescription("Skill not found.");
			} else {
				embed.WithThumbnail(skill.IconUrl);
				embed.WithFooter($"{skill.Type} - {skill.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(skill.Name, skill.Slug, "skills");
				descriptionBuilder.AppendGenericInfoDescription(skill.Info);
				descriptionBuilder.AppendSkillPropertiesDescription(skill.Properties);
				descriptionBuilder.AppendEffectsDescription(skill.Effects);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(60, embed);
		}
		
		private class AlbinaInfoDescriptionBuilder {
			private static string	AlbinaOnlineAddress	{get; set;} = Environment.GetEnvironmentVariable("ALBINA_ONLINE") ?? throw new InvalidOperationException("ALBINA_ONLINE not set");
			private StringBuilder	DescriptionBuilder	{get; set;} = new();

			public string	GetString() {
				return SmartText.Deserialize(DescriptionBuilder.ToString().Trim());
			}
			public void		AppendGenericInfoDescription<T>(T info)
				where T : IGenericInfo
			{
				if (info.Summary.Length > 0) {
					DescriptionBuilder.AppendLine("### Resumo:");
					foreach(var line in info.Summary)
						DescriptionBuilder.AppendLine($"- {line}");
				}
				if (info.Description.Length > 0) {
					DescriptionBuilder.AppendLine("### Descrição:");
					foreach(var line in info.Description)
						DescriptionBuilder.AppendLine($"- {line}");
				}
				DescriptionBuilder.AppendLine();
			}
			public void		AppendItemPropertiesDescription(ItemProperties properties) {
				DescriptionBuilder.AppendLine("```ansi\n");
				DescriptionBuilder.AppendLine($"[0;35m🔳Slot:[0m {properties.Slot}");
				DescriptionBuilder.AppendLine($"[0;35m🌸Atributo:[0m {properties.Attribute}");
				if (properties.Stats != null) {
					if (properties.Stats.Damage != "")
						DescriptionBuilder.AppendLine($"[0;31m🛠️Dano:[0m {properties.Stats.Damage}");
					if (properties.Stats.Accuracy != "")
						DescriptionBuilder.AppendLine($"[0;32m🎯Acerto:[0m {properties.Stats.Accuracy}");
					if (properties.Stats.Defense != "")
						DescriptionBuilder.AppendLine($"[0;41m🛡️Defesa:[0m {properties.Stats.Defense}");
					if (properties.Stats.DamageType != "")
						DescriptionBuilder.AppendLine($"[0;30m🔪Tipo de Dano:[0m {properties.Stats.DamageType}");
					if (properties.Stats.Range != "")
						DescriptionBuilder.AppendLine($"[0;30m📏Alcance:[0m {properties.Stats.Range}");
				}
				DescriptionBuilder.AppendLine("```");

				if (properties.Extras.Length > 0) {
					DescriptionBuilder.AppendLine("### 📌 Extras");
					foreach (var extra in properties.Extras) {
						DescriptionBuilder.AppendLine($"> ⦇`{extra.Key}`⦈ ⪩ {extra.Value}");
					}
					DescriptionBuilder.AppendLine();
				}
			}
			public void		AppendSkillPropertiesDescription(SkillProperties properties) {
				DescriptionBuilder.AppendLine("```ansi\n");
				if (properties.Components.Mana != "")
					DescriptionBuilder.AppendLine($"[0;34mMana:[0m {properties.Components.Mana}");
				if (properties.Components.Stamina != "")
					DescriptionBuilder.AppendLine($"[0;36mEstamina:[0m {properties.Components.Stamina}");
				if (properties.Components.Time != "")
					DescriptionBuilder.AppendLine($"[0;33mTempo:[0m {properties.Components.Time}");
				if (properties.Components.Duration != "")
					DescriptionBuilder.AppendLine($"[0;34mDuração:[0m {properties.Components.Duration}");
				if (properties.Components.Form != "")
					DescriptionBuilder.AppendLine($"[0;36mForma:[0m {properties.Components.Form}");
				if (properties.Components.Range != "")
					DescriptionBuilder.AppendLine($"[0;31mAlcance:[0m {properties.Components.Range}");
				if (properties.Components.Area != "")
					DescriptionBuilder.AppendLine($"[0;31mÁrea:[0m {properties.Components.Area}");
				DescriptionBuilder.AppendLine("```");

				if (properties.Extras.Length > 0) {
					DescriptionBuilder.AppendLine("### 📌 Extras");
					foreach (var extra in properties.Extras) {
						DescriptionBuilder.AppendLine($"> ⦇`{extra.Key}`⦈ ⪩ {extra.Value}");
					}
					DescriptionBuilder.AppendLine();
				}
			}
			public void		AppendEffectsDescription<T>(T[] effects)
				where T : IEffectDto
			{
				if (effects.Length > 0) {
					DescriptionBuilder.AppendLine("## **Efeitos:**");
					foreach(var effect in effects) {
						DescriptionBuilder.AppendLine($"\n```ansi\n🎯 [0;35m[{effect.Role}][0m {effect.Name}```");
						foreach (var content in effect.Contents) {
							if (content.Type == EffectContentType.Quote)
								DescriptionBuilder.AppendLine($"- {content.Value}");
							else
								DescriptionBuilder.AppendLine(content.Value);
						}
						DescriptionBuilder.AppendLine();
					}
					DescriptionBuilder.AppendLine();
				}
			}
			public void		AppendName(string name, string slug, string endpoint) {
				DescriptionBuilder.AppendLine($"# **[{name}]({AlbinaInfoDescriptionBuilder.AlbinaOnlineAddress}/{endpoint}/{slug})**\n");
			}
		}
	}
}
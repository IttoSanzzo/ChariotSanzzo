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
		[SlashCommand("Home", "Gives you the Albina Site link.")]
		public static async Task	FetchHomeLink(InteractionContext ctx) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();

			embed.WithTitle("AlbinaOnline");
			embed.WithDescription($"# **[Link para sua home]({LinkData.GetAlbinaSiteFullAdress()}/home)**");

			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Mastery", "Fetchs the info from a given mastery.")]
		public static async Task	FetchMastery(InteractionContext ctx, [Option("Name", "The mastery to fetch")] [Autocomplete(typeof(MasteryAutoCompleteProvider))] string masterySlug) {
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
			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Item", "Fetchs the info from a given item.")]
		public static async Task	FetchItem(InteractionContext ctx, [Option("Name", "The item to fetch")] [Autocomplete(typeof(ItemAutoCompleteProvider))] string itemSlug) {
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
			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Skill", "Fetchs the info from a given skill.")]
		public static async Task	FetchSkill(InteractionContext ctx, [Option("Name", "The skill to fetch")] [Autocomplete(typeof(SkillAutoCompleteProvider))] string skillSlug) {
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
			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Spell", "Fetchs the info from a given spell.")]
		public static async Task	FetchSpell(InteractionContext ctx, [Option("Name", "The spell to fetch")] [Autocomplete(typeof(SpellAutoCompleteProvider))] string spellSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			SpellDto spell = await Program.AlbinaConn.GetSpellAsync(spellSlug);

			if (spell.Id == Guid.Empty) {
				embed.WithDescription("Spell not found.");
			} else {
				embed.WithThumbnail(spell.IconUrl);
				embed.WithFooter($"{spell.Type} - {spell.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(spell.Name, spell.Slug, "spells");
				descriptionBuilder.AppendGenericInfoDescription(spell.Info);
				descriptionBuilder.AppendSpellPropertiesDescription(spell.Properties);
				descriptionBuilder.AppendEffectsDescription(spell.Effects);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Trait", "Fetchs the info from a given skill.")]
		public static async Task	FetchTrait(InteractionContext ctx, [Option("Name", "The trait to fetch")] [Autocomplete(typeof(TraitAutoCompleteProvider))] string traitSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			TraitDto trait = await Program.AlbinaConn.GetTraitAsync(traitSlug);

			if (trait.Id == Guid.Empty) {
				embed.WithDescription("Skill not found.");
			} else {
				embed.WithThumbnail(trait.IconUrl);
				embed.WithFooter($"{trait.Type} - {trait.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(trait.Name, trait.Slug, "traits");
				descriptionBuilder.AppendGenericInfoDescription(trait.Info);
				descriptionBuilder.AppendTraitRequirementsDescription(trait.Info.Requirements);
				descriptionBuilder.AppendEffectsDescription(trait.Effects);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(120, embed);
		}
		[SlashCommand("Race", "Fetchs the info from a given race.")]
		public static async Task	FetchRace(InteractionContext ctx, [Option("Name", "The race to fetch")] [Autocomplete(typeof(RaceAutoCompleteProvider))] string raceSlug) {
			await ctx.DeferAsync();
			var embed = new DiscordEmbedBuilder();
			RaceDto race = await Program.AlbinaConn.GetRaceAsync(raceSlug);


			if (race.Id == Guid.Empty) {
				embed.WithDescription("Race not found.");
			} else {
				embed.WithThumbnail(race.IconUrl);
				embed.WithFooter($"{race.Type} - {race.SubType}");
				var descriptionBuilder = new AlbinaInfoDescriptionBuilder();
				descriptionBuilder.AppendName(race.Name, race.Slug, "racas");
				descriptionBuilder.AppendRaceInfoDescription(race.Info);
				descriptionBuilder.AppendRaceParameters(race.Parameters);
				descriptionBuilder.AppendRaceGenerals(race.Generals);
				embed.WithDescription(descriptionBuilder.GetString());
			}
			await ctx.RespondWithEmbedAsync(120, embed);
		}

		private class AlbinaInfoDescriptionBuilder {
			private StringBuilder	DescriptionBuilder			{get; set;} = new();

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
			public void		AppendRaceInfoDescription(RaceInfo info) {
				if (info.Introduction.Length > 0) {
					DescriptionBuilder.AppendLine($"{info.Introduction[0]}");
				}
				if (info.Description.Length > 0) {
					DescriptionBuilder.AppendLine("### Descrição Visual:");
					foreach(var line in info.Description)
						DescriptionBuilder.AppendLine($"- {line}");
				}
				DescriptionBuilder.AppendLine();
			}
			public void		AppendRaceParameters(RaceParameters parameters) {
				DescriptionBuilder.AppendLine("```ansi\n");
				DescriptionBuilder.AppendLine($"[0;34mVitalidade:[0m {parameters.Vitality}");
				DescriptionBuilder.AppendLine($"[0;36mVigor:[0m {parameters.Vigor}");
				DescriptionBuilder.AppendLine($"[0;33mManapool:[0m {parameters.Manapool}");
				DescriptionBuilder.AppendLine($"[0;34mPoder Físico:[0m {parameters.PhysicalPower}");
				DescriptionBuilder.AppendLine($"[0;36mPoder Mágico:[0m {parameters.MagicalPower}");
				DescriptionBuilder.AppendLine("```");
			}
			public void		AppendRaceGenerals(RaceGenerals generals) {
				DescriptionBuilder.AppendLine("```ansi\n");
				DescriptionBuilder.AppendLine($"[0;34mAltura:[0m {generals.Height}");
				DescriptionBuilder.AppendLine($"[0;36mPeso:[0m {generals.Weight}");
				DescriptionBuilder.AppendLine($"[0;33mLongevidade:[0m {generals.Longevity}");
				DescriptionBuilder.AppendLine($"[0;34mVelocidade:[0m {generals.Speed}");
				DescriptionBuilder.AppendLine($"[0;36mLíngua:[0m {generals.Language}");
				DescriptionBuilder.AppendLine("```");
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
			public void		AppendSpellPropertiesDescription(SpellProperties properties) {
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
				if (properties.Extras.Length > 0) {
					DescriptionBuilder.AppendLine("### 🎼 Cantos");
					foreach (var chantLine in properties.Chants) {
						DescriptionBuilder.AppendLine($"⪩ {chantLine}");
					}
					DescriptionBuilder.AppendLine();
				}
			}
			public void		AppendTraitRequirementsDescription(string[] requirements) {
				if (requirements.Length > 0) {
					DescriptionBuilder.AppendLine("### 🔒 Requerimentos");
					foreach (var requirement in requirements) {
						DescriptionBuilder.AppendLine($"- {requirement}");
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
				DescriptionBuilder.AppendLine($"# **[{name}]({LinkData.GetAlbinaSiteFullAdress()}/{endpoint}/{slug})**\n");
			}
		}
	}
}
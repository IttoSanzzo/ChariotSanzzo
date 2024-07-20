using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ChariotSanzzo.Components.DiceRoller;
using Microsoft.VisualBasic;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("Rolling", "Commands for RNG.")]
	public class DiceCommands : ApplicationCommandModule {
		[SlashCommand("toss", "Toss a coin to your witcher.")]
		public async Task TossCoin(InteractionContext ctx) {
			await ctx.DeferAsync();
			var	coin = new Random();
			var res = coin.Next(1, 3);
			var	embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.VeryDarkGray)
				.WithTitle($"Result..: {(((res == 1) ? "<:CSHeads:1264065806246088806>": "<:CSTails:1264065821454372884>"))}")
				.WithFooter($"The winner was... {(((res == 1) ? "Heads": "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://i.postimg.cc/5ywj9bWv/CSHeads.png" 
					: "https://i.postimg.cc/FFqYjCHw/CSTails.png")}");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}

		[SlashCommand("dice", "Rolls a overly powerfull dice feature.")]
		public async Task Dice(InteractionContext ctx,
								[Option("Times", "How many times the set will be played. (1)")] double dTimes = 1,
								[Option("Count", "How many dices will be played each time. (1)")] double dCount = 1,
								[Option("Sides", "How much sides the dice have. (20)")] double dSides = 20,
								[Option("Advantage", "Advantage or disadvantage dice removals. (0)")] double dAdvan = 0,
								[Option("Equation", "Automatic calculation in each play. (null)")] string dEquat = "") {
			await ctx.DeferAsync();
			string	finalString = "";
			if (dTimes > 1)
				finalString += $"{dTimes}#";
			if (dCount > 1)
				finalString += $"{dCount}";
			finalString += $"d{dSides}";
			if (dAdvan != 0)
				finalString += $"a{dAdvan}";
			if (dEquat != "")
				finalString += $" ({dEquat})";
			DiceSet	dSet = new DiceSet() {
					_dString = finalString,
					_dEquat = dEquat,
					_dTimes = (int)dTimes,
					_dCount = (int)dCount,
					_dSides = (int)dSides,
					_dAdvan = (int)dAdvan
			};
			dSet.RunDice();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(dSet.GetFinalEmbed()));
		}
	}
}

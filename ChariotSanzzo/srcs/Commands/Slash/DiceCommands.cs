using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ChariotSanzzo.Components.DiceRoller;

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

		[SlashCommand("mdice", "Rolls a overly powerfull dice feature manually.")]
		public async Task MDice(InteractionContext ctx,
								[Option("Times", "How many times the set will be played. (1)")] double dTimes = 1,
								[Option("Count", "How many dices will be played each time. (1)")] double dCount = 1,
								[Option("Sides", "How much sides the dice have. (20)")] double dSides = 20,
								[Option("Advantage", "Advantage or disadvantage dice removals. (0)")] double dAdvan = 0,
								[Option("Equation", "Automatic calculation in each play. (null)")] string dEquat = "") {
			await ctx.DeferAsync();
			DiceSet	dSet = new DiceSet((int)dTimes, (int)dCount, (int)dSides, (int)dAdvan, dEquat);
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(dSet.GetEmbed()));
		}
		[SlashCommand("dice", "Rolls a overly powerfull dice feature.")]
		public async Task Dice(InteractionContext ctx, [Option("DiceSet", "Just roll your dices.")] string dLine) {
			await ctx.DeferAsync();
			DiceSet	dSet = new DiceSet(dLine);
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(dSet.GetEmbed()));
		}
	}
}

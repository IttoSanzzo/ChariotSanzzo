using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using ChariotSanzzo.Components.DiceRoller;

namespace ChariotSanzzo.Commands.Prefix {
	public class DiceCommands : BaseCommandModule {
		[Command("toss")]
		public async Task TossCoin(CommandContext ctx) {
			var	coin = new Random();
			var res = coin.Next(1, 3);
			var	embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.Yellow)
				.WithTitle("Result..:")
				.WithFooter($"The winner was... {(((res == 1) ? "Heads": "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://i.postimg.cc/5ywj9bWv/CSHeads.png" 
					: "https://i.postimg.cc/FFqYjCHw/CSTails.png")}");
			await ctx.Channel.SendMessageAsync(embed: embed);
		}

		[Command("dice")]
		public async Task Dice(CommandContext ctx) {
			await ctx.Message.RespondAsync(
				"Usages..:\n" +
				"`cs dice` `\"exact dice structure without spaces\"`\n\n" +
				"`cs dice` `\"Number of Dices\"` `\"Number of Sides per dice\"`\n\n" +
				"`cs dice` `\"Number of Repetitions\"` `\"Number of Dices\"` `\"Number of Sides per dice\"`\n\n" +
				"`cs dice` `\"Number of Repetitions\"` `\"Number of Dices\"` `\"Number of Sides per dice\"` `\"Advantage Level\"`\n\n" +
				"`cs dice` `\"Number of Repetitions\"` `\"Number of Dices\"` `\"Number of Sides per dice\"` `\"Advantage Level\"` `\"Equations\"`");
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, string line) {
			DiceSet	dSet = new DiceSet(line);
			dSet.RunDice();
			await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dCount, int dSides) {
			DiceSet	dSet = new DiceSet() {
					_dString = $"{dCount}d{dSides}",
					_dEquat = "",
					_dTimes = 1,
					_dCount = dCount,
					_dSides = dSides,
					_dAdvan = 0
			};
			dSet.RunDice();
			await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides) {
			DiceSet	dSet = new DiceSet() {
					_dString = $"{dTimes}#{dCount}d{dSides}",
					_dEquat = "",
					_dTimes = dTimes,
					_dCount = dCount,
					_dSides = dSides,
					_dAdvan = 0
			};
			dSet.RunDice();
			await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides, int dAdvan) {
			DiceSet	dSet = new DiceSet() {
					_dString = $"{dTimes}#{dCount}d{dSides}a{dAdvan}",
					_dEquat = "",
					_dTimes = dTimes,
					_dCount = dCount,
					_dSides = dSides,
					_dAdvan = dAdvan
			};
			dSet.RunDice();
			await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides, int dAdvan, string dEquat) {
			DiceSet	dSet = new DiceSet() {
					_dString = $"{dTimes}#{dCount}d{dSides}a{dAdvan}e{dEquat}",
					_dEquat = dEquat,
					_dTimes = dTimes,
					_dCount = dCount,
					_dSides = dSides,
					_dAdvan = dAdvan
			};
			dSet.RunDice();
			await ctx.Message.RespondAsync(dSet.GetFinalEmbed());
		}
	}
}

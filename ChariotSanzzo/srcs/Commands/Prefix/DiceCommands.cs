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
				.WithColor(DiscordColor.VeryDarkGray)
				.WithTitle($"Result..: {(((res == 1) ? "<:CSHeads:1264065806246088806>": "<:CSTails:1264065821454372884>"))}")
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
			await ctx.Message.RespondAsync(dSet.GetEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dCount, int dSides) {
			DiceSet	dSet = new DiceSet(1, dCount, dSides, 0, "");
			await ctx.Message.RespondAsync(dSet.GetEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides) {
			DiceSet	dSet = new DiceSet(dTimes, dCount, dSides, 0, "");
			await ctx.Message.RespondAsync(dSet.GetEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides, int dAdvan) {
			DiceSet	dSet = new DiceSet(dTimes, dCount, dSides, dAdvan, "");
			await ctx.Message.RespondAsync(dSet.GetEmbed());
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, int dTimes, int dCount, int dSides, int dAdvan, string dEquat) {
			DiceSet	dSet = new DiceSet(dTimes, dCount, dSides, dAdvan, dEquat);
			await ctx.Message.RespondAsync(dSet.GetEmbed());
		}
	}
}

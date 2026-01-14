using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using ChariotSanzzo.Components.DiceRoller;

namespace ChariotSanzzo.Commands.Prefix {
	public class DiceCommands : BaseCommandModule {
		[Command("toss")]
		public async Task TossCoin(CommandContext ctx) {
			var coin = new Random();
			var res = coin.Next(1, 3);
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.VeryDarkGray)
				.WithTitle($"Result..: {(((res == 1) ? "<:CSHeads:1264065806246088806>" : "<:CSTails:1264065821454372884>"))}")
				.WithFooter($"The winner was... {(((res == 1) ? "Heads" : "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://i.postimg.cc/5ywj9bWv/CSHeads.png"
					: "https://i.postimg.cc/FFqYjCHw/CSTails.png")}");
			await ctx.Channel.SendMessageAsync(embed: embed);
		}

		[Command("dice")]
		public async Task Dice(CommandContext ctx) {
			await ctx.Message.RespondAsync(
				"Usages..:\n`cs dice` `\"dice expression\"`\n\n");
		}
		[Command("dice")]
		public async Task Dice(CommandContext ctx, string line) {
			DiceExpression expression = new(line);
			var (_, embed) = expression.RollForDiscord();
			await ctx.Message.RespondAsync(embed);
		}
	}
}

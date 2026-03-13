using DSharpPlus.Entities;
using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus.Commands;
using System.ComponentModel;
using ChariotSanzzo.Infrastructure.Config;

namespace ChariotSanzzo.Services.Commands {
	[Command("Rolling"), Description("Commands for RNG.")]
	public class DiceCommands {
		[Command("toss"), Description("Toss a coin to your witcher.")]
		public async Task TossCoin(CommandContext ctx) {
			await ctx.DeferResponseAsync();
			var coin = new Random();
			var res = coin.Next(1, 3);
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.VeryDarkGray)
				.WithTitle($"Result..: {(((res == 1) ? EmojisConfig.CSHeads : EmojisConfig.CSTails))}")
				.WithFooter($"The winner was... {(((res == 1) ? "Heads" : "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://i.postimg.cc/5ywj9bWv/CSHeads.png"
					: "https://i.postimg.cc/FFqYjCHw/CSTails.png")}");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[Command("dice"), Description("Rolls a overly powerfull dice feature.")]
		public async Task Dice(CommandContext ctx, [Description("Just roll your dices.")] string Expression) {
			await ctx.DeferResponseAsync();
			DiceExpression expression = new(Expression);
			var (_, embed) = expression.RollForDiscord();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
		}
	}
}

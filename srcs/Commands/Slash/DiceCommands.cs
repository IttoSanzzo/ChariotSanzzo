using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using ChariotSanzzo.Components.DiceRoller;

namespace ChariotSanzzo.Commands.Slash {
	[SlashCommandGroup("Rolling", "Commands for RNG.")]
	public class DiceCommands : ApplicationCommandModule {
		[SlashCommand("toss", "Toss a coin to your witcher.")]
		public async Task TossCoin(InteractionContext ctx) {
			await ctx.DeferAsync();
			var coin = new Random();
			var res = coin.Next(1, 3);
			var embed = new DiscordEmbedBuilder()
				.WithColor(DiscordColor.VeryDarkGray)
				.WithTitle($"Result..: {(((res == 1) ? "<:CSHeads:1264065806246088806>" : "<:CSTails:1264065821454372884>"))}")
				.WithFooter($"The winner was... {(((res == 1) ? "Heads" : "Tails"))}!")
				.WithImageUrl($"{((res == 1)
					? "https://i.postimg.cc/5ywj9bWv/CSHeads.png"
					: "https://i.postimg.cc/FFqYjCHw/CSTails.png")}");
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed: embed));
		}
		[SlashCommand("dice", "Rolls a overly powerfull dice feature.")]
		public async Task Dice(InteractionContext ctx, [Option("Expression", "Just roll your dices.")] string line) {
			await ctx.DeferAsync();
			DiceExpression expression = new(line);
			var (_, embed) = expression.RollForDiscord();
			await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
		}
	}
}

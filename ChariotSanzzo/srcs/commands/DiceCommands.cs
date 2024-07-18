using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;

namespace ChariotSanzzo.Commands {
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
	}
}

using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
			int i = 0;
			if (i == 1) // TODO DiceRoller
				await ctx.Message.RespondAsync("Oka");
		}
	}
}

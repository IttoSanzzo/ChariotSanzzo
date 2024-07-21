using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
			if (ctx.Message.Content == "dice") {
				DiceSet dSet = new DiceSet(10, 10, 20, 2, "*5+15");
				if (dSet.CheckDice() == false)
					return ;
				await ctx.Message.RespondAsync(dSet.GetEmbed());
			}
		}
	}
}

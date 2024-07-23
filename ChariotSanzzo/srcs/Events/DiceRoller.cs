using ChariotSanzzo.Components.DiceRoller;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace ChariotSanzzo.Events {
	public static class STPDiceRoller {
		// static int messs = 0;
		public static async Task DiceRoller(DiscordClient sender, MessageCreateEventArgs ctx) {
			if (ctx.Author.IsBot)
				return ;
			if (ctx.Message.Content == "dice") {
				DiceSet dSetD = new DiceSet(10, 10, 20, 2, "*5+15");
				if (dSetD.CheckDice() == false)
					return ;
				await ctx.Message.RespondAsync(dSetD.GetEmbed());
				return ;
			}
			// Console.Write($"\n\n{++messs}-->");
			DiceSet	dSet = new DiceSet(ctx.Message.Content);
			if (dSet.CheckDice() == true) {
				await ctx.Message.RespondAsync(dSet.GetEmbed());
			}
		}
	}
}
